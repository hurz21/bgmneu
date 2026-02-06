Imports System.Security.Policy
Imports Org.BouncyCastle.Asn1.Esf

Public Class winDetail
    Property VGmyBitmapImage As New BitmapImage
    Private istgeladen As Boolean = False
    Public Property ObjektGuid As String = "88AFE39F-78FC-4053-BE6D-315E3745CF45"
    Public Property quelleSQL As String = "gisview2belastet"
    Public Property targetGISTabelle As String = "hartmann"
    Dim modus As String = "neu"
    Dim nurlesen As Boolean = True
    Sub New(gisID As String, _nurlesen As Boolean)
        InitializeComponent()
        If IsNumeric(gisID) AndAlso CInt(gisID) < 1 Then
            modus = "neu"
        Else
            modus = "edit"
            tbBaulastNr.Text = CType(gisID, String)
        End If
        nurlesen = _nurlesen
    End Sub
    Sub New()
        InitializeComponent()
        modus = "edit"
        tbBaulastNr.Text = ""
    End Sub

    Private Sub winDetail_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        e.Handled = True
        Dim abbruch As Boolean = False
        l("windetail loaded anfang")
        'btndigit.Visibility = Visibility.Collapsed
#If DEBUG Then
        If tbBaulastNr.Text.IsNothingOrEmpty Then
            tbBaulastNr.Text = "2026"
        End If
#End If
        If IsNumeric(tbBaulastNr.Text) Then
            refreshProbaug(CInt(tbBaulastNr.Text), quelleSQL, abbruch)
            If abbruch Then
                Close()
                End
            End If
            refreshGIS(CInt(tbBaulastNr.Text))
            'refreshTIFFbox()
            'hier wird firstrange calculiert
            'gidInString = clsGIStools.bildegidstring()
            'range = clsGIStools.calcNewRange(gidInString)
            'If Not range.istBrauchbar Then
            '    'btndigit.Visibility = Visibility.Visible
            '    'If My.Computer.Clipboard.ContainsText Then
            '    '    tools.wkt = My.Computer.Clipboard.GetText()
            '    '    If tools.wkt.Trim.ToLower.StartsWith("polygon") Then
            '    '        btndigit.ToolTip = "Klick = Übernehmen dieser Geometrie als temporäres Flurstück !" & tools.wkt
            '    '    Else
            '    '        btndigit.ToolTip = "Das ist keine gültige Geometrie: " & tools.wkt
            '    '    End If
            '    'Else
            '    '    MessageBox.Show("Sie können ein Flurstück selber markieren ! Näheres bei Frau Hartmann. ")
            '    'End If
            'End If
            'refreshMap()
            'flurstueckskennzeichen = tools.FSTausGISListe(0).flurstueckZuFKZ
            'tools.flurstueckZuFKZ(tools.FSTausGISListe(0).gemcode.ToString,
            '                                     tools.FSTausGISListe(0).flur.ToString,
            '                                     tools.FSTausGISListe(0).zaehler.ToString,
            '                                     tools.FSTausGISListe(0).nenner.ToString)
            Dim summe = ""
            For Each fst As clsFlurstueck In tools.FSTausGISListe
                summe = summe & "== Grundstück: " & fst.gemeindename & ", Gemarkung: " &
                    fst.gemarkungstext & ", Flur: " &
                    fst.flur & ", Fst: " &
                    fst.zaehler & "/" & fst.nenner & " =="
            Next
            summe = summe & Environment.NewLine
            tbEigentuemer.Text = summe & Environment.NewLine &
                toolsEigentuemer.geteigentuemerText(tools.FSTausGISListe)
        End If

        setTitle()
        showpdf()
        'If nurlesen Then
        '    dpMain.IsEnabled = False
        '    btnPDFaufrufen.IsEnabled = True

        'End If
        istgeladen = True
        l("windetail loaded ende")
    End Sub

    Private Sub setTitle()
        Title = "BGM: BaulastenGISManager 0.11. " & Environment.UserName & " V.: " & bgmVersion
    End Sub

    Private Sub refreshTIFFbox()
        'refreshTiffBitmap()
        If rawList.Count > 0 Then

            Dim fi As New IO.FileInfo(rawList(0).datei)
            If fi.Exists Then
                tbFiledate.Text = "Scan: " & fi.LastWriteTime.ToShortDateString
                tbFiledate.Foreground = New SolidColorBrush(Colors.Green)
                tbFiledate.Background = New SolidColorBrush(Colors.LightGray)
            Else
                tbFiledate.Text = "fehlt"
                tbFiledate.Foreground = New SolidColorBrush(Colors.Red)
                tbFiledate.Background = New SolidColorBrush(Colors.White)
            End If
        Else
            tbFiledate.Text = "keine gisdaten"
        End If
    End Sub

    Private Sub refreshGIS(BaulastBlattNr As Integer)
        dgAusGIS.DataContext = Nothing
        tools.FSTausGISListe.Clear()
        Dim hinweis As String = ""

        hinweis = clsGIStools.getGISrecord(BaulastBlattNr)
        'If tools.FSTausGISListe.Count < 1 Then
        '    tools.FSTausGISListeFehlt = clsGIStools.fromProbauGObjekt(FSTausPROBAUGListe)
        'Else
        If hinweis.StartsWith("(Noch") Then
            tbGISinfo.Text = hinweis
            stpPDF.Visibility = Visibility.Collapsed
            btnZumGIS.IsEnabled = False
            btnZumGISOBJ.IsEnabled = False
            btnZumGISPROBAUG.Content = "im GIS erfassen"
            btnZumGISPROBAUG.Width = 400
            btnZumGISPROBAUG.Height = 30

        Else
            tools.FSTausGISListe = clsGIStools.fstGIS2OBJ()
            ObjektGuid = tools.FSTausGISListe(0).GUID
            If tools.FSTausGISListe Is Nothing Then
                MsgBox("Die im GIS-Baulastkataster hinterlegten Flurstücksinfos sind mangelhaft. Bitte verbessern!")
            End If
            stpPDF.Visibility = Visibility.Visible
            dgAusGIS.DataContext = tools.FSTausGISListe
            tbGISinfo.Text = ""
            btnZumGIS.IsEnabled = True
            btnZumGISOBJ.IsEnabled = True
            btnZumGISPROBAUG.Content = "im GIS anzeigen"
            btnZumGISPROBAUG.Width = 100
            btnZumGISPROBAUG.Height = 15
        End If

        l("getSerialFromBasis---------------------- ende")
    End Sub



    Private Sub btnAusProbaug_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim abbruch As Boolean = False
        refreshGIS(CInt(tbBaulastNr.Text))
        refreshEigentuemer(CInt(tbBaulastNr.Text))
        'gidInString = clsGIStools.bildegidstring()
        'range = clsGIStools.calcNewRange(gidInString)
        refreshall(abbruch)
    End Sub

    Private Sub refreshEigentuemer(v As Integer)
        'Throw New NotImplementedException()
        tbEigentuemer.Text = ""
    End Sub

    Private Sub refreshall(abbruch As Boolean)

        refreshProbaug(CInt(tbBaulastNr.Text), quelleSQL, abbruch)
        If abbruch Then Exit Sub
        refreshGIS(CInt(tbBaulastNr.Text))
        'refreshTIFFbox()
        'refreshMap()
        showpdf()
    End Sub
    Private Sub clearCanvas()
        GC.Collect()
        VGmapCanvas.Children.Clear()
        If VGcanvasImage IsNot Nothing Then
            VGcanvasImage.Source = Nothing
            VGcanvasImage = Nothing
        End If
        VGcanvasImage = New Image
        leeresbild(VGcanvasImage)

    End Sub
    Private Sub leeresbild(canvasImage As Image)
        Dim myBitmapImage As New BitmapImage()
        Dim aufruf As String = tools.srv_host_web & "/apps/paradigma/ndman/leer.png" '"P:\a_vs\NEUPara\mgis\leer.png"
        Try
            myBitmapImage.BeginInit()
            myBitmapImage.UriSource = New Uri(aufruf, UriKind.Absolute)
            myBitmapImage.EndInit()
            canvasImage.Source = myBitmapImage
            GC.Collect()
        Catch ex As Exception
            l("fehler in leeresbild: " & aufruf & " /// " & ex.ToString)
        End Try
    End Sub
    Private Sub setPreviewImageFromHttpURL(url As String)
        'https mach tprobleme
        'Dim VGcanvasImage = New Image
        Try
            l(" setImageFromHttpURL ---------------------- anfang")
            clearCanvas()
            VGcanvasImage = New Image
            VGcanvasImage.Name = "canvasImage"
            VGmapCanvas.Children.Add(VGcanvasImage)
            VGmapCanvas.SetZIndex(VGcanvasImage, 100)

            VGmyBitmapImage = New BitmapImage
            VGmyBitmapImage.BeginInit()
            VGmyBitmapImage.UriSource = New Uri(url, UriKind.Absolute)
            VGmyBitmapImage.EndInit()
            AddHandler VGmyBitmapImage.DownloadCompleted, AddressOf vgmyBitmapImage_DownloadCompleted
            Threading.Thread.Sleep(900)
            'VGcanvasImage.Source = VGmyBitmapImage
            l(" setImageFromHttpURL ---------------------- ende")
        Catch ex As Exception
            l("Fehler in setImageFromHttpURL: " & ex.ToString())
        End Try
    End Sub
    Private Sub vgmyBitmapImage_DownloadCompleted(sender As Object, e As EventArgs)
        VGcanvasImage.Source = VGmyBitmapImage
        'clstools.saveImageasThumbnail2(clstools.auswahlBplan, clstools.BPLcachedir, VGmyBitmapImage)
    End Sub
    Private Sub refreshMap()
        'Dim url As String = mapTools.genPreviewURL(tools.range, CInt(VGmapCanvas.Width), CInt(VGmapCanvas.Height), "flurkarte", 10, tools.gidInString)
        'setPreviewImageFromHttpURL(url)
        'Canvas.SetTop(VGcanvasImage, 0)
        'Canvas.SetLeft(VGcanvasImage, 0)
    End Sub

    Sub refreshProbaug(baulastblattnr As Integer, sqlquelle As String, ByRef abbruch As Boolean)

        Try
            l(" MOD refreshProbaug anfang")
            dgAusProbaug.DataContext = Nothing
            tools.FSTausPROBAUGListe.Clear()

            clsProBGTools.holeProBaugDaten(baulastblattnr, sqlquelle, abbruch)
            'abbruch = False
            If abbruch Then
                MsgBox("Anwendung wird beendet !")
                abbruch = True
                Exit Sub
            End If
            'clsProBGTools.holeProBaugDatenZusatz(baulastblattnr, sqlquelle)
            dgAusProbaug.DataContext = FSTausPROBAUGListe
            tbBauort.Text = rawList(0).bauortNr
            tbDatum1.Text = rawList(0).datum1
            tbgueltig.Text = rawList(0).gueltig
            tbGemeinde.Text = rawList(0).gemeindeText
            tbBaulastNr2.Text = rawList(0).baulastnr
            tbBlattnr.Text = rawList(0).blattnr
            tblaufNR.Text = CType(rawList(0).laufnr, String)

            l(" MOD refreshProbaug ende")
        Catch ex As Exception
            l("Fehler in refreshProbaug: " & ex.ToString())
        End Try
    End Sub

    Public Function refreshTiffBitmap() As Boolean
        Return True
        'Dim bitmap As BitmapImage = New BitmapImage()

        'Try
        '    l(" MOD refreshTiffBitmap anfang")
        '    If rawList(0).dateiExistiert Then
        '        'btnTiffaufrufen.Visibility = Visibility.Visible 
        '        bitmap.BeginInit()
        '        bitmap.CacheOption = BitmapCacheOption.OnLoad ' verhindert fehler beim löschen
        '        bitmap.UriSource = New Uri(rawList(0).datei)
        '        bitmap.EndInit()
        '        imgTiff.Source = bitmap
        '        bitmap = Nothing
        '        Return True
        '    Else
        '        'btnTiffaufrufen.Visibility = Visibility.Collapsed
        '        Return Nothing
        '    End If

        '    l(" MOD refreshTiffBitmap ende")
        'Catch ex As Exception
        '    l("Fehler in refreshTiffBitmap: " & ex.ToString())
        '    Return False
        'End Try
    End Function



    'Private Sub btnGISeintraegeLoeschen_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    Dim anzahl As Integer
    '    anzahl = clsGIStools.loeschenGISDatensatz(tbBaulastNr.Text)
    '    MessageBox.Show("Es wurden Datensätze in GIS-Tabelle gelöscht: " & anzahl)
    '    refreshGIS(CInt(tbBaulastNr.Text))
    '    refreshMap()
    'End Sub

    'Private Sub btnVonProbaugNachGISkopieren_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    setzeQuellUndTargetTabelle()
    '    IO.Directory.CreateDirectory(tools.baulastenoutDir)
    '    getAllSerials(anzahl_mitSerial, tools.baulastenoutDir & "\Baulasten_ohneAktFlurstueck" & Now.ToString("yyyyMMddhhmm") & ".csv")
    '    If anzahl_mitSerial < 1 Then
    '        MsgBox("In der DB wurden KEINE geometrien gefunden!!!! " & vbNewLine &
    '               "Bitte per hand nachdigitalisieren")
    '        btndigit.Visibility = Visibility.Visible
    '    Else
    '        ___showdispatcher("  BL mit Geometrie: " & anzahl_mitSerial & Environment.NewLine)
    '        ___showdispatcher("BL werden in die DB geschrieben ...  bitte warten " & Environment.NewLine)

    '        'writeallWithSerials(CBool(cbAuchUnguetige.IsChecked), 1, targetGISTabelle) '1=aus katasterdaten übernommen
    '        writeallWithSerials(False, 1, targetGISTabelle) '1=aus katasterdaten übernommen

    '        ___showdispatcher("  ausschreiben fertig: " & Environment.NewLine)
    '        refreshGIS(CInt(tbBaulastNr.Text))
    '        Dim gidstring As String = clsGIStools.bildegidstring()
    '        range = clsGIStools.calcNewRange(gidstring)
    '        refreshMap()
    '    End If
    'End Sub
    'Sub writeallWithSerials(auchUngueltige As Boolean, genese As Integer, outputTablename As String)
    '    Dim iz As Integer = 0
    '    Dim erfolg As Boolean
    '    Dim sql As String
    '    Dim coordinatesystemNumber As String = "25832" '31467"'25832lt mapfile

    '    Dim datei As String = ""
    '    Dim datei2 As String = ""
    '    Try
    '        l("writeallWithSerials---------------------- anfang")
    '        For Each lok As clsBaulast In rawList
    '            Console.WriteLine("getAllSerials " & iz)
    '            If lok.blattnr = "8001" Then
    '                Debug.Print("")
    '            End If
    '            If lok.blattnr = "90764" Then
    '                Debug.Print("")
    '            End If
    '            If Not lok.katasterFormellOK Or lok.geloescht Then Continue For
    '            If lok.serial.IsNothingOrEmpty Then Continue For
    '            iz += 1
    '            datei = lok.datei.Replace(srv_unc_path & "\", "").Replace("\", "/")
    '            datei = datei.Replace("flurkarte.basis_f", "flurkarte.aktuell")
    '            datei = datei.Replace("h_flurkarte.j", "hist.Flurkarte.")
    '            datei = datei.Replace("_flurstueck_f", "")
    '            datei = datei.Replace("_basis_f", "")
    '            datei2 = datei
    '            If lok.dateiExistiert Then
    '            Else
    '                datei = "KeineDaten.htm"
    '            End If
    '            ___showdispatcher(" db ausschreiben  " & iz & " (" & anzahl_mitSerial & ")" & Environment.NewLine)
    '            If lok.geloescht Then Continue For

    '            If auchUngueltige Then
    '                write2postgis(lok, erfolg, sql, coordinatesystemNumber, datei, datei2, genese, outputTablename)
    '            Else
    '                If lok.gueltig.ToLower = "j" Then
    '                    write2postgis(lok, erfolg, sql, coordinatesystemNumber, datei, datei2, genese, outputTablename)
    '                End If
    '            End If


    '        Next
    '        l("writeallWithSerials---------------------- ende")
    '    Catch ex As Exception
    '        l("Fehler in writeallWithSerials: " & ex.ToString())
    '    End Try
    'End Sub
    Private Sub ___showdispatcher(v As String)

    End Sub

    Private Sub btnZumGIS_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim url As String
        '91197
        Try
            If tools.FSTausGISListe.Count > 0 Then
                flurstueckskennzeichen = tools.FSTausGISListe(0).flurstueckZuFKZ
                'url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                'url = "https://gis.kreis-of.de/LKOF/extensions/logout.asp?removeLostSession=true"
                'Process.Start(url)
                url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                l("url " & url)
                Process.Start(url)
                l(flurstueckskennzeichen)
            Else
                MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
                url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
                l("url " & url)
                Process.Start(url)
            End If
            'Dim gidstring As String = clsGIStools.bildegidstring()
            'range = clsGIStools.calcNewRange(gidstring)

            'Dim param, rangestring As String

            'Dim lu, ro As New myPoint
            'lu.X = range.xl
            'lu.Y = range.yl
            'ro.X = range.xh
            'ro.Y = range.yh
            'rangestring = clsGIStools.calcrangestring(lu, ro)
            'param = "modus=""bebauungsplankataster""  range=""" & rangestring & ""
            'Process.Start(tools.gisexe, param)

        Catch ex As Exception

            l(ex.ToString)
            'Return "fehler in btnZumGIS_Click"
        End Try
    End Sub

    Private Function makeurl4FST(v As String, results As String) As String
        l("in makurl")
        Try
            '&skipwelcome=true
            v = v & "app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val="
            v = v & results & "&skipwelcome=true"
            Return v
            'https://gis.kreis-of.de/LKOF/asp/main.asp?app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val=060729-005-00490/0000.000&skipwelcome=true
            ' Die endung  .000  ist wichtig - sonst gehts nicht
        Catch ex As Exception
            l(ex.ToString)
            Return "fehler in makeurl4FST"
        End Try
    End Function

    Private Function makeurl4Baulast(httpstring As String, baulast As String) As String
        l("in makurl")
        Try
            '&skipwelcome=true
            httpstring = httpstring & "lay=sp_mdat_0010_F&fld=text3&typ=string&val="
            httpstring = httpstring & baulast & "&skipwelcome=true"
            Return httpstring
            ' https://gis.kreis-of.de/LKOF/asp/main.asp?lay=sp_mdat_0010_F&fld=text3&typ=string&val=1001&skipwelcome=true
            ' Die endung  .000  ist wichtig - sonst gehts nicht
        Catch ex As Exception
            l(ex.ToString)
            Return "fehler in makeurl4FST"
        End Try
    End Function

    'Private Sub dropped(sender As Object, e As DragEventArgs)
    '    e.Handled = True
    '    'droptiff(e)
    '    dropPDF(e)
    'End Sub

    Private Sub dropPDF(e As DragEventArgs)
        Dim filenames As String()
        Dim zielname As String = ""
        Dim endung As String = ".pdf"
        Dim listeZippedFiles, listeNOnZipFiles, allFeiles As New List(Of String)
        Dim titelVorschlag As String = ""
        Try
            l(" MOD dropped anfang")
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                filenames = CType(e.Data.GetData(DataFormats.FileDrop), String())
            End If

            If filenames(0).ToLower.EndsWith(".pdf") Then
                endung = ".pdf"
            End If
            l(" MOD dropped 2")
            If filenames(0).ToLower.EndsWith(endung) Then
                l(" MOD dropped 3")
                zielname = IO.Path.Combine(srv_unc_path & "BAUL4ST_" & tbBaulastNr.Text.Trim & endung).Trim
                Dim fi As New IO.FileInfo(zielname)
                If Not fi.Exists Then
                    Dim mesres = MessageBox.Show("Möchten Sie die Datei überschreiben ?" & Environment.NewLine &
                                                    "  Ja    - Überschreiben " & Environment.NewLine &
                                                    "  Nein - Abbruch",
                                                    "Die Datei existiert bereits!", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No)
                    If mesres = MessageBoxResult.Yes Then
                        l(" MOD dropped 4 " & filenames(0).ToLower & " nach " & zielname)
                        IO.File.Copy(filenames(0).ToLower, zielname, True)
                        'der DB-eintrag existiert bereits also nichts weiter erforderlich
                        MsgBox("Datei wurde aktualisiert!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                    Else
                        Exit Sub
                    End If
                Else
                    l(" MOD dropped 4 " & filenames(0).ToLower & " nach " & zielname)
                    IO.File.Copy(filenames(0).ToLower, zielname, True)
                    MsgBox("Datei wurde aktualisiert!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                    'hier muss der db-eintrag gemacht werden
                    'insert
                    Dim erfolg As Boolean = toolsEigentuemer.insertBaulastPdfInDB(tbBaulastNr.Text & ".pdf", zielname, ObjektGuid)
                    MsgBox("DB für die Datei wurde gesetzt!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                End If


                'rawList(0).dateiExistiert = True
                'rawList(0).datei = zielname
                'l(" MOD dropped 5")
                ''pdfdatei erzeugen
                ''clsTIFFtools.zerlegeMultipageTIFF(zielname, tools.baulastenoutDir)
                ''refreshTIFFbox()
                'Dim erfolg As Boolean '= clsGIStools.updateGISDB(tbBaulastNr.Text, zielname, tools.FSTausPROBAUGListe(0).gemarkungstext.Trim, endung)
                'If erfolg Then
                '    Dim mesres As MessageBoxResult
                '    mesres = MessageBox.Show("Die tiff - Datei wurde erfolgreich ins GIS kopiert!" & Environment.NewLine &
                '                    "Ausserdem wurde die PDF-Datei erzeugt/erneuert." & Environment.NewLine &
                '                    "" & Environment.NewLine &
                '                    "Soll die Quelldatei gelöscht werden ? (J/N)" & Environment.NewLine &
                '                    " J - Löschen" & Environment.NewLine &
                '                    " N - bewahren " & Environment.NewLine,
                '                             "Quelldatei löschen?", MessageBoxButton.YesNo,
                '                                MessageBoxImage.Question, MessageBoxResult.Yes
                '                    )
                '    If mesres = MessageBoxResult.Yes Then
                '        If Not dateiLoeschen(filenames) Then
                '            MessageBox.Show("Datei liess sich nicht löschen. Haben Sie sie noch im Zugriff ? Abbruch!!")
                '        End If
                '    Else

                '    End If
                'Else
                '    MessageBox.Show("DB-Eintrag liess sich nicht erneuern. Bitte beim admin melden ? Abbruch!!")
                'End If


            End If

            l(" MOD dropped ende")
        Catch ex As Exception
            l("Fehler in dropped: " & zielname & Environment.NewLine &
              zielname.Trim.ToLower & "   " & ex.ToString())
            MessageBox.Show("Datei läßt sich nicht löschen. ")
        End Try
    End Sub
    'Private Sub droptiff(e As DragEventArgs)
    '    Dim filenames As String()
    '    Dim zielname As String = ""
    '    Dim endung As String = ".tiff"
    '    Dim listeZippedFiles, listeNOnZipFiles, allFeiles As New List(Of String)
    '    Dim titelVorschlag As String = ""
    '    Try
    '        l(" MOD dropped anfang")
    '        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
    '            filenames = CType(e.Data.GetData(DataFormats.FileDrop), String())
    '        End If
    '        If filenames(0).ToLower.EndsWith(".tiff") Then
    '            endung = ".tiff"
    '        End If
    '        If filenames(0).ToLower.EndsWith(".tif") Then
    '            endung = ".tif"
    '        End If
    '        l(" MOD dropped 2")
    '        If filenames(0).ToLower.EndsWith(".tiff") Or filenames(0).ToLower.EndsWith(".tif") Then
    '            l(" MOD dropped 3")
    '            zielname = IO.Path.Combine(srv_unc_path & "\fkat\baulasten", tools.FSTausPROBAUGListe(0).gemarkungstext.Trim & "\" & tbBaulastNr.Text.Trim & ".tiff").ToLower.Trim
    '            l(" MOD dropped 4 " & filenames(0).ToLower & " nach " & zielname)
    '            IO.File.Copy(filenames(0).ToLower, zielname, True)
    '            rawList(0).dateiExistiert = True
    '            rawList(0).datei = zielname
    '            l(" MOD dropped 5")
    '            'pdfdatei erzeugen
    '            clsTIFFtools.zerlegeMultipageTIFF(zielname, tools.baulastenoutDir)
    '            refreshTIFFbox()
    '            Dim erfolg As Boolean = clsGIStools.updateGISDB(tbBaulastNr.Text, zielname, tools.FSTausPROBAUGListe(0).gemarkungstext.Trim, endung)
    '            If erfolg Then
    '                Dim mesres As MessageBoxResult
    '                mesres = MessageBox.Show("Die tiff - Datei wurde erfolgreich ins GIS kopiert!" & Environment.NewLine &
    '                                "Ausserdem wurde die PDF-Datei erzeugt/erneuert." & Environment.NewLine &
    '                                "" & Environment.NewLine &
    '                                "Soll die Quelldatei gelöscht werden ? (J/N)" & Environment.NewLine &
    '                                " J - Löschen" & Environment.NewLine &
    '                                " N - bewahren " & Environment.NewLine,
    '                                         "Quelldatei löschen?", MessageBoxButton.YesNo,
    '                                            MessageBoxImage.Question, MessageBoxResult.Yes
    '                                )
    '                If mesres = MessageBoxResult.Yes Then
    '                    If Not dateiLoeschen(filenames) Then
    '                        MessageBox.Show("Datei liess sich nicht löschen. Haben Sie sie noch im Zugriff ? Abbruch!!")
    '                    End If
    '                Else

    '                End If
    '            Else
    '                MessageBox.Show("DB-Eintrag liess sich nicht erneuern. Bitte beim admin melden ? Abbruch!!")
    '            End If


    '        End If

    '        l(" MOD dropped ende")
    '    Catch ex As Exception
    '        l("Fehler in dropped: " & zielname & Environment.NewLine &
    '          zielname.Trim.ToLower & "   " & ex.ToString())
    '        MessageBox.Show("Datei läßt sich nicht löschen. ")
    '    End Try
    'End Sub

    Private Shared Function dateiLoeschen(filenames() As String) As Boolean
        Dim fi As IO.FileInfo
        Try
            l(" MOD dateiLoeschen anfang")
            fi = New IO.FileInfo(filenames(0).ToLower)
            If fi.Exists Then
                fi.Delete()
            Else

            End If
            fi = Nothing

            l(" MOD dateiLoeschen ende")
            Return True
        Catch ex As Exception
            l("Fehler in dateiLoeschen: " & ex.ToString())
            Return False
        End Try
    End Function

    Private Sub dropTheBomb(sender As Object, e As DragEventArgs)
        e.Handled = True
        dropPDF(e)
    End Sub

    Private Sub btndeleteTIFF_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim mesres As MessageBoxResult
        mesres = MessageBox.Show("Soll das Objekt wirklich gelöscht werden ? ", "Objekt löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
        If mesres = MessageBoxResult.No Then
            Exit Sub
        End If
        If clsGIStools.loescheTiffaufGISServer(tbBaulastNr.Text.Trim, tools.FSTausPROBAUGListe(0).gemarkungstext.Trim) Then
            'imgTiff.Source = Nothing
            MessageBox.Show("Gelöscht")
        Else
            MessageBox.Show("Fehler beim Löschen.")

        End If
        refreshTIFFbox()
    End Sub

    Private Sub btnPDFaufrufen_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        '\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_100005.pdf
        'Dim quelle = "\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_" & tbBaulastNr.Text & ".pdf"
        Dim ziel = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        ziel = IO.Path.Combine(ziel, tbBaulastNr.Text & ".pdf")
        '"\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_" & tbBaulastNr.Text & ".pdf"
        'IO.File.Copy(quelle, ziel, True)
        Process.Start(ziel)
    End Sub

    Private Sub showpdf()

        Dim quelle = srv_unc_path & "BAUL4ST_" & tbBaulastNr.Text & ".pdf"
        Dim ziel = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Try
            ziel = IO.Path.Combine(ziel, tbBaulastNr.Text & ".pdf")
            '"\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_" & tbBaulastNr.Text & ".pdf"
            IO.File.Copy(quelle, ziel, True)
            btnPDFaufrufen.IsEnabled = True
            tbPDFvorhanden.Text = "PDF ist verfügbar"
        Catch ex As Exception
            l("Fehler in showpdf " & ex.ToString)
            btnPDFaufrufen.IsEnabled = False
            tbPDFvorhanden.Text = "PDF fehlt"
        End Try

    End Sub

    Private Sub StackPanel_Drop(sender As Object, e As DragEventArgs)
        e.Handled = True
        Dim abbruch As Boolean = False
        'soll nur die nummer übernehmen
        Dim filenames As String()
        Dim zuielname As String = ""
        Dim listeZippedFiles, listeNOnZipFiles, allFeiles As New List(Of String)
        Dim titelVorschlag As String = ""
        Try
            l(" MOD StackPanel_Drop anfang")

            l(" MOD StackPanel_Drop anfang")
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                filenames = CType(e.Data.GetData(DataFormats.FileDrop), String())
            End If
            l(" MOD dropped 2")
            If filenames(0).ToLower.EndsWith(".tiff") Then
                Dim fi As New IO.FileInfo(filenames(0).ToLower.Trim)
                Dim a() As String
                a = fi.Name.Split("."c)
                tbBaulastNr.Text = a(0)

                fi = Nothing
            End If
            refreshall(abbruch)
            l(" MOD StackPanel_Drop ende")
        Catch ex As Exception
            l("Fehler in StackPanel_Drop: " & ex.ToString())
        End Try
    End Sub

    Private Sub btnplus_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim xdifalt As Double = range.xdif / 2
        Dim xdifnew As Double
        range.CalcCenter()
        xdifnew = CInt(xdifalt - (xdifalt / 4))
        range.xl = range.xcenter - xdifnew
        range.xh = range.xcenter + xdifnew
        range.yl = range.ycenter - xdifnew
        range.yh = range.ycenter + xdifnew
        refreshMap()
    End Sub

    Private Sub btnminus_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim xdifalt As Double = range.xdif / 2
        Dim xdifnew As Double
        range.CalcCenter()
        xdifnew = CInt(xdifalt * 1.5)
        range.xl = range.xcenter - xdifnew
        range.xh = range.xcenter + xdifnew
        range.yl = range.ycenter - xdifnew
        range.yh = range.ycenter + xdifnew
        refreshMap()
    End Sub

    'Private Sub btndigit_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    'genese = 2 '2-selbst digitalisiert, 1 = aus dem kataster
    '    setzeQuellUndTargetTabelle()
    '    If My.Computer.Clipboard.ContainsText Then
    '        tools.wkt = My.Computer.Clipboard.GetText()
    '        If tools.wkt.Trim.ToLower.StartsWith("polygon") Then
    '            btndigit.ToolTip = "Klick = Übernehmen dieser Geometrie als temporäres Flurstück !" & tools.wkt
    '        Else
    '            btndigit.ToolTip = "Das ist keine gültige Geometrie: " & tools.wkt
    '        End If

    '        Dim msgres As New MessageBoxResult
    '        msgres = MessageBox.Show("Ihr Polygon: " & vbNewLine & vbNewLine &
    '                                tools.wkt & vbNewLine & vbNewLine &
    '                                    "Möchten Sie diese Geometrie übernehmen? (j/n) ", "Geometrie übernehmen", MessageBoxButton.YesNo, MessageBoxImage.Question)
    '        If msgres = MessageBoxResult.Yes Then
    '            For Each item As clsBaulast In rawList
    '                item.serial = tools.wkt
    '            Next
    '            'writeallWithSerials(CBool(cbAuchUnguetige.IsChecked), 2, targetGISTabelle) '1=aus katasterdaten übernommen
    '            writeallWithSerials(False, 2, targetGISTabelle) '1=aus katasterdaten übernommen
    '        End If
    '    End If


    'End Sub

    Private Sub chkQuelle_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        setzeQuellUndTargetTabelle()
    End Sub

    Private Sub setzeQuellUndTargetTabelle()
        Dim grayBrush As SolidColorBrush = New SolidColorBrush(Colors.LightGray)
        Dim blueBrush As SolidColorBrush = New SolidColorBrush(Colors.AliceBlue)
        Dim abbruch As Boolean = False
        Try
            If chkQuelle.IsChecked Then
                quelleSQL = "   gisview2belastet "
                targetGISTabelle = "baulaschten_f"
                tbQuelle.Text = " Belastet aus Probaug"
                refreshProbaug(CInt(tbBaulastNr.Text), quelleSQL, abbruch)
                spTop.Background = grayBrush
            Else
                quelleSQL = "   gisview2 "
                tbQuelle.Text = " Begünstigt aus Probaug"
                spTop.Background = blueBrush
                targetGISTabelle = "baul_guen_f"
                refreshProbaug(CInt(tbBaulastNr.Text), quelleSQL, abbruch)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnZumGISPROBAUG_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim url As String
        ' C:\kreisoffenbach\mgis\ingradaadapter.exe    suchmodus=flurstueck gemarkung="Hainhausen" flur="4" fstueck="387/1" 
        '91197
        Try

            If tools.FSTausPROBAUGListe.Count > 0 Then
                flurstueckskennzeichen = tools.FSTausPROBAUGListe(0).flurstueckZuFKZ
                'url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                'url = "https://gis.kreis-of.de/LKOF/extensions/logout.asp?removeLostSession=true"
                'Process.Start(url)
                url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                l("url " & url)
                Process.Start(url)
                l(flurstueckskennzeichen)
            Else
                MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
                url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
                l("url " & url)
                Process.Start(url)
            End If
            'Dim gidstring As String = clsGIStools.bildegidstring()
            'range = clsGIStools.calcNewRange(gidstring) 
            'Dim param, rangestring As String 
            'Dim lu, ro As New myPoint
            'lu.X = range.xl
            'lu.Y = range.yl
            'ro.X = range.xh
            'ro.Y = range.yh
            'rangestring = clsGIStools.calcrangestring(lu, ro)
            'param = "modus=""bebauungsplankataster""  range=""" & rangestring & ""
            'Process.Start(tools.gisexe, param) 
        Catch ex As Exception
            l(ex.ToString)
            'Return "fehler in btnZumGIS_Click"
        End Try
    End Sub

    Private Sub btnZumGISOBJ_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim url As String
        '  https://gis.kreis-of.de/LKOF/asp/main.asp?lay=sp_mdat_0010_F&fld=text3&typ=string&val=1001&skipwelcome=true
        '91197
        '11368 hat keine gültigen flurstuecke	
        Try

            If tools.FSTausPROBAUGListe.Count > 0 Then
                flurstueckskennzeichen = tools.FSTausPROBAUGListe(0).flurstueckZuFKZ
                'url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                'url = "https://gis.kreis-of.de/LKOF/extensions/logout.asp?removeLostSession=true"
                'Process.Start(url)
                url = makeurl4Baulast("https://gis.kreis-of.de/LKOF/asp/main.asp?", tbBaulastNr.Text)
                l("url " & url)
                Process.Start(url)
                l(flurstueckskennzeichen)
            Else
                MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
                url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
                l("url " & url)
                Process.Start(url)
            End If
            'Dim gidstring As String = clsGIStools.bildegidstring()
            'range = clsGIStools.calcNewRange(gidstring) 
            'Dim param, rangestring As String 
            'Dim lu, ro As New myPoint
            'lu.X = range.xl
            'lu.Y = range.yl
            'ro.X = range.xh
            'ro.Y = range.yh
            'rangestring = clsGIStools.calcrangestring(lu, ro)
            'param = "modus=""bebauungsplankataster""  range=""" & rangestring & ""
            'Process.Start(tools.gisexe, param) 
        Catch ex As Exception
            l(ex.ToString)
            'Return "fehler in btnZumGIS_Click"
        End Try
    End Sub

    Private Sub btnEigentümerclip_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        My.Computer.Clipboard.SetText(tbEigentuemer.Text)
        MsgBox("Inhalt wurde in die Zwischenablage kopiert. Mit Strg-v können sie die Daten z.B. in Word einfügen.")
    End Sub

    Private Sub btnMakeworddok_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        filepath = IO.Path.Combine(filepath, "Eigentümer_BL_" & tbBaulastNr.Text & ".docx")

        Dim wp As New eigentuemerWord
        Dim erfolg = wp.machma(tbEigentuemer.Text, filepath, Format(Now, "dd.MM.yyyy"))
        If erfolg Then
            MsgBox("Die Worddatei wurde unter " & Environment.NewLine & filepath & Environment.NewLine & " abgelegt!")
        Else
            MsgBox("Die Worddatei konnte nicht erzeugt werden. Vermutlich haben sie sie noch geöffnet.")
        End If
        Process.Start(filepath)
        End
    End Sub
End Class
