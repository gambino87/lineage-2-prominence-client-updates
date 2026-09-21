using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LocalL2Keys {
 public sealed class SavedKey { public string Command; public int Occurrence; public int[] Keys; }
 public sealed class SavedKeys { public int Schema=1; public List<SavedKey> Bindings=new List<SavedKey>(); }
 // Store semantic assignments, never offsets into a particular release's UI layout.
 public static class KeybindPreferences {
  static string PathFor(string system) { return Path.Combine(system,"personal-keybinds.json"); }
  static SavedKeys Read(string system) {
   string path=PathFor(system);if(!File.Exists(path))return new SavedKeys();
   if(new FileInfo(path).Length>1024*1024)throw new InvalidDataException("Personal keybind settings are too large.");
   var saved=new JavaScriptSerializer().Deserialize<SavedKeys>(File.ReadAllText(path));
   if(saved==null||saved.Schema!=1||saved.Bindings==null||saved.Bindings.Count>1000)throw new InvalidDataException("Invalid personal keybind settings. Your settings have not been overwritten.");
   foreach(var b in saved.Bindings)if(b==null||b.Command==null||b.Occurrence<0||b.Keys==null||b.Keys.Length!=3)throw new InvalidDataException("Invalid saved key binding.");
   return saved;
  }
  public static bool Exists(string system) { return File.Exists(PathFor(system)); }
  public static void Record(string system,byte[] before,byte[] after) {
   var a=new XdatDocument(before).Actions;var b=new XdatDocument(after).Actions;
   if(!a.Select(x=>x.S("action")).SequenceEqual(b.Select(x=>x.S("action"))))throw new InvalidDataException("Shortcut actions changed while saving keys.");
   var saved=Read(system);var counts=new Dictionary<string,int>();
   for(int i=0;i<a.Count;i++) {
    string cmd=a[i].S("action");int ordinal;counts.TryGetValue(cmd,out ordinal);counts[cmd]=ordinal+1;
    var keys=new[]{b[i].I("key_1"),b[i].I("key_2"),b[i].I("key_3")};
    if(keys.SequenceEqual(new[]{a[i].I("key_1"),a[i].I("key_2"),a[i].I("key_3")}))continue;
    saved.Bindings.RemoveAll(x=>x.Command==cmd&&x.Occurrence==ordinal);
    saved.Bindings.Add(new SavedKey{Command=cmd,Occurrence=ordinal,Keys=keys});
   }
   string path=PathFor(system),temp=path+".tmp";
   File.WriteAllText(temp,new JavaScriptSerializer().Serialize(saved));
   if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
  }
  static byte[] Keys(string system,byte[] canonical) {
   var doc=new XdatDocument(canonical);var edits=new List<ByteEdit>();
   foreach(var b in Read(system).Bindings) {
    string command=b.Command;
    if(command==TargetDefaults.LegacyCommand&&doc.Actions.Any(x=>x.S("action")==TargetDefaults.Command))command=TargetDefaults.Command;
    var family=doc.Actions.Where(x=>x.S("action")==command).ToList();
    if(b.Occurrence>=family.Count)throw new InvalidDataException("An updated interface no longer supports a saved binding for "+b.Command+". Your settings are preserved; update was stopped.");
    for(int j=0;j<3;j++)edits.Add(XdatDocument.Edit(family[b.Occurrence],"key_"+(j+1),b.Keys[j]));
   }
   byte[] result=XdatDocument.Patch(canonical,edits);
   var updated=new XdatDocument(result);
   bool direct=updated.Actions.Any(x=>x.S("action")==TargetDefaults.Command);
   if(direct||updated.Actions.Any(x=>x.S("action")==TargetDefaults.LegacyCommand)) {
    edits.Clear();
    foreach(var a in updated.Actions) {
     if(a.S("action")==TargetDefaults.Command||a.S("action")==TargetDefaults.LegacyCommand) {
      edits.Add(XdatDocument.Edit(a,"key_1",direct&&a.S("action")==TargetDefaults.Command?9:0));edits.Add(XdatDocument.Edit(a,"key_2",0));edits.Add(XdatDocument.Edit(a,"key_3",0));
     } else if(a.I("key_1")==9&&a.I("key_2")==0&&a.I("key_3")==0)edits.Add(XdatDocument.Edit(a,"key_1",0));
    }
    result=XdatDocument.Patch(result,edits);
   }
   return result;
  }
  public static byte[] Render(string system,string relative,byte[] canonical,byte[] layout) {
   if(!Exists(system)) {
    string backups=Path.Combine(system,"backups"),current=Path.Combine(system,"interface.xdat");
    if(Directory.Exists(backups)&&File.Exists(current)) {
     byte[] edited=File.ReadAllBytes(current);string hash;
     using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(edited)).Replace("-","").ToLowerInvariant();
     foreach(string dir in Directory.GetDirectories(backups).OrderByDescending(x=>x)) {
      string receipt=Path.Combine(dir,"changes.json"),before=Path.Combine(dir,"interface.xdat");
      if(!File.Exists(receipt)||!File.Exists(before))continue;
      var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(receipt));
      object value;if(data!=null&&data.TryGetValue("afterXdat",out value)&&(string)value==hash) {
       // Preserve all current shortcuts when upgrading an older editor's saved configuration.
       var doc=new XdatDocument(edited);var saved=new SavedKeys();var counts=new Dictionary<string,int>();
       foreach(var action in doc.Actions) {string cmd=action.S("action");int n;counts.TryGetValue(cmd,out n);counts[cmd]=n+1;saved.Bindings.Add(new SavedKey{Command=cmd,Occurrence=n,Keys=new[]{action.I("key_1"),action.I("key_2"),action.I("key_3")}});}
       File.WriteAllText(PathFor(system),new JavaScriptSerializer().Serialize(saved));break;
      }
     }
    }
    if(!Exists(system))return canonical;
   }
   if(relative.Equals("system/interface.xdat",StringComparison.OrdinalIgnoreCase))
    return ClientUiLabels.Layout(Keys(system,canonical),File.ReadAllText(Path.Combine(system,"Option.ini")));
   if(relative.Equals("system/sysstring-e.dat",StringComparison.OrdinalIgnoreCase)) {
    var xdat=layout ?? Keys(system,File.ReadAllBytes(Path.Combine(system,"interface.xdat")));
    return PortableCrypt.Encode(ClientUiLabels.Tooltips(PortableCrypt.Decode(canonical),xdat));
   }
   return canonical;
  }
 }
}
