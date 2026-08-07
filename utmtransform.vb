Public Class utmtransform

    Public Shared Function UTM32NachWGS84(rechtswert As Double,
                                hochwert As Double) As (Laengengrad As Double, Breitengrad As Double)

        ' UTM Zone 32N -> WGS84
        ' Standardmäßig für Deutschland geeignet
        ' EPSG:25832 / EPSG:32632

        Const a As Double = 6378137.0
        Const eccSquared As Double = 0.006694380023
        Const k0 As Double = 0.9996

        Dim eccPrimeSquared As Double = eccSquared / (1 - eccSquared)

        ' UTM-Koordinaten zurückrechnen
        Dim x As Double = rechtswert - 500000.0
        Dim y As Double = hochwert

        ' Zone 32N -> kein Südhalbkugel-Offset erforderlich

        Dim M As Double = y / k0

        Dim mu As Double = M / (a * (1 - eccSquared / 4 _
                                  - 3 * eccSquared ^ 2 / 64 _
                                  - 5 * eccSquared ^ 3 / 256))

        Dim e1 As Double = (1 - Math.Sqrt(1 - eccSquared)) /
                       (1 + Math.Sqrt(1 - eccSquared))

        Dim J1 As Double = 3 * e1 / 2 - 27 * e1 ^ 3 / 32
        Dim J2 As Double = 21 * e1 ^ 2 / 16 - 55 * e1 ^ 4 / 32
        Dim J3 As Double = 151 * e1 ^ 3 / 96
        Dim J4 As Double = 1097 * e1 ^ 4 / 512

        Dim fp As Double = mu +
                       J1 * Math.Sin(2 * mu) +
                       J2 * Math.Sin(4 * mu) +
                       J3 * Math.Sin(6 * mu) +
                       J4 * Math.Sin(8 * mu)

        Dim sinFp As Double = Math.Sin(fp)
        Dim cosFp As Double = Math.Cos(fp)
        Dim tanFp As Double = Math.Tan(fp)

        Dim C1 As Double = eccPrimeSquared * cosFp ^ 2
        Dim T1 As Double = tanFp ^ 2

        Dim N1 As Double = a / Math.Sqrt(1 - eccSquared * sinFp ^ 2)
        Dim R1 As Double = a * (1 - eccSquared) /
                       (1 - eccSquared * sinFp ^ 2) ^ 1.5

        Dim D As Double = x / (N1 * k0)

        Dim latitude As Double =
        fp - (N1 * tanFp / R1) *
        (D ^ 2 / 2 -
         (5 + 3 * T1 + 10 * C1 - 4 * C1 ^ 2 - 9 * eccPrimeSquared) *
         D ^ 4 / 24 +
         (61 + 90 * T1 + 298 * C1 + 45 * T1 ^ 2 -
          252 * eccPrimeSquared - 3 * C1 ^ 2) *
         D ^ 6 / 720)

        Dim longitude As Double =
        (D -
         (1 + 2 * T1 + C1) * D ^ 3 / 6 +
         (5 - 2 * C1 + 28 * T1 - 3 * C1 ^ 2 +
          8 * eccPrimeSquared + 24 * T1 ^ 2) *
         D ^ 5 / 120) / cosFp

        ' Zentralmeridian von UTM Zone 32 = 9° Ost
        longitude = longitude * 180.0 / Math.PI + 9.0
        latitude = latitude * 180.0 / Math.PI

        Return (longitude, latitude)

    End Function
    '```

    '### Verwendung

    'Zum Beispiel : 

    '```vbnet
    'Dim rw As Double = 480000
    '    Dim hw As Double = 5550000

    '    Dim koordinaten = UTM32NachWGS84(rw, hw)

    '    Dim laenge As Double = koordinaten.Laengengrad
    '    Dim breite As Double = koordinaten.Breitengrad

    'Debug.Print("Länge: " & laenge.ToString("0.000000"))
    'Debug.Print("Breite: " & breite.ToString("0.000000"))
    '```

    'Google Maps erwartet die Reihenfolge **Breitengrad, Längengrad**, also beispielsweise:

    '```text
    '50.123456, 8.654321
    '```

    'Für einen Google-Maps-Link:

    '```vbnet
    'Dim googleUrl As String =
    '    "https://www.google.com/maps?q=" &
    '    breite.ToString(Globalization.CultureInfo.InvariantCulture) & "," &
    '    laenge.ToString(Globalization.CultureInfo.InvariantCulture)
    '```

    '**Wichtig** Die Funktion geht von **UTM Zone 32N** aus und entfernt den UTM-False-Easting von 500.000 m. Für deine Gegend im Rhein-Main-Gebiet passt das grundsätzlich.

    'Wenn du möchtest, kann ich dir auch eine **noch kürzere VB.NET-Funktion schreiben, die direkt aus `RW` und `HW` einen fertigen Google-Maps-Link zurückgibt**.

End Class
