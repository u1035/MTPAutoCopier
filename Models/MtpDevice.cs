using MediaDevices;

namespace MTPAutoCopier.Models
{
    public class MtpDevice
    {
        public string DeviceManufacturer { get; set; }
        public string DeviceName { get; set; }
        public string DeviceId { get; set; }

        public bool Equals(MediaDevice device)
        {
            return (device.Manufacturer == DeviceManufacturer && device.Model == DeviceName && device.DeviceId == DeviceId);
        }
    }
}
