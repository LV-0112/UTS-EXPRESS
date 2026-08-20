using System.Drawing.Drawing2D;

namespace UTSExpress_Principal.Utilidades;

public static class EstilosUI
{
    public static readonly Color Rojo = Color.FromArgb(188, 18, 38);
    public static readonly Color Amarillo = Color.FromArgb(247, 196, 38);
    public static readonly Color Fondo = Color.FromArgb(247, 248, 250);
    public static readonly Color Texto = Color.FromArgb(37, 42, 49);
    public static readonly Color Borde = Color.FromArgb(222, 225, 230);

    public static void Redondear(Control control, int radio)
    {
        void AplicarRegion()
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            int diametro = Math.Max(2, Math.Min(radio * 2, Math.Min(control.Width, control.Height)));
            Rectangle rectangulo = new(0, 0, control.Width, control.Height);
            using GraphicsPath ruta = new();

            ruta.AddArc(rectangulo.Left, rectangulo.Top, diametro, diametro, 180, 90);
            ruta.AddArc(rectangulo.Right - diametro, rectangulo.Top, diametro, diametro, 270, 90);
            ruta.AddArc(rectangulo.Right - diametro, rectangulo.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(rectangulo.Left, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();

            control.Region?.Dispose();
            control.Region = new Region(ruta);
        }

        control.Resize += (_, _) => AplicarRegion();
        AplicarRegion();
    }

    public static Panel AgregarSombra(Control objetivo, int desplazamiento = 7)
    {
        Control? padre = objetivo.Parent;
        Panel sombra = new()
        {
            BackColor = Color.FromArgb(220, 223, 228),
            Location = new Point(objetivo.Left + desplazamiento, objetivo.Top + desplazamiento),
            Size = objetivo.Size,
            Enabled = false,
            TabStop = false
        };

        if (padre is null)
        {
            return sombra;
        }

        padre.Controls.Add(sombra);
        sombra.SendToBack();
        objetivo.BringToFront();
        Redondear(sombra, 12);
        Redondear(objetivo, 12);

        objetivo.LocationChanged += (_, _) =>
            sombra.Location = new Point(objetivo.Left + desplazamiento, objetivo.Top + desplazamiento);
        objetivo.SizeChanged += (_, _) => sombra.Size = objetivo.Size;
        objetivo.VisibleChanged += (_, _) => sombra.Visible = objetivo.Visible;

        return sombra;
    }
}
