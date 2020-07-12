using MediaDevices;
using MTPAutoCopier.Models;
using MTPAutoCopier.MVVM;

namespace MTPAutoCopier.ViewModels
{
    public class MainVm : NotificationObject
    {
        public MtpEngine Engine { get; set; }

        public MediaDevice SelectedDevice
        {
            get => Engine.SelectedDevice;
            set
            {
                Engine.SelectedDevice = value;
                RaisePropertyChanged(nameof(IsAddTaskCommandAvailable));
            }
        }

        public bool IsAddTaskCommandAvailable => SelectedDevice != null;

        public Command ProcessTaskCommand { get; }
        public Command RefreshDevicesListCommand { get; }
        public Command AddTaskCommand { get; }

        public MainVm()
        {
            Engine = new MtpEngine();
            ProcessTaskCommand = new Command(Engine.ProcessTask);
            RefreshDevicesListCommand = new Command(Engine.RefreshDevicesList);
            AddTaskCommand = new Command(Engine.AddTask);
        }


    }
}
