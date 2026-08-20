namespace UTSExpress_Principal.Modelos;

public sealed class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Imagen { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int Stock { get; set; }
}
