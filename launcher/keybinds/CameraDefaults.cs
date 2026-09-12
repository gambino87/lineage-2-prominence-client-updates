using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace LocalL2Keys {
public static class CameraDefaults {
 public static byte[] Updated(byte[] original){
  NativeClientBindings.ValidateIni(original);
  byte[] plain=PortableCrypt.Decode(original);var encoding=Encoding.GetEncoding(28591);
  string text=encoding.GetString(plain);
  string changed=Regex.Replace(text,@"^([ \t]*RightMouse[ \t]*=)([^\r\n]*)",match=>{
   var commands=match.Groups[2].Value.Split('|');
   var retained=commands.Where(c=>!Regex.IsMatch(c.Trim(),@"^FixedDefaultCamera\b",RegexOptions.IgnoreCase)).ToArray();
   return retained.Length==commands.Length?match.Value:match.Groups[1].Value+string.Join(" | ",retained.Select(c=>c.Trim()));
  },RegexOptions.Multiline|RegexOptions.IgnoreCase);
  if(changed==text)return null;
  byte[] expected=encoding.GetBytes(changed),result=PortableCrypt.Encode(expected);
  NativeClientBindings.ValidateIni(result);
  if(!PortableCrypt.Decode(result).SequenceEqual(expected))throw new IOException("Camera settings verification failed.");
  return result;
 }
 public static bool Needs(string path){return File.Exists(path) && Updated(File.ReadAllBytes(path))!=null;}
 public static void Apply(string path,string backup){
  if(!File.Exists(path))return;
  byte[] original=File.ReadAllBytes(path),updated=Updated(original);if(updated==null)return;
  if(System.Diagnostics.Process.GetProcessesByName("L2").Length>0)throw new IOException("Close Lineage II before updating camera settings.");
  Directory.CreateDirectory(Path.GetDirectoryName(backup));
  string temporary=backup+".new";File.WriteAllBytes(temporary,updated);
  if(!File.ReadAllBytes(path).SequenceEqual(original))throw new IOException("Client settings changed during the update. Please retry.");
  File.Replace(temporary,path,backup);
 }
}
}
