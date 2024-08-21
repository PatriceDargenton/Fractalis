
Module Constantes

    Public Const sTitreMsg$ = "Fractalis"
    Public Const sDateVersionAppli$ = "21/08/2024"

#If DEBUG Then ' Pas de mode Release en DotNet2 : tjrs Debug
    Public Const bDebug As Boolean = True
    Public Const bDebugBugQuad As Boolean = False
    Public Const bDebugQuadGr As Boolean = False
    Public Const bDebugQuadGr2 As Boolean = False

    ' Si on met True, ne remplit pas le tour du cadre, seulement graine au coin HG
    Public Const bDebugRemp As Boolean = False
    Public Const bRelease As Boolean = False
#Else
        Public Const bDebug As Boolean = False
        Public Const bDebugBugQuad As Boolean = False ' False
        Public Const bDebugQuadGr As Boolean = False ' False
        Public Const bDebugQuadGr2 As Boolean = False ' False
        Public Const bDebugRemp As Boolean = False ' False
        Public Const bRelease As Boolean = True
#End If

    Public Const bAfficherPixelsFrontiereModeRemplissage As Boolean = True

    Public ReadOnly couleurFondCyan As Color = Color.Cyan

    Public Const iNbCouleursPalette% = 1024 ' 768 + 256

    Public Const kcCouleurPixelNonExamine As KnownColor = KnownColor.White
    Public Const kcCouleurPixelFrontiere As KnownColor = KnownColor.LightGreen

    ' La fonction Beep() standard de .NET est totalement inaudible, 
    '  celle-ci marche, mais elle n'est pas .NET :
    Public Declare Function Beep% Lib "kernel32" (dwFreq%, dwDuration%)

End Module

Public Structure TPrmPalette ' Paramètres de palette

    Dim bAfficherPalette As Boolean
    Dim bPaletteSysteme As Boolean
    Dim iPremCouleur%
    Dim iNbCouleurs%
    Dim iNbCyclesDegrade%
    Dim bPaletteAleatoire As Boolean

    ' Interpoler les couleurs de la palette d'une itération à l'autre,
    '  sur la base de la progression des 3 composantes RVB des 2 couleurs de la palette de dégradé
    '  avec un facteur dépendant de la vitesse de sortie du cercle unitaire (cf. algo. fractal)
    'Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
    Dim bFrontiereUnie As Boolean

    Dim bLisser As Boolean

End Structure

Public Structure TCoordFract ' Structure pour les coordonnées fractales

    ' Coordonnées absolues dans l'ensemble de Mandelbrot ou Julia
    '  Un rectangle serait bien, mais il n'y a pas de constructeur 
    '  en Decimal, seulement en Single : RectangleF
    '  (on a besoin de Decimal pour zoomer à fond)
    Dim rCoordAbsXMin, rCoordAbsXMax As Decimal
    Dim rCoordAbsYMin, rCoordAbsYMax As Decimal

    Dim rXAbs, rYAbs As Decimal
    Dim iPaveMaxX%, iPaveMaxY% ' Indices max. des pavés droite et bas
    Dim iMargeX%, iMargeY%
    Dim rLargPaveAbs As Decimal
    Dim rHautPaveAbs As Decimal

    Dim rZoomCible As Decimal

End Structure

Public Enum TFractal ' Types d'ensemble fractal
    Mandelbrot
    Julia
    MandelbrotEtJulia ' 01/02/2015
End Enum