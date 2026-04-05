Public Class clsBplan
    Public Property bplname As String
    Public Property bplnummer As String
    Public Property bplbeschreibung As String
    Public Property warnung As String
    Public Property ident As Integer
    Public Property nutzung As String
    Public Property gemeindetext As String
    Public Property gemarkungstext As String
    Public Property flaeche As String
    Public Property ueberlagertvon As String
    Public Property ueberlagertselbst As String
    Public Property object_guid As String
    Public Property verboten As Boolean
    Public Property rechtswirksam As Date
    Public Property aufstellung As Date
    Public Property anhaenge As List(Of myComboBoxItem)
    Public Function bildeTextOhneWarnung() As String
        Dim text As New Text.StringBuilder
        Try
            text.Append("Gemeinde:" & vbTab & gemeindetext & Environment.NewLine)
            text.Append("Gemarkung:" & vbTab & gemarkungstext & Environment.NewLine)
            text.Append("Kurzname:" & vbTab & bplnummer & Environment.NewLine)
            text.Append("Titel:" & vbTab & vbTab & bplbeschreibung & Environment.NewLine)
            text.Append("Nutzung:" & vbTab & nutzung & Environment.NewLine)
            text.Append("Überlagert:" & vbTab & ueberlagertselbst & Environment.NewLine)
            text.Append("ÜberlagertVon:" & vbTab & ueberlagertvon & Environment.NewLine)
            text.Append("Fläche:" & vbTab & flaeche & Environment.NewLine)
            text.Append("Aufstellung:" & vbTab & aufstellung & Environment.NewLine)
            text.Append("Rechtswirksam:" & vbTab & rechtswirksam & Environment.NewLine)
            Return text.ToString
        Catch ex As Exception
            Return text.ToString
        End Try
    End Function
End Class
