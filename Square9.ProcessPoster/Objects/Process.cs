using System;
using System.Linq;
using Newtonsoft.Json;

namespace Square9.ProcessPoster
{
    /// <summary>
    /// This class is a partial representation of the full process object that the GlobalCapture engine consumes.
    /// The properties defined in this class are only those needed for serialization when posting to the Capture API.
    /// The rest of the missing properties will be populated with default values when the process is accepted by the Capture API.
    /// </summary>
    public class Process
    {
        /// <summary>
        /// Creates a <see cref="Process"/> ready for posting to a Capture API.
        /// </summary>
        /// <param name="workflow"><see cref="Workflow"/> that the process will be based on.</param>
        /// <param name="filePath">File path of the file for the <see cref="Process"/> that exists on the Capture API server.</param>
        internal Process(Workflow workflow, string filePath)
        {
            //First, we set the Process properties "WorfklowID" and "WorkflowName"
            //with the information from the workflow.
            WorkflowID = workflow.ID;
            WorkflowName = workflow.Name;

            //Next, we have to generate a new BatchID for the 

            //Next, we update the FirstAccessed and LastAccessed 
            //properties with the current time.
            FirstAccessed = DateTime.Now;
            LastAccessed = DateTime.Now;

            //The ProcessType must be set to GlobalCapture
            ProcessType = ProcessType.GlobalCapture;

            //Setting the process status to "Ready" will indicate to
            //the engine that the process is ready to be picked up.
            Status = ProcessStatus.Ready;

            //The "CurrentNode" property indicates the Node that a process is currently at.
            //For new processes, CurrentProperty must be set to the initiator node, which has 
            //a category of "18". This way, the process will begin processing from the very first node in the workflow.
            var initiator = workflow.Nodes.First(n => n.Value.Category == "18").Key;
            CurrentNode = initiator;

            //Process properties are inherited from the associated workflow.
            Properties = workflow.Properties;

            //A BatchID must be set on the new process. To do this, generate a new Guid and 
            //set it to the value of the "BatchID" property, which has an ID of 0.
            Properties.First(p => p.ID == 0 && p.Name == "BatchID").Value = Guid.NewGuid().ToString();

            //The link between a new process and its file is held within the "FilePath" property, which has an ID of -1. 
            //Here, we set its value to the location of the posted file that now exists in the Capture API cache directory. 
            Properties.First(p => p.ID == -1 && p.Name == "FilePath").Value = filePath;

            //The history and filepages array properties cannot exist as null in the database. 
            //These properties must be set to zero length arrays in order for the engine to properly consume the process.
            History = new object[0];
            FilePages = new object[0];
        }

        /// <summary>
        /// The Process ID is generated when the process is successfully posted to the Capture API.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// ID of the associated workflow.
        /// </summary>
        public string WorkflowID { get; set; }
        /// <summary>
        /// Name of the associated workflow.
        /// </summary>
        public string WorkflowName { get; set; }
        /// <summary>
        /// DateTime object of when the process was first accessed.
        /// </summary>
        public DateTime FirstAccessed { get; set; }
        /// <summary>
        /// DateTime object of when the process was last accessed.
        /// </summary>
        public DateTime LastAccessed { get; set; }
        /// <summary>
        /// The node in the associated workflow that the process is currently at.
        /// </summary>
        public string CurrentNode { get; set; }
        /// <summary>
        /// Array of properties on the process.
        /// </summary>
        public Property[] Properties { get; set; }
        /// <summary>
        /// Process type (GlobalCapture or GlobalAction).
        /// </summary>
        public ProcessType ProcessType { get; set; }
        /// <summary>
        /// Status of the process.
        /// </summary>
        public ProcessStatus Status { get; set; }
        /// <summary>
        /// Array containing HistoryEntries for the process.
        /// </summary>
        public object[] History { get; set; }
        /// <summary>
        /// Array containing single page information for imported process that have undergone file separation.
        /// </summary>
        public object[] FilePages { get; set; }

        //This parameterless constructor allows NewtonsoftJson to serialize the process object.
        [JsonConstructor]
        internal Process() { }
    }

    /// <summary>
    /// Full list of the different statuses that a process can take.
    /// </summary>
    public enum ProcessStatus
    {
        #pragma warning disable 1591
        WaitQueue = 1,
        Processing = 2,
        Errored = 3,
        Completed = 4,
        Ready = 5,
        ManuallyCompleted = 6,
        WaitTimer = 7,
        QueueTimer = 8,
        Validation = 9,
        SubProcessing = 10
    }

    /// <summary>
    /// Type of process (GlobalCapture or GlobalAction)
    /// </summary>
    public enum ProcessType
    {
        GlobalAction = 1,
        GlobalCapture = 2
    }
}
