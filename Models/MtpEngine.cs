﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaDevices;
using Prism.Mvvm;

namespace MTPAutoCopier.Models
{
    public class MtpEngine : BindableBase
    {
        private ObservableCollection<MediaDevice> _availableDevices;
        private ObservableCollection<MtpTask> _tasksForSelectedDevice;
        private MediaDevice _selectedDevice;
        private Settings _config;
        private string _log;
        private Dispatcher _dispatcher;

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

        public ObservableCollection<MediaDevice> AvailableDevices
        {
            get => _availableDevices;
            set => SetProperty(ref _availableDevices, value);
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

        public ObservableCollection<MtpTask> TasksForSelectedDevice
        {
            get => _tasksForSelectedDevice;
            set => SetProperty(ref _tasksForSelectedDevice, value);
        }

        public MtpEngine()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _config = Settings.LoadConfig();
            AvailableDevices = new ObservableCollection<MediaDevice>(MediaDevice.GetDevices().ToList());
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
                    var files = sourceDirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (task.CreateSubfolder)
                        {
                            Directory.CreateDirectory(task.DestinationPath + "\\" + DateTime.Now.ToString(task.SubfolderFormat));
                            CopyFile(device, file, task.DestinationPath + "\\" + DateTime.Now.ToString(task.SubfolderFormat));
                        }
                        else
                        {
                            CopyFile(device, file, task.DestinationPath);
                        }

                        if (task.DeleteSourceAfterCopying)
                        {
                            //delete file on MTP device
                        }
                    }
                    device.Disconnect();
                }
            }
            catch (DirectoryNotFoundException)
            {

            }

        }

        private void CopyFile(MediaDevice device, MediaFileInfo file, string destinationDir)
        {
            _dispatcher.Invoke(() => { Log += $"Processing file {file.FullName}{Environment.NewLine}"; }, DispatcherPriority.DataBind);

            MemoryStream memoryStream = new MemoryStream();
            device.DownloadFile(file.FullName, memoryStream);
            memoryStream.Position = 0;
            WriteStreamToDisk($"{destinationDir}\\{file.Name}", memoryStream);
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



        //public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
