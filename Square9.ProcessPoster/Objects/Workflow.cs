using System.Collections.Generic;

namespace Square9.ProcessPoster
{
    /// <summary>
    /// This class is a partial representation of the full GlobalCapture Workflow object.
    /// For the purposes of cleanliness and clarity, the properties defined in this class 
    /// are only those needed for serialization when getting a workflow from the Capture API.
    /// </summary>
    internal class Workflow
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Property[] Properties { get; set; }
        public Dictionary<string, Node> Nodes { get; set; }
    }

    /// <summary>
    /// The node class must be partially serialized in order to detect which node is the 
    /// initiator node when setting the "CurrentNode" property in the Process object.
    /// </summary>
    internal class Node
    {
        public string Category { get; set; }
    }
}
