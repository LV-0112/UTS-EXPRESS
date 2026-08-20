-- =====================================================
-- UTS EXPRESS
-- Archivo: 04_dml_analitico_avanzado.sql
-- Contenido: consultas analíticas compatibles con la base unificada
-- Incluye: 3 INNER JOIN, 3 GROUP BY y 2 HAVING
-- Requisito: ejecutar primero 01 y 02
-- =====================================================

USE UTSExpressDB;
GO

-- CONSULTA 1 - INNER JOIN
-- Pregunta de negocio: ¿A qué categoría pertenece cada producto y cuál es su precio?
SELECT
    P.Id_Producto,
    P.Nombre AS Producto,
    C.Nombre AS Categoria,
    P.Precio
FROM Producto AS P
INNER JOIN Categoria AS C
    ON C.Id_Categoria = P.Id_Categoria
ORDER BY C.Nombre, P.Nombre;
GO

-- CONSULTA 2 - INNER JOIN
-- Pregunta de negocio: ¿Qué producto está asignado a cada día del menú semanal?
SELECT
    M.Dia,
    P.Nombre AS Producto,
    P.Precio
FROM Menu AS M
INNER JOIN Menu_Producto AS MP
    ON MP.Id_Menu = M.Id_Menu
INNER JOIN Producto AS P
    ON P.Id_Producto = MP.Id_Producto
ORDER BY M.Id_Menu;
GO

-- CONSULTA 3 - INNER JOIN
-- Pregunta de negocio: ¿Cuánto inventario disponible tiene cada producto?
SELECT
    P.Nombre AS Producto,
    I.Cantidad_Disponible,
    I.Stock_Minimo,
    I.Stock_Maximo,
    I.Ultima_Actualizacion
FROM Inventario AS I
INNER JOIN Producto AS P
    ON P.Id_Producto = I.Id_Producto
ORDER BY P.Nombre;
GO

-- CONSULTA 4 - AGREGACIÓN Y GROUP BY
-- Pregunta de negocio: ¿Cuántos productos hay registrados en cada categoría?
SELECT
    C.Nombre AS Categoria,
    COUNT(P.Id_Producto) AS TotalProductos
FROM Categoria AS C
LEFT JOIN Producto AS P
    ON P.Id_Categoria = C.Id_Categoria
GROUP BY C.Id_Categoria, C.Nombre
ORDER BY TotalProductos DESC, Categoria;
GO

-- CONSULTA 5 - AGREGACIÓN Y GROUP BY
-- Pregunta de negocio: ¿Cuál es el precio promedio, mínimo y máximo por categoría con productos?
SELECT
    C.Nombre AS Categoria,
    AVG(P.Precio) AS PrecioPromedio,
    MIN(P.Precio) AS PrecioMinimo,
    MAX(P.Precio) AS PrecioMaximo
FROM Categoria AS C
INNER JOIN Producto AS P
    ON P.Id_Categoria = C.Id_Categoria
GROUP BY C.Id_Categoria, C.Nombre
ORDER BY C.Nombre;
GO

-- CONSULTA 6 - AGREGACIÓN Y GROUP BY
-- Pregunta de negocio: ¿Cuántas unidades y qué valor de inventario tiene cada categoría?
SELECT
    C.Nombre AS Categoria,
    SUM(I.Cantidad_Disponible) AS UnidadesDisponibles,
    SUM(I.Cantidad_Disponible * P.Precio) AS ValorInventario
FROM Categoria AS C
INNER JOIN Producto AS P
    ON P.Id_Categoria = C.Id_Categoria
INNER JOIN Inventario AS I
    ON I.Id_Producto = P.Id_Producto
GROUP BY C.Id_Categoria, C.Nombre
ORDER BY ValorInventario DESC;
GO

-- CONSULTA 7 - HAVING
-- Pregunta de negocio: ¿Qué categorías tienen dos o más productos registrados?
SELECT
    C.Nombre AS Categoria,
    COUNT(P.Id_Producto) AS TotalProductos
FROM Categoria AS C
INNER JOIN Producto AS P
    ON P.Id_Categoria = C.Id_Categoria
GROUP BY C.Id_Categoria, C.Nombre
HAVING COUNT(P.Id_Producto) >= 2
ORDER BY TotalProductos DESC, Categoria;
GO

-- CONSULTA 8 - HAVING
-- Pregunta de negocio: ¿Qué categorías tienen un precio promedio mayor a 30 pesos?
SELECT
    C.Nombre AS Categoria,
    AVG(P.Precio) AS PrecioPromedio
FROM Categoria AS C
INNER JOIN Producto AS P
    ON P.Id_Categoria = C.Id_Categoria
GROUP BY C.Id_Categoria, C.Nombre
HAVING AVG(P.Precio) > 30.00
ORDER BY PrecioPromedio DESC;
GO
