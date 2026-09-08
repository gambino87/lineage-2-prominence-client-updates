using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using InterludeLauncher;

class WindowState {
 static int checks;
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
 static T Field<T>(Window window,string name){return (T)typeof(Window).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(window);}
 static ClientFile Entry(string path,byte[] bytes,bool preserve=false){return new ClientFile{Path=path,Sha256=Patcher.Hash(bytes),Size=bytes.Length,Preserve=preserve};}
 static void Zip(string path,Dictionary<string,byte[]> files){using(var archive=ZipFile.Open(path,ZipArchiveMode.Create))foreach(var file in files)using(var stream=archive.CreateEntry(file.Key).Open())stream.Write(file.Value,0,file.Value.Length);}
 static void Sign(string path,Manifest manifest,RSACryptoServiceProvider key){Patcher.WriteJson(path,manifest);File.WriteAllText(path+".sig",Convert.ToBase64String(key.SignData(File.ReadAllBytes(path),CryptoConfig.MapNameToOID("SHA256"))));}
 static void Wait(Func<bool> done){var timer=Stopwatch.StartNew();while(!done()){if(timer.ElapsedMilliseconds>30000)throw new Exception("Window operation timed out");Application.DoEvents();Thread.Sleep(5);}Application.DoEvents();}
 static void Checked(Window window){Wait(()=>!Field<bool>(window,"busy") && Field<int?>(window,"filesNeeded").HasValue);}
 static void Run(Window window,bool apply,bool fullVerification=false){var task=(Task)typeof(Window).GetMethod("Run",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(window,new object[]{apply,false,fullVerification});Check(!Field<Button>(window,"update").Enabled,"Primary button must remain disabled while busy");Wait(()=>task.IsCompleted);task.GetAwaiter().GetResult();}
 static void Snapshot(Window window,string path){using(var bitmap=new Bitmap(window.Width,window.Height)){window.DrawToBitmap(bitmap,new Rectangle(0,0,window.Width,window.Height));bitmap.Save(path);}}
 [STAThread] static void Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  string root=Path.GetFullPath(args[0]),client=Path.Combine(root,"client"),feed=Path.Combine(root,"feed");Directory.CreateDirectory(feed);
  byte[] executable=Encoding.ASCII.GetBytes("fixture executable"),dll=File.ReadAllBytes(args[1]),ini=Encoding.ASCII.GetBytes("personal settings fixture"),first=Encoding.ASCII.GetBytes("interface v1"),second=Encoding.ASCII.GetBytes("interface v2");
  string archive=Path.Combine(feed,"base.zip");Zip(archive,new Dictionary<string,byte[]>{{"system/L2.exe",executable},{"system/psetup.dll",dll},{"system/user.ini",ini}});
  var patch=Entry("system/interface.u",first);patch.Asset="interface-v1.zip";Zip(Path.Combine(feed,patch.Asset),new Dictionary<string,byte[]>{{patch.Path,first}});patch.AssetSha256=Patcher.FileHash(Path.Combine(feed,patch.Asset));patch.AssetSize=new FileInfo(Path.Combine(feed,patch.Asset)).Length;
  var manifest=new Manifest{Schema=1,Version="fixture-1",Notes="Isolated launcher button test.",Base=new BaseArchive{Url=new Uri(archive).AbsoluteUri,Size=new FileInfo(archive).Length,Sha256=Patcher.FileHash(archive)},Files=new List<ClientFile>{Entry("system/L2.exe",executable),Entry("system/psetup.dll",dll),Entry("system/user.ini",ini,true),patch}};
  using(var key=new RSACryptoServiceProvider(2048)){key.PersistKeyInCsp=false;string mf=Path.Combine(feed,"manifest.json");Sign(mf,manifest,key);
   var settings=new Settings{Title="Lineage 2 Prominence",Feed=new Uri(mf).AbsoluteUri,PublicKey=key.ToXmlString(false),AllowLocalFeed=true,ClientDirectory=client,ServerAddress="127.0.0.1"};
   using(var window=new Window(settings,Path.Combine(root,"launcher.json"))){
    Field<Patcher>(window,"patcher").TestMode=true;window.ShowInTaskbar=false;window.StartPosition=FormStartPosition.Manual;window.Location=new Point(-30000,-30000);
    var button=Field<Button>(window,"update");var play=Field<Button>(window,"play");var folder=Field<TextBox>(window,"folder");
    Check(button.Text=="Install" && !button.Enabled,"Empty folder starts with a disabled Install button until checked");
    window.Show();Checked(window);
    Check(button.Text=="Install" && button.Enabled,"Initial check enables Install for an empty folder");Snapshot(window,Path.Combine(root,"install.png"));
    Run(window,true);Check(button.Text=="Up to date" && !button.Enabled && play.Enabled,"Successful installation disables the primary button and enables Play");Snapshot(window,Path.Combine(root,"current.png"));
    patch.Sha256=Patcher.Hash(second);patch.Size=second.Length;patch.Asset="interface-v2.zip";Zip(Path.Combine(feed,patch.Asset),new Dictionary<string,byte[]>{{patch.Path,second}});patch.AssetSha256=Patcher.FileHash(Path.Combine(feed,patch.Asset));patch.AssetSize=new FileInfo(Path.Combine(feed,patch.Asset)).Length;manifest.Version="fixture-2";Sign(mf,manifest,key);
    Run(window,false);Check(button.Text=="Update" && button.Enabled && !play.Enabled,"New release enables Update and disables Play");Snapshot(window,Path.Combine(root,"update.png"));
    Run(window,true);Check(button.Text=="Up to date" && !button.Enabled && play.Enabled,"Successful update disables primary button again");
    string interfacePath=Path.Combine(client,"system/interface.u");DateTime modified=File.GetLastWriteTimeUtc(interfacePath);File.WriteAllBytes(interfacePath,new byte[second.Length]);File.SetLastWriteTimeUtc(interfacePath,modified);Run(window,true,true);Check(Patcher.FileHash(interfacePath)==Patcher.Hash(second),"Repair in the window performs a full scan even when metadata has not changed");
    string exe=Path.Combine(client,"system/L2.exe");File.Delete(exe);Run(window,false);Check(button.Text=="Update" && button.Enabled,"An installation with a missing executable remains an Update, not Install");Run(window,true);
    folder.Text=Path.Combine(root,"other-client");Check(button.Text=="Install" && !button.Enabled && !play.Enabled,"Changing folders invalidates the previous check immediately");Checked(window);Check(button.Text=="Install" && button.Enabled,"Folder changes automatically recheck the new folder");
    folder.Text=client;Check(!button.Enabled && !play.Enabled,"Returning to an existing client requires a fresh check");Checked(window);Check(button.Text=="Up to date" && !button.Enabled && play.Enabled,"Returning to a current client leaves Update disabled");
    Check(!File.Exists(Path.Combine(root,"other-client","system/L2.exe")),"Checking a new folder never installs client files");
    window.Close();
   }
  }
  Console.WriteLine(checks+" launcher UI checks passed. Screenshots: "+root);
 }
}
