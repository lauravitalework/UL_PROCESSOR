using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml; 
namespace UL_PROCESSOR
{
    public class ItsReader
    {
        public void read()
        {
            String fn = "C:\\LVL\\LADYBUGS1\\LENA_Data\\ITS\\e20170306_105912_014870.its";
            XmlDocument doc = new XmlDocument();
            doc.Load(fn);
            XmlNodeList nodes = doc.ChildNodes[2].SelectNodes("ProcessingUnit/Recording/Conversation");


            foreach (XmlNode conv in nodes)
            {
                XmlNodeList segments = conv.SelectNodes("Segment");
                foreach (XmlNode seg in segments)
                {
                     

                }

            }
            XmlTextReader textReader = new XmlTextReader(fn);
            textReader.Read();
            // If the node has value
            while (textReader.Read())
            {
                // Move to fist element
                textReader.MoveToElement();
                Console.WriteLine("XmlTextReader Properties Test");
                Console.WriteLine("===================");
                // Read this element's properties and display them on console
                Console.WriteLine("Name:" + textReader.Name);
                Console.WriteLine("Base URI:" + textReader.BaseURI);
                Console.WriteLine("Local Name:" + textReader.LocalName);
                Console.WriteLine("Attribute Count:" + textReader.AttributeCount.ToString());
                Console.WriteLine("Depth:" + textReader.Depth.ToString());
                Console.WriteLine("Line Number:" + textReader.LineNumber.ToString());
                Console.WriteLine("Node Type:" + textReader.NodeType.ToString());
                Console.WriteLine("Attribute Count:" + textReader.Value.ToString());
            }
        }
    }
}
