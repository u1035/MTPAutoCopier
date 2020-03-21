using System.IO;
using System.Threading;
using MediaDevices;

namespace MTPAutoCopier.Models
{
    public class MtpEngine
    {

        private readonly Settings _config = new Settings();
        private Timer _waitForDeviceTimer;
        private bool _timerWorkingNow;

        public MtpEngine()
        {
            _config.LoadConfig();
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
                
            }
            
        }


        static void WriteStreamToDisk(string filePath, MemoryStream memoryStream)
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
