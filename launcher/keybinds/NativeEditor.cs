using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LocalL2Keys
{
    internal static class NativeProgram
    {
        [STAThread] private static void Main(string[] args)
        {
            // Used by the local migration/check command, without opening a window.
            if (args.Length == 2 && args[0] == "--import-profile") { new NativeClientBindings(AppDomain.CurrentDomain.BaseDirectory).ImportProfile(args[1]); return; }
            if (args.Length == 1 && args[0] == "--sync-labels") { new NativeClientBindings(AppDomain.CurrentDomain.BaseDirectory).Apply(new NativeAssignment[0]); return; }
            bool created;
            using (var mutex = new Mutex(true, "LocalL2KeybindEditor", out created))
            {
                if (!created) { MessageBox.Show("Keybind Editor is already open."); return; }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                try {
                    string home=AppDomain.CurrentDomain.BaseDirectory, client=home;
                    if(args.Length==2 && args[0]=="--client-dir")client=args[1];
                    else if(File.Exists(Path.Combine(home,"launcher.json"))){
                        var config=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<System.Collections.Generic.Dictionary<string,object>>(File.ReadAllText(Path.Combine(home,"launcher.json")));
                        object selected;if(config.TryGetValue("ClientDirectory",out selected) && !string.IsNullOrWhiteSpace(selected as string))client=(string)selected;
                    }
                    Application.Run(new NativeEditor(Path.Combine(Path.GetFullPath(client),"system")));
                }
                catch (Exception error) { MessageBox.Show(error.Message, "Lineage II Keybind Editor"); }
            }
        }
    }
    internal sealed class NativeCapture : Form
    {
        private readonly Func<string, string> save;
        private readonly Label prompt;
        public NativeCapture(GameControl control, Func<string, string> assign)
        {
            save = assign; Text = "Change binding"; ClientSize = new Size(560, 255); Font = new Font("Segoe UI", 11);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(new Label { Text = control.Label, Bounds = new Rectangle(24, 20, 512, 35), Font = new Font("Segoe UI Semibold", 15) });
            prompt = new Label { Text = "Press the key you want to use", Bounds = new Rectangle(24, 68, 512, 104), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(234, 243, 251) }; Controls.Add(prompt);
            Controls.Add(new Label { Text = "Saves directly to the client. Any conflicting key is reassigned.\nEsc cancels. Restart the game after changing controls.", Bounds = new Rectangle(24, 188, 512, 48), Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleCenter });
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            int key = (int)(keyData & Keys.KeyCode);
            if (key == (int)Keys.Escape) { DialogResult = DialogResult.Cancel; return true; }
            if (Chord.IsModifier(key)) return true;
            int mods = ((keyData & Keys.Control) != 0 ? 1 : 0) | ((keyData & Keys.Alt) != 0 ? 2 : 0) | ((keyData & Keys.Shift) != 0 ? 4 : 0);
            string error = save(new Chord(key, mods).ToString());
            if (error == null) DialogResult = DialogResult.OK;
            else { prompt.Text = error + "\nPress another key, or Esc to cancel."; prompt.ForeColor = Color.FromArgb(151, 49, 36); }
            return true;
        }
    }
    internal sealed class NativeEditor : Form
    {
        private readonly NativeClientBindings store;
        private readonly DataGridView grid = new DataGridView();
        private readonly ComboBox category = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox search = new TextBox();
        private readonly Label count = new Label(), status = new Label(), detail = new Label(), chat = new Label();
        private readonly Font bindingFont;
        public NativeEditor(string clientSystem)
        {
            Text = "Lineage II — Keybind Editor"; ClientSize = new Size(1040, 760); MinimumSize = new Size(940, 660);
            Font = new Font("Segoe UI", 10); bindingFont = new Font(Font, FontStyle.Bold); StartPosition = FormStartPosition.CenterScreen; BackColor = Color.FromArgb(245, 247, 250);
            store = new NativeClientBindings(AppDomain.CurrentDomain.BaseDirectory,clientSystem);
            Controls.Add(new Label { Text = "Your controls", Font = new Font("Segoe UI Semibold", 23), AutoSize = true, Location = new Point(24, 18) });
            Controls.Add(new Label { Text = "Double-click a binding and press a key. Saved in the client — close this editor while playing.", AutoSize = true, Location = new Point(27, 66) });
            Controls.Add(new Label { Text = "Show", Location = new Point(27, 117), AutoSize = true });
            category.SetBounds(77, 113, 200, 30); Controls.Add(category);
            category.Items.Add("All controls"); foreach (string group in store.Controls.Select(c => c.Category).Distinct()) category.Items.Add(group); category.SelectedIndex = 0;
            Controls.Add(new Label { Text = "Search", Location = new Point(301, 117), AutoSize = true });
            search.SetBounds(360, 113, 330, 30); Controls.Add(search);
            count.SetBounds(710, 117, 303, 28); count.Anchor = AnchorStyles.Top | AnchorStyles.Right; count.TextAlign = ContentAlignment.TopRight; count.ForeColor = Color.DimGray; Controls.Add(count);
            grid.SetBounds(27, 161, 986, 430); grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.FixedSingle; grid.RowHeadersVisible = false; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.ReadOnly = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.ColumnHeadersHeight = 36; grid.RowTemplate.Height = 33;
            grid.Columns.Add("category", "Group"); grid.Columns[0].FillWeight = 105;
            grid.Columns.Add("action", "Action"); grid.Columns[1].FillWeight = 210;
            grid.Columns.Add("binding", "Binding · double-click"); grid.Columns[2].FillWeight = 130; grid.Columns[2].DefaultCellStyle.Font = bindingFont;
            grid.Columns.Add("status", "Status"); grid.Columns[3].FillWeight = 115;
            foreach (DataGridViewColumn column in grid.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
            Controls.Add(grid);
            grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) Edit((GameControl)grid.Rows[e.RowIndex].Tag); };
            grid.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F2) { e.Handled = true; e.SuppressKeyPress = true; Edit(Selected()); } };
            grid.SelectionChanged += delegate { ShowDetail(); };
            category.SelectedIndexChanged += delegate { Fill(); }; search.TextChanged += delegate { Fill(); };
            detail.SetBounds(27, 602, 986, 44); detail.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; detail.ForeColor = Color.FromArgb(80, 90, 104); Controls.Add(detail);
            var refresh = new Button { Text = "Refresh from client", Bounds = new Rectangle(27, 657, 170, 34), Anchor = AnchorStyles.Bottom | AnchorStyles.Left }; Controls.Add(refresh);
            refresh.Click += delegate { try { store.Reload(); Fill(); } catch (Exception error) { MessageBox.Show(this, error.Message); } };
            chat.SetBounds(220, 665, 793, 26); chat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; Controls.Add(chat);
            status.SetBounds(27, 718, 986, 28); status.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; status.Text = "Client controls loaded. Restart the game after editing. Every save creates a backup."; Controls.Add(status);
            Fill(); FormClosed += delegate { bindingFont.Dispose(); };
        }
        private GameControl Selected() { return grid.SelectedRows.Count == 0 ? null : grid.SelectedRows[0].Tag as GameControl; }
        private static string StableId(GameControl control) { return control.KeyOffset >= 0 ? "ui:" + control.KeyIndex : control.Id; }
        private void ShowDetail()
        {
            var selected = Selected();
            detail.Text = selected == null ? "Choose a binding to change. Hotbar keys also update their Ctrl and Shift versions." : !selected.Editable ? selected.Hint :
                (selected.KeyOffset >= 0 ? "Saved directly in interface.xdat. " : "Saved directly in user.ini. Movement and camera bindings accept a single key. ") +
                "The game handles this binding without the editor running.";
        }
        private void Fill()
        {
            var selected = Selected(); string id = selected == null ? null : StableId(selected); int scroll = grid.FirstDisplayedScrollingRowIndex;
            string query = search.Text.Trim(), group = (string)category.SelectedItem;
            grid.Rows.Clear();
            foreach (var control in store.Controls)
            {
                if (group != "All controls" && group != control.Category) continue;
                if (query.Length > 0 && (control.Category + " " + control.Label + " " + Pretty(control.Key)).IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
                int row = grid.Rows.Add(control.Category, control.Label, Pretty(control.Key), !control.Editable ? "Reference only" : string.IsNullOrEmpty(control.Key) ? "Unassigned" : "Saved in client"); grid.Rows[row].Tag = control;
                grid.Rows[row].Cells[2].Style.ForeColor = control.Editable ? Color.FromArgb(27, 95, 151) : Color.DimGray;
                grid.Rows[row].Cells[2].Style.BackColor = control.Editable ? Color.FromArgb(237, 246, 253) : Color.FromArgb(245, 245, 245);
                foreach (DataGridViewCell cell in grid.Rows[row].Cells) cell.ToolTipText = control.Hint;
            }
            grid.ClearSelection();
            var previous = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => StableId((GameControl)r.Tag) == id);
            if (previous != null) { grid.CurrentCell = previous.Cells[2]; previous.Selected = true; }
            if (scroll >= 0 && scroll < grid.Rows.Count) grid.FirstDisplayedScrollingRowIndex = scroll;
            count.Text = grid.Rows.Count + " of " + store.Controls.Count + " controls";
            bool? enter = store.EnterChat(); chat.Text = "Enter Chat: " + (enter.HasValue ? enter.Value ? "ON" : "OFF" : "unknown") + " · Chat focus is handled by the client.";
            ShowDetail();
        }
        private void Edit(GameControl control)
        {
            if (control == null || !control.Editable) return;
            using (var dialog = new NativeCapture(control, delegate(string key) {
                try { store.Apply(new[] { new NativeAssignment { Control = control, Key = key } }); Fill(); status.Text = "Bindings and in-game labels saved. Restart the game to load them. You can close the editor."; status.ForeColor = Color.FromArgb(27, 115, 71); return null; }
                catch (Exception error) { return error.Message; }
            })) dialog.ShowDialog(this);
        }
        internal static string Pretty(string text)
        {
            if (string.IsNullOrEmpty(text)) return "—";
            try
            {
                Chord chord = Chord.Parse(text); string key = ((Keys)chord.Key).ToString();
                if (chord.Key >= 48 && chord.Key <= 57) key = ((char)chord.Key).ToString();
                int[] codes = {187,189,219,221,186,222,188,190,191,220,192,33,34,44,13,27,109,107,106,111};
                string[] names = {"=","-","[","]",";","'",",",".","/","\\","`","Page Up","Page Down","Print Screen","Enter","Esc","Numpad -","Numpad +","Numpad *","Numpad /"};
                int index = Array.IndexOf(codes, chord.Key); if (index >= 0) key = names[index];
                return ((chord.Mods & 1) != 0 ? "Ctrl+" : "") + ((chord.Mods & 2) != 0 ? "Alt+" : "") + ((chord.Mods & 4) != 0 ? "Shift+" : "") + key;
            }
            catch (ArgumentException) { return text; }
        }
    }
}
