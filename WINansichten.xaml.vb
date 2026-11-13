
Imports System.Net
Imports System.Windows.Controls

Public Class WINansichten
    Sub New()
        InitializeComponent()
    End Sub
    Sub winansichten_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        cmbThemendatei.ItemsSource = themendateien
        cmbThemendatei.DisplayMemberPath = "mySttring"
        cmbThemendatei.SelectedValuePath = "myindex"
        cmbThemendatei.IsDropDownOpen = False
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

    Private Sub btnreset_Click(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        'lokale datei löschen
        Dim zieldatei = tools.makeFullpathMeineauswahlThema("themendateiMeineAuswahl.txt")
        If IO.File.Exists(zieldatei) Then
            IO.File.Delete(zieldatei)
        End If
        MessageBox.Show("Die lokale Themendatei wurde gelöscht. Bitte wählen sie ihr Standard-Thema neu in der Dropdownliste.", "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Information)
        Me.DialogResult = False
    End Sub

    Private Sub cmbThemendatei_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        e.Handled = True
        Dim dateiitem As myComboBoxItem
        Dim themendatei As String
        Dim themen As String
        Dim zieldatei As String
        Try
            dateiitem = CType(cmbThemendatei.SelectedItem, myComboBoxItem)
            themendatei = dateiitem.myindex
            If themendatei = "themendateiMeineAuswahl.txt" Then
                zieldatei = tools.makeFullpathMeineauswahlThema(themendatei)
                Dim fi As New IO.FileInfo(zieldatei)
                If Not fi.Exists Then
                    MessageBox.Show("Die gewählte Themendatei existiert nicht. Bitte wählen Sie eine andere Themendatei aus oder erzeugen sie eine eigene Auswahl.",
                                    "BGM Ingradatool", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                    Exit Sub
                Else
                    themen = tools.getthemen("", zieldatei)
                End If
            Else
                themen = tools.getthemen("", themendatei)
                'themendatei = tools.themendefinitionsdatei
            End If
            SetzeCheckboxenAusThemeString(themen)
            klicken()
        Catch ex As Exception
            l("fehler in cmbThemendatei_SelectionChanged " & ex.ToString)
        End Try
    End Sub



    Private Sub SetzeCheckboxenAusThemeString(themeString As String)
        If String.IsNullOrWhiteSpace(themeString) Then Return

        ' "theme=" entfernen, falls vorhanden
        Dim themeWert As String = themeString
        If themeWert.StartsWith("theme=", StringComparison.OrdinalIgnoreCase) Then
            themeWert = themeWert.Substring("theme=".Length)
        End If

        ' Einträge trennen und URL-Codierung dekodieren
        Dim ausgewaehlteThemen As New HashSet(Of String)(
        themeWert.Split(","c).
            Select(Function(x) WebUtility.UrlDecode(x.Trim())),
        StringComparer.OrdinalIgnoreCase
    )

        ' Checkboxen in beiden Panels setzen
        SetzeCheckboxenImPanel(LeftPanel, ausgewaehlteThemen)
        SetzeCheckboxenImPanel(RightPanel, ausgewaehlteThemen)
    End Sub

    Private Sub SetzeCheckboxenImPanel(
    panel As Panel,
    ausgewaehlteThemen As HashSet(Of String))

        For Each element As UIElement In panel.Children
            Dim cb As CheckBox = TryCast(element, CheckBox)

            If cb IsNot Nothing Then
                Dim inhalt As String = If(cb.Content?.ToString(), "")
                cb.IsChecked = ausgewaehlteThemen.Contains(inhalt)
            End If
        Next
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        e.Handled = True
        Dim info = ""
        Dim infotext As String = <![CDATA[== Probaug/ProUmwelt (PU) und die Themenzusammenstellungen im GIS ==

PU nutzt nicht die GIS-Themen die über das Zahnrad eingestellt sind!

Stattdessen nutzt PU aus der Nummer des Vorhabens die erste Ziffer um eine Zuordnung zu den GIS-Themen zu bekommen
z.B. 3307 = 3  => UNB Themen im GIS werden genutzt !
die Zuordnungen sind:
1 - Umwelt
2 - Umwelt
3 - UNB
4 - UWBB
5 - Immissionsschutz
6 - Feger
7 - themendatei7000 (= vorläufige Zuordnung)

Sobald eine pers. Themenzusammenstellung (Meine Auswahl) existiert, wird diese in PU genutzt. Die Vorhabennummer wird dann ignoriert.

-------------- Bauaufsicht ------------
Dies alles gilt nicht für die Bauaufsicht.
Im Bereich der Bauaufsicht wird im Moment nur der Denkmalschutz mit gesonderten GIS-Themen bedacht.
die Zuordnung geschieht über die Ring-Nummer '3'.]]>.Value

        ' Zuweisung an den WPF TextBlock im Code-Behind:
        ThemeStringTextBox.Text = infotext
    End Sub
End Class
