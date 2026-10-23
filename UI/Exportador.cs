using System.Globalization;
using System.Text;

namespace FarmedicalService.UI
{
    ///Exporta el contenido visible de una tabla a CSV (se abre directamente en Excel).
    public static class Exportador
    {
        public static void GuardarCsv(DataGridView grid, string nombreSugerido)
        {
            if (grid.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialogo = new SaveFileDialog
            {
                Title = "Exportar a Excel",
                Filter = "Excel (CSV)|*.csv",
                FileName = $"{nombreSugerido}_{DateTime.Today:yyyyMMdd}.csv"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            var separador = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var columnas = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(separador, columnas.Select(c => Escapar(c.HeaderText, separador))));

            foreach (DataGridViewRow fila in grid.Rows)
            {
                sb.AppendLine(string.Join(separador,
                    columnas.Select(c => Escapar(fila.Cells[c.Index].Value?.ToString() ?? "", separador))));
            }

            try
            {
                File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("Archivo exportado correctamente.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"No se pudo guardar el archivo. Verifica que no esté abierto en Excel.\n\n{ex.Message}",
                    "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string Escapar(string valor, string separador)
        {
            if (valor.Contains('"') || valor.Contains(separador) || valor.Contains('\n'))
                return "\"" + valor.Replace("\"", "\"\"") + "\"";
            return valor;
        }
    }
}
