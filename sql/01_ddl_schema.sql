USE master;
GO

IF DB_ID('UTSExpressDB') IS NOT NULL
BEGIN
    ALTER DATABASE UTSExpressDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE UTSExpressDB;
END
GO

CREATE DATABASE UTSExpressDB;
GO
USE UTSExpressDB;
GO

CREATE TABLE Usuarios
(
    Id_Usuario INT IDENTITY(1,1) PRIMARY KEY,
    Matricula VARCHAR(50) NOT NULL UNIQUE,
    [Contraseña] VARCHAR(50) NOT NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Cliente'
);
GO

CREATE TABLE Metodo_Pago
(
    Id_MetodoPago INT IDENTITY(1,1) PRIMARY KEY,
    Efectivo BIT NOT NULL,
    Tarjeta BIT NOT NULL
);
GO

CREATE TABLE Menu
(
    Id_Menu INT IDENTITY(1,1) PRIMARY KEY,
    Dia VARCHAR(15) NOT NULL UNIQUE,
    Id_Admin INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id_Usuario)
);
GO

CREATE TABLE Categoria
(
    Id_Categoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Id_Menu INT NOT NULL FOREIGN KEY REFERENCES Menu(Id_Menu),
    Id_CategoriaPadre INT NULL
);
GO
ALTER TABLE Categoria
ADD CONSTRAINT FK_Categoria_CategoriaPadre
FOREIGN KEY (Id_CategoriaPadre) REFERENCES Categoria(Id_Categoria);
GO

CREATE TABLE Producto
(
    Id_Producto INT IDENTITY(1,1) PRIMARY KEY,
    Id_Categoria INT NOT NULL FOREIGN KEY REFERENCES Categoria(Id_Categoria),
    Nombre VARCHAR(50) NOT NULL,
    [Descripción] VARCHAR(150) NULL,
    Precio DECIMAL(8,2) NOT NULL,
    Imagen VARCHAR(225) NULL
);
GO

-- Relación del proyecto de menú semanal
CREATE TABLE Menu_Producto
(
    Id_Menu INT NOT NULL FOREIGN KEY REFERENCES Menu(Id_Menu),
    Id_Producto INT NOT NULL FOREIGN KEY REFERENCES Producto(Id_Producto),
    CONSTRAINT PK_Menu_Producto PRIMARY KEY (Id_Menu, Id_Producto)
);
GO

CREATE TABLE Pedido
(
    Id_Pedido INT IDENTITY(1,1) PRIMARY KEY,
    Fecha_Pedido DATETIME NOT NULL,
    Total DECIMAL(8,2) NOT NULL,
    Estado VARCHAR(20) NULL,
    Id_Usuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id_Usuario),
    Id_MetodoPago INT NULL FOREIGN KEY REFERENCES Metodo_Pago(Id_MetodoPago)
);
GO

CREATE TABLE DetallePedido
(
    Id_Detalle INT IDENTITY(1,1) PRIMARY KEY,
    Id_Pedido INT NOT NULL FOREIGN KEY REFERENCES Pedido(Id_Pedido),
    Id_Producto INT NOT NULL FOREIGN KEY REFERENCES Producto(Id_Producto),
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(8,2) NOT NULL,
    Subtotal DECIMAL(8,2) NOT NULL
);
GO

CREATE TABLE [Reseña]
(
    [Id_Reseña] INT IDENTITY(1,1) PRIMARY KEY,
    Comentario VARCHAR(50) NOT NULL,
    Id_Producto INT NOT NULL FOREIGN KEY REFERENCES Producto(Id_Producto),
    Id_Usuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id_Usuario)
);
GO

CREATE TABLE Carrito
(
    Id_Carrito INT IDENTITY(1,1) PRIMARY KEY,
    Id_Usuario INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Usuarios(Id_Usuario)
);
GO

CREATE TABLE Proveedor
(
    Id_Proveedor INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Empresa VARCHAR(50) NOT NULL,
    Contacto VARCHAR(50) NULL,
    Telefono VARCHAR(15) NOT NULL,
    Correo VARCHAR(50) NULL,
    Direccion VARCHAR(100) NULL
);
GO

CREATE TABLE Inventario
(
    Id_Inventario INT IDENTITY(1,1) PRIMARY KEY,
    Id_Producto INT NOT NULL FOREIGN KEY REFERENCES Producto(Id_Producto),
    Cantidad_Disponible INT NOT NULL,
    Stock_Minimo INT NOT NULL,
    Stock_Maximo INT NOT NULL,
    Ultima_Actualizacion DATETIME NOT NULL,
    Id_Proveedor INT NULL FOREIGN KEY REFERENCES Proveedor(Id_Proveedor)
);
GO
