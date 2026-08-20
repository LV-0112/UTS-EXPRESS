using Microsoft.Data.SqlClient;
using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Datos;

public sealed class PedidoRepository
{
    public async Task<int> CrearPedidoAsync(
        IReadOnlyCollection<CarritoItem> carrito,
        int idMetodoPago)
    {
        if (carrito.Count == 0)
        {
            throw new InvalidOperationException("El pedido está vacío.");
        }

        await using SqlConnection conexion = Conexion.Crear();
        await conexion.OpenAsync();
        using SqlTransaction transaccion = conexion.BeginTransaction();

        try
        {
            decimal total = carrito.Sum(item => item.Subtotal);

            const string consultarUsoDeUsuario = """
                SELECT CASE
                    WHEN COL_LENGTH('dbo.Pedido', 'Id_Usuario') IS NULL
                        THEN 0
                    ELSE 1
                END;
                """;

            bool pedidoUsaUsuario;

            await using (SqlCommand comandoEstructura =
                new(consultarUsoDeUsuario, conexion, transaccion))
            {
                object? resultadoEstructura =
                    await comandoEstructura.ExecuteScalarAsync();

                pedidoUsaUsuario =
                    Convert.ToInt32(resultadoEstructura) == 1;
            }

            int? idUsuarioSistema = null;

            if (pedidoUsaUsuario)
            {
                const string obtenerUsuarioSistema = """
                    DECLARE @IdUsuario INT;

                    SELECT TOP 1 @IdUsuario = Id_Usuario
                    FROM Usuarios
                    WHERE Matricula = '20240001';

                    SELECT @IdUsuario;
                    """;

                await using SqlCommand comandoUsuario =
                    new(obtenerUsuarioSistema, conexion, transaccion);

                object? resultadoUsuario =
                    await comandoUsuario.ExecuteScalarAsync();

                if (resultadoUsuario is null ||
                    resultadoUsuario == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No fue posible obtener el usuario interno 20240001.");
                }

                idUsuarioSistema =
                    Convert.ToInt32(resultadoUsuario);
            }

            foreach (CarritoItem item in carrito)
            {
                const string consultarStock = """
                    SELECT ISNULL(Cantidad_Disponible, 0)
                    FROM Inventario WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id_Producto = @IdProducto;
                    """;

                await using SqlCommand comandoStock =
                    new(consultarStock, conexion, transaccion);

                comandoStock.Parameters.AddWithValue(
                    "@IdProducto",
                    item.Producto.IdProducto);

                object? resultadoStock =
                    await comandoStock.ExecuteScalarAsync();

                int stockActual =
                    resultadoStock is null || resultadoStock == DBNull.Value
                        ? 0
                        : Convert.ToInt32(resultadoStock);

                if (stockActual < item.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"No hay suficiente inventario de " +
                        $"{item.Producto.Nombre}. Disponible: {stockActual}.");
                }
            }

            const string insertarPedidoConUsuario = """
                INSERT INTO Pedido
                    (
                        Fecha_Pedido,
                        Total,
                        Estado,
                        Id_Usuario,
                        Id_MetodoPago
                    )
                OUTPUT INSERTED.Id_Pedido
                VALUES
                    (
                        GETDATE(),
                        @Total,
                        'Realizado',
                        @IdUsuario,
                        @IdMetodoPago
                    );
                """;

            const string insertarPedidoSinUsuario = """
                INSERT INTO Pedido
                    (
                        Fecha_Pedido,
                        Total,
                        Estado,
                        Id_MetodoPago
                    )
                OUTPUT INSERTED.Id_Pedido
                VALUES
                    (
                        GETDATE(),
                        @Total,
                        'Realizado',
                        @IdMetodoPago
                    );
                """;

            string insertarPedido = pedidoUsaUsuario
                ? insertarPedidoConUsuario
                : insertarPedidoSinUsuario;

            int idPedido;

            await using (SqlCommand comandoPedido =
                new(insertarPedido, conexion, transaccion))
            {
                comandoPedido.Parameters.AddWithValue("@Total", total);

                if (pedidoUsaUsuario)
                {
                    comandoPedido.Parameters.AddWithValue(
                        "@IdUsuario",
                        idUsuarioSistema!.Value);
                }

                comandoPedido.Parameters.AddWithValue(
                    "@IdMetodoPago",
                    idMetodoPago);

                object? resultado =
                    await comandoPedido.ExecuteScalarAsync();

                if (resultado is null || resultado == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "SQL Server no devolvió el número del pedido.");
                }

                idPedido = Convert.ToInt32(resultado);
            }

            foreach (CarritoItem item in carrito)
            {
                const string insertarDetalle = """
                    INSERT INTO DetallePedido
                        (
                            Id_Pedido,
                            Id_Producto,
                            Cantidad,
                            PrecioUnitario,
                            Subtotal
                        )
                    VALUES
                        (
                            @IdPedido,
                            @IdProducto,
                            @Cantidad,
                            @Precio,
                            @Subtotal
                        );
                    """;

                await using (SqlCommand comandoDetalle =
                    new(insertarDetalle, conexion, transaccion))
                {
                    comandoDetalle.Parameters.AddWithValue(
                        "@IdPedido",
                        idPedido);

                    comandoDetalle.Parameters.AddWithValue(
                        "@IdProducto",
                        item.Producto.IdProducto);

                    comandoDetalle.Parameters.AddWithValue(
                        "@Cantidad",
                        item.Cantidad);

                    comandoDetalle.Parameters.AddWithValue(
                        "@Precio",
                        item.Producto.Precio);

                    comandoDetalle.Parameters.AddWithValue(
                        "@Subtotal",
                        item.Subtotal);

                    await comandoDetalle.ExecuteNonQueryAsync();
                }

                const string descontarInventario = """
                    UPDATE Inventario
                    SET
                        Cantidad_Disponible =
                            Cantidad_Disponible - @Cantidad,
                        Ultima_Actualizacion = GETDATE()
                    WHERE
                        Id_Producto = @IdProducto
                        AND Cantidad_Disponible >= @Cantidad;
                    """;

                await using SqlCommand comandoInventario =
                    new(descontarInventario, conexion, transaccion);

                comandoInventario.Parameters.AddWithValue(
                    "@Cantidad",
                    item.Cantidad);

                comandoInventario.Parameters.AddWithValue(
                    "@IdProducto",
                    item.Producto.IdProducto);

                int filasActualizadas =
                    await comandoInventario.ExecuteNonQueryAsync();

                if (filasActualizadas == 0)
                {
                    throw new InvalidOperationException(
                        $"No se pudo descontar el inventario de " +
                        $"{item.Producto.Nombre}.");
                }
            }

            transaccion.Commit();
            return idPedido;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
}
