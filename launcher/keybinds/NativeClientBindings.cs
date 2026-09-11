using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace LocalL2Keys
{
    public sealed class NativeAssignment
    {
        public GameControl Control;
        public string Key;
    }

    // Configuration editor only: no keyboard hook, input injection or resident service.
    public sealed class NativeClientBindings
    {
        private readonly string root, xdatPath, iniPath, stringsPath, optionsPath, backupRoot;
        private byte[] xdat, encryptedIni, decodedIni, encryptedStrings, decodedStrings;
        public List<GameControl> Controls { get; private set; }
        public string LastBackup { get; private set; }
        public NativeClientBindings(string directory, string clientSystem = null)
        {
            root = directory; string system = clientSystem ?? Path.Combine(root, "client", "system"); xdatPath = Path.Combine(system, "interface.xdat"); iniPath = Path.Combine(system, "user.ini"); stringsPath = Path.Combine(system,"sysstring-e.dat"); optionsPath = Path.Combine(system,"Option.ini"); backupRoot = clientSystem == null ? Path.Combine(root, "backups", "client-bindings") : Path.Combine(clientSystem, "backups"); Reload();
        }
        private byte[] ConvertIni(byte[] input, bool encode)
        {
            return encode ? PortableCrypt.Encode(input) : PortableCrypt.Decode(input);
        }
        public static void ValidateIni(byte[] value)
        {
            byte[] header = Encoding.Unicode.GetBytes("Lineage2Ver413");
            if (value.Length < 176 || !value.Take(header.Length).SequenceEqual(header) || (value.Length - 48) % 128 != 0) throw new InvalidDataException("The encrypted INI header, blocks or footer are invalid.");
            uint crc = 0xffffffff;
            for (int i = 0; i < value.Length - 20; i++)
            {
                crc ^= value[i]; for (int bit = 0; bit < 8; bit++) crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
            }
            if (~crc != BitConverter.ToUInt32(value, value.Length - 8)) throw new InvalidDataException("The encrypted INI checksum is invalid.");
        }
        public void Reload()
        {
            byte[] nextXdat = File.ReadAllBytes(xdatPath), nextIni = File.ReadAllBytes(iniPath);
            ValidateIni(nextIni);
            byte[] nextDecoded = ConvertIni(nextIni, false);
            byte[] nextStrings = File.ReadAllBytes(stringsPath); ValidateIni(nextStrings);
            byte[] nextDecodedStrings = ConvertIni(nextStrings,false);
            var controls = ControlCatalog.ReadShortcuts(xdatPath);
            controls.AddRange(ControlCatalog.ReadInput(Encoding.GetEncoding(28591).GetString(nextDecoded)));
            string[] order = { "Hotbars", "Movement", "Interface", "Hotbar pages", "Hotbar modifiers", "Chat and targeting", "Camera and extras", "Mouse", "Gamepad" };
            Controls = controls.OrderBy(c => Array.IndexOf(order, c.Category)).ToList();
            xdat = nextXdat; encryptedIni = nextIni; decodedIni = nextDecoded;
            encryptedStrings = nextStrings; decodedStrings = nextDecodedStrings;
        }
        public bool? EnterChat()
        {
            try
            {
                string text = File.ReadAllText(optionsPath);
                var match = Regex.Match(text, @"(?im)^EnterChatting\s*=\s*(True|False)\s*$");
                return match.Success ? (bool?)bool.Parse(match.Groups[1].Value) : null;
            }
            catch (IOException) { return null; }
        }
        private static Chord ReadKey(byte[] bytes, int offset)
        {
            int mods = 0;
            for (int i = 1; i <= 2; i++) { int key = BitConverter.ToInt32(bytes, offset + i * 4); if (key == 17) mods |= 1; if (key == 18) mods |= 2; if (key == 16) mods |= 4; }
            return new Chord(BitConverter.ToInt32(bytes, offset), mods);
        }
        private static void SetKey(byte[] bytes, int offset, Chord chord)
        {
            var values = new List<int> { chord.Key };
            if ((chord.Mods & 1) != 0) values.Add(17); if ((chord.Mods & 2) != 0) values.Add(18); if ((chord.Mods & 4) != 0) values.Add(16);
            if (values.Count > 3) throw new ArgumentException("This client supports at most two modifiers per shortcut.");
            while (values.Count < 3) values.Add(0);
            for (int i = 0; i < 3; i++) Array.Copy(BitConverter.GetBytes(values[i]), 0, bytes, offset + i * 4, 4);
        }
        private static void ValidateKey(Chord chord)
        {
            if (chord.Key < 8 || chord.Key == 13 || chord.Key == 27) throw new ArgumentException("Enter and Escape stay available for chat and canceling. Choose another key.");
            if (chord.Mods == 7) throw new ArgumentException("Use at most two modifiers with a key.");
            if ((chord.Key == 9 && (chord.Mods & 2) != 0) || (chord.Key == 115 && (chord.Mods & 2) != 0) || (chord.Key == 46 && (chord.Mods & 3) == 3)) throw new ArgumentException("That shortcut is reserved by Windows.");
        }
        private static string IniName(int key)
        {
            var names = new Dictionary<int, string> { {33,"PageUp"}, {34,"PageDown"}, {106,"GreyStar"}, {107,"GreyPlus"}, {109,"GreyMinus"}, {110,"NumPadPeriod"}, {111,"GreySlash"}, {44,"PrintScrn"}, {186,"Semicolon"}, {187,"Equals"}, {188,"Comma"}, {189,"Minus"}, {190,"Period"}, {191,"Slash"}, {192,"Tilde"}, {219,"LeftBracket"}, {220,"Backslash"}, {221,"RightBracket"}, {222,"SingleQuote"} };
            string name; if (names.TryGetValue(key, out name)) return name;
            if (key >= 48 && key <= 57) return ((char)key).ToString();
            return ((System.Windows.Forms.Keys)key).ToString();
        }
        private static string SetInput(string text, string key, string command, bool add)
        {
            var section = Regex.Match(text, @"(?im)^\[Engine\.Input\][^\r\n]*(?:\r?\n)");
            if (!section.Success) throw new InvalidDataException("The client input section is missing.");
            int start = section.Index + section.Length;
            var next = Regex.Match(text.Substring(start), @"(?m)^\[");
            int end = next.Success ? start + next.Index : text.Length;
            string block = text.Substring(start, end - start);
            var line = new Regex(@"(?im)^" + Regex.Escape(key) + @"[ \t]*=[^\r\n]*");
            bool exists = line.IsMatch(block);
            block = line.Replace(block, key + "=" + command);
            if (!exists && add) block = key + "=" + command + "\r\n" + block;
            return text.Substring(0, start) + block + text.Substring(end);
        }
        private void ClearCollisions(byte[] bytes, ref string input, Chord key, int except)
        {
            foreach (var control in Controls.Where(c => c.KeyOffset >= 0 && c.KeyOffset != except))
            {
                Chord current = ReadKey(bytes, control.KeyOffset);
                if (current.Key == key.Key && current.Mods == key.Mods)
                    Array.Copy(BitConverter.GetBytes(0), 0, bytes, control.KeyOffset, 4); // Keep modifier group for the unbound row.
            }
            if (key.Mods == 0) input = SetInput(input, IniName(key.Key), "", false);
        }
        public void Apply(IEnumerable<NativeAssignment> assignments)
        {
            byte[] updatedXdat = (byte[])xdat.Clone();
            string input = Encoding.GetEncoding(28591).GetString(decodedIni);
            var descriptions = new List<string>();
            foreach (var assignment in assignments)
            {
                GameControl control = assignment.Control;
                if (!Controls.Contains(control) || !control.Editable) throw new ArgumentException("Select an editable keyboard control from the current list.");
                Chord key = Chord.Parse(assignment.Key); ValidateKey(key);
                if (control.KeyOffset >= 0)
                {
                    var family = new List<GameControl> { control };
                    if (control.Category == "Hotbars" && key.Mods == 0)
                        family.AddRange(Controls.Where(c => c.KeyOffset >= 0 && c.Command == control.Command && c.Category == "Hotbar modifiers"));
                    foreach (GameControl member in family)
                    {
                        var memberKey = member == control ? key : new Chord(key.Key, ReadKey(xdat, member.KeyOffset).Mods);
                        ClearCollisions(updatedXdat, ref input, memberKey, member.KeyOffset); SetKey(updatedXdat, member.KeyOffset, memberKey);
                    }
                }
                else
                {
                    if (key.Mods != 0) throw new ArgumentException("Movement and camera controls in user.ini need a single key.");
                    ClearCollisions(updatedXdat, ref input, key, -1);
                    input = SetInput(input, control.InputName, "", false);
                    input = SetInput(input, IniName(key.Key), control.Command, true);
                }
                descriptions.Add(control.Label + ": " + control.Key + " -> " + key);
            }
            byte[] updatedPlain = Encoding.GetEncoding(28591).GetBytes(input), updatedIni = encryptedIni;
            if (!updatedPlain.SequenceEqual(decodedIni))
            {
                updatedIni = ConvertIni(updatedPlain, true); ValidateIni(updatedIni);
                if (!ConvertIni(updatedIni, false).SequenceEqual(updatedPlain)) throw new InvalidDataException("The client settings did not survive encoding verification.");
            }
            updatedXdat = ClientUiLabels.Layout(updatedXdat,File.ReadAllText(optionsPath));
            byte[] updatedPlainStrings = ClientUiLabels.Tooltips(decodedStrings,updatedXdat), updatedStrings = encryptedStrings;
            if (!updatedPlainStrings.SequenceEqual(decodedStrings))
            {
                updatedStrings = ConvertIni(updatedPlainStrings,true); ValidateIni(updatedStrings);
                if (!ConvertIni(updatedStrings,false).SequenceEqual(updatedPlainStrings)) throw new InvalidDataException("The interface tooltips did not survive encoding verification.");
            }
            if (updatedXdat.SequenceEqual(xdat) && updatedIni.SequenceEqual(encryptedIni) && updatedStrings.SequenceEqual(encryptedStrings)) return;
            if (!File.ReadAllBytes(xdatPath).SequenceEqual(xdat) || !File.ReadAllBytes(iniPath).SequenceEqual(encryptedIni) || !File.ReadAllBytes(stringsPath).SequenceEqual(encryptedStrings)) throw new IOException("The client files changed since this list was loaded. Click Refresh and try again.");
            string backup = Path.Combine(backupRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6));
            Directory.CreateDirectory(backup); File.WriteAllBytes(Path.Combine(backup, "interface.xdat"), xdat); File.WriteAllBytes(Path.Combine(backup, "user.ini"), encryptedIni);
            File.WriteAllBytes(Path.Combine(backup,"sysstring-e.dat"),encryptedStrings);
            var manifest = new { changes = descriptions, uiLabels = true, beforeXdat = Hash(xdat), afterXdat = Hash(updatedXdat), beforeIni = Hash(encryptedIni), afterIni = Hash(updatedIni), beforeStrings = Hash(encryptedStrings), afterStrings = Hash(updatedStrings) };
            File.WriteAllText(Path.Combine(backup, "changes.json"), new JavaScriptSerializer().Serialize(manifest));
            bool wroteXdat = false, wroteIni = false;
            try
            {
                if (!updatedXdat.SequenceEqual(xdat)) { Replace(xdatPath, updatedXdat); wroteXdat = true; }
                if (!updatedIni.SequenceEqual(encryptedIni)) { Replace(iniPath, updatedIni); wroteIni = true; }
                if (!updatedStrings.SequenceEqual(encryptedStrings)) Replace(stringsPath,updatedStrings);
            }
            catch
            {
                if (wroteIni) Replace(iniPath,encryptedIni);
                if (wroteXdat) Replace(xdatPath, xdat);
                throw;
            }
            LastBackup = backup; Reload();
        }
        private static void Replace(string path, byte[] bytes)
        {
            string temp = path + ".bindings-tmp";
            try { File.WriteAllBytes(temp, bytes); File.Replace(temp, path, null); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        private static string Hash(byte[] bytes) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        public void ImportProfile(string path)
        {
            Profile profile = new JavaScriptSerializer().Deserialize<Profile>(File.ReadAllText(path)); Chord.Validate(profile);
            var changes = new List<NativeAssignment>();
            foreach (Binding binding in profile.Bindings.Where(b => b.Enabled))
            {
                GameControl control = Controls.FirstOrDefault(c => c.Id == binding.ControlId) ?? Controls.FirstOrDefault(c => c.Editable && c.Key == binding.To);
                if (control == null) throw new ArgumentException("Could not locate the client action for " + binding.Label + ". No changes were saved.");
                changes.Add(new NativeAssignment { Control = control, Key = binding.From });
            }
            Apply(changes);
        }
    }
}
