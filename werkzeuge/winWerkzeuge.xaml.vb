Public Class winWerkzeuge
    Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

    End Sub

    '   Select Case count(jahr_blattnr || baulastnr) As anzahl, jahr_blattnr  FROM " & tools.srv_schema & "." & tools.srv_tablename & "
    'group by jahr_blattnr,baulastnr
    'order by anzahl desc

    Private Sub btnPDFtool_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        If Environment.UserName <> "Feinen_J" Then
            MsgBox("Kein Zugriff")
            Exit Sub
        End If
        ' Get recursive List of all files starting in this directory.
        Dim list As List(Of String) = filehelper.GetFilesRecursive(tbPDFPfad.Text, "*.tiff")
        ' Loop through and display each path.
        For Each path In list
            Console.WriteLine(path)
            clsTIFFtools.zerlegeMultipageTIFF(path, tools.baulastenoutDir)
        Next
        Console.WriteLine(list.Count)
        MsgBox("pdfs erzeugt: " & list.Count)
    End Sub

    Private Sub winWerkzeuge_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        e.Handled = True
        tbPDFPfad.Text = srv_unc_path & "\fkat\baulasten"
        Title = "BGM: Werkzeuge " & " V.: " & bgmVersion
    End Sub

    Private Sub btnPruefung1_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        clsChecks.vollstaendig(srv_unc_path & "\fkat\baulasten")
    End Sub

    Private Sub btnPruefung2_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim datei = clsToolWerkzeuge.init()
        Process.Start(datei)
    End Sub

    Private Sub ___showdispatcher(v As String)

    End Sub

    Private Sub btnPruefung3_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim datei = "c:\kreisoffenbach\bgm\baulastenba.exe"
        MessageBox.Show("Es wird nun ein eigener prozess gestartet." & Environment.NewLine &
                        " " & Environment.NewLine &
                        " So können sie ungestört mit dem BGM weiterarbeiten." & Environment.NewLine &
                        " Die Ausgabedateien liegen immer unter '" & tools.baulastenoutDir & "'" & Environment.NewLine &
                        " " & Environment.NewLine &
                        " Die Datei wird automatisch geöffnet sobald der Prozess beendet ist." & Environment.NewLine &
                        " " & Environment.NewLine &
                        " hf " & Environment.NewLine)
        Process.Start(datei)
    End Sub



    Private Sub btnbeguenstigt_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'MsgBox("aaa")
        clsProBGTools.holeAlleBeguenstigten()
    End Sub
End Class
