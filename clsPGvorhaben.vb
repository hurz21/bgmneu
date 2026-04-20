''' <summary>Represents a project (Vorhaben) with year, number, and description.</summary>
Public Class PGVorhaben
    ''' <summary>The year of the project.</summary>
    Public Property Jahr As String = ""
    
    ''' <summary>The project number.</summary>
    Public Property Nr As String = ""
    
    ''' <summary>The project description.</summary>
    Public Property Vorhaben As String = ""

    ''' <summary>Gets a formatted display string of the project.</summary>
    Public ReadOnly Property Anzeige As String
        Get
            Return $"{Jahr}-{Nr}, {Vorhaben}"
        End Get
    End Property
End Class
