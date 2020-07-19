SELECT
	0					AS EF_Id,
	CONCAT(
			RPAD(sub.codsubr, 20, 'X'),
            RPAD(sub.codrubro, 20, 'X')) AS SubRubroId,
	sub.codsubr			AS PKSubRubroId,
    sub.codrubro		AS PKRubroId,
    sub.descripcion		AS Descripcion,
    sub.tipoart			AS TipoArticulo
FROM subrubros sub;