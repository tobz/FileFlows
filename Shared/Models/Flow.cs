namespace FileFlow.Shared.Models
{
    using System;
    using System.Collections.Generic;

    public class Flow : ViObject
    {
        public bool Enabled { get; set; }
        public List<FlowPart> Parts { get; set; }
    }
}