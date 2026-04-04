Imports System.ComponentModel
Imports DocumentFormat.OpenXml.Drawing

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
        tbblnr.Text = tools.readBLBlattCookie("bgm_blattnr_cookie.txt")
        Dim gemarkung, flur, zaehler, nenner, gemarkungsindex As String
        Dim gemeinde, strasse, hausnr, lage, gemeindeindex As String

        LoadHistory() : ComboHistory.ItemsSource = Nothing : ComboHistory.ItemsSource = historyList
        ComboHistory.DisplayMemberPath = "Anzeige"
        ComboHistory.IsDropDownOpen = True

        tools.readFSTCookie(gemarkung, flur, zaehler, nenner, "bgm_FST_cookie.txt")
        gemarkungsindex = gemarkung
            tbFlur.Text = flur
        tbZaehler.Text = zaehler
        tbnenner.Text = nenner



        If isAutho() Then
            'its ok  21478  21504
            '"POLYGON ((479015 5538655,479033 5538660,479035 5538656,479017 5538650,479015 5538655))" 
        Else
            'MessageBox.Show("Sie haben keine Berechtigung für diese Anwendung. Abbruch!")
            'Close() 
            stpAdminOnly.Visibility = Visibility.Visible

            btnEdit.IsEnabled = False
        End If
        initKatasterGemarkungtext()
        Dim gemeinden() = mapTools.init_katastergemeindeliste()
        katasterGemeindelist = mapTools.splitgemeindeliste(gemeinden)
        katasterGemarkungslist = splitKatasterGemarkung()
        Dim gameindeitems As New List(Of myComboBoxItem)
        Dim gamarkungsitems As New List(Of myComboBoxItem)

        For Each gema As myComboBoxItem In katasterGemarkungslist
            gamarkungsitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
        Next
        For Each gema As myComboBoxItem In katasterGemeindelist
            gameindeitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
        Next
        'gamarkungsitems.Add(New clsCombo With {.Text = "Deutschland", .Code = 1})
        'gamarkungsitems.Add(New clsCombo With {.Text = "Österreich", .Code = 2})
        'gamarkungsitems.Add(New clsCombo With {.Text = "Schweiz", .Code = 3})

        cmbGemarkungen.ItemsSource = gamarkungsitems
        cmbGemarkungen.DisplayMemberPath = "mySttring"
        cmbGemarkungen.SelectedValuePath = "myindex"
        cmbGemarkungen.IsDropDownOpen = False
        cmbGemarkungen.SelectedIndex = CInt(gemarkungsindex)

        cmbGemarkungen2.ItemsSource = gamarkungsitems
        cmbGemarkungen2.DisplayMemberPath = "mySttring"
        cmbGemarkungen2.SelectedValuePath = "myindex"
        cmbGemarkungen2.IsDropDownOpen = False
        cmbGemarkungen2.SelectedIndex = CInt(0)

        cmbGemeinden.ItemsSource = gameindeitems
        cmbGemeinden.DisplayMemberPath = "mySttring"
        cmbGemeinden.SelectedValuePath = "myindex"
        cmbGemarkungen.IsDropDownOpen = True
        cmbGemeinden.SelectedIndex = CInt(gemeindeindex)

        Title = "BGM " & " V.: " & bgmVersion
        istgeladen = True
    End Sub
    ' Auswahl -> in TextBox schreiben


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

    'Private Sub btnNeu_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    Dim neu As New winDetail("0", False) ' 0=modus neu
    '    neu.ShowDialog()
    'End Sub

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



        tools.writeBLBlattCookie(tbblnr.Text, "bgm_blattnr_cookie.txt")
        Dim neu As New winDetail((tbblnr.Text), False) ' 0=modus neu
        neu.ShowDialog()
        LoadHistory() : ComboHistory.ItemsSource = Nothing : ComboHistory.ItemsSource = historyList
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

                    lastPDF = ""

                    tbFlurstueckDisplay.Text = tbFlurstueckDisplay.Text & Environment.NewLine &
                         "Keine Baulast gefunden."
                Else



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


    'Private Sub btnBaulastdisplay_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    'If tbBaulast2.Text.Length < 1 Or (Not IsNumeric(tbBaulast2.Text.Trim)) Then
    '    '    MessageBox.Show("Sie sollten zuerst eine BaulastNummer eingeben!")
    '    '    Exit Sub
    '    'End If
    '    'If lastPDF.Length < 1 Then
    '    MessageBox.Show("Es werden 2 Fenster angezeigt. Das " & Environment.NewLine &
    '           " 1. Fenster zeigt allg. Infos zur Baulast und das" & Environment.NewLine &
    '           " 2. Fenster zeigt die PDF zur Baulast" & Environment.NewLine & Environment.NewLine & Environment.NewLine &
    '           "Sie können die Baulast-PDF mit 'speichern unter...'  abspeichern.", "Baulast ansehen",
    '           MessageBoxButton.OK, MessageBoxImage.Information)
    '    Dim neu As New winDetail((tbBaulast2.Text), True) ' 0=modus neu
    '    neu.ShowDialog()
    '    'Else
    '    '    Process.Start(lastPDF)
    '    'End If
    'End Sub


    Private Sub btnBaulast4FST_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True

    End Sub

    Private Sub btnShowPDF_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'feststellen ob es die baulast gibt - sonst 
        'besteht die gefahr, dass eine veraltete PDF gezogen wird
        If clsGIStools.getBaulastFromBaulastMDAT(CInt(tbblnr.Text.Trim), kategorie_guid_Baulasten) Then
            lastPDF = clsGIStools.copyOnlyPDF(tbblnr.Text.Trim)
            If lastPDF.ToLower.StartsWith("fehler") Or
               lastPDF.ToLower.StartsWith("keine") Then
                MsgBox(lastPDF)
            Else
                tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
                Process.Start(lastPDF)
            End If
        Else
            MsgBox("Diese Baulast gibt es im GIS nicht!")
        End If 'füllt fstREC




    End Sub

    Private Sub btnBaulastImGIS_Click(sender As Object, e As RoutedEventArgs)
        'https://gis.kreis-of.de/LKOF/asp/main.asp?app=sp_mdat&lay=sp_mdat_0010_F&fld=text3&typ=string&val=10001&skipwelcome=true
        e.Handled = True
        tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
        baulastAlsObjImGisZeigen(tbblnr.Text.Trim)
    End Sub

    Private Sub btnsucheeigentumer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        fkzlist = New List(Of clsFlurstueck)
        fkzlist = readFlurst_Form()
        fst_lage = fkzlist(0).gemarkungstext
        If tools.flurstueckExistiertImGis(fkzlist(0).flurstueckZuFKZ) Then
            eigentuemerWord(False, fkzlist, fst_lage)
        Else
            MsgBox("Dieses Flurstück existiert nicht im GIS!")
        End If
    End Sub

    Private Function readFlurst_Form() As List(Of clsFlurstueck)
        l("readFlurst_Form")
        Dim item As myComboBoxItem = CType(cmbGemarkungen.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
            Exit Function
        End If
        Dim code As Integer = CInt(cmbGemarkungen.SelectedValue)
        Dim gemindex As Integer = CInt(cmbGemarkungen.SelectedIndex)
        Dim gemtext As String = (item.mySttring).ToString
        Dim fst As New clsFlurstueck
        fkzlist = New List(Of clsFlurstueck)
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
            tools.writeFlurstCookie(gemindex.ToString, (tbFlur.Text.Trim), tbZaehler.Text.Trim, tbnenner.Text.Trim, "bgm_FST_cookie.txt")
            Return fkzlist
        Catch ex As Exception
            l("readFlurst_Form " & ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Sub eigentuemerWord(isbaulast As Boolean, fkzlist As List(Of clsFlurstueck), lage As String)
        Try
            Dim dateinameFST As String
            dateinameFST = "_" & fkzlist.Item(0).gemarkungstext & "_" & fkzlist.Item(0).flur & "_" & fkzlist.Item(0).zaehler & "_" & fkzlist.Item(0).nenner & "_"
            Dim summe As String
            summe = "Aus ProbauG:" & Environment.NewLine
            summe = summe & makeFlurstuecksAbstrakt(fkzlist)
            summe = summe.Replace("Aus ProbauG:", "Aus Liegenschaftsbuch:")
            summe = summe & Environment.NewLine
            summe = summe & lage & Environment.NewLine
            summe = summe & Environment.NewLine

            Dim result, datei As String
            If toolsEigentuemer.geteigentuemerText(fkzlist, result) Then
                summe = summe & Environment.NewLine & result
                If isbaulast Then
                    datei = tools.erzeugeWordDateiEigentuemer(summe, "")
                Else
                    datei = tools.erzeugeWordDateiEigentuemer(summe, dateinameFST)
                End If
                Threading.Thread.Sleep(1000)
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
        fkzlist = New List(Of clsFlurstueck)
        fkzlist = readFlurst_Form()
        Dim index As Integer = CInt(cmbGemarkungen.SelectedIndex)
        Try
            tools.writeFlurstCookie(index.ToString, (tbFlur.Text.Trim), tbZaehler.Text.Trim, tbnenner.Text.Trim, "bgm_FST_cookie.txt")
            MitFlurstueckInsGIS(fkzlist)
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub MitFlurstueckInsGIS(loklist As List(Of clsFlurstueck))
        l("fehler in btngis4fst_click ")
        Try
            fst_lage = loklist.Item(0).gemarkungstext
            If tools.flurstueckExistiertImGis(loklist(0).flurstueckZuFKZ) Then
                gisFuerProbaugFlurst(tbblnr.Text.Trim, loklist)
            Else
                MsgBox("Das Flurstück exisitert so nicht im GIS!")
            End If
        Catch ex As Exception
            l("fehler in btngis4fst_click " & ex.ToString)
        End Try
    End Sub






    Private Sub tbStrasse_TextChanged(sender As Object, e As TextChangedEventArgs)
        If tbStrasse Is Nothing Then Exit Sub
        e.Handled = True

        Dim oldstring As String = ""
        Dim cb As New myComboBoxItem
        Dim strassennamen As New List(Of myComboBoxItem)
        lageliste = clsGIStools.getLage(tbStrasse.Text, cmbGemeinden.SelectedValue.ToString, mitfkz:=False)
        Dim a() As String
        Dim newstring As String = ""

        cmbstrassen.ItemsSource = lageliste
        cmbstrassen.DisplayMemberPath = "mySttring"
        cmbstrassen.SelectedValuePath = "myindex"
        cmbstrassen.IsDropDownOpen = True

    End Sub

    Private Sub cmbstrassen_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True

        l("cmbstrassen_SelectionChanged ")
        Dim item As myComboBoxItem = CType(cmbstrassen.SelectedItem, myComboBoxItem)
        Dim gemeindeitem As myComboBoxItem = CType(cmbGemeinden.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
            Exit Sub
        End If
        l(item.myindex)
        Dim gemeindeschluessel As String = cmbGemeinden.SelectedValue.ToString
        Dim lage As String = item.mySttring
        Dim gemeindetext As String = gemeindeitem.mySttring
        Dim oldstring As String = ""
        Dim cb As New myComboBoxItem
        Dim fst As New clsFlurstueck
        Dim strassennamen As New List(Of myComboBoxItem)
        lage_lage = "== Lage: " & gemeindetext & ", " & lage & " =="
        lageliste = clsGIStools.getLage(lage, gemeindeschluessel, mitfkz:=True)
        If lageliste.Count > 0 Then
            'fkz zerlegen 
            fst.Flurstuecksskennzeichen = lageliste.Item(0).myindex.ToString
            '   flurstueckskennzeichen
            fst.fkzzerlegen()
            'gemarkungsindex = gemarkung
            'cmbGemarkungen.SelectedIndex = CInt(fst.gemcode)
            tbFlur.Text = fst.flur.ToString
            tbZaehler.Text = fst.zaehler.ToString
            tbnenner.Text = fst.nenner.ToString
            fkzlist_lage = New List(Of clsFlurstueck)
            fkzlist_lage.Add(fst)
            btnwordADR.IsEnabled = True
            btngis4adr.IsEnabled = True
        Else
            MsgBox("Kein entsprechendes Flurstück gefunden")
        End If
    End Sub

    Private Sub btnwordADR_Click(sender As Object, e As RoutedEventArgs)
        'Dim loklist = New List(Of clsFlurstueck)
        'loklist = readFlurst_Form()
        eigentuemerWord(False, fkzlist_lage, lage_lage)
    End Sub

    Private Sub btngis4adr_Click(sender As Object, e As RoutedEventArgs)
        Dim loklist = New List(Of clsFlurstueck)
        loklist = readFlurst_Form()
        Dim index As Integer = CInt(cmbGemarkungen.SelectedIndex)
        Try
            ' tools.writeFlurstCookie(gemaIndex.ToString, (fkzlist_lage.Item(0).flur.ToString), (fkzlist_lage.Item(0).zaehler.ToString), (fkzlist_lage.Item(0).nenner.ToString), "bgm_FST_cookie.txt")
            MitFlurstueckInsGIS(fkzlist_lage)
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub ComboHistory_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        'If ComboHistory.SelectedItem IsNot Nothing Then
        '    tbblnr.Text = ComboHistory.SelectedItem.ToString()
        'End If

        Dim item = TryCast(ComboHistory.SelectedItem, HistoryItem)
        If item IsNot Nothing Then
            tbblnr.Text = item.Nummer
        End If
    End Sub

    Private Sub cmbGemeinden2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid_Baulasten='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
        e.Handled = True
        If Not istgeladen Then Exit Sub
    End Sub

    Private Sub tbbplantfilter_TextChanged(sender As Object, e As TextChangedEventArgs)
        e.Handled = True

        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbGemarkungen2.SelectedItem, myComboBoxItem)
        Dim gemavalue As String = item.mySttring.ToString
        Dim bplanListe As New List(Of myComboBoxItem)
        Try

            ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid_Baulasten='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
            bplanListe = tools.sucheNachBplaenen(gemavalue.ToString, tbbplantfilter.Text, kategorie_guid_Bplaene)
            cmbbplaene.ItemsSource = bplanListe
            cmbbplaene.DisplayMemberPath = "mySttring"
            cmbbplaene.SelectedValuePath = "myindex"
            cmbbplaene.IsDropDownOpen = True
        Catch ex As Exception
            l("tbbplantfilter_TextChanged " & ex.ToString)
        End Try
    End Sub

    Private Sub cmbGemarkungen2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbGemarkungen2.SelectedItem, myComboBoxItem)
        Dim gemavalue As String = item.mySttring.ToString
        Dim bplanListe As New List(Of myComboBoxItem)
        Try

            ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid_Baulasten='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
            bplanListe = tools.sucheNachBplaenen(gemavalue.ToString, tbbplantfilter.Text, kategorie_guid_Bplaene)
            cmbbplaene.ItemsSource = bplanListe
            cmbbplaene.DisplayMemberPath = "mySttring"
            cmbbplaene.SelectedValuePath = "myindex"
            cmbbplaene.IsDropDownOpen = True
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub cmbbplaene_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbbplaene.SelectedItem, myComboBoxItem)
        Dim gemavalue As String = item.mySttring.ToString
        Dim gemaindex As String = item.myindex.ToString
        Dim bplanListe As New List(Of myComboBoxItem)
    End Sub

    Private Sub btngis4BPlAN_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
    End Sub

    Private Sub cmbGemarkungen_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True

    End Sub

    'Private Sub ButtonSaveHistory_Click(sender As Object, e As RoutedEventArgs)
    '    ' Button klick -> speichern
    '    e.Handled = True
    '    WriteHistoryCookie(tbblnr.Text)

    'End Sub

    'Private Sub tbStrasseFilter_TextChanged(sender As Object, e As TextChangedEventArgs)
    '    'If tbStrasseFilter Is Nothing Then Exit Sub
    '    'e.Handled = True

    '    'lageliste = clsGIStools.getLage(tbStrasseFilter.Text, cmbGemeinden.SelectedValue.ToString, tbHausnr.Text)
    '    ''lageliste = mapTools.lageohneZahl(lageliste)
    'End Sub



End Class
