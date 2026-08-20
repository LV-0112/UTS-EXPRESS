using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Formularios;

public sealed class FrmMetodoPago : Form
{
    public TipoMetodoPago MetodoSeleccionado { get; private set; }
        = TipoMetodoPago.Ninguno;

    public FrmMetodoPago(TipoMetodoPago metodoActual)
    {
        Text = "Método de pago";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(360, 310);
        BackColor = Color.FromArgb(248, 249, 250);

        Label titulo = new()
        {
            Text = "Elige tu método de pago",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 18),
            Size = new Size(320, 42)
        };

        Label instruccion = new()
        {
            Text = "Selecciona una opción para continuar con el pedido.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.DimGray,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(30, 62),
            Size = new Size(300, 38)
        };

        Button btnEfectivo = CrearBoton("Efectivo", new Point(55, 112));
        Button btnTarjeta = CrearBoton("Tarjeta", new Point(55, 174));

        Button btnCancelar = new()
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(115, 245),
            Size = new Size(130, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(33, 37, 41),
            Cursor = Cursors.Hand
        };

        btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(210, 214, 220);

        btnEfectivo.Click += (_, _) => Seleccionar(TipoMetodoPago.Efectivo);
        btnTarjeta.Click += (_, _) => Seleccionar(TipoMetodoPago.Tarjeta);

        if (metodoActual == TipoMetodoPago.Efectivo)
        {
            MarcarSeleccionado(btnEfectivo);
        }
        else if (metodoActual == TipoMetodoPago.Tarjeta)
        {
            MarcarSeleccionado(btnTarjeta);
        }

        Controls.AddRange([titulo, instruccion, btnEfectivo, btnTarjeta, btnCancelar]);

        AcceptButton = metodoActual == TipoMetodoPago.Tarjeta ? btnTarjeta : btnEfectivo;
        CancelButton = btnCancelar;
    }

    private static Button CrearBoton(string texto, Point ubicacion)
    {
        Button boton = new()
        {
            Text = texto,
            Location = ubicacion,
            Size = new Size(250, 48),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        boton.FlatAppearance.BorderColor = Color.FromArgb(192, 0, 0);
        boton.FlatAppearance.BorderSize = 1;
        return boton;
    }

    private static void MarcarSeleccionado(Button boton)
    {
        boton.BackColor = Color.FromArgb(255, 241, 241);
        boton.Text += "  ✓";
    }

    private void Seleccionar(TipoMetodoPago tipo)
    {
        MetodoSeleccionado = tipo;
        DialogResult = DialogResult.OK;
        Close();
    }
}
