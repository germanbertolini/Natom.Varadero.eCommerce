-- Tarea 1: CREO NUEVA TABLA USUARIO
CREATE TABLE usuario(
Id int not null,
EF_Id int primary key auto_increment not null,
ClienteCUIT nvarchar(20) null,
Email nvarchar(50) null,
Clave nvarchar(50) null,
FechaHoraRegistracion datetime null,
FechaHoraConfirmacionEmail datetime null,
SecretConfirmacionEmail char(32) null,
FechaHoraBaja datetime null,
SesionIP nvarchar(50) null,
SesionAgent text null,
SesionToken char(32) null,
SesionInicio datetime null,
SesionUltimaAccion datetime null
);

-- Tarea 4: INSERT INTO Usuario SELECT CLIENTE
INSERT Usuario(Id,ClienteCUIT,Email,Clave,FechaHoraRegistracion,FechaHoraConfirmacionEmail,SecretConfirmacionEmail,FechaHoraBaja,
			   SesionIP,SesionAgent,SesionToken,SesionInicio,SesionUltimaAccion)
SELECT
	C.EF_Id,
	C.CUIT,
	C.UsuarioEmail,
	C.UsuarioClave,
	now(),
	now(),
	"",
    null,
	C.SesionIP,
	C.SesionAgent,
	C.SesionToken,
	C.SesionInicio,
	C.SesionUltimaAccion
FROM Cliente C;


-- Tarea 5: QUITO COLUMNAS DE LA TABLA "Articulo"
ALTER TABLE Articulo DROP COLUMN MarcaId;
ALTER TABLE Articulo DROP COLUMN RubroId;
ALTER TABLE Articulo DROP COLUMN SubRubroId;
ALTER TABLE Articulo DROP COLUMN GrupoId;
ALTER TABLE Articulo DROP COLUMN PKArticuloId;
ALTER TABLE Articulo DROP COLUMN UnidMin;
ALTER TABLE Articulo DROP COLUMN UnidMay;
ALTER TABLE Articulo DROP COLUMN NroAlicIVA;
ALTER TABLE Articulo DROP COLUMN PrecCompra;
ALTER TABLE Articulo DROP COLUMN UnidCompra;

ALTER TABLE Articulo MODIFY COLUMN ArticuloCodigo VARCHAR(20)  COLLATE utf8_unicode_ci;

-- Tarea 6: AGREGO TRES CAMPOS A LA TABLA "Articulo"
ALTER TABLE Articulo ADD Marca varchar(100) null;
ALTER TABLE Articulo ADD Rubro varchar(100) null;
ALTER TABLE Articulo ADD SubRubro varchar(100) null;

-- Tarea 7: QUITO COLUMNAS DE LA TABLA "Cliente"
CREATE TEMPORARY TABLE CopiaCliente(
	EF_Id int not null,
    CUIT nvarchar(200) null
);
insert CopiaCliente(EF_Id,CUIT) select C.EF_Id,C.CUIT from cliente C;


ALTER TABLE Cliente DROP COLUMN ClienteId;
ALTER TABLE Cliente DROP COLUMN CUIT;
ALTER TABLE Cliente DROP COLUMN PKSucursalId;
ALTER TABLE Cliente DROP COLUMN PKClienteId;
ALTER TABLE Cliente DROP COLUMN CodigoProvincia;
ALTER TABLE Cliente DROP COLUMN Moneda;
ALTER TABLE Cliente DROP COLUMN UsuarioAlias;
ALTER TABLE Cliente DROP COLUMN UsuarioClave;
ALTER TABLE Cliente DROP COLUMN UsuarioEmail;
ALTER TABLE Cliente DROP COLUMN CondVtaId;
ALTER TABLE Cliente DROP COLUMN PorcentajeIIBB;
ALTER TABLE Cliente DROP COLUMN ResponsableId;
-- Tarea 8: AGREGO COLUMNAS EN TABLA "Cliente"
ALTER TABLE Cliente ADD Activo bit not null default 1;		
ALTER TABLE Cliente ADD CUIT nvarchar(200) null;
ALTER TABLE Cliente ADD Codigo nvarchar(20) null;

UPDATE Cliente INNER JOIN CopiaCliente ON CopiaCliente.EF_Id = Cliente.EF_Id SET Cliente.CUIT = CopiaCliente.CUIT, Cliente.Codigo = CopiaCliente.CUIT;
drop table CopiaCliente;

-- Tarea 9: QUITO COLUMNAS EN TABLA "ClienteDireccion"
ALTER TABLE ClienteDireccion DROP COLUMN ClienteDireccionId;
ALTER TABLE ClienteDireccion DROP COLUMN PKSucursalId;
ALTER TABLE ClienteDireccion DROP COLUMN PKClienteId;
ALTER TABLE ClienteDireccion DROP COLUMN PKDireccionId;

-- TAREA 10: AGREGO COLUMNA A TABLA "ClienteDireccion"
ALTER TABLE ClienteDireccion ADD ClienteCUIT nvarchar(20) null;

-- Tarea 11: QUITO COLUMNAS EN TABLA "ListaPrecios"
ALTER TABLE ListaPrecios DROP COLUMN PKListaDePreciosId;
ALTER TABLE ListaPrecios DROP COLUMN PKArticuloId;
ALTER TABLE ListaPrecios DROP COLUMN Variacion;

-- Tarea 12: AGREGO COLUMNAS A TABLA "ListaPrecios"
ALTER TABLE ListaPrecios ADD ListaDePreciosId int null;
ALTER TABLE ListaPrecios ADD CodigoArticulo nvarchar(20) null;
ALTER TABLE ListaPrecios ADD PrecioNeto decimal(18,2) null;

-- Tarea 13: QUITO COLUMNAS DE LA TABLA "Pedido"
ALTER TABLE Pedido DROP COLUMN ClienteId;
ALTER TABLE Pedido DROP COLUMN SucursalId;
ALTER TABLE Pedido DROP COLUMN ResponsableId;
ALTER TABLE Pedido DROP COLUMN CondVtaId;
ALTER TABLE Pedido DROP COLUMN PorcentajeIIBB;

-- Tarea 14: AGREGO COLUMNAS A TABLA "Pedido"
ALTER TABLE Pedido ADD ClienteCodigo nvarchar(20) null;
ALTER TABLE Pedido ADD Moneda  int null;
ALTER TABLE Pedido ADD Cotizacion decimal(18,2) null;

-- Tarea 15: ELIMINO LA TABLA "ArticulosDestacados"
DROP TABLE ArticuloDestacado;

-- Tarea 16: CREO DE NUEVO TABLA "ArticuloDestacado"
CREATE TABLE ArticuloDestacado(
EF_Id int primary key auto_increment not null,
ArticuloCodigo nvarchar(20) not null COLLATE utf8_unicode_ci,
Desde datetime null
);

ALTER TABLE Articulo ADD TienePVP BIT(1) default 0;

ALTER TABLE ListaPrecios MODIFY COLUMN CodigoArticulo VARCHAR(20)  COLLATE utf8_unicode_ci;

ALTER VIEW `vwstockreservado` AS
    SELECT 
        `d`.`Codigo` AS `ArticuloCodigo`,
        SUM(`d`.`Cantidad`) AS `Cantidad`
    FROM
        (`wi142361_ecomm`.`pedido` `p`
        JOIN `wi142361_ecomm`.`pedidodetalle` `d` ON ((`p`.`PedidoId` = `d`.`PedidoId`)))
    WHERE
        (ISNULL(`p`.`FechaHoraAnulacion`)
            AND ISNULL(`p`.`FechaHoraConfirmacion`)
            AND ISNULL(`p`.`FechaHoraFinSincronizado`))
    GROUP BY `d`.`Codigo`
    
-- Tarea 17: ALTER DE STORE PROCEDURE
DELIMITER $$
DROP PROCEDURE IF EXISTS spArticuloDestacadoAgregar$$
CREATE PROCEDURE spArticuloDestacadoAgregar(IN artCodigo varchar(20))
BEGIN
INSERT INTO articuloCodigo(ArticuloCodigo,Desde) VALUES (artCodigo, NOW());
END$$
DELIMITER ;

UPDATE Usuario SET ClienteCUIT = '99999999995' WHERE ClienteCUIT = '12121231231' and EF_Id > 0;

