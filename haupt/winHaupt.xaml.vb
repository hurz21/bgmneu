Imports System.ComponentModel
Imports System.Net
Imports System.Security.Policy
Imports System.Text
Imports DocumentFormat.OpenXml.Drawing
Imports DocumentFormat.OpenXml.EMMA
Imports DocumentFormat.OpenXml.Spreadsheet


Public Class winHaupt
    Private istgeladen As Boolean = False
    Private eigentuemerText As String = ""
    Private lastPDF As String = ""
    Private baulastnr As String = ""
    Public fst As New clsFlurstueck
    Public aktadr As New clsAdress
    Sub New()
        InitializeComponent()
    End Sub
    Private Sub winHaupt_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        e.Handled = True

        setLogfile(logfile) : l("Start " & Now) : l("mgisversion:" & bgmVersion)
        initdb()
        'tbblnr.Text = "6428"
        'tbblnr.Text = "21507"
        'tbblnr.Text = "131045"
        Dim cookieBl As String = tools.readBLBlattCookie("bgm_blattnr_cookie.txt")
        If String.IsNullOrWhiteSpace(cookieBl) Then
            tbblnr.Text = "131045"
            MessageBox.Show("Bitte die Nummer eingeben.", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        Else
            tbblnr.Text = cookieBl
        End If
        Dim gemarkung, flur, zaehler, nenner, gemarkungsindex As String
        Dim gemeinde, strasse, hausnr, lage, gemeindeindex As String

        LoadHistory() : ComboHistory.ItemsSource = Nothing : ComboHistory.ItemsSource = historyList
        ComboHistory.DisplayMemberPath = "Anzeige"
        If clsActiveDir.getall(Environment.UserName) Then
            Dim result = clsActiveDir.fdkurz
            Title = "BGM 2026, " & clsActiveDir.fdkurz
            tools.eigentuemerAbfrageErlaubt = (result.ToLower.Contains("umwelt") Or result.ToLower.Contains("bauaufsicht"))
        End If

        tools.readFSTCookie(gemarkung, flur, zaehler, nenner, "bgm_FST_cookie.txt")
        'gemarkungsindex = gemarkung
        tbFlur.Text = flur
        tbZaehler.Text = zaehler
        tbnenner.Text = nenner

        Dim stored = My.Settings.ImmerLogouten ' Boolean (Default: True)
        chkbImmerLogouten.IsChecked = stored
        gisLogouten = stored
        tools.themendefinitionsdatei = My.Settings.Themendatei

        If isAutho() Then
            ComboHistory.IsDropDownOpen = True
        Else
            tabEig.SelectedIndex = 1
            btnEdit.IsEnabled = False
        End If

        initKatasterGemarkungtext()
        Dim gemeinden() = mapTools.init_katastergemeindeliste()
        katasterGemeindelist = mapTools.splitgemeindeliste(gemeinden)
        katasterGemarkungslist = splitKatasterGemarkung()

        Dim gameindeitems As New List(Of myComboBoxItem)
        Dim gamarkungsitems As New List(Of myComboBoxItem)
        Dim themendateien As New List(Of myComboBoxItem)

        themendateien.Add(New myComboBoxItem With {.mySttring = "Baulasten", .myindex = "themendateiBaulasten.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "Bauaufsicht", .myindex = "themendateiBauaufsicht.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "Denkmalschutz", .myindex = "themendateiDenkmalschutz.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "Umwelt", .myindex = "themendateiUmwelt.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "Immissionsschutz", .myindex = "themendateiImmissionsschutz.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "UNB", .myindex = "themendateiUNB.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "UWBB", .myindex = "themendateiUWBB.txt"})
        themendateien.Add(New myComboBoxItem With {.mySttring = "Schornsteinfegerei", .myindex = "themendateiFeger.txt"})

        cmbThemendatei.ItemsSource = themendateien
        cmbThemendatei.DisplayMemberPath = "mySttring"
        cmbThemendatei.SelectedValuePath = "myindex"
        cmbThemendatei.IsDropDownOpen = False
        'cmbThemendatei.SelectedValue = My.Settings.Themendatei

        ' Sicherstellen, dass die gespeicherte Themendatei im ItemsSource vorhanden ist
        If themendateien.Any(Function(t) t.myindex = My.Settings.Themendatei) Then
            cmbThemendatei.SelectedValue = My.Settings.Themendatei
        ElseIf themendateien.Count > 0 Then
            cmbThemendatei.SelectedIndex = 0
        End If


        For Each gema As myComboBoxItem In katasterGemarkungslist
            gamarkungsitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
        Next
        For Each gema As myComboBoxItem In katasterGemeindelist
            gameindeitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
        Next


        cmbGemarkungen.ItemsSource = gamarkungsitems
        cmbGemarkungen.DisplayMemberPath = "mySttring"
        cmbGemarkungen.SelectedValuePath = "myindex"
        cmbGemarkungen.IsDropDownOpen = False
        'cmbGemarkungen.SelectedIndex = CInt(gemarkungsindex)

        ' sichere Konvertierung des gemarkungsindex (war String -> SelectedIndex benötigt Integer)
        Dim gemIndexInt As Integer = 0
        If Not Integer.TryParse(gemarkung, gemIndexInt) Then
            gemIndexInt = 0
        End If
        If gamarkungsitems.Count > 0 Then
            gemIndexInt = Math.Max(0, Math.Min(gemIndexInt, gamarkungsitems.Count - 1))
            cmbGemarkungen.SelectedIndex = gemIndexInt
        End If

        cmbGemarkungen2.ItemsSource = gamarkungsitems
        cmbGemarkungen2.DisplayMemberPath = "mySttring"
        cmbGemarkungen2.SelectedValuePath = "myindex"
        cmbGemarkungen2.IsDropDownOpen = False
        'cmbGemarkungen2.SelectedIndex = CInt(0)
        If gamarkungsitems.Count > 0 Then cmbGemarkungen2.SelectedIndex = 0

        cmbGemeinden.ItemsSource = gameindeitems
        cmbGemeinden.DisplayMemberPath = "mySttring"
        cmbGemeinden.SelectedValuePath = "myindex"
        cmbGemeinden.IsDropDownOpen = False
        'cmbGemeinden.SelectedIndex = 13
        If gameindeitems.Count > 13 Then
            cmbGemeinden.SelectedIndex = 13
        ElseIf gameindeitems.Count > 0 Then
            cmbGemeinden.SelectedIndex = 0
        End If


        dummyaufrufStarten()
        Dim liste As List(Of clsFlurstueck) = cls20Cookies.LadeFlurstuecke()
        cmb20fst.ItemsSource = liste
        Dim aliste As List(Of clsAdress) = cls20Cookies.LadeAdressen()
        cmb20adr.ItemsSource = aliste
        Dim vorhabenliste As List(Of clsPGvorhaben) = cls20Cookies.LadePGcookies()
        cmbPGNR.ItemsSource = vorhabenliste
        istgeladen = True
    End Sub

    Private Sub dummyaufrufStarten()
        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        If gisLogouten Then
            Process.Start(logout)
            'Process.Start(logout)
        End If
    End Sub
    Private Shared Function isAutho() As Boolean
        Try
            Dim appDir = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            Dim userFile = IO.Path.Combine(appDir, "bgmusers.txt")
            Dim allowed As HashSet(Of String)

            If IO.File.Exists(userFile) Then
                Dim lines = IO.File.ReadAllLines(userFile).
                            Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
                            Select(Function(s) s.Trim())
                allowed = New HashSet(Of String)(lines, StringComparer.OrdinalIgnoreCase)
            Else
                ' Fallback: eingebaute Liste (nur als Notfall). Bevorzugt: Datei pflegen.
                allowed = New HashSet(Of String)(New String() {
                                                    "benes_c",
                                                    "hartmann_s",
                                                    "briese_j",
                                                    "feinen_j",
                                                    "thieme_m",
                                                    "zahnlückenpimpf",
                                                    "neis_h"
                                                   }, StringComparer.OrdinalIgnoreCase)
            End If

            Dim currentUser = Environment.UserName
            Return allowed.Contains(currentUser)
        Catch ex As Exception
            l("Fehler in isAutho: " & ex.ToString())
            Return False
        End Try
    End Function
    'Private Shared Function isAutho() As Boolean
    '    'Return False
    '    Return Environment.UserName.ToLower = "benes_c" Or
    '            Environment.UserName.ToLower = "hartmann_s" Or
    '            Environment.UserName.ToLower = "briese_j" Or
    '            Environment.UserName.ToLower = "feinen_j" Or
    '            Environment.UserName.ToLower = "thieme_m" Or
    '            Environment.UserName.ToLower = "zahnlückenpimpf" Or
    '            Environment.UserName.ToLower = "neis_h"
    'End Function


    Private Sub btnGIS_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim gisexe = "https://gis.kreis-of.de/LKOF/asp/main.asp"

        Process.Start(gisexe)
    End Sub



    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
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
            End If

            l(" MOD ---------------------- ende")
        Catch ex As Exception
            l("Fehler in MOD: " & ex.ToString())
        End Try
    End Sub


    Private Sub btnbplan_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim p As New Process
        p.Start("\\gis\gdvell\apps\bplankat\bplanstart.bat")
    End Sub


    Private Sub btnEigentuemer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim fst As New wineigentuemerText(eigentuemerText)
        fst.ShowDialog()
    End Sub




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
                'MsgBox(lastPDF)
                MessageBox.Show(lastPDF, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Else
                tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
                Process.Start(lastPDF)
            End If
        Else
            'MsgBox("Diese Baulast gibt es im GIS nicht!")
            MessageBox.Show("Diese Baulast gibt es im GIS nicht!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If 'füllt fstREC




    End Sub

    Private Sub btnBaulastImGIS_Click(sender As Object, e As RoutedEventArgs)
        'https://gis.kreis-of.de/LKOF/asp/main.asp?app=sp_mdat&lay=sp_mdat_0010_F&fld=text3&typ=string&val=10001&skipwelcome=true
        e.Handled = True
        tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
        baulastAlsObjImGisZeigen(tbblnr.Text.Trim, tools.themendefinitionsdatei)
    End Sub

    Private Sub btnsucheeigentumer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If tools.eigentuemerAbfrageErlaubt Then
            eigentuemerWord(False, fkzlist_lage, lage_lage)
        Else
            'MsgBox("Keine Rechte vorhanden")
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        fkzlist = New List(Of clsFlurstueck)
        fkzlist = readFlurst_Form()
        fst_lage = fkzlist(0).gemarkungstext
        Dim gemeindeschluessel, lagebezeichnung As String 'aktadr.gemeindebigNRstring aktadr.lage
        If tools.flurstueckExistiertImGis(fkzlist(0).flurstueckZuFKZ, gemeindeschluessel, lagebezeichnung) Then
            eigentuemerWord(False, fkzlist, fst_lage)
        Else

            MessageBox.Show("Dieses Flurstück existiert nicht im GIS!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If
    End Sub

    Private Function readFlurst_Form() As List(Of clsFlurstueck)
        l("readFlurst_Form")
        Dim item As myComboBoxItem = CType(cmbGemarkungen.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            'MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
            MessageBox.Show("Die Eingabe war ungültig. Bitte korrigieren!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
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
                'MsgBox(result)
                MessageBox.Show(result, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
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
            MitFlurstueckInsGIS(fkzlist, tbFSTbemerkung.Text, False)
            aktualisiereFSTHistory()
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub



    Private Sub MitFlurstueckInsGIS(loklist As List(Of clsFlurstueck), azinfo As String, imAdressModus As Boolean)
        l("fehler in MitFlurstueckInsGIS ")
        Try
            fst_lage = loklist.Item(0).gemarkungstext
            l("fst_lage " & fst_lage)
            Dim gemeindeschluessel, lagebezeichnung As String 'aktadr.gemeindebigNRstring aktadr.lage
            If tools.flurstueckExistiertImGis(loklist(0).flurstueckZuFKZ, gemeindeschluessel, lagebezeichnung) Then
                l("flurstück zu adresse existiert")
                gisFuerProbaugFlurst(tbblnr.Text.Trim, loklist)
                loklist(0).AZ = azinfo 'tbFSTbemerkung.Text
                loklist(0).index = cmbGemarkungen.SelectedIndex
                If imAdressModus Then

                Else
                    aktadr.gemeindebigNRstring = gemeindeschluessel
                    aktadr.strasseName = lagebezeichnung
                    aktadr.fkz = loklist(0).Flurstuecksskennzeichen
                    aktadr.ingradaLageZerlegen(aktadr.strasseName)
                    'ihah
                    cls20Cookies.SpeichereFlurstueck(loklist(0))
                End If
            Else
                'MsgBox("Das Flurstück exisitert nicht im GIS!")
                MessageBox.Show("Das Flurstück exisitert nicht im GIS!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If
        Catch ex As Exception
            l("fehler in MitFlurstueckInsGIS " & ex.ToString)
        End Try
    End Sub
    'Private Sub tbStrasse_TextChanged(sender As Object, e As TextChangedEventArgs)
    '    If tbStrasseFilter Is Nothing Then Exit Sub
    '    e.Handled = True
    '    Exit Sub

    '    Dim oldstring As String = ""
    '    Dim cb As New myComboBoxItem
    '    Dim strassennamen As New List(Of myComboBoxItem)
    '    lageliste = clsGIStools.getLage(tbStrasseFilter.Text, cmbGemeinden.SelectedValue.ToString, mitfkz:=False)
    '    Dim a() As String
    '    Dim newstring As String = ""

    '    cmbstrassen.ItemsSource = lageliste
    '    cmbstrassen.DisplayMemberPath = "mySttring"
    '    cmbstrassen.SelectedValuePath = "myindex"
    '    cmbstrassen.IsDropDownOpen = True

    'End Sub

    Private Sub cmbstrassen_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        l("cmbstrassen_SelectionChanged ")
        Dim item As myComboBoxItem = CType(cmbstrassen.SelectedItem, myComboBoxItem)
        Dim gemeindeitem As myComboBoxItem = CType(cmbGemeinden.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            Exit Sub
        End If
        l(item.myindex)
        aktadr.lageindex = CInt(cmbstrassen.SelectedIndex)
        aktadr.gemeindeindex = CInt(cmbGemeinden.SelectedIndex)
        aktadr.gemeindebigNRstring = cmbGemeinden.SelectedValue.ToString
        aktadr.strasseName = item.mySttring
        aktadr.gemeindeName = gemeindeitem.mySttring
        tblage.Text = aktadr.strasseName
        Dim cb As New myComboBoxItem
        Dim fst As New clsFlurstueck
        Dim strassennamen As New List(Of myComboBoxItem)
        lage_lage = "== Lage: " & aktadr.gemeindeName & ", " & aktadr.strasseName & " =="
        lageliste = clsGIStools.getLage(aktadr.strasseName, aktadr.gemeindebigNRstring, mitfkz:=True)
        If lageliste.Count > 0 Then
            'fkz zerlegen  
            fst.Flurstuecksskennzeichen = lageliste.Item(0).myindex.ToString
            '   flurstueckskennzeichen
            fst.fkzzerlegen()
            tbFlur.Text = fst.flur.ToString
            tbZaehler.Text = fst.zaehler.ToString
            tbnenner.Text = fst.nenner.ToString
            fkzlist_lage = New List(Of clsFlurstueck)
            fkzlist_lage.Add(fst)
            btnwordADR.IsEnabled = True
            btngis4adr.IsEnabled = True
            If lageliste IsNot Nothing Then
                'Dim adr As New clsAdress
                'aktadr.gemeindeName = gemeindetext.ToString
                'aktadr.strasseName = lage.ToString
                aktadr.fkz = fst.Flurstuecksskennzeichen
                'aktadr.gemeindeindex = cmbGemeinden.SelectedIndex
                aktadr.AZ = tbADRbemerkung.Text

            End If
        Else
            'MsgBox("Kein entsprechendes Flurstück gefunden")
            MessageBox.Show("Kein entsprechendes Flurstück gefunden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If
    End Sub

    Private Sub btnwordADR_Click(sender As Object, e As RoutedEventArgs)
        'Dim loklist = New List(Of clsFlurstueck)
        'loklist = readFlurst_Form()
        If tools.eigentuemerAbfrageErlaubt Then
            eigentuemerWord(False, fkzlist_lage, lage_lage)
        Else
            'MsgBox("Keine Rechte vorhanden")
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
    End Sub

    Private Sub btngis4adr_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim a = aktadr.gemeindeName
        'Dim loklist = New List(Of clsFlurstueck)
        'loklist = readFlurst_Form()
        'Dim gemeindeindex As Integer = CInt(cmbGemarkungen.SelectedIndex)
        'Dim adr As New clsAdress
        Try
            ' tools.writeFlurstCookie(gemaIndex.ToString, (fkzlist_lage.Item(0).flur.ToString), (fkzlist_lage.Item(0).zaehler.ToString), (fkzlist_lage.Item(0).nenner.ToString), "bgm_FST_cookie.txt")
            Dim Url = gisLogoutUndStartFKZ(aktadr.fkz, gisLogouten)

            'MitFlurstueckInsGIS(fkzlist_lage, tbADRbemerkung.Text, True)

            aktadr.AZ = tbADRbemerkung.Text 'tbFSTbemerkung.Text
            'aktadr.gemeindeindex = cmbGemarkungen.SelectedIndex
            cls20Cookies.SpeichereAdresse(aktadr)

            l("adresse wird angezeigt")
            aktualisierenAdressHistory()
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub aktualisierenAdressHistory()
        Try
            Dim aliste As List(Of clsAdress) = cls20Cookies.LadeAdressen()
            cmb20adr.ItemsSource = aliste
        Catch ex As Exception
            l("aktualisierenAdressHistory " & ex.ToString)
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
        ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
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

            ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
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
        'Dim pdfListe As New List(Of myComboBoxItem)
        Try

            ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
            bplanListe = tools.sucheNachBplaenen(gemavalue.ToString, tbbplantfilter.Text, kategorie_guid_Bplaene)
            cmbbplaene.ItemsSource = bplanListe
            cmbbplaene.DisplayMemberPath = "mySttring"
            cmbbplaene.SelectedValuePath = "myindex"
            cmbbplaene.IsDropDownOpen = True

            'pdfListe = tools.sucheNachBplaenen(gemavalue.ToString, tbbplantfilter.Text, kategorie_guid_Bplaene)
            'cmbbplaene.ItemsSource = bplanListe
            'cmbbplaene.DisplayMemberPath = "mySttring"
            'cmbbplaene.SelectedValuePath = "myindex"
            'cmbbplaene.IsDropDownOpen = True


        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub cmbbplaene_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim bplantext As String
        Dim bplanindex As String
        Dim bplanitem As myComboBoxItem
        Dim bplanListe As New List(Of myComboBoxItem)
        Dim bplanPDFListe As New List(Of myComboBoxItem)
        Dim gemitem As myComboBoxItem
        If Not istgeladen Then Exit Sub
        Try
            bplanitem = CType(cmbbplaene.SelectedItem, myComboBoxItem)
            If bplanitem Is Nothing Then
                Exit Sub
            End If
            bplantext = bplanitem.mySttring.ToString
            bplanindex = bplanitem.myindex.ToString

            gemitem = CType(cmbGemarkungen2.SelectedItem, myComboBoxItem)

            If gemitem Is Nothing Then
                'MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
                MessageBox.Show("Die Eingabe war ungültig. Bitte korrigieren!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                Exit Sub
            End If
            Dim gemcode As Integer = CInt(cmbGemarkungen2.SelectedValue)
            Dim gemindex As Integer = CInt(cmbGemarkungen2.SelectedIndex)
            Dim gemtext As String = (gemitem.mySttring).ToString

            btngis4BPlAN.IsEnabled = True
            aktbplan = tools.getAllMetaData4ThisBplanIdentNr(bplanindex)
            tbBplanAbstract.Text = aktbplan.bildeTextOhneWarnung
            tbBplanWarnung.Text = aktbplan.warnung
            'jetzt die dateien

            bplanPDFListe = tools.getAllPDFFiles4GUID(aktbplan.object_guid, "\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\")
            cmbbplPDF.ItemsSource = bplanPDFListe
            cmbbplPDF.DisplayMemberPath = "mySttring"
            cmbbplPDF.SelectedValuePath = "myindex"
            cmbbplPDF.IsDropDownOpen = True

            tbAnzahlBplanPDFs.Text = "Anhänge: " & bplanPDFListe.Count


        Catch ex As Exception
            l("cmbbplaene_SelectionChanged " & ex.ToString)
        End Try
    End Sub

    Private Async Sub btngis4BPlAN_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'https://gis.kreis-of.de/LKOF/asp/main.asp?&app=sp_mdat&lay=sp_mdat_0013_F&fld=ident&typ=string&val=1674&skipwelcome=true   
        'https://gis.kreis-of.de/LKOF/asp/main.asp?&app=sp_mdat&lay=sp_mdat_0013_F&fld=ident&typ=string&val=1134&skipwelcome=true 
        Dim url = ""

        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        url = logout
        If gisLogouten Then
            Process.Start(logout)
            Threading.Thread.Sleep(1000)
        End If
        url = tools.bplanAlsObjImGisZeigen(aktbplan.ident, tools.themendefinitionsdatei)
        Process.Start(url)
    End Sub

    Private Sub cmbGemarkungen_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True

    End Sub

    Private Sub chkbImmerLogouten_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim isChecked = If(chkbImmerLogouten.IsChecked, True, False)
        gisLogouten = isChecked
        My.Settings.ImmerLogouten = isChecked
        My.Settings.Save()
    End Sub

    Private Sub cmbbplPDF_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim zieldatei As String
        Dim pdfitem As myComboBoxItem
        Dim fi As IO.FileInfo
        Dim quelldatei As String
        Dim immer_aus_dem_cache_die_bplanpdfs As Boolean = True
        Try
            pdfitem = CType(cmbbplPDF.SelectedItem, myComboBoxItem)
            quelldatei = pdfitem.myindex
            fi = New IO.FileInfo(quelldatei)

            zieldatei = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            zieldatei = IO.Path.Combine(zieldatei, "bgm\cache")
            zieldatei = IO.Path.Combine(zieldatei, fi.Name)

            fi = New IO.FileInfo(zieldatei)
            If fi.Exists And immer_aus_dem_cache_die_bplanpdfs Then
                Process.Start(zieldatei)
            Else
                IO.File.Copy(quelldatei, zieldatei)
                Process.Start(zieldatei)
            End If
        Catch ex As Exception
            l("cmbbplPDF_SelectionChanged " & ex.ToString)
        End Try
    End Sub

    Private Sub cmbThemendatei_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub

        Dim dateiitem As myComboBoxItem
        Dim fi As IO.FileInfo
        Dim themendatei As String
        Dim immer_aus_dem_cache_die_bplanpdfs As Boolean = True
        Try
            dateiitem = CType(cmbThemendatei.SelectedItem, myComboBoxItem)
            themendatei = dateiitem.myindex
            tools.themendefinitionsdatei = themendatei.Trim


            My.Settings.Themendatei = themendatei.Trim
            My.Settings.Save()

        Catch ex As Exception
            l("cmbThemendatei_SelectionChanged " & ex.ToString)
        End Try
    End Sub

    Private Sub btnTutorial_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Process.Start("https://gis.kreis-of.de/LKOF/upload/tutorial/videos.html")
    End Sub

    Private Sub cmbGemeinden_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbGemeinden.SelectionChanged
        e.Handled = True
        tbStrasseFilter.Text = ""
        cmbstrassen.ItemsSource = Nothing
    End Sub

    'Private Sub btnSucheadresse_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True

    '    Dim oldstring As String = ""
    '    Dim cb As New myComboBoxItem
    '    Dim strassennamen As New List(Of myComboBoxItem)
    '    Dim adr As New clsAdress
    '    lageliste = clsGIStools.getLage(tbStrasseFilter.Text, cmbGemeinden.SelectedValue.ToString, mitfkz:=False)

    '    Dim a() As String
    '    Dim newstring As String = ""
    '    If lageliste IsNot Nothing Then
    '        cmbstrassen.ItemsSource = lageliste

    '        cmbstrassen.DisplayMemberPath = "mySttring"
    '        cmbstrassen.SelectedValuePath = "myindex"
    '        cmbstrassen.IsDropDownOpen = True
    '    Else
    '        MsgBox("Keine Strassennamen mit diesem Anfang gefunden.")
    '    End If

    'End Sub

    Private Sub cmb20fst_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim fst = TryCast(cmb20fst.SelectedItem, clsFlurstueck)
        If fst IsNot Nothing Then
            Dim neu As New myComboBoxItem
            neu.myindex = fst.index.ToString
            neu.mySttring = fst.gemarkungstext
            'txtGemarkung.Text = adr.gemarkungstext 
            cmbGemarkungen.SelectedIndex = CInt(neu.myindex)
            tbFlur.Text = fst.flur.ToString
            tbZaehler.Text = fst.zaehler.ToString
            tbnenner.Text = fst.nenner.ToString
            tbFSTbemerkung.Text = fst.AZ
        End If
    End Sub



    Private Sub tabItemFST_Clicked(sender As Object, e As MouseButtonEventArgs)

        cmb20fst.IsDropDownOpen = True
    End Sub

    Private Sub cmb20adr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim adr = TryCast(cmb20adr.SelectedItem, clsAdress)
        If adr IsNot Nothing Then
            Dim neu As New myComboBoxItem
            neu.myindex = adr.gemeindeindex.ToString
            neu.mySttring = adr.gemeindeName
            'txtGemarkung.Text = adr.gemarkungstext 
            cmbGemeinden.SelectedIndex = CInt(neu.myindex)
            'cmbstrassen.SelectedIndex = CInt(adr.lageindex)
            'cmbstrassen.SelectedValue = CInt(adr.strasseName)
            tblage.Text = adr.strasseName
            adr.lageindex = cmbstrassen.SelectedIndex


            tbStrasseFilter.Text = adr.strasseName.ToString.Substring(0, 3)
            'tb.Text = adr.zaehler.ToString
            'tbnenner.Text = adr.nenner.ToString
            tbFSTbemerkung.Text = adr.AZ
            Dim loklist As New List(Of clsFlurstueck)
            Dim lokfst As New clsFlurstueck
            lokfst.Flurstuecksskennzeichen = adr.fkz
            lokfst.fkzzerlegen()
            loklist.Add(lokfst)
            btngis4adr.IsEnabled = True

            'aktadr muss aktualisiert werden
            aktadr.gemeindeName = adr.gemeindeName
            aktadr.strasseName = adr.strasseName
            aktadr.gemeindebigNRstring = adr.gemeindebigNRstring
            aktadr.fkz = adr.fkz


            'gisFuerProbaugFlurst(tbblnr.Text.Trim, loklist)
        End If
    End Sub

    Private Sub aktualisiereFSTHistory()
        Try
            Dim liste As List(Of clsFlurstueck) = cls20Cookies.LadeFlurstuecke()
            cmb20fst.ItemsSource = liste
        Catch ex As Exception
            l("aktualisiereFSTHistory " & ex.ToString)
        End Try
    End Sub

    Private Sub tbPGsuchestarten_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        suchePG(True)
    End Sub

    Private Sub suchePG(savecookie As Boolean)
        Dim vorhaben1 As String
        Dim fstliste As List(Of clsFlurstueck)
        Dim metadata As List(Of myComboBoxItem)
        If clsActiveDir.fdkurz.Contains("mwelt") Then
            If CInt(tbPGnr.Text) < 80000 Then

                MessageBox.Show("Dem FD Umwelt sind nur Nr > 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                If Environment.UserName <> "Feinen_J" Then Exit Sub
            End If
        End If
        If clsActiveDir.fdkurz.Contains("auaufsicht") Then
            If CInt(tbPGnr.Text) > 80000 Then
                MessageBox.Show("Dem FD Bauaufsicht sind nur Nr < 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)

                If Environment.UserName <> "Feinen_J" Then Exit Sub
            End If
        End If
        If Not (clsActiveDir.fdkurz.Contains("mwelt") Or clsActiveDir.fdkurz.Contains("auaufsicht")) Then
            MessageBox.Show("Dem FD Umwelt sind nur Nr > 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)


            If Environment.UserName <> "Feinen_J" Then Exit Sub
        End If
        fstliste = probaug.klaereanzahlFST(tbPGJahr.Text, tbPGnr.Text, metadata, vorhaben1)
        If fstliste Is Nothing Then
            'MsgBox("Das Aktenzeichen ist ungültig!")
            MessageBox.Show("Das Aktenzeichen ist ungültig!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        Dim fkzstring = probaug.bildeFKZstring(fstliste)
        Dim flurstueckskennzeichen = fkzstring
        tools.gisLogoutUndStartFKZ(flurstueckskennzeichen, gisLogouten)

        Dim result As String
        result = fstliste.Count & " gültige Flurstücke wurden gefunden!" & Environment.NewLine

        Dim sb As New StringBuilder
        For i = 0 To metadata.Count - 1
            If metadata(i).mySttring.Trim = String.Empty Then
            Else
                sb.Append(metadata(i).myindex & ": " & metadata(i).mySttring & Environment.NewLine)
            End If
        Next
        tbPGresult.Text = result & sb.ToString
        If savecookie Then cls20Cookies.PGcookiespeichern(tbPGJahr.Text, tbPGnr.Text, vorhaben1)
    End Sub

    Private Sub cmbPGNR_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        e.Handled = True
        Dim adr = TryCast(cmbPGNR.SelectedItem, clsPGvorhaben)
        If adr IsNot Nothing Then
            tbPGJahr.Text = adr.jahr
            tbPGnr.Text = adr.nr
            suchePG(False)
        End If
    End Sub

    Private Sub btnfst2PG_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim probaugVorgange As New List(Of myComboBoxItem)
        fkzlist = New List(Of clsFlurstueck)
        fkzlist = readFlurst_Form()
        Dim neuertext As String
        Dim index As Integer
        index = CInt(cmbGemarkungen.SelectedIndex)
        fkzlist(0).fstueckKombi = fkzlist(0).buildFstueckkombi() 'in prosoz immer mit nenner_0
        'berechneFstueckkombiOhneNull(fkzlist(0)) 'in prosoz immer mit nenner_0
        Try
            probaugVorgange = probaug.getVorgaengeZuFlurstueck(fkzlist(0))
            If probaugVorgange.Count < 1 Then

                MessageBox.Show("Keine vorgänge gefunden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Else
                neuertext = bildePGvorgangCookieString(probaugVorgange)
                MessageBox.Show("Es wurden " & probaugVorgange.Count & " Vorgänge gefunden:" & Environment.NewLine & Environment.NewLine &
                       neuertext & Environment.NewLine & Environment.NewLine &
                        Environment.NewLine & Environment.NewLine &
                       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                'MsgBox("Es wurden " & probaugVorgange.Count & " Vorgänge gefunden:" & Environment.NewLine & Environment.NewLine &
                '       neuertext & Environment.NewLine & Environment.NewLine &
                '        Environment.NewLine & Environment.NewLine &
                '       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!")
                mergeToPGCookie(neuertext)
                aktualisierePGvorgaengeHistory()
                tabEig.SelectedIndex = 4
            End If

        Catch ex As Exception
            l("fehler btnsucheeigentumer_Click " & ex.ToString)
        End Try

    End Sub

    Private Sub aktualisierePGvorgaengeHistory()
        Dim vorhabenliste As List(Of clsPGvorhaben) = cls20Cookies.LadePGcookies()
        cmbPGNR.ItemsSource = vorhabenliste
    End Sub

    Private Shared Sub mergeToPGCookie(a As String)
        Dim path = cls20Cookies.GetCookieFilePath("PGvorgangcookies.txt")
        Dim alterText As String = ""
        If IO.File.Exists(path) Then
            alterText = IO.File.ReadAllText(path)
        End If
        Dim neuerText = a & Environment.NewLine & alterText
        IO.File.WriteAllText(path, neuerText)
    End Sub

    Private Shared Function bildePGvorgangCookieString(probaugVorgange As List(Of myComboBoxItem)) As String
        Dim sb As New StringBuilder
        For i = 0 To probaugVorgange.Count - 1
            sb.Append(probaugVorgange(i).myindex.Trim & "|" & probaugVorgange(i).mySttring.Trim & Environment.NewLine)
        Next
        Return sb.ToString
    End Function

    Private Shared Sub berechneFstueckkombiOhneNull(fst As clsFlurstueck)
        If fst.nenner = 0 Then
            fst.fstueckKombi = fst.zaehler.ToString
        Else
            fst.fstueckKombi = fst.zaehler & "/" & fst.nenner
        End If
    End Sub

    Private Sub btnadr2PG_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If tools.eigentuemerAbfrageErlaubt Then
            ' eigentuemerWord(False, fkzlist_lage, lage_lage)
        Else
            'MsgBox("Keine Rechte vorhanden")
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        Dim lokadr As New clsAdress
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindeName = aktadr.gemeindeName
        lokadr.strasseName = tblage.Text
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.fkz = aktadr.fkz
        Dim probaugVorgange As New List(Of myComboBoxItem)
        Dim aa = aktadr.gemeindeName
        lokadr.ingradaLageZerlegen(lokadr.strasseName)
        Dim neuertext As String

        'jetzt das flurstücksformular ausfüllen
        'damit sofort nach vorgängen gesucht werden kann
        Dim lokfst As New clsFlurstueck
        lokfst.Flurstuecksskennzeichen = aktadr.fkz
        lokfst.fkzzerlegen()
        tbFlur.Text = lokfst.flur.ToString
        tbZaehler.Text = lokfst.zaehler.ToString
        tbnenner.Text = lokfst.nenner.ToString
        tbFSTbemerkung.Text = aktadr.AZ 'lokfst.AZ
        Dim gemindex = tools.getgemarkungsindex(lokfst.gemarkungstext)
        cmbGemarkungen.SelectedIndex = gemindex
        Try
            probaugVorgange = probaug.getVorgaengeZuAdresseUndFlurstueck(lokadr, lokfst)
            If probaugVorgange.Count < 1 Then
                'MsgBox("Keine Vorgänge zu dieser Adresse und dem entsp. Flurstück gefunden. " & Environment.NewLine &
                '    " " & Environment.NewLine &
                '    "  " & Environment.NewLine
                '       )
                MessageBox.Show("Keine Vorgänge zu dieser Adresse und dem entsp. Flurstück gefunden. " & Environment.NewLine &
                    " " & Environment.NewLine &
                    "  " & Environment.NewLine, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Else
                neuertext = bildePGvorgangCookieString(probaugVorgange)
                MessageBox.Show("Es wurden " & probaugVorgange.Count & " Vorgänge gefunden: " & Environment.NewLine & Environment.NewLine &
                       neuertext & Environment.NewLine & Environment.NewLine &
                        Environment.NewLine & Environment.NewLine &
                       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                'MsgBox("Es wurden " & probaugVorgange.Count & " Vorgänge gefunden: " & Environment.NewLine & Environment.NewLine &
                '       neuertext & Environment.NewLine & Environment.NewLine &
                '        Environment.NewLine & Environment.NewLine &
                '       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!")
                mergeToPGCookie(neuertext)
                aktualisierePGvorgaengeHistory()
                tabEig.SelectedIndex = 4
            End If

        Catch ex As Exception
            l("fehler btnsucheeigentumer_Click " & ex.ToString)
        End Try

    End Sub

    Private Async Sub tbStrasseFilter_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbStrasseFilter.TextChanged
        If Not istgeladen Then Return
        Dim filter = tbStrasseFilter.Text.Trim()
        If filter.Length < 2 Then
            cmbstrassen.ItemsSource = Nothing
            Return
        End If

        Dim gemeindeKey = If(cmbGemeinden.SelectedValue, "").ToString()
        Try
            Dim result As List(Of myComboBoxItem) =
                Await Task.Run(Function()
                                   Return clsGIStools.getLage(filter, gemeindeKey, mitfkz:=False)
                               End Function)
            If result Is Nothing Then
                Debug.Print("ss")
            End If
            cmbstrassen.ItemsSource = result
            cmbstrassen.DisplayMemberPath = "mySttring"
            cmbstrassen.SelectedValuePath = "myindex"
            cmbstrassen.IsDropDownOpen = (result IsNot Nothing AndAlso result.Count > 0)
        Catch ex As Exception
            l("tbStrasseFilter_TextChanged: " & ex.ToString())
        End Try
    End Sub

    'Private Sub tbStrasse_TextChanged(sender As Object, e As TextChangedEventArgs)

    'End Sub
End Class
