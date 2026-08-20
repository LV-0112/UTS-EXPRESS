-- =====================================================
-- UTS EXPRESS
-- Archivo: 02_seed_data.sql
-- Fuente: Base_de_Datos_UTSExpressDB_UNIFICADA.sql
-- Contenido: datos iniciales exactos de la base unificada
-- Requisito: ejecutar primero 01_ddl_schema.sql
-- =====================================================

USE UTSExpressDB;
GO

-- Usuarios que ya estaban en las bases del equipo.
INSERT INTO Usuarios (Matricula, [Contraseña], Rol)
VALUES
('ADMIN-UTS', 'admin123', 'Administrador'),
('20240001', 'alumno123', 'Cliente');
GO

INSERT INTO Metodo_Pago (Efectivo, Tarjeta)
VALUES (1,0), (0,1);
GO

INSERT INTO Menu (Dia, Id_Admin)
VALUES
('Lunes', 1),
('Martes', 1),
('Miércoles', 1),
('Jueves', 1),
('Viernes', 1);
GO

-- Categorías que ya existían en el proyecto visual.
INSERT INTO Categoria (Nombre, Id_Menu, Id_CategoriaPadre)
VALUES
('Dulces', 1, NULL),
('Bebidas', 1, NULL),
('Galletas', 1, NULL),
('Cafés', 1, NULL),
('Comidas', 1, NULL),
('Snacks', 1, NULL),
('Combos', 1, NULL);
GO

INSERT INTO Categoria (Nombre, Id_Menu, Id_CategoriaPadre)
VALUES
('Refrescos', 1, 2),
('Jugos', 1, 2),
('Lácteos', 1, 2),
('Naturales', 1, 2),
('Clásicas', 1, 3),
('Con Chips', 1, 3),
('Avena', 1, 3),
('Rellenas', 1, 3);
GO

-- Productos que ya estaban en el proyecto visual del compañero.
INSERT INTO Producto (Id_Categoria, Nombre, [Descripción], Precio, Imagen)
VALUES
(1, 'Muffin de Chocolate', 'Panquecito suave de chocolate', 28.00, 'muffin_chocolate.jpg'),
(2, 'Frappe de Vainilla', 'Bebida fria de vainilla', 40.00, 'frappe_vainilla.jpg'),
(3, 'Galleta Surtida', 'Seleccion de galletas', 18.00, 'galleta_avena.jpg'),
(4, 'Cafe Americano', 'Cafe negro clasico', 25.00, 'cafe_americano.jpg'),
(4, 'Capuchino', 'Cafe con espuma de leche', 35.00, 'capuchino.jpg'),
(4, 'Latte de Vainilla', 'Cafe con leche y vainilla', 38.00, 'latte_vainilla.jpg'),
(5, 'Sandwich de Pollo', 'Pollo, queso y vegetales', 55.00, 'sandwich_pollo.jpg'),
(5, 'Burrito de Frijol', 'Burrito con frijol y queso', 45.00, 'burrito_frijol.jpg'),
(6, 'Snack Express', 'Snack para acompañar', 20.00, 'producto_sin_imagen.jpg'),
(7, 'Combo Desayuno', 'Cafe, sandwich y galleta', 75.00, 'combo_desayuno.jpg'),
(7, 'Combo Express', 'Burrito, bebida y snack', 90.00, 'combo_express.jpg'),
(8, 'Coca Cola 600 ml', 'Refresco sabor cola original', 22.00, 'refresco_cola.jpg'),
(9, 'Jumex Mango', 'Jugo natural de mango', 20.00, 'producto_sin_imagen.jpg'),
(10, 'Bebida de Chocolate', 'Bebida lactea de chocolate', 24.00, 'producto_sin_imagen.jpg'),
(11, 'Agua Natural', 'Botella de agua natural', 15.00, 'agua_natural.jpg'),
(12, 'Galleta Canelitas', 'Galleta con sabor a canela', 20.00, 'galleta_avena.jpg'),
(13, 'Galleta ChocoChips', 'Galleta con trozos de chocolate', 15.50, 'muffin_chocolate.jpg'),
(14, 'Galleta Avena Express', 'Galleta de avena con pasas', 12.00, 'galleta_avena.jpg'),
(15, 'Galleta Rellena', 'Galleta rellena de chocolate', 18.00, 'muffin_chocolate.jpg');
GO

-- Los cinco productos que quedaron en el menú semanal
INSERT INTO Producto (Id_Categoria, Nombre, [Descripción], Precio, Imagen)
VALUES
(5, 'Enchiladas verdes', 'Orden de enchiladas verdes', 55.00, 'enchiladas.png'),
(5, 'Tacos dorados', 'Orden de tacos dorados', 50.00, 'tacos.png'),
(5, 'Hamburguesa', 'Hamburguesa con papas', 70.00, 'hamburguesa.png'),
(5, 'Milanesa de pollo', 'Milanesa acompañada', 65.00, 'pollo.png'),
(5, 'Espagueti rojo', 'Espagueti con salsa de tomate', 45.00, 'espagueti.png');
GO

-- La asignación original de esos cinco productos: uno por día.
INSERT INTO Menu_Producto (Id_Menu, Id_Producto)
SELECT M.Id_Menu, P.Id_Producto
FROM (VALUES
    ('Lunes', 'Enchiladas verdes'),
    ('Martes', 'Tacos dorados'),
    ('Miércoles', 'Hamburguesa'),
    ('Jueves', 'Milanesa de pollo'),
    ('Viernes', 'Espagueti rojo')
) AS X(Dia, Producto)
INNER JOIN Menu M ON M.Dia = X.Dia
INNER JOIN Producto P ON P.Nombre = X.Producto;
GO

-- El proyecto visual ya manejaba inventario para todos sus productos.
-- Se aplica la misma estructura a los productos del menú semanal para que ambos módulos funcionen juntos.
INSERT INTO Inventario
    (Id_Producto, Cantidad_Disponible, Stock_Minimo, Stock_Maximo, Ultima_Actualizacion, Id_Proveedor)
SELECT Id_Producto, 30, 5, 100, GETDATE(), NULL
FROM Producto;
GO

-- Consultas de verificación que estaban al final del archivo unificado.
SELECT 'Base creada correctamente' AS Resultado;
SELECT COUNT(*) AS TotalProductos FROM Producto;
SELECT M.Dia, P.Nombre
FROM Menu M
LEFT JOIN Menu_Producto MP ON MP.Id_Menu = M.Id_Menu
LEFT JOIN Producto P ON P.Id_Producto = MP.Id_Producto
ORDER BY M.Id_Menu;
GO
