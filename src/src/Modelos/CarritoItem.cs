namespace UTSExpress_Principal.Modelos;

public sealed class CarritoItem
{
    public required Producto Producto { get; init; }
    public int Cantidad { get; set; }
    public decimal Subtotal => Producto.Precio * Cantidad;
}
