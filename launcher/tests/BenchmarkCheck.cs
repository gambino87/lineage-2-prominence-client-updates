using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using InterludeLauncher;

class BenchmarkCheck {
 static void Main(string[] args) {
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  string workspace=Path.GetFullPath(args[0]);
  var config=Patcher.Json.Deserialize<Settings>(File.ReadAllText(args[1]));
  config.ClientDirectory=Path.Combine(workspace,"state/launcher-real-install");
  if(!File.Exists(Patcher.SafePath(config.ClientDirectory,".launcher/installed.json")))throw new Exception("The isolated full-client fixture is required");
  var patcher=new Patcher(config);var timer=Stopwatch.StartNew();patcher.LoadRelease();long feed=timer.ElapsedMilliseconds;
  timer.Restart();var before=patcher.Check(true);long full=timer.ElapsedMilliseconds,bytes=patcher.BytesHashed;int hashed=patcher.FilesHashed;
  Console.WriteLine("Full scan completed: "+full+" ms, "+hashed+" files hashed.");
  var reopened=new Patcher(config){Release=patcher.Release};timer.Restart();var after=reopened.Check();long quick=timer.ElapsedMilliseconds;
  if(!before.Select(f=>f.Path).SequenceEqual(after.Select(f=>f.Path)))throw new Exception("Quick and full check results differ");
  if(reopened.FilesHashed!=0)throw new Exception("Unchanged fixture files should not be hashed again");
  var result=new {FeedMilliseconds=feed,FullScanMilliseconds=full,QuickScanMilliseconds=quick,FullFilesHashed=hashed,FullBytesHashed=bytes,QuickFilesHashed=reopened.FilesHashed,QuickFilesReused=reopened.FilesReused,FilesNeedingUpdate=after.Count,ClientFiles=patcher.Release.Files.Count,TestDirectory=config.ClientDirectory};
  Patcher.WriteJson(Path.Combine(workspace,"state/launcher/startup-benchmark.json"),result);
  Console.WriteLine(Patcher.Json.Serialize(result));
 }
}
