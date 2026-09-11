using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LocalL2Keys
{
    // Interlude (ct0) layout fields, based on acmi/xdat_editor_schema.
    // Preserve original bytes and replace only named fields or our own added controls.
    internal sealed class UiNode
    {
        public string Kind, Parent;
        public int Start, End;
        public Dictionary<string, object> Values = new Dictionary<string, object>();
        public Dictionary<string, int[]> Fields = new Dictionary<string, int[]>();
        public List<UiNode> Children = new List<UiNode>();
        public string S(string key) { return (string)Values[key]; }
        public int I(string key) { return (int)Values[key]; }
    }
    internal sealed class ByteEdit
    {
        public int Start, End; public byte[] Value;
        public ByteEdit(int start, int end, byte[] value) { Start = start; End = end; Value = value; }
    }
    internal sealed class XdatDocument
    {
        public readonly byte[] Data;
        public readonly List<UiNode> Nodes = new List<UiNode>();
        public List<UiNode> Actions;
        private int pos;
        public XdatDocument(byte[] data)
        {
            Data = data;
            int count = Count(); for (int i = 0; i < count; i++) ReadNode("Window", "");
            count = Count();
            for (int i = 0; i < count; i++)
            {
                UiNode group = ReadNode("Shortcut", "");
                if (group.S("unk0") == "GamingStateShortcut") Actions = group.Children;
            }
            Int(); count = Count(); for (int i = 0; i < count; i++) ReadNode("WndDefPos", "");
            if ((Data.Length - pos != 0 && Data.Length - pos != 20) || Actions == null) throw new InvalidDataException("Unsupported client interface layout.");
        }
        private void Need(int count) { if (count < 0 || pos > Data.Length - count) throw new InvalidDataException("Truncated client interface data."); }
        private int Int() { Need(4); int value = BitConverter.ToInt32(Data, pos); pos += 4; return value; }
        private int Count() { int count = Int(); if (count < 0 || count > 10000) throw new InvalidDataException("Invalid interface entry count."); return count; }
        private string String() { return ReadString(Data, ref pos); }
        internal static string ReadString(byte[] data, ref int offset)
        {
            if (offset >= data.Length) throw new InvalidDataException("Truncated client string.");
            byte b = data[offset++]; int size = b & 63, sign = b & 128, more = b & 64, shift = 6;
            while (more != 0)
            {
                if (shift > 27 || offset >= data.Length) throw new InvalidDataException("Invalid client string length.");
                b = data[offset++]; size |= (b & 127) << shift; more = b & 128; shift += 7;
            }
            if (sign != 0) size = checked(size * 2);
            if (size < 0 || offset > data.Length - size) throw new InvalidDataException("Truncated client string.");
            string value = (sign != 0 ? Encoding.Unicode : Encoding.GetEncoding(28591)).GetString(data, offset, size).TrimEnd('\0'); offset += size; return value;
        }
        internal static byte[] StringBytes(string value)
        {
            if (value.Length == 0) return new byte[] { 0 };
            bool unicode = value.Any(c => c > 255);
            byte[] raw = (unicode ? Encoding.Unicode : Encoding.GetEncoding(28591)).GetBytes(value + "\0");
            int count = unicode ? raw.Length / 2 : raw.Length;
            var result = new List<byte>(); byte first = (byte)(count & 63); count >>= 6;
            if (unicode) first |= 128; if (count != 0) first |= 64; result.Add(first);
            while (count != 0) { byte b = (byte)(count & 127); count >>= 7; if (count != 0) b |= 128; result.Add(b); }
            result.AddRange(raw); return result.ToArray();
        }
        private object Field(UiNode node, string name, bool text = false)
        {
            int start = pos; object value = text ? (object)String() : Int(); node.Fields[name] = new[] { start, pos }; node.Values[name] = value; return value;
        }
        private int I(UiNode n, string name) { return (int)Field(n, name); }
        private void S(UiNode n, string name) { Field(n, name, true); }
        private UiNode ReadNode(string kind, string parent)
        {
            int start = pos; if (kind == null) kind = String();
            var node = new UiNode { Kind = kind, Start = start, Parent = parent };
            string schema;
            if (!XdatSchema.Fields.TryGetValue(kind, out schema)) throw new InvalidDataException("Unknown client interface element: " + kind);
            if (!new[] { "Action", "ComboBoxElement", "ListElement", "TabElement", "Shortcut", "WndDefPos" }.Contains(kind))
            {
                S(node, "name"); S(node, "superName"); I(node, "unk2"); I(node, "unk3"); S(node, "unk4"); S(node, "unk5"); S(node, "unk6"); I(node, "unk7");
                if (I(node, "size") != 0)
                {
                    if (I(node, "size_absolute_values") == 0) { Need(1); if (Data[pos++] != 0) throw new InvalidDataException("Unsupported relative size marker."); I(node, "size_percent_width"); I(node, "size_percent_height"); }
                    I(node, "size_absolute_width"); I(node, "size_absolute_height");
                }
                if (I(node, "anchor") != 0) { I(node, "anchor_parent"); I(node, "anchor_this"); S(node, "anchor_ctrl"); I(node, "anchor_x"); I(node, "anchor_y"); }
                I(node, "unk22"); I(node, "unk23"); I(node, "unk24"); S(node, "popupType"); I(node, "popupValue");
                parent += (parent.Length == 0 ? "" : ".") + node.S("name");
            }
            foreach (string field in schema.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = field.Split(':'); string name = parts[1];
                if (parts[0].StartsWith("@"))
                {
                    int count = I(node, name + "_count"); if (count < 0 || count > 10000) throw new InvalidDataException("Invalid interface child count.");
                    string subtype = parts[0].Substring(1);
                    for (int i = 0; i < count; i++) node.Children.Add(ReadNode(subtype == "DefaultProperty" ? null : subtype, parent));
                }
                else Field(node, name, parts[0] == "s");
            }
            node.End = pos; Nodes.Add(node); return node;
        }
        public static ByteEdit Edit(UiNode node, string field, object value)
        {
            int[] range = node.Fields[field]; return new ByteEdit(range[0], range[1], value is string ? StringBytes((string)value) : BitConverter.GetBytes((int)value));
        }
        public static byte[] Patch(byte[] original, IEnumerable<ByteEdit> edits, int start = 0, int end = -1)
        {
            if (end < 0) end = original.Length;
            using (var output = new MemoryStream())
            {
                int cursor = start;
                foreach (ByteEdit edit in edits.OrderBy(e => e.Start).ThenBy(e => e.End))
                {
                    if (edit.Start < cursor || edit.End < edit.Start || edit.End > end) throw new InvalidDataException("Overlapping interface changes.");
                    output.Write(original, cursor, edit.Start - cursor); output.Write(edit.Value, 0, edit.Value.Length); cursor = edit.End;
                }
                output.Write(original, cursor, end - cursor); return output.ToArray();
            }
        }
    }

    internal static class ClientUiLabels
    {
        private const string Prefix = "LocalKeyLabel_";
        public static string KeyLabel(UiNode action, bool compact = false)
        {
            int key = action.I("key_1"); if (key == 0) return "";
            var names = new Dictionary<int, string> { {192,"`"}, {189,"-"}, {187,"="}, {219,"["}, {221,"]"}, {220,"\\"}, {186,";"}, {222,"'"}, {188,","}, {190,"."}, {191,"/"}, {33,"PgUp"}, {34,"PgDn"}, {45,"Ins"}, {46,"Del"}, {8,"Bksp"}, {32,"Space"} };
            string name; if (!names.TryGetValue(key, out name)) name = key >= 48 && key <= 57 ? ((char)key).ToString() : ((Keys)key).ToString();
            var mods = new[] { action.I("key_2"), action.I("key_3") };
            return (mods.Contains(17) ? (compact ? "C+" : "Ctrl+") : "") + (mods.Contains(18) ? (compact ? "A+" : "Alt+") : "") + (mods.Contains(16) ? (compact ? "S+" : "Shift+") : "") + name;
        }
        private static int Option(string options, string key, int fallback)
        {
            var m = Regex.Match(options, @"(?im)^" + Regex.Escape(key) + @"\s*=\s*(\d+)"); int value; return m.Success && int.TryParse(m.Groups[1].Value, out value) ? value : fallback;
        }
        private static bool Enabled(string options, string key)
        {
            return !Regex.IsMatch(options, @"(?im)^" + Regex.Escape(key) + @"\s*=\s*False\s*$");
        }
        public static byte[] Layout(byte[] bytes, string options)
        {
            var doc = new XdatDocument(bytes); var edits = new List<ByteEdit>();
            UiNode template = doc.Nodes.Single(n => n.Kind == "TextBox" && n.Parent == "ShortcutWnd.ShortcutWndHorizontal" && n.S("name") == "PageNumTextBox");
            var bars = doc.Nodes.Where(n => n.Kind == "Window" && n.Parent == "ShortcutWnd" && Regex.IsMatch(n.S("name"), @"^ShortcutWnd(Horizontal|Vertical)(_[1-4])?$" )).ToList();
            if (bars.Count != 10) throw new InvalidDataException("The ten keyboard hotbar layouts could not be located.");
            foreach (UiNode bar in bars)
            {
                string name = bar.S("name"); Match suffix = Regex.Match(name, @"_(\d)$"); int panel = suffix.Success ? int.Parse(suffix.Groups[1].Value) + 1 : 1;
                UiNode strip = bar.Children.Single(n => n.Kind == "Texture" && n.S("name") == "Shortcut_keybind");
                edits.Add(XdatDocument.Edit(strip, "alpha", 0));
                var old = bar.Children.Where(n => n.S("name").StartsWith(Prefix, StringComparison.Ordinal)).ToList();
                foreach (UiNode node in old) edits.Add(new ByteEdit(node.Start, node.End, new byte[0]));
                int group = -1;
                string[] flags = { "UseFPad", "UseNumpad", "UseQwerty" };
                for (int g = 0; g < 3; g++) if (Option(options, "Panel" + (g + 1), g + 1) == panel && Enabled(options, flags[g])) group = g;
                int added = 0;
                using (var labels = new MemoryStream())
                {
                    if (group >= 0)
                    {
                        for (int slot = 0; slot < 12; slot++)
                        {
                            // This Interlude table stores each family in Ctrl, primary, Shift order.
                            // Keep using the primary record even when the user gives it a modifier.
                            var family = doc.Actions.Where(a => a.S("action") == "UseShortcutNum=" + (group * 12 + slot)).ToList();
                            if (family.Count != 3) throw new InvalidDataException("Unsupported hotbar shortcut family.");
                            string key = KeyLabel(family[1], true); if (key.Length == 0) continue;
                            UiNode item = bar.Children.Single(n => n.Kind == "ShortcutItemWindow" && n.S("name") == "Shortcut" + (slot + 1));
                            for (int shadow = 1; shadow >= 0; shadow--)
                            {
                                var values = new Dictionary<string, object> {
                                    {"name", Prefix + (slot + 1) + (shadow == 1 ? "_Shadow" : "")}, {"unk4",name}, {"unk2",1},
                                    {"size_absolute_width",36}, {"size_absolute_height",14}, {"anchor_parent",1}, {"anchor_this",1}, {"anchor_ctrl",""},
                                    {"anchor_x",item.I("anchor_x") + shadow}, {"anchor_y",item.I("anchor_y") + shadow - 1},
                                    {"text",key}, {"textAlign",1}, {"offsetY",0}, {"textColor",shadow == 1 ? unchecked((int)0xff000000) : unchecked((int)0xffffe6a6)},
                                    {"fontType",0}, {"emoticon",0}, {"autosize",0}
                                };
                                byte[] label = XdatDocument.Patch(bytes, values.Select(v => XdatDocument.Edit(template,v.Key,v.Value)), template.Start, template.End);
                                labels.Write(label,0,label.Length); added++;
                            }
                        }
                    }
                    edits.Add(new ByteEdit(bar.End,bar.End,labels.ToArray()));
                }
                edits.Add(XdatDocument.Edit(bar,"children_count",bar.Children.Count-old.Count+added));
            }
            byte[] result = XdatDocument.Patch(bytes,edits); var verify = new XdatDocument(result);
            if (!doc.Actions.Select(ActionSignature).SequenceEqual(verify.Actions.Select(ActionSignature))) throw new InvalidDataException("UI label update changed the native bindings.");
            return result;
        }
        private static string ActionSignature(UiNode a) { return a.S("action") + "|" + a.I("key_1") + "|" + a.I("key_2") + "|" + a.I("key_3"); }
        public static byte[] Tooltips(byte[] plain, byte[] xdat)
        {
            var actions = new XdatDocument(xdat).Actions;
            var commands = new Dictionary<int,string> { {193,"ShowSystemMenuWindow"}, {194,"ShowDetailStatusWnd"}, {195,"ShowInventoryWindow"}, {196,"ShowSkillWnd"}, {197,"ShowActionWnd"}, {198,"ShowQuestWnd"}, {390,"ShowBoardWindow"}, {895,"ShowClanWnd"}, {925,"RequestOpenMinimap"}, {1190,"ShowMacroListWindow"}, {1507,"ShowClanWnd"} };
            int count = BitConverter.ToInt32(plain,0), pos = 4; if (count < 0 || count > 10000) throw new InvalidDataException("Invalid tooltip table.");
            var edits = new List<ByteEdit>(); var found = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                if (pos > plain.Length - 4) throw new InvalidDataException("Truncated tooltip table.");
                int id = BitConverter.ToInt32(plain,pos); pos += 4; int start = pos; string text = XdatDocument.ReadString(plain,ref pos), command;
                if (!commands.TryGetValue(id,out command)) continue;
                found.Add(id);
                var keys = actions.Where(a => a.S("action") == command && a.I("key_1") != 0).Select(a => KeyLabel(a)).Distinct().ToList();
                string title = Regex.Replace(text,@"\s*\([^)]*\)\s*$","");
                string updated = title + (keys.Count == 0 ? " (Unbound)" : " (" + string.Join(" / ",keys) + ")");
                if (updated != text) edits.Add(new ByteEdit(start,pos,XdatDocument.StringBytes(updated)));
            }
            if (found.Count != commands.Count) throw new InvalidDataException("The expected interface tooltip strings are missing.");
            return XdatDocument.Patch(plain,edits);
        }
    }
}
