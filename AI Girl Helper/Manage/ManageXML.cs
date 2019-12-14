using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AIHelper.Manage
{
    class ManageXML
    {
        public static void ChangeXmlValue(string xmlpath, string nodename, string value)
        {
            //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlpath))
            {
                xmlDoc.Load(xmlpath);
            }
            else
            {
                //если не был создан, создать с нуля
                xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine + "<Setting>" + Environment.NewLine + "  <Size>1280 x 720 (16 : 9)</Size>" + Environment.NewLine + "  <Width>1280</Width>" + Environment.NewLine + "  <Height>720</Height>" + Environment.NewLine + "  <Quality>2</Quality>" + Environment.NewLine + "  <FullScreen>false</FullScreen>" + Environment.NewLine + "  <Display>0</Display>" + Environment.NewLine + "  <Language>0</Language>" + Environment.NewLine + "</Setting>");
            }

            XmlNode node = xmlDoc.SelectSingleNode(nodename);

            if (node == null || node.InnerText == value)
            {
            }
            else
            {
                node.InnerText = value;

                xmlDoc.Save(xmlpath);
            }
        }

        public static string ReadXmlValue(string xmlpath, string nodename, string defaultresult)
        {
            if (File.Exists(xmlpath))
            {
                //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(xmlpath);

                XmlNode node = xmlDoc.SelectSingleNode(nodename);

                if (node == null || node.InnerText == defaultresult)
                {
                }
                else
                {
                    return node.InnerText;
                }
            }
            return defaultresult;
        }
    }
}
