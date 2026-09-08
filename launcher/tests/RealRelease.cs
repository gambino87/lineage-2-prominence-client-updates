using System;using System.IO;using InterludeLauncher;
class RealRelease {
 static void Main(string[] args) {
  var c=Patcher.Json.Deserialize<Settings>(File.ReadAllText(args[0]));
  c.ClientDirectory=Path.GetFullPath(args[1]);
  if(Directory.Exists(c.ClientDirectory))throw new Exception("Use a new isolated test folder.");
  var p=new Patcher(c){ArchiveOverride=Path.GetFullPath(args[2]),TestMode=true};
  int n=0;p.Progress=(s,v)=>{if(++n%150==0)Console.WriteLine(s);};
  p.LoadRelease();p.Apply();if(p.Check().Count!=0)throw new Exception("Incomplete real-client installation");
  Console.WriteLine("Full real-client install and verification passed: "+p.Release.Files.Count+" files.");
 }
}
