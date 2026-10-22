using FarmedicalService.Models;
using FarmedicalService.UI;
using System.Drawing.Printing;

namespace FarmedicalService.Forms
{
    public class FormImpresion : DialogForm
    {
        private RichTextBox rtbFactura = null!;
        private readonly Factura _factura;
        private readonly List<DetalleFactura> _detalles;
        private PrintDocument _printDocument = null!;

        public FormImpresion(Factura factura, List<DetalleFactura> detalles)
            : base("Vista previa de la factura", 460, 700)
        {
            _factura = factura;
            _detalles = detalles;
            InicializarComponentes();
            GenerarVista();
        }

        private void InicializarComponentes()
        {
            Content.BackColor = Theme.Background;
            Content.Padding = Ui.P(28, 24, 28, 20);

            // "Papel" del recibo
            var papel = new Card
            {
                Dock = DockStyle.Fill,
                Radius = 8,
                Padding = Ui.P(20)
            };

            rtbFactura = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                TabStop = false
            };
            papel.Controls.Add(rtbFactura);
            Content.Controls.Add(papel);

            var btnImprimir = new AppButton("Imprimir", ButtonKind.Primary);
            btnImprimir.Click += BtnImprimir_Click;
            var btnCerrar = new AppButton("Cerrar", ButtonKind.Secondary) { DialogResult = DialogResult.Cancel };
            AddFooterButton(btnImprimir);
            AddFooterButton(btnCerrar);
            CancelButton = btnCerrar;

            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void GenerarVista()
        {
            var linea = new string('-', 40);
            var texto = new System.Text.StringBuilder();

            texto.AppendLine("========================================");
            texto.AppendLine(Centrar(FarmaciaConfig.Nombre.ToUpper()));
            texto.AppendLine(Centrar($"RNC {FarmaciaConfig.Rnc}"));
            texto.AppendLine(Centrar(FarmaciaConfig.Direccion));
            texto.AppendLine(Centrar($"Tel. {FarmaciaConfig.Telefono}"));
            texto.AppendLine("========================================");
            texto.AppendLine($"NCF:       {(string.IsNullOrEmpty(_factura.Ncf) ? "—" : _factura.Ncf)}");
            texto.AppendLine($"Factura #: {_factura.NumeroFactura}");
            texto.AppendLine($"Fecha:     {_factura.Fecha:dd/MM/yyyy HH:mm}");
            texto.AppendLine($"Cajero:    {_factura.UsuarioNombre}");
            if (!string.IsNullOrWhiteSpace(_factura.Cliente))
                texto.AppendLine($"Cliente:   {_factura.Cliente}");
            texto.AppendLine($"Pago:      {_factura.MetodoPago}");
            texto.AppendLine(linea);
            texto.AppendLine($"{"Medicamento",-22} {"Cant",4} {"Precio",10}");
            texto.AppendLine(linea);

            foreach (var d in _detalles)
            {
                var nombre = d.ProductoNombre.Length > 20
                    ? d.ProductoNombre.Substring(0, 20)
                    : d.ProductoNombre;
                texto.AppendLine($"{nombre,-22} {d.Cantidad,4} RD${d.Subtotal,8:N2}");
            }

            texto.AppendLine(linea);
            texto.AppendLine($"{"Subtotal:",-28} RD${_factura.Subtotal,8:N2}");
            texto.AppendLine($"{"ITBIS:",-28} RD${_factura.Itbis,8:N2}");
            texto.AppendLine($"{"TOTAL:",-28} RD${_factura.Total,8:N2}");
            texto.AppendLine(linea);
            texto.AppendLine($"{"Pagado:",-28} RD${_factura.MontoPagado,8:N2}");
            texto.AppendLine($"{"Cambio:",-28} RD${_factura.Cambio,8:N2}");
            texto.AppendLine("========================================");
            texto.AppendLine("      Gracias por su compra!            ");
            texto.AppendLine("========================================");

            rtbFactura.Text = texto.ToString();
        }

        private static string Centrar(string texto, int ancho = 40)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            return new string(' ', (ancho - texto.Length) / 2) + texto;
        }

        private void BtnImprimir_Click(object? sender, EventArgs e)
        {
            using var printDialog = new PrintDialog();
            printDialog.Document = _printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                _printDocument.Print();
                MessageBox.Show("Factura enviada a la impresora.", "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null) return;

            using var font = new Font("Courier New", 9);
            float y = 10;
            float lineHeight = font.GetHeight(e.Graphics);

            foreach (var linea in rtbFactura.Text.Split('\n'))
            {
                e.Graphics.DrawString(linea.TrimEnd(), font, Brushes.Black, 10, y);
                y += lineHeight;
            }
        }
    }
}
