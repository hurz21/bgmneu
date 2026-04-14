Imports System.Data.SqlClient
Imports System.Net.Mime.MediaTypeNames
Imports Microsoft.VisualBasic.Logging

Public Class probaug
    Public Shared gem(37) As String
    Public Shared katasterGem(35) As String
    Public Shared katasterGemarkungslist As New List(Of myComboBoxItem)
    Public Shared probaugGemarkungsdict As New Dictionary(Of Integer, String)
    Public Shared Function bildeFKZstring(fstliste As List(Of clsFlurstueck)) As String
        l("bildeFKZstring: ")
        Dim summe As String = ""
        Dim treffer As Integer = 0
        Try
            For i = 0 To fstliste.Count - 1
                If probaug.FlurstueckExistiertImINGRADAGis(fstliste(i).Flurstuecksskennzeichen, True) Then
                    treffer += 1
                    If treffer = 1 Then
                        summe = summe & fstliste(i).Flurstuecksskennzeichen
                    Else
                        summe = summe & "," & fstliste(i).Flurstuecksskennzeichen
                    End If
                Else

                End If

            Next
            Return summe
        Catch ex As Exception
            l("fehler inbildeFKZstring: " & ex.ToString)
            Return ""
        End Try
    End Function
    Public Shared Function klaereanzahlFST(jahr As String, vorgangsnummer As String, ByRef metadata As List(Of myComboBoxItem)) As List(Of clsFlurstueck)
        Dim fstliste As New List(Of clsFlurstueck)
        Dim dt As DataTable
        Dim sql = "" '"select * from GISVIEW6 where feld7='2026' and feld9='80006'"
        probaug.initProbaugNrProbaugGemarkungtext()
        probaug.initKatasterGemarkungtext()
        probaug.katasterGemarkungslist = probaug.splitKatasterGemarkung()
        probaug.probaugGemarkungsdict = probaug.splitgem()
        sql = "select * from GISVIEW1 where feld1='" & jahr.Trim & "' and feld3='" & vorgangsnummer.Trim & "'"
        'select * from GISVIEW6 where feld7='2026' and feld9='80006'
        l(sql)
        'Dim metad As New List(Of myComboBoxItem)
        Dim meta As New myComboBoxItem
        Try
            dt = probaug.getbalist2MSSQL(sql)
            metadata = probaug.dt2meta(dt)

            fstliste = probaug.dt2obj(dt)

            Return fstliste
        Catch ex As Exception
            l("fehler in    klaereanzahlFST   " & ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Shared Function dt2meta(dt As DataTable) As List(Of myComboBoxItem)
        l("in dt2meta: ")
        Dim test As Integer
        Dim meta As New myComboBoxItem
        Dim metaliste As New List(Of myComboBoxItem)
        Try
            For i = 0 To dt.Rows.Count - 1

                meta = New myComboBoxItem
                meta.myindex = "Vorhaben1"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD4")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorhaben2"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD5")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorhaben3"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD6")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorhaben4"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD7")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorhaben"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD8")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "verfahrensart"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD9")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorhabensmerkmal"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD10")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Bauort"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD11")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Gemarkung"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD12")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Flur"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD13")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Flurstück"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD14")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "anrede"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD15")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Titel"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD16")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Zusatz 1"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD17")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Zusatz 2"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD18")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Vorname"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD19")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Name"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD20")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Straße"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD21")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Hausnr"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD22")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "plz"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD23")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Ort"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD24")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Strasse"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD25")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "hausnr"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD26")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "BJVG.HSchl"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD27")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Gemarkungsbezeichnung"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD28")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Hochwert"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD29")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Rechtswert"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD30")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "zust. SB"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD31")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Ortsteil (KLAR)"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD32")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Ort (Katasterangaben KLAR)"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD33")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Ortsteil Antragsteller"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD34")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Kennziffer Verfahrensart"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD35")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Kennziffer Vorhaben"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD36")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Kennziffer Vorhaben-Merkmal"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD37")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "Gemarkung_Kataster"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD38")).Trim
                metaliste.Add(meta)
                meta = New myComboBoxItem
                meta.myindex = "OrderId"
                meta.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD39")).Trim
                metaliste.Add(meta)

                Exit For

            Next
            Return metaliste
        Catch ex As Exception
            l("Fehler in dt2obj: " & ex.ToString())
            Return Nothing
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
            'Dim host = "kh-w-sql02" : Dim schema = "Probaug" : Dim dbuser = "sgis" : Dim dbpw = "Grunt8-Cornhusk-Reporter"
            Dim host = "kh-w-sql02" : Dim schema = "prosozbau" : Dim dbuser = "sgis" : Dim dbpw = "Grunt8-Cornhusk-Reporter"
            Dim conbuil As New SqlClient.SqlConnectionStringBuilder
            Dim v = "Data Source=" & host & ";User ID=" & dbuser & ";Password=" & dbpw & ";" +
                "Initial Catalog=" & schema & ";"

            oOracleConn = New SqlClient.SqlConnection(v)

            'oOracleConn = New OracleConnection(OracleConnectionString)
            oOracleConn.Open()
            l("OracleConnection open")
            com = New SqlCommand(sql, oOracleConn) '"select * from " & tabname$
            Dim da As New SqlDataAdapter(com)
            'da.MissingSchemaAction = MissingSchemaAction.AddWithKey
            l("fill")
            'Console.WriteLine("vor fill")
            _mycount = da.Fill(dt)
            l("fillfertig: " & _mycount)
            l("in gisview2 wurden " & _mycount & " datensätze gefunden.=======================")
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
    Shared Function getVorhaben(az As String) As String
        l("in getVorhaben")
        l(az)
        Dim dt As DataTable
        Dim sql, achter, jahr, result As String
        '    sql = "select distinct * from " & quelleSQL & " where FELD1=" & baulastblattnr & " order by feld2"
        '"80006-2026"

        Try
            Dim jahr1 As String = ""
            Dim lfdnr As String = ""
            getVorgangsDupel(az, jahr1, lfdnr)
            sql = "select feld36 from GisView1  where FELD1='" & jahr1 & "' and feld3='" & lfdnr & "'"

            dt = getDATATAB(sql)
            Return dt.Rows(0).Item(0).ToString
            'Dim v2 = dt.Rows(1).Item(0)
            'Dim v3 = dt.Rows(0).Item(1)
            'Dim v4 = dt.Rows(1).Item(1)

        Catch ex As Exception
            l("getvorhaben " & az & " " & ex.ToString)
            Return "ErrorToString"
        End Try
    End Function

    Private Shared Sub getVorgangsDupel(az As String, ByRef jahr1 As String, ByRef lfdnr As String)
        Dim a As String()
        Try
            az = az.Trim
            a = az.Split("-"c)
            jahr1 = a(1).Trim
            lfdnr = a(0).Trim
        Catch ex As Exception
            l("getVorgangsDupel " & az & " " & ex.ToString)
            jahr1 = ""
            lfdnr = ""
        End Try
    End Sub

    Shared Function getDATATAB(sql As String) As DataTable
        Dim oOracleConn As SqlClient.SqlConnection
        Dim dt As System.Data.DataTable
        Dim com As SqlCommand
        Dim _mycount As Long
        dt = New DataTable
        Try
            l(" MOD getbalist2 anfang")
            'Dim host = "kh-w-sql02" : Dim schema = "Probaug" : Dim dbuser = "sgis" : Dim dbpw = "Grunt8-Cornhusk-Reporter"
            Dim host = "kh-w-sql02" : Dim schema = "prosozbau" : Dim dbuser = "sgis" : Dim dbpw = "Grunt8-Cornhusk-Reporter"
            Dim conbuil As New SqlClient.SqlConnectionStringBuilder
            Dim v = "Data Source=" & host & ";User ID=" & dbuser & ";Password=" & dbpw & ";" +
                "Initial Catalog=" & schema & ";"

            oOracleConn = New SqlClient.SqlConnection(v)

            'oOracleConn = New OracleConnection(OracleConnectionString)
            oOracleConn.Open()
            l("OracleConnection open")
            com = New SqlCommand(sql, oOracleConn) '"select * from " & tabname$
            Dim da As New SqlDataAdapter(com)
            'da.MissingSchemaAction = MissingSchemaAction.AddWithKey
            l("fill")
            'Console.WriteLine("vor fill")
            _mycount = da.Fill(dt)
            l("fillfertig: " & _mycount)
            l("in gisview2 wurden " & _mycount & " datensätze gefunden.=======================")
            oOracleConn.Close()
            com.Dispose()
            da.Dispose()
            Return dt
            l(" MOD getbalist2 ende")
        Catch ex As Exception
            l("Fehler in getDATATAB: " & ex.ToString())
            Return dt
        End Try
    End Function

    Friend Shared Function getIstProumwelt(az As String) As Boolean
        Dim a As String()
        l("getIstProumwelt " & az)
        Try
            az = az.Trim
            a = az.Split("-"c)
            l("a(0): " & a(0))
            If a(0).Count = 5 And a(0).StartsWith("8") Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            l("Fehler in getIstProumwelt: " & ex.ToString())
            Return False
        End Try
    End Function

    Friend Shared Function FlurstueckExistiertImINGRADAGis(ByRef fst As String, mitnenner As Boolean) As Boolean
        l("FlurstueckExistiertImINGRADAGis " & fst)
        Dim sql As String
        Dim dt As DataTable
        Dim myconn As New SqlConnection
        Dim myCmd As New SqlCommand
        Dim myReader As SqlDataReader
        Try
            If mitnenner Then
                sql = "SELECT  *  FROM   [LKOF].dbo.tbl_lieg_flurstueck AS f where flurstueckskennzeichen='" & fst & "'"
            Else
                sql = "SELECT  flurstueckskennzeichen  FROM   [LKOF].dbo.tbl_lieg_flurstueck AS f where flurstueckskennzeichen like '" & fst & "'"
            End If
            Dim cstring As String
            cstring = "Server=KH-W-INGRADA;Database=LKOF;User=Ingrada;Pwd=Starry-Footless6-Mashing-Backboned;"
            myconn = New SqlConnection(cstring)
            myCmd = myconn.CreateCommand
            myCmd.CommandText = sql
            l("vor open " & sql)
            myconn.Open()
            myReader = myCmd.ExecuteReader()
            If myReader.HasRows Then
                If mitnenner Then
                Else
                    l("vor read")
                    Do While myReader.Read()
                        fst = myReader.GetString(0)
                    Loop
                End If

                myReader.Close()
                myconn.Close()
                Return True
            Else
                fst = "fehler nicht gefunden"
                Return False
            End If
        Catch ex As Exception
            l("Fehler in getIstProumwelt: " & ex.ToString())
            Return False
        End Try
    End Function

    Friend Shared Sub spalteAZ(az As String, ByRef jahr As String, ByRef vorgangsnummer As String)
        Dim a() As String
        'az = "80010-2026"
        l("spalteAZ   " & az)
        Try
            az = az.Trim
            a = az.Split("-"c)
            vorgangsnummer = a(0)
            jahr = a(1)
        Catch ex As Exception
            l("Fehler in spalteAZ: " & ex.ToString())
        End Try
    End Sub

    Friend Shared Function dt2obj(dt As DataTable) As List(Of clsFlurstueck)
        l("in dt2obj: ")
        Dim probaugGemarkung As String
        Dim test As Integer
        Dim fst As New clsFlurstueck
        Dim fstliste As New List(Of clsFlurstueck)
        Try
            For i = 0 To dt.Rows.Count - 1
                fst = New clsFlurstueck
                probaugGemarkung = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD12")).Trim ' 
                ' ist int, z.b. 4 dazu nun der text
                test = CInt(probaugGemarkung)

                fst.gemarkungstext = probaugGemarkungsdict(test)
                If fst.gemarkungstext.Count < 3 Then
                    fst.gemarkungstext = (clsDBtools.fieldvalue(dt.Rows(i).Item("FELD28")).Trim)
                End If
                fst.gemcode = probaug.getKatasterGemarkung(fst.gemarkungstext, katasterGemarkungslist)
                fst.flur = CInt(clsDBtools.fieldvalue(dt.Rows(i).Item("FELD13")).Trim)
                fst.fstueckKombi = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD14")).Trim
                fst.GKhoch = CDbl(clsDBtools.fieldvalue(dt.Rows(i).Item("FELD29")).Trim)
                fst.GKrechts = CDbl(clsDBtools.fieldvalue(dt.Rows(i).Item("FELD30")).Trim)
                fst.Flurstuecksskennzeichen = fst.flurstueckZuFKZ
                fstliste.Add(fst)
            Next
            Return fstliste
        Catch ex As Exception
            l("Fehler in dt2obj: " & ex.ToString())
            Return Nothing
        End Try
    End Function
    Shared Sub initProbaugNrProbaugGemarkungtext()
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
    Shared Sub initKatasterGemarkungtext()
        katasterGem(0) = "Bieber                             ;725"
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
    Shared Function splitKatasterGemarkung() As List(Of myComboBoxItem)
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
    Shared Function splitgem() As Dictionary(Of Integer, String)
        Dim dict As New Dictionary(Of Integer, String)
        Dim a() As String
        For i = 0 To gem.Count - 1
            a = gem(i).Replace(vbTab, " ").Split(" "c)
            dict.Add(CInt(a(0).Trim), a(1).Trim)
        Next
        Return dict
    End Function


    Shared Function getKatasterGemarkung(gemarkungstext As String, katasterGemarkungslist As List(Of myComboBoxItem)) As Integer
        Try
            l("getKatasterGemarkung---------------------- anfang")
            For i = 0 To gem.Count - 1
                If (gemarkungstext).ToLower.Trim = katasterGemarkungslist(i).mySttring.ToLower.Trim Then
                    l("result:" & katasterGemarkungslist(i).myindex.Trim.ToLower)
                    Return (CInt(katasterGemarkungslist(i).myindex))

                End If
            Next

            l("probaugGemarkugnen ohne Kataster:" & gemarkungstext.Trim.ToLower)
            l("getKatasterGemarkung---------------------- ende")
            Return 0
        Catch ex As Exception
            l("Fehler in getKatasterGemarkung: " & ex.ToString())
            Return -1
        End Try
    End Function

    Friend Shared Function getVorgaengeZuFlurstueck(clsFlurstueck As clsFlurstueck) As List(Of myComboBoxItem)
        Dim sql, hinweis As String
        Dim dt As DataTable
        Dim cb As myComboBoxItem
        Dim vlist As New List(Of myComboBoxItem)
        sql = "select * from GISVIEW1 where feld28='" & clsFlurstueck.gemarkungstext &
                "' and feld13='" & clsFlurstueck.flur & "'" &
                " and feld14='" & clsFlurstueck.fstueckKombi & "' order by feld1 desc"
        'select * from GISVIEW6 where feld7='2026' and feld9='80006'
        l(sql)
        'Dim metad As New List(Of myComboBoxItem)

        Try
            dt = probaug.getbalist2MSSQL(sql)


            For i = 0 To dt.Rows.Count - 1
                cb = New myComboBoxItem
                cb.myindex = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD1")).Trim '
                cb.mySttring = clsDBtools.fieldvalue(dt.Rows(i).Item("FELD3")).Trim '
                vlist.Add(cb)
            Next
            Return vlist
        Catch ex As Exception
            l("fehler in getVorgaengeZuFlurstueck " & ex.ToString)
            Return vlist
        End Try
    End Function
End Class