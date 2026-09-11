using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace LocalL2Keys
{
    public sealed class Binding
    {
        public string Label { get; set; }
        public string ControlId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Hold { get; set; }
        public bool Enabled { get; set; }
        public Binding() { Enabled = true; Label = "Shortcut"; }
    }

    public sealed class Profile
    {
        public int Version { get; set; }
        public bool EnterChat { get; set; }
        public List<Binding> Bindings { get; set; }
        public Profile() { Version = 1; Bindings = new List<Binding>(); }
    }

    public struct Chord
    {
        public int Key;
        public int Mods; // Ctrl=1, Alt=2, Shift=4
        public Chord(int key, int mods) { Key = key; Mods = mods; }
        public static bool IsModifier(int k) { return k == 16 || k == 17 || k == 18 || (k >= 160 && k <= 165) || k == 91 || k == 92; }
        public static Chord Parse(string text)
        {
            int mods = 0, key = 0;
            foreach (string part in (text ?? "").Split('+'))
            {
                string p = part.Trim();
                if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)) mods |= 1;
                else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase)) mods |= 2;
                else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase)) mods |= 4;
                else
                {
                    Keys parsed;
                    if (key != 0 || !Enum.TryParse<Keys>(p, true, out parsed)) throw new ArgumentException("Choose a key or a Ctrl / Alt / Shift combination.");
                    key = (int)parsed;
                }
            }
            if (key <= 0 || key > 254 || IsModifier(key)) throw new ArgumentException("A binding needs a regular key, not just a modifier.");
            return new Chord(key, mods);
        }
        public override string ToString()
        {
            string name = ((Keys)Key).ToString();
            return ((Mods & 1) != 0 ? "Ctrl+" : "") + ((Mods & 2) != 0 ? "Alt+" : "") + ((Mods & 4) != 0 ? "Shift+" : "") + name;
        }
        public static void Validate(Profile profile)
        {
            if (profile == null || profile.Version != 1 || profile.Bindings == null || profile.Bindings.Count > 512) throw new ArgumentException("This profile is invalid or uses an unsupported version.");
            var used = new HashSet<string>();
            foreach (Binding b in profile.Bindings)
            {
                if (b == null) throw new ArgumentException("A profile contains an empty binding.");
                Chord a = Parse(b.From), z = Parse(b.To);
                if (a.Key == 13 || a.Key == 27 || (a.Key == 123 && a.Mods == 5)) throw new ArgumentException("Enter, Escape, and Ctrl+Shift+F12 are reserved for typing and pausing.");
                foreach (Chord c in new[] { a, z })
                    if ((c.Key == 9 && (c.Mods & 2) != 0) || (c.Key == 115 && (c.Mods & 2) != 0) || (c.Key == 46 && (c.Mods & 3) == 3) || (c.Key == 27 && (c.Mods & 1) != 0))
                        throw new ArgumentException("That combination is reserved by Windows. Choose a game control.");
                if (a.ToString() == z.ToString()) throw new ArgumentException("Choose a different game key; this binding would do nothing.");
                if (b.Hold && (a.Mods != 0 || z.Mods != 0)) throw new ArgumentException("Hold mode uses single keys. Use Tap for combinations.");
                if (b.Enabled && !used.Add(a.ToString())) throw new ArgumentException("Two enabled bindings use " + a + ".");
            }
        }
    }

}
