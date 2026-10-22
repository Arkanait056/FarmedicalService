namespace FarmedicalService.UI
{
    /// <summary>Pide un texto (por ejemplo una contraseña nueva o un motivo).</summary>
    public sealed class InputDialog : DialogForm
    {
        private readonly TextBox _txt;

        private InputDialog(string title, string prompt, bool password, string textoBoton) : base(title, 440, 250)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lbl = Ui.Lbl(prompt, Theme.Body, Theme.Text);
            lbl.MaximumSize = new Size(Ui.S(380), 0);
            lbl.Margin = new Padding(0, 0, 0, Ui.S(14));

            _txt = Ui.Txt("", password);
            var marco = new FieldFrame(_txt) { Dock = DockStyle.Fill, Margin = Padding.Empty, PasswordToggle = password };

            layout.Controls.Add(lbl, 0, 0);
            layout.Controls.Add(marco, 0, 1);
            Content.Controls.Add(layout);

            var ok = new AppButton(textoBoton, ButtonKind.Primary) { DialogResult = DialogResult.OK };
            var cancel = new AppButton("Cancelar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(ok);
            AddFooterButton(cancel);
            AcceptButton = ok;
            CancelButton = cancel;

            Shown += (s, e) => _txt.Focus();
        }

        /// <summary>Devuelve el texto escrito, o null si se canceló.</summary>
        public static string? Ask(string title, string prompt, bool password = false, string textoBoton = "Aceptar")
        {
            using var dlg = new InputDialog(title, prompt, password, textoBoton);
            return dlg.ShowDialog() == DialogResult.OK ? dlg._txt.Text : null;
        }
    }

    /// <summary>Pide elegir una opción de una lista.</summary>
    public sealed class ChoiceDialog : DialogForm
    {
        private readonly AppCombo _combo;

        private ChoiceDialog(string title, string prompt, IEnumerable<string> opciones, string? actual) : base(title, 440, 250)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lbl = Ui.Lbl(prompt, Theme.Body, Theme.Text);
            lbl.MaximumSize = new Size(Ui.S(380), 0);
            lbl.Margin = new Padding(0, 0, 0, Ui.S(14));

            _combo = Ui.Combo();
            _combo.Dock = DockStyle.Fill;
            _combo.Margin = Padding.Empty;
            foreach (var o in opciones) _combo.Items.Add(o);
            _combo.SelectedItem = actual;
            if (_combo.SelectedIndex < 0 && _combo.Items.Count > 0) _combo.SelectedIndex = 0;

            layout.Controls.Add(lbl, 0, 0);
            layout.Controls.Add(_combo, 0, 1);
            Content.Controls.Add(layout);

            var ok = new AppButton("Guardar", ButtonKind.Primary) { DialogResult = DialogResult.OK };
            var cancel = new AppButton("Cancelar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(ok);
            AddFooterButton(cancel);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        public static string? Ask(string title, string prompt, IEnumerable<string> opciones, string? actual = null)
        {
            using var dlg = new ChoiceDialog(title, prompt, opciones, actual);
            return dlg.ShowDialog() == DialogResult.OK ? dlg._combo.SelectedItem?.ToString() : null;
        }
    }
}
