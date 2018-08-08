// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.ItsReader
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Xml;

namespace UL_PROCESSOR
{
  public class ItsReader
  {
    public void read()
    {
      string str = "C:\\LVL\\LADYBUGS1\\LENA_Data\\ITS\\e20170306_105912_014870.its";
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(str);
      foreach (XmlNode selectNode1 in xmlDocument.ChildNodes[2].SelectNodes("ProcessingUnit/Recording/Conversation"))
      {
        foreach (XmlNode selectNode2 in selectNode1.SelectNodes("Segment"))
          ;
      }
      XmlTextReader xmlTextReader = new XmlTextReader(str);
      xmlTextReader.Read();
      while (xmlTextReader.Read())
      {
        xmlTextReader.MoveToElement();
        Console.WriteLine("XmlTextReader Properties Test");
        Console.WriteLine("===================");
        Console.WriteLine("Name:" + xmlTextReader.Name);
        Console.WriteLine("Base URI:" + xmlTextReader.BaseURI);
        Console.WriteLine("Local Name:" + xmlTextReader.LocalName);
        Console.WriteLine("Attribute Count:" + xmlTextReader.AttributeCount.ToString());
        Console.WriteLine("Depth:" + xmlTextReader.Depth.ToString());
        Console.WriteLine("Line Number:" + xmlTextReader.LineNumber.ToString());
        Console.WriteLine("Node Type:" + xmlTextReader.NodeType.ToString());
        Console.WriteLine("Attribute Count:" + xmlTextReader.Value.ToString());
      }
    }
  }
}
