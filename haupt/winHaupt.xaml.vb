﻿Imports System.ComponentModel

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
        If isAutho() Then
            'its ok  21478  21504
            tbblnr.Text = "21504"
            '"POLYGON ((479015 5538655,479033 5538660,479035 5538656,479017 5538650,479015 5538655))" 
        Else
            'MessageBox.Show("Sie haben keine Berechtigung für diese Anwendung. Abbruch!")
            'Close()
            stpAdminOnly.Visibility = Visibility.Collapsed
        End If
        setLogfile(logfile) : l("Start " & Now) : l("mgisversion:" & bgmVersion)
        initdb()
        Title = "BGM " & " V.: " & bgmVersion
        istgeladen = True
    End Sub

    Private Shared Function isAutho() As Boolean
        'Return False
        Return Environment.UserName.ToLower = "storcksdieck_a" Or
                Environment.UserName.ToLower = "hartmann_s" Or
                Environment.UserName.ToLower = "briese_j" Or
                Environment.UserName.ToLower = "feinen_j" Or
                Environment.UserName.ToLower = "thieme_m" Or
                Environment.UserName.ToLower = "zahnlückenpimpf" Or
                Environment.UserName.ToLower = "kroemmelbein_m"
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
        Dim gisexe = "C:\kreisoffenbach\mgis\mgis.exe"
        'Dim lu, ro As New myPoint
        'lu.X = range.xl
        'lu.Y = range.yl
        'ro.X = range.xh
        'ro.Y = range.yh
        'rangestring = calcrangestring(lu, ro)
        'param = "modus=""bebauungsplankataster""  range=""" & rangestring & ""
        Process.Start(gisexe)
    End Sub



    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'If tbblnr.Text.IsNothingOrEmpty Then
        '    MsgBox("bitte geben sie eine blnr ein!")
        '    Exit Sub
        'End If
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

    Private Sub btnPDFTool_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim ewrk As New winWerkzeuge
        ewrk.ShowDialog()

    End Sub

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
        eigentuemerText = kurz & toolsEigentuemer.geteigentuemertext(tools.FSTausGISListe)
        If eigentuemerText.Length > 1 Then
            btnEigentuemer.IsEnabled = True
            'btnBaulast4FST.IsEnabled = True

            baulastnr = getBaulastNr(tools.FSTausGISListe(0))
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

    Private Function getBaulastNr(fst As clsFlurstueck) As String
        'tools.FSTausGISListe
        If istgeladen Then
            Dim hinweis As String = ""
            fstREC.mydb.SQL = "select jahr_blattnr,tiff from " & srv_schema & "." & srv_tablename &
             " where gemcode = '" & fst.gemcode & "'" &
             " and flur='" & fst.flur & "'" &
             " and zaehler='" & fst.zaehler & "'" &
             " and nenner='" & fst.nenner & "'"
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return "keine BL"
                'tbBaulast2.Text = "keine BL"
                'lastPDF = ""
                'btnBaulastdisplay.IsEnabled = False
            Else
                'tbBaulast2.Text = fstREC.dt.Rows(0).Item(0).ToString.Trim
                lastPDF = fstREC.dt.Rows(0).Item(1).ToString.Trim
                lastPDF = lastPDF.ToLower.Replace(".tiff", ".pdf")
                lastPDF = lastPDF.ToLower.Replace(".tif", ".pdf")
                Return fstREC.dt.Rows(0).Item(0).ToString.Trim
                'btnBaulastdisplay.IsEnabled = True
            End If

        End If
    End Function
End Class
