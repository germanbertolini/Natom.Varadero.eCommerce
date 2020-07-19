	SELECT
		CAST( CONCAT(LPAD(lp.nrolistpre, 6, '0'), LPAD(lp.nroart, 12, '0') ) AS UNSIGNED INTEGER)	AS EF_Id,
		lp.nrolistpre																				AS PKListaDePreciosId,
		lp.nroart																					AS PKArticuloId,
		lp.variacion																				AS Variacion
	FROM
		listaspreciosart lp
UNION ALL
	SELECT
		CAST( CONCAT(LPAD(lp2.nrolistpre, 6, '0'), LPAD(0, 12, '0') ) AS UNSIGNED INTEGER)			AS EF_Id,
		lp2.nrolistpre																				AS PKListaDePreciosId,
		0																							AS PKArticuloId,
		lp2.variacion																				AS Variacion
	FROM
		listasprecios lp2