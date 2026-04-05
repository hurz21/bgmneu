Imports System.Security.Policy
Imports System.Text
Imports DocumentFormat.OpenXml.Office.MetaAttributes


Public Class winDetail
    Property VGmyBitmapImage As New BitmapImage
    Private istgeladen As Boolean = False
    Public Property ObjektGuid As String = "88AFE39F-78FC-4053-BE6D-315E3745CF45" '=kategorie

    Public Property quelleSQL As String = "gisview2belastet"
    Public Property targetGISTabelle As String = "hartmann"
    Dim modus As String = "neu"
    Dim nurlesen As Boolean = True
    'Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
    '    Await webView.EnsureCoreWebView2Async()
    '    webView.Source = New Uri("https://www.google.de")
    'End Sub
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
                'flurstueckskennzeichen = tools.FSTausPROBAUGListe(0).flurstueckZuFKZ
                'starteGISueberFLST(srv_host_web, flurstueckskennzeichen)
                Close()
                Return
                'End
            End If
            refreshGIS(CInt(tbBaulastNr.Text))
            Dim summe = ""
            summe = makeFlurstuecksAbstrakt(tools.FSTausGISListe)
            summe = summe & Environment.NewLine
            Dim result As String
            If toolsEigentuemer.geteigentuemerText(tools.FSTausGISListe, result) Then
                tbEigentuemer.Text = summe & Environment.NewLine & result
                'If tbEigentuemer.Text.Contains("keine eigen") Then

                'End If
            Else
                summe = makeFlurstuecksAbstrakt(FSTausPROBAUGListe)
                summe = summe & Environment.NewLine
                tbEigentuemer.Text = summe & Environment.NewLine & result ' toolsEigentuemer.geteigentuemerText(tools.FSTausPROBAUGListe)
                'MsgBox(result)
            End If

        End If
        Dim nummer = tbBaulastNr.Text
        Dim text = tools.FSTausGISListe(0).gemarkungstext & ",Flur: " & tools.FSTausGISListe(0).flur
        WriteCookie(nummer, text)

        setTitle()
        showPDF()
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
        Dim greenBrush As SolidColorBrush = New SolidColorBrush(Colors.LightGreen)
        Dim schonvorhanden As Boolean = False

        schonvorhanden = clsGIStools.getBaulastFromBaulastMDAT(BaulastBlattNr, kategorie_guid_Baulasten) 'füllt fstREC

        If schonvorhanden Then
            tools.FSTausGISListe = clsGIStools.fstGISdt2ObjListe()
            If tools.FSTausGISListe.Count < 1 Then
                tbGISinfo.Text = "Mit dem Knopf 'Übertrag' können Sie die Flurstücksinfos zum GIS"
                tbGISinfo2.Text = " in die Baulast-DB übertragen!"
                Exit Sub
            End If
            'ObjektGuid = tools.FSTausGISListe(0).GUID
            If tools.FSTausGISListe Is Nothing Then
                MsgBox("Die im GIS-Baulastkataster hinterlegten Flurstücksinfos sind mangelhaft. Bitte verbessern!")

            End If
            'Dim url = ""
            'Dim themen As String
            'themen = tools.getthemen("")
            'url = "https://gis.kreis-of.de/LKOF/asp/main.asp?" & themen & "&lay=sp_mdat_0010_F&fld=text3&typ=string&val=" & tbBaulastNr.Text.Trim & "&skipwelcome=true"
            'webView.Source = New Uri(url)

            btnUebertragMetadaten.IsEnabled = True
            spBL.Background = greenBrush
            stpPDF.Visibility = Visibility.Visible
            dgAusGIS.DataContext = tools.FSTausGISListe
            tbGISinfo.Text = ""
            tbGISinfo2.Text = ""
            btnZumGIS.IsEnabled = True
            btnZumGISOBJ.IsEnabled = True
            btnZumGISPROBAUG.Content = "im GIS anzeigen"
            btnZumGISPROBAUG.Width = 100
            btnZumGISPROBAUG.Height = 20
            'tools.FSTausGISListe(0).Flurstuecksskennzeichen = tools.FSTausGISListe(0).flurstueckZuFKZ
            Dim fkz As String = bildeflurstuecksstring(tools.FSTausGISListe)
            'starteGISueberFLST(srv_host_web, fkz)
        Else
            'btnUebertragMetadaten.IsEnabled = False
            tbGISinfo.Text = "Baulast ist in der Baulast-DB(MDAT) von Ingrada noch nicht vorhanden !"
            tbGISinfo2.Text = "Das GIS startet nun automatisch!"
            stpPDF.Visibility = Visibility.Collapsed
            btnZumGIS.IsEnabled = False
            btnZumGISOBJ.IsEnabled = False
            btnZumGISPROBAUG.Content = "im GIS erfassen"
            btnZumGISPROBAUG.Width = 400
            btnZumGISPROBAUG.Height = 30

            'tools.FSTausGISListe(0).Flurstuecksskennzeichen = tools.FSTausPROBAUGListe(0).flurstueckZuFKZ
            Dim fkz As String = bildeflurstuecksstring(tools.FSTausGISListe)
            'starteGISueberFLST(srv_host_web, fkz)
        End If

        l("getSerialFromBasis---------------------- ende")
    End Sub



    Private Sub btnAusProbaug_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim abbruch As Boolean = False
        tools.writeBLBlattCookie(tbBaulastNr.Text, "bgm_blattnr_cookie.txt")
        refreshGIS(CInt(tbBaulastNr.Text))
        refreshEigentuemer(CInt(tbBaulastNr.Text))
        'gidInString = clsGIStools.bildegidstring()
        'range = clsGIStools.calcNewRange(gidInString)
        refreshall(abbruch)
        If abbruch Then
            Me.Close()
        End If
    End Sub

    Private Sub refreshEigentuemer(v As Integer)
        'Throw New NotImplementedException()
        tbEigentuemer.Text = ""
    End Sub

    Private Sub refreshall(ByRef abbruch As Boolean)

        refreshProbaug(CInt(tbBaulastNr.Text), quelleSQL, abbruch)
        If abbruch Then Exit Sub
        refreshGIS(CInt(tbBaulastNr.Text))
        'refreshTIFFbox()
        'refreshMap()
        showPDF()
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




    Sub refreshProbaug(baulastblattnr As Integer, sqlquelle As String, ByRef abbruch As Boolean)

        Try
            l(" MOD refreshProbaug anfang")
            dgAusProbaug.DataContext = Nothing
            tools.FSTausPROBAUGListe.Clear()

            clsProBGTools.holeProBaugDaten(baulastblattnr, sqlquelle, abbruch) ' füllt FSTausPROBAUGListe
            'abbruch = False
            If abbruch Then
                'MsgBox("Anwendung wird beendet !")
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
        Dim lokfkzliste As String = ""
        Dim treffer As Integer = 0
        Dim fkztemp As String
        Try
            'hier besser eine schleife über alle flurstücke
            For i = 0 To tools.FSTausGISListe.Count - 1
                If tools.flurstueckExistiertImGis(tools.FSTausGISListe(i).flurstueckZuFKZ) Then
                    treffer += 1
                    fkztemp = tools.FSTausGISListe(i).flurstueckZuFKZ
                    ' tools.FSTausGISListe(i).Flurstuecksskennzeichen = tools.FSTausGISListe(i).flurstueckZuFKZ
                    'url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
                    'url = "https://gis.kreis-of.de/
                    'LKOF/extensions/logout.asp?removeLostSession=true"
                    'Process.Start(url)
                    If treffer = 1 Then
                        lokfkzliste = fkztemp
                    Else
                        lokfkzliste = lokfkzliste & "," & fkztemp
                    End If
                    Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
                    If gisLogouten Then
                        Process.Start(logout)
                        Threading.Thread.Sleep(1000)
                    End If

                    url = tools.makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", lokfkzliste)
                    l("url " & url)
                    Process.Start(url)
                    l(tools.FSTausGISListe(0).Flurstuecksskennzeichen)
                Else
                    'MsgBox("Flurstück existiert so nicht im GIS !  " & Environment.NewLine &
                    '       tools.FSTausGISListe(0).flurstueckZuFKZ)
                    'Exit Sub
                End If

            Next

            If tools.FSTausGISListe.Count < 1 Then
                MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
                url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
                l("url " & url)
                Process.Start(url)
            End If
        Catch ex As Exception
            l(ex.ToString)
        End Try
    End Sub



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
            l(" MOD dropped 2" & tools.FSTausGISListe.Count)
            If filenames(0).ToLower.EndsWith(endung) Then
                l(" MOD dropped 3")
                zielname = IO.Path.Combine(srv_unc_path & "BAUL4ST_" & tbBaulastNr.Text.Trim & endung).Trim

                Dim fi As New IO.FileInfo(zielname)
                If fi.Exists Then
                    'Dim mesres = MessageBox.Show("Möchten Sie die Datei überschreiben ?" & Environment.NewLine &
                    '                                "  Ja    - Überschreiben " & Environment.NewLine &
                    '                                "  Nein - Abbruch",
                    '                                "Die Datei existiert bereits!", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No)
                    'If mesres = MessageBoxResult.Yes Then
                    l(" MOD dropped 4 " & filenames(0).ToLower & " nach " & zielname)
                    IO.File.Copy(filenames(0).ToLower, zielname, True)
                    'der DB-eintrag existiert bereits also nichts weiter erforderlich
                    If toolsEigentuemer.existiertPDFinMDAT_FILES(tbBaulastNr.Text.Trim) Then
                        '
                    Else
                        Dim erfolg As Boolean
                        For i = 0 To tools.FSTausGISListe.Count - 1

                            erfolg = toolsEigentuemer.insertBaulastPdfInMDAT_Dateien(tbBaulastNr.Text & ".pdf", tools.FSTausGISListe(i).GUID)

                            If erfolg Then
                                l("DB für die Datei wurde gesetzt!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim & " " & tools.FSTausGISListe(i).GUID)
                            Else
                                l("DB für die Datei wurde NICHT gesetzt! Fehler (\dokumente\bgm)" & " " & tools.FSTausGISListe(i).GUID)
                            End If
                        Next
                    End If
                    MsgBox("Datei wurde aktualisiert!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                    'Else
                    '    Exit Sub
                    'End If
                Else
                    l(" MOD dropped 4 " & filenames(0).ToLower & " nach " & zielname)
                    IO.File.Copy(filenames(0).ToLower, zielname, True)
                    MsgBox("Datei wurde aktualisiert!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                    'hier muss der db-eintrag gemacht werden                    'insert
                    Dim erfolg As Boolean
                    For i = 0 To tools.FSTausGISListe.Count - 1

                        erfolg = toolsEigentuemer.insertBaulastPdfInMDAT_Dateien(tbBaulastNr.Text & ".pdf", tools.FSTausGISListe(i).GUID)

                        If erfolg Then
                            l("DB für die Datei wurde gesetzt!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim & " " & tools.FSTausGISListe(i).GUID)
                        Else
                            l("DB für die Datei wurde NICHT gesetzt! Fehler (\dokumente\bgm)" & " " & tools.FSTausGISListe(i).GUID)
                        End If
                    Next
                    'MsgBox("DB für die Datei wurde gesetzt!" & Environment.NewLine & tbBaulastNr.Text.Trim & endung.Trim)
                End If
            End If


            l(" MOD dropped ende")
        Catch ex As Exception
            l("Fehler in dropped: " & zielname & Environment.NewLine & zielname.Trim.ToLower & "   " & ex.ToString())
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

    'Private Sub btndeleteTIFF_Click(sender As Object, e As RoutedEventArgs)
    '    e.Handled = True
    '    Dim mesres As MessageBoxResult
    '    mesres = MessageBox.Show("Soll das Objekt wirklich gelöscht werden ? ", "Objekt löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
    '    If mesres = MessageBoxResult.No Then
    '        Exit Sub
    '    End If
    '    If clsGIStools.loescheTiffaufGISServer(tbBaulastNr.Text.Trim, tools.FSTausPROBAUGListe(0).gemarkungstext.Trim) Then
    '        'imgTiff.Source = Nothing
    '        MessageBox.Show("Gelöscht")
    '    Else
    '        MessageBox.Show("Fehler beim Löschen.")

    '    End If
    '    refreshTIFFbox()
    'End Sub

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

    Sub showPDF()
        Dim hinweis As String
        Dim quelle = srv_unc_path & "BAUL4ST_" & tbBaulastNr.Text & ".pdf"
        Dim ziel = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        ziel = IO.Path.Combine(ziel, tbBaulastNr.Text & ".pdf")
        Try

            If toolsEigentuemer.existiertPDFinMDAT_FILES(tbBaulastNr.Text.Trim) Then

                '"\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_" & tbBaulastNr.Text & ".pdf"
                IO.File.Copy(quelle, ziel, True)
                btnPDFaufrufen.IsEnabled = True
                tbPDFvorhanden.Text = "PDF ist verfügbar"
            Else
                btnPDFaufrufen.IsEnabled = False
                tbPDFvorhanden.Text = "PDF fehlt"
            End If
            Return

        Catch ex As Exception
            l("Fehler in showpdf " & ex.ToString)
            btnPDFaufrufen.IsEnabled = False
            tbPDFvorhanden.Text = "PDF fehlt"
        End Try
    End Sub



    'Private Sub showpdf()

    '    Dim quelle = srv_unc_path & "BAUL4ST_" & tbBaulastNr.Text & ".pdf"
    '    Dim ziel = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    '    Try
    '        ziel = IO.Path.Combine(ziel, tbBaulastNr.Text & ".pdf")
    '        '"\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\BAUL4ST_" & tbBaulastNr.Text & ".pdf"
    '        IO.File.Copy(quelle, ziel, True)
    '        btnPDFaufrufen.IsEnabled = True
    '        tbPDFvorhanden.Text = "PDF ist verfügbar"
    '    Catch ex As Exception
    '        l("Fehler in showpdf " & ex.ToString)
    '        btnPDFaufrufen.IsEnabled = False
    '        tbPDFvorhanden.Text = "PDF fehlt"
    '    End Try

    'End Sub

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
        gisFuerProbaugFlurst(tbBaulastNr.Text.Trim, tools.FSTausPROBAUGListe)
    End Sub



    Private Sub btnZumGISOBJ_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        baulastAlsObjImGisZeigen(tbBaulastNr.Text.Trim)
        'Dim url As String
        ''  https://gis.kreis-of.de/LKOF/asp/main.asp?app=sp_mdat&lay=sp_mdat_0010_F&fld=text3&typ=string&val=1001&skipwelcome=true
        ''91197
        ''11368 hat keine gültigen flurstuecke	
        'Try
        '    url = makeurl4Baulast("https://gis.kreis-of.de/LKOF/asp/main.asp?", tbBaulastNr.Text)
        '    l("url " & url)
        '    Process.Start(url)
        '    'If tools.FSTausPROBAUGListe.Count > 0 Then
        '    '    flurstueckskennzeichen = tools.FSTausPROBAUGListe(0).flurstueckZuFKZ
        '    '    'url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", flurstueckskennzeichen)
        '    '    'url = "https://gis.kreis-of.de/LKOF/extensions/logout.asp?removeLostSession=true"
        '    '    'Process.Start(url)
        '    '    url = makeurl4Baulast("https://gis.kreis-of.de/LKOF/asp/main.asp?", tbBaulastNr.Text)
        '    '    l("url " & url)
        '    '    Process.Start(url)
        '    '    l(flurstueckskennzeichen)
        '    'Else
        '    '    MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
        '    '    url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
        '    '    l("url " & url)
        '    '    Process.Start(url)
        '    'End If
        'Catch ex As Exception
        '    l(ex.ToString)
        '    'Return "fehler in btnZumGIS_Click"
        'End Try
    End Sub

    Private Sub btnEigentümerclip_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If IsNothingOrEmpty(tbEigentuemer.Text) Then
            MsgBox("Feld ist noch leer")
        Else
            My.Computer.Clipboard.SetText(tbEigentuemer.Text)
            MsgBox("Inhalt wurde in die Zwischenablage kopiert. Mit Strg-v können sie die Daten z.B. in Word einfügen.")
        End If

    End Sub

    Private Sub btnMakeworddok_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim datei As String
        datei = tools.erzeugeWordDateiEigentuemer(tbEigentuemer.Text, tbBaulastNr.Text)
        Process.Start(datei)
        'End
    End Sub



    Private Sub btnProtokoll_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
        IO.Directory.CreateDirectory(IO.Path.Combine(testfolder, "bgm"))
        logfile = IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "bgm")
        Process.Start(logfile)
    End Sub

    Private Sub btnEigentuemerProbaug_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim summe As String
        Try
            summe = "Aus ProbauG:" & Environment.NewLine
            summe = summe & makeFlurstuecksAbstrakt(tools.FSTausPROBAUGListe)
            summe = summe & Environment.NewLine
            Dim result As String
            If toolsEigentuemer.geteigentuemerText(tools.FSTausPROBAUGListe, result) Then
                summe = summe & Environment.NewLine & result
                tbEigentuemer.Text = summe
            Else
                MsgBox(result)
            End If
        Catch ex As Exception
            l("btnEigentuemerProbaug_Click " & ex.ToString)
        End Try
    End Sub

    Private Sub btnEigentuemerGIS_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim summe As String = "Aus GIS:" & Environment.NewLine
        summe = summe & makeFlurstuecksAbstrakt(tools.FSTausGISListe)
        summe = summe & Environment.NewLine
        Dim result As String
        If toolsEigentuemer.geteigentuemerText(tools.FSTausGISListe, result) Then
            tbEigentuemer.Text = summe & Environment.NewLine & result
        Else
            tbEigentuemer.Text = summe & Environment.NewLine & result
        End If
    End Sub

    Private Sub btnUebertragMetadaten_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        uebertrageAlleMetadatenNachGIS()
    End Sub

    Private Sub uebertrageAlleMetadatenNachGIS()
        l("uebertrageAlleMetadatenNachGIS")
        Try
            For Each bl As clsBaulast In rawList
                If transferItem2gis(bl) Then
                    l("uebertrageAlleMetadatenNachGIS item ok ")
                Else
                    l("uebertrageAlleMetadatenNachGIS item fail ")
                End If
            Next
            refreshGIS(CInt(tbBaulastNr.Text))
        Catch ex As Exception
            l("uebertrageAlleMetadatenNachGIS " & ex.ToString)
        End Try
    End Sub

    Private Function transferItem2gis(bl As clsBaulast) As Boolean
        'update  " set tiff2='fkat/baulasten/' || trim(gemarkung) || '/' || trim(jahr_blattnr) || '.tiff'
        '        sql = "update " & tools.srv_schema & "." & tools.srv_tablename & " Set tiff='" & neuerTIFFname & "' where jahr_blattnr='" & baulastblatnr & "'"
        l("transferItem2gis ")
        Dim mmemo, tooltip, update As String
        Dim hinweis As String
        Dim newid As Long
        Dim startInsertfile = "USE [LKOF_Bearb] GO"
        Dim result As String = ""
        mmemo = bl.Kennziffer_1.Trim & ", " & bl.Kennziffer_2.Trim & ", " & bl.Kennziffer_3.Trim & ", " & bl.Kennziffer_4.Trim
        tooltip = "BLNr: " & bl.blattnr & ", " & bl.baulastnr & ": Jahr,Az " & bl.AzJahr & ", " & bl.AzNr
        'tooltip = ""

        update = " update  [LKOF_Bearb].[dbo].[tbl_mdat_datensatz]  " &
            "set text5='Jahr,Az: " & bl.AzJahr & ", " & bl.AzNr & "', " &
            " text7='" & bl.gemeindeText.Trim & "', text8='" & bl.probaugNotationFST.gemarkungstext.Trim & "', " &
            " int1=" & bl.probaugNotationFST.flur & ", int2=" & bl.probaugNotationFST.zaehler & ", " &
            " int3=" & bl.probaugNotationFST.nenner & ", int4=" & bl.probaugNotationFST.gemcode & ", " &
            " memo='" & mmemo & "', tooltip='" & tooltip & "' " &
            " where kategorie_guid='" & kategorie_guid_Baulasten & "' " &
            " and text3='" & bl.blattnr & "' " &
            " and text2='" & bl.baulastnr & "' "

        Try
            fstREC.mydb.SQL = update
            l(fstREC.mydb.SQL)
            Dim retcode = fstREC.dboeffnen(hinweis)
            newid = fstREC.sqlexecute(newid)
            retcode = fstREC.dbschliessen(hinweis)
            If newid > 0 Then
                Return True
            Else

                Return False
            End If
        Catch ex As Exception
            l("transferItem2gis " & ex.ToString)
            Return False
        End Try
    End Function

    Private Sub btnBaulastLoeschen_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Process.Start("\\kh-w-ingrada\GIS-Daten\diverses\bgmingrada\AnleitungBGM.pdf")

    End Sub


    Private Sub starteGISueberBLObjekt(baseurl As String)
        Dim url = ""
        Dim themen As String
        themen = tools.getthemen("").Trim

        Dim logout = baseurl & "/login.asp?logout=true&m=1"
        Process.Start(logout)
        Threading.Thread.Sleep(1000)
        url = baseurl & "?" & themen & "&lay=sp_mdat_0010_F&fld=text3&typ=string&val=" & tbBaulastNr.Text.Trim & "&skipwelcome=true"
        Process.Start(url)
    End Sub
    Private Sub starteGISueberFLST(baseurl As String, flurstueckkennzeichen As String)
        '"https://gis.kreis-of.de/LKOF/asp/login.asp
        Dim url = ""
        Dim themen As String
        themen = tools.getthemen("").Trim

        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        'Process.Start(logout)

        'https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1
        'baseurl = baseurl & "app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val="
        'baseurl = baseurl & flurstueckskennzeichen & "&skipwelcome=true"


        Threading.Thread.Sleep(1000)

        url = baseurl & "?" & themen & "&app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val=" & flurstueckkennzeichen & "&skipwelcome=true"
        Process.Start(url)
    End Sub

    Private Sub btnZumGIS_Click(sender As Object, e As Object)

    End Sub
End Class
