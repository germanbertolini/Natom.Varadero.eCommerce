CREATE DATABASE ecommerce_db;
USE ecommerce_db;

CREATE TABLE ListaPrecios
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKListaDePreciosId INT,
    Descripcion VARCHAR(60),
    Activa BIT(1)
);
CREATE INDEX indListaPrecios ON ListaPrecios(PKListaDePreciosId);

CREATE TABLE CondicionDeVenta
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKCondicionVentaId INT,
    Descripcion VARCHAR(60),
    Dias INT
);
CREATE INDEX indCondicionDeVenta ON CondicionDeVenta(PKCondicionVentaId);

CREATE TABLE ClienteDireccion
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	ClienteDireccionId DECIMAL(18,0),
	PKSucursalId INT,
    PKClienteId INT,
    PKDireccionId INT,
    Direccion VARCHAR(60),
    CodigoPostal BIGINT,
    Telefono VARCHAR(45)
);
CREATE INDEX indClienteDireccion ON ClienteDireccion(ClienteDireccionId);

CREATE TABLE Cliente
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	ClienteId DECIMAL(18,0),
    
	PKSucursalId INT,
    PKClienteId INT,
    
    CUIT DOUBLE,
    RazonSocial VARCHAR(60),
    NombreFantasia VARCHAR(60),
    CodigoProvincia VARCHAR(20),
    Moneda CHAR(3),
    
    UsuarioAlias VARCHAR(15),
    UsuarioClave VARCHAR(100),
    UsuarioEmail VARCHAR(40),
    
    LimiteDeCredito DECIMAL(12,2),
    SaldoEnCtaCte DECIMAL(12,2),
    
    SesionIP NVARCHAR(50),
    SesionAgent TEXT,
    SesionToken CHAR(32),
    SesionInicio DATETIME,
    SesionUltimaAccion DATETIME
);
CREATE INDEX indCliente ON Cliente(ClienteId);

CREATE TABLE Articulo
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKArticuloId INT,
	ArticuloCodigo VARCHAR(20),
    ArticuloNombre VARCHAR(45),
    ArticuloDescripcion VARCHAR(120),
    ArticuloDescripcionAbreviada VARCHAR(60),
    ArticuloStock DECIMAL(12,3),
    ArticuloActivo BIT(1),
    MarcaId VARCHAR(20),
    RubroId VARCHAR(20),
    SubRubroId VARCHAR(20),
    GrupoId INT
);
CREATE INDEX indArticulo ON Articulo(PKArticuloId);

CREATE TABLE Rubro
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKRubroId VARCHAR(20),
    Descripcion VARCHAR(60),
    TipoArticulo CHAR(1)
);
CREATE INDEX indRubro ON Rubro(PKRubroId);

CREATE TABLE SubRubro
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	SubRubroId VARCHAR(40),
	PKSubRubroId VARCHAR(20),
    PKRubroId VARCHAR(20),
    Descripcion VARCHAR(60),
    TipoArticulo CHAR(1)
);
CREATE INDEX indSubRubro ON SubRubro(SubRubroId);

CREATE TABLE ArticuloGrupo
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKGrupoId INT,
    Descripcion VARCHAR(60)
);
CREATE INDEX indArticuloGrupo ON ArticuloGrupo(PKGrupoId);

CREATE TABLE Marca
(
	EF_Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	PKMarcaId VARCHAR(20),
    Descripcion VARCHAR(60)
);
CREATE INDEX indMarca ON Marca(PKMarcaId);

CREATE TABLE Pedido
(
	PedidoId INT AUTO_INCREMENT PRIMARY KEY,
    
    PuntoDeVenta INT NOT NULL,
    Numero INT NOT NULL,
    
    Fecha DATE,
    
    SucursalId INT,
    ClienteId INT,
    
    FechaHoraCreacion DATETIME,
    FechaHoraConfirmacion DATETIME,    
    FechaHoraConfirmacionEnvioEmail DATETIME,
    FechaHoraAnulacion DATETIME,    
    FechaHoraAnulacionEnvioEmail DATETIME,
    
    EnvioDireccion VARCHAR(60),
    EnvioCodigoPostal BIGINT,
    EnvioTelefono VARCHAR(45),
    
    Observaciones VARCHAR(200)
);

CREATE TABLE PedidoDetalle
(
	PedidoDetalleId INT AUTO_INCREMENT PRIMARY KEY,
    PedidoId INT,
    
    ArticuloId INT,
	Codigo VARCHAR(20),
    Nombre VARCHAR(45),
    ArticuloDescripcionAbreviada VARCHAR(60),
    Marca VARCHAR(60),
    
    Cantidad DECIMAL(18,2),
    PrecioUnitario DECIMAL(18,2)
);

CREATE TABLE Log
(
	Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    
    LogType NVARCHAR(50),
    Application NVARCHAR(50),
    
    SesionClienteId DECIMAL(18,0),
    SesionIP NVARCHAR(50),
    SesionAgent TEXT,
    
    FechaHora DATETIME,
    ActionUrl NVARCHAR(200),
    
    DataContext TEXT,
    Message TEXT,
    StackTrace TEXT
);

CREATE TABLE SyncSchedule
(
	SyncScheduleId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Hora TIME,
    Rutina NVARCHAR(50)
);

INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('03:05:00', 'ArticuloRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:07:00', 'ArticuloGrupoRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:09:00', 'CondicionDeVentaRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:11:00', 'ListaPreciosRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:13:00', 'MarcaRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:15:00', 'RubroRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:17:00', 'SubRubroRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:19:00', 'ClienteDireccionRoutine');
INSERT INTO SyncSchedule (Hora, Rutina) VALUES ('04:21:00', 'ClienteRoutine');

CREATE TABLE SyncSesion
(
	SyncSesionId DECIMAL(18,0) NOT NULL PRIMARY KEY,
    SesionIP NVARCHAR(50),
    FechaHoraInicio DATETIME,
    FechaHoraUltimaConexion DATETIME
);

DELIMITER $$
DROP PROCEDURE IF EXISTS `spSyncSesionRegistrarActividad` $$
CREATE PROCEDURE `spSyncSesionRegistrarActividad`(
	pSyncSesionId DECIMAL(18,0),
    pSesionIP NVARCHAR(50)
)
BEGIN

	DECLARE cant INT;
    
    SELECT
		COUNT(*)
	INTO
		cant
	FROM
		SyncSesion
	WHERE
		SyncSesionId = pSyncSesionId;
        
	IF cant = 0 THEN
		INSERT INTO SyncSesion (SyncSesionId, SesionIP, FechaHoraInicio, FechaHoraUltimaConexion) VALUES (pSyncSesionId, pSesionIP, NOW(), NOW());
    ELSE
		UPDATE SyncSesion SET FechaHoraUltimaConexion = NOW() WHERE SyncSesionId = pSyncSesionId;
	END IF;
    
END $$
DELIMITER ;

CREATE TABLE eCommerceStatus
(
	Id INT NOT NULL PRIMARY KEY,
    LastStartUp DATETIME,
    RunningSyncsCounter INT
);

INSERT INTO eCommerceStatus (Id, LastStartUp, RunningSyncsCounter) VALUES (1, NULL, 0);


/******** UPDATE 17/11/2019 *********/
ALTER TABLE pedidodetalle ADD PorcentajeDescuento DECIMAL(18,2);
ALTER TABLE pedidodetalle ADD PrecioUnitarioConDescuento DECIMAL(18,2);

/******** UPDATE 1/12/2019 **********/
ALTER TABLE Pedido ADD FechaHoraInicioSincronizado DATETIME;
ALTER TABLE Pedido ADD FechaHoraFinSincronizado DATETIME;

ALTER TABLE Pedido ADD AnuladoPorInactividad BIT NOT NULL DEFAULT 0;

Create View `vwStockReservado` AS 
	SELECT
		D.ArticuloId,
		SUM(D.Cantidad) AS Cantidad
	FROM
		Pedido P
		INNER JOIN PedidoDetalle D ON P.PedidoId = D.PedidoId
	WHERE
		P.FechaHoraAnulacion IS NULL 			/* NO ESTÉ ANULADO */
		AND P.FechaHoraConfirmacion IS NULL 	/* NO ESTÉ CONFIRMADO */
		AND FechaHoraFinSincronizado IS NULL 	/* Y NO ESTÉ SINCRONIZADO */
	GROUP BY
		D.ArticuloId;
        
INSERT INTO syncschedule (Hora, Rutina) VALUES ('06:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('06:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('06:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('07:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('07:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('07:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('08:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('08:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('08:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('09:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('09:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('09:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('10:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('10:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('10:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('11:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('11:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('11:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('12:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('12:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('12:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('13:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('13:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('13:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('14:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('14:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('14:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('15:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('15:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('15:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('16:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('16:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('16:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('17:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('17:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('17:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('18:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('18:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('18:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('19:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('19:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('19:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('20:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('20:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('20:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('21:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('21:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('21:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('22:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('22:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('22:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('23:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('23:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('23:42:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('00:02:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('00:22:00', 'PedidoRoutine');
INSERT INTO syncschedule (Hora, Rutina) VALUES ('00:42:00', 'PedidoRoutine');

ALTER TABLE Cliente ADD CondVtaId INT DEFAULT 0;
ALTER TABLE Pedido ADD CondVtaId INT DEFAULT 0;

ALTER TABLE Cliente ADD ListaPreciosId INT DEFAULT 0;
ALTER TABLE Pedido ADD ListaPreciosId INT DEFAULT 0;

/********** UPDATE 14/12/2019 **********/
ALTER TABLE Articulo ADD PrecioUnitario DECIMAL(12,4);
ALTER TABLE Articulo ADD UnidMin INT;
ALTER TABLE listaprecios DROP COLUMN Descripcion;
ALTER TABLE listaprecios DROP COLUMN Activa;
ALTER TABLE listaprecios ADD PKArticuloId INT;
ALTER TABLE listaprecios ADD Variacion DECIMAL(18,15);

/********** UPDATE 07/01/2020 **********/
ALTER TABLE Articulo ADD PrecioVentaPublico DECIMAL(12,4);
ALTER TABLE Articulo ADD PorcentajeIVA DECIMAL(12,4);
ALTER TABLE PedidoDetalle ADD PorcentajeIVA DECIMAL(12,4);
ALTER TABLE PedidoDetalle ADD PrecioUnitarioConDescuentoNeto DECIMAL(12,4);

/********** UPDATE 06/02/2020 ***********/
delete from pedidodetalle where pedidodetalleid > 0;
delete from pedido where pedidoid > 0;

alter table pedidodetalle add TienePVP bit(1);

/********** UPDATE 09/02/2020 **********/
create table ArticuloDestacado
(
	PKArticuloId int(11) not null primary key
);

/********** UPDATE 18/02/2020 **********/
ALTER TABLE Log ADD EjecucionId NVARCHAR(50);

/********** UPDATE 20/02/2020 **********/
DROP TABLE SyncSesion;
CREATE TABLE SyncSesion
(
	EjecucionId NVARCHAR(50) NOT NULL PRIMARY KEY,
    SesionIP NVARCHAR(50),
    FechaHoraInicio DATETIME,
    FechaHoraUltimaConexion DATETIME
);

ALTER TABLE SyncSesion ADD Aplicativo NVARCHAR(50);

DELIMITER $$
DROP PROCEDURE IF EXISTS `spSyncSesionRegistrarActividad` $$
CREATE PROCEDURE `spSyncSesionRegistrarActividad`(
	pEjecucionId NVARCHAR(50),
    pAplicativo NVARCHAR(50),
    pSesionIP NVARCHAR(50)
)
BEGIN

	DECLARE cant INT;
    
    SELECT
		COUNT(*)
	INTO
		cant
	FROM
		SyncSesion
	WHERE
		EjecucionId = pEjecucionId;
        
	IF cant = 0 THEN
		INSERT INTO SyncSesion (EjecucionId, Aplicativo, SesionIP, FechaHoraInicio, FechaHoraUltimaConexion) VALUES (pEjecucionId, pAplicativo, pSesionIP, NOW(), NOW());
    ELSE
		UPDATE SyncSesion SET FechaHoraUltimaConexion = NOW() WHERE EjecucionId = pEjecucionId;
	END IF;
    
END $$
DELIMITER ;

ALTER TABLE Log DROP COLUMN SesionClienteId;


/******** UPDATE 23/02/2020 ***********/
ALTER TABLE articulo ADD PrecCompra decimal(14,4);
ALTER TABLE articulo ADD UnidCompra decimal(6,2);
ALTER TABLE articulo ADD UnidMay smallint(4) unsigned;
ALTER TABLE articulo ADD NroAlicIVA tinyint(3);

ALTER TABLE PedidoDetalle ADD NroAlicIVA tinyint(3) NOT NULL DEFAULT 0;
ALTER TABLE PedidoDetalle ADD CostoUnitario DECIMAL(12,4) NOT NULL DEFAULT 0;
ALTER TABLE PedidoDetalle ADD ArticuloUnidMin smallint(4) unsigned not null default 1;



/******** UPDATE 26/02/2020 ***********/
ALTER TABLE Cliente ADD PorcentajeIIBB DECIMAL(18,2) DEFAULT 0;
ALTER TABLE Pedido ADD PorcentajeIIBB DECIMAL(18,2) DEFAULT 0;

/******** UPDATE 01/03/2020 ***********/
ALTER TABLE Cliente ADD ResponsableId INT;
ALTER TABLE Pedido ADD ResponsableId INT;

/******* UPDATE 16/07/2021 ***********/
CREATE TABLE Region
(
	RegionId INT NOT NULL AUTO_INCREMENT,
    Descripcion NVARCHAR(30),
    DeletedAt DATETIME,
    PRIMARY KEY (RegionId)
);

INSERT INTO Region (Descripcion) VALUES
	('INTERIOR'),
    ('PATAGONIA'),
    ('TIERRA DEL FUEGO'),
    ('CAPITAL - AMBA');
    
ALTER TABLE Cliente ADD RegionId INT;

CREATE TABLE RegionMontoMinimo
(
	RegionMontoMinimoId INT NOT NULL AUTO_INCREMENT,
    RegionId INT NOT NULL,
    DiaDeLaSemana INT NOT NULL,
    MontoMinimo DECIMAL(10, 2),
    DeletedAt DATETIME,
    PRIMARY KEY (RegionMontoMinimoId),
    FOREIGN KEY (RegionId) REFERENCES Region(RegionId)
);

ALTER TABLE Pedido ADD RegionMontoMinimoId INT;