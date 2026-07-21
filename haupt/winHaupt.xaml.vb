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
        Dim gemarkung As String
        'Dim gemeinde, strasse, hausnr, lage, gemeindeindex As String

        LoadHistory() : ComboHistory.ItemsSource = Nothing : ComboHistory.ItemsSource = historyList
        ComboHistory.DisplayMemberPath = "Anzeige"
        If clsActiveDir.getall(Environment.UserName) Then
            Dim result = clsActiveDir.fdkurz
            Title = "BGM, " & clsActiveDir.fdkurz & ", " & My.Settings.Themendatei.Replace(".txt", "").Replace("themendatei", "Thema: ")
            tools.eigentuemerAbfrageErlaubt = (result.ToLower.Contains("umwelt") Or result.ToLower.Contains("bauaufsicht"))
        End If

        ' tools.readFSTCookie(gemarkung, flur, zaehler, nenner, "bgm_FST_cookie.txt")
        'gemarkungsindex = gemarkung

        'nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "startmeup")
        'ausfüllenvermeiden
        ''tbFlur.Text = flur
        ''tbZaehler.Text = zaehler
        ''tbnenner.Text = nenner

        Dim stored = My.Settings.ImmerLogouten ' Boolean (Default: True)
        chkbImmerLogouten.IsChecked = stored
        gisLogouten = stored
        'gisLogouten = False ' vorübergehend deaktiviert, da es zu Problemen führt. Bitte in Zukunft überdenken und ggf. optimieren.
        tools.themendefinitionsdatei = My.Settings.Themendatei



        tabEig.SelectedIndex = My.Settings.ReiterAppNummer

        If isAutho() Then
            ComboHistory.IsDropDownOpen = True
            stpBaulastenmedels.Visibility = Visibility.Visible
            'If Environment.UserName = "Feinen_J" Then
            '    tabEig.SelectedIndex = 2
            '    '3=bplan  2=fst 1=adr  0=baulast
            'Else
            '    'tabEig.SelectedIndex = 1
            'End If
        Else
            'tabEig.SelectedIndex = 1
            stpBaulastenmedels.Visibility = Visibility.Collapsed
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
            'cmbGemarkungen.SelectedIndex = gemIndexInt
            cmbGemarkungen.SelectedIndex = 0

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


        'dummyaufrufStarten()
        Dim liste As List(Of clsFlurstueck) = cls20Cookies.LadeFlurstuecke()
        cmb20fst.ItemsSource = liste
        Dim aliste As List(Of clsAdress) = cls20Cookies.LadeAdressen()
        cmb20adr.ItemsSource = aliste
        Dim vorhabenliste As List(Of clsPGvorhaben) = cls20Cookies.LadePGcookies()
        cmbPGNR.ItemsSource = vorhabenliste


        If My.Settings.ReiterAppNummer = 2 Then
            cmbGemarkungen.IsDropDownOpen = True
        End If '  '3=bplan  2=fst 1=adr  0=baulast
        If My.Settings.ReiterAppNummer = 1 Then
            cmbGemeinden.IsDropDownOpen = True
        End If '  '3=bplan  2=fst 1=adr  0=baulast
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
        setzeReiterAppNummer(0)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        tools.writeBLBlattCookie(tbblnr.Text, "bgm_blattnr_cookie.txt")
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bl_detail")
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
        setzeReiterAppNummer(0)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        'feststellen ob es die baulast gibt - sonst 
        'besteht die gefahr, dass eine veraltete PDF gezogen wird
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bl_pdf")
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
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bl_gis")
        tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
        If clsGIStools.getBaulastFromBaulastMDAT(CInt(tbblnr.Text.Trim), kategorie_guid_Baulasten) Then
            baulastAlsObjImGisZeigen(tbblnr.Text.Trim, tools.themendefinitionsdatei)
        Else
            MessageBox.Show("Diese Baulast gibt es im GIS nicht!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If
    End Sub

    Private Sub btnsucheeigentumer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        setzeReiterAppNummer(2)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "fst_word")
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
            summe = summe & makeFlurstuecksAbstrakt(fkzlist, lage)
            summe = summe.Replace("Aus ProbauG:", "Aus Liegenschaftsbuch:")
            'summe = summe & Environment.NewLine
            'summe = summe & lage & Environment.NewLine
            'summe = summe & Environment.NewLine

            Dim dieNamenderEigentuemer, datei As String
            If toolsEigentuemer.geteigentuemerText(fkzlist(0).flurstueckZuFKZ, dieNamenderEigentuemer) Then
                'summe = summe & Environment.NewLine & dieNamenderEigentuemer
                If isbaulast Then
                    datei = tools.erzeugeWordDateiEigentuemer(summe, "")
                Else
                    datei = tools.erzeugeWordDateiEigentuemer(summe, dateinameFST)
                End If
                Threading.Thread.Sleep(1000)
                Process.Start(datei)
            Else
                'MsgBox(dieNamenderEigentuemer)
                MessageBox.Show(dieNamenderEigentuemer, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If
        Catch ex As Exception
            l("fehler in eigentuemerWord " & ex.ToString)
        End Try
    End Sub

    Private Sub btngis4fst_click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        setzeReiterAppNummer(2)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "fst_gis")
        fkzlist = New List(Of clsFlurstueck)
        fkzlist.Add(aktfst)
        Dim index As Integer = CInt(cmbGemarkungen.SelectedIndex)
        Try
            tools.writeFlurstCookie(index.ToString, (aktfst.flur.ToString), aktfst.zaehler.ToString.Trim, aktfst.nenner.ToString.Trim, "bgm_FST_cookie.txt")
            MitFlurstueckInsGIS(fkzlist, tbFSTbemerkung.Text, False)
            aktualisiereFSTHistory()
        Catch ex As Exception
            l("btnsucheeigentumer_Click " & ex.ToString)
        End Try


        'fkzlist = readFlurst_Form()
        'Dim index As Integer = CInt(cmbGemarkungen.SelectedIndex)
        'Try
        '    tools.writeFlurstCookie(index.ToString, (tbFlur.Text.Trim), tbZaehler.Text.Trim, tbnenner.Text.Trim, "bgm_FST_cookie.txt")
        '    MitFlurstueckInsGIS(fkzlist, tbFSTbemerkung.Text, False)
        '    aktualisiereFSTHistory()
        'Catch ex As Exception
        '    l("btnsucheeigentumer_Click " & ex.ToString)
        'End Try
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


    Private Sub cmbstrassen_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        l("cmbstrassen_SelectionChanged ")
        Dim hausnummernListe As New List(Of myComboBoxItem)
        Dim item As myComboBoxItem = CType(cmbstrassen.SelectedItem, myComboBoxItem)
        Dim gemeindeitem As myComboBoxItem = CType(cmbGemeinden.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            Exit Sub
        End If
        l(item.myindex)
        aktadr.gemeinde_guid = item.myindex.ToString
        Dim a() As String = aktadr.gemeinde_guid.Split("#"c)
        aktadr.strassenkennzeichen = a(1)
        aktadr.gemeinde_guid = a(0)
        aktadr.lageindex = CInt(cmbstrassen.SelectedIndex)
        aktadr.gemeindeindex = CInt(cmbGemeinden.SelectedIndex)
        aktadr.gemeindebigNRstring = cmbGemeinden.SelectedValue.ToString
        aktadr.strasseName = item.mySttring
        aktadr.gemeindeName = gemeindeitem.mySttring
        tbstrasse.Text = aktadr.strasseName
        Dim cb As New myComboBoxItem
        Dim fst As New clsFlurstueck
        Dim strassennamen As New List(Of myComboBoxItem)
        lage_lage = "== Lage: " & aktadr.gemeindeName & ", " & aktadr.strasseName & " " & aktadr.HausKombi & " =="
        'hausnummernListe = clsGIStools.getLage(aktadr.strasseName, aktadr.gemeindebigNRstring, mitfkz:=True, nurstart:=True)
        hausnummernListe = clsGIStools.getHausnummernZuStrasse(aktadr.strasseName, aktadr.strassenkennzeichen, aktadr.gemeinde_guid, mitfkz:=True, nurstart:=True)
        If hausnummernListe Is Nothing OrElse hausnummernListe.Count = 0 Then
            'schneise!
            'über die lage abfragen
            hausnummernListe.Clear()
            tbhausnr.Text = ""
            Dim fkzlist = clsGIStools.getLage(aktadr.strasseName, aktadr.gemeindebigNRstring, mitfkz:=True, nurstart:=True)
            If fkzlist.Count > 0 Then
                fkzlist_lage.Clear()
                fkzlist_lage = bildefstListeAusStrings(fkzlist)
                btnwordADR.IsEnabled = True
                btngis4adr.IsEnabled = True
                btnadr2PG.IsEnabled = False
            Else
                MessageBox.Show("Keine Hausnummern zu dieser Straße gefunden!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If

            Exit Sub
        End If
        cmbhausnr.ItemsSource = hausnummernListe
        cmbhausnr.DisplayMemberPath = "mySttring"
        cmbhausnr.SelectedValuePath = "myindex"
        cmbhausnr.IsDropDownOpen = (hausnummernListe IsNot Nothing AndAlso hausnummernListe.Count > 0)
        l("tbStrasseFilter_TextChanged ende")

    End Sub

    Private Function bildefstListeAusStrings(fkzlist As List(Of myComboBoxItem)) As List(Of clsFlurstueck)
        Dim fstliste As New List(Of clsFlurstueck)
        Dim fst As New clsFlurstueck
        Try
            For i = 0 To fkzlist.Count - 1
                fst = New clsFlurstueck
                fst.Flurstuecksskennzeichen = fkzlist(i).myindex.ToString
                fst.fkzzerlegen()
                fstliste.Add(fst)
                'If i = 0 Then
                '    summe = summe & fkzlist(i).myindex.ToString
                'Else
                '    summe = summe & "," & fkzlist(i).myindex.ToString
                'End If
            Next
            Return fstliste
        Catch ex As Exception
            l("bildefstListeAusStrings " & ex.ToString)
            Return fstliste
        End Try
    End Function

    Private Function bildefkzStringAusStrings(fkzlist As List(Of myComboBoxItem)) As String
        Dim summe = ""
        Try
            For i = 0 To fkzlist.Count - 1
                If i = 0 Then
                    summe = summe & fkzlist(i).myindex.ToString
                Else
                    summe = summe & "," & fkzlist(i).myindex.ToString
                End If
            Next
            Return summe
        Catch ex As Exception
            l("bildefkzStringAusStrings " & ex.ToString)
            Return summe
        End Try
    End Function

    Private Sub btnwordADR_Click(sender As Object, e As RoutedEventArgs)
        'Dim loklist = New List(Of clsFlurstueck)
        'loklist = readFlurst_Form()
        setzeReiterAppNummer(1)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "adr_word")
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
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "adr_gis")
        Dim a = aktadr.gemeindeName
        setzeReiterAppNummer(1)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
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
            l("btngis4adr_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub setzeReiterAppNummer(v As Integer)
        '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        Try
            My.Settings.ReiterAppNummer = v
            My.Settings.Save()
        Catch ex As Exception
            l("setzeReiterAppNummer " & ex.ToString)
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
        setzeReiterAppNummer(3)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bpl_gis")
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
        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbGemarkungen.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            'MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
            MessageBox.Show("Die Eingabe war ungültig. Bitte korrigieren!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        tbzaehlerFilter.Text = ""
        tbFlur.Text = ""
        tbnenner.Text = ""

        aktfst.clear()
        aktfst.gemcode = CInt(cmbGemarkungen.SelectedValue)
        'aktfst.gemindex As Integer = CInt(cmbGemarkungen.SelectedIndex)
        aktfst.gemarkungstext = item.mySttring.ToString
        tools.flurliste = tools.erzeugeFlurliste(aktfst.gemcode)
        cmbFlur.ItemsSource = tools.flurliste
        cmbFlur.DisplayMemberPath = "mySttring"
        cmbFlur.SelectedValuePath = "myindex"
        FocusManager.SetFocusedElement(Me, tbzaehlerFilter)
        cmbFlur.IsDropDownOpen = True
    End Sub

    Private Sub chkbImmerLogouten_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim isChecked = If(chkbImmerLogouten.IsChecked, True, False)
        gisLogouten = isChecked
        My.Settings.ImmerLogouten = isChecked
        'My.Settings.ImmerLogouten = False
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
            nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bpl_pdf")
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
            nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "themen_" & themendatei)
            tools.themendefinitionsdatei = themendatei.Trim


            My.Settings.Themendatei = themendatei.Trim
            My.Settings.Save()
            Title = "BGM, " & clsActiveDir.fdkurz & ", " & My.Settings.Themendatei.Replace(".txt", "").Replace("themendatei", "Thema: ")
        Catch ex As Exception
            l("cmbThemendatei_SelectionChanged " & ex.ToString)
        End Try
    End Sub

    Private Sub btnTutorial_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "tuts")
        Process.Start("https://gis.kreis-of.de/LKOF/upload/tutorial/videos.html")
    End Sub

    Private Sub cmbGemeinden_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbGemeinden.SelectionChanged
        e.Handled = True
        If Not istgeladen Then Exit Sub
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
            aktfst.gemcode = fst.gemcode
            aktfst.flur = fst.flur
            aktfst.zaehler = fst.zaehler
            aktfst.nenner = fst.nenner
            aktfst.AZ = fst.AZ
            aktfst.Flurstuecksskennzeichen = fst.flurstueckZuFKZ


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
            tbstrasse.Text = adr.strasseName
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
        setzeReiterAppNummer(4)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "pg_gis")
        suchePG(True)
    End Sub

    Private Sub suchePG(savecookie As Boolean)
        Dim vorhaben1 As String
        Dim fstliste As List(Of clsFlurstueck)
        Dim metadata As List(Of myComboBoxItem)
        'If clsActiveDir.fdkurz.Contains("mwelt") Then
        '    If CInt(tbPGnr.Text) < 80000 Then

        '        MessageBox.Show("Dem FD Umwelt sind nur Nr > 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        '        If Environment.UserName <> "Feinen_J" Then Exit Sub
        '    End If
        'End If
        'If clsActiveDir.fdkurz.Contains("auaufsicht") Then
        '    If CInt(tbPGnr.Text) > 80000 Then
        '        MessageBox.Show("Dem FD Bauaufsicht sind nur Nr < 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)

        '        If Environment.UserName <> "Feinen_J" Then Exit Sub
        '    End If
        'End If
        'If Not (clsActiveDir.fdkurz.Contains("mwelt") Or clsActiveDir.fdkurz.Contains("auaufsicht")) Then
        '    MessageBox.Show("Dem FD Umwelt sind nur Nr > 80000 erlaubt!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)


        '    If Environment.UserName <> "Feinen_J" Then Exit Sub
        'End If
        fstliste = probaug.klaereanzahlFST(tbPGJahr.Text, tbPGnr.Text, metadata, vorhaben1)
        If fstliste Is Nothing Then
            'MsgBox("Das Aktenzeichen ist ungültig!")
            MessageBox.Show("Das Aktenzeichen ist ungültig!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        Dim fkzstring = probaug.bildeFKZstring(fstliste, 150)
        Dim flurstueckskennzeichen = fkzstring
        tools.gisLogoutUndStartFKZ(flurstueckskennzeichen, gisLogouten)

        Dim result As String
        result = fstliste.Count & "  Flurstücke wurden gefunden!" & Environment.NewLine

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
        'e.Handled = True
        e.Handled = True
        Dim adr = TryCast(cmbPGNR.SelectedItem, clsPGVorhaben)
        If adr IsNot Nothing Then
            tbPGJahr.Text = adr.Jahr
            tbPGnr.Text = adr.Nr
            suchePG(False)
        End If
    End Sub

    Private Sub btnfst2PG_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        setzeReiterAppNummer(2)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "fst_vorgang")
        If Not tools.eigentuemerAbfrageErlaubt Then
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
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
                       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                Dim zieldatei As String
                zieldatei = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
                zieldatei = IO.Path.Combine(zieldatei, "bgm")
                zieldatei = IO.Path.Combine(zieldatei, "Vorgaenge_auf_adresse" & Now.ToString("yyyyMMddhhmm") & ".csv")
                Dim infozeile = "Historische; Vorgänge; auf Flurstück:;" & fkzlist(0).gemarkungstext & ";" & fkzlist(0).flur & ";" & fkzlist(0).fstueckKombi & Environment.NewLine &
                 "Jahr;Akenzeichen;Vorhaben;"
                If tools.erzeugeCSVDateiPGadresse(zieldatei, neuertext, infozeile) Then
                    Process.Start(zieldatei)
                Else
                    'MsgBox("Fehler bei der erzeugung der CSV-Datei: " & zieldatei)
                    MessageBox.Show("Fehler bei der erzeugung der CSV-Datei: " & zieldatei, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                End If
                mergeToPGCookie(neuertext)
                aktualisierePGvorgaengeHistory()
                tabEig.SelectedIndex = 4
            End If

        Catch ex As Exception
            l("fehler btnsucheeigentumer_Click " & ex.ToString)
        End Try

    End Sub

    Private Sub aktualisierePGvorgaengeHistory()
        Dim vorhabenliste As List(Of clsPGVorhaben) = cls20Cookies.LadePGcookies()
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
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "adr_vorgang")
        If Not tools.eigentuemerAbfrageErlaubt Then
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        setzeReiterAppNummer(1)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        Dim lokadr As New clsAdress
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindeName = aktadr.gemeindeName
        lokadr.strasseName = tbstrasse.Text
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.gemeindebigNRstring = aktadr.gemeindebigNRstring
        lokadr.hausNr = aktadr.hausNr
        lokadr.hausZusatz = aktadr.hausZusatz
        lokadr.HausKombi = aktadr.HausKombi
        lokadr.fkz = aktadr.fkz
        Dim probaugVorgange As New List(Of myComboBoxItem)
        Dim aa = aktadr.gemeindeName
        'lokadr.ingradaLageZerlegen(lokadr.strasseName)
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
                MessageBox.Show("Keine Vorgänge zu dieser Adresse und dem entsp. Flurstück gefunden. " & Environment.NewLine &
                    " " & Environment.NewLine &
                    "  " & Environment.NewLine, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Else
                neuertext = bildePGvorgangCookieString(probaugVorgange)
                MessageBox.Show("Es wurden " & probaugVorgange.Count & " Vorgänge gefunden: " & Environment.NewLine & Environment.NewLine &
                       "Diese Vorgänge werden unter dem Reiter 'ProBauG' der Combobox zuaddiert!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                Dim zieldatei As String
                zieldatei = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
                zieldatei = IO.Path.Combine(zieldatei, "bgm")
                zieldatei = IO.Path.Combine(zieldatei, "Vorgaenge_auf_adresse" & Now.ToString("yyyyMMddhhmmss") & ".csv")
                Dim infozeile = "Historische; Vorgänge; auf Adresse:;" & aktadr.gemeindeName & ";" & aktadr.strasseName & ";" & aktadr.HausKombi & Environment.NewLine &
                 "Jahr;Akenzeichen;Vorhaben;"
                If tools.erzeugeCSVDateiPGadresse(zieldatei, neuertext, infozeile) Then
                    Process.Start(zieldatei)
                Else
                    'MsgBox("Fehler bei der erzeugung der CSV-Datei: " & zieldatei)
                    MessageBox.Show("Fehler bei der erzeugung der CSV-Datei: " & zieldatei, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                End If

                mergeToPGCookie(neuertext)
                aktualisierePGvorgaengeHistory()
                tabEig.SelectedIndex = 4
            End If

        Catch ex As Exception
            l("fehler btnsucheeigentumer_Click " & ex.ToString)
        End Try

    End Sub

    Private Sub tbStrasseFilter_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbStrasseFilter.TextChanged
        e.Handled = True
        If Not istgeladen Then Exit Sub
        sucheFilteredStrassen()
    End Sub

    Private Sub sucheFilteredStrassen()
        Dim filter = tbStrasseFilter.Text
        If filter.Length < 3 Then
            cmbstrassen.ItemsSource = Nothing
            l("tbStrasseFilter_TextChanged: Filter zu kurz  " & filter)
            Exit Sub
        End If
        l("neustart")
        'Dim item As myComboBoxItem = CType(cmbstrassen.SelectedItem, myComboBoxItem)
        Dim gemeindeitem As myComboBoxItem = CType(cmbGemeinden.SelectedItem, myComboBoxItem)
        Dim nurstart As Boolean = True
        If chkNurStart.IsChecked Then
            nurstart = True
        Else
            nurstart = False
        End If
        l("gemeindeitem mySttring " & gemeindeitem.myindex & "  " & filter)

        Try
            'Dim dieNamenderEigentuemer As List(Of myComboBoxItem) =
            '    Await Task.Run(Function()
            '                       Return clsGIStools.getLage(filter, gemeindeitem.myindex, mitfkz:=False)
            '                   End Function)
            'Dim dieNamenderEigentuemer As List(Of myComboBoxItem) = clsGIStools.getLage(filter, gemeindeitem.myindex, mitfkz:=False, nurstart)
            Dim gemeindenummer = gemeindeitem.myindex.Replace("06438", "")
            Dim result As List(Of myComboBoxItem) = clsGIStools.getStrassennamen(filter, gemeindenummer, mitfkz:=False, nurstart)
            If result Is Nothing Then
                'Debug.Print("ss")
                'l("tbStrasseFilter_TextChanged: Keine Ergebnisse gefunden")
            End If
            'l("tbStrasseFilter_TextChanged: Ergebnisse gefunden: " & If(dieNamenderEigentuemer IsNot Nothing, dieNamenderEigentuemer.Count.ToString(), "0"))
            cmbstrassen.ItemsSource = result
            cmbstrassen.DisplayMemberPath = "mySttring"
            cmbstrassen.SelectedValuePath = "myindex"
            cmbstrassen.IsDropDownOpen = (result IsNot Nothing AndAlso result.Count > 0)
            l("tbStrasseFilter_TextChanged ende")
        Catch ex As Exception
            l("tbStrasseFilter_TextChanged: " & ex.ToString())
        End Try
    End Sub

    Private Sub btnBLloeschen_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        setzeReiterAppNummer(0)   '4=probaug  3=bplan  2=fst 1=adr  0=baulast
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "bl_loeschen")
        tools.writeBLBlattCookie(tbblnr.Text.Trim, "bgm_blattnr_cookie.txt")
        baulastAlsObjImGisZeigen(tbblnr.Text.Trim, tools.themendefinitionsdatei)
        Process.Start("\\kh-w-ingrada\GIS-Daten\diverses\bgmingrada\objektImGisLoeschen.rtf")
    End Sub

    Private Sub cmbFlur_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbFlur.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            'MessageBox.Show("Die Eingabe war ungültig. Bitte korrigieren!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        aktfst.flur = CInt(item.mySttring)
        tbZaehler.Text = ""
        tbnenner.Text = ""
        tbzaehlerFilter.Text = ""
        'aktfst.gemindex As Integer = CInt(cmbGemarkungen.SelectedIndex)
        'aktfst.gemarkungstext = item.mySttring.ToString
        tools.fstkombiliste = tools.erzeugeFSTkombiliste(aktfst.gemcode, aktfst.flur)
        cmbFstKombi.ItemsSource = tools.fstkombiliste
        cmbFstKombi.DisplayMemberPath = "mySttring"
        cmbFstKombi.SelectedValuePath = "myindex"
        'FocusManager.SetFocusedElement(Me, tbzaehlerFilter)
        tbzaehlerFilter.Focus()
        'cmbFstKombi.IsDropDownOpen = True
        tbFlur.Text = aktfst.flur.ToString


    End Sub

    Private Sub cmbFstKombi_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        Dim item As myComboBoxItem = CType(cmbFstKombi.SelectedItem, myComboBoxItem)
        If item Is Nothing Then
            'MsgBox("Die Eingabe war ungültig. Bitte korrigieren!")
            'MessageBox.Show("Die Eingabe war ungültig. Bitte korrigieren!", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        aktfst.Flurstuecksskennzeichen = (item.myindex)
        Dim a() As String
        a = item.mySttring.Split("/"c)
        aktfst.zaehler = CInt(a(0))
        aktfst.nenner = CInt(a(1))
        'aktfst.gemindex As Integer = CInt(cmbGemarkungen.SelectedIndex)
        'aktfst.gemarkungstext = item.mySttring.ToString
        'tools.fstkombiliste = tools.erzeugeFSTkombiliste(aktfst.gemcode, aktfst.flur)
        'cmbFstKombi.ItemsSource = tools.fstkombiliste
        'cmbFstKombi.DisplayMemberPath = "mySttring"
        'cmbFstKombi.SelectedValuePath = "myindex"
        'cmbFstKombi.IsDropDownOpen = True
        tbZaehler.Text = aktfst.zaehler.ToString
        tbnenner.Text = aktfst.nenner.ToString
    End Sub

    Private Sub tbzaehlerFilter_TextChanged(sender As Object, e As TextChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        Dim filter = tbzaehlerFilter.Text.Trim()
        If filter.Length < 1 Then
            cmbFstKombi.ItemsSource = Nothing
            Return
        End If
        Dim gefilterteListe = tools.fstkombiliste.Where(Function(x) x.mySttring.StartsWith(filter)).ToList()
        cmbFstKombi.ItemsSource = gefilterteListe
        cmbFstKombi.DisplayMemberPath = "mySttring"
        cmbFstKombi.SelectedValuePath = "myindex"
        cmbFstKombi.IsDropDownOpen = (gefilterteListe IsNot Nothing AndAlso gefilterteListe.Count > 0)
    End Sub

    Private Sub chkNurStart_Checked(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        sucheFilteredStrassen()
    End Sub

    Private Sub btnKreisgis_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim url As String
        Dim themen As String
        tools.themendefinitionsdatei = My.Settings.Themendatei
        themen = tools.getthemen("", tools.themendefinitionsdatei)
        l(" themen " & themen)
        'theme=BauenUndUmwelt,Eigene%20Daten,Grenzen,Liegenschaften
        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        If gisLogouten Then
            Process.Start(logout)
            Threading.Thread.Sleep(1000)
        End If

        url = "https://gis.kreis-of.de/LKOF/asp/main.asp?" & themen & "&skipwelcome=true"
        l("url: " & url)
        Process.Start(url)

    End Sub

    Private Sub btnOptionen_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        tabEig.SelectedItem = tabOptionen
    End Sub

    Private Sub btnsucheeigentuemer_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim vergleichName As String = "="
        Dim vergleichVName As String = "="
        Dim name, vname As String
        If Not tools.eigentuemerAbfrageErlaubt Then
            MessageBox.Show("Keine Rechte vorhanden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        nutzprotokoll.NutzungProtokollieren(AppDomain.CurrentDomain.BaseDirectory, "eigentuemerbulk")
        tbeigbulkAuswahl.Text = ""
        btnBULKexcel.IsEnabled = False
        btnBULKimGIS.IsEnabled = False
        cmbBULKeig.ItemsSource = Nothing

        If tbeigbulkNAME.Text = String.Empty Then
            MsgBox("Der name fehlt")
            Exit Sub
        End If
        If tbeigbulkNAME.Text.Count < 2 Then
            MsgBox("Der Name ist zu kurz")
            Exit Sub
        End If

        name = tbeigbulkNAME.Text
        vname = tbeigbulkVORNAME.Text
        If tbeigbulkNAME.Text.Contains("*") Then
            name = tbeigbulkNAME.Text.Replace("*", "%")
            vergleichName = "like"
        End If
        If tbeigbulkVORNAME.Text.Contains("*") Then
            vname = tbeigbulkVORNAME.Text.Replace("*", "%")
            vergleichVName = "like"
        End If

        'Dim vgerleichname = DirectCast(cmbBULKvergleichName.SelectedItem, ComboBoxItem).Content.ToString()
        'Dim vgerleichvname = DirectCast(cmbBULKvergleichVorname.SelectedItem, ComboBoxItem).Content.ToString()

        setzeReiterAppNummer(6)   '6=eigentümer  4=probaug  3=bplan  2=fst 1=adr  0=baulast

        mapTools.BULKeigentuemerliste = mapTools.getBULKeigentuemervorschlaege(name, vergleichName,
                                                                      vname, vergleichVName)
        If mapTools.BULKeigentuemerliste.Count > 0 Then
            btnBULKexcel.IsEnabled = True
            btnBULKimGIS.IsEnabled = True
        Else
            tbeigbulkAuswahl.Text = "nothing"
        End If
        cmbBULKeig.ItemsSource = mapTools.BULKeigentuemerliste
        cmbstrassen.DisplayMemberPath = "mySttring"
        cmbBULKeig.SelectedValuePath = "myindex"
        cmbBULKeig.IsDropDownOpen = True
    End Sub

    Private Sub cmbBULKeig_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        l("cmbBULKeig_SelectionChanged ")
        If cmbBULKeig.SelectedItem Is Nothing Then Exit Sub
        Dim personAuswahl As myComboBoxItem = CType(cmbBULKeig.SelectedItem, myComboBoxItem)
        Dim namensteile As String()
        namensteile = personAuswahl.myindex.Split("#"c)
        mapTools.BULKfst2nameList = mapTools.getFST4nameVname(namensteile(0), namensteile(1), namensteile(2), namensteile(3), namensteile(4))
        tbeigbulkAuswahl.Text = personAuswahl.myindex.Replace("#", " ") & Environment.NewLine &
                                 mapTools.BULKfst2nameList.Count & " Flurstücke gefunden."

        'flurstueckskennzeichen


    End Sub

    Private Sub btnBULKexcel_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim zieldatei As String
        zieldatei = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
        zieldatei = IO.Path.Combine(zieldatei, "bgm")
        zieldatei = IO.Path.Combine(zieldatei, "FSTliste" & Now.ToString("yyyyMMddhhmm") & ".csv")
        If tools.erzeugeCSVDateiFSTbulk(zieldatei, mapTools.BULKfst2nameList, tbeigbulkAuswahl.Text) Then
            Process.Start(zieldatei)
        Else
            'MsgBox("Fehler bei der erzeugung der CSV-Datei: " & zieldatei)
            MessageBox.Show("Fehler bei der erzeugung der CSV-Datei: " & zieldatei, "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        End If
    End Sub

    Private Sub btnBULKimGIS_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim maximum = 150
        If mapTools.BULKfst2nameList.Count > 150 Then
            MessageBox.Show("Hinweis: Es können nicht mehr als 150 Flurstücke im GIS dargestellt werden: " & mapTools.BULKfst2nameList.Count & " sind zuviel", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)

        End If
        Dim fkzstring = probaug.bildeFKZstring(mapTools.BULKfst2nameList, maximum)
        tools.gisLogoutUndStartFKZ(fkzstring, gisLogouten)
    End Sub

    Private Sub tbPGExcel_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub btnAktuell_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim appDir = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        Dim aktuelles = IO.Path.Combine(appDir, "aktuelles.html")
        Process.Start(aktuelles)
    End Sub

    Private Sub cmbhausnr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        If Not istgeladen Then Exit Sub
        If cmbhausnr.SelectedItem Is Nothing Then
            Exit Sub
        End If
        Dim hausnummernListe As New List(Of myComboBoxItem)
        'Dim item As myComboBoxItem = CType(cmbhausnr.SelectedItem, myComboBoxItem)
        Dim hausnritem As myComboBoxItem = CType(cmbhausnr.SelectedItem, myComboBoxItem)
        If hausnritem.myindex Is Nothing Then
            aktadr.hausNr = CInt(hausnritem.mySttring.Trim)
            aktadr.hausZusatz = ""
            aktadr.HausKombi = hausnritem.mySttring.Trim
        Else
            Dim a() As String
            a = hausnritem.myindex.Split("#"c)
            aktadr.hausNr = CInt(a(0))
            aktadr.hausZusatz = a(1)
            aktadr.HausKombi = aktadr.hausNr & " " & aktadr.hausZusatz
        End If

        lage_lage = "== Lage: " & aktadr.gemeindeName & ", " & aktadr.strasseName & " " & aktadr.HausKombi & " =="

        aktadr.HausKombi = hausnritem.mySttring.Trim
        tbhausnr.Text = aktadr.HausKombi
        aktadr.fkz = getFKZ4Hausnr(aktadr.strassenkennzeichen, aktadr.hausNr, aktadr.hausZusatz)

        fkzlist_lage.Clear()
        Dim fsttemp As New clsFlurstueck
        fsttemp.Flurstuecksskennzeichen = aktadr.fkz
        fsttemp.fkzzerlegen()
        fkzlist_lage.Clear()
        fkzlist_lage.Add(fsttemp)

        If aktadr.fkz Is Nothing Or aktadr.fkz = String.Empty Then
            MessageBox.Show("Kein entsprechendes Flurstück gefunden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            btnwordADR.IsEnabled = False
            btngis4adr.IsEnabled = False
            btnadr2PG.IsEnabled = False
            Exit Sub
        Else
            btnwordADR.IsEnabled = True
            btngis4adr.IsEnabled = True
            btnadr2PG.IsEnabled = True
        End If

    End Sub

    Private Function getFKZ4Hausnr(strassenkennzeichen As String, hausNr As Integer, hausZusatz As String) As String
        Dim fkz = ""
        Try
            l("getFKZ4Hausnr " & strassenkennzeichen)
            l("hausNr " & hausNr)
            l("hausZusatz " & hausZusatz)
            Dim suchausdruck As String
            If hausZusatz.Trim = String.Empty Then
                fstREC.mydb.SQL = "SELECT distinct flst_flurstueckskennzeichen  FROM [LKOF].[dbo].[tbl_lieg_strasse2flurstueck]  where strasse_strassenkennzeichen='" &
                      strassenkennzeichen.Trim & "'" &
                    " And hausnummer = '" & hausNr.ToString.Trim & "' "
            Else
                fstREC.mydb.SQL = "SELECT distinct flst_flurstueckskennzeichen  FROM [LKOF].[dbo].[tbl_lieg_strasse2flurstueck]  where strasse_strassenkennzeichen='" &
                           strassenkennzeichen.Trim & "'" &
                         " And hausnummer = '" & hausNr.ToString.Trim & "' " &
                         " And zusatz = '" & hausZusatz.Trim & "' "
            End If

            l(fstREC.mydb.SQL)
            Dim hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count > 0 Then
                'If mitfkz Then
                For i = 0 To fstREC.dt.Rows.Count - 1
                    fkz = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item(0)).ToString.Trim
                Next
            Else
                MessageBox.Show("Kein entsprechendes Flurstück gefunden", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            End If
            Return fkz
        Catch ex As Exception
            l("Fehler in getFKZ4Hausnr " & ex.ToString)
            Return Nothing
        End Try
    End Function

End Class
