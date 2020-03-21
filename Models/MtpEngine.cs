using System;
using System.IO;
using System.Linq;
using System.Threading;
using MediaDevices;

namespace MTPAutoCopier.Models
{
    public class MtpEngine
    {

        private readonly Settings _config;
        private Timer _waitForDeviceTimer;
        private bool _timerWorkingNow;

        public MtpEngine()
        {
            _config = Settings.LoadConfig();
            _waitForDeviceTimer = new Timer(OnWaitForDeviceTimerTick, null, 0, 10000);

        }

        private void OnWaitForDeviceTimerTick(object state)
        {
            if (_timerWorkingNow)
                return;

            _timerWorkingNow = true;
            CheckForConnectedDevices();
            _timerWorkingNow = false;
        }

        private void CheckForConnectedDevices()
        {
            var devices = MediaDevice.GetDevices();

            foreach (var device in devices)
            {
                var connectedDevice = new MtpDevice
                {
                    DeviceName = device.Model,
                    DeviceManufacturer = device.Manufacturer,
                    DeviceId = device.DeviceId
                };

                if (_config.DevicesToWatch.Contains(connectedDevice))
                    ProcessTasks(device);
            }
        }

        private void ProcessTasks(MediaDevice device)
        {
            var tasksForThisDevice = _config.Tasks.Where(d => d.SourceDevice.Equals(device));
            if (!tasksForThisDevice.Any())
                return;

            try
            {
                foreach (var task in tasksForThisDevice)
                {
                    device.Connect();
                    var sourceDirInfo = device.GetDirectoryInfo(task.SourcePath);
                    var files = sourceDirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        CopyFile(device, file, task.DestinationPath);
                    }
                    device.Disconnect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private void CopyFile(MediaDevice device, MediaFileInfo file, string destinationDir)
        {
            MemoryStream memoryStream = new System.IO.MemoryStream();
            device.DownloadFile(file.FullName, memoryStream);
            memoryStream.Position = 0;
            WriteStreamToDisk($"{destinationDir}\\{file.Name}", memoryStream);
        }

        private void WriteStreamToDisk(string filePath, MemoryStream memoryStream)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                file.Write(bytes, 0, bytes.Length);
                memoryStream.Close();
            }
        }
    }
}
