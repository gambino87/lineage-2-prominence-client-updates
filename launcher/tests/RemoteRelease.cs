using System;
using System.IO;
using System.Linq;
using System.Net;
using InterludeLauncher;

// Exercises the public feed against the existing disposable full-client fixture only.
class RemoteRelease {
 static void Main(string[] args) {
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  string workspace=Path.GetFullPath(args[0]);
  string root=Path.Combine(workspace,"state/launcher-real-install");
  if(!Directory.Exists(root) || !File.Exists(Patcher.SafePath(root,".launcher/installed.json")))
   throw new Exception("The previously validated isolated test client is required.");
  var config=Patcher.Json.Deserialize<Settings>(File.ReadAllText(args[1]));
  if(config.AllowLocalFeed || !config.Feed.StartsWith("https://github.com/"))
   throw new Exception("This test requires the public HTTPS feed.");
  config.ClientDirectory=root;
  var patcher=new Patcher(config){TestMode=true};
  patcher.LoadRelease();
  if(patcher.Check().Count!=0)throw new Exception("The fixture must match the published release before the download test.");
  var preserved=patcher.Release.Files.Where(f=>f.Preserve).ToDictionary(f=>f.Path,f=>Patcher.FileHash(Patcher.SafePath(root,f.Path)));
  var patches=patcher.Release.Files.Where(f=>!string.IsNullOrEmpty(f.Asset)).ToList();
  foreach(var file in patches) {
   File.WriteAllText(Patcher.SafePath(root,file.Path),"Outdated isolated test content");
   string cache=Patcher.SafePath(root,".launcher/cache/"+file.Asset);
   if(File.Exists(cache))File.Delete(cache);
  }
  if(patcher.Check().Count!=patches.Count)throw new Exception("Did not detect all outdated fixture files.");
  patcher.Progress=(message,value)=>{if(message.StartsWith("Installing "))Console.WriteLine(message);};
  patcher.Apply();
  if(patcher.Check().Count!=0)throw new Exception("Public-feed update did not fully verify.");
  foreach(var file in preserved)if(Patcher.FileHash(Patcher.SafePath(root,file.Key))!=file.Value)
   throw new Exception("Personal INI changed: "+file.Key);
  Console.WriteLine("Public HTTPS release "+patcher.Release.Version+": "+patches.Count+" patches downloaded and applied; "+patcher.Release.Files.Count+" files verified; "+preserved.Count+" INIs preserved. No game launched.");
 }
}
