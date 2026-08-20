using System.Globalization;
using UTSExpress_Principal.Datos;
using UTSExpress_Principal.Formularios;
using UTSExpress_Principal.Modelos;
using UTSExpress_Principal.Utilidades;

namespace UTSExpress_Principal;

public partial class FrmPrincipal : Form
{
    private readonly ProductoRepository _productoRepository = new();
    private readonly PedidoRepository _pedidoRepository = new();
    private readonly MetodoPagoRepository _metodoPagoRepository = new();
    private readonly List<CarritoItem> _carrito = [];
    private readonly CultureInfo _culturaMexico = CultureInfo.GetCultureInfo("es-MX");

    private Button[] _botonesCategoria = [];
    private Button? _categoriaActiva;
    private TipoMetodoPago _metodoPagoSeleccionado = TipoMetodoPago.Ninguno;
    private string? _categoriaSeleccionada;
    private bool _cargandoProductos;

    public FrmPrincipal()
    {
        InitializeComponent();
        ConfigurarInterfaz();
        ConectarEventos();
    }

    private void ConfigurarInterfaz()
    {
        Text = "UTS Express - Cafetería universitaria";
        MinimumSize = new Size(1180, 720);

        btnCafes.Text = "Cafés";
        btnBebidas.Text = "Bebidas";
        btnComidas.Text = "Comidas";
        btnSnaks.Text = "Snacks";
        btnCombos.Text = "Combos";
        btnVerMenu.Text = "Ver menú";
        btnIniciarSesion.Text = "Cerrar sesión";

        flowLayoutPanel2.AutoScroll = true;
        flowLayoutPanel2.WrapContents = true;
        flowLayoutPanel2.Padding = new Padding(5);
        flowLayoutPanel2.BackColor = Color.FromArgb(248, 249, 250);

        FlpCarrito.AutoScroll = true;
        FlpCarrito.FlowDirection = FlowDirection.TopDown;
        FlpCarrito.WrapContents = false;
        FlpCarrito.Padding = new Padding(4);

        _botonesCategoria = [btnTodos, btnCafes, btnBebidas, btnComidas, btnSnaks, btnCombos];
        foreach (Button boton in _botonesCategoria)
        {
            boton.Cursor = Cursors.Hand;
            boton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        btnRealizarPedido.Cursor = Cursors.Hand;
        btnMetodoPago.Cursor = Cursors.Hand;
        btnCancelarpedido.Cursor = Cursors.Hand;
        btnIniciarSesion.Cursor = Cursors.Default;
        btnIniciarSesion.TabStop = false;
        btnMiPedido.Cursor = Cursors.Hand;
        btnVerMenu.Cursor = Cursors.Hand;

        ConfigurarEstiloVisual();
        RenderizarCarrito();
        MarcarCategoriaActiva(btnTodos);
    }

    private void ConfigurarEstiloVisual()
    {
        BackColor = EstilosUI.Fondo;
        panel1.BackColor = Color.White;
        EstilosUI.AgregarSombra(panel1, 8);

        btnMetodoPago.BackColor = EstilosUI.Amarillo;
        btnMetodoPago.ForeColor = EstilosUI.Texto;
        btnMetodoPago.Text = "Método de pago";
        btnRealizarPedido.BackColor = EstilosUI.Rojo;
        btnMiPedido.BackColor = EstilosUI.Rojo;

        foreach (Button boton in _botonesCategoria)
        {
            boton.FlatAppearance.BorderSize = 0;
            EstilosUI.Redondear(boton, 12);
        }

        EstilosUI.Redondear(btnIniciarSesion, 10);
        EstilosUI.Redondear(btnMiPedido, 10);
        EstilosUI.Redondear(btnVerMenu, 10);
        EstilosUI.Redondear(btnRealizarPedido, 10);
        EstilosUI.Redondear(btnMetodoPago, 10);
        EstilosUI.Redondear(btnCancelarpedido, 10);

        lblTotalPagar.ForeColor = EstilosUI.Rojo;
        lblTotalPagar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        FlpCarrito.BackColor = Color.White;
    }

    private void ConectarEventos()
    {
        Load += FrmPrincipal_Load;

        btnTodos.Click += async (_, _) => await SeleccionarCategoriaAsync(null, btnTodos);
        btnCafes.Click += async (_, _) => await SeleccionarCategoriaAsync("Cafés", btnCafes);
        btnBebidas.Click += async (_, _) => await SeleccionarCategoriaAsync("Bebidas", btnBebidas);
        btnComidas.Click += async (_, _) => await SeleccionarCategoriaAsync("Comidas", btnComidas);
        btnSnaks.Click += async (_, _) => await SeleccionarCategoriaAsync("Snacks", btnSnaks);
        btnCombos.Click += async (_, _) => await SeleccionarCategoriaAsync("Combos", btnCombos);

        btnMetodoPago.Click += BtnMetodoPago_Click;
        btnRealizarPedido.Click += BtnRealizarPedido_Click;
        btnCancelarpedido.Click += BtnCancelarPedido_Click;
        btnMiPedido.Click += BtnMiPedido_Click;
        btnVerMenu.Click += (_, _) =>
        {
            using FrmMenuSemanal menuSemanal = new();
            menuSemanal.ShowDialog(this);
        };
    }

    private async void FrmPrincipal_Load(object? sender, EventArgs e)
    {
        try
        {
            CambiarEstadoCarga(true, "Preparando productos...");
            await CargarProductosAsync();
        }
        catch (Exception ex)
        {
            MostrarErrorConexion(ex);
        }
        finally
        {
            CambiarEstadoCarga(false);
        }
    }

    private async Task SeleccionarCategoriaAsync(string? categoria, Button botonSeleccionado)
    {
        if (_cargandoProductos)
        {
            return;
        }

        _categoriaSeleccionada = categoria;
        MarcarCategoriaActiva(botonSeleccionado);
        await CargarProductosAsync(categoria);
        flowLayoutPanel2.AutoScrollPosition = Point.Empty;
    }

    private async Task CargarProductosAsync(string? categoria = null)
    {
        try
        {
            CambiarEstadoCarga(true, "Cargando productos...");
            List<Producto> productos = await _productoRepository.ObtenerProductosAsync(categoria);

            LimpiarControles(flowLayoutPanel2);

            if (productos.Count == 0)
            {
                flowLayoutPanel2.Controls.Add(new Label
                {
                    Text = "No hay productos disponibles en esta categoría.",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.DimGray,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(650, 110),
                    Margin = new Padding(20)
                });
            }
            else
            {
                foreach (Producto producto in productos)
                {
                    flowLayoutPanel2.Controls.Add(CrearTarjetaProducto(producto));
                }
            }

            flowLayoutPanel2.Controls.Add(CrearPanelRegresarInicio());
        }
        catch (Exception ex)
        {
            MostrarErrorConexion(ex);
        }
        finally
        {
            CambiarEstadoCarga(false);
        }
    }

    private Control CrearTarjetaProducto(Producto producto)
    {
        Panel contenedor = new()
        {
            Size = new Size(218, 292),
            BackColor = Color.Transparent,
            Margin = new Padding(8),
            Tag = producto
        };

        Panel sombra = new()
        {
            Location = new Point(5, 6),
            Size = new Size(208, 280),
            BackColor = Color.FromArgb(221, 224, 229)
        };

        Panel tarjeta = new()
        {
            Location = new Point(0, 0),
            Size = new Size(208, 280),
            BackColor = Color.White,
            Padding = new Padding(8),
            Tag = producto
        };

        PictureBox imagen = new()
        {
            Location = new Point(14, 12),
            Size = new Size(180, 112),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(250, 250, 250),
            Image = CargarImagenProducto(producto.Imagen, producto.Nombre)
        };

        Label nombre = new()
        {
            Text = producto.Nombre,
            Location = new Point(10, 130),
            Size = new Size(188, 28),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = EstilosUI.Texto,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true
        };

        Label descripcion = new()
        {
            Text = producto.Descripcion,
            Location = new Point(12, 159),
            Size = new Size(184, 40),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.TopCenter,
            AutoEllipsis = true
        };

        Label precio = new()
        {
            Text = producto.Precio.ToString("C2", _culturaMexico),
            Location = new Point(12, 211),
            Size = new Size(92, 27),
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = EstilosUI.Rojo,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label stock = new()
        {
            Text = producto.Stock > 0 ? $"Stock: {producto.Stock}" : "Agotado",
            Location = new Point(12, 241),
            Size = new Size(84, 24),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = producto.Stock > 0 ? Color.DimGray : Color.Firebrick
        };

        Button agregar = new()
        {
            Text = producto.Stock > 0 ? "Agregar" : "Agotado",
            Location = new Point(105, 220),
            Size = new Size(91, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = producto.Stock > 0 ? EstilosUI.Amarillo : Color.LightGray,
            ForeColor = producto.Stock > 0 ? EstilosUI.Texto : Color.DimGray,
            Enabled = producto.Stock > 0,
            Cursor = producto.Stock > 0 ? Cursors.Hand : Cursors.Default,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };
        agregar.FlatAppearance.BorderSize = 0;
        agregar.Click += (_, _) => AgregarAlCarrito(producto);

        tarjeta.Controls.AddRange([imagen, nombre, descripcion, precio, stock, agregar]);
        contenedor.Controls.Add(sombra);
        contenedor.Controls.Add(tarjeta);
        tarjeta.BringToFront();

        EstilosUI.Redondear(sombra, 12);
        EstilosUI.Redondear(tarjeta, 12);
        EstilosUI.Redondear(agregar, 14);
        return contenedor;
    }

    private Control CrearPanelRegresarInicio()
    {
        int ancho = Math.Max(660, flowLayoutPanel2.ClientSize.Width - 35);
        Panel contenedor = new()
        {
            Size = new Size(ancho, 62),
            Margin = new Padding(8, 14, 8, 20),
            BackColor = Color.Transparent
        };

        Button regresar = new()
        {
            Text = "Regresar al inicio",
            Size = new Size(190, 42),
            Location = new Point(10, 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = EstilosUI.Rojo,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        regresar.FlatAppearance.BorderColor = EstilosUI.Rojo;
        EstilosUI.Redondear(regresar, 12);
        regresar.Click += async (_, _) =>
        {
            await SeleccionarCategoriaAsync(null, btnTodos);
            ActiveControl = btnTodos;
        };

        contenedor.Controls.Add(regresar);
        return contenedor;
    }

    private void AgregarAlCarrito(Producto producto)
    {
        CarritoItem? item = _carrito.FirstOrDefault(
            existente => existente.Producto.IdProducto == producto.IdProducto);

        int cantidadActual = item?.Cantidad ?? 0;
        if (cantidadActual >= producto.Stock)
        {
            MessageBox.Show(
                $"Solo hay {producto.Stock} unidades disponibles de {producto.Nombre}.",
                "Inventario insuficiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (item is null)
        {
            _carrito.Add(new CarritoItem
            {
                Producto = producto,
                Cantidad = 1
            });
        }
        else
        {
            item.Cantidad++;
        }

        RenderizarCarrito();
    }

    private void RenderizarCarrito()
    {
        LimpiarControles(FlpCarrito);

        FlpCarrito.Controls.Add(new Label
        {
            Text = "Tu pedido",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Size = new Size(280, 30),
            Margin = new Padding(4, 0, 4, 6)
        });

        if (_carrito.Count == 0)
        {
            FlpCarrito.Controls.Add(new Label
            {
                Text = "Agrega productos para comenzar.",
                ForeColor = Color.DimGray,
                Size = new Size(280, 58),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(4, 18, 4, 4)
            });
        }
        else
        {
            foreach (CarritoItem item in _carrito)
            {
                FlpCarrito.Controls.Add(CrearFilaCarrito(item));
            }
        }

        ActualizarTotales();
    }

    private Control CrearFilaCarrito(CarritoItem item)
    {
        Panel fila = new()
        {
            Size = new Size(285, 82),
            BackColor = Color.White,
            Margin = new Padding(3, 3, 3, 7)
        };

        PictureBox imagen = new()
        {
            Location = new Point(6, 8),
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(248, 248, 248),
            Image = CargarImagenProducto(item.Producto.Imagen, item.Producto.Nombre)
        };

        Label nombre = new()
        {
            Text = item.Producto.Nombre,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Location = new Point(62, 6),
            Size = new Size(137, 23),
            AutoEllipsis = true
        };

        Label subtotal = new()
        {
            Text = item.Subtotal.ToString("C2", _culturaMexico),
            Location = new Point(200, 6),
            Size = new Size(77, 23),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = EstilosUI.Rojo,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };

        Button disminuir = CrearBotonCantidad("−", new Point(62, 37));
        Label cantidad = new()
        {
            Text = item.Cantidad.ToString(CultureInfo.InvariantCulture),
            Location = new Point(97, 39),
            Size = new Size(32, 26),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        Button aumentar = CrearBotonCantidad("+", new Point(133, 37));
        Button eliminar = new()
        {
            Text = "Quitar",
            Location = new Point(203, 38),
            Size = new Size(74, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = EstilosUI.Rojo,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8F)
        };
        eliminar.FlatAppearance.BorderColor = EstilosUI.Borde;

        disminuir.Click += (_, _) => CambiarCantidad(item, -1);
        aumentar.Click += (_, _) => CambiarCantidad(item, 1);
        eliminar.Click += (_, _) =>
        {
            _carrito.Remove(item);
            RenderizarCarrito();
        };

        fila.Controls.AddRange([imagen, nombre, subtotal, disminuir, cantidad, aumentar, eliminar]);
        EstilosUI.Redondear(fila, 10);
        return fila;
    }

    private static Button CrearBotonCantidad(string texto, Point ubicacion)
    {
        Button boton = new()
        {
            Text = texto,
            Location = ubicacion,
            Size = new Size(31, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(33, 37, 41),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        boton.FlatAppearance.BorderColor = Color.LightGray;
        return boton;
    }

    private void CambiarCantidad(CarritoItem item, int cambio)
    {
        int nuevaCantidad = item.Cantidad + cambio;

        if (nuevaCantidad <= 0)
        {
            _carrito.Remove(item);
        }
        else if (nuevaCantidad > item.Producto.Stock)
        {
            MessageBox.Show(
                $"Solo hay {item.Producto.Stock} unidades disponibles.",
                "Inventario insuficiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }
        else
        {
            item.Cantidad = nuevaCantidad;
        }

        RenderizarCarrito();
    }

    private void ActualizarTotales()
    {
        decimal subtotal = _carrito.Sum(item => item.Subtotal);
        decimal descuento = 0M;
        decimal total = subtotal - descuento;

        lblSubtotalTotal.Text = subtotal.ToString("C2", _culturaMexico);
        lblDescuento.Text = "-" + descuento.ToString("C2", _culturaMexico);
        lblTotalPagar.Text = total.ToString("C2", _culturaMexico);

        btnRealizarPedido.Enabled = _carrito.Count > 0;
        btnCancelarpedido.Enabled = _carrito.Count > 0;
    }

    private void BtnMetodoPago_Click(object? sender, EventArgs e)
    {
        using FrmMetodoPago formulario = new(_metodoPagoSeleccionado);
        if (formulario.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _metodoPagoSeleccionado = formulario.MetodoSeleccionado;
        btnMetodoPago.Text = _metodoPagoSeleccionado switch
        {
            TipoMetodoPago.Efectivo => "Método: Efectivo",
            TipoMetodoPago.Tarjeta => "Método: Tarjeta",
            _ => "Método de Pago"
        };
    }

    private async void BtnRealizarPedido_Click(object? sender, EventArgs e)
    {
        if (_carrito.Count == 0)
        {
            MessageBox.Show("Agrega al menos un producto.", "Pedido vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_metodoPagoSeleccionado == TipoMetodoPago.Ninguno)
        {
            MessageBox.Show(
                "Selecciona Efectivo o Tarjeta antes de realizar el pedido.",
                "Método de pago requerido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            BtnMetodoPago_Click(btnMetodoPago, EventArgs.Empty);

            if (_metodoPagoSeleccionado == TipoMetodoPago.Ninguno)
            {
                return;
            }
        }

        try
        {
            btnRealizarPedido.Enabled = false;
            btnRealizarPedido.Text = "Guardando...";

            int idMetodoPago = await _metodoPagoRepository.ObtenerIdAsync(_metodoPagoSeleccionado);
            int idPedido = await _pedidoRepository.CrearPedidoAsync(
                _carrito,
                idMetodoPago);

            MessageBox.Show(
                $"Pedido realizado correctamente.\nNúmero de pedido: {idPedido}",
                "Pedido registrado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LimpiarPedido();
            await CargarProductosAsync(_categoriaSeleccionada);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No fue posible registrar el pedido.\n\n" + ex.Message,
                "Error al realizar pedido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnRealizarPedido.Text = "Realizar pedido";
            ActualizarTotales();
        }
    }

    private void BtnCancelarPedido_Click(object? sender, EventArgs e)
    {
        if (_carrito.Count == 0)
        {
            return;
        }

        DialogResult respuesta = MessageBox.Show(
            "¿Deseas cancelar y quitar todos los productos del pedido?",
            "Cancelar pedido",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (respuesta == DialogResult.Yes)
        {
            LimpiarPedido();
        }
    }

    private void LimpiarPedido()
    {
        _carrito.Clear();
        _metodoPagoSeleccionado = TipoMetodoPago.Ninguno;
        btnMetodoPago.Text = "Método de Pago";
        RenderizarCarrito();
    }

    private async void BtnMiPedido_Click(object? sender, EventArgs e)
    {
        panel1.BringToFront();
        FlpCarrito.Focus();

        Color colorOriginal = panel1.BackColor;
        panel1.BackColor = Color.FromArgb(255, 244, 244);
        await Task.Delay(220);
        panel1.BackColor = colorOriginal;
    }

    private void MarcarCategoriaActiva(Button botonActivo)
    {
        foreach (Button boton in _botonesCategoria)
        {
            bool activo = ReferenceEquals(boton, botonActivo);
            bool esTodos = ReferenceEquals(boton, btnTodos);

            boton.BackColor = activo
                ? (esTodos ? EstilosUI.Rojo : EstilosUI.Amarillo)
                : Color.FromArgb(232, 235, 239);
            boton.ForeColor = activo && esTodos ? Color.White : EstilosUI.Texto;
        }

        _categoriaActiva = botonActivo;
    }

    private void CambiarEstadoCarga(bool cargando, string mensaje = "")
    {
        _cargandoProductos = cargando;
        foreach (Button boton in _botonesCategoria)
        {
            boton.Enabled = !cargando;
        }

        UseWaitCursor = cargando;

        if (cargando && flowLayoutPanel2.Controls.Count == 0)
        {
            flowLayoutPanel2.Controls.Add(new Label
            {
                Name = "lblCargando",
                Text = mensaje,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Size = new Size(650, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(20)
            });
        }
    }

    private Image CargarImagenProducto(string referenciaImagen, string nombreProducto)
    {
        if (string.IsNullOrWhiteSpace(referenciaImagen))
        {
            return CrearImagenPredeterminada(nombreProducto);
        }

        try
        {
            if (Uri.TryCreate(referenciaImagen, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using HttpClient cliente = new()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                byte[] datos = cliente.GetByteArrayAsync(uri).GetAwaiter().GetResult();
                using MemoryStream memoria = new(datos);
                using Image original = Image.FromStream(memoria);
                return new Bitmap(original);
            }

            string ruta = Path.IsPathRooted(referenciaImagen)
                ? referenciaImagen
                : Path.Combine(AppContext.BaseDirectory, "ImagenesProductos", referenciaImagen);

            if (File.Exists(ruta))
            {
                using Image original = Image.FromFile(ruta);
                return new Bitmap(original);
            }
        }
        catch
        {
            // Si la ruta o URL falla, se usa una imagen de respaldo.
        }

        return CrearImagenPredeterminada(nombreProducto);
    }

    private static Image CrearImagenPredeterminada(string nombreProducto)
    {
        Bitmap imagen = new(400, 220);
        using Graphics grafico = Graphics.FromImage(imagen);
        grafico.Clear(Color.FromArgb(245, 237, 230));

        using Brush circulo = new SolidBrush(Color.FromArgb(192, 0, 0));
        grafico.FillEllipse(circulo, 145, 28, 110, 110);

        using Font inicial = new("Segoe UI", 34F, FontStyle.Bold);
        using Brush textoBlanco = new SolidBrush(Color.White);
        string letra = string.IsNullOrWhiteSpace(nombreProducto) ? "U" : nombreProducto[..1].ToUpperInvariant();
        SizeF medida = grafico.MeasureString(letra, inicial);
        grafico.DrawString(letra, inicial, textoBlanco, 200 - medida.Width / 2, 83 - medida.Height / 2);

        using Font nombre = new("Segoe UI", 15F, FontStyle.Bold);
        using Brush texto = new SolidBrush(Color.FromArgb(60, 60, 60));
        StringFormat formato = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        grafico.DrawString(nombreProducto, nombre, texto, new RectangleF(25, 150, 350, 50), formato);
        formato.Dispose();

        return imagen;
    }

    private static void LimpiarControles(Control contenedor)
    {
        Control[] controles = contenedor.Controls.Cast<Control>().ToArray();
        contenedor.Controls.Clear();
        foreach (Control control in controles)
        {
            control.Dispose();
        }
    }

    private void MostrarErrorConexion(Exception ex)
    {
        MessageBox.Show(
            "No fue posible conectarse con UTSExpressDB.\n\n" +
            "Servidor configurado: localhost\n\n" +
            "Verifica que SQL Server esté iniciado y que la base UTSExpressDB exista.\n\n" +
            "Detalle: " + ex.Message,
            "Error de base de datos",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
