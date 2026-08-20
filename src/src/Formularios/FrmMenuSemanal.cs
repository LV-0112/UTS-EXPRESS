using System.Globalization;
using UTSExpress_Principal.Datos;
using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Formularios;

public sealed class FrmMenuSemanal : Form
{
    private readonly MenuSemanalRepository _menuRepository = new();
    private readonly PedidoRepository _pedidoRepository = new();
    private readonly MetodoPagoRepository _metodoPagoRepository = new();
    private readonly List<CarritoItem> _carrito = [];
    private readonly CultureInfo _culturaMexico = CultureInfo.GetCultureInfo("es-MX");

    private readonly FlowLayoutPanel _productos = new();
    private readonly FlowLayoutPanel _pedido = new();
    private readonly Label _lblSubtotal = new();
    private readonly Label _lblTotal = new();
    private readonly Label _lblMetodo = new();
    private readonly Button[] _botonesDias;
    private TipoMetodoPago _metodoPago = TipoMetodoPago.Ninguno;
    private string _diaActual = "Lunes";

    public FrmMenuSemanal()
    {
        Text = "UTS Express - Menú semanal";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1120, 700);
        Size = new Size(1180, 760);
        BackColor = Color.White;

        Panel encabezado = CrearEncabezado();
        Controls.Add(encabezado);

        FlowLayoutPanel dias = new()
        {
            Location = new Point(28, 150),
            Size = new Size(700, 48),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.White
        };

        string[] nombresDias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes"];
        List<Button> botones = [];
        foreach (string dia in nombresDias)
        {
            Button boton = new()
            {
                Text = dia,
                Size = new Size(122, 36),
                Margin = new Padding(0, 0, 12, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = dia
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.Click += async (_, _) => await SeleccionarDiaAsync(boton);
            botones.Add(boton);
            dias.Controls.Add(boton);
        }
        _botonesDias = [.. botones];
        Controls.Add(dias);

        GroupBox grupoProductos = new()
        {
            Text = "Productos del día",
            Location = new Point(28, 210),
            Size = new Size(720, 480),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        _productos.Dock = DockStyle.Fill;
        _productos.AutoScroll = true;
        _productos.WrapContents = true;
        _productos.Padding = new Padding(12);
        _productos.BackColor = Color.White;
        grupoProductos.Controls.Add(_productos);
        Controls.Add(grupoProductos);

        GroupBox grupoPedido = new()
        {
            Text = "Tu pedido",
            Location = new Point(770, 130),
            Size = new Size(360, 560),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        _pedido.Location = new Point(15, 32);
        _pedido.Size = new Size(330, 305);
        _pedido.AutoScroll = true;
        _pedido.FlowDirection = FlowDirection.TopDown;
        _pedido.WrapContents = false;
        _pedido.BackColor = Color.White;
        grupoPedido.Controls.Add(_pedido);

        Label subtotalTitulo = CrearEtiqueta("Subtotal:", 18, 355, 120);
        _lblSubtotal.Text = "$0.00";
        _lblSubtotal.Location = new Point(230, 355);
        _lblSubtotal.Size = new Size(100, 24);
        _lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
        grupoPedido.Controls.Add(subtotalTitulo);
        grupoPedido.Controls.Add(_lblSubtotal);

        Label totalTitulo = CrearEtiqueta("Total:", 18, 385, 120, true);
        _lblTotal.Text = "$0.00";
        _lblTotal.Location = new Point(230, 385);
        _lblTotal.Size = new Size(100, 24);
        _lblTotal.TextAlign = ContentAlignment.MiddleRight;
        _lblTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grupoPedido.Controls.Add(totalTitulo);
        grupoPedido.Controls.Add(_lblTotal);

        _lblMetodo.Text = "Método: no seleccionado";
        _lblMetodo.Location = new Point(18, 418);
        _lblMetodo.Size = new Size(310, 24);
        _lblMetodo.ForeColor = Color.DimGray;
        grupoPedido.Controls.Add(_lblMetodo);

        Button btnMetodo = CrearBotonAccion("Método de pago", new Point(18, 452), Color.Gold, Color.Black);
        btnMetodo.Click += BtnMetodoPago_Click;
        grupoPedido.Controls.Add(btnMetodo);

        Button btnRealizar = CrearBotonAccion("Realizar pedido", new Point(178, 452), Color.Firebrick, Color.White);
        btnRealizar.Click += BtnRealizarPedido_Click;
        grupoPedido.Controls.Add(btnRealizar);

        Button btnCancelar = new()
        {
            Text = "Cancelar pedido",
            Location = new Point(98, 505),
            Size = new Size(160, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnCancelar.Click += (_, _) => LimpiarPedido();
        grupoPedido.Controls.Add(btnCancelar);
        Controls.Add(grupoPedido);

        Shown += async (_, _) => await CargarDiaAsync(_diaActual);
    }

    private Panel CrearEncabezado()
    {
        Panel panel = new()
        {
            Location = new Point(28, 20),
            Size = new Size(720, 115),
            BackColor = Color.White
        };

        string logoPath = Path.Combine(AppContext.BaseDirectory, "ImagenesMenuSemanal", "logo.png");
        PictureBox logo = new()
        {
            Location = new Point(0, 0),
            Size = new Size(210, 105),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        if (File.Exists(logoPath))
        {
            using Image temporal = Image.FromFile(logoPath);
            logo.Image = new Bitmap(temporal);
        }

        Label titulo = new()
        {
            Text = "Menú semanal",
            Location = new Point(235, 8),
            Size = new Size(380, 42),
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Color.Firebrick
        };
        Label subtitulo = new()
        {
            Text = "Tu menú, tu semana, tu energía.",
            Location = new Point(238, 52),
            Size = new Size(410, 30),
            Font = new Font("Segoe UI", 12F)
        };
        Label descripcion = new()
        {
            Text = "Planifica tus comidas y disfruta cada día.",
            Location = new Point(238, 81),
            Size = new Size(450, 28),
            Font = new Font("Segoe UI", 10F)
        };

        panel.Controls.AddRange([logo, titulo, subtitulo, descripcion]);
        return panel;
    }

    private static Label CrearEtiqueta(string texto, int x, int y, int ancho, bool negrita = false)
    {
        return new Label
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, 24),
            Font = new Font("Segoe UI", 9F, negrita ? FontStyle.Bold : FontStyle.Regular)
        };
    }

    private static Button CrearBotonAccion(string texto, Point ubicacion, Color fondo, Color textoColor)
    {
        Button boton = new()
        {
            Text = texto,
            Location = ubicacion,
            Size = new Size(150, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = fondo,
            ForeColor = textoColor,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };
        boton.FlatAppearance.BorderSize = 0;
        return boton;
    }

    private async Task SeleccionarDiaAsync(Button boton)
    {
        _diaActual = boton.Tag?.ToString() ?? "Lunes";
        foreach (Button item in _botonesDias)
        {
            item.BackColor = item == boton ? Color.Maroon : Color.Firebrick;
        }
        await CargarDiaAsync(_diaActual);
    }

    private async Task CargarDiaAsync(string dia)
    {
        try
        {
            _productos.Controls.Clear();
            List<Producto> productos = await _menuRepository.ObtenerPorDiaAsync(dia);

            if (productos.Count == 0)
            {
                _productos.Controls.Add(new Label
                {
                    Text = $"No hay productos registrados para {dia}.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F)
                });
                return;
            }

            foreach (Producto producto in productos)
            {
                _productos.Controls.Add(CrearTarjeta(producto));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo cargar el menú semanal.\n\n" + ex.Message,
                "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Control CrearTarjeta(Producto producto)
    {
        Panel tarjeta = new()
        {
            Size = new Size(205, 300),
            Margin = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

        PictureBox imagen = new()
        {
            Location = new Point(12, 12),
            Size = new Size(178, 115),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Gainsboro,
            Image = CargarImagenSemanal(producto.Imagen)
        };

        Label nombre = new()
        {
            Text = producto.Nombre,
            Location = new Point(10, 138),
            Size = new Size(182, 28),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Label descripcion = new()
        {
            Text = producto.Descripcion,
            Location = new Point(10, 170),
            Size = new Size(182, 44),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.TopCenter
        };
        Label precio = new()
        {
            Text = producto.Precio.ToString("C2", _culturaMexico),
            Location = new Point(12, 225),
            Size = new Size(85, 28),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.Firebrick
        };
        Button agregar = new()
        {
            Text = "Agregar",
            Location = new Point(105, 220),
            Size = new Size(85, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Gold,
            Cursor = Cursors.Hand,
            Enabled = producto.Stock > 0
        };
        agregar.FlatAppearance.BorderSize = 0;
        agregar.Click += (_, _) => AgregarProducto(producto);

        tarjeta.Controls.AddRange([imagen, nombre, descripcion, precio, agregar]);
        return tarjeta;
    }

    private static Image? CargarImagenSemanal(string archivo)
    {
        if (string.IsNullOrWhiteSpace(archivo))
            return null;

        string ruta = Path.Combine(AppContext.BaseDirectory, "ImagenesMenuSemanal", archivo);
        if (!File.Exists(ruta))
            return null;

        using Image temporal = Image.FromFile(ruta);
        return new Bitmap(temporal);
    }

    private void AgregarProducto(Producto producto)
    {
        CarritoItem? existente = _carrito.FirstOrDefault(x => x.Producto.IdProducto == producto.IdProducto);
        int cantidadActual = existente?.Cantidad ?? 0;
        if (cantidadActual >= producto.Stock)
        {
            MessageBox.Show($"Solo hay {producto.Stock} unidades disponibles.", "Inventario");
            return;
        }

        if (existente is null)
        {
            _carrito.Add(new CarritoItem { Producto = producto, Cantidad = 1 });
        }
        else
        {
            existente.Cantidad++;
        }
        RenderizarPedido();
    }

    private void RenderizarPedido()
    {
        _pedido.Controls.Clear();
        foreach (CarritoItem item in _carrito)
        {
            Panel fila = new() { Size = new Size(300, 42), Margin = new Padding(2) };
            Label nombre = new()
            {
                Text = $"{item.Producto.Nombre} x{item.Cantidad}",
                Location = new Point(2, 8),
                Size = new Size(195, 26)
            };
            Label total = new()
            {
                Text = item.Subtotal.ToString("C2", _culturaMexico),
                Location = new Point(200, 8),
                Size = new Size(95, 26),
                TextAlign = ContentAlignment.MiddleRight
            };
            fila.Controls.AddRange([nombre, total]);
            _pedido.Controls.Add(fila);
        }

        decimal totalPedido = _carrito.Sum(x => x.Subtotal);
        _lblSubtotal.Text = totalPedido.ToString("C2", _culturaMexico);
        _lblTotal.Text = totalPedido.ToString("C2", _culturaMexico);
    }

    private void BtnMetodoPago_Click(object? sender, EventArgs e)
    {
        using FrmMetodoPago ventana = new(_metodoPago);
        if (ventana.ShowDialog(this) == DialogResult.OK)
        {
            _metodoPago = ventana.MetodoSeleccionado;
            _lblMetodo.Text = "Método: " + (_metodoPago == TipoMetodoPago.Efectivo ? "Efectivo" : "Tarjeta");
        }
    }

    private async void BtnRealizarPedido_Click(object? sender, EventArgs e)
    {
        if (_carrito.Count == 0)
        {
            MessageBox.Show("Agrega al menos un producto.", "Pedido vacío");
            return;
        }
        if (_metodoPago == TipoMetodoPago.Ninguno)
        {
            MessageBox.Show("Selecciona primero un método de pago.", "Método de pago");
            return;
        }

        try
        {
            int idMetodo = await _metodoPagoRepository.ObtenerIdAsync(_metodoPago);
            int idPedido = await _pedidoRepository.CrearPedidoAsync(_carrito, idMetodo);
            MessageBox.Show($"Pedido realizado correctamente.\nNúmero de pedido: {idPedido}", "Pedido");
            LimpiarPedido();
            await CargarDiaAsync(_diaActual);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo realizar el pedido.\n\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarPedido()
    {
        _carrito.Clear();
        _metodoPago = TipoMetodoPago.Ninguno;
        _lblMetodo.Text = "Método: no seleccionado";
        RenderizarPedido();
    }
}
