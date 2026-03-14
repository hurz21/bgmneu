Public Class mapTools

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

    'Friend Shared Function lageohneZahl(lageliste() As String) As String()
    '    Dim neu() As String
    '    Dim temp As String
    '    Dim a() As String
    '    l("lageohneZahl")
    '    Try
    '        ReDim neu(lageliste.Count - 1)
    '        For i = 0 To lageliste.Count - 1
    '            a = lageliste(i).Split(" "c)
    '            For j = 0 To a.Count - 1
    '                If 
    '                neu(i) =
    '            Next
    '        Next
    '    Catch ex As Exception
    '        l("lageohneZahl " & ex.ToString)
    '        Return Nothing
    '    End Try
    'End Function

    'Function getstrasse() As String
    '    Dim sql As String
    '    Try
    '        sql = "SELECT *  FROM   dbo.tbl_lieg_flurstueck AS f LEFT OUTER JOIN       dbo.tbl_reg_gemeinde AS g ON f.gemeinde_gemeindeschluessel = g.gemeindeschluessel " &
    '                "where lagebezeichnung is not null and lagebezeichnung like 'am r%' and gemeinde ='Dietzenbach'  " &
    '                "order by lagebezeichnung"
    '    Catch ex As Exception

    '    End Try
    'End Function







End Class
