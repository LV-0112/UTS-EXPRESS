using Microsoft.Data.SqlClient;
using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Datos;

public sealed class ProductoRepository
{
    public async Task<List<Producto>> ObtenerProductosAsync(string? categoria = null)
    {
        const string sql = """
            WITH ProductosFiltrados AS
            (
                SELECT
                    p.Id_Producto,
                    p.Nombre,
                    ISNULL(p.[Descripción], '') AS Descripcion,
                    p.Precio,
                    ISNULL(p.Imagen, '') AS Imagen,
                    p.Id_Categoria,
                    c.Nombre AS Categoria,
                    padre.Nombre AS CategoriaPadre,
                    ISNULL(inventario.Cantidad_Disponible, 0) AS Stock
                FROM Producto p
                INNER JOIN Categoria c
                    ON c.Id_Categoria = p.Id_Categoria
                LEFT JOIN Categoria padre
                    ON padre.Id_Categoria = c.Id_CategoriaPadre
                OUTER APPLY
                (
                    SELECT TOP (1)
                        i.Cantidad_Disponible
                    FROM Inventario i
                    WHERE i.Id_Producto = p.Id_Producto
                    ORDER BY
                        i.Ultima_Actualizacion DESC,
                        i.Id_Inventario DESC
                ) inventario
                WHERE
                    LTRIM(RTRIM(p.Nombre)) NOT IN
                    (
                        'Galleta Quaker',
                        'Refresco de Cola'
                    )
                    AND
                    (
                        @Categoria IS NULL
                        OR c.Nombre = @Categoria
                        OR padre.Nombre = @Categoria
                        OR
                        (
                            @Categoria = 'Snacks'
                            AND
                            (
                                c.Nombre IN
                                (
                                    'Dulces',
                                    'Galletas',
                                    'Clásicas',
                                    'Con Chips',
                                    'Avena',
                                    'Rellenas'
                                )
                                OR padre.Nombre IN ('Dulces', 'Galletas')
                            )
                        )
                    )
            ),
            ProductosSinDuplicar AS
            (
                SELECT
                    *,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY UPPER(LTRIM(RTRIM(Nombre)))
                        ORDER BY Id_Producto
                    ) AS NumeroFila
                FROM ProductosFiltrados
            )
            SELECT
                Id_Producto,
                Nombre,
                Descripcion,
                Precio,
                Imagen,
                Id_Categoria,
                Categoria,
                Stock
            FROM ProductosSinDuplicar
            WHERE NumeroFila = 1
            ORDER BY
                COALESCE(CategoriaPadre, Categoria),
                Categoria,
                Nombre;
            """;

        List<Producto> productos = [];

        await using SqlConnection conexion = Conexion.Crear();
        await conexion.OpenAsync();

        await using SqlCommand comando = new(sql, conexion);
        comando.Parameters.AddWithValue(
            "@Categoria",
            string.IsNullOrWhiteSpace(categoria)
                ? DBNull.Value
                : categoria);

        await using SqlDataReader lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            productos.Add(new Producto
            {
                IdProducto = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Descripcion = lector.GetString(2),
                Precio = lector.GetDecimal(3),
                Imagen = lector.GetString(4),
                IdCategoria = lector.GetInt32(5),
                Categoria = lector.GetString(6),
                Stock = lector.GetInt32(7)
            });
        }

        return productos;
    }
}
