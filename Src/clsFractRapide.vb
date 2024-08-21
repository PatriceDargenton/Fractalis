
' Classe rapide pour les fractales de degré 2
' - Parallélisation du code
' - Tracé dans un tableau directement affiché en bitmap
' - Algorithme rapide (limité au dégré 2, et 
'    incorrect pour Julia : décalage du point de départ)

' D'après The beauty of fractals - A simple fractal rendering program done in C#
' https://www.codeproject.com/Articles/38514/The-beauty-of-fractals-A-simple-fractal-rendering

Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks ' Parallel.For

Public Class clsFractRapide : Inherits clsFract

#Region "Déclarations"

    ' Shadows indique que l'on masque l'événement de la classe de base
    Public Shadows Event EvMajBmp()

    ' Définition de l'avancement du remplissage :
    '  on se base sur la proportion de pixels à examiner, 
    '  proportion obtenue à la résolution précédente
    Public Shadows Event EvMajAvancement(iAvancement%)

    Protected Shadows Const bAlgoRapide As Boolean = True

    Private m_palette%() = Nothing

#End Region

#Region "Tracé des images fractales"

    Public Overrides Sub InitConfig()
        MyBase.InitConfig()
        m_bEffacerImgDef = False
    End Sub

    Public Overrides Sub InitPalette()
        MyBase.InitPalette()
        InitPalette0(m_palette)
    End Sub

    Private Sub InitPalette0(ByRef colors0%())

        Dim i As Integer
        Dim iMax% = m_iNbCouleurs + iNbCouleursReservees - 1
        ReDim colors0(iMax)
        For i = 0 To iMax
            Dim color0 As Color = CouleurPalette(i, bFrontiere:=False)
            colors0(i) = color0.ToArgb
        Next i

    End Sub

    Protected Overrides Sub TracerFract(iPas%)

        ' Le 64 bits est 2x plus rapide que le 32 bits ! 
        ' (mais pas de vidéo possible à cause de l'API vidéo en 32 bits)
        ' Le Single n'est pas plus rapide que le Double 
        ' (et même un peu plus lent, surtout en 64 bits !)
        ' Le Décimal est très lent (20x plus lent)

        ' Programmation générique : pas simple :
        ' http://stackoverflow.com/questions/1267902/generics-where-t-is-a-number
        ' http://www.codeproject.com/Articles/8531/Using-generics-for-calculations
        ' http://tomasp.net/blog/fsharp-generic-numeric.aspx/

        If m_bDecimal Then
            PartialRender_Decimal()
        Else
            'PartialRender_Single() inutile
            PartialRender_Double()
        End If
    End Sub

    Sub PartialRender_Double()

        ' FracMaster
        ' https://www.codeproject.com/Articles/38514/The-beauty-of-fractals-A-simple-fractal-rendering

        Dim W, H, X2, Y2, xs, ys, xd, yd As Double

        W = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        H = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Ne marche pas : on n'obtient pas exactement le même point de zoom
        ' (il faudrait commencer en décimal, mais 20x + lent)
        'If W < rPaveMin OrElse H < rPaveMin Then
        '    PartialRender_Decimal()
        '    Exit Sub
        'End If

        'Debug.WriteLine("Nb iter.= " & m_iMemNbIterationsMin & " -> " & m_iNbIterationsMax & _
        '    ", " & W & ", " & H)

        Dim width% = m_bmpCache.Width
        Dim heigth% = m_bmpCache.Height

        Dim pdate As BitmapData = Me.m_bmpCache.LockBits(New Rectangle(0, 0, width, heigth),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb)
        Dim pscan0 As IntPtr = pdate.Scan0

        'Dim iTaille% = width * heigth
        Dim dst%(width * heigth)
        'If dst.Length - 1 <> iTaille Then
        '    Debug.WriteLine("!")
        'End If

        Dim iTaillePal% = m_palette.Length

        ' Test Decimal : 20x + lent que Double !
        X2 = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Y2 = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
        xs = X2 - (W / 2)
        ys = Y2 - (H / 2)
        xd = W / CDbl(width)
        yd = H / CDbl(heigth)
        Dim bMEtJ As Boolean = False
        Dim bJulia0 As Boolean = False
        If m_prm.typeFract = TFractal.Julia Then bJulia0 = True
        If m_prm.typeFract = TFractal.MandelbrotEtJulia Then bMEtJ = True

        'For y As Integer = 0 To heigth - 1
        Parallel.For(0, heigth - 1, Sub(y)
                                        Dim y1 As Double = ys + yd * y
                                        Dim x1, r1, i1, r1pow2, i1pow2, rpow, rlastpow, rCount_f, rFactor, a, b As Double

                                        Dim idx% = y * width
                                        x1 = xs
                                        For x As Integer = 0 To width - 1

                                            r1 = 0
                                            i1 = 0
                                            a = 0
                                            b = 0
                                            r1pow2 = 0
                                            i1pow2 = 0

                                            If bJulia0 OrElse bMEtJ Then
                                                a = m_prm.rRe
                                                b = m_prm.rIm
                                                r1 = x1
                                                i1 = y1
                                                r1pow2 = r1 * r1
                                                i1pow2 = i1 * i1
                                            End If

                                            rpow = 0
                                            rlastpow = 0

                                            Dim iNbIter% = 0
                                            Do While iNbIter < m_iNbIterationsMax AndAlso rpow < 4
                                                r1pow2 = r1 * r1
                                                i1pow2 = i1 * i1

                                                If bJulia0 Then
                                                    i1 = 2 * i1 * r1 + b
                                                    r1 = r1pow2 - i1pow2 + a
                                                ElseIf bMEtJ Then
                                                    i1 = (2 * i1) * r1 + y1 + b
                                                    r1 = r1pow2 - i1pow2 + x1 + a
                                                Else
                                                    i1 = (2 * i1) * r1 + y1
                                                    r1 = r1pow2 - i1pow2 + x1
                                                End If

                                                rlastpow = rpow
                                                rpow = r1pow2 + i1pow2
                                                iNbIter += 1
                                            Loop
                                            Dim bFrontiere As Boolean = False
                                            If iNbIter >= m_iNbIterationsMax Then bFrontiere = True
                                            'If rpow >= 4 Then Dans ce cas on a quitté la boucle sans atteindre la frontière

                                            ' Noter le nombre min. d'itérations pour déterminer le max.
                                            If iNbIter < m_iNbIterationsMin Then m_iNbIterationsMin = iNbIter

                                            Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
                                            If bInterpoler AndAlso bFrontiere Then

                                                rCount_f = iNbIter - 1 + (4 - rlastpow) / (rpow - rlastpow)
                                                rFactor = (1.0# - iNbIter + rCount_f) * 255
                                                Dim iFactor% = 0
                                                If rFactor >= Integer.MinValue AndAlso
                                                    rFactor <= Integer.MaxValue Then iFactor = CInt(rFactor)
                                                Dim iCoul1% = iCalculerCouleur(iNbIter - 1, bFrontiere:=False)
                                                Dim iCoul2% = iCalculerCouleur(iNbIter, bFrontiere:=False)
                                                dst(idx) = InterpolateColors(iCoul1, iCoul2, iFactor)
                                                idx += 1

                                                'Dim factor As Integer = CInt(((1.0 - (iter - count_f)) * 255))
                                                'dst(idx++) = Utils.InterpolateColors(Palette((iter - 1)), Palette(iter), factor)

                                            Else

                                                'iNbIter = (iNbIter Mod Palette.Length)
                                                'dst(idx) = Palette(iNbIter)

                                                Dim iIndiceCoul% = iCalculerCouleur(iNbIter, bFrontiere)
                                                dst(idx) = m_palette(iIndiceCoul)
                                                idx += 1

                                            End If
                                            x1 += xd
                                        Next x
                                        'y1 += yd

                                    End Sub)
        'Next y

        ' Filtrage bilinéaire : appliquer un flou calculé en 2D pour éviter la pixellisation
        ' https://fr.wikipedia.org/wiki/Filtrage_bilinéaire
        ' Le filtrage bilinéaire est un algorithme utilisé en infographie permettant de calculer des
        '  pixels intermédiaires entre les pixels d'une image ou d'une texture que l'on change de taille. 
        ' C'est un des procédés les plus utilisés depuis la fin des années 1990 par les cartes 
        '  accélératrices 3D pour éviter l'effet de crènelage apparaissant dans le cas d'un filtrage linéaire.
        ' Ce filtrage utilise une interpolation bilinéaire qui, contrairement à une interpolation linéaire 
        '  qui se contente d'interpoler en 1D (sur les lignes par exemple), l'interpolation est faite 
        '  en 2D (lignes, colonnes). Ceci résulte en un effet de flou, bien plus agréable à l'œil que 
        '  les carrés ou rectangles visibles habituellement sur une image agrandie.
        If m_prmPalette.bLisser Then
            RaiseEvent EvMajAvancement(80)
            Dim filteredColorTable As Integer() = New Integer(width * heigth - 1) {}
            Dim idxs11% = 0
            Dim idxs12% = 0
            Dim idxs21% = 0
            Dim idxs22% = 0
            For y As Integer = 0 To (heigth - 1) - 1
                idxs11 = y * width
                idxs12 = idxs11 + 1
                idxs21 = idxs11 + width
                idxs22 = idxs21 + 1
                For x As Integer = 0 To (width - 1) - 1
                    Dim colf1% = InterpolateColors(dst(idxs11), dst(idxs12), &H7F)
                    Dim colf2% = InterpolateColors(dst(idxs21), dst(idxs22), &H7F)
                    filteredColorTable(idxs11) = InterpolateColors(colf1, colf2, &H80)
                    idxs11 += 1
                    idxs12 += 1
                    idxs21 += 1
                    idxs22 += 1
                Next
            Next
            dst = filteredColorTable
        End If

        RaiseEvent EvMajAvancement(90)

        Marshal.Copy(dst, 0, pscan0, dst.Length - 1)
        Me.m_bmpCache.UnlockBits(pdate)

        RaiseEvent EvMajBmp()

        RaiseEvent EvMajAvancement(100)

    End Sub

    Sub PartialRender_Decimal()

        ' FracMaster
        ' https://www.codeproject.com/Articles/38514/The-beauty-of-fractals-A-simple-fractal-rendering

        Dim W, H, X2, Y2, xs, ys, xd, yd As Decimal

        W = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        H = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Ne marche pas : on n'obtient pas exactement le même point de zoom
        ' (il faudrait commencer en décimal, mais 20x + lent)
        'If W < rPaveMin OrElse H < rPaveMin Then
        '    PartialRender_Decimal()
        '    Exit Sub
        'End If

        'Debug.WriteLine("Nb iter.= " & m_iMemNbIterationsMin & " -> " & m_iNbIterationsMax & _
        '    ", " & W & ", " & H)

        Dim width% = m_bmpCache.Width
        Dim heigth% = m_bmpCache.Height

        Dim pdate As BitmapData = Me.m_bmpCache.LockBits(New Rectangle(0, 0, width, heigth),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb)
        Dim pscan0 As IntPtr = pdate.Scan0

        Dim dst%(width * heigth)

        Dim iTaillePal% = m_palette.Length

        ' Test Decimal : 20x + lent que Double !
        X2 = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Y2 = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
        xs = X2 - (W / 2)
        ys = Y2 - (H / 2)
        xd = W / CDec(width)
        yd = H / CDec(heigth)
        'y1 = ys
        Dim bMEtJ As Boolean = False
        Dim bJulia0 As Boolean = False
        If m_prm.typeFract = TFractal.Julia Then bJulia0 = True
        If m_prm.typeFract = TFractal.MandelbrotEtJulia Then bMEtJ = True

        'For y As Integer = 0 To heigth - 1
        Parallel.For(0, heigth - 1, Sub(y)
                                        Dim y1 As Decimal = ys + yd * y
                                        Dim x1, r1, i1, r1pow2, i1pow2, rpow, rlastpow, rCount_f, rFactor, a, b As Decimal

                                        Dim idx% = y * width
                                        x1 = xs
                                        For x As Integer = 0 To width - 1

                                            r1 = 0
                                            i1 = 0
                                            a = 0
                                            b = 0
                                            r1pow2 = 0
                                            i1pow2 = 0

                                            If bJulia0 OrElse bMEtJ Then
                                                a = m_prm.rRe
                                                b = m_prm.rIm
                                                r1 = x1
                                                i1 = y1
                                                r1pow2 = r1 * r1
                                                i1pow2 = i1 * i1
                                            End If

                                            rpow = 0
                                            rlastpow = 0

                                            Dim iNbIter% = 0
                                            Do While iNbIter < m_iNbIterationsMax AndAlso rpow < 4
                                                r1pow2 = r1 * r1
                                                i1pow2 = i1 * i1

                                                If bJulia0 Then
                                                    i1 = 2 * i1 * r1 + b
                                                    r1 = r1pow2 - i1pow2 + a
                                                ElseIf bMEtJ Then
                                                    i1 = (2 * i1) * r1 + y1 + b
                                                    r1 = r1pow2 - i1pow2 + x1 + a
                                                Else
                                                    i1 = (2 * i1) * r1 + y1
                                                    r1 = r1pow2 - i1pow2 + x1
                                                End If

                                                rlastpow = rpow
                                                rpow = r1pow2 + i1pow2
                                                iNbIter += 1
                                            Loop
                                            Dim bFrontiere As Boolean = False
                                            If iNbIter >= m_iNbIterationsMax Then bFrontiere = True
                                            'If rpow >= 4 Then Dans ce cas on a quitté la boucle sans atteindre la frontière

                                            ' Noter le nombre min. d'itérations pour déterminer le max.
                                            If iNbIter < m_iNbIterationsMin Then m_iNbIterationsMin = iNbIter

                                            Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
                                            If bInterpoler AndAlso bFrontiere Then

                                                rCount_f = iNbIter - 1 + (4 - rlastpow) / (rpow - rlastpow)
                                                rFactor = (1D - iNbIter + rCount_f) * 255
                                                Dim iFactor% = 0
                                                If rFactor >= Integer.MinValue AndAlso
                                                    rFactor <= Integer.MaxValue Then iFactor = CInt(rFactor)
                                                Dim iCoul1% = iCalculerCouleur(iNbIter - 1, bFrontiere:=False)
                                                Dim iCoul2% = iCalculerCouleur(iNbIter, bFrontiere:=False)
                                                dst(idx) = InterpolateColors(iCoul1, iCoul2, iFactor)
                                                idx += 1

                                                'Dim factor As Integer = CInt(((1.0 - (iter - count_f)) * 255))
                                                'dst(idx++) = Utils.InterpolateColors(Palette((iter - 1)), Palette(iter), factor)

                                            Else

                                                'iNbIter = (iNbIter Mod Palette.Length)
                                                'dst(idx) = Palette(iNbIter)

                                                Dim iIndiceCoul% = iCalculerCouleur(iNbIter, bFrontiere)
                                                dst(idx) = m_palette(iIndiceCoul)
                                                idx += 1

                                            End If
                                            x1 += xd
                                        Next x
                                        'y1 += yd

                                    End Sub)
        'Next y

        ' Filtrage bilinéaire : appliquer un flou calculé en 2D pour éviter la pixellisation
        ' https://fr.wikipedia.org/wiki/Filtrage_bilinéaire
        ' Le filtrage bilinéaire est un algorithme utilisé en infographie permettant de calculer des
        '  pixels intermédiaires entre les pixels d'une image ou d'une texture que l'on change de taille. 
        ' C'est un des procédés les plus utilisés depuis la fin des années 1990 par les cartes 
        '  accélératrices 3D pour éviter l'effet de crènelage apparaissant dans le cas d'un filtrage linéaire.
        ' Ce filtrage utilise une interpolation bilinéaire qui, contrairement à une interpolation linéaire 
        '  qui se contente d'interpoler en 1D (sur les lignes par exemple), l'interpolation est faite 
        '  en 2D (lignes, colonnes). Ceci résulte en un effet de flou, bien plus agréable à l'œil que 
        '  les carrés ou rectangles visibles habituellement sur une image agrandie.
        If m_prmPalette.bLisser Then
            RaiseEvent EvMajAvancement(80)
            Dim filteredColorTable As Integer() = New Integer(width * heigth - 1) {}
            Dim idxs11% = 0
            Dim idxs12% = 0
            Dim idxs21% = 0
            Dim idxs22% = 0
            For y As Integer = 0 To (heigth - 1) - 1
                idxs11 = y * width
                idxs12 = idxs11 + 1
                idxs21 = idxs11 + width
                idxs22 = idxs21 + 1
                For x As Integer = 0 To (width - 1) - 1
                    Dim colf1% = InterpolateColors(dst(idxs11), dst(idxs12), &H7F)
                    Dim colf2% = InterpolateColors(dst(idxs21), dst(idxs22), &H7F)
                    filteredColorTable(idxs11) = InterpolateColors(colf1, colf2, &H80)
                    idxs11 += 1
                    idxs12 += 1
                    idxs21 += 1
                    idxs22 += 1
                Next
            Next
            dst = filteredColorTable
        End If

        RaiseEvent EvMajAvancement(90)

        Marshal.Copy(dst, 0, pscan0, dst.Length - 1)
        Me.m_bmpCache.UnlockBits(pdate)
        RaiseEvent EvMajBmp()

        RaiseEvent EvMajAvancement(100)

    End Sub

#End Region

End Class