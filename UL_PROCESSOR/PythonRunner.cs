// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.PythonRunner
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Diagnostics;

namespace UL_PROCESSOR
{
  public class PythonRunner
  {
    public string myPythonApp1 = "..//..//researchtopic1.py";
    public string myPythonApp = "..//..//test1.py";
    public string python = "C://Users//Psychology//Anaconda3/python.exe";

    public void runScript()
    {
      int num1 = 2;
      int num2 = 5;
      ProcessStartInfo processStartInfo = new ProcessStartInfo(this.python);
      processStartInfo.UseShellExecute = false;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.Arguments = this.myPythonApp + " " + (object) num1 + " " + (object) num2;
      Process process = new Process();
      process.StartInfo = processStartInfo;
      Console.WriteLine("Calling Python script with arguments {0} and {1}", (object) num1, (object) num2);
      process.Start();
      string str = process.StandardOutput.ReadLine();
      process.WaitForExit();
      process.Close();
      Console.WriteLine("Value received from script: " + str);
    }
  }
}
