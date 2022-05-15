SELECT
    A.ArticuloCodigo AS Codigo,
    A.ArticuloNombre AS Nombre,
    A.ArticuloDescripcionAbreviada AS Descripcion,
    A.ArticuloStock - COALESCE(SR.Cantidad, 0) AS Stock,
    A.Marca As Marca,
    L1.PrecioNeto AS PrecioNeto,
	L1.PrecioNeto * ((A.PorcentajeIVA / 100) + 1) AS PrecioBruto,
    0 AS PorcentajeDescuento,
	COALESCE(A.PorcentajeIVA, 0) AS PorcentajeIVA,
    COALESCE(L1.PrecioNeto, 0) AS PrecioConDescuentoNeto,
	COALESCE(L1.PrecioNeto * ((A.PorcentajeIVA / 100) + 1), 0) AS PrecioConDescuentoBruto,

	COALESCE(A.PrecioVentaPublico, 0) AS PrecioVentaPublico,
	CASE WHEN A.PrecioVentaPublico IS NULL THEN 0 ELSE
		COALESCE((((((L1.PrecioNeto * ((A.PorcentajeIVA / 100) + 1)) / A.PrecioVentaPublico) * 100) - 100) * -1), 0)
	END AS PorcentajeDescRespectoPVP,

	A.TienePVP AS TienePVP,
	CASE WHEN D.ArticuloCodigo IS NULL THEN FALSE ELSE TRUE END AS EsDestacado
FROM
	Articulo A
	LEFT JOIN ArticuloDestacado D ON D.ArticuloCodigo = A.ArticuloCodigo
    LEFT JOIN ListaPrecios L1 ON L1.CodigoArticulo = A.ArticuloCodigo AND L1.ListaDePreciosId = @@LISTADEPRECIOSID@@
	LEFT JOIN vwStockReservado SR ON SR.ArticuloCodigo = A.ArticuloCodigo
WHERE
	1 = 1
	AND L1.PrecioNeto IS NOT NULL
    AND A.ArticuloActivo = 1
	AND A.PrecioVentaPublico > 0
    /**[[-WHERE_SENTENCIES-]]**/
	/**[[-ORDERBY_SENTENCIES-]]**/
	/**[[-LIMIT_SENTENCIES-]]**/