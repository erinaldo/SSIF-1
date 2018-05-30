﻿EXEC dbo.SpAccountsToPay
	@ProviderCode = '',
	@ProviderName = '',
	@BusinessName = '',
	@Money = 'C'
GO

SELECT * FROM Compra ORDER BY Compra.N
GO

UPDATE Compra SET FECHACREDITOVENCIMIENTO = '2018-05-20 20:01:38.597'
WHERE N = 8
GO


/****** Object:  StoredProcedure [dbo].[SpProductosVendidos]    Script Date: 23/05/2018 17:00:21 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		MICHEL ROBERTO TRAÑA TABLADA
-- Create date: 23/05/2018
-- Description:	Retorna listado de clientes con su deuda actual
-- =============================================
CREATE PROCEDURE [dbo].[SpAccountsToPay]

	-- Add the parameters for the stored procedure here
	@ProviderCode AS VARCHAR(50),
	@ProviderName AS VARCHAR(100),
	@BusinessName AS VARCHAR(100),
	@Money AS CHAR(1)

AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Statements for procedure here
	SELECT
		res.ProviderCode,
		res.DNI,
		res.ProviderName,
		res.BusinessName,
		res.Phone,
		res.CreditTerm,
		res.CreditLimit,
		SUM(res.Billed) AS Billed,
		SUM(res.AmountExpired) AS AmountExpired,
		SUM(res.AmountAvailable) AS AmountAvailable
	FROM
		(
			(
				SELECT
					Proveedor.N_PROVEEDOR AS ProviderCode,
					Proveedor.IDENTIFICACION AS DNI,
					CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS) AS ProviderName,
					Proveedor.RAZONSOCIAL AS BusinessName,
					Proveedor.TELEFONO AS Phone,
					Proveedor.PLAZO AS CreditTerm,
					Proveedor.LIMITECREDITO AS CreditLimit,
					SUM(
						CASE @Money WHEN 'C' THEN
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO
							ELSE
								Compra.TAZACAMBIO * Compra.SALDOCREDITO
							END
						ELSE
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO / Compra.TAZACAMBIO
							ELSE
								Compra.SALDOCREDITO
							END
						END
					)
					AS
						Billed,
					0.0 AS AmountExpired,
					Proveedor.LIMITECREDITO -
					SUM(
						CASE @Money WHEN 'C' THEN
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO
							ELSE
								Compra.TAZACAMBIO * Compra.SALDOCREDITO
							END
						ELSE
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO / Compra.TAZACAMBIO
							ELSE
								Compra.SALDOCREDITO
							END
						END
					)
					AS
						AmountAvailable
				FROM
					Proveedor
					INNER JOIN Compra ON Compra.IDPROVEEDOR = Proveedor.IDPROVEEDOR
				WHERE
						Proveedor.N_PROVEEDOR LIKE @ProviderCode + '%'
					AND
						CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS) LIKE @ProviderName + '%'
					AND
						Proveedor.RAZONSOCIAL LIKE @BusinessName + '%'
				GROUP BY
					Proveedor.N_PROVEEDOR,
					Proveedor.IDENTIFICACION,
					CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS),
					Proveedor.RAZONSOCIAL,
					Proveedor.TELEFONO,
					Proveedor.PLAZO,
					Proveedor.LIMITECREDITO
			)
		UNION
			(
				SELECT
					Proveedor.N_PROVEEDOR AS ProviderCode,
					Proveedor.IDENTIFICACION AS DNI,
					CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS) AS ProviderName,
					Proveedor.RAZONSOCIAL AS BusinessName,
					Proveedor.TELEFONO AS Phone,
					Proveedor.PLAZO AS CreditTerm,
					Proveedor.LIMITECREDITO AS CreditLimit,
					0.0 AS Billed,
					SUM(
						CASE @Money WHEN 'C' THEN
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO
							ELSE
								Compra.TAZACAMBIO * Compra.SALDOCREDITO
							END
						ELSE
							CASE Compra.MONEDA WHEN 'C' THEN
								Compra.SALDOCREDITO / Compra.TAZACAMBIO
							ELSE
								Compra.SALDOCREDITO
							END
						END
					)
					AS
						AmountExpired,
					0.0 AS AmountAvailable
				FROM
					Proveedor
					INNER JOIN Compra ON Compra.IDPROVEEDOR = Proveedor.IDPROVEEDOR
				WHERE
						Proveedor.N_PROVEEDOR LIKE @ProviderCode + '%'
					AND
						CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS) LIKE @ProviderName + '%'
					AND
						Proveedor.RAZONSOCIAL LIKE @BusinessName + '%'
					AND
						Compra.CREDITO = 1
					AND
						Compra.FECHACREDITOVENCIMIENTO IS NOT NULL
					AND
						Compra.FECHACREDITOVENCIMIENTO < GETDATE()
				GROUP BY
					Proveedor.N_PROVEEDOR,
					Proveedor.IDENTIFICACION,
					CONCAT(Proveedor.NOMBRES, ' ', Proveedor.APELLIDOS),
					Proveedor.RAZONSOCIAL,
					Proveedor.TELEFONO,
					Proveedor.PLAZO,
					Proveedor.LIMITECREDITO
			)
		)
	AS
		res
	GROUP BY
		res.ProviderCode,
		res.DNI,
		res.ProviderName,
		res.BusinessName,
		res.Phone,
		res.CreditTerm,
		res.CreditLimit
	ORDER BY
		res.ProviderCode
	--FIN DEL STATEMENT

END