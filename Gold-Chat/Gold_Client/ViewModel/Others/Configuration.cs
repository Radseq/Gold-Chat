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

        public void SaveConfig(Configuration conf)
        {
            try
            {
                string m_ConfigFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");
                //FormToConfig();
                XmlSerializer xmlSer = new XmlSerializer(typeof(Configuration));
                FileStream fStream = new FileStream(m_ConfigFileName, FileMode.Create);
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
                string m_ConfigFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");

                if (File.Exists(m_ConfigFileName))
                {
                    XmlSerializer xmlSer = new XmlSerializer(typeof(Configuration));
                    StreamReader sReader = new StreamReader(m_ConfigFileName);
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
