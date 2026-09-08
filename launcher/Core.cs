using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace InterludeLauncher {
public class Settings {
 public string Title = "Interlude Test Launcher";
 public string Feed = "";
 public string PublicKey = "";
 public bool AllowLocalFeed = false;
 public string ClientDirectory = "";
 public string ServerAddress = "127.0.0.1";
}
public class BaseArchive { public string Url; public string Sha256; public long Size; }
public class ClientFile {
 public string Path; public string Sha256; public long Size;
 public string Asset; public string AssetSha256; public long AssetSize;
 public bool Preserve;
}
public class Manifest {
 public int Schema; public string Version; public string Notes; public string Published; public string AssetBaseUrl;
 public BaseArchive Base; public List<ClientFile> Files;
}
public class Change { public string Path; public bool Existed; }
public class Journal { public string Phase; public List<Change> Changes = new List<Change>(); }

public class Patcher {
 public static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024 };
 public Settings Config;
 public Manifest Release;
 public Action<string,int> Progress = delegate {};
 public string ArchiveOverride;
 public bool TestMode;
 public int FailAfter = -1;
 string root;
 public Patcher(Settings config) { Config=config; }
 public static string Hash(byte[] bytes) { using(var h=SHA256.Create()) return Hex(h.ComputeHash(bytes)); }
 public static string FileHash(string file) { using(var h=SHA256.Create()) using(var f=File.OpenRead(file)) return Hex(h.ComputeHash(f)); }
 static string Hex(byte[] bytes) { return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant(); }
 public static void WriteJson(string path, object value) {
  Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)));
  string temp=path+".tmp"; File.WriteAllText(temp,Json.Serialize(value));
  if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
 }
 public static string SafePath(string directory, string relative) {
  if(string.IsNullOrWhiteSpace(relative) || relative.Contains(":") || relative.Contains("\\") || relative.StartsWith("/") || relative.Split('/').Any(p=>p==".." || p=="." || p.Length==0 || p.EndsWith(".") || p.EndsWith(" ") || p.IndexOfAny(System.IO.Path.GetInvalidFileNameChars())>=0 || System.Text.RegularExpressions.Regex.IsMatch(p,@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\.|$)",System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
   throw new IOException("Unsafe file path in update: "+relative);
  string baseDir=System.IO.Path.GetFullPath(directory).TrimEnd('\\')+"\\";
  string full=System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir,relative.Replace('/','\\')));
  if(!full.StartsWith(baseDir,StringComparison.OrdinalIgnoreCase)) throw new IOException("Update path escapes the client folder.");
  string check=full;
  while(check!=null) {
   if((File.Exists(check)||Directory.Exists(check)) && (File.GetAttributes(check)&FileAttributes.ReparsePoint)!=0) throw new IOException("Linked folders are not supported: "+check);
   check=System.IO.Path.GetDirectoryName(check);
  }
  return full;
 }
 static void ValidHash(string hash) { if(hash==null || hash.Length!=64 || hash.Any(c=>!Uri.IsHexDigit(c))) throw new IOException("Invalid checksum in release."); }
 public static void Validate(Manifest m) {
  if(m==null || m.Schema!=1 || string.IsNullOrWhiteSpace(m.Version) || m.Base==null || m.Files==null || m.Files.Count==0) throw new IOException("Unsupported or empty release manifest.");
  ValidHash(m.Base.Sha256); if(m.Base.Size<=0) throw new IOException("Invalid base archive size.");
  var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var f in m.Files) {
   SafePath(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"l2-path-validation"),f.Path);
   if(f.Path.StartsWith(".launcher",StringComparison.OrdinalIgnoreCase)||!seen.Add(f.Path)) throw new IOException("Duplicate or reserved update path.");
   ValidHash(f.Sha256); if(f.Size<0 || f.Size>2L*1024*1024*1024) throw new IOException("Invalid client file size.");
   if(!string.IsNullOrEmpty(f.Asset)) {
    if(f.Asset.Contains("/")||f.Asset.Contains("\\")||f.Asset.Contains(":")) throw new IOException("Invalid asset name.");
    ValidHash(f.AssetSha256); if(f.AssetSize<=0) throw new IOException("Invalid asset size.");
   }
  }
  if(!seen.Contains("system/L2.exe") || !seen.Contains("system/psetup.dll")) throw new IOException("Release is missing client essentials.");
 }
 Uri Address(string location) {
  Uri u; if(!Uri.TryCreate(location,UriKind.Absolute,out u)) u=new Uri(System.IO.Path.GetFullPath(location));
  if(u.Scheme!="https" && !(Config.AllowLocalFeed && u.IsFile)) throw new IOException("Updates require HTTPS. Local feeds must be explicitly enabled.");
  return u;
 }
 byte[] GetSmall(Uri u, int max) {
  if(u.IsFile) { if(new FileInfo(u.LocalPath).Length>max)throw new IOException("Update metadata is too large.");return File.ReadAllBytes(u.LocalPath); }
  var req=(HttpWebRequest)WebRequest.Create(u); req.UserAgent="InterludeLauncher/1"; req.Timeout=30000;
  using(var res=(HttpWebResponse)req.GetResponse()) {
   if(res.ResponseUri.Scheme!="https")throw new IOException("Insecure update redirect.");
   using(var input=res.GetResponseStream()) using(var output=new MemoryStream()) {var b=new byte[65536];int n;while((n=input.Read(b,0,b.Length))>0){if(output.Length+n>max)throw new IOException("Update metadata is too large.");output.Write(b,0,n);}return output.ToArray();}
  }
 }
 public Manifest LoadRelease() {
  if(string.IsNullOrWhiteSpace(Config.Feed)||string.IsNullOrWhiteSpace(Config.PublicKey))throw new IOException("The launcher has no update channel configured yet.");
  Progress("Checking for updates…",0);
  Uri u=Address(Config.Feed);var bytes=GetSmall(u,16*1024*1024);
  var sig=Convert.FromBase64String(Encoding.UTF8.GetString(GetSmall(new Uri(u.AbsoluteUri+".sig"),16384)).Trim());
  using(var rsa=new RSACryptoServiceProvider()) {rsa.PersistKeyInCsp=false;rsa.FromXmlString(Config.PublicKey);if(!rsa.VerifyData(bytes,CryptoConfig.MapNameToOID("SHA256"),sig))throw new IOException("The update signature could not be verified. No files were changed.");}
  Release=Json.Deserialize<Manifest>(Encoding.UTF8.GetString(bytes));Validate(Release);return Release;
 }
 public static void ValidateServer(string host) {
  IPAddress ip;if(!IPAddress.TryParse(host,out ip)||ip.AddressFamily!=AddressFamily.InterNetwork||host!=ip.ToString())throw new IOException("Enter a full IPv4 server address, such as 100.80.20.10.");
 }
 public static byte[] ConnectionBytes(byte[] canonical,string host) {
  ValidateServer(host);
  if(canonical.Length!=90624 || Hash(canonical)!="f2793b9097b33b3e3f3aa0956e72f75ce70b0c2b24446a89e23e40ea0b0bf893")throw new IOException("Unrecognized connection DLL. Connection settings were not changed.");
  byte[] bytes=(byte[])canonical.Clone();Array.Clear(bytes,0x12f7c,16);Encoding.ASCII.GetBytes(host).CopyTo(bytes,0x12f7c);return bytes;
 }
 bool Matches(ClientFile f, string directory) {
  string path=SafePath(directory,f.Path);if(!File.Exists(path))return false;
  if(f.Preserve)return true;
  if(new FileInfo(path).Length!=f.Size)return false;
  if(f.Path.Equals("system/psetup.dll",StringComparison.OrdinalIgnoreCase)) {
   byte[] bytes=File.ReadAllBytes(path);if(bytes.Length!=90624)return false;
   string host=Encoding.ASCII.GetString(bytes,0x12f7c,16).TrimEnd('\0');
   Array.Clear(bytes,0x12f7c,16);Encoding.ASCII.GetBytes("127.0.0.1").CopyTo(bytes,0x12f7c);
   return Hash(bytes)==f.Sha256 && host==Config.ServerAddress;
  }
  return FileHash(path)==f.Sha256;
 }
 void SetRoot() {
  if(string.IsNullOrWhiteSpace(Config.ClientDirectory))throw new IOException("Choose a client folder first.");
  root=System.IO.Path.GetFullPath(Config.ClientDirectory).TrimEnd('\\');
  if(root.Length<4 || root.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows),StringComparison.OrdinalIgnoreCase))throw new IOException("Choose a dedicated client folder.");
  SafePath(root,".launcher/probe");ValidateServer(Config.ServerAddress);
 }
 public List<ClientFile> Check() {
  SetRoot(); if(Release==null)LoadRelease();
  if(File.Exists(SafePath(root,".launcher/transaction.json")))throw new IOException("An interrupted update needs recovery. Click Update / Repair.");
  var needed=new List<ClientFile>();int n=0;
  foreach(var f in Release.Files) {Progress("Checking "+f.Path,(++n)*100/Release.Files.Count);if(!Matches(f,root))needed.Add(f);}
  Progress(needed.Count==0?"Ready to play":"Update available — "+needed.Count+" files",100);return needed;
 }
 void AssertClosed() {if(!TestMode && Process.GetProcessesByName("l2").Length>0)throw new IOException("Close Lineage II before applying updates.");}
 void Download(Uri url,string path,string hash,long length) {
  if(File.Exists(path) && new FileInfo(path).Length==length && FileHash(path)==hash)return;
  Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));string temp=path+".partial";
  try {
   if(url.IsFile)File.Copy(url.LocalPath,temp,true);
   else {
    var req=(HttpWebRequest)WebRequest.Create(url);req.UserAgent="InterludeLauncher/1";req.Timeout=30000;req.ReadWriteTimeout=60000;
    using(var res=(HttpWebResponse)req.GetResponse()) {
     if(res.ResponseUri.Scheme!="https")throw new IOException("Insecure download redirect.");
     using(var input=res.GetResponseStream()) using(var output=File.Create(temp)) {
      byte[] b=new byte[1024*1024];int n;long total=0;
      while((n=input.Read(b,0,b.Length))>0){total+=n;if(total>length)throw new IOException("Download exceeded expected size.");output.Write(b,0,n);Progress("Downloading "+System.IO.Path.GetFileName(path)+" — "+(total/1048576)+" MB",(int)(total*100/length));}
     }
    }
   }
   if(new FileInfo(temp).Length!=length || FileHash(temp)!=hash)throw new IOException("Download checksum failed: "+System.IO.Path.GetFileName(path));
   if(File.Exists(path))File.Delete(path);File.Move(temp,path);
  } finally {if(File.Exists(temp))File.Delete(temp);}
 }
 string GetBase() {
  if(!string.IsNullOrEmpty(ArchiveOverride)) {
   if(!File.Exists(ArchiveOverride)||new FileInfo(ArchiveOverride).Length!=Release.Base.Size||FileHash(ArchiveOverride)!=Release.Base.Sha256)throw new IOException("That archive is not the supported base-client version.");
   return ArchiveOverride;
  }
  string cache=SafePath(root,".launcher/cache/base-"+Release.Base.Sha256+".zip");
  Download(Address(Release.Base.Url),cache,Release.Base.Sha256,Release.Base.Size);return cache;
 }
 void ExtractEntry(ZipArchiveEntry entry,string path,ClientFile f) {
  if(entry==null || entry.Length!=f.Size)throw new IOException("Archive entry is missing or has the wrong size: "+f.Path);
  Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));using(var input=entry.Open())using(var output=File.Create(path)){input.CopyTo(output);}
  if(FileHash(path)!=f.Sha256)throw new IOException("Extracted file checksum failed: "+f.Path);
 }
 void Recover() {
  string jp=SafePath(root,".launcher/transaction.json");if(!File.Exists(jp))return;
  AssertClosed();var journal=Json.Deserialize<Journal>(File.ReadAllText(jp));
  if(journal.Phase!="committed")foreach(var c in journal.Changes.AsEnumerable().Reverse()) {
   string target=SafePath(root,c.Path),backup=SafePath(root,".launcher/backup/"+c.Path);
   if(c.Existed) {if(!File.Exists(backup))throw new IOException("Recovery backup is missing: "+c.Path);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));File.Copy(backup,target,true);}
   else if(File.Exists(target))File.Delete(target);
  }
  File.Delete(jp);Progress("Previous update recovered",0);
 }
 public void Apply() {
  SetRoot();AssertClosed();if(Release==null)LoadRelease();
  if(Directory.Exists(root) && !File.Exists(SafePath(root,".launcher/installed.json")) && !File.Exists(SafePath(root,".launcher/transaction.json"))) {
   string executable=SafePath(root,"system/L2.exe");
   if(File.Exists(executable)) {
    var expected=Release.Files.First(f=>f.Path.Equals("system/L2.exe",StringComparison.OrdinalIgnoreCase));
    if(FileHash(executable)!=expected.Sha256)throw new IOException("This is not the supported client version. Choose an empty folder for a clean installation.");
   } else if(Directory.GetFileSystemEntries(root).Any(f=>!System.IO.Path.GetFileName(f).StartsWith(".launcher",StringComparison.OrdinalIgnoreCase)))throw new IOException("Choose an empty folder or the supported client folder containing system/L2.exe.");
  }
  Directory.CreateDirectory(root);
  using(var guard=new FileStream(SafePath(root,".launcher-lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)) {
   Recover();if(Release==null)LoadRelease();var needed=Check();if(needed.Count==0)return;
   string stage=SafePath(root,".launcher/stage");Directory.CreateDirectory(stage);
   var baseFiles=needed.Where(f=>string.IsNullOrEmpty(f.Asset)).ToList();
   if(baseFiles.Count>0)using(var z=ZipFile.OpenRead(GetBase())) {
    foreach(var f in baseFiles){Progress("Preparing "+f.Path,0);ExtractEntry(z.GetEntry(f.Path),SafePath(stage,f.Path),f);}
   }
   foreach(var f in needed.Where(f=>!string.IsNullOrEmpty(f.Asset))) {
    var uri=new Uri(Address(string.IsNullOrEmpty(Release.AssetBaseUrl)?Config.Feed:Release.AssetBaseUrl),f.Asset);Address(uri.AbsoluteUri);
    string cache=SafePath(root,".launcher/cache/"+f.Asset);
    Download(uri,cache,f.AssetSha256,f.AssetSize);
    using(var z=ZipFile.OpenRead(cache)) {if(z.Entries.Count!=1 || z.Entries[0].FullName!=f.Path)throw new IOException("Unexpected patch archive content.");ExtractEntry(z.Entries[0],SafePath(stage,f.Path),f);}
   }
   foreach(var f in needed)if(f.Path.Equals("system/psetup.dll",StringComparison.OrdinalIgnoreCase)) {string path=SafePath(stage,f.Path);File.WriteAllBytes(path,ConnectionBytes(File.ReadAllBytes(path),Config.ServerAddress));}
   AssertClosed();var j=new Journal {Phase="applying"};string jp=SafePath(root,".launcher/transaction.json");
   try {
    int done=0;
    foreach(var f in needed) {
     string target=SafePath(root,f.Path),backup=SafePath(root,".launcher/backup/"+f.Path);
     if(f.Preserve && File.Exists(target))continue;
     bool existed=File.Exists(target);if(existed){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(backup));File.Copy(target,backup,true);}
     j.Changes.Add(new Change {Path=f.Path,Existed=existed});WriteJson(jp,j);
     Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));
     string source=SafePath(stage,f.Path);
     if(existed)File.Replace(source,target,null);else File.Move(source,target);
     if(!Matches(f,root))throw new IOException("Installed file verification failed: "+f.Path);
     Progress("Installing "+f.Path,(++done)*100/needed.Count);
     if(FailAfter>=0 && done>=FailAfter)throw new IOException("Injected update failure");
    }
    WriteJson(SafePath(root,".launcher/installed.json"),new {Version=Release.Version,Updated=DateTime.UtcNow.ToString("o")});
    j.Phase="committed";WriteJson(jp,j);File.Delete(jp);
   } catch {Recover();throw;}
   Progress("Ready to play — "+Release.Version,100);
  }
 }
 public void Play() {
  SetRoot();if(Release==null||Check().Count!=0)throw new IOException("Update or repair the client before playing.");
  if(!TestMode) {
   var p=new ProcessStartInfo(SafePath(root,"system/L2.exe"));p.WorkingDirectory=SafePath(root,"system");p.UseShellExecute=false;p.EnvironmentVariables["__COMPAT_LAYER"]="RunAsInvoker";Process.Start(p);
  }
 }
}
}
