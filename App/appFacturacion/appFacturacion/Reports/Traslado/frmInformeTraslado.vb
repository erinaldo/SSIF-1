﻿Imports Sadara.Models.V1.Database
Imports Sadara.Models.V1.POCO
'Imports System.Data.Entity

Public Class frmInformeTraslado

    Dim FormLoad As Boolean

    Sub Lista(ByVal Fecha1 As DateTime, ByVal Fecha2 As DateTime, Optional ByVal bodegap As String = "", Optional ByVal seriep As String = "", Optional ByVal IdEmpleadoP As String = "", Optional ByVal EmpleadoP As String = "")
        Try
            Using db As New CodeFirst
                If dtRegistro.Visible Then
                    Dim consulta = (From tra In db.Traslados Join ser In db.SERIES On ser.IDSERIE Equals tra.IDSERIE Where tra.FECHATRASLADO >= Fecha1 And tra.FECHATRASLADO <= Fecha2 And ser.Bodega.IDBODEGA.Contains(bodegap) And ser.IDSERIE.Contains(seriep) Order By tra.N Select ANULADO = If(tra.ANULADO.Equals("S"), "Anulado", ""), ID = tra.IDTRASLADO, SERIE = ser.NOMBRE, tra.CONSECUTIVO, tra.FECHATRASLADO, tra.Empleado.N_TRABAJADOR, EMPLEADO = tra.Empleado.NOMBRES & " " & tra.Empleado.APELLIDOS, BodegaSale = tra.Bodega.N_BODEGA & " - " & tra.Bodega.DESCRIPCION, Concepto = If(tra.ANULADO.Equals("N"), tra.CONCEPTO, ""), tra.REFERENCIA, TOTAL = If(tra.ANULADO.Equals("N"), tra.TOTAL, Nothing), tra.N).ToList

                    If IdEmpleadoP.Trim <> "" Then 'filtro empleado
                        consulta = consulta.Where(Function(f) f.N_TRABAJADOR.Contains(IdEmpleadoP) And f.ANULADO.Equals("")).ToList
                    End If
                    If EmpleadoP.Trim <> "" Then
                        consulta = consulta.Where(Function(f) f.EMPLEADO.Contains(EmpleadoP) And f.ANULADO.Equals("")).ToList
                    End If

                    dtRegistro.DataSource = consulta.OrderBy(Function(f) f.N).ToList
                    If dtRegistro.Columns.Count > 0 Then
                        With dtRegistro
                            .Columns(0).Width = 55 : .Columns(0).HeaderText = ""
                            .Columns(1).Visible = False : .Columns(1).DefaultCellStyle.ForeColor = Color.White
                            .Columns(3).HeaderText = vbNewLine & "Nº TRASLADO" & vbNewLine
                            .Columns(4).HeaderText = "FECHA"
                            .Columns(5).HeaderText = "Nº EMPLEADO" : .Columns(5).Width = 110
                            .Columns(6).HeaderText = "NOMBRES Y APELLIDOS DEL EMPLEADO" : .Columns(6).Width = 250
                            .Columns(7).HeaderText = "SALE DE" : .Columns(7).Width = 180
                            .Columns(8).HeaderText = "CONCEPTO DE LA TRASLADO" : .Columns(8).Width = 250
                            .Columns(9).HeaderText = "REFERENCIA" : .Columns(9).Width = 150
                            .Columns(10).DefaultCellStyle.Format = Config.f_m
                            .Columns(11).Visible = False
                        End With
                    End If
                    For Each c As DataGridViewColumn In dtRegistro.Columns
                        c.HeaderCell.Style.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Bold)
                    Next
                    If consulta.Count() > 0 Then
                        txtTotal.Value = consulta.Sum(Function(f) f.TOTAL)
                    Else
                        txtTotal.Value = 0
                    End If
                    If dtRegistro.Rows.Count = 0 Then
                        dtDetalle.DataSource = Nothing
                        expDetalle.TitleText = "MOSTRAR DETALLE DEL TRASLADO SELECCIONADA"
                    End If
                Else
                    Dim consulta = (From tra In db.Traslados Join ser In db.SERIES On ser.IDSERIE Equals tra.IDSERIE Where tra.FECHATRASLADO >= Fecha1 And tra.FECHATRASLADO <= Fecha2 And ser.Bodega.IDBODEGA.Contains(bodegap) And ser.IDSERIE.Contains(seriep) Order By tra.N Select ANULADO = If(tra.ANULADO.Equals("S"), "Anulado", ""), ser.IDBODEGA, BODEGA = ser.Bodega.N_BODEGA & " - " & ser.Bodega.DESCRIPCION, ID = tra.IDTRASLADO, SERIE = ser.NOMBRE, tra.CONSECUTIVO, Fecha = tra.FECHATRASLADO, NEmpleado = tra.Empleado.N_TRABAJADOR, Empleado = tra.Empleado.NOMBRES & " " & tra.Empleado.APELLIDOS, BodegaSale = tra.Bodega.N_BODEGA & " - " & tra.Bodega.DESCRIPCION, Concepto = If(tra.ANULADO.Equals("N"), tra.CONCEPTO, ""), tra.REFERENCIA, TOTAL = If(tra.ANULADO.Equals("N"), tra.TOTAL, Nothing), tra.N).ToList

                    If IdEmpleadoP.Trim <> "" Then 'filtro empleado
                        consulta = consulta.Where(Function(f) f.NEmpleado.Contains(IdEmpleadoP) And f.ANULADO.Equals("")).ToList
                    End If
                    If EmpleadoP.Trim <> "" Then
                        consulta = consulta.Where(Function(f) f.EMPLEADO.Contains(EmpleadoP) And f.ANULADO.Equals("")).ToList
                    End If

                    If consulta.Count() > 0 Then
                        txtTotal.Value = consulta.Sum(Function(f) f.TOTAL)
                    Else
                        txtTotal.Value = 0
                    End If

                    Dim rpt As New rptInformeTrasladosList
                    Config.CrystalTitle("INFORME DE TRASLADOS", rpt)
                    Dim band As CrystalDecisions.CrystalReports.Engine.TextObject
                    band = rpt.Section2.ReportObjects("txtFecha1") : band.Text = Fecha1.ToLongDateString()
                    band = rpt.Section2.ReportObjects("txtFecha2") : band.Text = Fecha2.ToLongDateString()
                    If bodegap <> "" Then
                        band = rpt.Section2.ReportObjects("txtSucursal") : band.Text = bodegap
                    Else
                        band = rpt.Section2.ReportObjects("txtSucursal") : band.Text = "%Todos%"
                    End If
                    If seriep <> "" Then
                        band = rpt.Section2.ReportObjects("txtSerie") : band.Text = seriep
                    Else
                        band = rpt.Section2.ReportObjects("txtSerie") : band.Text = "%Todos%"
                    End If
                    If IdEmpleadoP <> "" Then
                        band = rpt.Section2.ReportObjects("txtNEmpleado") : band.Text = IdEmpleadoP
                    Else
                        band = rpt.Section2.ReportObjects("txtNEmpleado") : band.Text = "%Todos%"
                    End If
                    If EmpleadoP <> "" Then
                        band = rpt.Section2.ReportObjects("txtEmpleado") : band.Text = EmpleadoP
                    Else
                        band = rpt.Section2.ReportObjects("txtEmpleado") : band.Text = "%Todos%"
                    End If

                    'totales
                    'band = rpt.Section5.ReportObjects("txtTotal") : band.Text = txtTotal.Value.ToString(Config.f_m)


                    rpt.SetDataSource(consulta.ToList)
                    CrystalReportViewer1.ReportSource = rpt
                    CrystalReportViewer1.Zoom(75)
                    rpt = Nothing : band = Nothing : consulta = Nothing
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error, " & ex.Message)
        End Try
    End Sub

    Sub ListaDetalle(ByVal id As String)
        Try
            Using db As New CodeFirst
                Dim v = (From tra In db.Traslados Join ser In db.SERIES On tra.IDSERIE Equals ser.IDSERIE Where tra.IDTRASLADO = id Select tra.IDTRASLADO, tra.ANULADO, SERIE = ser.NOMBRE, tra.CONSECUTIVO).FirstOrDefault()
                If Not v Is Nothing Then
                    If v.ANULADO = "N" Then
                        expDetalle.TitleText = "MOSTRAR DETALLE DEL TRASLADO '" & v.SERIE & " - " & v.CONSECUTIVO & "'"
                        dtDetalle.DataSource = (From mar In db.Marcas Join pro In db.Productos On mar.IDMARCA Equals pro.IDMARCA Join exi In db.EXISTENCIAS On pro.IDPRODUCTO Equals exi.IDPRODUCTO Join det In db.TrasladosDetalles On exi.IDEXISTENCIA Equals det.IDEXISTENCIA Where det.IDTRASLADO = id Select pro.IDALTERNO, pro.IDORIGINAL, pro.DESCRIPCION, MARCA = If(pro.Marca.ACTIVO.Equals("S"), mar.DESCRIPCION, "SIN ESPECIFICAR"), pro.MODELO, UNIDAD_DE_MEDIDA = If(pro.UnidadMedida.ACTIVO.Equals("S"), pro.UnidadMedida.DESCRIPCION, "SIN ESPECIFICAR"), pro.CONTIENE, PRESENTACIÓN = If(pro.Presentacion.ACTIVO.Equals("S"), pro.Presentacion.DESCRIPCION, "SIN ESPECIFICAR"), LABORATORIO = If(pro.Laboratorio.ACTIVO.Equals("S"), pro.Laboratorio.DESCRIPCION, "SIN ESPECIFICAR"), det.CANTIDAD, det.COSTO, det.TOTAL).ToList
                        If dtDetalle.Columns.Count > 0 Then
                            dtDetalle.Columns(0).HeaderText = vbNewLine & "Nº PRODUCTO" & vbNewLine : dtDetalle.Columns(0).Width = 120
                            dtDetalle.Columns(1).HeaderText = "ID ORIGINAL"
                            dtDetalle.Columns(2).HeaderText = "DESCRIPCIÓN DEL PRODUCTO" : dtDetalle.Columns(2).Width = 250
                            dtDetalle.Columns(3).HeaderText = "MARCA"
                            dtDetalle.Columns(4).HeaderText = "MODELO"
                            dtDetalle.Columns(5).HeaderText = "U / M"
                            dtDetalle.Columns(6).HeaderText = "CONT. X UNI."
                            dtDetalle.Columns(7).HeaderText = "PRESENTACIÓN"
                            dtDetalle.Columns(8).HeaderText = "LABORATORIO"
                            dtDetalle.Columns(9).HeaderText = "CANTIDAD" : dtDetalle.Columns(9).DefaultCellStyle.Format = Config.f_m
                            dtDetalle.Columns(10).HeaderText = "COSTO" : dtDetalle.Columns(10).DefaultCellStyle.Format = Config.f_m
                            dtDetalle.Columns(11).HeaderText = "TOTAL" : dtDetalle.Columns(11).DefaultCellStyle.Format = Config.f_m
                        End If
                        For Each c As DataGridViewColumn In dtDetalle.Columns
                            c.HeaderCell.Style.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Bold)
                        Next
                    Else
                        expDetalle.TitleText = "MOSTRAR DETALLE DEL TRASLADO SELECCIONADA"
                        dtDetalle.DataSource = Nothing
                        expDetalle.TitleText = "NO SE PUEDE MOSTRAR DETALLE DE LA TRASLADO '" & v.SERIE & " - " & v.CONSECUTIVO & "' POR QUE ESTÁ ANULADO."
                    End If
                Else
                    dtDetalle.DataSource = Nothing
                    MessageBox.Show("No se encuentra esta Traslado")
                End If
                v = Nothing
            End Using
        Catch ex As Exception
            MessageBox.Show("Error, " & ex.Message)
        End Try
    End Sub

    Sub CargarLista()
        If Me.FormLoad Then
            If Not cmbBodega.SelectedValue Is Nothing And Not cmbBodega.SelectedIndex = -1 Then
                If Not cmbSerie.SelectedValue Is Nothing And Not cmbSerie.SelectedIndex = -1 Then
                    Lista(dtpFechaInicial.Value.ToShortDateString() & " 00:00:00", dtpFechaFinal.Value.ToShortDateString() & " 23:59:59", cmbBodega.SelectedValue.ToString(), cmbSerie.SelectedValue.ToString(), txtNVendedor.Text.Trim, txtNombreVendedor.Text.Trim)
                Else
                    Lista(dtpFechaInicial.Value.ToShortDateString() & " 00:00:00", dtpFechaFinal.Value.ToShortDateString() & " 23:59:59", cmbBodega.SelectedValue.ToString(), , txtNVendedor.Text.Trim, txtNombreVendedor.Text.Trim)
                End If
            Else
                Lista(dtpFechaInicial.Value.ToShortDateString() & " 00:00:00", dtpFechaFinal.Value.ToShortDateString() & " 23:59:59", , , txtNVendedor.Text.Trim, txtNombreVendedor.Text.Trim)
            End If
        End If
    End Sub

    Sub ListaSerie(ByVal bodega As String)
        Me.FormLoad = False
        Try
            Using db As New CodeFirst
                cmbSerie.DataSource = db.SERIES.Where(Function(f) f.ACTIVO = "S" And f.OPERACION = "TRASLADO" And f.IDBODEGA = bodega).ToList() : cmbSerie.ValueMember = "IDSERIE" : cmbSerie.DisplayMember = "NOMBRE" : cmbSerie.SelectedIndex = -1
            End Using
        Catch ex As Exception
            MessageBox.Show("Error, " & ex.Message)
        End Try
        Me.FormLoad = True
    End Sub

    Private Sub frmInformeEntrada_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        gbFecha.Left = PanelEx4.Width - gbFecha.Width - 4
    End Sub

    Private Sub frmInformeTraslado_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Log.Instance.RegisterActivity(
            If(Config.currentBusiness IsNot Nothing, Config.currentBusiness.IdEmpresa, Guid.Empty),
            "ProductTransferReport",
            "Load",
            "Load ProductTransferReport",
            userId:=If(Config.currentUser IsNot Nothing, Guid.Parse(Config.currentUser.IDUsuario), Nothing)
        )

        frmInformeEntrada_Resize(Nothing, Nothing)
        Me.FormLoad = False
        'Formatos de Totales
        txtTotal.DisplayFormat = Config.f_m

        txtTotal.Value = 0.0
        'Fin Formatos

        Try
            dtRegistro.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Regular)
            dtDetalle.Font = New Font(Me.Font.FontFamily, Me.Font.Size, FontStyle.Regular)
            dtpFechaInicial.Value = DateTime.Now : dtpFechaFinal.Value = DateTime.Now
            Using db As New CodeFirst
                cmbBodega.DataSource = (From bod In db.Bodegas Where bod.ACTIVO = "S" Select bod.IDBODEGA, NOMBRE = bod.N_BODEGA & " - " & bod.DESCRIPCION).ToList() : cmbBodega.ValueMember = "IDBODEGA" : cmbBodega.DisplayMember = "NOMBRE" : cmbBodega.SelectedIndex = -1
            End Using

            Lista(DateTime.Now.ToShortDateString() & " 00:00:00", DateTime.Now.ToShortDateString() & " 23:59:59")
        Catch ex As Exception
            MessageBox.Show("Error, " & ex.Message)
        End Try
        Me.FormLoad = True
    End Sub

    Private Sub expDetalle_ExpandedChanging(sender As Object, e As DevComponents.DotNetBar.ExpandedChangeEventArgs) Handles expDetalle.ExpandedChanging
        If Me.FormLoad Then
            If Not expDetalle.Expanded Then
                expDetalle.Height = dtRegistro.Height / 2
                dtRegistro.Height = expDetalle.Height
            End If
        End If
    End Sub

    Private Sub btBuscar_Click(sender As Object, e As EventArgs) Handles btBuscar.Click
        dtRegistro.Visible = True
        expDetalle.Visible = True
        CrystalReportViewer1.Visible = False
        Me.CargarLista()
    End Sub

    Private Sub btLimpiar_Click(sender As Object, e As EventArgs) Handles btLimpiar.Click
        Me.FormLoad = False
        dtpFechaInicial.Value = DateTime.Now
        dtpFechaFinal.Value = DateTime.Now
        cmbBodega.SelectedIndex = -1
        cmbSerie.SelectedIndex = -1
        txtNVendedor.Clear()
        txtNombreVendedor.Clear()
        Lista(DateTime.Now.ToShortDateString() & " 00:00:00", DateTime.Now.ToShortDateString() & " 23:59:59")
        Me.FormLoad = True
    End Sub

    Private Sub btImprimir_Click(sender As Object, e As EventArgs) Handles btImprimir.Click
        dtRegistro.Visible = False
        expDetalle.Visible = False
        CrystalReportViewer1.Visible = True
        'cargar
        Me.CargarLista()
    End Sub

    Private Sub dtRegistro_SelectionChanged(sender As Object, e As EventArgs) Handles dtRegistro.SelectionChanged
        If Me.FormLoad Then
            Try
                If dtRegistro.SelectedRows.Count > 0 Then
                    ListaDetalle(dtRegistro.SelectedRows(0).Cells(1).Value.ToString)
                End If
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub btBusqueda_Click(sender As Object, e As EventArgs) Handles btBusqueda.Click
        If tbFiltros.Visible Then
            TabControl1.SelectedTabIndex = 0
            tbFiltros.Visible = False
            tcpFiltros.Visible = False
            Me.FormLoad = False
            cmbBodega.SelectedIndex = -1
            cmbSerie.SelectedIndex = -1
            txtNVendedor.Clear()
            txtNombreVendedor.Clear()
            Lista(dtpFechaInicial.Value.ToShortDateString & " 00:00:00", dtpFechaFinal.Value.ToShortDateString & " 23:59:59")
            Me.FormLoad = True
        Else
            tbFiltros.Visible = True
            tcpFiltros.Visible = True
            TabControl1.SelectedTabIndex = 1
            cmbBodega.Focus()
        End If
    End Sub

    Private Sub dtpFechaInicial_ValueChanged(sender As Object, e As EventArgs) Handles dtpFechaInicial.ValueChanged, dtpFechaFinal.ValueChanged
        Me.CargarLista()
    End Sub



    Private Sub cmbBodega_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbBodega.SelectedIndexChanged
        If Me.FormLoad Then
            If Not cmbBodega.SelectedValue Is Nothing And Not cmbBodega.SelectedIndex = -1 Then
                ListaSerie(cmbBodega.SelectedValue.ToString())
                Me.CargarLista()
                cmbSerie.Focus()
            Else
                Me.CargarLista()
            End If
        End If
    End Sub

    Private Sub cmbSerie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSerie.SelectedIndexChanged
        If Me.FormLoad Then
            If Not cmbSerie.SelectedValue Is Nothing And Not cmbSerie.SelectedIndex = -1 Then
                Me.CargarLista()
            End If
        End If
    End Sub

    Private Sub txtEmpleado_KeyDown(sender As Object, e As KeyEventArgs) Handles txtNVendedor.KeyDown, txtNombreVendedor.KeyDown
        If e.KeyData = Keys.Enter Then
            Me.CargarLista()
        End If
    End Sub

    Private Sub frmInformeVentas_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control Then
            Select Case e.KeyCode
                Case Keys.B : btBuscar_Click(Nothing, Nothing)
                Case Keys.Delete : btLimpiar_Click(Nothing, Nothing)
                Case Keys.A : btBusqueda_Click(Nothing, Nothing)
                Case Keys.P : btImprimir_Click(Nothing, Nothing)
            End Select
        End If
    End Sub
End Class