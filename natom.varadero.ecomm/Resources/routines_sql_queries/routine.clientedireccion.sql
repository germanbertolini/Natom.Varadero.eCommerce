SELECT
	CAST(
		CONCAT(
			LPAD(dir.codsuc, 5, '0'),
            LPAD(dir.nrocli, 10, '0'),
            LPAD(dir.ordendir, 3, '0')
		) AS UNSIGNED INTEGER) AS EF_Id,
	CAST(
		CONCAT(
			RPAD(dir.codsuc, 5, '0'),
            RPAD(dir.nrocli, 10, '0'),
            RPAD(dir.ordendir, 3, '0')
		) AS DECIMAL(18,0)) AS ClienteDireccionId,
	dir.codsuc 			AS PKSucursalId,
    dir.nrocli 			AS PKClienteId,
    dir.ordendir		AS PKDireccionId,
    dir.direccion		AS Direccion,
    dir.codpostal		AS CodigoPostal,
    dir.telefono		AS Telefono
FROM
	clientesdir dir
WHERE	
	dir.activa = 1;