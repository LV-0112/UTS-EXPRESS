using Microsoft.Data.SqlClient;

namespace UTSExpress_Principal.Datos;

public static class Conexion
{
    // Conexión a la base unificada. Si cambia de computadora, solo ajusta Server.
    public const string CadenaConexion =
        @"Server=(local);Database=UTSExpressDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;Connect Timeout=8;";

    public static SqlConnection Crear() => new(CadenaConexion);
}
