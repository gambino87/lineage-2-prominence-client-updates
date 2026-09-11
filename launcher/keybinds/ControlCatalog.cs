using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalL2Keys
{
    public sealed class GameControl
    {
        public string Id, Category, Label, Key, Hint, Command, InputName;
        public int KeyOffset = -1;
        public int KeyIndex = -1;
        public bool Hold, Editable;
    }

    // Reads the installed client's gaming shortcuts, not a generic Interlude preset.
    public static class ControlCatalog
    {
        private static string ReadString(BinaryReader reader)
        {
            int first = reader.ReadByte(), length = first & 63, shift = 6;
            bool more = (first & 64) != 0;
            while (more) { int next = reader.ReadByte(); length |= (next & 127) << shift; shift += 7; more = (next & 128) != 0; if (shift > 27) throw new InvalidDataException("Invalid client string."); }
            if (length > 4096) throw new InvalidDataException("Invalid client string length.");
            int bytes = (first & 128) != 0 ? length * 2 : length;
            byte[] value = reader.ReadBytes(bytes);
            if (value.Length != bytes) throw new EndOfStreamException();
            return ((first & 128) != 0 ? Encoding.Unicode : Encoding.ASCII).GetString(value).TrimEnd('\0');
        }

        public static List<GameControl> ReadShortcuts(string path)
        {
            byte[] data = File.ReadAllBytes(path), marker = Encoding.ASCII.GetBytes("\x14GamingStateShortcut\0\x0cGamingState\0");
            int offset = -1;
            for (int i = 0; i <= data.Length - marker.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < marker.Length; j++) if (data[i + j] != marker[j]) { match = false; break; }
                if (match) { if (offset >= 0) throw new InvalidDataException("More than one game shortcut table was found."); offset = i; }
            }
            if (offset < 0) throw new InvalidDataException("The client's shortcut table was not found.");
            var controls = new List<GameControl>();
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                reader.BaseStream.Position = offset;
                ReadString(reader); ReadString(reader);
                int count = reader.ReadInt32();
                if (count < 1 || count > 1000) throw new InvalidDataException("Invalid client shortcut count.");
                for (int i = 0; i < count; i++)
                {
                    int keyOffset = (int)reader.BaseStream.Position;
                    int key = reader.ReadInt32(), second = reader.ReadInt32(), third = reader.ReadInt32();
                    string command = ReadString(reader);
                    int mods = 0;
                    foreach (int modifier in new[] { second, third })
                    {
                        if (modifier == 17) mods |= 1; else if (modifier == 18) mods |= 2; else if (modifier == 16) mods |= 4;
                        else if (modifier != 0) throw new InvalidDataException("Unsupported client shortcut modifier.");
                    }
                    var chord = new Chord(key, mods);
                    var control = new GameControl { Id = "ui:" + command + ":" + chord, Command = command, KeyOffset = keyOffset, KeyIndex = i, Category = "Interface", Label = ActionLabel(command), Key = key == 0 ? "" : chord.ToString(), Editable = key != 13 && key != 27, Hint = "Saved in interface.xdat. The client handles shortcut and chat focus." };
                    if (command.StartsWith("UseShortcutNum="))
                    {
                        int slot = int.Parse(command.Substring(15));
                        control.Category = mods == 0 ? "Hotbars" : "Hotbar modifiers";
                        control.Label = "Hotbar " + (slot / 12 + 1) + " · Slot " + (slot % 12 + 1) + (mods == 0 ? "" : mods == 1 ? " · Ctrl" : " · Shift");
                    }
                    else if (command.StartsWith("SetShortcutPage Num="))
                    {
                        control.Category = "Hotbar pages"; control.Label = "Select hotbar page " + (int.Parse(command.Substring(20)) + 1);
                        if (chord.Key == 115 && (chord.Mods & 2) != 0) { control.Editable = false; control.Hint = "The client lists Alt+F4 here, but Windows reserves this combination for closing a window."; }
                    }
                    else if (key == 13 || key == 27) { control.Category = "Chat and targeting"; control.Hint = "Kept available for typing and canceling."; }
                    controls.Add(control);
                }
            }
            return controls;
        }

        private static string ActionLabel(string action)
        {
            var labels = new Dictionary<string, string> {
                {"TargetCancel", "Cancel target / close"}, {"FocusChatWindow", "Focus chat"}, {"TabShowInventoryWindow", "Inventory · Tab"},
                {"ShowInventoryWindow", "Inventory"}, {"ShowChatWindow", "Show / hide chat"}, {"HideAllWindow", "Show / hide interface"},
                {"CloseAllWindow", "Close all windows"}, {"SetPrevChatType", "Previous chat tab"}, {"SetNextChatType", "Next chat tab"},
                {"ShowOptionWnd", "Options"}, {"ShowClanWnd", "Clan"}, {"ShowQuestWnd", "Quests"}, {"ShowDetailStatusWnd", "Character status"},
                {"ShowSystemMenuWindow", "System menu"}, {"ShowPartyBuff", "Party buffs"}, {"ShowSkillWnd", "Skills"}, {"ShowBoardWindow", "Community board"},
                {"ShowActionWnd", "Actions"}, {"RequestOpenMinimap", "Map"}, {"ShowGMWindow", "GM window"}, {"ShowGMPetitionWindow", "GM petitions"},
                {"ApplyMinFrame", "Minimum frame rate mode"}, {"ShowMessengerWindow", "Messenger"}, {"ShowMacroListWindow", "Macros"}
            };
            string label; return labels.TryGetValue(action, out label) ? label : Regex.Replace(action, "([a-z])([A-Z])", "$1 $2");
        }

        public static List<GameControl> ReadInput(string text)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool input = false;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.StartsWith("[")) { input = line.Equals("[Engine.Input]", StringComparison.OrdinalIgnoreCase); continue; }
                if (!input || line.StartsWith(";") || line.StartsWith("Aliases[")) continue;
                int equals = line.IndexOf('='); if (equals <= 0) continue;
                values[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
            }
            var controls = new List<GameControl>();
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { {"GreySlash", "Divide"}, {"GreyStar", "Multiply"}, {"GreyPlus", "Add"}, {"GreyMinus", "Subtract"}, {"PrintScrn", "PrintScreen"}, {"NumPadPeriod", "Decimal"}, {"Semicolon","OemSemicolon"}, {"Equals","Oemplus"}, {"Comma","Oemcomma"}, {"Minus","OemMinus"}, {"Period","OemPeriod"}, {"Slash","OemQuestion"}, {"Tilde","Oemtilde"}, {"LeftBracket","OemOpenBrackets"}, {"Backslash","OemPipe"}, {"RightBracket","OemCloseBrackets"}, {"SingleQuote","OemQuotes"} };
            foreach (var pair in values)
            {
                if (pair.Value.Length == 0) continue;
                string key; if (!names.TryGetValue(pair.Key, out key)) key = pair.Key;
                string command = pair.Value;
                var control = new GameControl { Id = "input:" + pair.Key, Command = command, InputName = pair.Key, Category = "Camera and extras", Label = pair.Key, Key = key, Hint = command, Editable = true, Hold = command.Contains("OnRelease") };
                if (command.StartsWith("LeftTurning")) { control.Category = "Movement"; control.Label = "Turn left"; }
                else if (command.StartsWith("RightTurning")) { control.Category = "Movement"; control.Label = "Turn right"; }
                else if (command.StartsWith("KeyboardMoveStart Dir=1 |")) { control.Category = "Movement"; control.Label = "Move forward"; }
                else if (command.StartsWith("KeyboardMoveStart Dir=4 |")) { control.Category = "Movement"; control.Label = "Move backward"; }
                else if (command == "KeyboardPermanentMove") { control.Category = "Movement"; control.Label = "Auto-run"; }
                else if (pair.Key == "PageUp") control.Label = "Previous camera preset";
                else if (pair.Key == "PageDown") control.Label = "Next camera preset";
                else if (pair.Key == "Home") control.Label = "Default camera";
                else if (pair.Key == "End") control.Label = "Show frame rate";
                else if (pair.Key == "PrintScrn") control.Label = "Screenshot";
                else if (pair.Key == "Delete") control.Label = "Toggle fog";
                else if (pair.Key == "Space") control.Label = "Camera move up / pause replay";
                else if (pair.Key == "Ctrl") control.Label = "Camera move down";
                else if (pair.Key == "GreySlash") control.Label = "Increase camera acceleration";
                else if (pair.Key == "GreyStar") control.Label = "Decrease camera acceleration";
                else if (pair.Key == "GreyPlus") control.Label = "Increase camera / replay speed";
                else if (pair.Key == "GreyMinus") control.Label = "Decrease camera / replay speed";
                else if (pair.Key == "Escape") control.Label = "Cancel camera selection";
                else if (pair.Key == "LeftMouse") control.Label = "Click to move / interact";
                else if (pair.Key == "RightMouse") control.Label = command.Contains("FixedDefaultCamera") ? "Rotate / reset camera" : "Rotate camera";
                else if (pair.Key == "MiddleMouse") control.Label = "Turn camera back";
                else if (pair.Key == "MouseX") control.Label = "Camera yaw";
                else if (pair.Key == "MouseY") control.Label = "Camera pitch";
                else if (pair.Key == "MouseWheelDown") control.Label = "Camera zoom in";
                else if (pair.Key == "MouseWheelUp") control.Label = "Camera zoom out";
                if (pair.Key.StartsWith("Joy")) { control.Category = "Gamepad"; control.Editable = false; }
                else if (pair.Key.Contains("Mouse")) { control.Category = "Mouse"; control.Editable = false; }
                try { Chord chord = Chord.Parse(key); control.Key = chord.ToString(); if (chord.Key == 13 || chord.Key == 27) control.Editable = false; }
                catch (ArgumentException) { control.Editable = false; }
                if (!control.Editable) control.Hint = "Shown for reference; this helper reassigns regular keyboard keys only. " + command;
                controls.Add(control);
            }
            return controls;
        }

        public static List<GameControl> Load(string root)
        {
            var controls = ReadShortcuts(Path.Combine(root, "client", "system", "interface.xdat"));
            string temp = Path.GetTempFileName();
            try
            {
                var start = new ProcessStartInfo(Path.Combine(root, "runtime", "jdk-25.0.4", "bin", "java.exe"),
                    "-cp \"" + Path.Combine(root, "tools", "client-config") + "\" ClientConfig decode \"" + Path.Combine(root, "client", "system", "user.ini") + "\" \"" + temp + "\"") {
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true
                };
                using (var process = Process.Start(start))
                {
                    if (!process.WaitForExit(10000)) { process.Kill(); throw new IOException("Reading the client controls timed out."); }
                    if (process.ExitCode != 0) throw new IOException("Could not read the client's movement controls.");
                }
                controls.AddRange(ReadInput(File.ReadAllText(temp, Encoding.Default)));
            }
            finally { File.Delete(temp); }
            string[] order = { "Hotbars", "Movement", "Interface", "Hotbar pages", "Hotbar modifiers", "Chat and targeting", "Camera and extras", "Mouse", "Gamepad" };
            return controls.OrderBy(c => Array.IndexOf(order, c.Category)).ToList();
        }
    }

    public sealed class ControlRow
    {
        public GameControl Control;
        public Binding Override;
        public string Key, State;
    }

    public static class ControlRows
    {
        public static Profile Copy(Profile value)
        {
            return new Profile { Version = value.Version, EnterChat = value.EnterChat, Bindings = value.Bindings.Select(b => new Binding { Label = b.Label, From = b.From, To = b.To, Enabled = b.Enabled, Hold = b.Hold, ControlId = b.ControlId }).ToList() };
        }
        public static List<ControlRow> Build(List<GameControl> controls, Profile profile)
        {
            var remaining = new List<Binding>(profile.Bindings);
            var rows = new List<ControlRow>();
            foreach (GameControl control in controls)
            {
                Binding binding = remaining.FirstOrDefault(b => b.ControlId == control.Id);
                if (binding == null) binding = remaining.FirstOrDefault(b => string.IsNullOrEmpty(b.ControlId) && b.To == control.Key);
                if (binding != null) remaining.Remove(binding);
                rows.Add(Make(control, binding, profile));
            }
            foreach (Binding binding in remaining)
                rows.Add(Make(new GameControl { Id = "custom:" + binding.From + ":" + binding.To, Category = "Custom", Label = binding.Label, Key = binding.To, Hold = binding.Hold, Editable = true, Hint = "Custom shortcut sending " + binding.To }, binding, profile));
            return rows;
        }
        private static ControlRow Make(GameControl control, Binding binding, Profile profile)
        {
            var row = new ControlRow { Control = control, Override = binding, Key = binding != null && binding.Enabled ? binding.From : control.Key, State = binding == null ? "Client default" : binding.Enabled ? "Changed" : "Override off" };
            if (!control.Editable) row.State = "Reference only";
            if (binding == null || !binding.Enabled)
            {
                try
                {
                    Chord source = Chord.Parse(control.Key);
                    Binding redirect = profile.Bindings.FirstOrDefault(b => b.Enabled && Chord.Parse(b.From).ToString() == source.ToString()) ?? profile.Bindings.FirstOrDefault(b => b.Enabled && Chord.Parse(b.From).Key == source.Key && Chord.Parse(b.From).Mods == 0);
                    if (redirect != null)
                    {
                        Chord dest = Chord.Parse(redirect.To), from = Chord.Parse(redirect.From);
                        var actual = new Chord(dest.Key, (source.Mods & ~from.Mods) | dest.Mods);
                        if (actual.ToString() != source.ToString()) { row.Key = "—"; row.State = control.Key + " reassigned"; }
                    }
                }
                catch (ArgumentException) { }
            }
            return row;
        }
        public static Profile Assign(Profile profile, ControlRow row, string key)
        {
            if (!row.Control.Editable) throw new ArgumentException(row.Control.Hint);
            Chord chord = Chord.Parse(key);
            var candidate = Copy(profile);
            if (row.Override != null) candidate.Bindings.RemoveAt(profile.Bindings.IndexOf(row.Override));
            string normalized = chord.ToString();
            Binding conflict = candidate.Bindings.FirstOrDefault(b => b.Enabled && Chord.Parse(b.From).ToString() == normalized);
            if (conflict != null) throw new ArgumentException(normalized + " is already assigned to " + conflict.Label + ". Choose another key, or reset that binding first.");
            if (normalized != row.Control.Key)
                candidate.Bindings.Add(new Binding { ControlId = row.Control.Category == "Custom" ? null : row.Control.Id, Label = row.Control.Label, From = normalized, To = row.Control.Key, Hold = row.Control.Hold });
            Chord.Validate(candidate);
            return candidate;
        }
    }
}
