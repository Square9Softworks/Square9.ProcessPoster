using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Square9.ProcessPoster
{
    /// <inheritdoc />
    /// <summary>
    /// Provides methods for creating and posting a <see cref="T:Square9.ProcessPoster.Process" /> to a Capture API.
    /// </summary>
    public class ProcessPoster : IDisposable
    {
        private HttpClient CaptureApi { get; }

        /// <summary>
        /// Initializes a new member of the <see cref="ProcessPoster"/> class.
        /// </summary>
        /// <param name="url">Capture API URL.</param>
        /// <param name="username">Authorizing User.</param>
        /// <param name="password">Password for authorizing user.</param>
        public ProcessPoster(string url, string username, string password)
        {
            CaptureApi = new HttpClient {BaseAddress = new Uri(url)};

            //The Capture API requires a basic authorization header.
            //The basic authorization value must be a Base64 encoded string of the following format: 
            //"username:password"
            var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
            var base64 = Convert.ToBase64String(bytes);
            CaptureApi.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

            CaptureApi.DefaultRequestHeaders.Accept.Clear();
            CaptureApi.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Posts a new <see cref="Process"/> to the Capture API.
        /// </summary>
        /// <param name="workflowId">Workflow to spawn the process from.</param>
        /// <param name="portalId">Batch Portal to post the process to.</param>
        /// <param name="filePath">Path to a local file for the process.</param>
        /// <returns>Process object as it exists in the database.</returns>
        public async Task<Process> PostProcess(string workflowId, int portalId, string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            return await PostProcess(workflowId, portalId, bytes, fileName);
        }

        /// <summary>
        /// Posts a new <see cref="Process"/> to the Capture API.
        /// </summary>
        /// <param name="workflowId">ID of <see cref="Workflow"/> to spawn the process from.</param>
        /// <param name="portalId">Batch Portal to post the process to.</param>
        /// <param name="bytes">Byte Array containing the file for the process.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Process object as it exists in the database.</returns>
        public async Task<Process> PostProcess(string workflowId, int portalId, byte[] bytes, string fileName)
        {
            //The post operation begins by posting the file to the API's cache directory. 
            //This will create a file on the Capture API server that the engine will consume when the process runs. 
            var path = await PostFile(bytes, fileName);

            //The next operation is to get the Workflow that the process will be based on.
            //Process properties are based on the workflow that spawned the process. 
            var workflow = await GetWorkflow(workflowId, portalId);

            //The logic for creating a process based on a workflow is 
            //contained in the constructor for the Process object.
            var process = new Process(workflow, path);

            //Finally, we post the process to the Capture API.
            return await PostProcess(process, portalId);
        }

        /// <summary>
        /// Posts a file to the Capture API.
        /// </summary>
        /// <param name="bytes">Byte array of the file to post.</param>
        /// <param name="fileName">File name, including extension, of the file to post.</param>
        /// <returns>Full path of the posted file as it exists on the Capture API server.</returns>
        private async Task<string> PostFile(byte[] bytes, string fileName)
        {
            using (var content = new MultipartFormDataContent())
            {
                var byteContent = new ByteArrayContent(bytes);
                content.Add(byteContent, fileName, fileName);
                var response = await CaptureApi.PostAsync("files", content);

                //Return the failure message if the call fails to complete.
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                //The "files" route of the Capture API returns an array of the new file paths.
                var json = await response.Content.ReadAsStringAsync();
                var files = JsonConvert.DeserializeObject<string[]>(json);

                //In the case of larger files, The Capture API will need to have
                //MaxRequestLength and MaxAllowedContentLength set to max values.
                if (files.Length == 0)
                    throw new Exception("Failed to post the file to the API.");

                //Since a single file was posted, return the first element in the array.
                return files[0];
            }
        }

        /// <summary>
        /// Gets a <see cref="Workflow"/> from the Capture API.
        /// </summary>
        /// <param name="workflowId">ID of the workflow to get.</param>
        /// <param name="portalId">ID of the portal to get from.</param>
        /// <returns>Workflow from the Capture API.</returns>
        private async Task<Workflow> GetWorkflow(string workflowId, int portalId)
        {
            var response = await CaptureApi.GetAsync($"portal/{portalId}/workflow/{workflowId}");

            //Return the failure message if the call fails to complete.
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            var workflow = JsonConvert.DeserializeObject<Workflow>(json);
            return workflow;
        }

        /// <summary>
        /// Posts a single <see cref="Process"/> to the Capture API.
        /// </summary>
        /// <param name="process">Process object that will be posted.</param>
        /// <param name="portalId">ID of the portal to post to.</param>
        /// <returns>The process object as it exists in the database.</returns>
        private async Task<Process> PostProcess(Process process, int portalId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(process), Encoding.UTF8, "application/json");
            var response = await CaptureApi.PostAsync($"portal/{portalId}/process", content);

            //Return the failure message if the call fails to complete.
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Process>(json);
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes the <see cref="T:Square9.ProcessPoster.ProcessPoster" /> HTTP client.
        /// </summary>
        public void Dispose() => CaptureApi.Dispose();
    }
}
