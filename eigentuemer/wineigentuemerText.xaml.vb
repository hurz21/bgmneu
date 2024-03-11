Public Class wineigentuemerText
    Dim fst2 As String
    'Sub New(fst As clsFlurstueck)

    '    ' Dieser Aufruf ist für den Designer erforderlich.
    '    InitializeComponent()

    '    ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
    '    fst2 = fst
    'End Sub
    Sub New(fst As String)

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        fst2 = fst
    End Sub
    Private Sub btnabbruch_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub btnclip_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        My.Computer.Clipboard.SetText(fst2)
    End Sub
    Private Sub wineigentuemerText_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        e.Handled = True
        tb.Text = fst2
    End Sub


End Class
