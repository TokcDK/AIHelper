using System;
using System.IO;
using System.Xml;

namespace AIHelper.Manage
{
    class ManageXml
    {
        /// <summary>
        /// changes UserData/setup.xml values
        /// </summary>
        /// <param name="xmlpath"></param>
        /// <param name="nodename"></param>
        /// <param name="value"></param>
        public static void ChangeSetupXmlValue(string xmlpath, string nodename, string value)
        {
            if(string.IsNullOrWhiteSpace(xmlpath))
            {
                return;
            }

            //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
#pragma warning disable CA3075 // Insecure DTD processing in XML
            XmlDocument xmlDoc = new XmlDocument();
#pragma warning restore CA3075 // Insecure DTD processing in XML
            var xmlFound = File.Exists(xmlpath);
            if (xmlFound)
            {
#pragma warning disable CA3075 // Insecure DTD processing in XML
                xmlDoc.Load(xmlpath);
#pragma warning restore CA3075 // Insecure DTD processing in XML
            }
            else
            {
                //если не был создан, создать с нуля
#pragma warning disable CA3075 // Insecure DTD processing in XML
                xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-16\"?>"
                    + Environment.NewLine + "<Setting>"
                    + Environment.NewLine + "  <Size>1280 x 720 (16 : 9)</Size>"
                    + Environment.NewLine + "  <Width>1280</Width>"
                    + Environment.NewLine + "  <Height>720</Height>"
                    + Environment.NewLine + "  <Quality>2</Quality>"
                    + Environment.NewLine + "  <FullScreen>false</FullScreen>"
                    + Environment.NewLine + "  <Display>0</Display>"
                    + Environment.NewLine + "  <Language>0</Language>"
                    + Environment.NewLine + "</Setting>");
#pragma warning restore CA3075 // Insecure DTD processing in XML
            }

            XmlNode node = xmlDoc.SelectSingleNode(nodename);

            if (node == null || (node.InnerText == value && xmlFound))
            {
            }
            else
            {
                node.InnerText = value;

                if (!xmlFound)
                {
                    xmlpath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(xmlpath, false, true);
                }

                xmlDoc.Save(xmlpath);
            }
        }

        public static string ReadXmlValue(string xmlpath, string nodename, string defaultresult = "")
        {
            if (File.Exists(xmlpath))
            {
                //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
#pragma warning disable CA3075 // Insecure DTD processing in XML
                XmlDocument xmlDoc = new XmlDocument();
#pragma warning restore CA3075 // Insecure DTD processing in XML

#pragma warning disable CA3075 // Insecure DTD processing in XML
                xmlDoc.Load(xmlpath);
#pragma warning restore CA3075 // Insecure DTD processing in XML

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

        public static string ReadXmlValue(Stream xmlStream, string nodename, string defaultresult = "")
        {
            if (xmlStream != null)
            {
                try
                {
                    //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
#pragma warning disable CA3075 // Insecure DTD processing in XML
                    XmlDocument xmlDoc = new XmlDocument();
#pragma warning restore CA3075 // Insecure DTD processing in XML

#pragma warning disable CA3075 // Insecure DTD processing in XML
                    xmlDoc.Load(xmlStream);
#pragma warning restore CA3075 // Insecure DTD processing in XML

                    XmlNode node = xmlDoc.SelectSingleNode(nodename);

                    if (node == null || node.InnerText == defaultresult)
                    {
                    }
                    else
                    {
                        return node.InnerText;
                    }
                }
                catch
                {
                }
            }
            return defaultresult;
        }
    }
}
