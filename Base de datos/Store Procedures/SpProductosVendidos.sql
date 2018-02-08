/****** Object:  StoredProcedure [dbo].[SpProductosVendidos]    Script Date: 24/06/2017 11:28:21 a.m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		MICHEL ROBERTO TRAÑA TABLADA
-- Create date: 12/07/2016
-- Description:	ESTE PROCEDIMIENTO ALMACENADO PERMITE SELECCIONAR EL LISTADO DE ProductoS VENDIDOS EN UN RANGO DE TIEMPO
-- =============================================
ALTER PROCEDURE [dbo].[SpProductosVendidos]
	-- Add the parameters for the stored procedure here
@Inicio AS DATETIME,
@Final AS DATETIME,
@IDBodega AS VARCHAR(36),
@IDSerie AS VARCHAR(36),
@NEmpleado AS VARCHAR(50),
@Empleado AS VARCHAR(100),
@NCliente AS VARCHAR(50),
@Cliente AS VARCHAR(100),
@TipoVenta AS INTEGER,
@MonInv AS Bit,
@Moneda AS Bit,
@Taza AS DECIMAL
AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--DECLARAR VARIABLES DE TAZA DE CAMBIO
	DECLARE @TazaCordoba AS DECIMAL(18,4)
	DECLARE @TazaDolar AS DECIMAL(18,4)
	IF @Moneda = 1
		BEGIN
			SET @TazaCordoba = 1
			SET @TazaDolar = @Taza
		END
	ELSE
		BEGIN
			SET @TazaDolar = 1
			SET @TazaCordoba = @Taza
		END
	-- 1. SELECCIONAR TODOS LOS DATOS
	IF @TipoVenta <> 1 AND @TipoVenta <> 2
	BEGIN
		SELECT
			
			res.IDALTERNO AS IDAlterno,

			res.DESCRIPCION AS Descripcion,

			SUM(res.Cantidad)  AS Cantidad,

			SUM(res.DetalleCostoTotal) / SUM(res.Cantidad) AS CostoPromedio,

			SUM(res.DetalleCostoTotal) AS CostoTotal,

			SUM(res.SubTotal) / SUM(res.Cantidad) AS PrecioPromedio,

			SUM(res.SubTotal)  AS SubTotal,

			SUM(res.Descuento)  AS Descuento,

			SUM(res.Iva)  AS Iva,

			SUM(res.Total)  AS Total,

			SUM(res.SubTotal) - SUM(res.DetalleCostoTotal)  AS Utilidad

		FROM (
			(SELECT
				
				NEWID() AS Id,

				Producto.IDALTERNO,

				Producto.DESCRIPCION,
				
				SUM
					((CASE @Moneda WHEN 1 THEN
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO
						ELSE
							VentaDetalle.COSTO * Venta.TazaCambio
						END)
					 ELSE
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO / Venta.TazaCambio
						ELSE
							VentaDetalle.COSTO
						END)
					 END)
					 *
					 VentaDetalle.CANTIDAD)
				AS DetalleCostoTotal,
				
				SUM(VentaDetalle.CANTIDAD) AS Cantidad,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.DESCUENTO_DIN_TOTAL_C ELSE VentaDetalle.DESCUENTO_DIN_TOTAL_D END) AS Descuento,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.SUBTOTAL_C ELSE VentaDetalle.SUBTOTAL_D END) AS SubTotal,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.IVA_DIN_TOTAL_C ELSE VentaDetalle.IVA_DIN_TOTAL_D END) AS Iva,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.TOTAL_C ELSE VentaDetalle.TOTAL_D END) AS Total
				
			FROM
				Producto
				INNER JOIN Existencia ON Producto.IDProducto = Existencia.IDProducto
				INNER JOIN VentaDetalle ON Existencia.IDExistencia = VentaDetalle.IDExistencia
				INNER JOIN Venta ON VentaDetalle.IDVenta = Venta.IDVenta
				INNER JOIN Empleado ON Venta.IDEmpleado = Empleado.IDEmpleado
				INNER JOIN Cliente ON Venta.IDCliente = Cliente.IDCliente
				INNER JOIN Serie ON Venta.IDSerie = Serie.IDSerie
				INNER JOIN Bodega ON Serie.IDBodega = Bodega.IDBodega
			WHERE
				Venta.ANULADO = 'N'
				AND Venta.FECHAFACTURA >= @INICIO
				AND Venta.FECHAFACTURA <= @FINAL
				AND Bodega.IDBodega LIKE (@IDBodega + '%')
				AND Serie.IDSerie LIKE (@IDSerie + '%')
				AND Empleado.N_TRABAJADOR LIKE (@NEmpleado + '%')
				AND (Empleado.NOMBRES + ' ' + Empleado.APELLIDOS) LIKE (@Empleado + '%')
				AND Cliente.N_Cliente LIKE (@NCliente + '%')
				AND (Cliente.NOMBRES + ' ' + Cliente.APELLIDOS) LIKE (@Cliente + '%')
			GROUP BY
				Producto.IDALTERNO,
				Producto.DESCRIPCION
			HAVING
				SUM(VentaDetalle.CANTIDAD) > 0
			)
		UNION
			(SELECT

				NEWID() AS Id,

				Producto.IDALTERNO,

				Producto.DESCRIPCION,
				
				SUM
					((CASE @Moneda WHEN 1 THEN
						(CASE @MonInv WHEN 1 THEN 
							VentaDetalle.COSTO
						ELSE 
							VentaDetalle.COSTO * Venta.TazaCambio
						END)
					 ELSE 
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO / Venta.TazaCambio
						ELSE
							VentaDetalle.COSTO
						END)
					 END)
					 *
					 VentaDetalle.CANTIDAD)
				AS DetalleCostoTotal,
				
				SUM(VentaDetalle.CANTIDAD) AS Cantidad,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.DESCUENTO_DIN_TOTAL_C ELSE VentaDetalle.DESCUENTO_DIN_TOTAL_D END) AS Descuento,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.SUBTOTAL_C ELSE VentaDetalle.SUBTOTAL_D END) AS SubTotal,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.IVA_DIN_TOTAL_C ELSE VentaDetalle.IVA_DIN_TOTAL_D END) AS Iva,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.TOTAL_C ELSE VentaDetalle.TOTAL_D END) AS Total

			FROM
				Producto
				INNER JOIN Existencia ON Producto.IDProducto = Existencia.IDProducto
				INNER JOIN VentaDetalle ON Existencia.IDExistencia = VentaDetalle.IDExistencia
				INNER JOIN Venta ON VentaDetalle.IDVenta = Venta.IDVenta
				INNER JOIN Empleado ON Venta.IDEmpleado = Empleado.IDEmpleado
				INNER JOIN Serie ON Venta.IDSerie = Serie.IDSerie
				INNER JOIN Bodega ON Serie.IDBodega = Bodega.IDBodega
			WHERE
				Venta.ANULADO = 'N'
				AND Venta.IDCliente IS NULL
				AND Venta.FECHAFACTURA >= @INICIO
				AND Venta.FECHAFACTURA <= @FINAL
				AND Bodega.IDBodega LIKE (@IDBodega + '%')
				AND Serie.IDSerie LIKE (@IDSerie + '%')
				AND Empleado.N_TRABAJADOR LIKE (@NEmpleado + '%')
				AND (Empleado.NOMBRES + ' ' + Empleado.APELLIDOS) LIKE (@Empleado + '%')
				AND RTRIM(@NCliente) = ('')
				AND (Venta.ClienteCONTADO) LIKE (@Cliente + '%')
			GROUP BY
				Producto.IDALTERNO,
				Producto.DESCRIPCION
			HAVING
				SUM(VentaDetalle.CANTIDAD) > 0
			)
		)
		AS
			res
		GROUP BY
			res.IDALTERNO, res.DESCRIPCION
		ORDER BY
			res.IDALTERNO ASC
	END






	--2. SELECCIONAR VentaS DE CONTADO
	IF @TipoVenta = 1
	BEGIN
		SELECT
			
			res.IDALTERNO AS IDAlterno,

			res.DESCRIPCION AS Descripcion,

			SUM(res.Cantidad)  AS Cantidad,

			SUM(res.DetalleCostoTotal) / SUM(res.Cantidad) AS CostoPromedio,

			SUM(res.DetalleCostoTotal) AS CostoTotal,

			SUM(res.SubTotal) / SUM(res.Cantidad) AS PrecioPromedio,

			SUM(res.SubTotal)  AS SubTotal,

			SUM(res.Descuento)  AS Descuento,

			SUM(res.Iva)  AS Iva,

			SUM(res.Total)  AS Total,

			SUM(res.SubTotal) - SUM(res.DetalleCostoTotal)  AS Utilidad

		FROM (
			(SELECT
				NEWID() AS Id,

				Producto.IDALTERNO,

				Producto.DESCRIPCION,
				
				SUM
					((CASE @Moneda WHEN 1 THEN
						(CASE @MonInv WHEN 1 THEN 
							VentaDetalle.COSTO
						ELSE 
							VentaDetalle.COSTO * Venta.TazaCambio
						END)
					 ELSE 
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO / Venta.TazaCambio
						ELSE
							VentaDetalle.COSTO
						END)
					 END)
					 *
					 VentaDetalle.CANTIDAD)
				AS DetalleCostoTotal,
				
				SUM(VentaDetalle.CANTIDAD) AS Cantidad,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.DESCUENTO_DIN_TOTAL_C ELSE VentaDetalle.DESCUENTO_DIN_TOTAL_D END) AS Descuento,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.SUBTOTAL_C ELSE VentaDetalle.SUBTOTAL_D END) AS SubTotal,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.IVA_DIN_TOTAL_C ELSE VentaDetalle.IVA_DIN_TOTAL_D END) AS Iva,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.TOTAL_C ELSE VentaDetalle.TOTAL_D END) AS Total
			FROM
				Producto
				INNER JOIN Existencia ON Producto.IDProducto = Existencia.IDProducto
				INNER JOIN VentaDetalle ON Existencia.IDExistencia = VentaDetalle.IDExistencia
				INNER JOIN Venta ON VentaDetalle.IDVenta = Venta.IDVenta
				INNER JOIN Empleado ON Venta.IDEmpleado = Empleado.IDEmpleado
				INNER JOIN Cliente ON Venta.IDCliente = Cliente.IDCliente
				INNER JOIN Serie ON Venta.IDSerie = Serie.IDSerie
				INNER JOIN Bodega ON Serie.IDBodega = Bodega.IDBodega
			WHERE
				Venta.ANULADO = 'N'
				AND Venta.FECHAFACTURA >= @INICIO
				AND Venta.FECHAFACTURA <= @FINAL
				AND Bodega.IDBodega LIKE (@IDBodega + '%')
				AND Serie.IDSerie LIKE (@IDSerie + '%')
				AND Empleado.N_TRABAJADOR LIKE (@NEmpleado + '%')
				AND (Empleado.NOMBRES + ' ' + Empleado.APELLIDOS) LIKE (@Empleado + '%')
				AND Cliente.N_Cliente LIKE (@NCliente + '%')
				AND (Cliente.NOMBRES + ' ' + Cliente.APELLIDOS) LIKE (@Cliente + '%')
				AND Venta.CREDITO = 0
			GROUP BY
				Producto.IDALTERNO,
				Producto.DESCRIPCION
			HAVING
				SUM(VentaDetalle.CANTIDAD) > 0
			)
		UNION
			(SELECT
				
				NEWID() AS Id,

				Producto.IDALTERNO,

				Producto.DESCRIPCION,
				
				SUM
					(CASE @Moneda WHEN 1 THEN
						(CASE @MonInv WHEN 1 THEN 
							VentaDetalle.COSTO
						ELSE 
							VentaDetalle.COSTO * Venta.TazaCambio
						END)
					 ELSE 
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO / Venta.TazaCambio
						ELSE
							VentaDetalle.COSTO
						END)
					 END)
				AS DetalleCostoTotal,
				
				SUM(VentaDetalle.CANTIDAD) AS Cantidad,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.DESCUENTO_DIN_TOTAL_C ELSE VentaDetalle.DESCUENTO_DIN_TOTAL_D END) AS Descuento,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.SUBTOTAL_C ELSE VentaDetalle.SUBTOTAL_D END) AS SubTotal,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.IVA_DIN_TOTAL_C ELSE VentaDetalle.IVA_DIN_TOTAL_D END) AS Iva,
				
				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.TOTAL_C ELSE VentaDetalle.TOTAL_D END) AS Total

			FROM
				Producto
				INNER JOIN Existencia ON Producto.IDProducto = Existencia.IDProducto
				INNER JOIN VentaDetalle ON Existencia.IDExistencia = VentaDetalle.IDExistencia
				INNER JOIN Venta ON VentaDetalle.IDVenta = Venta.IDVenta
				INNER JOIN Empleado ON Venta.IDEmpleado = Empleado.IDEmpleado
				INNER JOIN Serie ON Venta.IDSerie = Serie.IDSerie
				INNER JOIN Bodega ON Serie.IDBodega = Bodega.IDBodega
			WHERE
				Venta.ANULADO = 'N'
				AND Venta.IDCliente IS NULL
				AND Venta.FECHAFACTURA >= @INICIO
				AND Venta.FECHAFACTURA <= @FINAL
				AND Bodega.IDBodega LIKE (@IDBodega + '%')
				AND Serie.IDSerie LIKE (@IDSerie + '%')
				AND Empleado.N_TRABAJADOR LIKE (@NEmpleado + '%')
				AND (Empleado.NOMBRES + ' ' + Empleado.APELLIDOS) LIKE (@Empleado + '%')
				AND RTRIM(@NCliente) = ('')
				AND (Venta.ClienteCONTADO) LIKE (@Cliente + '%')
			GROUP BY
				Producto.IDALTERNO,
				Producto.DESCRIPCION
			HAVING
				SUM(VentaDetalle.CANTIDAD) > 0
			)
		)
		AS
			res
		GROUP BY
			res.IDALTERNO, res.DESCRIPCION
		ORDER BY
			res.IDALTERNO ASC
	END






	--3. SELECCIONAR Ventas DE CREDITO
	IF @TipoVenta = 2
	BEGIN
		SELECT
			
			res.IDALTERNO AS IDAlterno,

			res.DESCRIPCION AS Descripcion,

			SUM(res.Cantidad)  AS Cantidad,

			SUM(res.DetalleCostoTotal) / SUM(res.Cantidad)  AS CostoPromedio,

			SUM(res.DetalleCostoTotal) AS CostoTotal,

			SUM(res.SubTotal) / SUM(res.Cantidad) AS PrecioPromedio,

			SUM(res.SubTotal)  AS SubTotal,

			SUM(res.Descuento)  AS Descuento,

			SUM(res.Iva)  AS Iva,

			SUM(res.Total)  AS Total,

			SUM(res.SubTotal) - SUM(res.DetalleCostoTotal)  AS Utilidad

		FROM (
			(SELECT 
				
				NEWID() AS Id,
				
				Producto.IDALTERNO,
				
				Producto.DESCRIPCION,
				
				SUM
					((CASE @Moneda WHEN 1 THEN
						(CASE @MonInv WHEN 1 THEN 
							VentaDetalle.COSTO
						ELSE 
							VentaDetalle.COSTO * Venta.TazaCambio
						END)
					ELSE 
						(CASE @MonInv WHEN 1 THEN
							VentaDetalle.COSTO / Venta.TazaCambio
						ELSE
							VentaDetalle.COSTO
						END)
					END)
					*
					VentaDetalle.CANTIDAD)
				AS DetalleCostoTotal,

				SUM(CASE Venta.MONEDA WHEN 'C' THEN VentaDetalle.SUBTOTAL_C / @TazaCordoba ELSE VentaDetalle.SUBTOTAL_D * @TazaDolar END) / SUM(VentaDetalle.CANTIDAD) AS PrecioPromedio,

				SUM(VentaDetalle.CANTIDAD) AS Cantidad,

				SUM(VentaDetalle.COSTO * VentaDetalle.CANTIDAD) AS Costo_Total,

				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.DESCUENTO_DIN_TOTAL_C ELSE VentaDetalle.DESCUENTO_DIN_TOTAL_D END) AS Descuento,

				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.SUBTOTAL_C ELSE VentaDetalle.SUBTOTAL_D END) AS SubTotal,

				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.IVA_DIN_TOTAL_C ELSE VentaDetalle.IVA_DIN_TOTAL_D END) AS Iva,

				SUM(CASE @Moneda WHEN 1 THEN VentaDetalle.TOTAL_C ELSE VentaDetalle.TOTAL_D END) AS Total

			FROM
				Producto
				INNER JOIN Existencia ON Producto.IDProducto = Existencia.IDProducto
				INNER JOIN VentaDetalle ON Existencia.IDExistencia = VentaDetalle.IDExistencia
				INNER JOIN Venta ON VentaDetalle.IDVenta = Venta.IDVenta
				INNER JOIN Empleado ON Venta.IDEmpleado = Empleado.IDEmpleado
				INNER JOIN Cliente ON Venta.IDCliente = Cliente.IDCliente
				INNER JOIN Serie ON Venta.IDSerie = Serie.IDSerie
				INNER JOIN Bodega ON Serie.IDBodega = Bodega.IDBodega
			WHERE
				Venta.ANULADO = 'N'
				AND Venta.FECHAFACTURA >= @INICIO
				AND Venta.FECHAFACTURA <= @FINAL
				AND Bodega.IDBodega LIKE (@IDBodega + '%')
				AND Serie.IDSerie LIKE (@IDSerie + '%')
				AND Empleado.N_TRABAJADOR LIKE (@NEmpleado + '%')
				AND (Empleado.NOMBRES + ' ' + Empleado.APELLIDOS) LIKE (@Empleado + '%')
				AND Cliente.N_Cliente LIKE (@NCliente + '%')
				AND (Cliente.NOMBRES + ' ' + Cliente.APELLIDOS) LIKE (@Cliente + '%')
				AND Venta.CREDITO = 1
			GROUP BY
				Producto.IDALTERNO,
				Producto.DESCRIPCION
			HAVING
				SUM(VentaDetalle.CANTIDAD) > 0
			)
		)
		AS
			res
		GROUP BY
			res.IDALTERNO, res.DESCRIPCION
		ORDER BY
			res.IDALTERNO ASC
	END
	--FIN DEL STATEMENT


END