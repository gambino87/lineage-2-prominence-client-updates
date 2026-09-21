using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using InterludeLauncher;
using LocalL2Keys;
public class KeybindUpdateTests {
 static int checks;static void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
 static ClientFile Patch(string feed,string name,byte[] data,string tag){string asset=tag+".zip",path=Path.Combine(feed,asset);using(var z=ZipFile.Open(path,ZipArchiveMode.Create)){using(var o=z.CreateEntry(name).Open())o.Write(data,0,data.Length);}return new ClientFile{Path=name,Sha256=Patcher.Hash(data),Size=data.Length,Asset=asset,AssetSha256=Patcher.FileHash(path),AssetSize=new FileInfo(path).Length};}
 public static void Main(string[] args){
  string root=Path.GetFullPath(args[0]),fixture=Path.Combine(root,"state/keybind-update-tests/"+Guid.NewGuid().ToString("N")),system=Path.Combine(fixture,"system"),feed=Path.Combine(fixture,"feed");Directory.CreateDirectory(system);Directory.CreateDirectory(feed);Directory.CreateDirectory(Path.Combine(fixture,".launcher/cache"));File.WriteAllText(Path.Combine(fixture,".launcher/installed.json"),"{}");
  string source=Path.Combine(root,"state/launcher-staging/0.2.172/system");foreach(string f in new[]{"interface.xdat","sysstring-e.dat"})File.Copy(Path.Combine(source,f),Path.Combine(system,f));File.Copy(Path.Combine(root,"outputs/test-bench/client/system/user.ini"),Path.Combine(system,"user.ini"));File.Copy(Path.Combine(root,"outputs/test-bench/client/system/Option.ini"),Path.Combine(system,"Option.ini"));
  byte[] ui=File.ReadAllBytes(Path.Combine(system,"interface.xdat")),strings=File.ReadAllBytes(Path.Combine(system,"sysstring-e.dat"));
  var a=Patch(feed,"system/interface.xdat",ui,"ui1");var b=Patch(feed,"system/sysstring-e.dat",strings,"strings1");foreach(var f in new[]{a,b})File.Copy(Path.Combine(feed,f.Asset),Path.Combine(fixture,".launcher/cache",f.Asset));
  var p=new Patcher(new Settings{ClientDirectory=fixture,AllowLocalFeed=true,ServerAddress="127.0.0.1"}){TestMode=true,Release=new Manifest{Schema=1,Version="test1",AssetBaseUrl=new Uri(feed+Path.DirectorySeparatorChar).AbsoluteUri,Files=new List<ClientFile>{a,b}}};p.Personalize=(rel,data,layout)=>KeybindPreferences.Render(system,rel,data,layout);
  Check(p.Check().Count==0,"Unmodified release ready");
  var editor=new NativeClientBindings(root,system);editor.Apply(new[]{new NativeAssignment{Control=editor.Controls.First(c=>c.Command=="ShowInventoryWindow"),Key="Ctrl+J"}});
  Check(KeybindPreferences.Exists(system),"Editor persists preferences");Check(p.Check().Count==0,"Personal keys do not trigger update");Check(p.Check(true).Count==0,"Full verification recognizes personal keys");
  byte[] personal=File.ReadAllBytes(Path.Combine(system,"interface.xdat"));p.Apply();Check(personal.SequenceEqual(File.ReadAllBytes(Path.Combine(system,"interface.xdat"))),"No-op update preserves keys");
  var doc=new XdatDocument(ui);var node=doc.Nodes.First(n=>n.Kind=="TextBox"&&n.Fields.ContainsKey("size_absolute_width"));byte[] next=XdatDocument.Patch(ui,new[]{XdatDocument.Edit(node,"size_absolute_width",node.I("size_absolute_width")==254?253:254)});
  a=Patch(feed,"system/interface.xdat",next,"ui2");p.Release.Files[0]=a;p.Release.Version="test2";Check(p.Check().Count>0,"Real UI update detected");p.FailAfter=1;try{p.Apply();throw new Exception("Expected failure");}catch(IOException){}Check(personal.SequenceEqual(File.ReadAllBytes(Path.Combine(system,"interface.xdat"))),"Failed update rolls back personalized UI");p.FailAfter=-1;p.Apply();
  Check(p.Check().Count==0,"Updated personalized UI verifies");editor.Reload();Check(editor.Controls.First(c=>c.Command=="ShowInventoryWindow").Key=="Ctrl+J","New release retains binding");
  var updated=new XdatDocument(File.ReadAllBytes(Path.Combine(system,"interface.xdat")));Check(updated.Nodes.First(n=>n.Kind=="TextBox"&&n.Fields.ContainsKey("size_absolute_width")).I("size_absolute_width")!=node.I("size_absolute_width"),"New UI property installed");
  File.WriteAllText(Path.Combine(system,"sysstring-e.dat"),"corrupt");Check(p.Check(true).Count==1,"Unrelated corruption detected");p.Apply(true);Check(p.Check(true).Count==0,"Repair restores personalized labels");editor.Reload();Check(editor.Controls.First(c=>c.Command=="ShowInventoryWindow").Key=="Ctrl+J","Repair keeps keys");
  File.Delete(Path.Combine(system,"personal-keybinds.json")); // A prior editor's hash receipt can migrate its intact output.
  File.WriteAllBytes(Path.Combine(system,"interface.xdat"),personal);p.Release.Files[0]=Patch(feed,"system/interface.xdat",ui,"ui3");File.Copy(Path.Combine(feed,"ui3.zip"),Path.Combine(fixture,".launcher/cache/ui3.zip"));
  p.Check();Check(KeybindPreferences.Exists(system),"Legacy editor settings migrated");p.Apply();Check(p.Check(true).Count==0,"Migrated bindings verify");
  foreach(string zip in Directory.GetFiles(Path.Combine(fixture,".launcher/cache"),"*.zip"))File.Delete(zip);
  File.WriteAllText(Path.Combine(system,"interface.xdat"),"corrupt");
  p.Apply(true);Check(p.Check(true).Count==0,"Missing cache and corrupt UI recover with saved bindings");
  editor.Reload();Check(editor.Controls.First(c=>c.Command=="ShowInventoryWindow").Key=="Ctrl+J","Cache rebuild preserves keys");
  Console.WriteLine(checks+" keybind update checks passed.");
 }
}


