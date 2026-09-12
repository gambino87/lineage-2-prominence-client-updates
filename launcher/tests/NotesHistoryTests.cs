using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using InterludeLauncher;
class NotesHistoryTests {
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool PrintWindow(IntPtr h,IntPtr dc,uint f);
 static T Get<T>(object w,string n){return (T)w.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w);}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 [STAThread] static void Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  var m=Patcher.Json.Deserialize<Manifest>(File.ReadAllText(args[0]));
  var settings=new Settings{Title="Prominence - "+LauncherUpdate.Channel,ClientDirectory="C:\\Games\\Test Bench",ServerAddress="127.0.0.1"};
  using(var window=new Window(settings,args[1]+".json",true)){
   window.SetReleaseNotes(m);var combo=Get<ComboBox>(window,"notesVersions");var notes=Get<ReleaseNotesBox>(window,"notes");
   Check(combo.Items.Count>1,"Historical notes missing");Check(combo.SelectedIndex==0,"Newest not selected");
   foreach(ReleaseNote row in combo.Items)Check(row.Channel==LauncherUpdate.Channel,"Channel mixed");
   var patcher=Get<Patcher>(window,"patcher");patcher.Release=m;
   combo.SelectedIndex=1;Check(notes.Text.Contains(((ReleaseNote)combo.SelectedItem).Notes.Split('\n')[0].TrimStart('#',' ').Trim()),"Old notes not displayed");
   Check(patcher.Release==m,"Browsing changed update target");window.SetReleaseNotes(m);Check(combo.SelectedIndex==1,"Recheck lost browsing selection");
   var hostile=new Manifest{Version="0.2.999",Notes="Newest",NotesHistory=new System.Collections.Generic.List<ReleaseNote>{new ReleaseNote{Version="alpha-0.0.8",Channel=LauncherUpdate.Channel=="live"?"test-bench":"live",Notes="Must not show"}}};
   window.SetReleaseNotes(hostile);Check(combo.Items.Count==1 && combo.SelectedIndex==0,"Cross-channel history accepted");
   window.SetReleaseNotes(m);window.ShowInTaskbar=false;window.StartPosition=FormStartPosition.Manual;window.Location=new Point(-30000,-30000);window.Show();Application.DoEvents();
   using(var bitmap=new Bitmap(window.Width,window.Height))using(var g=Graphics.FromImage(bitmap)){var dc=g.GetHdc();try{PrintWindow(window.Handle,dc,0);}finally{g.ReleaseHdc(dc);}bitmap.Save(args[1]);}
   Console.WriteLine("History selection, channel isolation, newest default, browsing preservation, and unchanged update target passed; "+combo.Items.Count+" releases.");
  }
 }
}

