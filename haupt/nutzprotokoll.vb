Public Class nutzprotokoll


    Public Shared Sub NutzungProtokollieren(pfad As String, app As String)
#If debug Then
        pfad="W:\diverses\bgmingrada"
#End If


        Dim dateiPfad As String = pfad & "\hurz"
        ' Pfad zur Datei im selben Ordner wie die EXE
        Try
            dateiPfad = IO.Path.Combine(dateiPfad, "nutzung_" & DateTime.Now.ToString("yyyy-MM-dd") & ".txt")

            ' Inhalt der neuen Zeile
            Dim eintrag As String = Environment.UserName & ";" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & ";" & app

            ' Datei anlegen falls nicht vorhanden und neue Zeile anhängen
            IO.File.AppendAllText(dateiPfad, eintrag & Environment.NewLine)

        Catch ex As Exception
            l("NutzungProtokollieren " & ex.ToString)
        End Try
    End Sub
End Class
