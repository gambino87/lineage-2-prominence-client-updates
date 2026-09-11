using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace InterludeLauncher {
public class LauncherPayload { public string Version; public string Channel; public string Sha256; public long Size; public string ExeSha256; }
class LauncherFeed { public string AssetBaseUrl; public LauncherPayload Launcher; }
public static class LauncherUpdate {
 public const string Version="1.1.1";
#if TEST_BENCH
 public const string Channel="test-bench";
#else
 public const string Channel="live";
#endif
 static byte[] Fetch(Settings settings,Uri uri,int limit) {
  if(uri.Scheme!="https" && !(settings.AllowLocalFeed && uri.IsFile))throw new IOException("Launcher updates require HTTPS or the private local feed.");
  using(var output=new MemoryStream()) {
   if(uri.IsFile){using(var input=File.OpenRead(uri.LocalPath))Copy(input,output,limit);}
   else {var request=(HttpWebRequest)WebRequest.Create(uri);request.CachePolicy=new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);request.Timeout=30000;request.ReadWriteTimeout=30000;using(var response=request.GetResponse())using(var input=response.GetResponseStream())Copy(input,output,limit);}
   return output.ToArray();
  }
 }
 static void Copy(Stream input,Stream output,int limit){var buffer=new byte[81920];int read,total=0;while((read=input.Read(buffer,0,buffer.Length))>0){total+=read;if(total>limit)throw new IOException("Launcher update exceeds its size limit.");output.Write(buffer,0,read);}}
 public static string Prepare(Settings settings,string home) {
  var feed=new Uri(settings.Feed);var bytes=Fetch(settings,feed,16*1024*1024);
  var signature=Convert.FromBase64String(Encoding.UTF8.GetString(Fetch(settings,new Uri(feed.AbsoluteUri+".sig"),16384)).Trim());
  using(var rsa=new RSACryptoServiceProvider()){rsa.PersistKeyInCsp=false;rsa.FromXmlString(settings.PublicKey);if(!rsa.VerifyData(bytes,CryptoConfig.MapNameToOID("SHA256"),signature))throw new IOException("Launcher update signature is invalid.");}
  var manifest=Patcher.Json.Deserialize<LauncherFeed>(Encoding.UTF8.GetString(bytes));
  var item=manifest.Launcher;
  if(item==null)throw new IOException("This feed predates launcher updates. Use the new launcher package once; later updates use this button.");
  if(item.Channel!=Channel)throw new IOException("Launcher update belongs to a different environment.");
  if(Patcher.FileHash(Path.Combine(home,"Interlude Launcher.exe"))==item.ExeSha256)return null;
  var archive=Fetch(settings,new Uri(new Uri(manifest.AssetBaseUrl),"Launcher.zip"),32*1024*1024);
  if(archive.LongLength!=item.Size || Patcher.Hash(archive)!=item.Sha256)throw new IOException("Launcher package checksum failed.");
  byte[] executable;
  using(var zip=new ZipArchive(new MemoryStream(archive),ZipArchiveMode.Read)) {
   var entry=zip.GetEntry("Interlude Launcher.exe");
   if(zip.Entries.Count!=3 || entry==null || zip.GetEntry("launcher.json")==null || zip.GetEntry("START HERE.txt")==null)throw new IOException("Unexpected launcher package content.");
   using(var output=new MemoryStream()){using(var input=entry.Open())Copy(input,output,16*1024*1024);executable=output.ToArray();}
  }
  if(Patcher.Hash(executable)!=item.ExeSha256)throw new IOException("Launcher executable checksum failed.");
  string staged=Patcher.SafePath(home,".launcher-update/"+Guid.NewGuid().ToString("N")+"/Interlude Launcher.exe");
  Directory.CreateDirectory(Path.GetDirectoryName(staged));File.WriteAllBytes(staged,executable);return staged;
 }
 public static void StartReplacement(string staged,string home) {
  Process.Start(new ProcessStartInfo(staged,"--finish-launcher-update \""+home.TrimEnd('\\')+"\" "+Process.GetCurrentProcess().Id+" "+Patcher.FileHash(staged)){UseShellExecute=false});
 }
 public static void Finish(string home,int parent,string expected,bool restart=true) {
  home=Path.GetFullPath(home);
  string source=Process.GetCurrentProcess().MainModule.FileName;
  string allowed=Patcher.SafePath(home,".launcher-update")+Path.DirectorySeparatorChar;
  if(!source.StartsWith(allowed,StringComparison.OrdinalIgnoreCase) || Patcher.FileHash(source)!=expected)throw new IOException("Invalid staged launcher.");
  try{using(var process=Process.GetProcessById(parent)){if(!process.WaitForExit(60000))throw new IOException("Close the previous launcher before updating.");}}catch(ArgumentException){}
  string target=Patcher.SafePath(home,"Interlude Launcher.exe"),next=Patcher.SafePath(home,".launcher-next.exe"),backup=Patcher.SafePath(home,".launcher-backup.exe");
  File.Copy(source,next,true);
  if(File.Exists(target))File.Replace(next,target,backup,true);else File.Move(next,target);
  // The existing launcher.json (folder, feed and server settings) is never replaced.
  if(restart)Process.Start(new ProcessStartInfo(target){UseShellExecute=true,WorkingDirectory=home});
 }
}
}
