Imports System.Data


Module tools
    Public themendefinitionsdatei As String = "themendateiBaulasten.txt"
    Public gisLogouten As Boolean = True
    Public historyFile As String = "history.txt"
    Public aktbplan As New clsBplan
    Public maxItems As Integer = 20
    Public historyList As New List(Of HistoryItem)

    Public kategorie_guid_Baulasten As String = "88AFE39F-78FC-4053-BE6D-315E3745CF45"
    Public kategorie_guid_Bplaene As String = "F52CBA15-FAFF-4EDD-BBD3-B821920F1360"
    Public genese As Integer = 1
    Public range As New clsRange
    'Public flurstueckskennzeichen As String
    Public fkzlist As New List(Of clsFlurstueck)
    Public fkzlist_lage As New List(Of clsFlurstueck)
    Public lageliste As New List(Of myComboBoxItem)
    Public lage_lage As String = ""
    Public fst_lage As String = ""
    Dim url As String = ""

    'die alte config mit postgis
    'Public srv_host_web As String = "http://gis.kreis-of.local"
    'Public srv_host As String = "gis"
    'Public srv_schema As String = "paradigma_userdata"
    'Public srv_subdirBaulsten As String = "paradigmacache/baulasten"
    'Public srv_unc_path As String = "\\gis\gdvell"

    'neu ingrada
    'SELECT * FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='88AFE39F-78FC-4053-BE6D-315E3745CF45'    '
    Public srv_host_web As String = "https://gis.kreis-of.de/LKOF/asp/main.asp"
    Public srv_host As String = "KH-W-INGRADA"
    'Public srv_schema As String = "paradigma_userdata"
    Public srv_subdirBaulsten As String = "paradigmacache/baulasten"
    Public srv_unc_path As String = "\\kh-w-ingrada\lkof\data\upload\FILES\LKOF\sp_mdat\dat\"

    'Public srv_unc_path As String = "\\gis\d$"
    Public gisexe As String = "C:\kreisoffenbach\mgis\mgis.exe"
    'Public bplanexe As String = "C:\kreisoffenbach\bplankat\bplaninternet.exe"
    Public bgmVersion As String = My.Resources.BuildDate.Trim.Replace(vbCrLf, "")
    Public Property baulastenoutDir As String = "c:\baulastenout"
    Public Property wkt As String = ""
    Public Property FSTausGISListeFehlt As List(Of clsFlurstueck)
    Public Property eigentuemerAbfrageErlaubt As Boolean = False
    Public logfile As String = "C:\kreisoffenbach\bgm\" ' & Environment.UserName & "_"
    'Public logfile As String = srv_unc_path & "\apps\test\bgm\" & "logs\" ' & Environment.UserName & "_"
    Public pfad As String = srv_unc_path & "\fkat\baulasten\"

    'Private Const OracleConnectionString As String = "Data Source=  (DESCRIPTION =  " &
    '                                            "  (ADDRESS = (PROTOCOL = TCP)(HOST = ora-clu-scan.kreis-of.local)(PORT = 1521))  " &
    '                                            "  (LOAD_BALANCE = yes)  " &
    '                                            "  (CONNECT_DATA =    " &
    '                                            "  (SERVER = DEDICATED)  " &
    '                                            "    (SERVICE_NAME = bau.kreis-of.local) " &
    '                                            "   )  );User Id=bauguser;Password=test;"
    Public srv_tablename As String = "baulaschten_f"
    Public FSTausPROBAUGListe As New List(Of clsFlurstueck)
    Public FSTausGISListe As New List(Of clsFlurstueck)
    Public gidInString As String = ""
    Public baulastListe As New List(Of clsBaulast)

    Public probaugGemarkungsdict As New Dictionary(Of Integer, String)
    Public katasterGemarkungslist As New List(Of myComboBoxItem)
    Public katasterGemeindelist As New List(Of myComboBoxItem)
    Public gemeindedict As New Dictionary(Of Integer, String)
    Public gem(37) As String
    Public gemeinde(13) As String
    Public katasterGem(35) As String
    Public rawListOfclsBaulast As New List(Of clsBaulast)
    Public list4Geloscht As New List(Of clsBaulast)
    Public fstREC As New clsDBspecMSSQL
    Public anzahltiff, anzahl_dateiexitiert, anzahl_blattNrIst0, anzahlKatasterFormellOK, anzahlGeloschte, vierergeloescht, anzahl_mitSerial As Integer
    Public enc As System.Text.Encoding = System.Text.Encoding.GetEncoding(1252)

    ' 🔑 WriteHistoryCookie Funktion
    'Public Sub WriteHistoryCookie(value As String)
    '    If String.IsNullOrWhiteSpace(value) Then Exit Sub
    '    Dim locfile As String
    '    ' Falls schon vorhanden → entfernen
    '    historyList.Remove(value)

    '    ' Neu oben einfügen
    '    historyList.Insert(0, value)

    '    ' Max 20 Einträge
    '    If historyList.Count > maxItems Then
    '        historyList = historyList.Take(maxItems).ToList()
    '    End If

    '    Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
    '    Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
    '    locfile = IO.Path.Combine(test, historyFile)
    '    ' In Datei speichern
    '    IO.File.WriteAllLines(locfile, historyList)

    '    ' ComboBox aktualisieren
    '    LoadHistory()
    'End Sub
    Public Sub WriteCookie(nummer As String, text As String)
        If String.IsNullOrWhiteSpace(nummer) Then Exit Sub

        Dim anzeige = nummer & "-" & text

        ' vorhandenen Eintrag entfernen (nach Nummer!)
        historyList.RemoveAll(Function(x) x.Nummer = nummer)

        ' neu hinzufügen (oben)
        historyList.Insert(0, New HistoryItem With {
            .Nummer = nummer,
            .Anzeige = anzeige
        })

        ' max 20
        If historyList.Count > maxItems Then
            historyList = historyList.Take(maxItems).ToList()
        End If

        Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
        Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
        Dim locfile = IO.Path.Combine(test, historyFile)

        ' speichern
        Dim lines = historyList.Select(Function(x) x.Nummer & "|" & x.Anzeige)
        IO.File.WriteAllLines(locfile, lines)

        LoadHistory()
    End Sub
    Public Sub LoadHistory()
        Dim locfile As String
        Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
        Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
        locfile = IO.Path.Combine(test, historyFile)
        'If IO.File.Exists(locfile) Then
        '    historyList = IO.File.ReadAllLines(locfile).ToList()
        'End If

        historyList.Clear()

        If IO.File.Exists(locfile) Then
            For Each line In IO.File.ReadAllLines(locfile)
                Dim parts = line.Split("|"c)
                If parts.Length = 2 Then
                    historyList.Add(New HistoryItem With {
                        .Nummer = parts(0),
                        .Anzeige = parts(1)
                    })
                End If
            Next
        End If


    End Sub
    Sub setLogfile(logfile As String)
        With My.Log.DefaultFileLogWriter
            '#If DEBUG Then
            '.CustomLocation = mgisUserRoot & "logs\"
            logfile = "d:\" & "" ' & Environment.UserName & "_"
            Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            IO.Directory.CreateDirectory(IO.Path.Combine(testfolder,
                                 "bgm\logs"))
            IO.Directory.CreateDirectory(IO.Path.Combine(testfolder,
                                 "bgm\cookies"))
            IO.Directory.CreateDirectory(IO.Path.Combine(testfolder,
                                 "bgm\div"))
            IO.Directory.CreateDirectory(IO.Path.Combine(testfolder,
                                 "bgm\cache"))
            'testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)

            'IO.Directory.CreateDirectory(IO.Path.Combine(testfolder,
            '                     "bgm"))
            logfile = IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                                 "bgm\logs")
            '#Else
            '#End If
            '.CustomLocation = My.Computer.FileSystem.SpecialDirectories.Temp & "\mgis_logs\"
            .CustomLocation = logfile '
            '.BaseFileName = GisUser.username & "_" & Format(Now, "yyyyMMddhhmmss")
            .BaseFileName = Environment.UserName & "_bgm_" ' & Format(Now, "yyyyMMddhhmmss")
            .AutoFlush = True
            .Append = False
        End With
    End Sub

    Sub initdb()
        fstREC.mydb = New clsDatenbankZugriff
        fstREC.mydb.Host = tools.srv_host
        fstREC.mydb.username = "Ingrada" : fstREC.mydb.password = "Starry-Footless6-Mashing-Backboned"
        fstREC.mydb.Schema = "LKOF"
        l("initdb  ende")
    End Sub

    Sub istKatnichtOKaberTiffVorhanden(balist As List(Of clsBaulast), ByRef katnichtOKAberMitTiff_summe As String)
        katnichtOKAberMitTiff_summe = ""
        Dim iz As Integer = 0
        Dim summme As New Text.StringBuilder
        Try
            l("istKatnichtOKaberTiffVorhanden---------------------- anfang")
            For Each lok As clsBaulast In balist
                If Not lok.katasterFormellOK Then
                    summme.Append(" " & lok.gemeindeText & lok.baulastnr & " " & lok.bauortNr & " " & lok.blattnr & Environment.NewLine)
                    If Not lok.dateiExistiert Then
                    End If
                End If
            Next
            katnichtOKAberMitTiff_summe = summme.ToString
            l("istKatnichtOKaberTiffVorhanden---------------------- ende")
        Catch ex As Exception
            l("Fehler in istKatnichtOKaberTiffVorhanden: " & ex.ToString())
        End Try
    End Sub

    Friend Function bildeGeloeschteListe(rawList As List(Of clsBaulast), ByRef anzahlGeloschte As Integer) As List(Of clsBaulast)

        anzahlGeloschte = 0
        Dim newlist As New List(Of clsBaulast)
        'status
        '1 - eintrag
        '2 - änderung
        '3 - 
        '4 - verz gelöscht
        Try
            l("bildeGeloeschteListe---------------------- anfang")
            For Each lok As clsBaulast In rawList
                If lok.datumgeloescht.Trim <> String.Empty Then
                    lok.geloescht = True
                    newlist.Add(lok)
                    anzahlGeloschte += 1
                End If
            Next
            Return newlist
            l("bildeGeloeschteListe---------------------- ende")
        Catch ex As Exception
            l("Fehler inbildeGeloeschteListe : " & ex.ToString())
            Return Nothing
        End Try
    End Function

    Sub istKatasterFormellOK(balist As List(Of clsBaulast), ByRef anzahlKatasterFormellOK As Integer)
        anzahlKatasterFormellOK = 0
        Dim iz As Integer = 0

        Try
            l("istKatasterFormellOK---------------------- anfang")
            For Each lok As clsBaulast In balist
                If lok.katFST.gemcode < 1 Then
                    lok.katasterFormellOK = False
                    Continue For
                End If
                If lok.katFST.flur < 1 Then
                    lok.katasterFormellOK = False
                    Continue For
                End If
                If lok.katFST.zaehler < 1 Then
                    lok.katasterFormellOK = False
                    Continue For
                End If
                lok.katFST.FS = lok.katFST.buildFS()
#If DEBUG Then
                'If lok.katFST.FS = "FS0607570080000300300" Then
                '    Debug.Print("")
                'End If
                'If lok.kennzeichen1 = "4" Then
                '    Debug.Print("")
                'End If
#End If

                anzahlKatasterFormellOK += 1
                lok.katasterFormellOK = True
            Next
            l("istKatasterFormellOK---------------------- ende")
        Catch ex As Exception
            l("Fehler in istKatasterFormellOK: " & ex.ToString())
        End Try
    End Sub

    Function splitKatasterGemarkung() As List(Of myComboBoxItem)
        Dim dict As New List(Of myComboBoxItem)
        Dim a() As String
        Dim my As New myComboBoxItem
        For i = 0 To katasterGem.Count - 1
            my = New myComboBoxItem
            a = katasterGem(i).Replace(vbTab, " ").Split(";"c)
            my.myindex = a(1).Trim
            my.mySttring = (a(0).Trim)
            dict.Add(my)
        Next
        Return dict
    End Function
    Function splitgemeinde() As Dictionary(Of Integer, String)
        Dim dict As New Dictionary(Of Integer, String)
        Dim a() As String
        For i = 0 To gemeinde.Count - 1
            a = gemeinde(i).Trim.Replace(vbTab, "").Split(";"c)
            dict.Add(CInt(a(0).Trim), a(1).Trim)
        Next
        Return dict
    End Function
    'Function getbalist2Oracle(sql As String) As DataTable
    '    Dim oOracleConn As OracleConnection
    '    Dim dt As System.Data.DataTable
    '    Dim com As OracleCommand
    '    Dim _mycount As Long
    '    dt = New DataTable
    '    Try
    '        l(" MOD getbalist2 anfang")
    '        oOracleConn = New OracleConnection(OracleConnectionString)
    '        oOracleConn.Open()
    '        nachricht("OracleConnection open")
    '        com = New OracleCommand(sql, oOracleConn) '"select * from " & tabname$
    '        Dim da As New OracleDataAdapter(com)
    '        da.MissingSchemaAction = MissingSchemaAction.AddWithKey
    '        nachricht("fill")
    '        Console.WriteLine("vor fill")
    '        _mycount = da.Fill(dt)
    '        nachricht("fillfertig: " & _mycount)
    '        nachricht("in gisview2 wurden " & _mycount & " datensätze gefunden 
    '        oOracleConn.Close()
    '        com.Dispose()
    '        da.Dispose()
    '        Return dt
    '        l(" MOD getbalist2 ende")
    '    Catch ex As Exception
    '        l("Fehler in getbalist2: " & ex.ToString())
    '        Return dt
    '    End Try
    'End Function

    Function mylog(ttt As String) As Boolean
        Console.WriteLine(ttt)
    End Function

    Function splitgem() As Dictionary(Of Integer, String)
        Dim dict As New Dictionary(Of Integer, String)
        Dim a() As String
        For i = 0 To gem.Count - 1
            a = gem(i).Replace(vbTab, " ").Split(" "c)
            dict.Add(CInt(a(0).Trim), a(1).Trim)
        Next
        Return dict
    End Function

    Function dtnachobjALT(balistDT As DataTable, geschlossen As DataTable) As List(Of clsBaulast)
        Dim nlist As New List(Of clsBaulast)
        Dim lok As New clsBaulast
        Dim evtlFlur As String
        Dim b As String
        Dim iz As Integer = 0
        Try
            l("dtnachobj ---------------------- anfang")
#If DEBUG Then
            'For i = 0 To 100
            For i = 0 To balistDT.Rows.Count - 1
#Else
            For i = 0 To balistDT.Rows.Count - 1
#End If
                lok = New clsBaulast
                lok.blattnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a1")).Trim '21478
                lok.baulastnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a2")).Trim '1
#If DEBUG Then
                If lok.blattnr = "90764" Then
                    Debug.Print("")
                End If
#End If
                lok.bauortNr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a4")).Trim '2
                lok.probaugNotationFST.gemcode = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("a5")).Trim) '5
                evtlFlur = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a6")).Trim '10
                Console.WriteLine("iz1 " & iz)

                If evtlFlur.IsNothingOrEmpty Then
                    lok.probaugNotationFST.flur = 0
                Else
                    If IsNumeric(evtlFlur) Then
                        lok.probaugNotationFST.flur = CInt(evtlFlur)
                    Else
                        lok.probaugNotationFST.flur = 0
                    End If
                End If
                lok.probaugNotationFST.fstueckKombi = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a7")).Trim '406/1
                lok.gueltig = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a8")).Trim 'J
                lok.datum = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("a10"))).Trim 'leer
                lok.status = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a3")).Trim '1
                lok.laufnr = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("a9"))) '17655
                lok.datum1 = clsDBtools.fieldvalue(balistDT.Rows(i).Item("angelegt")).Trim '"2020.07.10"
                lok.datumgeloescht = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                lok.probaugNotationFST.zeigtauf = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                'b = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a13")).Trim
                If istgeschlossen(lok.blattnr, geschlossen) Then Continue For
                iz += 1
                nlist.Add(lok)
            Next
            Return nlist
            l("dtnachobj ---------------------- ende")
        Catch ex As Exception
            l("Fehler in dtnachobj: " & ex.ToString())
            Return Nothing
        End Try
    End Function
    Function dtnachobj(balistDT As DataTable, geschlossen As DataTable) As List(Of clsBaulast)
        Dim rawlist As New List(Of clsBaulast)
        Dim lok As New clsBaulast
        Dim evtlFlur As String
        Dim b As String
        Dim iz As Integer = 0
        Try
            l("dtnachobj ---------------------- anfang")

            'For i = 0 To 100
            For i = 0 To balistDT.Rows.Count - 1

                lok = New clsBaulast
                lok.blattnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD1")).Trim ' 
                lok.baulastnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD2")).Trim '1  sonst als gebucht
#If DEBUG Then
                If lok.blattnr = "90764" Then
                    Debug.Print("")
                End If
#End If
                lok.bauortNr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD4")).Trim '2
                lok.probaugNotationFST.gemcode = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD5")).Trim) '5
                evtlFlur = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD6")).Trim '10
                Console.WriteLine("iz1 " & iz)

                If evtlFlur.IsNothingOrEmpty Then
                    lok.probaugNotationFST.flur = 0
                Else
                    If IsNumeric(evtlFlur) Then
                        lok.probaugNotationFST.flur = CInt(evtlFlur)
                    Else
                        lok.probaugNotationFST.flur = 0
                    End If
                End If
                lok.probaugNotationFST.fstueckKombi = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD7")).Trim '406/1
                lok.gueltig = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD8")).Trim 'J

                lok.status = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD3")).Trim '1
                'lok.laufnr = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD9"))) '17655
                lok.laufnr = 0 'CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD2"))) 'keine ahnung

                lok.datum = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD10"))).Trim 'leer
                lok.AzJahr = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD10"))).Trim 'leer
                lok.AzOG = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD11"))).Trim 'leer blödsinn
                lok.AzNr = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD12"))).Trim 'leer
                lok.Rechtswert = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD13"))).Trim 'leer
                lok.Hochwert = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD14"))).Trim 'leer
                lok.Prefix = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD15"))).Trim 'leer immmer
                lok.Kennziffer_1 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD16"))).Trim 'leer
                lok.Kennziffer_2 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD17"))).Trim 'leer
                lok.Kennziffer_3 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD18"))).Trim 'leer
                lok.Kennziffer_4 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD19"))).Trim 'leer

                lok.datumgeloescht = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("datedeleted"))).Trim 'leer
                lok.datum = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("dateadded"))).Trim 'leer
                '   lok.datum1 = clsDBtools.fieldvalue(balistDT.Rows(i).Item("angelegt")).Trim '"2020.07.10"
                '  lok.datumgeloescht = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                ' lok.probaugNotationFST.zeigtauf = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                'b = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a13")).Trim
                If istgeschlossen(lok.blattnr, geschlossen) Then Continue For
                iz += 1
                rawlist.Add(lok)
            Next
            Return rawlist
            l("dtnachobj ---------------------- ende")
        Catch ex As Exception
            l("Fehler in dtnachobj: " & ex.ToString())
            Return Nothing
        End Try
    End Function
    Function dtnachobj2(balistDT As DataTable, geschlossen As DataTable) As List(Of clsBaulast)
        Dim rawlist As New List(Of clsBaulast)
        Dim lok As New clsBaulast
        Dim evtlFlur As String
        Dim b As String
        Dim iz As Integer = 0
        Try
            l("dtnachobj ---------------------- anfang")

            'For i = 0 To 100
            For i = 0 To balistDT.Rows.Count - 1
                Try
                    lok = New clsBaulast
                    lok.blattnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD1")).Trim ' 
                    lok.baulastnr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD2")).Trim '1  sonst als gebucht
#If DEBUG Then
                    If lok.blattnr = "90764" Then
                        Debug.Print("")
                    End If
#End If
                    lok.bauortNr = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD4")).Trim '2
                    'lok.probaugNotationFST.gemcode = 
                    If clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD5")) IsNot Nothing Then

                        If IsNumeric(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD5"))) Then
                            lok.probaugNotationFST.gemcode = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD5")))
                        Else
                            mylog("fehler in lok.blattnr, gemarkung kaputt , " & lok.blattnr)
                            Continue For
                        End If
                    End If
                    evtlFlur = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD6")).Trim '10
                    Console.WriteLine("iz1 " & iz)

                    If evtlFlur.IsNothingOrEmpty Then
                        lok.probaugNotationFST.flur = 0
                    Else
                        If IsNumeric(evtlFlur) Then
                            lok.probaugNotationFST.flur = CInt(evtlFlur)
                        Else
                            lok.probaugNotationFST.flur = 0
                        End If
                    End If
                    lok.probaugNotationFST.fstueckKombi = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD7")).Trim '406/1
                    lok.gueltig = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD8")).Trim 'J

                    lok.status = clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD3")).Trim '1
                    'lok.laufnr = CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD9"))) '17655
                    lok.laufnr = 0 'CInt(clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD2"))) 'keine ahnung

                    lok.datum = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD10"))).Trim 'leer
                    lok.AzJahr = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD10"))).Trim 'leer
                    lok.AzOG = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD11"))).Trim 'leer blödsinn
                    lok.AzNr = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD12"))).Trim 'leer
                    lok.Rechtswert = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD13"))).Trim 'leer
                    lok.Hochwert = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD14"))).Trim 'leer
                    lok.Prefix = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD15"))).Trim 'leer immmer
                    lok.Kennziffer_1 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD16"))).Trim 'leer
                    lok.Kennziffer_2 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD17"))).Trim 'leer
                    lok.Kennziffer_3 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD18"))).Trim 'leer
                    lok.Kennziffer_4 = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("FELD19"))).Trim 'leer

                    lok.datumgeloescht = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("datedeleted"))).Trim 'leer
                    lok.datum = (clsDBtools.fieldvalue(balistDT.Rows(i).Item("dateadded"))).Trim 'leer
                    '   lok.datum1 = clsDBtools.fieldvalue(balistDT.Rows(i).Item("angelegt")).Trim '"2020.07.10"
                    '  lok.datumgeloescht = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                    ' lok.probaugNotationFST.zeigtauf = clsDBtools.fieldvalue(balistDT.Rows(i).Item("loesch")).Trim 'leer
                    'b = clsDBtools.fieldvalue(balistDT.Rows(i).Item("a13")).Trim
                    If istgeschlossen(lok.blattnr, geschlossen) Then Continue For
                    iz += 1
                    rawlist.Add(lok)
                Catch ex As Exception
                    l("Fehler in dtnachobj: " & ex.ToString())
                    Return Nothing
                End Try
            Next
            Return rawlist
            l("dtnachobj ---------------------- ende")
        Catch ex As Exception
            l("Fehler in dtnachobj: " & ex.ToString())
            Return Nothing
        End Try
    End Function
    Private Function istgeschlossen(blattnr As String, geschlossen As DataTable) As Boolean
        Try
            l(" MOD istgeschlossen anfang")
            If geschlossen Is Nothing Then Return False
            For Each ds As DataRow In geschlossen.AsEnumerable
                If CStr(ds.Item(0)) = blattnr Then
                    Return False
                    'Return True
                End If
            Next
            Return False
            l(" MOD istgeschlossen ende")
            Return True
        Catch ex As Exception
            l("Fehler in istgeschlossen: " & ex.ToString())
            Return False
        End Try
    End Function

    Function calcDateiname(lok As clsBaulast) As String
        Dim datei As String
        datei = pfad & lok.probaugNotationFST.gemarkungstext & "\" & lok.blattnr & ".pdf"
        Return datei
    End Function

    Function getBauort(bauortNr As String) As String
        Dim test As Integer
        test = CInt(bauortNr.Trim)
        Dim retval As String = bauortNr
        Try
            retval = gemeindedict(test)
        Catch ex As Exception
            retval = bauortNr
        End Try
        Return retval
    End Function

    Function getTiff(lok As clsBaulast, pfad As Object) As Boolean
        Return False
    End Function

    Function getProbaugGemarkungsText(probaugGemarkung As Integer) As String
        Dim test As Integer
        Dim retval As String
        Try
            l("---------------------- anfang")
            test = CInt(probaugGemarkung)
            retval = CType(probaugGemarkung, String)
            Try
                retval = probaugGemarkungsdict(test)
            Catch lex As Exception
                retval = CType(probaugGemarkung, String)
            End Try

            Return retval
            l("---------------------- ende")
        Catch zex As Exception
            l("Fehler in : " & zex.ToString())
            Return "unbekannt"
        End Try
    End Function
    Sub l(v As String)
        nachricht(v)
    End Sub
    Sub nachricht(ByVal text$)
        My.Log.WriteEntry(text)
    End Sub

    Function calcDateiExistiert(lok As clsBaulast) As Boolean

        Dim fi As New IO.FileInfo(lok.datei)
        If fi.Exists Then
            fi = Nothing
            Return True
        Else
            fi = Nothing
            Return False
        End If
    End Function
    'Function istSchonVorhanden(fS As String) As Boolean
    '    Dim hinweis As String = ""
    '    fstREC.mydb.SQL = "select * from " & tools.srv_schema & "." & tools.srv_tablename & "   where fs='" & fS & "'"
    '    l(fstREC.mydb.SQL)
    '    hinweis = fstREC.getDataDT()
    '    If fstREC.dt.Rows.Count < 1 Then

    '        Return False
    '    Else
    '        Return True
    '    End If
    'End Function


    Sub initKatasterGemarkungtext()
        katasterGem(0) = "                              ; "
        katasterGem(1) = "Buchschlag                         ;726"
        katasterGem(2) = "Bürgel                             ;727"
        katasterGem(3) = "Dietesheim                         ;728"
        katasterGem(4) = "Dietzenbach                        ;729"
        katasterGem(5) = "Dreieichenhain                     ;730"
        katasterGem(6) = "Dudenhofen                         ;731"
        katasterGem(7) = "Egelsbach                          ;732"
        katasterGem(8) = "Froschhausen                       ;733"
        katasterGem(9) = "Götzenhain                         ;734"
        katasterGem(10) = "Hainhausen                         ;735"
        katasterGem(11) = "Hainstadt                          ;736"
        katasterGem(12) = "Hausen                             ;737"
        katasterGem(13) = "Heusenstamm                        ;738"
        katasterGem(14) = "Jügesheim                          ;739"
        katasterGem(15) = "Klein-Krotzenburg                  ;740"
        katasterGem(16) = "Klein-Welzheim                     ;741"
        katasterGem(17) = "Lämmerspiel                        ;742"
        katasterGem(18) = "Langen                             ;743"
        katasterGem(19) = "Mainflingen                        ;744"
        katasterGem(20) = "Messenhausen                       ;745"
        katasterGem(21) = "Mühlheim                           ;746"
        katasterGem(22) = "Nieder-Roden                       ;747"
        katasterGem(23) = "Neu-Isenburg                       ;748"
        katasterGem(24) = "Ober-Roden                         ;749"
        katasterGem(25) = "Offenbach                          ;751"
        katasterGem(26) = "Offenthal                          ;752"
        katasterGem(27) = "Rembrücken                         ;753"
        katasterGem(28) = "Rumpenheim                         ;754"
        katasterGem(29) = "Seligenstadt                       ;755"
        katasterGem(30) = "Sprendlingen                       ;756"
        katasterGem(31) = "Urberach                           ;757"
        katasterGem(32) = "Weiskirchen                        ;758"
        katasterGem(33) = "Zellhausen                         ;759"
        katasterGem(34) = "Zeppelinheim                       ;760"
        katasterGem(35) = "Obertshausen                       ;750"

    End Sub
    Sub initProbaugNrProbaugGemarkungtext()
        gem(0) = "4	Dreieichenhain"
        gem(1) = "5	Sprendlingen"
        gem(2) = "6	Offenthal"
        gem(3) = "7	Götzenhain"
        gem(4) = "8	Buchschlag"
        gem(5) = "9	Hainstadt"
        gem(6) = "10 Klein-Krotzenburg"
        gem(7) = "11 Rembrücken"
        gem(8) = "12 Mainflingen"
        gem(9) = "13 Zellhausen"
        gem(10) = "14	Lämmerspiel"
        gem(11) = "15	Dietesheim"
        gem(12) = "16	Obertshausen"
        gem(13) = "17	Hausen"
        gem(14) = "18	Zeppelinheim"
        gem(15) = "20	Jügesheim"
        gem(16) = "21	Dudenhofen"
        gem(17) = "22	Nieder-Roden"
        gem(18) = "23	Hainhausen"
        gem(19) = "24	Weiskirchen"
        gem(20) = "25	Urberach"
        gem(21) = "26	Ober-Roden"
        gem(22) = "28	Messenhausen"
        gem(23) = "29	Froschhausen"
        gem(24) = "30	Klein-Welzheim"
        gem(25) = "32	Heusenstamm"
        gem(26) = "34	Seligenstadt"
        gem(27) = "35	Egelsbach"
        gem(28) = "36	Mühlheim"
        gem(29) = "40	Dietzenbach"
        gem(30) = "41	Langen"
        gem(31) = "42	Neu-Isenburg"
        gem(32) = "2	Bayerseich"
        gem(33) = "60	Im-Brühl"
        gem(34) = "27	Unbekannt27"
        gem(35) = "3	Unbekannt3"
        gem(36) = "33	Unbekannt33"
        gem(37) = "0	Unbekannt0"
    End Sub
    Function initgemeinde() As String
        gemeinde(0) = "1 ;Dietzenbach                        "
        gemeinde(1) = "2 ;Dreieich                           "
        gemeinde(2) = "3 ;Egelsbach                          "
        gemeinde(3) = "4 ;Hainburg                           "
        gemeinde(4) = "5 ;Heusenstamm                        "
        gemeinde(5) = "6 ;Langen                             "
        gemeinde(6) = "7 ;Mainhausen                         "
        gemeinde(7) = "8 ;Mühlheim                           "
        gemeinde(8) = "9 ;Neu-Isenburg                       "
        gemeinde(9) = "10;Obertshausen                       "
        gemeinde(10) = "0 ;Offenbach                          "
        gemeinde(11) = "11;Rodgau                             "
        gemeinde(12) = "12;Rödermark                          "
        gemeinde(13) = "13;Seligenstadt                       "
        'gemeinde(14) = "0 ;                          "

    End Function

    Function objErweitern(balist As List(Of clsBaulast), ByRef anzahltiff As Integer,
                              ByRef anzahl_dateiexitiert As Integer,
                              ByRef anzahl_blattnrIst0 As Integer) As Boolean
        anzahltiff = 0
        anzahl_dateiexitiert = 0
        anzahl_blattnrIst0 = 0
        Dim iz As Integer = 0
        Try
            l("objErweitern---------------------- anfang")
            For Each lok As clsBaulast In balist
                Try
                    lok.probaugNotationFST.gemarkungstext = getProbaugGemarkungsText(lok.probaugNotationFST.gemcode)
                Catch lex As Exception
                    ' lok.probaugFST.gemarkungstext = "unbekannt" ' (" & lok.probaugFST.gemcode.ToString & ")"
                End Try

                'setKatasterGemarkung(lok, katasterGemarkungsdict)
                'If iz = 7300 Then
                '    Debug.Print("")
                'End If
                Console.WriteLine(iz.ToString & " von " & balist.Count)
                iz += 1
                getKatasterGemarkung(lok, katasterGemarkungslist)
                lok.gemeindeText = getBauort(lok.bauortNr)
                lok.katFST.flur = getKatFlur(lok)
                lok.katFST.fstueckKombi = lok.katFST.buildFstueckkombi
                lok.katFST.zaehler = getKatzaehler(lok)
                If lok.katFST.zaehler < 1 Then
                    getKatZaehlerUndNenner(lok)
                End If

                lok.hatTiff = getTiff(lok, pfad)
                If lok.hatTiff Then anzahltiff += 1
                lok.datei = calcDateiname(lok)
                lok.dateiExistiert = calcDateiExistiert(lok)
                If lok.dateiExistiert Then anzahl_dateiexitiert += 1
                If lok.blattnr = "0" Or lok.blattnr.IsNothingOrEmpty Then
                    anzahl_blattnrIst0 += 1
                End If
            Next
            Return True
            l("objErweitern---------------------- ende")
        Catch ex As Exception
            l("Fehler in objErweitern: " & ex.ToString())
            Return False
        End Try
    End Function

    Sub getKatZaehlerUndNenner(lok As clsBaulast)
        Dim temp, a(), b() As String
        Try
            l("getKatZaehlerUndNenner---------------------- anfang")
            '1468/3 tlw.
            If lok.probaugNotationFST.fstueckKombi.IsNothingOrEmpty Then
                lok.katFST.zaehler = 0
                lok.katFST.nenner = 0
            End If
            temp = lok.probaugNotationFST.fstueckKombi.Replace("\", "/").ToLower
            temp = temp.Replace("//", "/")
            temp = temp.Replace("(", " ")
            temp = temp.Replace(")", " ")
            temp = temp.Replace("a", " ")
            temp = temp.Replace("b", " ")
            temp = temp.Replace("c", " ")
            temp = temp.Replace("d", " ")
            temp = temp.Replace("e", " ")
            temp = temp.Replace("f", " ")
            temp = temp.Replace("g", " ")
            temp = temp.Trim
            If temp.EndsWith("/") Then
                temp = temp.Replace("/", "")
            End If

            If (temp.Contains("/")) Then

                b = temp.Split("/"c)
                'zaehler
                If IsNumeric(b(0)) Then
                    lok.katFST.zaehler = CInt(b(0))
                Else
                    lok.katFST.zaehler = 0
                End If
                'nenner
                If IsNumeric(b(1)) Then
                    lok.katFST.nenner = CInt(b(1))
                Else
                    b(1) = b(1).Replace("-", " ")
                    b(1) = b(1).Replace(".", " ")
                    a = b(1).Split(" "c)
                    If IsNumeric(a(0)) Then
                        lok.katFST.nenner = CInt(a(0))
                    Else
                        lok.katFST.nenner = 0
                    End If
                End If

            Else
                If IsNumeric(temp) Then
                    lok.katFST.zaehler = CInt(temp)

                    lok.katFST.zaehler = 0
                Else
                    lok.katFST.zaehler = 0
                    lok.katFST.nenner = 0
                End If
            End If
            l("getKatZaehlerUndNenner---------------------- ende")
        Catch ex As Exception
            l("Fehler in getKatZaehlerUndNenner: " & ex.ToString())

        End Try
    End Sub

    Function getKatzaehler(lok As clsBaulast) As Integer
        Try
            l("getKatzaehler---------------------- anfang")
            If lok.probaugNotationFST.fstueckKombi.IsNothingOrEmpty Then
                Return 0
            End If
            If IsNumeric(lok.probaugNotationFST.fstueckKombi) Then
                Return CInt(lok.probaugNotationFST.fstueckKombi)
            End If
            Return 0
            l("getKatzaehler---------------------- ende")
        Catch ex As Exception
            l("Fehler in getKatzaehler: " & ex.ToString())
            Return 0
        End Try

    End Function

    Function getKatFlur(lok As clsBaulast) As Integer
        Try
            l("getKatFlur---------------------- anfang")
            If lok.probaugNotationFST.flur < 1 Then
                Debug.Print("")
                Return 0
            End If
            Return lok.probaugNotationFST.flur
            l("getKatFlur---------------------- ende")
        Catch ex As Exception
            l("Fehler in getKatFlur: " & ex.ToString())
            Return 0
        End Try
    End Function

    Private Sub getKatasterGemarkung(lok As clsBaulast, katasterGemarkungslist As List(Of myComboBoxItem))
        Try
            l("getKatasterGemarkung---------------------- anfang")
            For i = 0 To katasterGemarkungslist.Count - 1
                If lok.probaugNotationFST.gemarkungstext.Trim.ToLower = katasterGemarkungslist(i).mySttring.ToLower Then
                    lok.katFST.gemcode = CInt(katasterGemarkungslist(i).myindex.ToLower)
                    Exit Sub
                End If
            Next
            lok.katFST.gemcode = 0
            nachricht("probaugGemarkugnen ohne Kataster:" & lok.probaugNotationFST.gemarkungstext.Trim.ToLower)
            l("getKatasterGemarkung---------------------- ende")
        Catch ex As Exception
            l("Fehler in getKatasterGemarkung: " & ex.ToString())
        End Try
    End Sub
    Friend Function loescheEintragInRawList(geloescht As clsBaulast) As Boolean
        Dim retval As Boolean = False
        Return True
        Try
            l("loescheEintragInRawList---------------------- anfang")
            For Each lok As clsBaulast In rawListOfclsBaulast
                If lok.bauortNr = geloescht.bauortNr And
                   lok.blattnr = geloescht.blattnr And
                   lok.geloescht = False Then
                    lok.geloescht = True
                    retval = True
                End If
            Next

            l("loescheEintragInRawList---------------------- ende")
            Return retval
        Catch ex As Exception
            l("Fehler in loescheEintragInRawList: " & ex.ToString())
            Return False
        End Try
    End Function

    Sub viererLoeschen(ByRef viererGeloescht As Integer)
        viererGeloescht = 0
        l("viererLoeschen---------------------- anfang")
        Try
            l("viererLoeschen---------------------- anfang")
            For Each geloescht As clsBaulast In list4Geloscht
                If Not geloescht.katasterFormellOK Then Continue For
                'If istSchonVorhanden(lok.katFST.FS) Then
                'End If
                If tools.loescheEintragInRawList(geloescht) Then
                    viererGeloescht += 1
                End If
            Next
            l("viererLoeschen---------------------- ende")
        Catch ex As Exception
            l("Fehler in viererLoeschen: " & ex.ToString())
        End Try
    End Sub

    Friend Function getSerialFromBasis(lok As clsBaulast, Tabname As String) As String
        Dim hinweis As String = ""
        Try
            l("getSerialFromBasis---------------------- anfang")
            fstREC.mydb.SQL = "select ST_AsText(ST_CurveToLine(geom)) from " & Tabname & "   where fs='" & lok.katFST.FS & "'"
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return ""
            Else
                Return clsDBtools.fieldvalue(fstREC.dt.Rows(0).Item(0))
            End If
            l("getSerialFromBasis---------------------- ende")
        Catch ex As Exception
            l("Fehler in getSerialFromBasis: " & ex.ToString())
            Return ""
        End Try
    End Function
    Sub getAllSerials(ByRef anzahl_mitSerial As Integer, OUTohneFlurstueck As String)
        l("getAllSerials---------------------- anfang: " & OUTohneFlurstueck)
        Dim temp, tabname As String
        anzahl_mitSerial = 0
        Dim iz As Integer = 0
        Dim trenn As String = ";"
        Dim sw As IO.StreamWriter
        Try
            tabname = "flurkarte.basis_f"
            sw = New IO.StreamWriter(OUTohneFlurstueck)
            sw.WriteLine("gemeinde " & trenn &
                        "baulast: " & trenn &
                        "Bauort " & trenn &
                        "blattnr " & trenn &
                        "Kat. gemnr: " & trenn &
                        "Kat. gemarkung: " & trenn &
                        "Kat. flur: " & trenn &
                        "Kat. zaehler: " & trenn)

            l("getAllSerials---------------------- anfang 2")
            For Each lok As clsBaulast In rawListOfclsBaulast
                Console.WriteLine("getAllSerials " & iz)
                If lok.blattnr = "90764" Then
                    Debug.Print("")
                End If
                iz += 1
                If Not lok.katasterFormellOK Then Continue For
                If lok.geloescht Then Continue For
                temp = tools.getSerialFromBasis(lok, tabname)
                If temp.IsNothingOrEmpty Then
                    sw.WriteLine(lok.gemeindeText & trenn &
                                  lok.baulastnr & trenn &
                                  lok.bauortNr & trenn &
                                  lok.blattnr & trenn &
                                  lok.katFST.gemcode & trenn &
                                    lok.katFST.gemarkungstext & trenn &
                                  lok.katFST.flur & trenn &
                                  lok.katFST.zaehler & trenn)
                    tools.getSerialFromHistBasis(lok, tabname, anzahl_mitSerial)
                Else
                    lok.serial = temp
                    lok.gefundenIn = tabname
                    anzahl_mitSerial += 1
                End If
            Next
            l("getAllSerials---------------------- ende")
            sw.Close()
            sw.Dispose()
        Catch ex As Exception
            l("Fehler ingetAllSerials : " & ex.ToString())
            sw.Close()
            sw.Dispose()
        End Try
    End Sub

    Private Function getSerialFromHistBasis(lok As clsBaulast, ByRef gefundenin As String, ByRef anzahl_mitSerial As Integer) As String
        Dim basisarray(), tabname, temp As String
        Try
            l("getSerialFromHistBasis---------------------- anfang")
            basisarray = getBasisArray()
            For i = 0 To basisarray.Count - 1
                tabname = "h_flurkarte." & basisarray(i)
                temp = tools.getSerialFromBasis(lok, tabname)
                If Not temp.IsNothingOrEmpty Then
                    lok.serial = temp
                    lok.gefundenIn = tabname
                    anzahl_mitSerial += 1
                    Return temp
                End If
            Next
            lok.serial = ""
            lok.gefundenIn = ""
            Return ""
            l("getSerialFromHistBasis---------------------- ende")
        Catch ex As Exception
            l("Fehler in getSerialFromHistBasis: " & ex.ToString())
            Return ""
        End Try
    End Function

    Private Function getBasisArray() As String()
        Dim basis(14) As String
        basis(0) = "j2019_basis_f"
        basis(1) = "j2018_basis_f"
        basis(2) = "j2017_basis_f"
        basis(3) = "j2016_basis_f"
        basis(4) = "j2015_basis_f"
        basis(5) = "j2014_basis_f"
        basis(6) = "j2013_basis_f"
        basis(7) = "j2012_basis_f"
        basis(8) = "j2011_basis_f"
        basis(9) = "j2010_basis_f"

        basis(10) = "j1998_flurstueck_f"
        basis(11) = "j1999_flurstueck_f"
        basis(12) = "j2000_flurstueck_f"
        basis(13) = "j2001_flurstueck_f"
        basis(14) = "j2002_flurstueck_f"

        Return basis
    End Function

    'Sub write2postgis(lok As clsBaulast, ByRef erfolg As Boolean, ByRef sql As String,
    '                  coordinatesystemNumber As String, datei As String, datei2 As String,
    '                  genese As Integer, outputTablename As String)
    '    l("write2postgis nichtverwendet " & tools.srv_tablename)
    '    l("write2postgis verwendet " & outputTablename)

    '    Try
    '        sql = "INSERT INTO " & tools.srv_schema & "." & outputTablename & " " &
    '                     "(geom,fs,kennzeichen1,baulastnr,jahr_blattnr,bauort,gueltig," &
    '                     "datum,flur,flurstueck,zaehler,nenner,gefundenin,tiff,gemeinde,gemarkung,gemcode,genese,tiff2) " &
    '                     "VALUES( ST_GeomFromText('" & lok.serial & "'," & coordinatesystemNumber & "),'" &
    '                        lok.katFST.FS & "','" &
    '                        lok.status.Trim & "','" &
    '                        lok.baulastnr.Trim & "','" &
    '                        lok.blattnr.Trim & "','" &
    '                        lok.bauortNr.Trim & "','" &
    '                        lok.gueltig.Trim & "','" &
    '                        lok.datum.Trim & "','" &
    '                        lok.katFST.flur & "','" &
    '                        lok.katFST.fstueckKombi.Trim & "','" &
    '                        lok.katFST.zaehler & "','" &
    '                        lok.katFST.nenner & "','" &
    '                        lok.gefundenIn & "','" &
    '                        datei & "','" &
    '                        lok.gemeindeText & "','" &
    '                        lok.probaugNotationFST.gemarkungstext & "','" &
    '                        lok.katFST.gemcode & "','" &
    '                        genese & "','" &
    '                        datei2 & "')"
    '        Dim dtRBplus As New DataTable
    '        erfolg = sqlausfuehren(sql, fstREC.mydb, dtRBplus)
    '        l("write2postgis ende")
    '    Catch ex As Exception
    '        l("fehler in write2postgis" & ex.ToString)

    '    End Try
    'End Sub
    Sub createDir(targetroot As String)
        Try
            l(" createDir ---------------------- anfang" & targetroot)
            'MsgBox("Vor targetroot createdir " & targetroot)
            IO.Directory.CreateDirectory(targetroot)
            l(" createDir ---------------------- ende")

        Catch ex As Exception
            l("Fehler in createDir: " & ex.ToString())
            MsgBox(ex.Message & " fehler in createdir  " & targetroot)
        End Try
    End Sub
    Public Function ReadSemicolonFileAllText(path As String) As List(Of String())
        Dim result As New List(Of String())
        Dim content As String = IO.File.ReadAllText(path)
        ' Zeilen sauber trennen (Windows + Linux kompatibel)
        Dim lines() As String = content.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        Try
            For Each line As String In lines
                result.Add(line.Split(";"c))
            Next
            Return result
        Catch ex As Exception
            Return result
        End Try
    End Function
    'Friend Function HoleThemaFuerSachgebiet(sachgebiet As String, themen As List(Of String())) As String
    '    Dim thema As String = ""
    '    Dim backup As String
    '    l("HoleThemaFuer Sachgebiet " & sachgebiet)
    '    If sachgebiet = String.Empty Then
    '        sachgebiet = "1"
    '    End If
    '    l("HoleThemaFuer Sachgebiet " & sachgebiet)
    '    Try
    '        For Each str As String() In themen
    '            If str(0).Substring(0, 1) = sachgebiet Then
    '                backup = str(1)
    '                Return str(1)
    '            End If
    '        Next
    '        l("backup: " & backup)
    '        Return backup
    '    Catch ex As Exception
    '        l("ThemenAusThemenDateiHolen " & ex.ToString)
    '        Return thema
    '    End Try
    'End Function
    Public Function bplanAlsObjImGisZeigen(ident As Integer, themendatei As String) As String
        Dim url As String
        Dim themen As String
        themen = tools.getthemen("", themendatei)

        'url = "https://gis.kreis-of.de/LKOF/extensions/logout.asp?removeLostSession=true"




        If IsNumeric(ident) Then
            url = "https://gis.kreis-of.de/LKOF/asp/main.asp?" & themen &
                "&app=sp_mdat&lay=sp_mdat_0013_F&fld=ident&typ=string&val=" & ident & "&skipwelcome=true"
            '  Process.Start(url)
            Return url
        Else
            'MsgBox("Die Bplan ident. '" & ident & "' ist ungültig!")
            Return ""
        End If
    End Function
    Public Sub baulastAlsObjImGisZeigen(baulastblatt As String, themendatei As String)
        Dim url As String
        Dim themen As String
        themen = tools.getthemen("", themendatei)
        'theme=BauenUndUmwelt,Eigene%20Daten,Grenzen,Liegenschaften
        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        If gisLogouten Then
            Process.Start(logout)
            Threading.Thread.Sleep(1000)
        End If
        If IsNumeric(baulastblatt) Then
            url = "https://gis.kreis-of.de/LKOF/asp/main.asp?" & themen & "&app=sp_mdat&lay=sp_mdat_0010_F&fld=text3&typ=string&val=" & baulastblatt & "&skipwelcome=true"
            Process.Start(url)
        Else
            MsgBox("Die BaulastNr. '" & baulastblatt & "' ist ungültig!")
        End If
    End Sub
    Friend Function getthemen(url As String, themendefinitionsdatei As String) As String
        Dim exepath, themendatei As String
        Dim theme As String
        Dim a() As String
        Try
            exepath = AppDomain.CurrentDomain.BaseDirectory
#If DEBUG Then
            exepath = "W:\diverses"
#End If
            l("exepath: " & exepath)
            themendatei = IO.Path.Combine(exepath, themendefinitionsdatei)
            l("themendatei: " & themendatei)
            themendatei = themendatei.Replace("bgmingrada\", "")
            l("themendatei: " & themendatei)
            Dim fi As New IO.FileInfo(themendatei)
            If fi.Exists Then
                theme = IO.File.ReadAllText(themendatei)
                l("eingelesen: " & theme)
                a = theme.Split(";"c)
                Return a(1).Trim
            Else
                l("fehler themendatei: ''")
                Return ""
            End If
        Catch ex As Exception
            l("getthemen " & ex.ToString)
            Return ""
        End Try
    End Function

    'Private Sub makeConnection(ByVal host As String, datenbank As String, ByVal dbuser As String, ByVal dbpw As String, ByVal dbport As String)
    '    Dim csb As New NpgsqlConnectionStringBuilder
    '    Try
    '        l("makeConnection")
    '        'If String.IsNullOrEmpty(mydb.ServiceName) Then
    '        'klassisch
    '        csb.Host = host
    '        ' csb. = mydb.Schema
    '        csb.UserName = dbuser
    '        csb.Password = dbpw
    '        csb.Database = datenbank
    '        csb.Port = CInt(dbport)
    '        csb.Pooling = False
    '        csb.MinPoolSize = 1
    '        csb.MaxPoolSize = 20
    '        csb.Timeout = 15
    '        csb.SslMode = SslMode.Disable
    '        myconn = New NpgsqlConnection(csb.ConnectionString)
    '        l("makeConnection fertig " & csb.ConnectionString)
    '    Catch ex As Exception
    '        l("fehler in makeConnection" & ex.ToString)
    '    End Try
    'End Sub


    'Public myconn As NpgsqlConnection


    'Function sqlausfuehren(sql As String, Postgis_MYDB As clsDatenbankZugriff, tempdt As DataTable) As Boolean
    '    '  ini_PGREC(tablename)
    '    makeConnection(Postgis_MYDB.Host, Postgis_MYDB.Schema, Postgis_MYDB.username, Postgis_MYDB.password, "5432")
    '    l("in sqlausfuehren")
    '    l(sql)
    '    Try
    '        myconn.Open()
    '        Dim com As New NpgsqlCommand(sql, myconn)
    '        Dim da As New NpgsqlDataAdapter(com)
    '        'da.MissingSchemaAction = MissingSchemaAction.AddWithKey
    '        ' dtRBplus = New DataTable
    '        Dim _mycount = da.Fill(tempdt)
    '        myconn.Close()
    '        myconn.Dispose()
    '        com.Dispose()
    '        da.Dispose()
    '        l("sqlausfuehren fertig")
    '        Return True
    '    Catch ex As Exception
    '        l("fehler in sqlausfuehren: " & ex.ToString)
    '        Return False
    '    End Try
    'End Function


    'Function getallTiffsinDB(temp As String, postgis_mydb As clsDatenbankZugriff, sql As String) As Boolean
    '    Dim hinweis As String = ""
    '    Try
    '        l(" MOD istInHartmannDB anfang")
    '        makeConnection(postgis_mydb.Host, postgis_mydb.Schema, postgis_mydb.username, postgis_mydb.password, "5432")
    '        fstREC.mydb.SQL = sql  '   where lower(trim(tiff2))='" & temp.Trim.ToLower & "'"
    '        l(fstREC.mydb.SQL)
    '        hinweis = fstREC.getDataDT()
    '        If fstREC.dt.Rows.Count < 1 Then
    '            Return False
    '        Else
    '            Return True
    '        End If
    '        l(" MOD istInHartmannDB ende")
    '        Return True
    '    Catch ex As Exception
    '        l("Fehler in istInHartmannDB: " & ex.ToString())
    '        Return False
    '    End Try
    'End Function
    'Function flurstueckZuFKZ(gemcode As String, flur As String, zaehler As String, nenner As String) As String
    '    l("in flurstueckZuFKZ")
    '    Dim fuell, fs2, _flur As String
    '    'Dim gemcode As String
    '    Dim result = "060"
    '    Try
    '        'splitFstueckkombi(fstueck, zaehler, nenner)
    '        l("zn " & zaehler & "_" & nenner)
    '        'gemcode = clsFlurauswahl.getGemcode(gemarkung)
    '        result = result & CInt(gemcode)
    '        result = result & "-"

    '        fuell = "000"
    '        fs2 = fuell.Substring(flur.ToString.Length) & flur


    '        result = result & fs2
    '        result = result & "-"

    '        fuell = "00000"
    '        fs2 = fuell.Substring(zaehler.ToString.Length) & zaehler

    '        result = result & fs2
    '        result = result & "/"

    '        fuell = "0000"
    '        fs2 = fuell.Substring(nenner.ToString.Length) & nenner

    '        result = result & fs2
    '        result = result & ".000"
    '        Return result
    '        '060729-005-00495/0001.000
    '        '060729-012-00530/0008.000
    '        '061301-026-00004/0001.000
    '    Catch ex As Exception
    '        l(ex.ToString)
    '        Return "fehler in adresseZuFKZ"
    '    End Try
    'End Function
    Public Function makeFlurstuecksAbstrakt(dieliste As List(Of clsFlurstueck)) As String
        Dim summe As String
        Try
            For Each fst As clsFlurstueck In dieliste
                summe = summe & "== Grundstück: " & fst.gemeindename & ", Gemarkung: " &
                fst.gemarkungstext & ", Flur: " &
                fst.flur & ", Fst: " &
                fst.zaehler & "/" & fst.nenner & " =="

                'summe = summe & "== Lage: " & fst.gemeindename & ", " & lage &
                '   " =="
            Next
            Return summe
        Catch ex As Exception
            l(ex.ToString)
            Return "Fehler - Flurstück nicht vorhanden?"
        End Try
    End Function
    Public Function erzeugeWordDateiEigentuemer(eigentuemertext As String, baulastnr As String) As String
        Dim filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Try
            filepath = IO.Path.Combine(filepath, "Eigentümer_BL_" & baulastnr & ".docx")

            Dim wp As New eigentuemerWord
            Dim erfolg = wp.machma(eigentuemertext, filepath, Format(Now, "dd.MM.yyyy"))
            If erfolg Then
                'MsgBox("Die Worddatei wurde unter " & Environment.NewLine & filepath & Environment.NewLine & " abgelegt!")
            Else
                MsgBox("Die Worddatei konnte nicht erzeugt werden. Vermutlich haben Sie sie noch geöffnet.")
            End If
            Return filepath
        Catch ex As Exception
            l("fehler in erzeugeword")
            l(ex.ToString)
            Return "fehler"
        End Try
    End Function
    Public Sub gisFuerProbaugFlurst(baulast As String, flstliste As List(Of clsFlurstueck))
        Dim url As String
        l("gisFuerProbaugFlurst ")
        ' C:\kreisoffenbach\mgis\ingradaadapter.exe    suchmodus=flurstueck gemarkung="Hainhausen" flur="4" fstueck="387/1" 
        '91197
        Dim lokfkzliste As String
        Try
            lokfkzliste = machMultipleFstString(flstliste)
            If lokfkzliste.Length > 1 Then
                url = gisLogoutUndStartFKZ(lokfkzliste, gisLogouten)
            Else
                MsgBox("Keine Flurstücke zugeordnet!!!  GIS wird ohne Flurstück gestartet!")
                url = "https://gis.kreis-of.de/LKOF/asp/main.asp?"
                l("url " & url)
                Process.Start(url)
            End If
        Catch ex As Exception
            l("gisFuerProbaugFlurst " & ex.ToString)
        End Try
    End Sub

    Private Function machMultipleFstString(flstliste As List(Of clsFlurstueck)) As String
        Dim treffer As Integer = 0
        Dim fkztemp As String = ""
        Dim lokfkzliste As String = ""
        Try
            For i = 0 To flstliste.Count - 1
                fkztemp = flstliste(i).flurstueckZuFKZ
                Dim gemeindeschluessel, lagebezeichnung As String 'aktadr.gemeindebigNRstring aktadr.lage
                If tools.flurstueckExistiertImGis(fkztemp, gemeindeschluessel, lagebezeichnung) Then
                    treffer += 1
                    If treffer = 1 Then
                        lokfkzliste = fkztemp
                    Else
                        lokfkzliste = lokfkzliste & "," & fkztemp
                    End If
                Else

                End If
            Next
            l("treffer " & treffer)
            Return lokfkzliste
        Catch ex As Exception
            l("machMultipleFstString " & ex.ToString)
            Return lokfkzliste
        End Try
    End Function

    Public Function gisLogoutUndStartFKZ(lokfkzliste As String, mitlogout As Boolean) As String
        Dim url As String
        Dim logout = "https://gis.kreis-of.de/LKOF/asp/login.asp?logout=true&m=1"
        Try
            If mitlogout Then
                Process.Start(logout)
                Threading.Thread.Sleep(1000)
            End If
            url = makeurl4FST("https://gis.kreis-of.de/LKOF/asp/main.asp?", lokfkzliste, tools.themendefinitionsdatei)
            url = url.Replace("?&", "?").Replace("&&", "&")
            l("url " & url)
            Process.Start(url)
            l(lokfkzliste)
            l("fertig abgeschickt ")
            Return url
        Catch ex As Exception
            l("gisLogoutUndStartFKZ " & ex.ToString)
        End Try
    End Function

    Public Function makeurl4FST(baseurl As String, flurstueckskennzeichen As String, themendatei As String) As String
        l("in makurl")
        Try
            Dim themen As String
            themen = tools.getthemen("", themendatei)

            baseurl = baseurl & "app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val="
            baseurl = baseurl & flurstueckskennzeichen & "&" & themen & "&skipwelcome=true"
            baseurl = baseurl.Replace("?&", "?").Replace("&&", "&")
            Return baseurl
            'https://gis.kreis-of.de/LKOF/asp/main.asp?app=sp_lieg&obj=flu&fld=flurstueckskennzeichen&typ=string&val=060729-005-00490/0000.000&skipwelcome=true
            ' Die endung  .000  ist wichtig - sonst gehts nicht
        Catch ex As Exception
            l(ex.ToString)
            Return "fehler in makeurl4FST"
        End Try
    End Function

    Friend Function flurstueckExistiertImGis(flurstueckZuFKZ As String, ByRef gemeindeschluessel As String, ByRef lagebezeichnung As String) As Boolean
        Dim sql, hinweis As String
        Dim newid As Long
        l("flurstueckExistiertImGis")
        Try
            fstREC.mydb.SQL = "use LKOF;select * FROM [LKOF].[dbo].[tbl_lieg_flurstueck] where flurstueckskennzeichen='" & flurstueckZuFKZ & "'"
            l(fstREC.mydb.SQL)
            Dim retcode = fstREC.dboeffnen(hinweis)
            Dim com As New SqlCommand(fstREC.mydb.SQL, fstREC.myconn)
            Dim da As New SqlDataAdapter(com)
            Dim reader As SqlDataReader = com.ExecuteReader()

            While reader.Read()
                hinweis = reader("flurstueckskennzeichen").ToString
                fst_lage = "== Lage: " & fst_lage & ", " & reader("lagebezeichnung").ToString & " =="
                gemeindeschluessel = reader("gemeinde_gemeindeschluessel").ToString
                lagebezeichnung = reader("lagebezeichnung").ToString
            End While

            retcode = fstREC.dbschliessen(hinweis)
            If hinweis = flurstueckZuFKZ Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            l("fehler in flurstueckExistiertImGis Abfrage gescheitert " & ex.ToString)
            Return False
        End Try
    End Function

    Friend Function readBLBlattCookie(cookiefile As String) As String
        l("readBLBlattCookie")
        '  Dim cookiefile = "bgm_blattnr_cookie.txt"
        Dim result As String
        Try
            Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)

            Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
            cookiefile = IO.Path.Combine(test, cookiefile)
            result = IO.File.ReadAllText(cookiefile)
            Return result
        Catch ex As Exception
            l("fehler in readBLBlattCookie Abfrage gescheitert " & ex.ToString)
            Return "6428"
        End Try
    End Function
    Friend Sub writeBLBlattCookie(text As String, cookiefile As String)
        l("readBLBlattCookie")
        'Dim cookiefile = "bgm_blattnr_cookie.txt"
        Dim result As String = ""
        Try
            Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
            cookiefile = IO.Path.Combine(test, cookiefile)
            IO.File.WriteAllText(cookiefile, text)
        Catch ex As Exception
            l("fehler in writeBLBlattCookie   gescheitert " & ex.ToString)
        End Try
    End Sub
    Friend Sub writeFlurstCookie(gemarkung As String, flur As String, zaehler As String, nenner As String, cookiefile As String)
        l("readBLBlattCookie")
        'Dim cookiefile = "bgm_FST_cookie.txt"
        Dim result As String = ""
        Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
        Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
        Try
            result = gemarkung.Trim & "," & flur.Trim & "," & zaehler.Trim & "," & nenner.Trim & ","
            cookiefile = IO.Path.Combine(test, cookiefile)
            IO.File.WriteAllText(cookiefile, result)
        Catch ex As Exception
            l("fehler in writeFlurstCookie   gescheitert " & ex.ToString)
        End Try
    End Sub
    Friend Function readFSTCookie(ByRef gemarkung As String, ByRef flur As String, ByRef zaehler As String, ByRef nenner As String, cookiefile As String) As Boolean
        l("readFSTCookie")
        '  Dim cookiefile = "bgm_FST_cookie.txt"
        Dim result As String
        Dim a() As String
        Try
            Dim testfolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            Dim test = IO.Path.Combine(testfolder, "bgm\cookies")
            cookiefile = IO.Path.Combine(test, cookiefile)
            result = IO.File.ReadAllText(cookiefile)
            l("result")
            a = result.Split(","c)
            gemarkung = a(0)
            flur = a(1)
            zaehler = a(2)
            nenner = a(3)
            Return True
        Catch ex As Exception
            l("fehler in readFSTCookie Abfrage gescheitert " & ex.ToString)
            gemarkung = "0"
            flur = ""
            zaehler = ""
            nenner = ""
            Return False
        End Try
    End Function
    Public Function bildeflurstuecksstring(fSTausGISListe As List(Of clsFlurstueck)) As String
        Dim lokfkzliste As String
        Dim treffer As Integer = 0
        Dim fkztemp As String = ""
        Try
            For i = 0 To fSTausGISListe.Count - 1
                treffer += 1
                fkztemp = fSTausGISListe(i).flurstueckZuFKZ
                If treffer = 1 Then
                    lokfkzliste = fkztemp
                Else
                    lokfkzliste = lokfkzliste & "," & fkztemp
                End If
            Next
            Return lokfkzliste
        Catch ex As Exception
            l("fehler in bildeflurstuecksstring-- " & ex.ToString)
            Return ""
        End Try
    End Function

    Friend Function sucheNachBplaenen(gemarkung As String, bplNNamensFilter As String, bplankategorie As String) As List(Of myComboBoxItem)
        Dim liste As New List(Of myComboBoxItem)
        Dim bpl As myComboBoxItem
        Dim hinweis As String = ""
        ' SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='F52CBA15-FAFF-4EDD-BBD3-B821920F1360' and text1 ='Seligenstadt'
        Try
            l(" MOD ---------------------- anfang")
            l("getSerialFromBasis---------------------- anfang")
            'fstREC.mydb.SQL = "select * from " & tools.srv_schema & "." & tools.srv_tablename & " where jahr_blattnr ='" & BaulastNR & "' order by gemcode, flur, zaehler, nenner"

            If bplNNamensFilter = String.Empty Then
                fstREC.mydb.SQL = "SELECT * FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz]" &
                        " where kategorie_guid='" & bplankategorie & "' " &
                        " and text2='" & gemarkung.Trim & "' order by text1, text2, text3, text4"
            Else
                fstREC.mydb.SQL = "SELECT * FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz]" &
                        " where kategorie_guid='" & bplankategorie & "' " &
                        " and text2='" & gemarkung.Trim &
                        "' and (text3 like '%" & bplNNamensFilter & "%' or text4 like '%" & bplNNamensFilter & "%')" &
                        " order by text1, text2, text3, text4"
            End If


            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return liste
            Else
                For i = 0 To fstREC.dt.Rows.Count - 1
                    bpl = New myComboBoxItem
                    bpl.myindex = fstREC.dt.Rows(i).Item("ident").ToString
                    bpl.mySttring = fstREC.dt.Rows(i).Item("text3").ToString & " -> " & fstREC.dt.Rows(i).Item("text4").ToString
                    liste.Add(bpl)
                Next
                Return liste
            End If

        Catch ex As Exception
            l("fehler in sucheNachBplaenen-- " & ex.ToString)
            Return liste
        End Try
    End Function

    Friend Function getAllMetaData4ThisBplanIdentNr(bplanindex As String) As clsBplan
        Dim apl As New clsBplan
        Dim hinweis As String
        l(" getAllMetaData4ThisBplanIdentNr  " & bplanindex)
        Try
            fstREC.mydb.SQL = "SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='" & kategorie_guid_Bplaene &
                "' and ident =" & bplanindex
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return apl 'darf nicht vorkommen
            Else
                For i = 0 To fstREC.dt.Rows.Count - 1
                    apl = New clsBplan
                    apl.ident = CInt(clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("ident")).ToString)
                    apl.gemeindetext = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text1")).ToString
                    apl.gemarkungstext = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text2")).ToString
                    apl.bplnummer = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text3")).ToString
                    apl.bplbeschreibung = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text4")).ToString
                    apl.nutzung = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text5")).ToString
                    apl.warnung = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text6")).ToString
                    apl.ueberlagertvon = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text7")).ToString
                    apl.ueberlagertselbst = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text8")).ToString
                    apl.flaeche = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("int1")).ToString
                    apl.rechtswirksam = clsDBtools.fieldvalueDate(fstREC.dt.Rows(i).Item("date2"))
                    apl.aufstellung = clsDBtools.fieldvalueDate(fstREC.dt.Rows(i).Item("date1"))
                    apl.object_guid = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("guid")).ToString
                    apl.object_guid = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("guid")).ToString.ToUpper


                Next
                Return apl
            End If
            Return apl
        Catch ex As Exception
            l("fehler in getAllMetaData4ThisBplanIdentNr-- " & ex.ToString)
            Return apl
        End Try
    End Function

    Friend Function getAllPDFFiles4GUID(object_guid As String, quellpfad As String) As List(Of myComboBoxItem)
        Dim pdfliste As New List(Of myComboBoxItem)
        Dim pdf As New myComboBoxItem
        Dim hinweis As String
        l(" getAllPDFFiles4GUID  " & object_guid)
        Try
            'fstREC.mydb.SQL = "SELECT *  FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz] where kategorie_guid='" & kategorie_guid_Bplaene &
            '    "' and ident =" & bplanindex

            'select * from [LKOF_Bearb].[dbo].[tbl_mdat_dateien] where file_key like 'BPL4N%'
            ''                  and object_guid='de97242d-99c2-438b-a1f0-f76b56df9473' order by sys_stamp_in desc

            fstREC.mydb.SQL = "Select * From [LKOF_Bearb].[dbo].[tbl_mdat_dateien] Where  file_key like 'BPL4N%'  " &
                " and object_guid='" & object_guid & "'"
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return pdfliste 'darf nicht vorkommen
            Else
                For i = 0 To fstREC.dt.Rows.Count - 1
                    pdf = New myComboBoxItem
                    pdf.myindex = (clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("file_key")).ToString)
                    pdf.myindex = quellpfad & pdf.myindex
                    pdf.mySttring = clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("file_name")).ToString
                    pdfliste.Add(pdf)
                Next
                Return pdfliste
            End If
            Return pdfliste

        Catch ex As Exception
            l("fehler in getAllPDFFiles4GUID-- " & ex.ToString)
            Return pdfliste
        End Try
    End Function

    Friend Function erzeugeCSVDateiBestand(csvdatei As String) As Boolean
        Dim dt As DataTable
        Dim hinweis As String = ""
        Dim sw As IO.StreamWriter

        Dim sb As New Text.StringBuilder
        Dim t As String = ";"
        Dim fi As IO.FileInfo
        Dim fo As IO.FileInfo
        Try
            sw = New IO.StreamWriter(csvdatei)
            sw.AutoFlush = True
            sb.Append("Gemeinde" & t)
            sb.Append("Gemarkung" & t)
            sb.Append("BlattNr" & t)
            sb.Append("laufnr" & t)
            sb.Append("Kennz" & t)
            sb.Append("flur" & t)
            sb.Append("zaehler" & t)
            sb.Append("nenner")

            sw.WriteLine(sb.ToString)


            fstREC.mydb.SQL = "SELECT * FROM [LKOF_Bearb].[dbo].[tbl_mdat_datensatz]" &
                        " where kategorie_guid='" & kategorie_guid_Baulasten & "' " &
                        "  order by text7, text8, text3, text2,text1,int1,int2,int3"
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return False 'darf nicht vorkommen
            Else

                For i = 0 To fstREC.dt.Rows.Count - 1
                    sb = New Text.StringBuilder

                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text7")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text8")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text3")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text2")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text1")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("int1")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("int2")).ToString) & t)
                    sb.Append((clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("int3")).ToString))
                    sw.WriteLine(sb.ToString)
                    sb.Clear()
                Next
                'For i = 0 To fstREC.dt.Rows.Count - 1
                '    Dim datei = srv_unc_path & "BAUL4ST_" & clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text3")).ToString & ".pdf"
                '    fi = New IO.FileInfo(datei)
                '    Dim quelldatei As String

                '    If Not fi.Exists Then
                '        If clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text8")).ToString.ToLower = String.Empty Then
                '        Else
                '            quelldatei = "L:\fkat\baulasten\" & clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text8")).ToString.ToLower
                '            quelldatei = quelldatei & "\" & clsDBtools.fieldvalue(fstREC.dt.Rows(i).Item("text3")).ToString & ".pdf"
                '            fo = New IO.FileInfo(quelldatei)
                '            If fo.Exists Then
                '                IO.File.Copy(quelldatei, datei)
                '            Else
                '            End If
                '            sw.WriteLine("fehlt" & datei)
                '        End If


                '    End If
                'Next
            End If
            sw.Close()
            sw.Dispose()
            Return True
        Catch ex As Exception
            l("fehler in getAllPDFFiles4GUID-- " & ex.ToString)
            Return False
        End Try
    End Function

    Friend Function getgemarkungsindex(gemarkungstext As String) As Integer
        Try
            'For Each gema As myComboBoxItem In katasterGemarkungslist
            '    gamarkungsitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex})
            'Next

            For i = 0 To katasterGemarkungslist.Count - 1
                If gemarkungstext.Trim.ToLower = katasterGemarkungslist(i).mySttring.ToLower Then
                    Return i
                End If
                'gamarkungsitems.Add(New myComboBoxItem With {.mySttring = gema.mySttring, .myindex = gema.myindex}) Then
            Next
            Return -2
        Catch ex As Exception
            l("fehler in getgemarkungsindex-- " & ex.ToString)
            Return -1
        End Try
    End Function
End Module
