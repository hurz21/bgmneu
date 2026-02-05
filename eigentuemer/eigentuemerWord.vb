Imports System.IO
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports FontSize = DocumentFormat.OpenXml.Wordprocessing.FontSize
Public Class eigentuemerWord

    Public Function machma(text As String, filepath As String, datum As String) As Boolean

        Try
            Using wordDocument As WordprocessingDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document)
                Dim mainPart As MainDocumentPart = wordDocument.AddMainDocumentPart()

                mainPart.Document = New Document()
                Dim body As Body = mainPart.Document.AppendChild(New Body())
                Dim para As Paragraph = body.AppendChild(New Paragraph())
                Dim run As Run = para.AppendChild(New Run())
                'run.AppendChild(New Text("Kreis Offenbach   Postfach 1265  63112 Dietzenbach "))
                ''Dim para2 As Paragraph = body.AppendChild(New Paragraph())
                ''para2.AppendChild(New Run())
                ''Dim run2 As Run = para2.AppendChild(New Run())
                'run.AppendChild(New Text("Datum: " & datum & ", " & Environment.UserName))
                'para.AppendChild(New Run())
                'run.AppendChild(New Text("Auskunft aus dem Liegenschaftsbuch (ALKIS)"))
                'para.AppendChild(New Run())
                'run.AppendChild(New Text(" "))
                'para.AppendChild(New Run())
                'run.AppendChild(New Text(" "))

                Dim v As String = "     Der Auszug aus dem Amtlichen Liegenschaftskataster-Informationssystem (ALKIS) darf nur " &
                        "intern verwendet werden." & Environment.NewLine &
                        "Eine Weitergabe des Auszugs an Dritte ist unzulässig." & Environment.NewLine &
                        "Auskünfte aus dem ALKIS an Dritte erteilt - bei Vorliegen eines berechtigten Interesses - " & Environment.NewLine &
                        "das Katasteramt." & Environment.NewLine &
                "(kundenservice.afb - heppenheim@hvbg.hessen.de)." & Environment.NewLine &
                "Alle Zugriffe werden protokolliert."
                'run.AppendChild(New Text(v))
                'run.AppendChild(New Text("--------------------------------------------------------------"))
                'run.AppendChild(New Text("--------------------------------------------------------------"))
                TextAbsatzErzeugen(filepath, "Kreis Offenbach   Postfach 1265  63112 Dietzenbach", "Arial", "20", True, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, "Datum:  " & datum & ", " & Environment.UserName, "Arial", "16", True, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, "Auskunft aus dem Liegenschaftsbuch (ALKIS)", "Arial", "16", True, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, "Erstellt am: " & Now.ToString("dd.MM.yyyy"), "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)

                Dim a As String()
                a = text.Split(New String() {Environment.NewLine}, StringSplitOptions.None)
                For Each line As String In a
                    TextAbsatzErzeugen(filepath, line, "Arial", "12", False, para)
                Next


                'TextAbsatzErzeugen(filepath, text, "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)
                TextAbsatzErzeugen(filepath, " ", "Arial", "12", False, para)

                a = v.Split(New String() {Environment.NewLine}, StringSplitOptions.None)
                For Each line As String In a
                    TextAbsatzErzeugen(filepath, line, "Arial", "12", False, para)
                Next

                'TextAbsatzErzeugen(filepath, v, "Arial", "12", False, para)
                'run.AppendChild(New Text(text))

                'Datum: " & Format(Now, "dd.MM.yyyy") & ", " & Environment.UserName, bf, 8, zeilenabstand_gross
                'Auskunft aus dem Liegenschaftsbuch (ALKIS) " & " ", bf, 14, zeilenabstand_gross)
                ' ---------- Schnellauskunft bis 4500 Zeichen ---------- " & " ", bf, 12, zeilenabstand_gross)

            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    'Public Sub CreateWordprocessingDocument(ByVal filepath As String)
    '    Try
    '        l(" MOD CreateWordprocessingDocument anfang")
    '        ' Create a document by supplying the filepath.
    '        wordDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document)
    '        ' Add a main document part. 
    '        mainPart = wordDocument.AddMainDocumentPart()
    '        ' Create the document structure and add some text. 
    '        mainPart.Document = New Document()

    '        koerper = mainPart.Document.AppendChild(New Body())
    '        l(" MOD CreateWordprocessingDocument ende")
    '    Catch ex As Exception
    '        l("Fehler in CreateWordprocessingDocument: " & ex.ToString())
    '    End Try
    'End Sub

    Public Sub TextAbsatzErzeugen(ByVal filepath As String, txt As String, fontname As String,
                               fontsize As String, fett As Boolean, para As Paragraph)
        Dim run As Run = para.AppendChild(New Run)
        Dim a() As String
        'Dim run As New Run
        Dim rPr As New RunProperties
        Dim runf As New RunFonts
        runf.Ascii = fontname
        Dim size As New FontSize()
        size.Val = fontsize

        rPr.Append(runf)
        rPr.Append(size)

        Dim Bold As Bold = New Bold()
        Bold.Val = OnOffValue.FromBoolean(fett)
        rPr.AppendChild(Bold)

        run.AppendChild(Of RunProperties)(rPr)

        a = txt.Split("#"c)
        For i = 0 To a.GetUpperBound(0)
            run.AppendChild(New Text(a(i)))
            run.AppendChild(New Break())
        Next
        ' Close the handle explicitly.
        'wordprocessingDocument.Close()
    End Sub


    'Private Shared Function AddRun(word As String,
    '                    Optional font As String = "Arial",
    '                    Optional size As String = "20",
    '                    Optional bold As Boolean = False,
    '                    Optional italic As Boolean = False,
    '                    Optional underline As Boolean = False,
    '                    Optional preserveSpace As Boolean = True) As Run
    '    ' Create Run instance.
    '    Dim run As New Run()

    '    ' Create RunFonts instance.
    '    Dim runFont As New RunFonts() With {.Ascii = font}

    '    ' Create FontSize instance.
    '    ' It must be multiplication twice of the required size.
    '    Dim fontSize As New FontSize() With {.Val = New StringValue(size)}

    '    ' Create Text instance.
    '    Dim text As New Text(word)

    '    ' Create RunProperties instance.
    '    Dim runProperties As New RunProperties()
    '    If bold Then
    '        ' Applying Bold.
    '        runProperties.Bold = New Bold()
    '    End If
    '    If italic Then
    '        ' Applying Italic.
    '        runProperties.Italic = New Italic()
    '    End If
    '    If underline Then
    '        ' Applying Underline.
    '        runProperties.Underline = New Underline()
    '    End If
    '    If preserveSpace Then
    '        ' Defines the SpaceProcessingModeValues.
    '        text.Space = SpaceProcessingModeValues.Preserve
    '    End If

    '    ' Adding Font to RunProperties.
    '    runProperties.Append(runFont)

    '    ' Adding FontSize to RunProperties.
    '    runProperties.Append(fontSize)

    '    ' Adding RunProperties to Run.
    '    run.Append(runProperties)

    '    ' Adding Text to Run.
    '    run.Append(text)

    '    Return run
    'End Function

    'Protected Sub OnCreate(sender As Object, e As EventArgs)
    '    Using stream As New MemoryStream()
    '        ' Create a document.
    '        Using wordDocument As WordprocessingDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, True)
    '            ' Add main document part.
    '            Dim mainPart As MainDocumentPart = wordDocument.AddMainDocumentPart()

    '            ' Create document structure.
    '            mainPart.Document = New Document()

    '            ' Create document body.
    '            Dim body As Body = mainPart.Document.AppendChild(New Body())

    '            ' Create paragraph.
    '            Dim paragraph As New Paragraph()

    '            ' Creating Run.
    '            Dim run As Run = Me.AddRun("Hi,")

    '            ' Adding Run to Paragraph.
    '            paragraph.Append(run)

    '            ' Adding Paragraph to Body.
    '            body.Append(paragraph)

    '            ' Adding new Paragraph.
    '            paragraph = New Paragraph()
    '            run = Me.AddRun("This is ")
    '            paragraph.Append(run)

    '            ' Adding Text with Bold and Italic.
    '            run = Me.AddRun("Mudassar Khan", bold:=True, italic:=True)
    '            paragraph.Append(run)

    '            run = Me.AddRun(".")
    '            paragraph.Append(run)

    '            ' Adding Paragraph to Body.
    '            body.Append(paragraph)

    '            ' Adding Paragraph for Hyperlink.
    '            paragraph = New Paragraph()

    '            ' Adding Hyperlink to Paragraph.
    '            'paragraph.Append(Me.AddLink(mainPart, "aspsnippets", "https://www.aspsnippets.com"))

    '            ' Adding Hyperlink Paragraph to Document Body.
    '            body.Append(paragraph)
    '        End Using

    '        'Response.Clear()
    '        'Response.Buffer = True
    '        'Response.Charset = ""
    '        'Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    '        'Response.AppendHeader("Content-Disposition", "attachment; filename=Sample.docx")
    '        'Response.BinaryWrite(stream.ToArray())
    '        'Response.Flush()
    '        'Response.End()
    '    End Using
    'End Sub
End Class
