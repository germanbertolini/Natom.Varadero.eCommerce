SELECT
	0					AS EF_Id,
	art.nroart 			AS PKArticuloId,
	art.codart 			AS ArticuloCodigo,
    art.nombrecomercial AS ArticuloNombre,
    art.descrweb 		AS ArticuloDescripcion,
    art.descrabrev		AS ArticuloDescripcionAbreviada,
    art.stockact 		AS ArticuloStock,
    art.activo 			AS ArticuloActivo,
    art.codmarca		AS MarcaId,
    art.codrubro		AS RubroId,
    art.codsubr			AS SubRubroId,
    art.nrogrupo		AS GrupoId,
	art.costoun			AS PrecioUnitario,
	art.unidmin			AS UnidMin,
	art.unidmay			AS UnidMay,
	art.preccompra		AS PrecCompra,
	art.unidcompra		AS UnidCompra,
	CASE WHEN art.nrogrupo = 3 THEN
		art.preccompra * art.unidmin * 4 ###NULL	### HOSPITALARIO ###
	ELSE
		art.preccompra * art.unidmin
	END					AS PrecioVentaPublico,
	art.nroaliciva		AS NroAlicIVA,
	iva.aliciva			AS PorcentajeIVA
FROM articulos art
	INNER JOIN aliciva iva ON iva.nroaliciva = art.nroaliciva
where
	UPPER(art.codmarca) != 'GENERICA'		### SE QUITAN LOS DE MARCA 'GENÉRICA' ###
	AND UPPER(art.codrubro) != 'CABA'	### QUE EL RUBRO NO SEA 'CABA' ###
	AND art.nrogrupo IN (2, 3) ### 2 = FARMACIA ### 3 = HOSPITALARIO ###