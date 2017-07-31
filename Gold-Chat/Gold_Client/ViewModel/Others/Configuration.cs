using Gold_Client.Model;
using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Gold_Client.ViewModel
{
    public class Configuration<T> : IConfiguration<T>
    {
        string ConfigFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");
        XmlSerializer xmlSer = new XmlSerializer(typeof(T));

        public void SaveConfig(T item)
        {
            try
            {
                FileStream fStream = new FileStream(ConfigFile, FileMode.Create);
                xmlSer.Serialize(fStream, item);
                fStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public T LoadConfig(T obj)
        {
            T conf;
            try
            {
                if (File.Exists(ConfigFile))
                {
                    XmlSerializer xmlSer = new XmlSerializer(typeof(T));
                    StreamReader sReader = new StreamReader(ConfigFile);
                    return conf = (T)xmlSer.Deserialize(sReader);
                }
                else
                {
                    FileStream fStream = new FileStream(ConfigFile, FileMode.Create);
                    xmlSer.Serialize(fStream, obj);
                    fStream.Close();
                }

                return obj;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return obj;
        }
    }
}
