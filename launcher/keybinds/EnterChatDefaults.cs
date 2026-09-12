using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace LocalL2Keys {
public static class EnterChatDefaults {
 public static byte[] Updated(byte[] original){
  Encoding encoding=Encoding.GetEncoding(28591);int prefix=0;
  if(original.Length>=2 && original[0]==255 && original[1]==254){encoding=Encoding.Unicode;prefix=2;}
  else if(original.Length>=2 && original[0]==254 && original[1]==255){encoding=Encoding.BigEndianUnicode;prefix=2;}
  string text=encoding.GetString(original,prefix,original.Length-prefix),newline=text.Contains("\r\n")?"\r\n":"\n";
  var section=Regex.Match(text,@"(?im)^\[Game\][ \t]*\r?$[\s\S]*?(?=^\[|\z)");
  string changed;
  if(section.Success){
   string part=section.Value;
   var setting=new Regex(@"(?im)^([ \t]*EnterChatting[ \t]*=[ \t]*)[^\r\n]*");
   string replacement=setting.IsMatch(part)?setting.Replace(part,"${1}True"):part+(part.EndsWith("\n")?"":newline)+"EnterChatting=True"+newline;
   changed=text.Substring(0,section.Index)+replacement+text.Substring(section.Index+section.Length);
  }else changed=text+(text.Length==0||text.EndsWith("\n")?"":newline)+"[Game]"+newline+"EnterChatting=True"+newline;
  if(changed==text)return null;
  return original.Take(prefix).Concat(encoding.GetBytes(changed)).ToArray();
 }
 public static bool Needs(string path){return File.Exists(path) && Updated(File.ReadAllBytes(path))!=null;}
 public static void Apply(string path,string backup){
  if(!File.Exists(path))return;
  byte[] original=File.ReadAllBytes(path),updated=Updated(original);if(updated==null)return;
  if(System.Diagnostics.Process.GetProcessesByName("L2").Length>0)throw new IOException("Close Lineage II before updating Enter Chat settings.");
  Directory.CreateDirectory(Path.GetDirectoryName(backup));
  string temporary=backup+".new";File.WriteAllBytes(temporary,updated);
  if(!File.ReadAllBytes(path).SequenceEqual(original))throw new IOException("Client settings changed during the update. Please retry.");
  File.Replace(temporary,path,backup);
 }
}
}
