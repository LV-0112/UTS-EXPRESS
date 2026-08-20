using Microsoft.Data.SqlClient;
using UTSExpress_Principal.Modelos;

namespace UTSExpress_Principal.Datos;

public sealed class MetodoPagoRepository
{
    public async Task<int> ObtenerIdAsync(TipoMetodoPago tipo)
    {
        if (tipo == TipoMetodoPago.Ninguno)
        {
            throw new InvalidOperationException("No se seleccionó un método de pago.");
        }

        bool efectivo = tipo == TipoMetodoPago.Efectivo;
        bool tarjeta = tipo == TipoMetodoPago.Tarjeta;

        const string sql = """
            DECLARE @IdMetodoPago INT;

            SELECT TOP 1 @IdMetodoPago = Id_MetodoPago
            FROM Metodo_Pago
            WHERE Efectivo = @Efectivo
              AND Tarjeta = @Tarjeta
            ORDER BY Id_MetodoPago;

            IF @IdMetodoPago IS NULL
            BEGIN
                INSERT INTO Metodo_Pago (Efectivo, Tarjeta)
                VALUES (@Efectivo, @Tarjeta);

                SET @IdMetodoPago = SCOPE_IDENTITY();
            END;

            SELECT @IdMetodoPago;
            """;

        await using SqlConnection conexion = Conexion.Crear();
        await conexion.OpenAsync();
        await using SqlCommand comando = new(sql, conexion);
        comando.Parameters.AddWithValue("@Efectivo", efectivo);
        comando.Parameters.AddWithValue("@Tarjeta", tarjeta);

        object? resultado = await comando.ExecuteScalarAsync();
        if (resultado is null || resultado == DBNull.Value)
        {
            throw new InvalidOperationException("No fue posible registrar el método de pago.");
        }

        return Convert.ToInt32(resultado);
    }
}
