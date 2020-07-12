using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaDevices;
using MTPAutoCopier.MVVM;
using MTPAutoCopier.Views;

namespace MTPAutoCopier.Models
{
    public class MtpEngine : NotificationObject
    {
        private MediaDevice _selectedDevice;
        private Settings _config;
        private string _log;
        private readonly Dispatcher _dispatcher;

        public string Log
        {
            get => _log;
            set => SetProperty(ref _log, value);
        }

        public Settings Config
        {
            get => _config;
            set => SetProperty(ref _config, value);
        }

        public MediaDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                SetProperty(ref _selectedDevice, value);
                ShowTasksForSelectedDevice();
            }
        }

        public ObservableCollection<MediaDevice> AvailableDevices { get; private set; }
        public ObservableCollection<MtpTask> TasksForSelectedDevice { get; private set; } 

        public MtpEngine()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _config = Settings.LoadConfig();
            TasksForSelectedDevice = new ObservableCollection<MtpTask>();
            AvailableDevices = new ObservableCollection<MediaDevice>(MediaDevice.GetDevices().ToList());
        }

        public void AddTask()
        {
            if (SelectedDevice == null)
                return;

            var device = new MtpDevice
            {
                DeviceName = SelectedDevice.Description,
                DeviceManufacturer = SelectedDevice.Manufacturer,
                DeviceId = SelectedDevice.DeviceId
            };

            var newTask = new MtpTask()
            {
                SourceDevice = device
            };

            var taskEditor = new TaskEditView() {DataContext = newTask};
            var result = taskEditor.ShowDialog();
            if (result != null && result == true)
            {
                _config.Tasks.Add(newTask);
                _config.DevicesToWatch.Add(device);
                _config.SaveConfig();
            }
        }

        private void ShowTasksForSelectedDevice()
        {
            if (SelectedDevice != null)
            {
                TasksForSelectedDevice = new ObservableCollection<MtpTask>(Config.Tasks.Where(o =>
                    o.SourceDevice.DeviceName == SelectedDevice.Description &&
                    o.SourceDevice.DeviceManufacturer == SelectedDevice.Manufacturer &&
                    o.SourceDevice.DeviceId == SelectedDevice.DeviceId).ToArray());
            }
            else
            {
                TasksForSelectedDevice = new ObservableCollection<MtpTask>();
            }
        }

        public void ProcessTask()
        {
            CheckForConnectedDevices();
            //if (SelectedDevice != null)
            //{
            //    var dev = new MtpDevice
            //    {
            //        DeviceName = SelectedDevice.Description,
            //        DeviceManufacturer = SelectedDevice.Manufacturer,
            //        DeviceId = SelectedDevice.DeviceId
            //    };
            //    var tsk = new MtpTask
            //    {
            //        SourceDevice = dev,
            //        SourcePath = "Съемное хранилище\\DCIM\\100NIKON",
            //        DestinationPath = "C:\\cam1_records"
            //    };
            //    _config.Tasks.Add(tsk);
            //    _config.DevicesToWatch.Add(dev);
            //    _config.SaveConfig();
            //}
        }

        public void RefreshDevicesList()
        {
            AvailableDevices = new ObservableCollection<MediaDevice>(MediaDevice.GetDevices().ToList());
        }

        private void CheckForConnectedDevices()
        {
            if (Config.DevicesToWatch.Any(o => o.DeviceName == SelectedDevice.Description &&
                                               o.DeviceManufacturer == SelectedDevice.Manufacturer &&
                                               o.DeviceId == SelectedDevice.DeviceId))
            {
                var tsk = new Task(() => { ProcessTasks(SelectedDevice); });
                tsk.Start();
            }
        }

        private void ProcessTasks(MediaDevice device)
        {
            Log = "";
            var tasksForThisDevice = Config.Tasks.Where(o => o.SourceDevice.DeviceName == device.Description &&
                                                            o.SourceDevice.DeviceManufacturer == device.Manufacturer &&
                                                            o.SourceDevice.DeviceId == device.DeviceId).ToArray();
            if (!tasksForThisDevice.Any())
                return;

            try
            {
                foreach (var task in tasksForThisDevice)
                {
                    device.Connect();
                    var sourceDirInfo = device.GetDirectoryInfo(task.SourcePath);
                    var files = sourceDirInfo.EnumerateFiles("*.*");
                    int i = 0;
                    foreach (var file in files)
                    {
                        if (task.CreateSubfolder)
                        {
                            Directory.CreateDirectory(task.DestinationPath + "\\" + DateTime.Now.ToString(task.SubfolderFormat));
                            CopyFile(device, file, task.DestinationPath + "\\" + DateTime.Now.ToString(task.SubfolderFormat));
                        }
                        else
                        {
                            if (!Directory.Exists(task.DestinationPath))
                                Directory.CreateDirectory(task.DestinationPath);
                            CopyFile(device, file, task.DestinationPath);
                        }

                        if (task.DeleteSourceAfterCopying)
                        {
                            device.DeleteFile(file.FullName);
                        }

                        i++;
                        _dispatcher.Invoke(() => { Log = $"Processed {i} files. Last processed file - {file.FullName}"; }, DispatcherPriority.DataBind);
                    }
                    device.Disconnect();
                }
            }
            catch (DirectoryNotFoundException)
            {

            }
            _dispatcher.Invoke(() => { Log = "All task completed"; }, DispatcherPriority.DataBind);
        }

        private void CopyFile(MediaDevice device, MediaFileInfo file, string destinationDir)
        {
            var destinationFile = $"{destinationDir}\\{file.Name}";
            if (!File.Exists(destinationFile))
            {
                MemoryStream memoryStream = new MemoryStream();
                device.DownloadFile(file.FullName, memoryStream);
                memoryStream.Position = 0;
                WriteStreamToDisk(destinationFile, memoryStream);
            }
            else
            {
                _dispatcher.Invoke(() => { Log = $"File {file.Name} already exists in destination directory"; }, DispatcherPriority.DataBind);
            }
        }

        private void WriteStreamToDisk(string filePath, MemoryStream memoryStream)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                file.Write(bytes, 0, bytes.Length);
                memoryStream.Close();
            }
        }
    }
}
