using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

namespace InterludeLauncher {
class PrimaryActionButton : Button {
 protected override void OnPaint(PaintEventArgs e) {
  if(Enabled){base.OnPaint(e);return;}
  e.Graphics.Clear(BackColor);
  using(var border=new Pen(FlatAppearance.BorderColor))e.Graphics.DrawRectangle(border,0,0,Width-1,Height-1);
  TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Color.FromArgb(125,135,149),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
 }
}
class ReleaseNotesBox : RichTextBox {
 public ReleaseNotesBox(){ReadOnly=true;DetectUrls=false;WordWrap=true;ScrollBars=RichTextBoxScrollBars.Vertical;BorderStyle=BorderStyle.FixedSingle;AccessibleName="Release notes";}
 public void ShowNotes(string value){
  Clear();string[] lines=(value??"").Replace("\r\n","\n").Replace('\r','\n').Split('\n');
  using(var regular=new Font(Font,FontStyle.Regular))using(var bold=new Font(Font,FontStyle.Bold)){
   for(int i=0;i<lines.Length;i++){
    string line=lines[i].TrimEnd();bool bullet=line.StartsWith("- ")||line.StartsWith("* ");
    int next=i+1;while(next<lines.Length && string.IsNullOrWhiteSpace(lines[next]))next++;
    bool heading=line.StartsWith("#") || (!bullet && line.Length>0 && (i==0 || (next<lines.Length && (lines[next].StartsWith("- ")||lines[next].StartsWith("* ")))));
    SelectionStart=TextLength;SelectionLength=0;SelectionFont=heading?bold:regular;
    SelectionIndent=bullet?16:0;SelectionHangingIndent=bullet?12:0;
    if(line.StartsWith("#"))line=line.TrimStart('#',' ');
    AppendText((bullet?"• "+line.Substring(2):line)+(i<lines.Length-1?"\n":""));
   }
  }
  Select(0,0);ScrollToCaret();
 }
}
class Window : Form {
 readonly Settings settings;
 readonly string settingsPath;
 readonly Patcher patcher;
 readonly TextBox folder=new TextBox(),host=new TextBox();
 readonly ReleaseNotesBox notes=new ReleaseNotesBox();
 readonly ComboBox notesVersions=new ComboBox();
 string notesReleaseVersion;
 readonly Label status=new Label(),server=new Label(),version=new Label();
 readonly ProgressBar progress=new ProgressBar();
 readonly Button update=new PrimaryActionButton(),repair=new Button(),play=new Button(),browse=new Button(),check=new Button(),launcherUpdate=new Button();
 readonly Timer refreshTimer=new Timer {Interval=600};
 readonly bool automaticChecks;
 int? filesNeeded;
 string operationText="Checking…";
 bool busy;
 public Window(Settings config,string path,bool preview=false) {
  settings=config;settingsPath=path;patcher=new Patcher(settings);automaticChecks=!preview;
  Text=config.Title;ClientSize=new Size(780,680);MinimumSize=new Size(796,719);MaximumSize=MinimumSize;
  StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(23,27,34);ForeColor=Color.FromArgb(229,234,240);Font=new Font("Segoe UI",10);
  var title=new Label {Text=config.Title,Location=new Point(28,22),Size=new Size(700,35),Font=new Font("Segoe UI Semibold",21)};Controls.Add(title);
  version.Text="Private playtest • Interlude";version.SetBounds(30,66,560,24);Controls.Add(version);
  var keybinds=new Button();MakeButton(keybinds,"Keybinds",610,62,140);
  keybinds.Click+=delegate {if(busy)return;try {string tool=BundledTools.Ensure(AppDomain.CurrentDomain.BaseDirectory);System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tool,"--client-dir \""+Path.GetFullPath(folder.Text.Trim()).TrimEnd('\\')+"\""){UseShellExecute=true});}catch(Exception e){MessageBox.Show(this,e.Message,Text);}};
  AddLabel("CLIENT FOLDER",30,109);folder.SetBounds(30,134,600,28);folder.Text=string.IsNullOrWhiteSpace(config.ClientDirectory)?AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar):config.ClientDirectory;Style(folder);Controls.Add(folder);
  MakeButton(browse,"Browse…",642,132,108);browse.Click+=delegate {using(var d=new FolderBrowserDialog()){d.Description="Select a compatible client, or an empty folder for a new installation.";d.SelectedPath=folder.Text;if(d.ShowDialog()==DialogResult.OK){folder.Text=d.SelectedPath;play.Enabled=false;}}};
  AddLabel("SERVER ADDRESS",30,178);host.SetBounds(30,203,210,28);host.Text=config.ServerAddress;Style(host);Controls.Add(host);
  server.SetBounds(255,205,490,24);server.Text="Server status unchecked";Controls.Add(server);
  MakeButton(check,"Check updates",30,244,200);check.Click+=async delegate {await Run(false,false);};
  var clientLink=new LinkLabel {Text="Download base client (browser)",LinkColor=Color.FromArgb(133,193,239),ActiveLinkColor=Color.White,VisitedLinkColor=Color.FromArgb(133,193,239),AutoSize=true,Location=new Point(254,253),AccessibleName="Download base client in your browser"};
  clientLink.LinkClicked+=delegate {try {System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://interlude.l2mobius.net/downloads/Lineage%20II%20Mobius%20Interlude.zip") {UseShellExecute=true});}catch(Exception e){MessageBox.Show(this,"Could not open your browser: "+e.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Information);}};
  Controls.Add(clientLink);
  Controls.Add(new Label{Text="PATCH NOTES",Location=new Point(30,291),Size=new Size(120,20),ForeColor=Color.FromArgb(151,167,185),Font=new Font("Segoe UI",9,FontStyle.Bold)});notesVersions.SetBounds(165,287,585,28);notesVersions.DropDownStyle=ComboBoxStyle.DropDownList;notesVersions.DrawMode=DrawMode.OwnerDrawFixed;notesVersions.ItemHeight=22;notesVersions.FlatStyle=FlatStyle.Flat;
  notesVersions.DrawItem+=delegate(object sender,DrawItemEventArgs e){if(e.Index<0)return;using(var fill=new SolidBrush((e.State&DrawItemState.Selected)!=0?Color.FromArgb(55,75,98):Color.FromArgb(35,41,51)))e.Graphics.FillRectangle(fill,e.Bounds);TextRenderer.DrawText(e.Graphics,notesVersions.Items[e.Index].ToString(),Font,e.Bounds,ForeColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter);e.DrawFocusRectangle();};notesVersions.BackColor=Color.FromArgb(35,41,51);notesVersions.ForeColor=ForeColor;notesVersions.AccessibleName="Patch notes version";notesVersions.Enabled=false;Controls.Add(notesVersions);
  notesVersions.SelectedIndexChanged+=delegate {var selected=notesVersions.SelectedItem as ReleaseNote;if(selected!=null)notes.ShowNotes((string.IsNullOrEmpty(selected.SourceBenchVersion)?"":"Promoted from test bench "+selected.SourceBenchVersion+"\n\n")+selected.Notes);};
  notes.SetBounds(30,323,720,216);notes.BackColor=Color.FromArgb(35,41,51);notes.ForeColor=ForeColor;notes.ShowNotes("Install the supported base client, apply project updates, and play. Existing INI settings are preserved.\r\n\r\nChoose an empty folder to install, or select your existing client folder.");Controls.Add(notes);
  status.SetBounds(30,553,720,25);status.Text="Ready to check updates";Controls.Add(status);
  progress.SetBounds(30,586,720,9);Controls.Add(progress);
  MakeButton(update,"Install",30,618,200);update.Click+=async delegate {await Run(true,false);};
  MakeButton(repair,"Repair",244,618,150);repair.Click+=async delegate {await Run(true,false,true);};
  MakeButton(play,"Play",550,616,200);play.BackColor=Color.FromArgb(56,130,108);play.Enabled=false;play.Click+=async delegate {await Run(false,true);};
  folder.TextChanged+=delegate{InvalidateCheck();};host.TextChanged+=delegate{InvalidateCheck();};
  refreshTimer.Tick+=async delegate {
   if(busy)return;refreshTimer.Stop();if(string.IsNullOrWhiteSpace(folder.Text))return;
   try {Patcher.ValidateServer(host.Text.Trim());Patcher.SafePath(Path.GetFullPath(folder.Text.Trim()),".launcher/probe");}
   catch(Exception e){status.Text=e.Message;return;}
   await Run(false,false);
  };
  Disposed+=delegate{refreshTimer.Dispose();};
  patcher.Progress=(text,value)=>{if(!IsDisposed && IsHandleCreated)BeginInvoke((Action)(()=>{status.Text=text;progress.Value=Math.Max(0,Math.Min(100,value));}));};
  FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;MessageBox.Show(this,"Please wait for the current operation to finish. Updates are recovered automatically if interrupted.",Text);}};
  MakeButton(launcherUpdate,"Update launcher",405,618,135);launcherUpdate.Click+=async delegate {
   if(busy)return;busy=true;launcherUpdate.Enabled=false;
   try {string staged=await Task.Run(()=>LauncherUpdate.Prepare(settings,AppDomain.CurrentDomain.BaseDirectory));if(staged==null){status.Text="Launcher "+LauncherUpdate.Version+" is up to date";return;}LauncherUpdate.StartReplacement(staged,AppDomain.CurrentDomain.BaseDirectory);busy=false;Close();}
   catch(Exception e){MessageBox.Show(this,e.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Information);}
   finally{busy=false;launcherUpdate.Enabled=true;}
  };
  version.Text="Launcher "+LauncherUpdate.Version+" | "+LauncherUpdate.Channel;
  RefreshUpdateButton();
#if TEST_BENCH
  folder.ReadOnly=true;host.ReadOnly=true;browse.Visible=false;
  version.Text="OWNER TEST BENCH • Local server only";
  notes.ShowNotes("Your private test client connects only to the local test server. Click Install to create its separate client folder.\r\n\r\nChanges here are tested privately before promotion to the hosted server.");
#endif
  if(automaticChecks)Shown+=async delegate {if(!string.IsNullOrWhiteSpace(settings.Feed))await Run(false,false);};
 }
 internal void SetPreviewNotes(string text){notes.ShowNotes(text);}
 internal void SetReleaseNotes(Manifest release){
  if(notesReleaseVersion==release.Version)return;
  notesReleaseVersion=release.Version;notesVersions.Items.Clear();
  notesVersions.Items.Add(new ReleaseNote{Version=release.Version,Channel=LauncherUpdate.Channel,Notes=release.Notes,Published=release.Published,SourceBenchVersion=release.SourceBenchVersion});
  var seen=new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase){release.Version};
  if(release.NotesHistory!=null)foreach(var entry in release.NotesHistory){
   if(entry!=null && entry.Channel==LauncherUpdate.Channel && !string.IsNullOrWhiteSpace(entry.Version) && seen.Add(entry.Version))notesVersions.Items.Add(entry);
  }
  notesVersions.Enabled=true;notesVersions.SelectedIndex=0;
 }
 bool HasInstallation() {
  try {
   string directory=folder.Text.Trim();
   return directory.Length>0 && (File.Exists(Path.Combine(directory,"system","L2.exe")) || File.Exists(Path.Combine(directory,".launcher","installed.json")) || File.Exists(Path.Combine(directory,".launcher","transaction.json")));
  } catch(ArgumentException) {return false;} catch(NotSupportedException) {return false;}
 }
 void RefreshUpdateButton() {
  update.Enabled=false;
  if(busy)update.Text=operationText;
  else if(string.IsNullOrWhiteSpace(folder.Text))update.Text="Select folder";
  else if(!HasInstallation()){update.Text="Install";update.Enabled=filesNeeded.HasValue && filesNeeded.Value>0;}
  else if(!filesNeeded.HasValue)update.Text="Check updates";
  else if(filesNeeded.Value>0){update.Text="Update";update.Enabled=true;}
  else update.Text="Up to date";
  update.BackColor=update.Enabled?Color.FromArgb(44,53,67):Color.FromArgb(31,36,44);
  update.FlatAppearance.BorderColor=update.Enabled?Color.FromArgb(77,90,109):Color.FromArgb(49,56,65);
 }
 void InvalidateCheck() {
  filesNeeded=null;play.Enabled=false;RefreshUpdateButton();
  status.Text="Ready to check updates";
  refreshTimer.Stop();if(automaticChecks)refreshTimer.Start();
 }
 void AddLabel(string text,int x,int y){var l=new Label {Text=text,Location=new Point(x,y),Size=new Size(400,20),ForeColor=Color.FromArgb(151,167,185),Font=new Font("Segoe UI",9,FontStyle.Bold)};Controls.Add(l);}
 void Style(TextBox t){t.BackColor=Color.FromArgb(35,41,51);t.ForeColor=ForeColor;t.BorderStyle=BorderStyle.FixedSingle;}
 void MakeButton(Button b,string text,int x,int y,int width){b.Text=text;b.SetBounds(x,y,width,36);b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=Color.FromArgb(77,90,109);b.BackColor=Color.FromArgb(44,53,67);b.ForeColor=ForeColor;Controls.Add(b);}
 async Task Run(bool apply,bool launch,bool fullVerification=false) {
  if(busy)return;refreshTimer.Stop();operationText=apply?(HasInstallation()?"Updating…":"Installing…"):"Checking…";
  busy=true;filesNeeded=null;foreach(var b in new[]{update,repair,play,browse,check,launcherUpdate})b.Enabled=false;folder.Enabled=host.Enabled=false;RefreshUpdateButton();
  try {
   settings.ClientDirectory=folder.Text.Trim();settings.ServerAddress=host.Text.Trim();Patcher.ValidateServer(settings.ServerAddress);Patcher.WriteJson(settingsPath,settings);
   int needed=await Task.Run(()=>{patcher.LoadRelease();if(apply){patcher.Apply(fullVerification);if(!patcher.TestMode)BundledTools.ClientDefaults(settings.ClientDirectory,true);}if(launch){patcher.Play();return 0;}return patcher.Check().Count+(!patcher.TestMode && BundledTools.ClientDefaults(settings.ClientDirectory,false)?1:0);});
   filesNeeded=needed;SetReleaseNotes(patcher.Release);version.Text="Client "+patcher.Release.Version+" | Launcher "+LauncherUpdate.Version+" | "+LauncherUpdate.Channel;
   status.Text=!HasInstallation()?"Ready to install":needed==0?"Ready to play":"Update available — "+needed+" files";
   if(launch)status.Text="Lineage II launched";
   await ServerStatus();
  } catch(Exception e) {status.Text=e.Message;MessageBox.Show(this,e.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Information);}
  finally {busy=false;foreach(var b in new[]{repair,browse,check,launcherUpdate})b.Enabled=true;folder.Enabled=host.Enabled=true;play.Enabled=filesNeeded.HasValue && filesNeeded.Value==0;RefreshUpdateButton();}
 }
 async Task ServerStatus(){bool ok=await Task.Run(()=>{try{using(var c=new TcpClient()){var r=c.BeginConnect(settings.ServerAddress,2106,null,null);if(!r.AsyncWaitHandle.WaitOne(1500))return false;c.EndConnect(r);return true;}}catch{return false;}});server.Text=ok?"Login server reachable":"Login server unavailable • Check server / Tailscale";}
}
static class Program {
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd,IntPtr hdc,uint flags);
 [STAThread] static int Main(string[] args) {
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  try {
   if(args.Length==4 && args[0]=="--finish-launcher-update"){LauncherUpdate.Finish(args[1],int.Parse(args[2]),args[3]);return 0;}
   if(args.Length>0 && args[0]=="--create-key") {
    if(File.Exists(args[1]))throw new IOException("Signing key already exists.");
    using(var r=new RSACryptoServiceProvider(3072)){r.PersistKeyInCsp=false;File.WriteAllText(args[1],r.ToXmlString(true));File.WriteAllText(args[2],r.ToXmlString(false));}return 0;
   }
   if(args.Length>0 && args[0]=="--sign") {using(var r=new RSACryptoServiceProvider()){r.PersistKeyInCsp=false;r.FromXmlString(File.ReadAllText(args[1]));File.WriteAllText(args[2]+".sig",Convert.ToBase64String(r.SignData(File.ReadAllBytes(args[2]),CryptoConfig.MapNameToOID("SHA256"))));}return 0;}
   string home=AppDomain.CurrentDomain.BaseDirectory,path=Path.Combine(home,"launcher.json");
   if(!File.Exists(path))throw new IOException("launcher.json is missing. Keep it next to the launcher.");
   var settings=Patcher.Json.Deserialize<Settings>(File.ReadAllText(path));
#if TEST_BENCH
   settings.Title="Prominence — TEST BENCH";
   settings.ServerAddress="127.0.0.1";
   settings.ClientDirectory=Path.Combine(home,"client");
   Uri benchFeed;
   if(!settings.AllowLocalFeed || !Uri.TryCreate(settings.Feed,UriKind.Absolute,out benchFeed) || !benchFeed.IsFile)
    throw new IOException("The test bench requires its private local update feed.");
#endif
   if(args.Length>0 && args[0]=="--check") {
#if !TEST_BENCH
    if(args.Length>1)settings.ClientDirectory=args[1];
#endif
    var p=new Patcher(settings);p.LoadRelease();var missing=p.Check();Console.WriteLine("Release "+p.Release.Version+": "+missing.Count+" files need updating.");return missing.Count==0?0:2;
   }
   if(args.Length==0)BundledTools.Ensure(home);
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   bool preview=args.Length>0 && args[0]=="--preview";
   using(var w=new Window(settings,path,preview)) {
    if(preview){if(args.Length>2)w.SetPreviewNotes(File.ReadAllText(args[2]));w.ShowInTaskbar=false;w.StartPosition=FormStartPosition.Manual;w.Location=new Point(-30000,-30000);w.Show();Application.DoEvents();using(var b=new Bitmap(w.Width,w.Height)){using(var g=Graphics.FromImage(b)){IntPtr dc=g.GetHdc();try{PrintWindow(w.Handle,dc,0);}finally{g.ReleaseHdc(dc);}}b.Save(args[1]);}w.Hide();return 0;}
    Application.Run(w);
   }
   return 0;
  } catch(Exception e) {if(args.Length>0){Console.Error.WriteLine(e.ToString());return 1;}MessageBox.Show(e.Message,"Interlude Launcher",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}
 }
}
}
