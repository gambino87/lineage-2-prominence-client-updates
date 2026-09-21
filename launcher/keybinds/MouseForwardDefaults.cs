using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Script.Serialization;
namespace LocalL2Keys {
 public static class MouseForwardDefaults {
  static bool Enabled(string path) {
   string marker=Path.Combine(Path.GetDirectoryName(path),"prominence-controls.json");
   if(!File.Exists(marker))return false;
   var flags=new JavaScriptSerializer().Deserialize<Dictionary<string,int>>(File.ReadAllText(marker));
   return flags!=null&&flags.ContainsKey("MouseForward")&&flags["MouseForward"]==1;
  }
  public static byte[] Updated(byte[] original) {
   NativeClientBindings.ValidateIni(original);var encoding=Encoding.GetEncoding(28591);
   string text=encoding.GetString(PortableCrypt.Decode(original));
   var section=Regex.Match(text,@"(?ims)^\[Engine\.Input\][^\r\n]*(?:\r?\n)(.*?)(?=^\[|\z)");
   if(!section.Success)throw new IOException("Engine.Input section is missing; mouse movement settings were not changed.");
   string body=section.Groups[1].Value;
   foreach(string button in new[]{"LeftMouse","RightMouse"}) {
    int source=button=="LeftMouse"?32:64;int matches=0;
    body=Regex.Replace(body,@"(?im)^([ \t]*"+button+@"[ \t]*=)([^\r\n]*)",match=>{
     matches++;
     var commands=match.Groups[2].Value.Split('|').Select(x=>x.Trim()).Where(x=>x.Length>0&&!Regex.IsMatch(x,@"^KeyboardMove(?:Start Dir=(?:32|64)|Finish OnRelease Dir=(?:32|64))$",RegexOptions.IgnoreCase));
     return match.Groups[1].Value+"KeyboardMoveStart Dir="+source+" | KeyboardMoveFinish OnRelease Dir="+source+" | "+string.Join(" | ",commands);
    });
    if(matches!=1)throw new IOException("Expected one "+button+" binding; mouse settings were not changed.");
   }
   int middleMatches=0;
   body=Regex.Replace(body,@"(?im)^([ \t]*MiddleMouse[ \t]*=)([^\r\n]*)",match=>{middleMatches++;return match.Groups[1].Value+"KeyboardPermanentMove";});
   if(middleMatches!=1)throw new IOException("Expected one MiddleMouse binding; mouse settings were not changed.");
   body=Regex.Replace(body,@"(?im)^([ \t]*[A-Za-z0-9]+[ \t]*=)([^\r\n]*)",match=>{
    if(match.Groups[1].Value.TrimStart().StartsWith("MiddleMouse",StringComparison.OrdinalIgnoreCase))return match.Value;
    var commands=match.Groups[2].Value.Split('|');
    if(!commands.Any(c=>c.Trim().Equals("KeyboardPermanentMove",StringComparison.OrdinalIgnoreCase)))return match.Value;
    return match.Groups[1].Value+string.Join(" | ",commands.Where(c=>!c.Trim().Equals("KeyboardPermanentMove",StringComparison.OrdinalIgnoreCase)).Select(c=>c.Trim()));
   });
   string result=text.Substring(0,section.Groups[1].Index)+body+text.Substring(section.Groups[1].Index+section.Groups[1].Length);
   if(result==text)return null;
   byte[] plain=encoding.GetBytes(result),encoded=PortableCrypt.Encode(plain);NativeClientBindings.ValidateIni(encoded);
   if(!PortableCrypt.Decode(encoded).SequenceEqual(plain))throw new IOException("Mouse movement settings verification failed.");
   return encoded;
  }
  public static bool Needs(string path){return File.Exists(path)&&Enabled(path)&&Updated(File.ReadAllBytes(path))!=null;}
  public static void Apply(string path,string backup){
   if(!File.Exists(path)||!Enabled(path))return;
   byte[] before=File.ReadAllBytes(path),after=Updated(before);if(after==null)return;
   if(System.Diagnostics.Process.GetProcessesByName("L2").Length>0)throw new IOException("Close Lineage II before updating mouse movement settings.");
   Directory.CreateDirectory(Path.GetDirectoryName(backup));string temp=backup+".new";File.WriteAllBytes(temp,after);
   if(!File.ReadAllBytes(path).SequenceEqual(before))throw new IOException("Client settings changed during the update. Please retry.");
   File.Replace(temp,path,backup);
  }
 }
}
