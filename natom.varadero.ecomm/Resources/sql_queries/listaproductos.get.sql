SELECT
	A.PKArticuloId AS Id,
    A.ArticuloCodigo AS Codigo,
    A.ArticuloNombre AS Nombre,
    A.ArticuloDescripcionAbreviada AS Descripcion,
    A.ArticuloStock - COALESCE(SR.Cantidad, 0) AS Stock,
    M.Descripcion As Marca,
    A.PrecioUnitario * ((COALESCE(L1.Variacion, L2.Variacion) / 100) + 1) * A.UnidMin / ((A.PorcentajeIVA / 100) + 1) AS PrecioNeto,
	A.PrecioUnitario * ((COALESCE(L1.Variacion, L2.Variacion) / 100) + 1) * A.UnidMin AS PrecioBruto,
    COALESCE(L1.Variacion, L2.Variacion)	AS PorcentajeDescuento,
	A.PorcentajeIVA,
    A.PrecioUnitario * ((COALESCE(L1.Variacion, L2.Variacion) / 100) + 1) * A.UnidMin / ((A.PorcentajeIVA / 100) + 1) AS PrecioConDescuentoNeto,
	A.PrecioUnitario * ((COALESCE(L1.Variacion, L2.Variacion) / 100) + 1) * A.UnidMin AS PrecioConDescuentoBruto,

	A.PrecioVentaPublico AS PrecioVentaPublico,
	CASE WHEN A.PrecioVentaPublico IS NULL THEN 0 ELSE
		(((((A.PrecioUnitario * ((COALESCE(L1.Variacion, L2.Variacion) / 100) + 1) * A.UnidMin) / A.PrecioVentaPublico) * 100) - 100) * -1)
	END AS PorcentajeDescRespectoPVP,

	CASE WHEN A.GrupoId != 3 THEN TRUE ELSE FALSE END AS TienePVP,
	CASE WHEN D.PKArticuloId IS NULL THEN FALSE ELSE TRUE END AS EsDestacado
FROM
	Articulo A
    INNER JOIN Marca M ON M.PKMarcaId = A.MarcaId
	LEFT JOIN ArticuloDestacado D ON D.PKArticuloId = A.PKArticuloId
    LEFT JOIN ListaPrecios L1 ON L1.PKArticuloId = A.PKArticuloId AND L1.PKListaDePreciosId = @@LISTADEPRECIOSID@@
    LEFT JOIN ListaPrecios L2 ON L2.PKArticuloId = 0 AND L2.PKListaDePreciosId = @@LISTADEPRECIOSID@@
	LEFT JOIN vwStockReservado SR ON SR.ArticuloId = A.PKArticuloId
WHERE
	1 = 1
    AND A.ArticuloActivo = 1
	AND A.preciounitario IS NOT NULL
	AND COALESCE(L1.Variacion, L2.Variacion) IS NOT NULL
	AND A.PrecioVentaPublico > 0
    /**[[-WHERE_SENTENCIES-]]**/
/**[[-ORDERBY_SENTENCIES-]]**/
/**[[-LIMIT_SENTENCIES-]]**/