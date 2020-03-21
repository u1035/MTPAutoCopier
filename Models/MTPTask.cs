namespace MTPAutoCopier.Models
{
    public class MtpTask
    {
        public string SourceDeviceManufacturer { get; set; }
        public string SourceDeviceName { get; set; }
        public string SourceDeviceId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public bool DeleteSourceAfterCopying { get; set; }
        public bool AlreadyProcessed { get; set; }
        public bool IgnoreThisDevice { get; set; }

    }
}
