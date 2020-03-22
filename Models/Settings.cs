using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace MTPAutoCopier.Models
{
    [Serializable]
    public class Settings
    {
        public List<MtpTask> Tasks { get; set; } = new List<MtpTask>();
        public List<MtpDevice> DevicesToWatch { get; set; } = new List<MtpDevice>();



        public void SaveConfig()
        {
            string configFileName = Environment.CurrentDirectory + "\\config.xml";
            var serializer = new XmlSerializer(typeof(Settings));

            try
            {
                using (Stream writer = new FileStream(configFileName, FileMode.OpenOrCreate))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error while saving config - {ex.Message}");
            }

        }

        public static Settings LoadConfig()
        {
            string configFileName = Environment.CurrentDirectory + "\\config.xml";
            var serializer = new XmlSerializer(typeof(Settings));

            try
            {
                if (File.Exists(configFileName))
                {
                    using (Stream reader = new FileStream(configFileName, FileMode.Open))
                    {
                        return (Settings)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    Debug.Print($"No such file - {configFileName}");
                    return new Settings();
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error while loading config - {ex.Message}");
                return new Settings();
            }

        }
    }
}
