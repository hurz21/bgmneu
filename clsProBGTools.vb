﻿Imports System.Data
Imports Devart.Common.Utils
Imports iTextSharp.text.rtf

Public Class clsProBGTools
    Shared Property ProbauGIstOracle As Boolean = False

    Public Shared Sub holeProBaugDaten(baulastblattnr As Integer, quelleSQL As String, ByRef abbruch As Boolean)
        Dim sql, sqlgeschlossen As String
        abbruch = False
        Try
            l(" MOD holeProBaugDaten anfang")
            '
            FSTausPROBAUGListe.Clear()
            'sql = getSQLProbaug(baulastblattnr)
            'sql = getSQLProbaugALt(baulastblattnr)
            sql = "select distinct * from " & quelleSQL & " where FELD1=" & baulastblattnr & " order by feld2"


            sql = "SELECT distinct DateAdded,DateDeleted,isdeleted, a2.* " &
                    "  FROM [prosozbau].[dbo].[PBSBaulastblattSerieTatbestand] c," &
                    "       [prosozbau].[dbo].[GISVIEW2Belastet] a2 " &
                     " where PBSBaulastblattSerie_Key1 " &
                    "  in (SELECT id   FROM [prosozbau].[dbo].[PBSBaulastblattSerie]  d " &
                     "            where   PBSBaulastblatt_Key1= " &
                     "        (SELECT  distinct id FROM [prosozbau].[dbo].[GISVIEW2Belastet] a,[PBSBaulastblatt] b " &
                     "                 where feld1=" & baulastblattnr & " and b.id=a.feld9 )" &
                     "    ) " &
                    "	 and feld1=" & baulastblattnr & "  and Nr=feld2 and IsDeleted=0 " &
                    " order by feld2"

            'sqlgeschlossen = "SELECT  feld3 from obj01bla "
            sqlgeschlossen = sql
            initBaulastBlattnr(sql, sqlgeschlossen) ' liefert balistDT1 und geschlossenDT as dt

            If rawList Is Nothing Then
                abbruch = True
                Exit Sub
            End If
            Debug.Print(rawList.Count.ToString)
            If rawList.Count < 1 Then
                MessageBox.Show("Probaug lieferte keine sauberen Daten zu BaulastBlattNr: " & baulastblattnr & ". Bitte zuerst auf ProbauG-Seite in Ordnung bringen.")
            Else
                l("vor schlkeife ")
                For i = 0 To rawList.Count - 1
                    rawList(i).katFST.gemarkungstext = rawList(i).katFST.gemparms.gemcode2gemarkungstext(rawList(i).katFST.gemcode)
                    rawList(i).katFST.fstueckKombi = rawList(i).katFST.buildFstueckkombi
                    rawList(i).katFST.gueltig = rawList(i).gueltig
                    rawList(i).katFST.gebucht = rawList(i).baulastnr
                    rawList(i).katFST.Prefix = rawList(i).Prefix
                    rawList(i).katFST.AzNr = rawList(i).AzNr
                    rawList(i).katFST.AzJahr = rawList(i).AzJahr
                    rawList(i).katFST.AzOG = rawList(i).AzOG
                    rawList(i).katFST.Kennziffer_1 = rawList(i).Kennziffer_1
                    rawList(i).katFST.Kennziffer_2 = rawList(i).Kennziffer_2
                    rawList(i).katFST.Kennziffer_3 = rawList(i).Kennziffer_3
                    rawList(i).katFST.Kennziffer_4 = rawList(i).Kennziffer_4
                    FSTausPROBAUGListe.Add(rawList(i).katFST)
                Next
            End If
            l(" MOD holeProBaugDaten ende")
        Catch ex As Exception
            l("Fehler in holeProBaugDaten: " & sql & ex.ToString())
        End Try
    End Sub

    'Private Shared Function getSQLProbaugALt(baulastblattnr As Integer) As String
    '    Dim sql As String = "select * from gisview2 order by feld9 desc"
    '    Try
    '        l(" MOD ---------------------- anfang")
    '        sql = "SELECT OBJ01BL.FELD4, OBJ01BL.FELD5, OBJ01BL.FELD9, " &
    '        "OBJ03.FELD3, OBJ03.FELD4, OBJ03.FELD5, OBJ03.FELD6, OBJ03.FELD7 ,OBJ01BL.FELD1, OBJVG.FELD1, OBJVG.FELD2, " &
    '        "OBJVG.FELD3, ALB.FELD5, ALB.FELD4 ,OBJ01BL.FELD3,OBJ01BL.FELD6 as krof2,OBJ01BL.FELD7 as krof2 " &
    '        "FROM OBJ01BL LEFT OUTER JOIN OBJVG ON OBJVG.FELD4 = OBJ01BL.FELD1,  OBJ03 LEFT OUTER JOIN ALB ON ALB.FELD1 = OBJ03.FELD4 AND ALB.FELD2 = OBJ03.FELD5 AND ALB.FELD3 = OBJ03.FELD6 WHERE OBJ01BL.FELD1 = OBJ03.FELD1"

    '        sql = "SELECT OBJ01BL.FELD4 as a1, OBJ01BL.FELD5 as a2, OBJ01BL.FELD9 as a3, OBJ03.FELD3 as a4, OBJ03.FELD4 as a5, " &
    '            "OBJ03.FELD5 as a6, OBJ03.FELD6 as a7, OBJ03.FELD7 as a8,OBJ01BL.FELD1 as a9, OBJVG.FELD1 as a10," &
    '            "OBJVG.FELD2 as a11, OBJVG.FELD3 as a12, ALB.FELD5 as a13, ALB.FELD4 as a14,OBJ01BL.FELD3 as a15,OBJ01BL.FELD6 as angelegt," &
    '            "OBJ01BL.FELD7 as loesch,OBJ01BL.FELD8 as beschr  " &
    '            "FROM OBJ01BL LEFT OUTER JOIN OBJVG ON OBJVG.FELD4 = OBJ01BL.FELD1, " &
    '            " OBJ03 LEFT OUTER JOIN ALB ON ALB.FELD1 = OBJ03.FELD4 AND ALB.FELD2 = OBJ03.FELD5 AND ALB.FELD3 = OBJ03.FELD6 " &
    '            "WHERE OBJ01BL.FELD1 = OBJ03.FELD1"

    '        'OBJ01BL.FELD7  is gelöscht datum
    '        'OBJ01BL.FELD4 is blnr
    '        sql = sql & " and    OBJ01BL.FELD7 =' '"
    '        '----------------------------
    '        'sql = sql & "   and OBJ01BL.FELD4 ='90764' "
    '        'sql = sql & "   and OBJ01BL.FELD4 ='90764' "
    '        'sql = sql & "   and OBJ01BL.FELD4 ='20937' " 'pose mehrfach
    '        'sql = sql & "   and OBJ01BL.FELD4 ='2026' " 'pose mehrfach
    '        'sql = sql & "   and OBJ01BL.FELD4 =3103  'and OBJ01BL.FELD7 =' '"
    '        'sql = "select * from gisview2  where feld10=2017  order by feld9 desc"
    '        sql = sql & "   and OBJ01BL.FELD4 ='" & baulastblattnr & "' "
    '        Return sql


    '        l(" MOD getSQLProbaug ende")
    '    Catch ex As Exception
    '        l("Fehler in getSQLProbaug: " & ex.ToString())
    '        Return ""
    '    End Try
    'End Function

    'Friend Shared Function getSQLProbaug(baulastblattnr As Integer) As String
    '    Dim sql As String = "select * from gisview2 order by feld9 desc"
    '    Try
    '        l(" MOD ---------------------- anfang")
    '        sql = "SELECT OBJ01BL.FELD4, OBJ01BL.FELD5, OBJ01BL.FELD9, " &
    '        "OBJ03.FELD3, OBJ03.FELD4, OBJ03.FELD5, OBJ03.FELD6, OBJ03.FELD7 ,OBJ01BL.FELD1, OBJVG.FELD1, OBJVG.FELD2, " &
    '        "OBJVG.FELD3, ALB.FELD5, ALB.FELD4 ,OBJ01BL.FELD3,OBJ01BL.FELD6 as krof2,OBJ01BL.FELD7 as krof2 " &
    '        "FROM OBJ01BL LEFT OUTER JOIN OBJVG ON OBJVG.FELD4 = OBJ01BL.FELD1,  OBJ03 LEFT OUTER JOIN ALB ON ALB.FELD1 = OBJ03.FELD4 AND ALB.FELD2 = OBJ03.FELD5 AND ALB.FELD3 = OBJ03.FELD6 WHERE OBJ01BL.FELD1 = OBJ03.FELD1"

    '        sql = "SELECT OBJ01BL.FELD4 as a1, OBJ01BL.FELD5 as a2, OBJ01BL.FELD9 as a3, OBJ03.FELD3 as a4, OBJ03.FELD4 as a5, " &
    '            "OBJ03.FELD5 as a6, OBJ03.FELD6 as a7, OBJ03.FELD7 as a8,OBJ01BL.FELD1 as a9, OBJVG.FELD1 as a10," &
    '            "OBJVG.FELD2 as a11, OBJVG.FELD3 as a12, ALB.FELD5 as a13, ALB.FELD4 as a14,OBJ01BL.FELD3 as a15,OBJ01BL.FELD6 as angelegt," &
    '            "OBJ01BL.FELD7 as loesch,OBJ01BL.FELD8 as beschr  " &
    '            "FROM OBJ01BL LEFT OUTER JOIN OBJVG ON OBJVG.FELD4 = OBJ01BL.FELD1, " &
    '            " OBJ03 LEFT OUTER JOIN ALB ON ALB.FELD1 = OBJ03.FELD4 AND ALB.FELD2 = OBJ03.FELD5 AND ALB.FELD3 = OBJ03.FELD6 " &
    '            "WHERE OBJ01BL.FELD1 = OBJ03.FELD1"

    '        'OBJ01BL.FELD7  is gelöscht datum
    '        'OBJ01BL.FELD4 is blnr
    '        sql = sql & " and    OBJ01BL.FELD7 =' '"
    '        '----------------------------
    '        'sql = sql & "   and OBJ01BL.FELD4 ='90764' "
    '        'sql = sql & "   and OBJ01BL.FELD4 ='90764' "
    '        'sql = sql & "   and OBJ01BL.FELD4 ='20937' " 'pose mehrfach
    '        'sql = sql & "   and OBJ01BL.FELD4 ='2026' " 'pose mehrfach
    '        'sql = sql & "   and OBJ01BL.FELD4 =3103  'and OBJ01BL.FELD7 =' '"
    '        'sql = "select * from gisview2  where feld10=2017  order by feld9 desc"
    '        sql = sql & "   and OBJ01BL.FELD4 ='" & baulastblattnr & "' "
    '        Return sql


    '        l(" MOD getSQLProbaug ende")
    '    Catch ex As Exception
    '        l("Fehler in getSQLProbaug: " & ex.ToString())
    '        Return ""
    '    End Try
    'End Function
    Shared Function getGISVIEW2(sql As String) As DataTable

        '    Dim sql = "select * from gisview1 order by feld9 desc"
        Dim geschlossenDT As DataTable

        If ProbauGIstOracle Then
            'geschlossenDT = getbalist2Oracle(sql)
        Else
            geschlossenDT = getbalist2MSSQL(sql)
        End If
        Return geschlossenDT
    End Function
    Friend Shared Function initBaulastBlattnr(sql As String, sqlgeschlossen As String) As String

        'order nach laufnr
        Dim balistDT1 As System.Data.DataTable
        Dim geschlossenDT As System.Data.DataTable
        Try
            l(" MOD initBaulastBlattnr anfang")
            'checkTiffs()
            ___showdispatcher(sql & Environment.NewLine)
            initProbaugNrProbaugGemarkungtext() : initgemeinde()
            initKatasterGemarkungtext()
            katasterGemarkungslist = splitKatasterGemarkung()
            probaugGemarkungsdict = splitgem()
            gemeindedict = splitgemeinde()
            ___showdispatcher("gemeinde verzeichnis erstellt" & Environment.NewLine)


            l(" MOD initBaulastBlattnr 1")
            ___showdispatcher("baulasten einlesen " & Environment.NewLine)
            If ProbauGIstOracle Then
                'balistDT1 = getbalist2Oracle(sql)
            Else
                balistDT1 = getbalist2MSSQL(sql)
            End If
            l("SQL: " & sql)
            l(" MOD initBaulastBlattnr 2")
            'geschlossenDT = getbalist2Oracle(sqlgeschlossen)

            'If ProbauGIstOracle Then
            '    'geschlossenDT = getbalist2Oracle(sqlgeschlossen)
            'Else
            '    geschlossenDT = getbalist2MSSQL(sqlgeschlossen)
            'End If

            l(" MOD initBaulastBlattnr 3")
            '___showdispatcher("baulasten geschlossenDT: " & geschlossenDT.Rows.Count & Environment.NewLine)


            ___showdispatcher("datentabelle " & balistDT1.Rows.Count & " baulasten eingelesen" & Environment.NewLine)
            ___showdispatcher("baulasten liste erstellen ")

            rawList = dtnachobj(balistDT1, geschlossenDT)
            If rawList Is Nothing Then
                MessageBox.Show("Die Probaug Daten dieser Baulast " & "sind nicht in Ordnung. " & vbCrLf &
                    "  " & vbCrLf &
                    "  " & vbCrLf &
                   "Anzahl Teilflächen: " & balistDT1.Rows.Count & vbCrLf &
                                "Abbruch!!!", "Fehler in ProbauG-Daten", MessageBoxButton.OK, MessageBoxImage.Error)
                Return Nothing
            End If
            l(" MOD initBaulastBlattnr 4")
            ___showdispatcher(" - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("baulasten liste jetzt erweitern ... ")
            ___showdispatcher("")
            l(" MOD initBaulastBlattnr 5")
            objErweitern(rawList, anzahltiff, anzahl_dateiexitiert, anzahl_blattNrIst0) 'balist
            ___showdispatcher("prüfen ob katasterdaten Ok " & Environment.NewLine)
            l(" MOD initBaulastBlattnr 6")
            istKatasterFormellOK(rawList, anzahlKatasterFormellOK)
            ___showdispatcher("prüfen ob katasterdaten Ok  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("Liste der als gelöscht markierten Objekte bilden" & Environment.NewLine)
            l(" MOD initBaulastBlattnr 7")
            'list4Geloscht = tools.bildeGeloeschteListe(rawList, anzahlGeloschte)

            ___showdispatcher("Liste der als gelöscht markierten Objekte  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("Alle als gelöscht markierten objekte löschen" & Environment.NewLine)
            l(" MOD initBaulastBlattnr 8")
            viererLoeschen(vierergeloescht)
            ___showdispatcher("Alle als gelöscht markierten  Objekte löschen - abgeschlossen " & Environment.NewLine)
            Dim katnichtOKAberMitTiff_summe As String
            ___showdispatcher("Prüfen ob Baulasten mit Tiff aber ohne Katasterangaben " & Environment.NewLine)
            l(" MOD initBaulastBlattnr 9")
            istKatnichtOKaberTiffVorhanden(rawList, katnichtOKAberMitTiff_summe)

            ___showdispatcher("Prüfen ob Baulasten mit Tiff aber ohne Katasterangaben  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("baulasten liste jetzt erweitern - abgeschlossen " & Environment.NewLine)
            ' showdispatcher("    mit Tiff-Datei: " & anzahltiff)
            ___showdispatcher("   Tiff-Datei existiert: " & anzahl_dateiexitiert & Environment.NewLine)
            ___showdispatcher("   BlattNr = 0: " & anzahl_blattNrIst0 & Environment.NewLine)
            ___showdispatcher("   KatasterOK: " & anzahlKatasterFormellOK & Environment.NewLine)
            ___showdispatcher("   katnichtOKAberMitTiff_summe: " & Environment.NewLine & katnichtOKAberMitTiff_summe & Environment.NewLine)
            ___showdispatcher("   anzahlGeloschtMarkiert: " & anzahlGeloschte & Environment.NewLine)
            ___showdispatcher("   real gelöscht: " & vierergeloescht & Environment.NewLine)
            l(" MOD initBaulastBlattnr fertig")
            l(" MOD initBaulastBlattnr ende")
            Return sqlgeschlossen
        Catch ex As Exception
            l("Fehler in initBaulastBlattnr: " & ex.ToString())
        End Try
    End Function
    Friend Shared Function initBaulastBlattnr2(sql As String, sqlgeschlossen As String) As String

        'order nach laufnr
        Dim balistDT1 As System.Data.DataTable
        Dim geschlossenDT As System.Data.DataTable
        Try
            l(" MOD initBaulastBlattnr anfang")
            'checkTiffs()
            ___showdispatcher(sql & Environment.NewLine)
            initProbaugNrProbaugGemarkungtext() : initgemeinde()
            initKatasterGemarkungtext()
            katasterGemarkungslist = splitKatasterGemarkung()
            probaugGemarkungsdict = splitgem()
            gemeindedict = splitgemeinde()
            ___showdispatcher("gemeinde verzeichnis erstellt" & Environment.NewLine)


            l(" MOD initBaulastBlattnr 1")
            ___showdispatcher("baulasten einlesen " & Environment.NewLine)
            If ProbauGIstOracle Then
                'balistDT1 = getbalist2Oracle(sql)
            Else
                balistDT1 = getbalist2MSSQL(sql)
            End If
            l(" MOD initBaulastBlattnr 2")
            'geschlossenDT = getbalist2Oracle(sqlgeschlossen)

            'If ProbauGIstOracle Then
            '    'geschlossenDT = getbalist2Oracle(sqlgeschlossen)
            'Else
            '    geschlossenDT = getbalist2MSSQL(sqlgeschlossen)
            'End If

            l(" MOD initBaulastBlattnr 3")
            '___showdispatcher("baulasten geschlossenDT: " & geschlossenDT.Rows.Count & Environment.NewLine)


            ___showdispatcher("datentabelle " & balistDT1.Rows.Count & " baulasten eingelesen" & Environment.NewLine)
            ___showdispatcher("baulasten liste erstellen ")

            rawList = dtnachobj2(balistDT1, geschlossenDT)
            l(" MOD initBaulastBlattnr 4")
            ___showdispatcher(" - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("baulasten liste jetzt erweitern ... ")
            ___showdispatcher("")
            l(" MOD initBaulastBlattnr 5")
            objErweitern(rawList, anzahltiff, anzahl_dateiexitiert, anzahl_blattNrIst0) 'balist
            ___showdispatcher("prüfen ob katasterdaten Ok " & Environment.NewLine)
            l(" MOD initBaulastBlattnr 6")
            istKatasterFormellOK(rawList, anzahlKatasterFormellOK)
            ___showdispatcher("prüfen ob katasterdaten Ok  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("Liste der als gelöscht markierten Objekte bilden" & Environment.NewLine)
            l(" MOD initBaulastBlattnr 7")
            'list4Geloscht = tools.bildeGeloeschteListe(rawList, anzahlGeloschte)

            ___showdispatcher("Liste der als gelöscht markierten Objekte  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("Alle als gelöscht markierten objekte löschen" & Environment.NewLine)
            l(" MOD initBaulastBlattnr 8")
            viererLoeschen(vierergeloescht)
            ___showdispatcher("Alle als gelöscht markierten  Objekte löschen - abgeschlossen " & Environment.NewLine)
            Dim katnichtOKAberMitTiff_summe As String
            ___showdispatcher("Prüfen ob Baulasten mit Tiff aber ohne Katasterangaben " & Environment.NewLine)
            l(" MOD initBaulastBlattnr 9")
            istKatnichtOKaberTiffVorhanden(rawList, katnichtOKAberMitTiff_summe)

            ___showdispatcher("Prüfen ob Baulasten mit Tiff aber ohne Katasterangaben  - abgeschlossen" & Environment.NewLine)
            ___showdispatcher("baulasten liste jetzt erweitern - abgeschlossen " & Environment.NewLine)
            ' showdispatcher("    mit Tiff-Datei: " & anzahltiff)
            ___showdispatcher("   Tiff-Datei existiert: " & anzahl_dateiexitiert & Environment.NewLine)
            ___showdispatcher("   BlattNr = 0: " & anzahl_blattNrIst0 & Environment.NewLine)
            ___showdispatcher("   KatasterOK: " & anzahlKatasterFormellOK & Environment.NewLine)
            ___showdispatcher("   katnichtOKAberMitTiff_summe: " & Environment.NewLine & katnichtOKAberMitTiff_summe & Environment.NewLine)
            ___showdispatcher("   anzahlGeloschtMarkiert: " & anzahlGeloschte & Environment.NewLine)
            ___showdispatcher("   real gelöscht: " & vierergeloescht & Environment.NewLine)
            l(" MOD initBaulastBlattnr fertig")
            l(" MOD initBaulastBlattnr ende")
            Return sqlgeschlossen
        Catch ex As Exception
            l("Fehler in initBaulastBlattnr: " & ex.ToString())
        End Try
    End Function
    Shared Function getbalist2MSSQL(sql As String) As DataTable
        Dim oOracleConn As SqlClient.SqlConnection
        Dim dt As System.Data.DataTable
        Dim com As SqlCommand
        Dim _mycount As Long
        dt = New DataTable
        Try
            l(" MOD getbalist2 anfang")
            'Dim host = "kh-w-sql02" : Dim schema = "Probaug" : Dim dbuser = "sgis" : Dim dbpw = " WinterErschranzt.74"
            Dim host = "kh-w-sql02" : Dim schema = "prosozbau" : Dim dbuser = "sgis" : Dim dbpw = " WinterErschranzt.74"
            Dim conbuil As New SqlClient.SqlConnectionStringBuilder
            Dim v = "Data Source=" & host & ";User ID=" & dbuser & ";Password=" & dbpw & ";" +
                "Initial Catalog=" & schema & ";"

            oOracleConn = New SqlClient.SqlConnection(v)

            'oOracleConn = New OracleConnection(OracleConnectionString)
            oOracleConn.Open()
            nachricht("OracleConnection open")
            com = New SqlCommand(sql, oOracleConn) '"select * from " & tabname$
            Dim da As New SqlDataAdapter(com)
            'da.MissingSchemaAction = MissingSchemaAction.AddWithKey
            nachricht("fill")
            Console.WriteLine("vor fill")
            _mycount = da.Fill(dt)
            nachricht("fillfertig: " & _mycount)
            nachricht("in gisview2 wurden " & _mycount & " datensätze gefunden.=======================")
            oOracleConn.Close()
            com.Dispose()
            da.Dispose()
            Return dt
            l(" MOD getbalist2 ende")
        Catch ex As Exception
            l("Fehler in getbalist2: " & ex.ToString())
            Return dt
        End Try
    End Function

    Private Shared Sub ___showdispatcher(v As String)
        nachricht(v)
    End Sub

    Friend Shared Sub holeAlleBeguenstigten()
        ' get alle begünstigten

        ' schleife
        '     objekte bilden
        '     write
        Dim targetGISTabelle = "baul_guen_f"
        Dim sql, sqlgeschlossen As String
        Try
            l(" MOD holeProBaugDaten anfang")
            '
            FSTausPROBAUGListe.Clear()
            'sql = getSQLProbaug(baulastblattnr)
            'sql = getSQLProbaugALt(baulastblattnr)
            sql = "select distinct * from gisview2 " '& quelleSQL '& " where FELD1=" & baulastblattnr & " order by feld2"
            'sqlgeschlossen = "SELECT  feld3 from obj01bla "

            'Dim baulastblattnr As String = "111458"

            'sql = "SELECT distinct DateAdded,DateDeleted,isdeleted, a2.* " &
            '        "  FROM [prosozbau].[dbo].[PBSBaulastblattSerieTatbestand] c," &
            '        "       [prosozbau].[dbo].[GISVIEW2] a2 " &
            '         " where PBSBaulastblattSerie_Key1 " &
            '        "  in (SELECT id   FROM [prosozbau].[dbo].[PBSBaulastblattSerie]  d " &
            '         "            where   PBSBaulastblatt_Key1= " &
            '         "        (SELECT  distinct id FROM [prosozbau].[dbo].[GISVIEW2] a,[PBSBaulastblatt] b " &
            '         "                 where feld1=" & baulastblattnr & " and b.id=a.feld9 )" &
            '         "    ) " &
            '        "	 and feld1=" & baulastblattnr & "  and Nr=feld2 and IsDeleted=0 " &
            '        " order by feld2"
            ' funzt




            sqlgeschlossen = sql
            initBaulastBlattnr2(sql, sqlgeschlossen) ' liefert balistDT1 und geschlossenDT as dt





            Debug.Print(rawList.Count.ToString)
            If rawList.Count < 1 Then
                'MessageBox.Show("Probaug lieferte keine sauberen Daten zu BaulastBlattNr: " & baulastblattnr & ". Bitte zuerst auf ProbauG-Seite in Ordnung bringen.")
            Else
                l("vor schlkeife ")
                For i = 0 To rawList.Count - 1
                    rawList(i).katFST.gemarkungstext = rawList(i).katFST.gemparms.gemcode2gemarkungstext(rawList(i).katFST.gemcode)
                    rawList(i).katFST.fstueckKombi = rawList(i).katFST.buildFstueckkombi
                    rawList(i).katFST.gueltig = rawList(i).gueltig
                    rawList(i).katFST.gebucht = rawList(i).baulastnr
                    rawList(i).katFST.Prefix = rawList(i).Prefix
                    rawList(i).katFST.AzNr = rawList(i).AzNr
                    rawList(i).katFST.AzJahr = rawList(i).AzJahr
                    rawList(i).katFST.AzOG = rawList(i).AzOG
                    rawList(i).katFST.Kennziffer_1 = rawList(i).Kennziffer_1
                    rawList(i).katFST.Kennziffer_2 = rawList(i).Kennziffer_2
                    rawList(i).katFST.Kennziffer_3 = rawList(i).Kennziffer_3
                    rawList(i).katFST.Kennziffer_4 = rawList(i).Kennziffer_4
                    FSTausPROBAUGListe.Add(rawList(i).katFST)
                Next
            End If
            l(" MOD holeProBaugDaten ende")
        Catch ex As Exception
            l("Fehler in holeProBaugDaten: " & sql & ex.ToString())
        End Try
    End Sub

    'Friend Shared Sub holeProBaugDatenZusatz(baulastblattnr As Integer, quelleSQL As String)
    '    Dim sql, sqlgeschlossen As String
    '    Try
    '        l(" MOD holeProBaugDaten anfang")
    '        '
    '        '     FSTausPROBAUGListe.Clear()
    '        'sql = getSQLProbaug(baulastblattnr)
    '        'sql = getSQLProbaugALt(baulastblattnr)
    '        sql = "select distinct * from " & quelleSQL & " where FELD1=" & baulastblattnr & " order by feld2"


    '        sql = "SELECT DateAdded,DateDeleted,IsDeleted,Nr " &
    '                "  From [prosozbau].[dbo].[PBSBaulastblattSerieTatbestand] " &
    '                " Where PBSBaulastblattSerie_Key1 in (SELECT id" &
    '                " From [prosozbau].[dbo].[PBSBaulastblattSerie] " &
    '                "   where   PBSBaulastblatt_Key1= " &
    '                "  (SELECT  distinct id FROM [prosozbau].[dbo].[" & quelleSQL & "] a,[PBSBaulastblatt] b " &
    '                " where feld1 = " & baulastblattnr & " And b.id = a.feld9 ) " &
    '                " )   order by nr"









    '        'sqlgeschlossen = "SELECT  feld3 from obj01bla "
    '        sqlgeschlossen = sql
    '        initBaulastBlattnr(sql, sqlgeschlossen) ' liefert balistDT1 und geschlossenDT as dt
    '        Debug.Print(rawList.Count.ToString)
    '        If rawList.Count < 1 Then
    '            MessageBox.Show("Probaug lieferte keine sauberen Daten zu BaulastBlattNr: " & baulastblattnr & ". Bitte zuerst auf ProbauG-Seite in Ordnung bringen.")
    '        Else
    '            l("vor schlkeife ")
    '            For i = 0 To rawList.Count - 1
    '                rawList(i).katFST.gemarkungstext = rawList(i).katFST.gemparms.gemcode2gemarkungstext(rawList(i).katFST.gemcode)
    '                rawList(i).katFST.fstueckKombi = rawList(i).katFST.buildFstueckkombi
    '                rawList(i).katFST.gueltig = rawList(i).gueltig
    '                rawList(i).katFST.gebucht = rawList(i).baulastnr
    '                rawList(i).katFST.Prefix = rawList(i).Prefix
    '                rawList(i).katFST.AzNr = rawList(i).AzNr
    '                rawList(i).katFST.AzJahr = rawList(i).AzJahr
    '                rawList(i).katFST.AzOG = rawList(i).AzOG
    '                rawList(i).katFST.Kennziffer_1 = rawList(i).Kennziffer_1
    '                rawList(i).katFST.Kennziffer_2 = rawList(i).Kennziffer_2
    '                rawList(i).katFST.Kennziffer_3 = rawList(i).Kennziffer_3
    '                rawList(i).katFST.Kennziffer_4 = rawList(i).Kennziffer_4
    '                FSTausPROBAUGListe.Add(rawList(i).katFST)
    '            Next
    '        End If
    '        l(" MOD holeProBaugDaten ende")
    '    Catch ex As Exception
    '        l("Fehler in holeProBaugDaten: " & sql & ex.ToString())
    '    End Try
    'End Sub
End Class
