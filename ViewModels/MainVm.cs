using System.IO;
using System.Linq;
using MediaDevices;
using MTPAutoCopier.Models;

namespace MTPAutoCopier.ViewModels
{
    public class MainVm
    {

        public MainVm()
        {
            var devices = MediaDevice.GetDevices();
            foreach (var dev in devices.Where(d=>d.Manufacturer!="Generic-"))
            {
                var tmp = new MtpTask
                {
                    SourceDeviceName = dev.Description,
                    SourceDeviceId=dev.DeviceId, 
                    SourceDeviceManufacturer = dev.Manufacturer,
                    SourcePath= "Внутр. накопитель\\DCIM\\Camera",
                    DestinationPath = ""
                };
                dev.Connect();


                //var entries = dev.EnumerateFileSystemEntries("\\");
                var photoDir = dev.GetDirectoryInfo(@"Внутр. накопитель\\DCIM\\Camera");

                var files = photoDir.EnumerateFiles("*.*", SearchOption.AllDirectories);

                //foreach (var file in files)
                //{
                //    MemoryStream memoryStream = new System.IO.MemoryStream();
                //    device.DownloadFile(file.FullName, memoryStream);
                //    memoryStream.Position = 0;
                //    WriteSreamToDisk($@"D:\PHOTOS\{file.Name}", memoryStream);
                //}
                dev.Disconnect();

            }


        }
    }
}
