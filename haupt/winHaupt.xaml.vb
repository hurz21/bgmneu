Imports System.ComponentModel

Public Class winHaupt
    Private istgeladen As Boolean = False
    Private eigentuemerText As String = ""
    Private lastPDF As String = ""
    Private baulastnr As String = ""
    Public fst As New clsFlurstueck
    Sub New()
        InitializeComponent()
    End Sub
    Private Sub winHaupt_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        e.Handled = True
        setLogfile(logfile) : l("Start " & Now) : l("mgisversion:" & bgmVersion)
        initdb()
        tbblnr.Text = "6428"
        tbblnr.Text = "21507"
        tbblnr.Text = "131045"
        tbblnr.Text = tools.readBLBlattCookie

        If isAutho() Then
            'its ok  21478  21504
            '"POLYGON ((479015 5538655,479033 5538660,479035 5538656,479017 5538650,479015 5538655))" 
        Else
            'MessageBox.Show("Sie haben keine Berechtigung für diese Anwendung. Abbruch!")
            'Close() 
            stpAdminOnly.Visibility = Visibility.Visible
            grpBaulasten.Height = 99
            btnEdit.IsEnabled = False
        End If
        initKatasterGemarkungtext()
        katasterGemarkungslist = splitKatasterGemarkung()
        Dim items As New List(Of myComboBoxItem)

        For Each gema As myComboBoxItem In katasterGemarkungslist
            items.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
        Next
        'items.Add(New clsCombo With {.Text = "Deutschland", .Code = 1})
        'items.Add(New clsCombo With {.Text = "Österreich", .Code = 2})
        'items.Add(New clsCombo With {.Text = "Schweiz", .Code = 3})

        cmbGemarkungen.ItemsSource = items
        cmbGemarkungen.DisplayMemberPath = "mySttring"
        cmbGemarkungen.SelectedValuePath = "myindex"
        cmbGemarkungen.IsDropDownOpen = False
        Title = "BGM " & " V.: " & bgmVersion
        istgeladen = True
    End Sub


    Private Shared Function isAutho() As Boolean
        'Return False
        Return Environment.UserName.ToLower = "benes_c" Or
                Environment.UserName.ToLower = "hartmann_s" Or
                Environment.UserName.ToLower = "briese_j" Or
                Environment.UserName.ToLower = "feinen_j" Or
                Environment.UserName.ToLower = "thieme_m" Or
                Environment.UserName.ToLower = "zahnlückenpimpf" Or
                Environment.UserName.ToLower = "neis_h"
    End Function

    Private Sub btnNeu_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim neu As New winDetail("0", False) ' 0=modus neu
        neu.ShowDialog()
    End Sub

    Private Sub btnBestand_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim best As New winBestand()
        best.Show()
    End Sub

    Private Sub btnGIS_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim gisexe = "https://gis.kreis-of.de/LKOF/asp/main.asp"

        Process.Start(gisexe)
    End Sub



    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'If tbblnr.Text.IsNothingOrEmpty Then
        '    MsgBox("bitte geben sie eine blnr ein!")
        '    Exit Sub
        'End If
        tools.writeBLBlattCookie(tbblnr.Text)
        Dim neu As New winDetail((tbblnr.Text), False) ' 0=modus neu
        neu.ShowDialog()
    End Sub

    Private Sub Window_Drop(sender As Object, e As DragEventArgs)
        e.Handled = True

        Dim filenames As String()
        Dim zuielname As String = ""
        Dim listeZippedFiles, listeNOnZipFiles, allFeiles As New List(Of String)
        Dim titelVorschlag As String = ""
        Try
            l(" MOD ---------------------- anfang")

            l(" MOD dropped anfang")
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                filenames = CType(e.Data.GetData(DataFormats.FileDrop), String())
            End If
            l(" MOD dropped 2")
            If filenames(0).ToLower.EndsWith(".tiff") Then
                Dim fi As New IO.FileInfo(filenames(0).ToLower.Trim)
                Dim a() As String
                a = fi.Name.Split("."c)
                tbblnr.Text = a(0)
                fi = Nothing
                'Dim neu As New winDetail((tbblnr.Text)) ' 0=modus neu
                'neu.ShowDialog()
            End If

            l(" MOD ---------------------- ende")
        Catch ex As Exception
            l("Fehler in MOD: " & ex.ToString())
        End Try
    End Sub

    'Private Sub btnPDFTool_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    Dim ewrk As New winWerkzeuge
    '    ewrk.ShowDialog()

    'End Sub

    Private Sub btnbplan_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim p As New Process
        p.Start("\\gis\gdvell\apps\bplankat\bplanstart.bat")
    End Sub

    Private Sub btngetFlurstueck_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim f As New winFlurstueck
        f.ShowDialog()
        Dim kurz As String
        kurz = f.normflst.gemarkungstext &
            ", Flur: " & f.normflst.flur &
            ": : " & f.normflst.zaehler &
            "/" & f.normflst.nenner & Environment.NewLine & Environment.NewLine
        tbFlurstueckDisplay.Text = kurz

        'tbFlurstueckDisplay.Background=
        tools.FSTausGISListe.Clear()
        tools.FSTausGISListe.Add(f.normflst)
        Dim result As String
        If toolsEigentuemer.geteigentuemerText(tools.FSTausGISListe, result) Then
            eigentuemerText = kurz & result
            If eigentuemerText.Length > 1 Then
                btnEigentuemer.IsEnabled = True
                'btnBaulast4FST.IsEnabled = True

                '''''  baulastnr = getBaulastNr(tools.FSTausGISListe(0)) '????
                If Not IsNumeric(baulastnr) Then
                    tbBaulast2.Text = "keine BL"
                    lastPDF = ""
                    btnBaulastdisplay.IsEnabled = False
                    tbFlurstueckDisplay.Text = tbFlurstueckDisplay.Text & Environment.NewLine &
                         "Keine Baulast gefunden."
                Else

                    tbBaulast2.Text = baulastnr
                    btnBaulastdisplay.IsEnabled = True
                    tbFlurstueckDisplay.Text = tbFlurstueckDisplay.Text & Environment.NewLine &
                       "BaulastNr: " & baulastnr
                End If
            End If
        Else
            eigentuemerText = kurz & result
        End If


    End Sub
    Private Sub btnEigentuemer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim fst As New wineigentuemerText(eigentuemerText)
        fst.ShowDialog()
    End Sub


    Private Sub btnBaulastdisplay_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'If tbBaulast2.Text.Length < 1 Or (Not IsNumeric(tbBaulast2.Text.Trim)) Then
        '    MessageBox.Show("Sie sollten zuerst eine BaulastNummer eingeben!")
        '    Exit Sub
        'End If
        'If lastPDF.Length < 1 Then
        MessageBox.Show("Es werden 2 Fenster angezeigt. Das " & Environment.NewLine &
               " 1. Fenster zeigt allg. Infos zur Baulast und das" & Environment.NewLine &
               " 2. Fenster zeigt die PDF zur Baulast" & Environment.NewLine & Environment.NewLine & Environment.NewLine &
               "Sie können die Baulast-PDF mit 'speichern unter...'  abspeichern.", "Baulast ansehen",
               MessageBoxButton.OK, MessageBoxImage.Information)
        Dim neu As New winDetail((tbBaulast2.Text), True) ' 0=modus neu
        neu.ShowDialog()
        'Else
        '    Process.Start(lastPDF)
        'End If
    End Sub

    Private Sub tbBaulast2_TextChanged(sender As Object, e As TextChangedEventArgs)
        e.Handled = True
        If istgeladen Then

            btnBaulastdisplay.IsEnabled = True
        End If
    End Sub
    Private Sub btnBaulast4FST_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True

    End Sub

    Private Sub btnShowPDF_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        lastPDF = clsGIStools.copyOnlyPDF(tbblnr.Text.Trim)
        If lastPDF.ToLower.StartsWith("fehler") Or
           lastPDF.ToLower.StartsWith("keine") Then
            MsgBox(lastPDF)
        Else
            tools.writeBLBlattCookie(tbblnr.Text.Trim)
            Process.Start(lastPDF)
        End If

    End Sub

    Private Sub btnBaulastImGIS_Click(sender As Object, e As RoutedEventArgs)
        'https://gis.kreis-of.de/LKOF/asp/main.asp?lay=sp_mdat_0010_F&fld=text3&typ=string&val=10001&skipwelcome=true
        e.Handled = True
        tools.writeBLBlattCookie(tbblnr.Text.Trim)
        baulastAlsObjImGisZeigen(tbblnr.Text.Trim)
    End Sub

    Private Sub btnsucheeigentumer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        eigentuemerWord()
    End Sub

    Private Sub eigentuemerWord()
        Dim item As myComboBoxItem = CType(cmbGemarkungen.SelectedItem, myComboBoxItem)
        Dim code As Integer = CInt(cmbGemarkungen.SelectedValue)
        Dim gemtext As String = (item.mySttring).ToString
        Dim fst As New clsFlurstueck
        Dim fkzlist As New List(Of clsFlurstueck)
        Try
            fst.gemcode = code
            fst.gemarkungstext=gemtext
            fst.flur = CInt(tbFlur.Text.Trim)
            fst.zaehler = CInt(tbZaehler.Text.Trim)
            If tbnenner.Text = String.Empty Then
                fst.nenner = 0
            Else
                fst.nenner = CInt(tbnenner.Text.Trim)
            End If
            fkzlist.Add(fst)
            Dim summe As String
            summe = "Aus ProbauG:" & Environment.NewLine
            summe = summe & makeFlurstuecksAbstrakt(fkzlist)
            summe = summe.Replace("Aus ProbauG:", "Aus Liegenschaftsbuch:")
            summe = summe & Environment.NewLine
            Dim result As String
            If toolsEigentuemer.geteigentuemerText(fkzlist, result) Then
                summe = summe & Environment.NewLine & result
                Dim datei = tools.erzeugeWordDateiEigentuemer(summe, "")
                Process.Start(datei)
            Else
                MsgBox(result)
            End If
        Catch ex As Exception
            l("fehler in eigentuemerWord " & ex.ToString)
        End Try
    End Sub

    Private Sub btngis4fst_click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        MitFlurstueckInsGIS()

    End Sub

    Private Sub MitFlurstueckInsGIS()
        l("fehler in btngis4fst_click ")
        Dim item As myComboBoxItem = CType(cmbGemarkungen.SelectedItem, myComboBoxItem)
        Dim code As Integer = CInt(cmbGemarkungen.SelectedValue)
        Dim gemtext As String = (item.mySttring).ToString
        Dim fst As New clsFlurstueck
        Dim fkzlist As New List(Of clsFlurstueck)
        Try
            fst.gemcode = code
            fst.gemarkungstext = gemtext
            fst.flur = CInt(tbFlur.Text.Trim)
            fst.zaehler = CInt(tbZaehler.Text.Trim)
            If tbnenner.Text = String.Empty Then
                fst.nenner = 0
            Else
                fst.nenner = CInt(tbnenner.Text.Trim)
            End If
            fkzlist.Add(fst)
            If tools.flurstueckExistiertImGis(fkzlist(0).flurstueckZuFKZ) Then
                gisFuerProbaugFlurst(tbblnr.Text.Trim, fkzlist(0).flurstueckZuFKZ)
            Else
                MsgBox("Das Flurstück exisitert so nicht im GIS!")
            End If
        Catch ex As Exception
            l("fehler in btngis4fst_click " & ex.ToString)
        End Try
    End Sub



    'Private Function getBaulastNr(fst As clsFlurstueck) As String
    '    'tools.FSTausGISListe
    '    If istgeladen Then
    '        Dim hinweis As String = ""
    '        fstREC.mydb.SQL = "select jahr_blattnr,tiff from " & srv_schema & "." & srv_tablename &
    '         " where gemcode = '" & fst.gemcode & "'" &
    '         " and flur='" & fst.flur & "'" &
    '         " and zaehler='" & fst.zaehler & "'" &
    '         " and nenner='" & fst.nenner & "'"
    '        l(fstREC.mydb.SQL)
    '        hinweis = fstREC.getDataDT()
    '        If fstREC.dt.Rows.Count < 1 Then
    '            Return "keine BL"
    '            'tbBaulast2.Text = "keine BL"
    '            'lastPDF = ""
    '            'btnBaulastdisplay.IsEnabled = False
    '        Else
    '            'tbBaulast2.Text = fstREC.dt.Rows(0).Item(0).ToString.Trim
    '            lastPDF = fstREC.dt.Rows(0).Item(1).ToString.Trim
    '            lastPDF = lastPDF.ToLower.Replace(".tiff", ".pdf")
    '            lastPDF = lastPDF.ToLower.Replace(".tif", ".pdf")
    '            Return fstREC.dt.Rows(0).Item(0).ToString.Trim
    '            'btnBaulastdisplay.IsEnabled = True
    '        End If

    '    End If
    'End Function
End Class
