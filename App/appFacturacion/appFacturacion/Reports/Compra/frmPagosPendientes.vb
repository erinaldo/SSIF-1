﻿Imports Sadara.Models.V1.Database
Imports Sadara.Models.V1.POCO

Public Class frmPagosPendientes

    Public IdProveedor As String
    Public Moneda As String
    Public Frm As Integer = 0
    Public Taza As Decimal

    Sub Llenar()
        Try
            Using db As New CodeFirst
                Dim p = db.Proveedores.Where(Function(f) f.IDPROVEEDOR.Equals(Me.IdProveedor) And f.ACTIVO.Equals("S")).FirstOrDefault
                If Not p Is Nothing Then
                    Dim query = (From com In db.Compras Join pro In db.Proveedores On com.IDPROVEEDOR Equals pro.IDPROVEEDOR Join ser In db.Series On com.IDSERIE Equals ser.IDSERIE Where com.ANULADO = "N" And pro.ACTIVO = "S" And ser.ACTIVO = "S" And com.CREDITO = True And com.SALDOCREDITO > 0.0 And com.IDPROVEEDOR = IdProveedor Select com.IDCOMPRA, ser.NOMBRE, com.CONSECUTIVO, com.N_COMPRA, com.FECHACOMPRA, com.FECHACREDITOVENCIMIENTO, MONEDA = If(com.MONEDA.Equals(Config.cordoba), "Córdoba", "Dólar"), com.TAZACAMBIO, TOTAL = If(com.MONEDA.Equals(Config.cordoba), com.TOTAL_C, com.TOTAL_D), com.SALDOCREDITO Order By FECHACOMPRA Ascending).ToList()
                    dtRegistro.DataSource = query

                    'llenar textbox
                    txtMoneda.Text = If(p.MONEDA.Equals(Config.cordoba), "Córdoba", "Dólar")
                    txtCordoba.Text = query.Where(Function(f) f.MONEDA.Equals("Córdoba")).Sum(Function(f) f.TOTAL).ToString(Config.f_m)
                    txtDolar.Text = query.Where(Function(f) f.MONEDA.Equals("Dólar")).Sum(Function(f) f.TOTAL).ToString(Config.f_m)
                    txtSaldoCordoba.Text = query.Where(Function(f) f.MONEDA.Equals("Córdoba")).Sum(Function(f) f.SALDOCREDITO).ToString(Config.f_m_r)
                    txtSaldoDolar.Text = query.Where(Function(f) f.MONEDA.Equals("Dólar")).Sum(Function(f) f.SALDOCREDITO).ToString(Config.f_m_r)

                    If dtRegistro.Columns.Count > 0 Then
                        With dtRegistro
                            .DefaultCellStyle.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Regular)
                            .Columns(0).Visible = False
                            .Columns(1).HeaderText = vbNewLine & "Serie" & vbNewLine : .Columns(1).Width = 120
                            .Columns(2).HeaderText = "N° Registro" : .Columns(2).Width = 120
                            .Columns(3).HeaderText = "N° Compra" : .Columns(3).Width = 120
                            .Columns(4).HeaderText = "Fecha" : .Columns(4).Width = 120
                            .Columns(5).HeaderText = "Vencimiento" : .Columns(5).Width = 120
                            .Columns(6).HeaderText = "Moneda" : .Columns(6).Width = 120
                            .Columns(7).HeaderText = "T. Cambio" : .Columns(7).Width = 120 : .Columns(7).DefaultCellStyle.Format = Config.f_m
                            .Columns(8).HeaderText = "Total" : .Columns(8).Width = 120 : .Columns(8).DefaultCellStyle.Format = Config.f_m
                            .Columns(9).HeaderText = "Saldo" : .Columns(9).Width = 120 : .Columns(9).DefaultCellStyle.Format = Config.f_m
                            For Each c As DataGridViewColumn In .Columns
                                c.HeaderText = c.HeaderText.ToUpper
                                'c.HeaderCell.Style.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Bold)
                            Next
                        End With
                    End If
                    query = Nothing

                    'destruir
                    p = Nothing
                Else
                    MessageBox.Show("Este 'Proveedor' no se encuentra. Probablemente ha sido eliminado.")
                    Me.Close()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error, " & ex.Message)
        End Try
    End Sub

    Private Sub frmDocumentosPendientes_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Log.Instance.RegisterActivity(
            If(Config.currentBusiness IsNot Nothing, Config.currentBusiness.IdEmpresa, Guid.Empty),
            "PurchaseDocumentPending",
            "Load",
            "Load PurchaseDocumentPending",
            userId:=If(Config.currentUser IsNot Nothing, Guid.Parse(Config.currentUser.IDUsuario), Nothing)
        )

        Llenar()
        If dtRegistro.Rows.Count > 0 Then
            dtRegistro.Focus()
            dtRegistro.Rows(0).Selected = True
        End If
    End Sub

    Private Sub frmDocumentosPendientes_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Me.Dispose()
    End Sub

    Private Sub dtRegistro_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dtRegistro.CellDoubleClick
        Try
            If dtRegistro.SelectedRows.Count > 0 Then
                Using db As New CodeFirst
                    Dim Id = dtRegistro.SelectedRows(0).Cells(0).Value.ToString
                    Dim v = db.Compras.Where(Function(f) f.IDCOMPRA = Id And f.ANULADO.Equals("N")).FirstOrDefault
                    Id = Nothing
                    If Not v Is Nothing Then
                        If v.SALDOCREDITO > 0.0 Then
                            Select Case Me.Frm
                                Case 0
                                    With frmReciboCompra
                                        .txtIdFactura.Text = v.IDCOMPRA
                                        .txtFactura.Text = v.Serie.NOMBRE & " | " & v.CONSECUTIVO & "(" & v.N_COMPRA & ")"
                                        .txtMonto.Value = If(Moneda.Equals(Config.cordoba), If(v.MONEDA.Equals(Config.cordoba), v.SALDOCREDITO, v.SALDOCREDITO * Config.exchangeRate), If(v.MONEDA.Equals(Config.cordoba), v.SALDOCREDITO / Config.exchangeRate, v.SALDOCREDITO))
                                    End With
                                Case 1
                                    With frmNotaDevolucionCompra
                                        .txtIdFactura.Text = v.IDCOMPRA
                                        .txtFactura.Text = v.Serie.NOMBRE & " | " & v.CONSECUTIVO
                                        .txtSaldo.Value = If(Moneda.Equals(Config.cordoba), If(v.MONEDA.Equals(Config.cordoba), v.SALDOCREDITO, v.SALDOCREDITO * Config.exchangeRate), If(v.MONEDA.Equals(Config.cordoba), v.SALDOCREDITO / Config.exchangeRate, v.SALDOCREDITO))

                                        'Cargar detalle de la factura
                                        Dim item As LST_DETALLE_DEVOLUCION_COMPRA
                                        .detalles.RemoveAll(Function(f) True)
                                        For Each d In v.ComprasDetalles
                                            item = New LST_DETALLE_DEVOLUCION_COMPRA
                                            item.IDEXISTENCIA = d.Existencia.IDEXISTENCIA
                                            item.IDALTERNO = d.Existencia.Producto.IDALTERNO
                                            item.DESCRIPCION = d.Existencia.Producto.DESCRIPCION
                                            item.IVA = d.Existencia.Producto.IVA
                                            item.MARCA = If(d.Existencia.Producto.Marca.ACTIVO.Equals(Config.vTrue), d.Existencia.Producto.Marca.DESCRIPCION, Config.textNull)
                                            item.UNIDAD_DE_MEDIDA = If(d.Existencia.Producto.UnidadMedida.ACTIVO = "S", d.Existencia.Producto.UnidadMedida.DESCRIPCION, Config.textNull)
                                            item.PRESENTACION = If(d.Existencia.Producto.Presentacion.ACTIVO = "S", d.Existencia.Producto.Presentacion.DESCRIPCION, Config.textNull)
                                            item.EXISTENCIA = d.Existencia.CANTIDAD
                                            item.CANTIDAD = d.CANTIDAD
                                            If v.TAZACAMBIO = Me.Taza Then
                                                item.PRECIOUNITARIO_C = d.PRECIOUNITARIO_C : item.PRECIOUNITARIO_D = d.PRECIOUNITARIO_D
                                                item.DESCUENTO_POR = d.DESCUENTO_POR
                                                item.DESCUENTO_DIN_C = d.DESCUENTO_DIN_C : item.DESCUENTO_DIN_D = d.DESCUENTO_DIN_D
                                                item.DESCUENTO_DIN_TOTAL_C = d.DESCUENTO_DIN_TOTAL_C : item.DESCUENTO_DIN_TOTAL_D = d.DESCUENTO_DIN_TOTAL_D
                                                item.IVA_POR = d.IVA_POR
                                                item.IVA_DIN_C = d.IVA_DIN_C : item.IVA_DIN_D = d.IVA_DIN_D
                                                item.IVA_DIN_TOTAL_C = d.IVA_DIN_TOTAL_C : item.IVA_DIN_TOTAL_D = d.IVA_DIN_TOTAL_D
                                                item.PRECIONETO_C = d.PRECIONETO_C : item.PRECIONETO_D = d.PRECIONETO_D
                                                item.SUBTOTAL_C = d.SUBTOTAL_C : item.SUBTOTAL_D = d.SUBTOTAL_D
                                                item.TOTAL_C = d.TOTAL_C : item.TOTAL_D = d.TOTAL_D
                                                .detalles.Add(item) : item = Nothing
                                            Else
                                                If v.MONEDA.Equals(Config.cordoba) Then
                                                    item.PRECIOUNITARIO_C = d.PRECIOUNITARIO_C : item.PRECIOUNITARIO_D = d.PRECIOUNITARIO_C / Me.Taza
                                                    item.DESCUENTO_POR = d.DESCUENTO_POR
                                                    item.DESCUENTO_DIN_C = d.DESCUENTO_DIN_C : item.DESCUENTO_DIN_D = d.DESCUENTO_DIN_D
                                                    item.DESCUENTO_DIN_TOTAL_C = d.DESCUENTO_DIN_TOTAL_C : item.DESCUENTO_DIN_TOTAL_D = d.DESCUENTO_DIN_TOTAL_C / Me.Taza
                                                    item.IVA_POR = d.IVA_POR
                                                    item.IVA_DIN_C = d.IVA_DIN_C : item.IVA_DIN_D = d.IVA_DIN_C / Me.Taza
                                                    item.IVA_DIN_TOTAL_C = d.IVA_DIN_TOTAL_C : item.IVA_DIN_TOTAL_D = d.IVA_DIN_TOTAL_C / Me.Taza
                                                    item.PRECIONETO_C = d.PRECIONETO_C : item.PRECIONETO_D = d.PRECIONETO_C / Me.Taza
                                                    item.SUBTOTAL_C = d.SUBTOTAL_C : item.SUBTOTAL_D = d.SUBTOTAL_C / Me.Taza
                                                    item.TOTAL_C = d.TOTAL_C : item.TOTAL_D = d.TOTAL_C / Me.Taza
                                                    .detalles.Add(item) : item = Nothing
                                                Else
                                                    item.PRECIOUNITARIO_C = d.PRECIOUNITARIO_D * Me.Taza : item.PRECIOUNITARIO_D = d.PRECIOUNITARIO_D
                                                    item.DESCUENTO_POR = d.DESCUENTO_POR
                                                    item.DESCUENTO_DIN_C = d.DESCUENTO_DIN_D * Me.Taza : item.DESCUENTO_DIN_D = d.DESCUENTO_DIN_D
                                                    item.DESCUENTO_DIN_TOTAL_C = d.DESCUENTO_DIN_TOTAL_D * Me.Taza : item.DESCUENTO_DIN_TOTAL_D = d.DESCUENTO_DIN_TOTAL_D
                                                    item.IVA_POR = d.IVA_POR
                                                    item.IVA_DIN_C = d.IVA_DIN_D * Me.Taza : item.IVA_DIN_D = d.IVA_DIN_D
                                                    item.IVA_DIN_TOTAL_C = d.IVA_DIN_TOTAL_D * Me.Taza : item.IVA_DIN_TOTAL_D = d.IVA_DIN_TOTAL_D
                                                    item.PRECIONETO_C = d.PRECIONETO_D * Me.Taza : item.PRECIONETO_D = d.PRECIONETO_D
                                                    item.SUBTOTAL_C = d.SUBTOTAL_D * Me.Taza : item.SUBTOTAL_D = d.SUBTOTAL_D
                                                    item.TOTAL_C = d.TOTAL_D * Me.Taza : item.TOTAL_D = d.TOTAL_D
                                                    .detalles.Add(item) : item = Nothing
                                                End If
                                            End If
                                        Next
                                        .Grid()
                                        .lblContador.Text = "N° ITEM: " & .dtRegistro.Rows.Count
                                    End With
                            End Select
                            Me.Close()
                        Else
                            MessageBox.Show("Esta 'Compra' ya se encuentra cancelada.")
                            Llenar()
                        End If
                        v = Nothing
                    Else
                        MessageBox.Show("Esta 'Compra' no se encuentra. Probablemente ha sido eliminada.")
                        Llenar()
                    End If
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub dtRegistro_KeyDown(sender As Object, e As KeyEventArgs) Handles dtRegistro.KeyDown
        If e.KeyData = Keys.Enter Then
            dtRegistro_CellDoubleClick(Nothing, Nothing)
        End If
    End Sub
End Class