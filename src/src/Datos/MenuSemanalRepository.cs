using Microsoft.Data.SqlClient;
using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Datos;

public sealed class MenuSemanalRepository
{
    public async Task<List<Producto>> ObtenerPorDiaAsync(string dia)
    {
        const string sql = """
            SELECT
                P.Id_Producto,
                P.Nombre,
                ISNULL(P.[Descripción], '') AS Descripcion,
                P.Precio,
                ISNULL(P.Imagen, '') AS Imagen,
                P.Id_Categoria,
                C.Nombre AS Categoria,
                ISNULL(I.Cantidad_Disponible, 0) AS Stock
            FROM Menu AS M
            INNER JOIN Menu_Producto AS MP ON MP.Id_Menu = M.Id_Menu
            INNER JOIN Producto AS P ON P.Id_Producto = MP.Id_Producto
            INNER JOIN Categoria AS C ON C.Id_Categoria = P.Id_Categoria
            LEFT JOIN Inventario AS I ON I.Id_Producto = P.Id_Producto
            WHERE M.Dia = @Dia
            ORDER BY P.Nombre;
            """;

        List<Producto> productos = [];

        await using SqlConnection conexion = Conexion.Crear();
        await conexion.OpenAsync();
        await using SqlCommand comando = new(sql, conexion);
        comando.Parameters.AddWithValue("@Dia", dia);

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
