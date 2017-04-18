using System.IO;
using System.Xml;

namespace Host.XMLReader
{
    public class DataReader
    {
        public string GetSettings()
        {
            string address = "";
            foreach (string file in GetXMLFiles())
            {
                XmlReader xmlReader = XmlReader.Create(file, new XmlReaderSettings { IgnoreWhitespace = true });
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "Connection"))
                    {
                        if (xmlReader.HasAttributes)
                        {
                            address = xmlReader.GetAttribute("Address");
                        }
                    }
                }
            }
            return address;
        }

        private string[] GetXMLFiles()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Data\");
        }
    }
}
