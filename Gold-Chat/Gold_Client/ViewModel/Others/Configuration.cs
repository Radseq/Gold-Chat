using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Gold_Client.ViewModel
{
    public class Configuration
    {
        public bool loginEmailNotyfication { get; set; }
        public string SaveFilePatch { get; set; }

        string ConfigFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");
        XmlSerializer xmlSer = new XmlSerializer(typeof(Configuration));

        public void SaveConfig(Configuration conf)
        {
            try
            {
                FileStream fStream = new FileStream(ConfigFile, FileMode.Create);
                xmlSer.Serialize(fStream, conf);
                fStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public Configuration loadConfig()
        {
            Configuration conf;
            try
            {
                if (File.Exists(ConfigFile))
                {
                    StreamReader sReader = new StreamReader(ConfigFile);
                    return conf = (Configuration)xmlSer.Deserialize(sReader);
                }

                return new Configuration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return new Configuration();
        }
    }
}
