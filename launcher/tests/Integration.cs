using System;using System.IO;using System.IO.Compression;using System.Collections.Generic;using System.Linq;using System.Security.Cryptography;using System.Text;
using InterludeLauncher;
class Integration {
 static int checks;
 static void Check(bool x,string msg){if(!x)throw new Exception(msg);checks++;}
 static void Fails(Action a,string msg){try{a();}catch{checks++;return;}throw new Exception("Expected rejection: "+msg);}
 static ClientFile Entry(string path,byte[] b,bool preserve=false){return new ClientFile{Path=path,Size=b.Length,Sha256=Patcher.Hash(b),Preserve=preserve};}
 static void Zip(string path,Dictionary<string,byte[]> entries){using(var z=ZipFile.Open(path,ZipArchiveMode.Create))foreach(var e in entries)using(var s=z.CreateEntry(e.Key).Open())s.Write(e.Value,0,e.Value.Length);}
 static void Sign(string path,RSACryptoServiceProvider rsa){File.WriteAllText(path+".sig",Convert.ToBase64String(rsa.SignData(File.ReadAllBytes(path),CryptoConfig.MapNameToOID("SHA256"))));}
 static void Main(string[]args) {
  string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);string feed=Path.Combine(root,"feed");Directory.CreateDirectory(feed);
  byte[] dll=File.ReadAllBytes(args[1]),exe=Encoding.ASCII.GetBytes("fixture executable"),ini=Encoding.ASCII.GetBytes("default bindings"),v1=Encoding.ASCII.GetBytes("interface-v1"),v2=Encoding.ASCII.GetBytes("interface-v2");
  string bzip=Path.Combine(feed,"base.zip");Zip(bzip,new Dictionary<string,byte[]>{{"system/L2.exe",exe},{"system/psetup.dll",dll},{"system/user.ini",ini}});
  var patch=Entry("system/interface.u",v1);patch.Asset="interface.zip";Zip(Path.Combine(feed,patch.Asset),new Dictionary<string,byte[]>{{patch.Path,v1}});patch.AssetSha256=Patcher.FileHash(Path.Combine(feed,patch.Asset));patch.AssetSize=new FileInfo(Path.Combine(feed,patch.Asset)).Length;
  var manifest=new Manifest{Schema=1,Version="test1",Notes="test",Base=new BaseArchive{Url=new Uri(bzip).AbsoluteUri,Size=new FileInfo(bzip).Length,Sha256=Patcher.FileHash(bzip)},Files=new List<ClientFile>{Entry("system/L2.exe",exe),Entry("system/psetup.dll",dll),Entry("system/user.ini",ini,true),patch}};
  string mf=Path.Combine(feed,"manifest.json");
  using(var rsa=new RSACryptoServiceProvider(2048)){rsa.PersistKeyInCsp=false;Patcher.WriteJson(mf,manifest);Sign(mf,rsa);
   var settings=new Settings{Feed=new Uri(mf).AbsoluteUri,PublicKey=rsa.ToXmlString(false),AllowLocalFeed=true,ClientDirectory=Path.Combine(root,"client"),ServerAddress="100.80.20.10"};
   var p=new Patcher(settings){TestMode=true};p.LoadRelease();
   string intended=settings.ClientDirectory;settings.ClientDirectory=Path.Combine(root,"unrelated");Directory.CreateDirectory(settings.ClientDirectory);File.WriteAllText(Path.Combine(settings.ClientDirectory,"notes.txt"),"Keep this");Fails(()=>p.Apply(),"Unrelated populated directory rejected");settings.ClientDirectory=intended;
   p.Apply();Check(p.Check().Count==0,"Clean install");p.Play();Check(true,"Play gate permits verified installation");
   Check(Encoding.ASCII.GetString(File.ReadAllBytes(Path.Combine(settings.ClientDirectory,"system/psetup.dll")),0x12f7c,16).TrimEnd('\0')==settings.ServerAddress,"Connection address");
   string user=Path.Combine(settings.ClientDirectory,"system/user.ini");File.WriteAllText(user,"personal bindings");p.Apply();Check(File.ReadAllText(user)=="personal bindings","Keep personal INI");
   string live=Path.Combine(settings.ClientDirectory,"system/interface.u");File.WriteAllText(live,"broken");Check(p.Check().Count==1,"Find corrupted managed file");Fails(()=>p.Play(),"Play rejects corrupt installation");p.Apply();Check(File.ReadAllBytes(live).SequenceEqual(v1),"Repair corruption");
   File.WriteAllText(Path.Combine(settings.ClientDirectory,"system/L2.exe"),"broken");p.Apply();Check(p.Check().Count==0,"Repair base-only file from verified archive");
   patch.Sha256=Patcher.Hash(v2);patch.Size=v2.Length;patch.Asset="interface2.zip";Zip(Path.Combine(feed,patch.Asset),new Dictionary<string,byte[]>{{patch.Path,v2}});patch.AssetSha256=Patcher.FileHash(Path.Combine(feed,patch.Asset));patch.AssetSize=new FileInfo(Path.Combine(feed,patch.Asset)).Length;
   manifest.Version="test2";Patcher.WriteJson(mf,manifest);Sign(mf,rsa);p.LoadRelease();p.FailAfter=1;Fails(()=>p.Apply(),"Failure injection");Check(File.ReadAllBytes(live).SequenceEqual(v1),"Rollback restores previous file");p.FailAfter=-1;p.Apply();Check(File.ReadAllBytes(live).SequenceEqual(v2),"Retry after rollback");
   string jp=Path.Combine(settings.ClientDirectory,".launcher/transaction.json"),bp=Path.Combine(settings.ClientDirectory,".launcher/backup/system/interface.u");File.WriteAllBytes(bp,v2);File.WriteAllText(live,"partial write");Patcher.WriteJson(jp,new Journal{Phase="applying",Changes=new List<Change>{new Change{Path="system/interface.u",Existed=true}}});p.Apply();Check(p.Check().Count==0,"Crash journal recovery");
   File.AppendAllText(mf," ");Fails(()=>p.LoadRelease(),"Tampered signature");Patcher.WriteJson(mf,manifest);Sign(mf,rsa);p.LoadRelease();
   File.WriteAllText(live,"broken");File.WriteAllText(Path.Combine(feed,patch.Asset),"bad ZIP");string cached=Path.Combine(settings.ClientDirectory,".launcher/cache/"+patch.Asset);File.Delete(cached);Fails(()=>p.Apply(),"Corrupted download");Check(File.ReadAllText(live)=="broken","Failed download never mutates installation");
   settings.AllowLocalFeed=false;Fails(()=>p.LoadRelease(),"Local feed disabled by default");settings.AllowLocalFeed=true;
   settings.Feed="http://example.com/manifest.json";Fails(()=>p.LoadRelease(),"HTTP rejected");
  }
  foreach(string path in new[]{"../escape","/absolute","system/../escape","system/x:stream","system\\x","system/CON",". /bad","system/a."})Fails(()=>Patcher.SafePath(root,path),"Path "+path);
  Fails(()=>Patcher.ValidateServer("example.com"),"Only supported IPv4 format");Fails(()=>Patcher.ConnectionBytes(new byte[90624],"127.0.0.1"),"Unknown connection DLL");
  Console.WriteLine(checks+" launcher integration checks passed.");
 }
}
