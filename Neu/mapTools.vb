Imports DocumentFormat.OpenXml.Drawing
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class mapTools
    Public Shared Property BULKeigentuemerliste As List(Of myComboBoxItem)
    Public Shared Property BULKfst2nameList As List(Of clsFlurstueck)

    Shared Function init_katastergemeindeliste() As String()
        'SELECT  distinct gemeinde,gemeindeschluessel  FROM   dbo.tbl_lieg_flurstueck AS f LEFT OUTER JOIN       dbo.tbl_reg_gemeinde AS g ON f.gemeinde_gemeindeschluessel = g.gemeindeschluessel

        Dim a() As String
        ReDim a(12)
        Try
            a(0) = "Dietzenbach	;06438001"
            a(1) = "Dreieich	;06438002"
            a(2) = "Egelsbach	;06438003"
            a(3) = "Hainburg	;06438004"
            a(4) = "Heusenstamm	;06438005"
            a(5) = "Langen (Hessen)	;06438006"
            a(6) = "Mainhausen	;06438007"
            a(7) = "Mühlheim am Main	;06438008"
            a(8) = "Neu-Isenburg	;06438009"
            a(9) = "Obertshausen	;06438010"
            a(10) = "Rödermark	;06438012"
            a(11) = "Rodgau	;06438011"
            a(12) = "Seligenstadt	;06438013"
            Return a
        Catch ex As Exception
            l("init_gemeindeliste " & ex.ToString)
            Return a
        End Try
    End Function

    Friend Shared Function splitgemeindeliste(kataster As String()) As List(Of myComboBoxItem)
        Dim dict As New List(Of myComboBoxItem)
        Dim a() As String
        Dim my As New myComboBoxItem
        Try
            For i = 0 To kataster.Count - 1
                my = New myComboBoxItem
                a = kataster(i).Replace(vbTab, " ").Split(";"c)
                my.myindex = a(1).Trim
                my.mySttring = (a(0).Trim)
                dict.Add(my)
            Next
            Return dict
        Catch ex As Exception
            l("init_gemeindeliste " & ex.ToString)
            Return Nothing
        End Try
    End Function

    Friend Shared Function getBULKeigentuemervorschlaege(name As String, vergleichname As String,
                                                         vname As String, vergleichvname As String) As List(Of myComboBoxItem)
        'SELECT  g.flurstueckskennzeichen,[Anrede],[akademischegrade],[name],[vorname],[wohnortstrasse],[wohnortplz], [wohnort],[geburtsdatum],[namenszusatz],[postfach],[wohnortland],[eigentuemerzusatz],[geburtsname], [nationalitaet],[adressherkunft],[wohnortortsteil],[postfachplz]     
        ' From [LKOF].[dbo].[VW_lieg_eigentuemerGST_web] g, [LKOF].[dbo].[VW_lieg_eigentuemer_web] p   
        ' Where p.guid = g.person_guid And p.name Like 'bürger%'  
        Dim aliste As New List(Of myComboBoxItem)
        Dim liste As New List(Of myComboBoxItem)
        Dim bpl As myComboBoxItem
        Dim hinweis As String = ""
        Dim spalten = " distinct  [akademischegrade],[name],[vorname],[wohnortstrasse],[wohnortplz], [wohnort],[geburtsdatum],[namenszusatz],[postfach],[wohnortland],[eigentuemerzusatz],[geburtsname], [nationalitaet],[adressherkunft],[wohnortortsteil],[postfachplz]"
        Try
            l(" MOD getBULKeigentuemervorschlaege anfang")
            If vname = String.Empty Then
                'ohne vname
                fstREC.mydb.SQL = "SELECT  " & spalten & " FROM   [LKOF].[dbo].[VW_lieg_eigentuemer_web] p  " &
                                  "          where  p.name " & vergleichname & " '" & name & "'  " &
                              "    order by name,vorname,wohnortstrasse"
            Else
                'mitvname
                fstREC.mydb.SQL = "SELECT  " & spalten & " FROM   [LKOF].[dbo].[VW_lieg_eigentuemer_web] p  " &
                              "          where  p.name " & vergleichname & " '" & name & "'  " &
                              "          and    p.vorname " & vergleichvname & " '" & vname & "'  " &
                              "    order by name,vorname,wohnortstrasse"
            End If
            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return liste
            Else
                Dim gebdatum, gebname As String

                For i = 0 To fstREC.dt.Rows.Count - 1
                    bpl = New myComboBoxItem
                    'bpl.myindex = fstREC.dt.Rows(i).Item("guid").ToString
                    bpl.myindex = fstREC.dt.Rows(i).Item("name").ToString & "#" &
                                    fstREC.dt.Rows(i).Item("vorname").ToString & "#" &
                                    fstREC.dt.Rows(i).Item("wohnortstrasse").ToString
                    If fstREC.dt.Rows(i).Item("geburtsDatum").ToString = String.Empty Then
                        gebdatum = ""
                    Else
                        gebdatum = clsDBtools.fieldvalueDate(fstREC.dt.Rows(i).Item("geburtsDatum")).ToShortDateString
                    End If
                    If fstREC.dt.Rows(i).Item("geburtsName").ToString = String.Empty Then
                        gebname = ""
                    Else
                        gebname = ", geb. " & fstREC.dt.Rows(i).Item("geburtsName").ToString
                    End If
                    bpl.mySttring = 'fstREC.dt.Rows(i).Item("anrede").ToString & " " &
                                    fstREC.dt.Rows(i).Item("name").ToString & " " &
                                    fstREC.dt.Rows(i).Item("vorname").ToString & ", " &
                                    fstREC.dt.Rows(i).Item("wohnortstrasse").ToString & " " &
                                    fstREC.dt.Rows(i).Item("wohnortplz").ToString & " " &
                                    fstREC.dt.Rows(i).Item("wohnort").ToString & ", " &
                                    gebdatum &
                                    gebname
                    liste.Add(bpl)
                Next
                Return liste
            End If
        Catch ex As Exception
            l("fehler in erzeugeFlurliste-- " & ex.ToString)
            Return liste
        End Try
    End Function

    Friend Shared Function getFST4nameVname(nachname As String, vorname As String, wohnortstrasse As String) As List(Of clsFlurstueck)
        'SELECT  g.flurstueckskennzeichen,[Anrede],[akademischegrade],[name],[vorname],[wohnortstrasse],[wohnortplz], [wohnort],[geburtsdatum],[namenszusatz],[postfach],[wohnortland],[eigentuemerzusatz],[geburtsname], [nationalitaet],[adressherkunft],[wohnortortsteil],[postfachplz]     
        ' From [LKOF].[dbo].[VW_lieg_eigentuemerGST_web] g, [LKOF].[dbo].[VW_lieg_eigentuemer_web] p   
        ' Where p.guid = g.person_guid And p.name Like 'bürger%'  
        Dim aliste As New List(Of clsFlurstueck)
        Dim liste As New List(Of clsFlurstueck)
        Dim bpl As clsFlurstueck
        Dim personensql As String
        Dim hinweis As String = ""
        Dim spalten = " distinct  [akademischegrade],[name],[vorname],[wohnortstrasse],[wohnortplz], [wohnort],[geburtsdatum],[namenszusatz],[postfach],[wohnortland],[eigentuemerzusatz],[geburtsname], [nationalitaet],[adressherkunft],[wohnortortsteil],[postfachplz]"
        spalten = " distinct g.flurstueckskennzeichen"
        Try
            l(" MOD getBULKeigentuemervorschlaege anfang")
            personensql = makesqlperson(nachname, vorname, wohnortstrasse)
            fstREC.mydb.SQL = "SELECT  " & spalten &
                              " FROM   [LKOF].[dbo].[VW_lieg_eigentuemerGST_web] g, [LKOF].[dbo].[VW_lieg_eigentuemer_web] p    " &
                              "          where p.guid = g.person_guid  " &
                    personensql &
                              "    order by g.flurstueckskennzeichen"

            '"          and p.name ='" & nachname & "'" &
            '        "          and p.vorname='" & vorname & "'" &
            '        "          and p.wohnortstrasse='" & wohnortstrasse & "'" &

            l(fstREC.mydb.SQL)
            hinweis = fstREC.getDataDT()
            If fstREC.dt.Rows.Count < 1 Then
                Return liste
            Else
                Dim gebdatum, gebname As String

                For i = 0 To fstREC.dt.Rows.Count - 1
                    bpl = New clsFlurstueck
                    bpl.Flurstuecksskennzeichen = fstREC.dt.Rows(i).Item("flurstueckskennzeichen").ToString
                    bpl.fkzzerlegen()
                    liste.Add(bpl)
                Next
                Return liste
            End If
            Return liste
        Catch ex As Exception
            l("fehler in erzeugeFlurliste-- " & ex.ToString)
            Return liste
        End Try
    End Function

    Private Shared Function makesqlperson(nachname As String, vorname As String, wohnortstrasse As String) As String
        Dim result, nachnamestring, vornamestring, wohnstrassestring As String
        ' "          and p.name ='" & nachname & "'" &
        result = ""
        Try
            If IsNothingOrEmpty(nachname) Then
                nachnamestring = " "
            Else
                nachnamestring = " and p.name ='" & nachname & "' "
            End If
            If IsNothingOrEmpty(vorname) Then
                vornamestring = " "
            Else
                vornamestring = " and p.vorname ='" & vorname & "' "
            End If
            If IsNothingOrEmpty(vorname) Then
                wohnortstrasse = " "
            Else
                wohnortstrasse = " and p.wohnortstrasse ='" & wohnortstrasse & "' "
            End If
            result = nachnamestring & vornamestring & wohnstrassestring
            Return result
        Catch ex As Exception
            Return result
        End Try
    End Function
End Class
