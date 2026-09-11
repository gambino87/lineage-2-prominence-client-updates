using System;
using System.IO;
using System.Linq;
using LocalL2Keys;

internal static class NativeBindingsTests
{
    private static int passed;
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); passed++; }
    private static void Main(string[] args)
    {
        string root = args[0], fixture = Path.Combine(root, "state", "native-binding-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixture);
        byte[] liveXdat = File.ReadAllBytes(Path.Combine(root,"client","system","interface.xdat")), originalIni = File.ReadAllBytes(Path.Combine(root,"client","system","user.ini"));
        byte[] originalXdat = File.ReadAllBytes(Path.Combine(root,"backups","client-bindings","20260905-015541-409-329ff9","interface.xdat"));
        File.WriteAllBytes(Path.Combine(fixture,"interface.xdat"),originalXdat); File.WriteAllBytes(Path.Combine(fixture,"user.ini"),originalIni);
        foreach (string name in new[]{"sysstring-e.dat","Option.ini"}) File.Copy(Path.Combine(root,"client","system",name),Path.Combine(fixture,name));
        var store = new NativeClientBindings(root, fixture);
        Check(store.Controls.Count(c=>c.Category=="Hotbars")==36,"Load all hotbar slots, including unassigned keys.");
        Check(store.Controls.Any(c=>c.InputName=="RightMouse" && c.Label=="Rotate camera"),"Retain the confirmed camera fix.");
        store.ImportProfile(Path.Combine(root,"keybinds","profiles","Starter.json"));
        Check(store.Controls.Single(c=>c.Category=="Hotbars" && c.Command=="UseShortcutNum=0").Key=="D1","Migrate 1 to first hotbar slot.");
        Check(store.Controls.Single(c=>c.Category=="Hotbars" && c.Command=="UseShortcutNum=12").Key=="","Clear the conflicting second hotbar key.");
        Check(store.Controls.Any(c=>c.Command=="UseShortcutNum=0" && c.Key=="Ctrl+D1"),"Ctrl variant follows the new key.");
        Check(store.Controls.Any(c=>c.Command=="UseShortcutNum=0" && c.Key=="Shift+D1"),"Shift variant follows the new key.");
        Check(store.Controls.Single(c=>c.Command=="ShowInventoryWindow").Key=="B","Inventory becomes a native B shortcut.");
        Check(!store.Controls.Any(c=>c.Command=="ShowBoardWindow" && c.Key=="B"),"B must not also open the community board.");
        Check(store.Controls.Any(c=>c.Command=="UseShortcutNum=11" && c.Key=="Tab"),"Tab becomes hotbar slot 12.");
        Check(!store.Controls.Any(c=>c.Command=="TabShowInventoryWindow" && c.Key=="Tab"),"Tab must no longer toggle Inventory.");
        Check(File.ReadAllBytes(Path.Combine(fixture,"user.ini")).SequenceEqual(originalIni),"Importing UI shortcuts does not touch the camera INI.");
        byte[] imported = File.ReadAllBytes(Path.Combine(fixture,"interface.xdat"));
        var layout = new XdatDocument(imported);
        Check(layout.Nodes.Count(n=>n.Kind=="TextBox" && n.S("name").StartsWith("LocalKeyLabel_"))==120,"Add labels for bound slots in both orientations, leaving collisions blank.");
        Check(layout.Nodes.Single(n=>n.Kind=="TextBox" && n.Parent=="ShortcutWnd.ShortcutWndHorizontal" && n.S("name")=="LocalKeyLabel_1").S("text")=="1","First hotbar label matches the saved binding.");
        Check(layout.Nodes.Single(n=>n.Kind=="TextBox" && n.Parent=="ShortcutWnd.ShortcutWndVertical" && n.S("name")=="LocalKeyLabel_12").S("text")=="Tab","Vertical hotbar shows Tab.");
        store.Apply(new NativeAssignment[0]);
        Check(File.ReadAllBytes(Path.Combine(fixture,"interface.xdat")).SequenceEqual(imported),"Label sync is idempotent and does not duplicate controls.");
        var inventory = store.Controls.Single(c=>c.Command=="ShowInventoryWindow");
        store.Apply(new[]{new NativeAssignment{Control=inventory,Key="Alt+V"}});
        Check(new NativeClientBindings(root,fixture).Controls.Single(c=>c.Command=="ShowInventoryWindow").Key=="Alt+V","Saved shortcut survives closing and reopening the editor.");
        var move = store.Controls.Single(c=>c.InputName=="W");
        store.Apply(new[]{new NativeAssignment{Control=move,Key="E"}});
        Check(store.Controls.Any(c=>c.InputName=="E" && c.Command==move.Command),"Write the full held-movement command to the new INI key.");
        Check(!store.Controls.Any(c=>c.InputName=="W"),"Remove the old movement key.");
        Check(!store.Controls.Any(c=>c.KeyOffset>=0 && c.Key=="E"),"Movement must not also fire a native hotbar shortcut.");
        NativeClientBindings.ValidateIni(File.ReadAllBytes(Path.Combine(fixture,"user.ini"))); passed++;
        Check(store.Controls.Single(c=>c.InputName=="RightMouse").Command=="CameraRotationModeOn | CameraRotationModeOff","Re-encoding movement preserves the camera fix.");
        byte[] beforeFailure=File.ReadAllBytes(Path.Combine(fixture,"interface.xdat")); bool rejected=false;
        try { store.Apply(new[]{new NativeAssignment{Control=store.Controls.Single(c=>c.Command=="ShowInventoryWindow"),Key="Enter"}}); } catch(ArgumentException){rejected=true;}
        Check(rejected && File.ReadAllBytes(Path.Combine(fixture,"interface.xdat")).SequenceEqual(beforeFailure),"Rejected key must not alter files.");
        File.AppendAllText(Path.Combine(fixture,"interface.xdat"),"changed"); rejected=false;
        try { store.Apply(new[]{new NativeAssignment{Control=store.Controls.Single(c=>c.Command=="ShowInventoryWindow"),Key="B"}}); } catch(IOException){rejected=true;}
        Check(rejected,"Reject a stale editor snapshot instead of overwriting external changes.");
        Check(File.ReadAllBytes(Path.Combine(root,"client","system","interface.xdat")).SequenceEqual(liveXdat) && File.ReadAllBytes(Path.Combine(root,"client","system","user.ini")).SequenceEqual(originalIni),"Tests must leave the real client untouched.");
        Console.WriteLine(passed+" native binding checks passed. Fixture: "+fixture);
    }
    private static System.Collections.Generic.HashSet<int> ToHashSetCompat(this System.Collections.Generic.IEnumerable<int> values) { return new System.Collections.Generic.HashSet<int>(values); }
}
