using System;
using System.IO;
using System.Linq;
using System.Text;
using LocalL2Keys;
class EnterChatDefaultsTests {
 static int checks;
 static void Check(bool ok){checks++;if(!ok)throw new Exception("Check "+checks+" failed");}
 static void Main(string[] args){
  foreach(var e in new[]{Encoding.GetEncoding(28591),Encoding.Unicode,Encoding.BigEndianUnicode,new UTF8Encoding(true)})foreach(var nl in new[]{"\r\n","\n"}){
   string source="[Video]"+nl+"Width=1920"+nl+"[Game]"+nl+"EnterChatting=False"+nl+"OldChatting=True"+nl+"[Audio]"+nl+"Volume=0.2"+nl;
   byte[] bytes=e.GetPreamble().Concat(e.GetBytes(source)).ToArray();
   byte[] result=EnterChatDefaults.Updated(bytes);
   Check(result.SequenceEqual(e.GetPreamble().Concat(e.GetBytes(source.Replace("EnterChatting=False","EnterChatting=True")))));
   Check(EnterChatDefaults.Updated(result)==null);
  }
  foreach(string source in new[]{"[Game]\nOther=True\n[Video]\nWidth=1\n","[Video]\nWidth=1","[Game]\n", ""}){
   byte[] result=EnterChatDefaults.Updated(Encoding.ASCII.GetBytes(source));Check(Encoding.ASCII.GetString(result).Contains("EnterChatting=True"));Check(EnterChatDefaults.Updated(result)==null);
  }
  string dir=Path.Combine(args[0],Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  string path=Path.Combine(dir,"Option.ini"),backup=Path.Combine(dir,"backup.ini");File.WriteAllText(path,"[Game]\r\nEnterChatting=False\r\n");
  byte[] original=File.ReadAllBytes(path);Check(EnterChatDefaults.Needs(path));Check(File.ReadAllBytes(path).SequenceEqual(original));
  if(System.Diagnostics.Process.GetProcessesByName("L2").Length==0){EnterChatDefaults.Apply(path,backup);Check(File.ReadAllBytes(backup).SequenceEqual(original));Check(!EnterChatDefaults.Needs(path));}
  else {try{EnterChatDefaults.Apply(path,backup);throw new Exception("Running-game guard failed");}catch(IOException){Check(File.ReadAllBytes(path).SequenceEqual(original));}}
  Console.WriteLine(checks+" Enter Chat default checks passed.");
 }
}
