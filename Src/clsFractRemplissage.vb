
' Fichier ClsFractRemplissage.vb
' ------------------------------

Public Class ClsFractRemplissage : Inherits clsFract

#Region "Déclarations"

    ' Shadows indique que l'on masque l'événement de la classe de base
    Public Shadows Event EvMajBmp()
    ' Définition de l'avancement du remplissage :
    '  on se base sur la proportion de pixels à examiner, 
    '  proportion obtenue à la résolution précédente
    Public Shadows Event EvMajAvancement(iAvancement%)
    Private m_iNbPixels%       ' Nombre de pixels examinés
    Private m_rTauxMaxSurfaceRemp! ' Taux max. de surface remplie
    ' Taux min. de surface remplie lorsque la résolution des images augmente 
    Private m_rTauxMinSurfaceRempImages!

    ' Par convention, 0 est le code d'un pixel non examiné
    'Protected Const iCodePixelNonExamine% = 0
    'Protected Const iCodePixelFrontiere% = 1 : défini dans la classe de base
    'Private Const iCodePixelDessin% = 2
    ' Tableau pour mémoriser les codes des pixels analysés
    Protected m_aiCodesPixel(,) As Byte

    Protected m_pile As New ClsPile()

#End Region

#Region "Tracé des images fractales avec le remplissage"

    Protected Overrides Sub InitialiserTracerFract()

        ' Par défaut toute l'image doit être remplie
        m_rTauxMinSurfaceRempImages = 1

    End Sub

    Protected Overrides Sub TracerFract(iPas%)

        m_rTauxMaxSurfaceRemp = 0
        m_iNbPixels = 0

        Dim iPaveX%, iPaveY%
        ReDim m_aiCodesPixel(m_cf.iPaveMaxX, m_cf.iPaveMaxY)
        m_pile.Initialiser()

        If bDebugRemp Then
            If Not bRemplissage(0, 0, iPas) Then Exit Sub
            Exit Sub
        End If

        iPaveY = 0
        For iPaveX = 0 To m_cf.iPaveMaxX - 1
            If m_aiCodesPixel(iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bRemplissage(iPaveX, iPaveY, iPas) Then Exit Sub
        Next iPaveX

        iPaveX = m_cf.iPaveMaxX
        For iPaveY = 0 To m_cf.iPaveMaxY - 1
            If m_aiCodesPixel(iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bRemplissage(iPaveX, iPaveY, iPas) Then Exit Sub
        Next iPaveY

        iPaveY = m_cf.iPaveMaxY
        For iPaveX = m_cf.iPaveMaxX To 0 Step -1 ' 03/08/2014
            If m_aiCodesPixel(iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bRemplissage(iPaveX, iPaveY, iPas) Then Exit Sub
        Next iPaveX

        iPaveX = 0
        For iPaveY = m_cf.iPaveMaxY - 1 To 1 Step -1 ' 03/08/2014
            If m_aiCodesPixel(iPaveX, iPaveY) = iCodePixelNonExamine Then _
                If Not bRemplissage(iPaveX, iPaveY, iPas) Then Exit Sub
        Next iPaveY

        If m_rTauxMaxSurfaceRemp < m_rTauxMinSurfaceRempImages Then _
            m_rTauxMinSurfaceRempImages = m_rTauxMaxSurfaceRemp
        RaiseEvent EvMajAvancement(100)

    End Sub

    Protected Overridable Function bRemplissage(iPaveX%, iPaveY%, iPas%) _
        As Boolean

        If m_bQuitterTrace Then Exit Function

        If iPaveX < 0 OrElse iPaveX > m_cf.iPaveMaxX OrElse
           iPaveY < 0 OrElse iPaveY > m_cf.iPaveMaxY Then GoTo Echec

        ' On peut initialiser la pile à chaque remplissage, on est quand même sûr 
        '  que l'on oubliera aucun pixels (car le remplissage est redondant),
        '  et la taille de la pile sera plus faible
        m_pile.Initialiser()
        If Not m_pile.bEmpiler(iPaveX, iPaveY) Then GoTo Echec

        Dim bRestePixel As Boolean
        bRestePixel = False
        Dim pt As PointPile ' PointShort
        Dim iNbIterations%
        Dim iCodePixel As Byte

        Do
            ' 04/08/2014
            If m_pile.bPileVide Then GoTo ParcourirPile

            pt = m_pile.ptDepilerPtPile

            iPaveX = pt.X
            iPaveY = pt.Y

            If m_aiCodesPixel(iPaveX, iPaveY) <> iCodePixelNonExamine Then _
                GoTo ParcourirPile ' Pixel déjà examiné

            bRestePixel = True
            m_iNbPixels += 1

            m_cf.rXAbs = (iPaveX + 0.5D) * m_cf.rLargPaveAbs + m_cf.rCoordAbsXMin
            m_cf.rYAbs = (iPaveY + 0.5D) * m_cf.rHautPaveAbs + m_cf.rCoordAbsYMin
            Dim bFrontiere As Boolean = False
            iNbIterations = iCompterIterations(m_cf.rXAbs, m_cf.rYAbs, iPaveX, iPaveY, iPas, m_cf, bFrontiere)

            If bFrontiere Then
                iCodePixel = iCodePixelDessin
            Else
                iCodePixel = iCodePixelFrontiere
            End If
            m_aiCodesPixel(iPaveX, iPaveY) = iCodePixel
            If bFrontiere AndAlso Not bAfficherPixelsFrontiereModeRemplissage Then GoTo ParcourirPile

            Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
            If Not m_bAlgoRapide Then bInterpoler = False ' Ne marche qu'avec l'algo. rapide
            If bInterpoler AndAlso bFrontiere Then
                m_remplissage.Color = m_prm.coulInterpolee
            Else
                m_remplissage.Color = CouleurPalette(iNbIterations, bFrontiere)
            End If

            m_gr.FillRectangle(m_remplissage,
                m_cf.iMargeX + iPaveX * iPas,
                m_cf.iMargeY + iPaveY * iPas, iPas, iPas)
            If bDebugRemp Then
                RaiseEvent EvMajBmp()
            End If
            ' Si on interpole, cela ne sert plus à rien de faire du remplissage
            If bFrontiere And Not bInterpoler Then GoTo ParcourirPile

            ' Coeur de l'algorithme de remplissage
            If iPaveY > 0 AndAlso
                m_aiCodesPixel(iPaveX, iPaveY - 1) = iCodePixelNonExamine Then _
                    If Not m_pile.bEmpiler(iPaveX, iPaveY - 1) Then GoTo Echec
            If iPaveX > 0 AndAlso
                m_aiCodesPixel(iPaveX - 1, iPaveY) = iCodePixelNonExamine Then _
                    If Not m_pile.bEmpiler(iPaveX - 1, iPaveY) Then GoTo Echec
            If iPaveY < m_cf.iPaveMaxY AndAlso
                m_aiCodesPixel(iPaveX, iPaveY + 1) = iCodePixelNonExamine Then _
                    If Not m_pile.bEmpiler(iPaveX, iPaveY + 1) Then GoTo Echec
            If iPaveX < m_cf.iPaveMaxX AndAlso
                m_aiCodesPixel(iPaveX + 1, iPaveY) = iCodePixelNonExamine Then _
                    If Not m_pile.bEmpiler(iPaveX + 1, iPaveY) Then GoTo Echec

ParcourirPile:
            Dim bBouclePile As Boolean
            If bClassePilePerso Then
                bBouclePile = m_pile.bParcourirPile
            Else
                bBouclePile = m_pile.bPileVide
            End If
            If bBouclePile Then
                If Not bRestePixel Then Exit Do
                bRestePixel = False
                RaiseEvent EvMajBmp()

                If m_rTauxMinSurfaceRempImages > 0 Then
                    Dim rTauxSurfaceRemp! = CSng(m_iNbPixels /
                        ((m_cf.iPaveMaxX + 1) * (m_cf.iPaveMaxY + 1)))
                    Dim iAv% = CInt(100 * rTauxSurfaceRemp /
                        m_rTauxMinSurfaceRempImages)
                    RaiseEvent EvMajAvancement(iAv)
                End If
            Else
                ' 04/08/2014 Rafraichir l'écran à chaque itération : optimiser ? ToDo
                If m_pile.bMajBmp() Then RaiseEvent EvMajBmp()
            End If

            If bQuitter() Then Exit Function

        Loop While Not m_bQuitterTrace

        Dim rTauxSurfaceRempFin! = CSng(m_iNbPixels /
            ((m_cf.iPaveMaxX + 1) * (m_cf.iPaveMaxY + 1)))
        If rTauxSurfaceRempFin > m_rTauxMaxSurfaceRemp Then _
            m_rTauxMaxSurfaceRemp = rTauxSurfaceRempFin

        bRemplissage = Not m_bQuitterTrace
        Exit Function

Echec:
        Exit Function

    End Function

#End Region

End Class