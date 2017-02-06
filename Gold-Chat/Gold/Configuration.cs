using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Gold
{
    /// <summary>
    /// Config
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Config
        /// </summary>
        public Configuration()
        {

        }

        public bool loginEmailNotyfication { get; set; }


        #region getters and setters 
        #endregion getters and setters

        //----------------------------------------------------------------
        //Write Data
        //----------------------------------------------------------------
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
        //----------------------------------------------------------------
        //Load Data
        //---------------------------------------------------------------- 
        public Configuration loadConfig()
        {
            Configuration conf;
            try
            {
                string m_ConfigFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");
                //If file exists
                if (File.Exists(m_ConfigFileName))
                {
                    XmlSerializer xmlSer = new XmlSerializer(typeof(Configuration));
                    StreamReader sReader = new StreamReader(m_ConfigFileName);
                    return conf = (Configuration)xmlSer.Deserialize(sReader);
                    //sReader.Close();
                }

                return new Configuration();
                //Show Data
                //ConfigToForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return new Configuration();
        }
    }
}
