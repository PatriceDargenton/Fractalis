
' Fichier ClsFractQuadTreeR.vb
' ----------------------------

Imports System.Text ' Pour StringBuilder

Public Class ClsFractQuadTreeR : Inherits ClsFractRemplissage

#Region "Configuration"

    Private Const bModeLent As Boolean = False
    Private Const iDelaisMSec% = 10 '50 Si mode lent

    ' QT : QuadTree
    Private Const iPasMaxQT% = 32 ' Pavé de 32x32 pixels
    Private Const iPasMinQT% = 1

    Private Const bModeRemplissageQT As Boolean = False

#End Region

#Region "Déclarations"

    Public Shadows Event EvFinTrace()

    ' m_cfMax : coord. fract. avec des pavés de taille max. (iPasMaxQT)
    ' m_cfMin : coord. fract. avec des pavés de taille min. (iPasMinQT)
    Private m_cfMax, m_cfMin As TCoordFract

    ' Rapport entre les tailles de pavé
    Private Const iFact% = iPasMaxQT \ iPasMinQT
    Private Const iFactSur2% = iFact \ 2
    Private Const iFactSur4% = iFact \ 4
    Private m_iIndiceMaxPas%

    ' Tableau de piles correspondant à chaque taille de pavé
    Private m_aPiles() As ClsPile

    Protected m_remplissageCyan As New SolidBrush(couleurFondCyan)

    ' Shadows indique que l'on masque l'événement de la classe de base
    Public Shadows Event EvMajBmp()
    ' Définition de l'avancement du remplissage :
    '  on se base sur la proportion de pixels à examiner, 
    '  proportion obtenue à la résolution précédente
    Public Shadows Event EvMajAvancement(iAvancement%)
    Private m_iNbPixels% ' Nombre de pixels examinés
    Private m_rTauxMaxSurfaceRemp! ' Taux max. de surface remplie
    ' Taux min. de surface remplie lorsque la résolution des images augmente 
    Private m_rTauxMinSurfaceRempImages!

    ' Tableau pour mémoriser les codes des pixels analysés
    Private m_aiCodesPixelQT(,,) As Byte

#End Region

#Region "Tracé des images fractales avec le remplissage"

    Public Overrides Sub TracerFractDepart(bmpCache As Bitmap)
        MyBase.InitTracerFractDepart()
        TracerFractQuadTree()
    End Sub

    Private Sub TracerFractQuadTree()

        ' iNbIterationsMax dépend du nombre d'itérations min. précédant :
        '  cela évite de le définir trop élevé dès le début, alors que 
        '  c'est seulement pour un zoom profond que l'on a besoin de 
        '  beaucoup d'itérations
        m_iMemNbIterationsMin = m_iNbIterationsMin
        If Not m_bZoomMoins AndAlso m_iNbIterationsMin < iIntegerMax Then _
            m_iNbIterationsMax = m_prm.iNbIterationsMaxDepart + m_iNbIterationsMin

        m_iNbIterationsMin = iIntegerMax

        m_gr.Clear(couleurFondCyan)
        RaiseEvent EvMajBmp()

        ' Recopier les coord. du zoom
        m_cfMax = m_cf
        ' D'abord voir combien il y a de gros carrés dans l'écran
        MyBase.InitCoordFract(m_cfMax, iPasMaxQT)
        m_cfMin = m_cfMax ' Recopier les marges + coord. du zoom
        ' Ensuite diviser les gros carrés en petits carrés
        m_cfMin.iPaveMaxX = m_cfMax.iPaveMaxX * iFact
        m_cfMin.iPaveMaxY = m_cfMax.iPaveMaxY * iFact
        m_cfMin.rLargPaveAbs = m_cfMax.rLargPaveAbs / iFact
        m_cfMin.rHautPaveAbs = m_cfMax.rHautPaveAbs / iFact

        ' Pour algo. rapide
        ' -----------------
        Dim W As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim H As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin
        Dim width% = m_szTailleEcran.Width
        Dim heigth% = m_szTailleEcran.Height
        Dim X2 As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Dim Y2 As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
        Dim xs As Decimal = X2 - (W / 2)
        Dim ys As Decimal = Y2 - (H / 2)
        m_prm.rXd = W / CDec(width)
        m_prm.rYd = H / CDec(heigth)
        ' -----------------

        ' Pour cacher les gros pixels hors zone
        If m_bEffacerImg Then m_gr.Clear(couleurFondCyan)

        m_iIndiceMaxPas = CInt(Math.Log(iFact) / Math.Log(2))
        ReDim m_aPiles(m_iIndiceMaxPas)

        Dim i%
        For i = 0 To m_iIndiceMaxPas
            m_aPiles(i) = New ClsPile()
            m_aPiles(i).Initialiser()
        Next i
        ' 05/08/2014 Même sans remplissage, on va l'utiliser, car bPtDejaTracé n'est plus utilisé
        'ReDim m_aiCodesPixelQT(m_iIndiceMaxPas, m_cfMin.iPaveMaxX + 1, m_cfMin.iPaveMaxY + 1)
        ' Optimisation à faire : pas besoin de toute la largeur à chaque niveau !
        ReDim m_aiCodesPixelQT(m_iIndiceMaxPas, m_cfMin.iPaveMaxX + iFact, m_cfMin.iPaveMaxY + iFact)

        If bModeRemplissageQT Then
            If Not bModeRemplissage() Then Exit Sub
        Else
            TracerFract()
        End If

        RaiseEvent EvMajBmp()
        RaiseEvent EvFinTrace()
        'If Not m_bQuitterTrace Then Beep(600, 20)

    End Sub

    Private Function bModeRemplissage() As Boolean

        Dim iIndicePasAct% = 0
        Dim iPaveX%, iPaveY%

        If bDebugRemp Then
            If Not bTracerQT(0, 0) Then Exit Function
            Return True
        End If

        iPaveY = 0
        For iPaveX = 0 To m_cfMin.iPaveMaxX - iFact Step iFact
            If m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bTracerQT(iPaveX, iPaveY) Then Exit Function
            If bQuitter() Then Exit Function
        Next iPaveX

        iPaveX = m_cfMin.iPaveMaxX
        For iPaveY = 0 To m_cfMin.iPaveMaxY - iFact Step iFact
            If m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bTracerQT(iPaveX, iPaveY) Then Exit Function
            If bQuitter() Then Exit Function
        Next iPaveY

        iPaveY = m_cfMin.iPaveMaxY
        For iPaveX = m_cfMin.iPaveMaxX To 0 Step -iFact
            If m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bTracerQT(iPaveX, iPaveY) Then Exit Function
            If bQuitter() Then Exit Function
        Next iPaveX

        iPaveX = 0
        For iPaveY = m_cfMin.iPaveMaxY - iFact To iFact Step -iFact
            If m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bTracerQT(iPaveX, iPaveY) Then Exit Function
            If bQuitter() Then Exit Function
        Next iPaveY

        Return True

    End Function

    Private Shadows Sub TracerFract()

        m_rTauxMaxSurfaceRemp = 0
        m_iNbPixels = 0

        Dim iPaveX%, iPaveY%

        For iPaveX = 0 To m_cfMax.iPaveMaxX
            For iPaveY = 0 To m_cfMax.iPaveMaxY
                If Not bTracerQT(iPaveX * iFact, iPaveY * iFact) Then Exit Sub
                If bQuitter() Then Exit Sub
            Next iPaveY : Next iPaveX

        If m_rTauxMaxSurfaceRemp < m_rTauxMinSurfaceRempImages Then _
            m_rTauxMinSurfaceRempImages = m_rTauxMaxSurfaceRemp
        RaiseEvent EvMajAvancement(100)

    End Sub

    Private Function bTracerQT(iPaveX%, iPaveY%) As Boolean

        If m_bQuitterTrace Then Exit Function

        Dim bRestePixel(m_iIndiceMaxPas) As Boolean

        Dim pt As PointPile

        Dim iPasSuivant%, iTaillePave%, iTaillePaveSuiteUPM%, iTaillePaveUPM%
        Dim rDec, rDec2 As Decimal
        Dim iPasAct%

        ' IndicePas va de 0 (résolution actuelle : correspond à des pavés iPasMaxQT) 
        '  jusqu'au niveau de profondeur m_iIndiceMaxPas 
        '  (correspond à des pavés iPasMinQT)
        Dim iIndicePasAct% = 0

        CalculerPasSuivant(iIndicePasAct, iPasAct, iPasSuivant,
            iTaillePave, rDec, rDec2, iTaillePaveSuiteUPM, iTaillePaveUPM)

        'Debug.WriteLine("iIndicePasAct=" & iIndicePasAct)

        ' On peut initialiser la pile à chaque remplissage, on est quand même sûr 
        '  que l'on oubliera aucun pixels (car le remplissage est redondant),
        '  et la taille de la pile sera plus faible
        'm_aPiles(iIndicePasAct).Initialiser() ' 05/08/2014
        If Not m_aPiles(iIndicePasAct).bEmpiler(iPaveX, iPaveY) Then GoTo Echec

        'Debug.WriteLine( _
        '    "iIndicePasAct=" & iIndicePasAct & ", iPasAct=" & iPasAct & _
        '    ", iPasSuivant=" & iPasSuivant & ", iTaillePave=" & iTaillePave & _
        '    ", rDec=" & rDec & ", rDec2=" & rDec2 & _
        '    ", iTaillePaveSuiteUPM=" & iTaillePaveSuiteUPM)

        Do
            If m_aPiles(iIndicePasAct).bPileVide Then GoTo ParcourirPile

            pt = m_aPiles(iIndicePasAct).ptDepilerPtPile
            iPaveX = pt.X
            iPaveY = pt.Y

            If bDebugQuadGr2 And iPasAct > 4 And bModeLent Then
                'm_remplissage.Color = Color.Turquoise
                'm_gr.FillRectangle(m_remplissage, _
                '    m_cfMin.iMargeX + iPaveX * iPasMinQT, _
                '    m_cfMin.iMargeY + iPaveY * iPasMinQT, iPasAct, iPasAct)
                Dim penPixel As New Pen(Color.White, 2)
                'penPixel.Color = Color.FromKnownColor(kcCouleurPalette(iNbIterations))
                'm_gr.DrawLine(penPixel, _
                '    m_cfMin.iMargeX, m_cfMin.iMargeY, _
                '    m_cfMin.iMargeX + iPaveX * iPasMinQT, m_cfMin.iMargeY + iPaveY * iPasMinQT)
                m_gr.DrawLine(penPixel,
                    m_cfMin.iMargeX + iPaveX * iPasMinQT, m_cfMin.iMargeY + iPaveY * iPasMinQT,
                    m_cfMin.iMargeX + iPaveX * iPasMinQT + iPasAct,
                    m_cfMin.iMargeY + iPaveY * iPasMinQT + iPasAct)
                RaiseEvent EvMajBmp()
                Application.DoEvents()
                If iDelaisMSec > 0 Then Threading.Thread.Sleep(iDelaisMSec)
            End If

            If m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) <> iCodePixelNonExamine Then
                'Debug.WriteLine(iPaveX & ", " & iPaveY & " : " & m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY))
                GoTo ParcourirPile
            End If

            bRestePixel(iIndicePasAct) = True
            m_iNbPixels += 1

            Dim rPaveXCentre As Decimal = iPaveX + rDec
            Dim rPaveYCentre As Decimal = iPaveY + rDec
            m_cfMin.rXAbs = rPaveXCentre * m_cfMin.rLargPaveAbs + m_cfMin.rCoordAbsXMin
            m_cfMin.rYAbs = rPaveYCentre * m_cfMin.rHautPaveAbs + m_cfMin.rCoordAbsYMin
            Dim bFrontiere As Boolean = False
            Dim iNbIterations% = MyBase.iCompterIterations(m_cfMin.rXAbs, m_cfMin.rYAbs,
                iPaveX, iPaveY, iPasMinQT, m_cfMin, bFrontiere) ' iPasAct

            ' 03/08/2014
            Dim iCodePixel As Byte
            If Not bFrontiere Then 'iNbIterations > iCodePixelFrontiere Then
                iCodePixel = iCodePixelDessin
            Else
                iCodePixel = iCodePixelFrontiere
            End If

            'If iPaveX = 8 AndAlso iPaveY = 0 AndAlso iIndicePasAct = 0 AndAlso iCodePixel = 2 Then Stop
            m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY) = iCodePixel
            If bFrontiere AndAlso Not bAfficherPixelsFrontiereModeRemplissage Then
                GoTo ParcourirPile
            End If

            If bDebugQuadGr And iPasAct > 4 And bModeLent Then
                m_remplissage.Color = Color.Yellow
                m_gr.FillRectangle(m_remplissage,
                    m_cfMin.iMargeX + iPaveX * iPasMinQT,
                    m_cfMin.iMargeY + iPaveY * iPasMinQT, iPasAct, iPasAct)
                RaiseEvent EvMajBmp()
                Application.DoEvents()
                If iDelaisMSec > 0 Then Threading.Thread.Sleep(iDelaisMSec)
            End If

            Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
            If Not m_bAlgoRapide Then bInterpoler = False ' Ne marche qu'avec l'algo. rapide
            If bInterpoler AndAlso bFrontiere Then
                m_remplissage.Color = m_prm.coulInterpolee
            Else
                m_remplissage.Color = CouleurPalette(iNbIterations, bFrontiere)
            End If

            m_gr.FillRectangle(m_remplissage,
                m_cfMin.iMargeX + iPaveX * iPasMinQT,
                m_cfMin.iMargeY + iPaveY * iPasMinQT, iPasAct, iPasAct)

            If bDebugQuadGr And iPasAct > 4 Then

                ' Contour du pavé courant
                m_gr.DrawRectangle(Pens.Yellow,
                    m_cfMin.iMargeX + iPaveX * iPasMinQT,
                    m_cfMin.iMargeY + iPaveY * iPasMinQT, iPasAct, iPasAct)

                ' Rectangle de 2 pixels au centre du pavé courant
                Dim iLargPixel% = 2
                Dim iPosG% = iLargPixel \ 2
                m_gr.FillRectangle(Brushes.Tomato,
                    m_cfMin.iMargeX + rPaveXCentre * iPasMinQT - iPosG,
                    m_cfMin.iMargeY + rPaveYCentre * iPasMinQT - iPosG, iLargPixel, iLargPixel)

            End If

            If bModeLent Then
                RaiseEvent EvMajBmp()
                Application.DoEvents()
                If iDelaisMSec > 0 Then Threading.Thread.Sleep(iDelaisMSec)
            End If

            ' Coeur de l'algorithme de QuadTree
            ' Réduire le pas jusqu'a minPas
            If iIndicePasAct < m_iIndiceMaxPas Then

                Const bVerifierCentreCoins As Boolean = False ' False
                Const bVerifierMilieux As Boolean = False ' False
                ' Les coins suffisent
                Const bVerifierCoins As Boolean = True ' True
                Const bVerifierMilieuxBords As Boolean = False ' False

                Dim rPaveXCentreCoinG As Decimal = iPaveX + rDec - rDec2
                Dim rPaveYCentreCoinH As Decimal = iPaveY + rDec - rDec2
                Dim rPaveXCentreCoinD As Decimal = iPaveX + rDec + rDec2
                Dim rPaveYCentreCoinB As Decimal = iPaveY + rDec + rDec2
                Dim rPaveXBordDroite As Decimal = iPaveX + rDec + rDec
                Dim rPaveYBordBas As Decimal = iPaveY + rDec + rDec

                If bVerifierCentreCoins Then
                    ' Centre du coin GH Gauche Haut
                    If bApprofondir(rPaveXCentreCoinG, rPaveYCentreCoinH, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Centre du coin DB Droite Bas
                    If bApprofondir(rPaveXCentreCoinD, rPaveYCentreCoinB, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Centre du coin GB Gauche Bas
                    If bApprofondir(rPaveXCentreCoinG, rPaveYCentreCoinB, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Centre du coin DH Droite Haut
                    If bApprofondir(rPaveXCentreCoinD, rPaveYCentreCoinH, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                End If

                If bVerifierMilieux Then
                    ' Milieu Haut
                    If bApprofondir(rPaveXCentre, rPaveYCentreCoinH, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu Droite
                    If bApprofondir(rPaveXCentreCoinD, rPaveYCentre, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu Bas
                    If bApprofondir(rPaveXCentre, rPaveYCentreCoinB, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu Gauche
                    If bApprofondir(rPaveXCentreCoinG, rPaveYCentre, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                End If

                If bVerifierCoins Then
                    ' Coin HG
                    If bApprofondir(iPaveX, iPaveY, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Coin HD
                    If bApprofondir(rPaveXBordDroite, rPaveYBordBas, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Coin HD
                    If bApprofondir(rPaveXBordDroite, iPaveY, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Coin BG
                    If bApprofondir(iPaveX, rPaveYBordBas, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                End If

                If bVerifierMilieuxBords Then
                    ' Milieu bord Haut
                    If bApprofondir(rPaveXCentre, iPaveY, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu bord Droit
                    If bApprofondir(rPaveXBordDroite, rPaveYCentre, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu bord Bas
                    If bApprofondir(rPaveXCentre, rPaveYBordBas, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                    ' Milieu bord Gauche
                    If bApprofondir(iPaveX, rPaveYCentre, iNbIterations, iPasAct, iPaveX, iPaveY) Then GoTo EmpilerCarresInf
                End If

                If bDebugQuadGr AndAlso iIndicePasAct = 0 Then

                    If bModeLent Then
                        RaiseEvent EvMajBmp()
                        Application.DoEvents()
                        If iDelaisMSec > 0 Then Threading.Thread.Sleep(iDelaisMSec)
                    End If

                    'Debug.WriteLine("!")
                End If

                GoTo Remplissage

EmpilerCarresInf:
                ' 25/08/2009 Ne pas empiler si iTaillePaveSuiteUPM=0 car bug précision ?
                If iTaillePaveSuiteUPM > 0 Then

                    'Debug.WriteLine( _
                    '    "iIndicePasAct=" & iIndicePasAct & ", iPasAct=" & iPasAct & _
                    '    ", iPasSuivant=" & iPasSuivant & ", iTaillePave=" & iTaillePave & _
                    '    ", rDec=" & rDec & ", rDec2=" & rDec2 & _
                    '    ", iTaillePaveSuiteUPM=" & iTaillePaveSuiteUPM)
                    'If iTaillePaveSuiteUPM = 1 Then
                    '    Debug.WriteLine("!")
                    'End If

                    If m_aiCodesPixelQT(iIndicePasAct + 1, iPaveX, iPaveY) = iCodePixelNonExamine Then
                        If Not m_aPiles(iIndicePasAct + 1).bEmpiler(iPaveX, iPaveY) Then GoTo Echec
                    End If
                    If m_aiCodesPixelQT(iIndicePasAct + 1, iPaveX + iTaillePaveSuiteUPM, iPaveY) =
                        iCodePixelNonExamine Then
                        If Not m_aPiles(iIndicePasAct + 1).bEmpiler(
                            iPaveX + iTaillePaveSuiteUPM, iPaveY) Then GoTo Echec
                    End If
                    If m_aiCodesPixelQT(iIndicePasAct + 1,
                            iPaveX + iTaillePaveSuiteUPM,
                            iPaveY + iTaillePaveSuiteUPM) = iCodePixelNonExamine Then
                        If Not m_aPiles(iIndicePasAct + 1).bEmpiler(
                            iPaveX + iTaillePaveSuiteUPM,
                            iPaveY + iTaillePaveSuiteUPM) Then GoTo Echec
                    End If
                    If m_aiCodesPixelQT(iIndicePasAct + 1, iPaveX, iPaveY + iTaillePaveSuiteUPM) =
                        iCodePixelNonExamine Then
                        If Not m_aPiles(iIndicePasAct + 1).bEmpiler(
                            iPaveX, iPaveY + iTaillePaveSuiteUPM) Then GoTo Echec
                    End If

                End If
            End If

Remplissage:
            If bModeRemplissageQT AndAlso (Not bFrontiere OrElse bInterpoler) Then
                ' Coeur de l'algorithme de remplissage
                If iPaveY >= iTaillePaveUPM AndAlso iPaveX <= m_cfMin.iPaveMaxX AndAlso iPaveY <= m_cfMin.iPaveMaxY AndAlso
                   m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY - iTaillePaveUPM) = iCodePixelNonExamine Then
                    If Not m_aPiles(iIndicePasAct).bEmpiler(iPaveX, iPaveY - iTaillePaveUPM) Then GoTo Echec
                End If
                If iPaveX >= iTaillePaveUPM AndAlso iPaveX <= m_cfMin.iPaveMaxX AndAlso iPaveY <= m_cfMin.iPaveMaxY AndAlso
                    m_aiCodesPixelQT(iIndicePasAct, iPaveX - iTaillePaveUPM, iPaveY) = iCodePixelNonExamine Then _
                        If Not m_aPiles(iIndicePasAct).bEmpiler(iPaveX - iTaillePaveUPM, iPaveY) Then GoTo Echec
                If iPaveY <= m_cfMin.iPaveMaxY - iTaillePaveUPM AndAlso iPaveX <= m_cfMin.iPaveMaxX AndAlso
                   m_aiCodesPixelQT(iIndicePasAct, iPaveX, iPaveY + iTaillePaveUPM) = iCodePixelNonExamine Then
                    If Not m_aPiles(iIndicePasAct).bEmpiler(iPaveX, iPaveY + iTaillePaveUPM) Then GoTo Echec
                End If
                If iPaveX <= m_cfMin.iPaveMaxX - iTaillePaveUPM AndAlso iPaveY <= m_cfMin.iPaveMaxY AndAlso
                    m_aiCodesPixelQT(iIndicePasAct, iPaveX + iTaillePaveUPM, iPaveY) = iCodePixelNonExamine Then _
                        If Not m_aPiles(iIndicePasAct).bEmpiler(iPaveX + iTaillePaveUPM, iPaveY) Then GoTo Echec
            End If

ParcourirPile:
            ' Si bParcourirPile alors on reboucle sur la pile
            Dim bBouclePile As Boolean
            If bClassePilePerso Then
                bBouclePile = m_aPiles(iIndicePasAct).bParcourirPile
            Else
                bBouclePile = m_aPiles(iIndicePasAct).bPileVide
            End If
            If bBouclePile Then
                'Debug.WriteLine("Fin de pile")
                ' Dans ce cas, s'il ne reste plus de pixel, alors on passe
                '  à la profondeur supérieure
                If Not bRestePixel(iIndicePasAct) Then
                    iIndicePasAct += 1
                    If iIndicePasAct > m_iIndiceMaxPas Then Exit Do
                    'Debug.WriteLine("Profondeur : " & iIndicePasAct)
                    CalculerPasSuivant(iIndicePasAct, iPasAct, iPasSuivant,
                        iTaillePave, rDec, rDec2, iTaillePaveSuiteUPM, iTaillePaveUPM)
                    'Debug.WriteLine( _
                    '    "iIndicePasAct=" & iIndicePasAct & ", iPasAct=" & iPasAct & _
                    '    ", iPasSuivant=" & iPasSuivant & ", iTaillePave=" & iTaillePave & _
                    '    ", rDec=" & rDec & ", rDec2=" & rDec2 & _
                    '    ", iTaillePaveSuiteUPM=" & iTaillePaveSuiteUPM)
                End If
                bRestePixel(iIndicePasAct) = False
                RaiseEvent EvMajBmp()

                If m_rTauxMinSurfaceRempImages > 0 Then
                    Dim rTauxSurfaceRemp! = CSng(m_iNbPixels /
                        ((m_cfMin.iPaveMaxX + 1) * (m_cfMin.iPaveMaxY + 1)))
                    Dim iAv% = CInt(100 * rTauxSurfaceRemp /
                        m_rTauxMinSurfaceRempImages)
                    RaiseEvent EvMajAvancement(iAv)
                End If

            Else
                ' Rafraichir l'écran à chaque itération : optimiser ? ToDo
                If m_aPiles(iIndicePasAct).bMajBmp() Then RaiseEvent EvMajBmp()
            End If

            If bQuitter() Then Exit Function

        Loop While Not m_bQuitterTrace

        Dim rTauxSurfaceRempFin! = CSng(m_iNbPixels /
            ((m_cfMin.iPaveMaxX + 1) * (m_cfMin.iPaveMaxY + 1)))
        If rTauxSurfaceRempFin > m_rTauxMaxSurfaceRemp Then _
            m_rTauxMaxSurfaceRemp = rTauxSurfaceRempFin

        If m_bQuitterTrace Then Return False
        Return True

Echec:
        Return False

    End Function

    Private Function bApprofondir(rPaveX As Decimal, rPaveY As Decimal, iNbIterations%, iPasAct%,
            iPaveX%, iPaveY%) As Boolean

        If bDebugQuadGr AndAlso iPasAct > 8 Then
            Dim iLargPixel% = 4 '3
            Dim iPosG% = iLargPixel \ 2
            m_gr.FillRectangle(Brushes.Yellow,
                m_cfMin.iMargeX + rPaveX * iPasMinQT - iPosG,
                m_cfMin.iMargeY + rPaveY * iPasMinQT - iPosG, iLargPixel, iLargPixel)

            'If bModeLent Then
            '    RaiseEvent EvMajBmp()
            '    Application.DoEvents()
            '    If iDelaisMSec > 0 Then Threading.Thread.Sleep(iDelaisMSec)
            'End If

        End If

        m_cfMin.rXAbs = rPaveX * m_cfMin.rLargPaveAbs + m_cfMin.rCoordAbsXMin
        m_cfMin.rYAbs = rPaveY * m_cfMin.rHautPaveAbs + m_cfMin.rCoordAbsYMin
        Dim bFrontiere As Boolean = False
        Dim iNbIter% = MyBase.iCompterIterations(m_cfMin.rXAbs, m_cfMin.rYAbs,
            iPaveX, iPaveY, iPasAct, m_cfMin, bFrontiere)
        If iNbIter <> iNbIterations Then Return True
        Return False

    End Function

    Private Sub CalculerPasSuivant(iIndicePasAct%,
            ByRef iPasAct%, ByRef iPasSuivant%,
            ByRef iTaillePave%,
            ByRef rDec As Decimal, ByRef rDec2 As Decimal,
            ByRef iTaillePaveSuiteUPM%, ByRef iTaillePaveUPM%)

        ' Le quadrillage se base sur le pas minimal (les pas supérieurs sont 
        '  des multiples du pas de base)

        iPasAct = iPasMaxQT
        iTaillePaveUPM = iPasMaxQT \ iPasMinQT

        ' Calcul du centre du pavé actuel, unité = taille pavé min.
        If iPasMaxQT = iPasMinQT Then
            ' Si le pas max. = min. alors le centre est la moitié d'un pavé min.
            rDec = 0.5D
            rDec2 = 0 ' 20/07/2014 Il n'y a pas de pavé plus petit
            iPasSuivant = 0 ' 20/07/2014 Il n'y a pas de pavé plus petit
            ' Taille pavé suite dans l'unité du pavé min. (UPM)
            iTaillePaveSuiteUPM = 0 'iPasMinQT
        Else
            ' Sinon le centre est par défaut la moitié d'un pavé max.
            rDec = CDec(iPasMaxQT / iPasMinQT) / 2D
            rDec2 = rDec / 2D ' 20/07/2014
            iTaillePaveSuiteUPM = (iPasMaxQT \ iPasMinQT) \ 2
            iPasSuivant = iPasAct \ 2 ' Si 0 alors ne pas empiler
        End If

        Dim i% = iIndicePasAct

        While i > 0
            iPasAct \= 2
            iPasSuivant \= 2
            ' Calcul du centre du pavé actuel
            If iPasSuivant = 0 Then
                rDec = 0.5D
                iTaillePaveSuiteUPM = 0
            Else
                ' Sinon le centre est par défaut la moitié d'un pavé max.
                rDec = CDec(iPasAct / iPasMinQT) / 2D
                iTaillePaveSuiteUPM = (iPasAct \ iPasMinQT) \ 2
            End If
            rDec2 = rDec / 2D

            i -= 1
        End While
        iTaillePave = iPasAct

        'Debug.WriteLine( _
        '    "iIndicePasAct=" & iIndicePasAct & ", iPasAct=" & iPasAct & _
        '    ", iPasSuivant=" & iPasSuivant & ", iTaillePave=" & iTaillePave & _
        '    ", rDec=" & rDec & ", rDec2=" & rDec2 & _
        '    ", iTaillePaveSuiteUPM=" & iTaillePaveSuiteUPM)

    End Sub

#End Region

End Class