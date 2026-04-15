Imports DocumentFormat.OpenXml.Drawing

Public Class clsPGvorhaben
    Public Property jahr As String = ""
    Public Property nr As String = ""
    Public Property vorhaben1 As String = ""

    Public ReadOnly Property Anzeige As String
        Get
            Return $"{jahr}-{nr}, {vorhaben1} "
        End Get
    End Property
End Class
