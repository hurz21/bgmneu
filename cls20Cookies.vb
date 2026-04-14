Public Class cls20Cookies
    Public Shared Function LadeFlurstuecke() As List(Of clsFlurstueck)
        Dim path = GetCookieFilePath("alle20FSTcookies.txt")
        Dim result As New List(Of clsFlurstueck)

        If IO.File.Exists(path) Then
            For Each line In IO.File.ReadAllLines(path)
                Dim parts = line.Split("|"c)
                If parts.Length = 6 Then
                    result.Add(New clsFlurstueck With {
                    .gemarkungstext = parts(0),
                    .flur = CInt(parts(1)),
                    .zaehler = CInt(parts(2)),
                    .nenner = CInt(parts(3)),
                    .index = CInt(parts(4)),
                    .AZ = parts(5)
                })
                End If
            Next
        End If

        Return result
    End Function
    Public Shared Function GetCookieFilePath(cookiename As String) As String
        Dim docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim folder = System.IO.Path.Combine(docPath, "bgm", "cookies")

        If Not IO.Directory.Exists(folder) Then
            IO.Directory.CreateDirectory(folder)
        End If

        Return IO.Path.Combine(folder, cookiename)
    End Function

    Public Shared Sub SpeichereFlurstueck(fst As clsFlurstueck)
        Dim path = GetCookieFilePath("alle20FSTcookies.txt")
        Dim liste As New List(Of String)

        ' Bestehende laden
        If IO.File.Exists(path) Then
            liste = IO.File.ReadAllLines(path).ToList()
        End If

        Dim neuerEintrag = $"{fst.gemarkungstext}|{fst.flur}|{fst.zaehler}|{fst.nenner}|{fst.index}|{fst.AZ}"

        ' Entferne vorhandenen gleichen Eintrag (Duplikate vermeiden)
        liste = liste.Where(Function(x) x <> neuerEintrag).ToList()

        ' Neuen oben einfügen
        liste.Insert(0, neuerEintrag)

        ' Auf 20 begrenzen
        liste = liste.Take(20).ToList()

        IO.File.WriteAllLines(path, liste)
    End Sub

    Public Shared Sub SpeichereAdresse(adr As clsAdress)
        Dim path = GetCookieFilePath("alle20ADRcookies.txt")
        Dim liste As New List(Of String)
        Try


            ' Bestehende laden
            If IO.File.Exists(path) Then
                liste = IO.File.ReadAllLines(path).ToList()
            End If

            Dim neuerEintrag = $"{adr.gemeindeName}|{adr.strasseName}  |{adr.fkz}|{adr.index}|{adr.AZ}"

            ' Entferne vorhandenen gleichen Eintrag (Duplikate vermeiden)
            liste = liste.Where(Function(x) x <> neuerEintrag).ToList()

            ' Neuen oben einfügen
            liste.Insert(0, neuerEintrag)

            ' Auf 20 begrenzen
            liste = liste.Take(20).ToList()

            IO.File.WriteAllLines(path, liste)
        Catch ex As Exception
            l("fehler in SpeichereAdresse" & ex.ToString)
        End Try
    End Sub
    Public Shared Function LadeAdressen() As List(Of clsAdress)
        Dim path = GetCookieFilePath("alle20adrcookies.txt")
        Dim result As New List(Of clsAdress)

        If IO.File.Exists(path) Then
            For Each line In IO.File.ReadAllLines(path)
                Dim parts = line.Split("|"c)
                If parts.Length = 5 Then
                    result.Add(New clsAdress With {
                    .gemeindeName = parts(0),
                    .strasseName = (parts(1)),
                    .fkz = (parts(2)),
                    .index = CInt(parts(3)),
                    .AZ = parts(4)
                })
                End If
            Next
        End If

        Return result
    End Function
End Class
