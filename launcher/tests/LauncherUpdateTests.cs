using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using InterludeLauncher;
class LauncherUpdateTests {
 static int checks;
 static void Check(bool ok){if(!ok)throw new Exception("Check failed "+checks);checks++;}
 static void Reject(Action action){try{action();}catch(IOException){checks++;return;}throw new Exception("Unsafe update accepted");}
 static int Main(string[] args){
  if(args.Length>0 && args[0]=="replace"){LauncherUpdate.Finish(args[1],int.MaxValue,Patcher.FileHash(Process.GetCurrentProcess().MainModule.FileName),false);return 0;}
  string home=Path.GetFullPath(args[0]);Directory.CreateDirectory(home);string feed=Path.Combine(home,"feed");Directory.CreateDirectory(feed);
  string target=Path.Combine(home,"Interlude Launcher.exe");File.WriteAllText(target,"old launcher");File.WriteAllText(Path.Combine(home,"launcher.json"),"KEEP SETTINGS");
  byte[] exe=File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
  string package=Path.Combine(feed,"Launcher.zip");using(var zip=ZipFile.Open(package,ZipArchiveMode.Create)){using(var output=zip.CreateEntry("Interlude Launcher.exe").Open())output.Write(exe,0,exe.Length);zip.CreateEntry("launcher.json");zip.CreateEntry("START HERE.txt");}
  var payload=new LauncherPayload {Version="test",Channel=LauncherUpdate.Channel,Size=new FileInfo(package).Length,Sha256=Patcher.FileHash(package),ExeSha256=Patcher.Hash(exe)};
  string manifest=Path.Combine(feed,"manifest.json");
  using(var rsa=new RSACryptoServiceProvider(2048)){rsa.PersistKeyInCsp=false;var settings=new Settings{Feed=new Uri(manifest).AbsoluteUri,PublicKey=rsa.ToXmlString(false),AllowLocalFeed=true};
   Action sign=()=>{Patcher.WriteJson(manifest,new {AssetBaseUrl=new Uri(feed+Path.DirectorySeparatorChar).AbsoluteUri,Launcher=payload});File.WriteAllText(manifest+".sig",Convert.ToBase64String(rsa.SignData(File.ReadAllBytes(manifest),CryptoConfig.MapNameToOID("SHA256"))));};
   sign();string staged=LauncherUpdate.Prepare(settings,home);Check(Patcher.FileHash(staged)==payload.ExeSha256);Check(File.ReadAllText(target)=="old launcher");
   using(var child=Process.Start(new ProcessStartInfo(staged,"replace \""+home+"\""){UseShellExecute=false})){child.WaitForExit();Check(child.ExitCode==0);}
   Check(Patcher.FileHash(target)==payload.ExeSha256);Check(File.ReadAllText(Path.Combine(home,"launcher.json"))=="KEEP SETTINGS");Check(File.ReadAllText(Path.Combine(home,".launcher-backup.exe"))=="old launcher");Check(LauncherUpdate.Prepare(settings,home)==null);
   payload.Channel="wrong-channel";sign();Reject(()=>LauncherUpdate.Prepare(settings,home));payload.Channel=LauncherUpdate.Channel;sign();File.AppendAllText(manifest," ");Reject(()=>LauncherUpdate.Prepare(settings,home));
   File.WriteAllText(target,"old launcher");sign();File.AppendAllText(package,"tampered");Reject(()=>LauncherUpdate.Prepare(settings,home));
  }
  Console.WriteLine(checks+" launcher self-update checks passed");return 0;
 }
}
