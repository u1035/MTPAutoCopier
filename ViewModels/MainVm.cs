using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MediaDevices;
using MTPAutoCopier.Annotations;
using MTPAutoCopier.Models;
using Prism.Commands;

namespace MTPAutoCopier.ViewModels
{
    public class MainVm : INotifyPropertyChanged
    {
        public MtpEngine Engine { get; set; }

        public ObservableCollection<MediaDevice> AvailableMediaDevices => Engine.AvailableDevices;

        public MediaDevice SelectedDevice
        {
            get => Engine.SelectedDevice;
            set => Engine.SelectedDevice = value;
        }

        public DelegateCommand TestCommand { get; private set; }

        public MainVm()
        {
            Engine = new MtpEngine();
            TestCommand = new DelegateCommand(Engine.Test);
        }





        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
