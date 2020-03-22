using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MediaDevices;
using MTPAutoCopier.Annotations;

namespace MTPAutoCopier.Models
{
    public class MtpEngine : INotifyPropertyChanged
    {

        private readonly Settings _config;
        private Timer _waitForDeviceTimer;
        private bool _timerWorkingNow;
        private ObservableCollection<MediaDevice> _availableDevices;
        private MediaDevice _selectedDevice;

        public ObservableCollection<MediaDevice> AvailableDevices
        {
            get => _availableDevices;
            set
            {
                if (_availableDevices != value)
                {
                    _availableDevices = value;
                    OnPropertyChanged();
                }
            }
        }

        public MediaDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public MtpEngine()
        {
            _config = Settings.LoadConfig();
            _waitForDeviceTimer = new Timer(OnWaitForDeviceTimerTick, null, 0, 10000);
            AvailableDevices = new ObservableCollection<MediaDevice>(MediaDevice.GetDevices().ToList());
        }

        public void Test()
        {
            if (SelectedDevice != null)
            {
                var dev = new MtpDevice
                {
                    DeviceName = SelectedDevice.Description,
                    DeviceManufacturer = SelectedDevice.Manufacturer,
                    DeviceId = SelectedDevice.DeviceId
                };
                var tsk = new MtpTask
                {
                    SourceDevice = dev, SourcePath = "Внутр. накопитель\\DCIM\\Camera",
                    DestinationPath = "C:\\cam1_records"
                };
                _config.Tasks.Add(tsk);
                _config.DevicesToWatch.Add(dev);
                _config.SaveConfig();
            }
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
            AvailableDevices = new ObservableCollection<MediaDevice>(MediaDevice.GetDevices().ToList());

            foreach (var device in AvailableDevices)
            {
                var connectedDevice = new MtpDevice
                {
                    DeviceName = device.Description,
                    DeviceManufacturer = device.Manufacturer,
                    DeviceId = device.DeviceId
                };

                if (_config.DevicesToWatch.Contains(connectedDevice))
                    ProcessTasks(device);
            }
        }

        private void ProcessTasks(MediaDevice device)
        {
            var tasksForThisDevice = _config.Tasks.Where(d => d.SourceDevice.Equals(device)).ToArray().ToArray();
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
                        if (task.DeleteSourceAfterCopying)
                        {
                            //delete file on MTP device
                        }
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



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
