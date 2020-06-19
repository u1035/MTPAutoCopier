﻿using MediaDevices;
using MTPAutoCopier.Models;
using MTPAutoCopier.MVVM;

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

        public Command ProcessTask { get; private set; }
        public Command RefreshDevicesList { get; private set; }

        public MainVm()
        {
            Engine = new MtpEngine();
            ProcessTask = new Command(Engine.ProcessTask);
            RefreshDevicesList = new Command(Engine.RefreshDevicesList);
        }


    }
}
