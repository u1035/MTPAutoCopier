using MediaDevices;
using MTPAutoCopier.Models;
using Prism.Commands;

namespace MTPAutoCopier.ViewModels
{
    public class MainVm
    {
        public MtpEngine Engine { get; set; }

        public MediaDevice SelectedDevice
        {
            get => Engine.SelectedDevice;
            set => Engine.SelectedDevice = value;
        }

        public DelegateCommand ProcessTask { get; private set; }
        public DelegateCommand RefreshDevicesList { get; private set; }

        public MainVm()
        {
            Engine = new MtpEngine();
            ProcessTask = new DelegateCommand(Engine.ProcessTask);
            RefreshDevicesList = new DelegateCommand(Engine.RefreshDevicesList);
        }


    }
}
