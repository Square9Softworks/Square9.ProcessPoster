namespace Square9.ProcessPoster
{
    /// <summary>
    /// This is a full representation of the Property class as it exists on both the Workflow and Process objects.
    /// Fully deserializing this object ensures that no disparities exist between 
    /// the properties of a newly posted process and the workflow that spawned it.
    /// </summary>
    public class Property
    {
        #pragma warning disable 1591
        public int ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public bool SystemProperty { get; set; }
        public string Value { get; set; }
        public object MValue { get; set; }
        public int Confidence { get; set; }
        public int SourceType { get; set; }
        public int PortalID { get; set; }
        public int DBID { get; set; }
        public int FieldID { get; set; }
        public object TableFields { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Page { get; set; }
        public bool TemplateProperty { get; set; }
    }
}