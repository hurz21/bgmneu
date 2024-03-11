Imports Microsoft.VisualBasic.Logging

Public Class winFlurstueck
    Public Property normflst As New clsFlurstueck
    Sub New()
        InitializeComponent()
    End Sub

    Private Sub winFlurstueck_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        e.Handled = True
        initGemarkungsCombo()
        cmbgemarkung.IsDropDownOpen = True
    End Sub
    Sub initGemarkungsCombo()
        Dim existing As XmlDataProvider = TryCast(Me.Resources("XMLSourceComboBoxgemarkungen"), XmlDataProvider)
        existing.Source = New Uri("C:\kreisoffenbach\common\Combos\gemarkungen.xml")
    End Sub

    Private Sub cmbgemarkung_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles cmbgemarkung.SelectionChanged
        If cmbgemarkung.SelectedItem Is Nothing Then Exit Sub

        Dim myvali$ = CStr(cmbgemarkung.SelectedValue)
        Dim myvalx = CType(cmbgemarkung.SelectedItem, System.Xml.XmlElement)
        Dim myvals$ = myvalx.Attributes(1).Value.ToString

        tbGemarkung.Text = myvals
        normflst.gemcode = CInt(myvali)
        normflst.gemarkungstext = tbGemarkung.Text
        initFlureCombo()
        cmbFlur.IsEnabled = True
        cmbFlur.IsDropDownOpen = True
        e.Handled = True
    End Sub
    Sub initFlureCombo()
        Dim hinweis As String = ""
        fstREC.mydb.SQL = "select distinct flur from flurkarte.basis_f " &
         " where gemcode = " & normflst.gemcode &
" order by flur "
        l(fstREC.mydb.SQL)
        hinweis = fstREC.getDataDT()


        'dtFlure = modgetdt4sql.getDT4Query(Sql, toolsEigentuemer., hinweis)
        'DB_Oracle_sharedfunctions.holeFlureDT()
        'cmbFlur.DataContext = myGlobalz.sitzung.postgresREC.dt
        cmbFlur.DataContext = fstREC.dt
    End Sub

    Private Sub cmbFlur_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles cmbFlur.SelectionChanged
        Dim item2 As DataRowView = CType(cmbFlur.SelectedItem, DataRowView)
        If item2 Is Nothing Then Exit Sub

        cmbZaehler.IsEnabled = True
        Dim item3$ = item2.Row.ItemArray(0).ToString
        tbflur.Text = item2.Row.ItemArray(0).ToString
        'Me.tbStrasse.Text=item4
        normflst.flur = CInt(item3$)
        normflst.gemarkungstext = Me.tbGemarkung.Text
        initZaehlerCombo()
        cmbZaehler.IsDropDownOpen = True
        e.Handled = True
    End Sub
    Sub initZaehlerCombo()
        'DB_Oracle_sharedfunctions.holeZaehlerDT()
        'cmbZaehler.DataContext = myGlobalz.sitzung.postgresREC.dt
        Dim hinweis As String = ""
        fstREC.mydb.SQL = "select distinct zaehler from flurkarte.basis_f " &
         " where gemcode = " & normflst.gemcode &
         " and flur = " & normflst.flur &
         " order by zaehler  "
        l(fstREC.mydb.SQL)
        hinweis = fstREC.getDataDT()


        'dtFlure = modgetdt4sql.getDT4Query(Sql, toolsEigentuemer., hinweis)
        'DB_Oracle_sharedfunctions.holeFlureDT()
        'cmbFlur.DataContext = myGlobalz.sitzung.postgresREC.dt
        cmbZaehler.DataContext = fstREC.dt
    End Sub

    Private Sub cmbZaehler_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles cmbZaehler.SelectionChanged
        Dim item2 As DataRowView = CType(cmbZaehler.SelectedItem, DataRowView)
        If item2 Is Nothing Then Exit Sub
        Dim item3$ = item2.Row.ItemArray(0).ToString
        cmbNenner.IsEnabled = True
        tbZaehler.Text = item2.Row.ItemArray(0).ToString

        normflst.zaehler = CInt(item3$)

        normflst.nenner = Nothing
        initNennerCombo()
        If fstREC.dt.Rows.Count = 1 Then
            tbNenner.Text = fstREC.dt.Rows(0).Item(0).ToString
            NennerVerarbeiten()
            'cmbFunktionsvorschlaege.IsDropDownOpen = True
        Else
            cmbNenner.IsDropDownOpen = True
        End If
        e.Handled = True
    End Sub

    Sub initNennerCombo()
        'DB_Oracle_sharedfunctions.holeNennerDT()
        'cmbNenner.DataContext = myGlobalz.sitzung.postgresREC.dt
        Dim hinweis As String = ""
        fstREC.mydb.SQL = "select distinct nenner from flurkarte.basis_f " &
         " where gemcode = " & normflst.gemcode &
         " and flur = " & normflst.flur &
         " and zaehler = " & normflst.zaehler &
         " order by nenner  "
        l(fstREC.mydb.SQL)
        hinweis = fstREC.getDataDT()


        'dtFlure = modgetdt4sql.getDT4Query(Sql, toolsEigentuemer., hinweis)
        'DB_Oracle_sharedfunctions.holeFlureDT()
        'cmbFlur.DataContext = myGlobalz.sitzung.postgresREC.dt
        cmbNenner.DataContext = fstREC.dt
    End Sub

    Private Sub NennerVerarbeiten()
        normflst.nenner = CInt(tbNenner.Text)
        'FST_tools.nennerUndFSPruefen()
        normflst.FS = normflst.buildFS()
        'FST_tools.hole_FSTKoordinaten_undZuweisePunkt(normflst)
        'tbCoords.Text = String.Format("{0},{1}", normflst.punkt.X, normflst.punkt.Y)

        'tbarea.Text = CStr(normflst.flaecheqm)
        'lblFS.Text = normflst.FS
        'btnSpeichernFlurstueck.IsEnabled = True
        Close()
    End Sub

    Private Sub cmbNenner_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles cmbNenner.SelectionChanged
        Dim item2 As DataRowView = CType(cmbNenner.SelectedItem, DataRowView)
        If item2 Is Nothing Then Exit Sub
        Try

        Catch ex As Exception
            Exit Sub
        End Try
        tbNenner.Text = item2.Row.ItemArray(0).ToString

        NennerVerarbeiten()

        e.Handled = True
    End Sub
End Class
