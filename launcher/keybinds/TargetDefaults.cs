using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace LocalL2Keys {
 public static class TargetDefaults {
  public const string Command="ProminenceNextTarget";
  public const string LegacyCommand="NPCTargetNext range=1500";
  static bool Enabled(string path) {
   string layout=Path.Combine(Path.GetDirectoryName(path),"interface.xdat");
   return File.Exists(layout)&&ControlCatalog.ReadShortcuts(layout).Any(c=>c.Command==Command);
  }
  public static byte[] Updated(byte[] original) {
   NativeClientBindings.ValidateIni(original);var encoding=Encoding.GetEncoding(28591);
   string text=encoding.GetString(PortableCrypt.Decode(original));
   var section=Regex.Match(text,@"(?ims)^\[Engine\.Input\][^\r\n]*(?:\r?\n)(.*?)(?=^\[|\z)");
   if(!section.Success)throw new IOException("Engine.Input section is missing; targeting settings were not changed.");
   string body=section.Groups[1].Value,nl=text.Contains("\r\n")?"\r\n":"\n";
   string changed=Regex.Replace(body,@"(?im)^[ \t]*Tab[ \t]*=[^\r\n]*(?:\r?\n|$)","");
   changed="Tab="+nl+changed;
   string result=text.Substring(0,section.Groups[1].Index)+changed+text.Substring(section.Groups[1].Index+body.Length);
   if(result==text)return null;
   byte[] plain=encoding.GetBytes(result),encoded=PortableCrypt.Encode(plain);
   NativeClientBindings.ValidateIni(encoded);
   if(!PortableCrypt.Decode(encoded).SequenceEqual(plain))throw new IOException("Targeting settings verification failed.");
   return encoded;
  }
  public static bool Needs(string path){return File.Exists(path)&&Enabled(path)&&Updated(File.ReadAllBytes(path))!=null;}
  public static void Apply(string path,string backup){
   if(!File.Exists(path)||!Enabled(path))return;
   byte[] before=File.ReadAllBytes(path),after=Updated(before);if(after==null)return;
   if(System.Diagnostics.Process.GetProcessesByName("L2").Length>0)throw new IOException("Close Lineage II before updating targeting settings.");
   Directory.CreateDirectory(Path.GetDirectoryName(backup));string temp=backup+".new";File.WriteAllBytes(temp,after);
   if(!File.ReadAllBytes(path).SequenceEqual(before))throw new IOException("Client settings changed during the update. Please retry.");
   File.Replace(temp,path,backup);
  }
 }
}
