SELECT
	0								AS EF_Id,
	0								AS ClienteId,
	1 								AS PKSucursalId,
    0 								AS PKClienteId,
    12121231231 					AS CUIT,
    'Razon social testing'			AS RazonSocial,
    'Fantasia testing' 				AS NombreFantasia,
    'CABA'							AS CodigoProvincia,
    'PES'							AS Moneda,
    
    'testing'						AS UsuarioAlias,
    '1234'							AS UsuarioClave,
    'german.bertolini@gmail.com'	AS UsuarioEmail,

	11								AS CondVtaId,
	23								AS ListaPreciosId,
    
    10000							AS LimiteDeCredito,
    500								AS SaldoEnCtaCte,

	NULL							AS SesionIP,
	NULL							AS SesionAgent,
	NULL							AS SesionToken,

	NULL							AS SesionInicio,
	NULL							AS SesionUltimaAccion,
    
    1								AS PorcentajeIIBB,
	1								AS ResponsableId
    
UNION

SELECT
	0					AS EF_Id,
	CAST(
		CONCAT(
			RPAD(cli.codsuc, 7, '0'),
            RPAD(cli.nrocli, 11, '0')
		) AS DECIMAL(18,0)) AS ClienteId,
	cli.codsuc 			AS PKSucursalId,
    cli.nrocli 			AS PKClienteId,
    cli.cuit			AS CUIT,
    cli.razonsoc 		AS RazonSocial,
    cli.nombrefant 		AS NombreFantasia,
    cli.codextprov		AS CodigoProvincia,
    cli.codmoneda		AS Moneda,
    
    cli.weblogusr		AS UsuarioAlias,
    cli.weblogpas		AS UsuarioClave,
    cli.webdiremail		AS UsuarioEmail,

	cli.condvta			AS CondVtaId,
	nrolistpre			AS ListaPreciosId,
    
    cli.limcred			AS LimiteDeCredito,
    cli.saldoencta		AS SaldoEnCtaCte,

	NULL				AS SesionIP,
	NULL				AS SesionAgent,
	NULL				AS SesionToken,

	NULL				AS SesionInicio,
	NULL				AS SesionUltimaAccion,
    
    COALESCE(iibb.alicuota, 0)	AS PorcentajeIIBB,
	cli.nrorespcta				AS ResponsableId
FROM
	clientes cli
    LEFT JOIN clientesperciib iibb ON iibb.nrocli = cli.nrocli AND iibb.codjurisdiccion = 901
WHERE
	cli.weblogusr IS NOT NULL
    AND LENGTH(TRIM(cli.weblogusr)) > 0
    AND cli.Activo = 1;