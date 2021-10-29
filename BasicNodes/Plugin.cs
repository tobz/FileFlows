namespace FileFlow.BasicNodes
{
    using System.ComponentModel.DataAnnotations;
    using FileFlow.Plugins.Attributes;

    public class Plugin : FileFlow.Plugins.Plugin
    {
        public override string Name => "Basic Nodes";

        [Required]
        [File(1, "exe")]
        public string HandBrakeCli { get; set; }

        [Required]
        [File(2, "exe")]
        public string FFProbeExe { get; set; }
    }
}