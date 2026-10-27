Public Class WINansichten
    Sub New()
        InitializeComponent()
    End Sub
    Sub winansichten_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

    End Sub
    ' Beispiel-Methode, die z.B. bei einem Klick auf einen Bestätigungs-Button aufgerufen wird
    Private Sub GeneriereThemeString_Click(sender As Object, e As RoutedEventArgs)
        klicken()
    End Sub

    Private Sub klicken()
        ' 1. Alle Checkboxen aus beiden Panels zusammenführen
        Dim allCheckboxes = LeftPanel.Children.OfType(Of CheckBox)().
                            Concat(RightPanel.Children.OfType(Of CheckBox)())

        ' 2. Filtern (nur angeklickte), Content auslesen und Leerzeichen zu "%20" machen
        ' Korrigierte Zeile mit Option Strict On Kompatibilität
        Dim selectedThemes = allCheckboxes.
                     Where(Function(cb) cb.IsChecked.GetValueOrDefault()).
                     Select(Function(cb) cb.Content.ToString().Replace(" ", "%20"))

        ' 3. Zu einem einzigen String mit Komma-Trennung zusammenfügen
        Dim resultString As String = String.Join(",", selectedThemes)

        ' Ausgabe zur Kontrolle (z.B. in einer MessageBox oder im Debug-Output)

        ThemeStringTextBox.Text = resultString

        ' Hier können Sie den resultString nun für Ihre URL / API / Logik verwenden
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'speichern
        Dim zieldatei = tools.makeFullpathMeineauswahlThema("themendateiMeineAuswahl.txt")
        IO.File.WriteAllText(zieldatei, "0;theme=" & ThemeStringTextBox.Text)
        'Dim fi As New IO.FileInfo(zieldatei)
        'If Not fi.Exists Then
        '    MessageBox.Show("Die gewählte Themendatei existiert nicht. Bitte wählen Sie eine andere Themendatei aus.", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
        '    Exit Sub
        'End If
        Me.DialogResult = True
        'Close()
    End Sub

    Private Sub AlleAuswaehlen_Click(sender As Object, e As RoutedEventArgs)
        Dim allCheckboxes = LeftPanel.Children.OfType(Of CheckBox)().
                            Concat(RightPanel.Children.OfType(Of CheckBox)())

        For Each cb In allCheckboxes
            cb.IsChecked = True
        Next
        klicken()
    End Sub

    Private Sub AlleAbwaehlen_Click(sender As Object, e As RoutedEventArgs)
        Dim allCheckboxes = LeftPanel.Children.OfType(Of CheckBox)().
                            Concat(RightPanel.Children.OfType(Of CheckBox)())

        For Each cb In allCheckboxes
            cb.IsChecked = False
        Next
        ThemeStringTextBox.Text = ""
    End Sub

    Private Sub btnaabruch_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
    End Sub
End Class
