-- =====================================================
-- UTS EXPRESS
-- Archivo: 03_dml_transaccional.sql
-- Contenido: ejemplo de INSERT, UPDATE, SELECT y DELETE
-- Basado exclusivamente en el esquema unificado
-- Requisito: ejecutar primero 01 y 02
-- =====================================================
-- Este ejercicio usa ROLLBACK al final para no cambiar los datos originales.

USE UTSExpressDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @IdUsuario INT;
    DECLARE @IdMetodoPago INT;
    DECLARE @IdProducto INT;
    DECLARE @Precio DECIMAL(8,2);
    DECLARE @IdPedido INT;

    SELECT @IdUsuario = Id_Usuario
    FROM Usuarios
    WHERE Matricula = '20240001';

    SELECT TOP (1) @IdMetodoPago = Id_MetodoPago
    FROM Metodo_Pago
    WHERE Efectivo = 1 AND Tarjeta = 0
    ORDER BY Id_MetodoPago;

    SELECT
        @IdProducto = Id_Producto,
        @Precio = Precio
    FROM Producto
    WHERE Nombre = 'Enchiladas verdes';

    -- INSERT: registrar un pedido y su detalle.
    INSERT INTO Pedido
        (Fecha_Pedido, Total, Estado, Id_Usuario, Id_MetodoPago)
    VALUES
        (GETDATE(), @Precio, 'Pendiente', @IdUsuario, @IdMetodoPago);

    SET @IdPedido = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO DetallePedido
        (Id_Pedido, Id_Producto, Cantidad, PrecioUnitario, Subtotal)
    VALUES
        (@IdPedido, @IdProducto, 1, @Precio, @Precio);

    -- UPDATE: descontar una unidad del inventario.
    UPDATE Inventario
    SET Cantidad_Disponible = Cantidad_Disponible - 1,
        Ultima_Actualizacion = GETDATE()
    WHERE Id_Producto = @IdProducto
      AND Cantidad_Disponible >= 1;

    -- UPDATE: cambiar el estado del pedido.
    UPDATE Pedido
    SET Estado = 'Pagado'
    WHERE Id_Pedido = @IdPedido;

    -- SELECT: consultar el pedido recién registrado.
    SELECT
        PE.Id_Pedido,
        PE.Fecha_Pedido,
        U.Matricula,
        PR.Nombre AS Producto,
        DP.Cantidad,
        DP.PrecioUnitario,
        DP.Subtotal,
        PE.Total,
        PE.Estado
    FROM Pedido AS PE
    INNER JOIN Usuarios AS U
        ON U.Id_Usuario = PE.Id_Usuario
    INNER JOIN DetallePedido AS DP
        ON DP.Id_Pedido = PE.Id_Pedido
    INNER JOIN Producto AS PR
        ON PR.Id_Producto = DP.Id_Producto
    WHERE PE.Id_Pedido = @IdPedido;

    -- DELETE: eliminar primero el detalle y después el pedido.
    DELETE FROM DetallePedido
    WHERE Id_Pedido = @IdPedido;

    DELETE FROM Pedido
    WHERE Id_Pedido = @IdPedido;

    -- ROLLBACK conserva la base exactamente como estaba antes del ejercicio.
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
