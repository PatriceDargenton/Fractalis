
' Fichier clsFract.vb
' -------------------

Imports System.Collections.Generic

Public Class clsFract

#Region "Constantes"

    ' Constantes par défaut

    Protected Const iPasMax% = 1 ' 2 : Pavé de 2x2 pixels
    Protected Const iPasMin% = 1 ' 1 pixel
    'Protected Const iPasMax% = 4
    'Protected Const iPasMin% = 4

    Public Const bDecimalDef As Boolean = False ' False
    Public m_bDecimal As Boolean = bDecimalDef

    Public Const bLisserDef As Boolean = False ' False
    Public m_bLisser As Boolean = bLisserDef

    ' Si clsFractRapide, cela n'est pas désactivable
    Public Const bAlgoRapideDef As Boolean = True
    Public m_bAlgoRapide As Boolean = bAlgoRapideDef

    ' Si on met False, on interpole les couleurs là où l'on sort du cercle 
    ' (dépassement des itérations max.)
    Public Const bFrontiereUnieDef As Boolean = False ' False

    ' En mode Strict On, As doit être utilisé
    'Public Const typeFractDef As TFractal = TFractal.Mandelbrot ' Défaut
    'Public Const bPaletteSystemeDef As Boolean = False 'True ' False
    'Public Const typeFractDef As TFractal = TFractal.Julia
    Public Const typeFractDef As TFractal = TFractal.MandelbrotEtJulia
    Public Const bPaletteSystemeDef As Boolean = True

    ' Shared : les instances de classe constantes doivent être partagées
    Public Shared ptfJuliaDef As PointF = New PointF(0, 0) ' MandelbrotJulia
    'Public Shared ptfJuliaDef As PointF = New PointF(0, 1.95)
    'Public Shared ptfJuliaDef As PointF = New PointF(-6, 0.8)
    'Public Shared ptfJuliaDef As PointF = New PointF(6, 0.8)
    'Public Shared ptfJuliaDef As PointF = New PointF(-8, 1.8)

    Public Const iDegreAlgoDef% = 2  ' Z -> Z^2 + C par défaut

    Public m_bEffacerImgDef As Boolean = False 'True

    Public Const rAmplitudeMinOkDouble# = 0.00000000000005 ' 5E-14 en Double
    Public Const rAmplitudeMinOkDecimal As Decimal = CDec(2.0E-25) ' 2E-25 en Decimal

    Public Const iNbIterationsMaxDepartDef% = 128

    ' Petit zoom avec les flèches
    Private Const rDeltaZoom As Decimal = 0.01D
    Public Const rFactPetitZoomMoins As Decimal = 1 + rDeltaZoom '1.02D '1.05D
    Public Const rFactPetitZoomPlus As Decimal = 1 - rDeltaZoom  '0.98D '0.95D
    Public Const rPetitDeplacement As Decimal = 0.01D
    Public Const rPetitDeplacementJulia As Decimal = 0.001D
    Public Const rTresPetitDeplacementJulia As Decimal = 0.00001D

    ' 08/02/2015 En mode cible, afficher le % log du zoom
    Public m_bModeCible As Boolean = False

    ' 25/01/2015 En mode zoom - ne pas augmenter le nombre d'itération max.
    Public m_bZoomMoins As Boolean = False

    ' Zoom par défaut en coordonnées absolues
    '  cercle entier visible à l'écran : rZoomDef = 2
    Public Const rZoomDef As Decimal = 2
    ' Pour un zoom arrière, on multiplie par 2 l'amplitude actuelle 
    '  en coord. abs de l'image
    Public Const rFacteurZoomMoins As Decimal = 2

    Public Const bPaletteAleatoireDef As Boolean = False

    ' Commencer le modulo par la 10ème couleur (pour éviter le blanc,
    '  qui est déjà utilisé pour le détail des itérations)
    Public Const iPremCouleurDef% = 10

    Public Const iCouleurMaxDef% = KnownColor.YellowGreen  ' = 167 '4

    ' Nombre de couleur max. dans la palette 768 : réduction de palette progressive si la division progresse
    ' 768/1   : 768 dégradés de couleurs
    ' 768/2   : 384
    ' 768/4   : 192
    ' 768/8   :  96
    ' 768/16  :  48 -> 3, 4, 6 ou 8 zones
    ' 768/32  :  24 -> 3 ou 4 zones
    ' 768/64  :  12 -> 3 ou 4 zones
    ' 768/128 :   6 -> 1 zone
    ' 768/256 :   3 -> 1 zone
    Public Const iNbCyclesPaletteDef% = 4 '1 '32 '2 '32 max.

    ' 0 : Non examiné, 1 : frontière, 2 à n : palette effective

    Protected Const iNbCouleursReservees% = 2

    Protected Const iCodePixelNonExamine% = 0
    ' Par convention, 1 est le code d'un pixel frontière : NbItérations max.
    Protected Const iCodePixelFrontiere% = 1
    Protected Const iCodePixelDessin% = 2

    Protected Const iIntegerMax% = Integer.MaxValue ' System.Int32.MaxValue ' = 2147483647

#End Region

#Region "Déclarations"

    ' Gestion des événements qui seront récupérés depuis la feuille principale
    '  (cela évite la méthode bourrine consistant à passer la feuille principale
    '   en propriété et à appeler directement ses méthodes :-)
    Public Event EvMajBmp()
    Public Event EvMajAvancement(iAvancement%)
    Public Event EvFinTrace()
    Public Event EvDetailIterations(aPt() As Point)

    Protected m_bModeTranslation As Boolean
    Protected m_bModeDetailIterations As Boolean
    Public m_bEffacerImg As Boolean
    Protected m_szTailleEcran As New Size() ' Dimension du tracé en pixels
    Public m_bQuitterTrace As Boolean  ' Pour quitter + vite le thread
    Protected m_gr As Graphics ' Graphique de tracé dans le bitmap de cache

    Public m_iNbCouleurs%
    Private m_aiCouleurs%()

    Protected m_remplissage As New SolidBrush(Color.Black)

    Public m_prmPalette As TPrmPalette

    ' Faire une pause dans le tracé pour récupérer du temps
    ' (alternative au thread)
    Public m_bPause As Boolean
    'Public m_bStop As Boolean

#Region "Gestion du niveau d'itération pour le zoom -"

    Public Class clsCoupleLogIter
        Public rLog#, iNbIter%
        Public Sub New(rLog0#, iNbIter0%)
            rLog = rLog0
            iNbIter = iNbIter0
        End Sub
    End Class
    Private m_lstNivIter As List(Of clsCoupleLogIter)

    Private Sub InitNivIter()
        m_lstNivIter = New List(Of clsCoupleLogIter)
    End Sub

    Private Sub AjouterNivIter(couple As clsCoupleLogIter)
        ' Ajouter un couple à la fin tant que le log est plus petit
        '  sinon couper la fin
        Dim lstNivIter As New List(Of clsCoupleLogIter)
        For Each couple0 As clsCoupleLogIter In m_lstNivIter
            If couple.rLog > couple0.rLog Then Exit For
            lstNivIter.Add(couple0)
        Next
        lstNivIter.Add(couple)
        m_lstNivIter = lstNivIter
    End Sub

    Private Function iTrouverNivIter%(rLog#)

        ' Retourner le nombre d'itération correspondant au log de l'amplitude de dessin

        Dim iNbIter% = 0
        Dim iMemNbIter% = 0
        Dim rMemLog# = 0
        For Each couple0 As clsCoupleLogIter In m_lstNivIter
            iNbIter = couple0.iNbIter
            If couple0.rLog <= rLog Then
                ' Faire une règle de 3 si possible
                If rMemLog <> 0 Then
                    Dim rNbIterMoy# = iMemNbIter + (rLog - rMemLog) *
                        (iNbIter - iMemNbIter) / (couple0.rLog - rMemLog)
                    Return CInt(rNbIterMoy)
                End If
                Return iNbIter
            End If
            rMemLog = couple0.rLog
            iMemNbIter = iNbIter
        Next
        Return iNbIter

    End Function

#End Region

    Public Structure TPrmFract   ' Paramètres de tracé

        Dim iDegre%                 ' Degré de l'équation Z -> Z^Degré + C
        Dim iNbIterationsMaxDepart% ' Itérations max. au départ du zoom
        Dim typeFract As TFractal   ' Type Mandelbrot ou Julia
        ' Pour les ensembles de Julia : Parties réelle et imag. du complexe Z
        Dim rRe, rIm As Decimal
        Dim rAngle, rRayon As Decimal ' Pour faire bouger le point Julia
        Dim rDeltaAngle As Decimal
        Dim rAngleZoom As Decimal

        ' Algo rapide
        ' -----------
        Dim rXd, rYd As Decimal      ' Pour algo. rapide
        Dim coulInterpolee As Color
        ' -----------

    End Structure
    Public m_prm As TPrmFract
    Public m_iNbIterationsMin%   ' Itérations min. constatés après un tracé
    Public m_iNbIterationsMax%   ' Itérations max. pour un tracé
    Public m_iMemNbIterationsMin%

    Public m_cf As TCoordFract

    ' Classe pour gérer l'affichage du détail des itérations 
    '  de l'équation sur un pixel
    Private Class clsDetailIterations

        Private m_iNbPts%
        Private m_aPt() As Point

        Public Sub Initialiser()
            m_iNbPts = 0
        End Sub

        Public Sub AjouterPointDetailIterations(a As Double, b As Double, ByRef cf As TCoordFract)

            If cf.rLargPaveAbs = 0 Or cf.rHautPaveAbs = 0 Then Exit Sub

            Dim pt As Point
            pt.X = CInt(cf.iMargeX + iPasMin * (a - cf.rCoordAbsXMin) /
                cf.rLargPaveAbs + iPasMin \ 2)
            pt.Y = CInt(cf.iMargeY + iPasMin * (b - cf.rCoordAbsYMin) /
                cf.rHautPaveAbs + iPasMin \ 2)
            ReDim Preserve m_aPt(m_iNbPts)
            m_aPt(m_iNbPts) = pt
            m_iNbPts += 1

        End Sub

        Public Function aptLirePoint() As Point()
            aptLirePoint = m_aPt
        End Function

    End Class
    Private m_oDetailIter As New clsDetailIterations()

#End Region

#Region "Propriétés"

    Public Property szTailleEcran() As Size
        Get
            Return m_szTailleEcran
        End Get
        Set(szVal As Size)
            m_szTailleEcran = szVal
        End Set
    End Property

    Public WriteOnly Property Gr() As Graphics
        Set(gr As Graphics)
            m_gr = gr
            ' Pas de différence constatée
            m_gr.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            m_gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighSpeed
            'm_gr.QualityMode = Drawing2D.QualityMode.Low ' Non trouvé ?
        End Set
    End Property

    Public Property bQuitterTrace() As Boolean
        Get
            Return m_bQuitterTrace
        End Get
        Set(bVal As Boolean)
            m_bQuitterTrace = bVal
        End Set
    End Property

    Public Property typeFrac As TFractal
        Get
            Return m_prm.typeFract
        End Get
        Set(bVal As TFractal)
            m_prm.typeFract = bVal
            'Select Case bVal
            'Case TFractal.Mandelbrot
            'Case TFractal.Julia
            'Case TFractal.MandelbrotEtJulia
            'End Select
        End Set
    End Property

    Public Property bJulia() As Boolean
        Get
            Return (m_prm.typeFract = TFractal.Julia OrElse
                    m_prm.typeFract = TFractal.MandelbrotEtJulia)
        End Get
        Set(bVal As Boolean)
            m_prm.typeFract = TFractal.Mandelbrot
            If bVal Then m_prm.typeFract = TFractal.Julia
        End Set
    End Property

    Public Property ptfJulia() As PointF
        Get
            ptfJulia.X = m_prm.rRe
            ptfJulia.Y = m_prm.rIm
        End Get
        Set(ptf As PointF)
            m_prm.rRe = CDec(ptf.X)
            m_prm.rIm = CDec(ptf.Y)
        End Set
    End Property

    Public Property iDegre%()
        Get
            Return m_prm.iDegre
        End Get
        Set(iVal%)
            m_prm.iDegre = iVal
        End Set
    End Property

    Public Property iNbIterationsMaxDepart%()
        Get
            Return m_prm.iNbIterationsMaxDepart
        End Get
        Set(iVal%)
            m_prm.iNbIterationsMaxDepart = iVal
        End Set
    End Property

    Public ReadOnly Property iNbIterationsMax%()
        Get
            Return m_iNbIterationsMax
        End Get
    End Property
    Public ReadOnly Property iNbIterationsMin%()
        Get
            Return m_iNbIterationsMin
        End Get
    End Property

    Public Property bModeDetailIterations() As Boolean
        Get
            Return m_bModeDetailIterations
        End Get
        Set(bVal As Boolean)
            m_bModeDetailIterations = bVal
        End Set
    End Property

    Public Property bModeTranslation() As Boolean
        Get
            Return m_bModeTranslation
        End Get
        Set(bVal As Boolean)
            m_bModeTranslation = bVal
        End Set
    End Property

    Public Property bEffacerImg() As Boolean
        Get
            Return m_bEffacerImg
        End Get
        Set(bVal As Boolean)
            m_bEffacerImg = bVal
        End Set
    End Property

    Public ReadOnly Property rCentreX() As Decimal
        Get
            Return (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        End Get
    End Property

    Public ReadOnly Property rCentreY() As Decimal
        Get
            Return (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
        End Get
    End Property

    Public ReadOnly Property rAmplitX() As Decimal
        Get
            Return m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        End Get
    End Property
    Public ReadOnly Property rAmplitY() As Decimal
        Get
            Return m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin
        End Get
    End Property

#End Region

#Region "Tracé des images fractales"

    Public Sub InitialiserPrmFract()

        m_cf.rCoordAbsXMin = -rZoomDef : m_cf.rCoordAbsXMax = rZoomDef
        m_cf.rCoordAbsYMin = -rZoomDef : m_cf.rCoordAbsYMax = rZoomDef

        ' Test Bug QuadTree 
        Dim iNbIterationsMaxDepartDef0% = iNbIterationsMaxDepartDef
        If bDebugBugQuad Then
            m_cf.rCoordAbsXMin = -0.5D
            m_cf.rCoordAbsXMax = 0.1D
            m_cf.rCoordAbsYMin = -0.5D
            m_cf.rCoordAbsYMax = 0.5D
        End If

        RespecterRatioZoneAbs()
        InitNivIter()
        m_prm.typeFract = typeFractDef
        m_prm.rRe = CDec(ptfJuliaDef.X)
        m_prm.rIm = CDec(ptfJuliaDef.Y)
        m_prm.iDegre = iDegreAlgoDef
        m_prm.iNbIterationsMaxDepart = iNbIterationsMaxDepartDef0
        m_iNbIterationsMin = 0
        m_bEffacerImg = m_bEffacerImgDef

        CalculerNbCouleurs()
        If Not m_prmPalette.bPaletteSysteme Then InitPaletteCalc()

        m_bZoomMoins = False ' 25/01/2015

    End Sub

    Public Function rLirePointJuliaX() As Decimal
        Return m_prm.rRe
    End Function
    Public Function rLirePointJuliaY() As Decimal
        Return m_prm.rIm
    End Function

    Public Overridable Sub InitConfig()
    End Sub

    Public Overridable Sub InitPalette()
    End Sub

    Public Sub InitialiserIterations()
        m_iNbIterationsMin = 0
    End Sub

    Public Sub ZoomerZonePixels(m_rectCoordPixels As Rectangle)

        ' Calcul des nouvelles coordonnées absolues
        Dim rNewCoordAbsXMin As Decimal =
            CDec(m_rectCoordPixels.Left / m_szTailleEcran.Width)
        Dim rNewCoordAbsYMin As Decimal =
            CDec(m_rectCoordPixels.Top / m_szTailleEcran.Height)
        Dim rNewCoordAbsXMax As Decimal =
            CDec((m_rectCoordPixels.Left + m_rectCoordPixels.Width) /
                m_szTailleEcran.Width)
        Dim rNewCoordAbsYMax As Decimal =
            CDec((m_rectCoordPixels.Top + m_rectCoordPixels.Height) /
                m_szTailleEcran.Height)

        ' Ancienne amplitude absolue
        Dim rAncAmplitX As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim rAncAmplitY As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin
        m_cf.rCoordAbsXMax = m_cf.rCoordAbsXMin + rNewCoordAbsXMax * rAncAmplitX
        m_cf.rCoordAbsYMax = m_cf.rCoordAbsYMin + rNewCoordAbsYMax * rAncAmplitY
        m_cf.rCoordAbsXMin = m_cf.rCoordAbsXMin + rNewCoordAbsXMin * rAncAmplitX
        m_cf.rCoordAbsYMin = m_cf.rCoordAbsYMin + rNewCoordAbsYMin * rAncAmplitY

        RespecterRatioZoneAbs()

    End Sub

    Public Sub RespecterRatioZoneAbs()

        ' Attention : il faut conserver le ratio quelque soit celui de l'écran

        If m_szTailleEcran.Height >= m_szTailleEcran.Width Then ' Ratio <=1
            If m_szTailleEcran.Width <> 0 Then

                ' Centre du zoom
                Dim rCentreY0 As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
                ' Ratio inverse : Hauteur sur Largeur : si le ratio normal est de 0.5
                '  alors le ratio inverse est de 2
                Dim rRatioEcranHSurL As Decimal = CDec(m_szTailleEcran.Height / m_szTailleEcran.Width)
                Dim rDepY As Decimal = (m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin) * rRatioEcranHSurL
                m_cf.rCoordAbsYMin = rCentreY0 - rDepY / 2
                m_cf.rCoordAbsYMax = rCentreY0 + rDepY / 2

            End If
        Else ' Ratio >1
            If m_szTailleEcran.Height <> 0 Then

                ' Centre du zoom
                Dim rCentreX0 As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
                Dim rRatioEcran As Decimal = CDec(m_szTailleEcran.Width / m_szTailleEcran.Height)
                Dim rDepX As Decimal = (m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin) * rRatioEcran
                m_cf.rCoordAbsXMin = rCentreX0 - rDepX / 2
                m_cf.rCoordAbsXMax = rCentreX0 + rDepX / 2

            End If
        End If

    End Sub

    Public Sub ZoomerFacteur(rFacteurZoom As Decimal, bZoomMoins As Boolean)

        If rFacteurZoom = 1D Then Exit Sub

        ' Centre du zoom
        Dim rCentreX As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Dim rCentreY As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2

        ' Amplitude actuelle du zoom
        Dim rAmplitX0 As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim rAmplitY0 As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Reculer le zoom
        m_cf.rCoordAbsXMin = rCentreX - rAmplitX0 * rFacteurZoom / 2
        m_cf.rCoordAbsXMax = rCentreX + rAmplitX0 * rFacteurZoom / 2
        m_cf.rCoordAbsYMin = rCentreY - rAmplitY0 * rFacteurZoom / 2
        m_cf.rCoordAbsYMax = rCentreY + rAmplitY0 * rFacteurZoom / 2

        If bZoomMoins Then
            ' Diminuer le nombre d'itération minimum
            Dim W As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
            Dim rLogW# = Math.Log10(W)
            m_iNbIterationsMin = iTrouverNivIter%(rLogW)
            Debug.WriteLine("Nb. itérations trouvé : " & rLogW & " -> " & m_iNbIterationsMin)

            m_bZoomMoins = True ' 21/01/2015
        Else
            m_bZoomMoins = False ' 21/01/2015
        End If

    End Sub

    Public Sub Zoomer(rFacteurZoom!)

        ' Centre du zoom
        Dim rCentreX As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Dim rCentreY As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2

        ' Amplitude actuelle du zoom
        Dim rAmplitX As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim rAmplitY As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Zoomer
        Dim rFact As Decimal = CDec(rFacteurZoom)
        m_cf.rCoordAbsXMin = Me.rCentreX - rAmplitX * rFact / 2
        m_cf.rCoordAbsXMax = Me.rCentreX + rAmplitX * rFact / 2
        m_cf.rCoordAbsYMin = Me.rCentreY - rAmplitY * rFact / 2
        m_cf.rCoordAbsYMax = Me.rCentreY + rAmplitY * rFact / 2

    End Sub

    Public Sub Deplacer(rDepRelatifX!, rDepRelatifY!)

        ' Amplitude actuelle du zoom
        Dim rAmplitX As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim rAmplitY As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Déplacement asbolu
        Dim rDepX As Decimal = CDec(rDepRelatifX) * rAmplitX
        Dim rDepY As Decimal = CDec(rDepRelatifY) * rAmplitY

        m_cf.rCoordAbsXMin += rDepX
        m_cf.rCoordAbsXMax += rDepX
        m_cf.rCoordAbsYMin += rDepY
        m_cf.rCoordAbsYMax += rDepY

    End Sub

    Public Sub DeplacerPtJulia(rDepRelatifX!, rDepRelatifY!)

        m_prm.rRe += CDec(rDepRelatifX)
        m_prm.rIm += CDec(rDepRelatifY)

    End Sub

    Public Sub FixerAnglePtJulia(rDepRelatifAngle!)

        m_prm.rAngle += CDec(rDepRelatifAngle)

    End Sub

    Public Sub FixerAngleZoomPtJulia(rDepRelatifAngle!)

        m_prm.rAngleZoom += CDec(rDepRelatifAngle)

    End Sub

    Public Function rLireAngleZoomJulia() As Decimal
        Return m_prm.rAngleZoom
    End Function

    Public Sub TournerPtJulia()

        m_prm.rRayon = 1.1D
        m_prm.rRe = m_prm.rRayon * CDec(Math.Cos(m_prm.rAngle + Math.PI / 4))
        m_prm.rIm = m_prm.rRayon * CDec(Math.Sin(m_prm.rAngle + Math.PI / 4))

    End Sub

    Public Sub InitPtJulia()

        m_prm.rRe = 0
        m_prm.rIm = 0

    End Sub

    Public Sub ViserPoint(rCentreX0 As Decimal, rCentreY0 As Decimal,
        rZoomDepart As Decimal, iNbIter%, rZoomCible As Decimal)

        ' Définir la vue via une cible et un facteur de zoom

        Dim rRatioEcran As Decimal = CDec(m_szTailleEcran.Width / m_szTailleEcran.Height)
        Dim rDemiAmplitX As Decimal = rZoomDepart * rRatioEcran / 2
        Dim rDemiAmplitY As Decimal = rZoomDepart / 2
        m_cf.rCoordAbsXMin = rCentreX0 - rDemiAmplitX
        m_cf.rCoordAbsXMax = rCentreX0 + rDemiAmplitX
        m_cf.rCoordAbsYMin = rCentreY0 - rDemiAmplitY
        m_cf.rCoordAbsYMax = rCentreY0 + rDemiAmplitY
        m_iNbIterationsMin = iNbIter
        m_iNbIterationsMax = iNbIter + iNbIterationsMaxDepart
        m_cf.rZoomCible = rZoomCible

    End Sub

    Public Sub DefinirCible(rFacteurZoom!)

        ' Centre du zoom
        Dim rCentreX As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Dim rCentreY As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2

        ' Amplitude actuelle du zoom
        Dim rAmplitX As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim rAmplitY As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin

        ' Reculer le zoom
        Dim rFacteurZoomDec As Decimal = CDec(rFacteurZoom)
        m_cf.rCoordAbsXMin = rCentreX - rAmplitX * rFacteurZoomDec / 2
        m_cf.rCoordAbsXMax = rCentreX + rAmplitX * rFacteurZoomDec / 2
        m_cf.rCoordAbsYMin = rCentreY - rAmplitY * rFacteurZoomDec / 2
        m_cf.rCoordAbsYMax = rCentreY + rAmplitY * rFacteurZoomDec / 2

    End Sub

    Protected Sub InitTracerFractDepart()

        Me.m_bPause = False
        Me.m_bQuitterTrace = False
        Me.m_bModeDetailIterations = False

    End Sub

    Protected m_bmpCache As Bitmap
    Public Overridable Sub TracerFractDepart(bmpCache As Bitmap)
        Me.m_bmpCache = bmpCache
        InitTracerFractDepart()
        TracerFractProgressif()
    End Sub

    Protected Overridable Sub TracerFractProgressif()

        ' iNbIterationsMax dépend du nombre d'itérations min. précédant :
        '  cela évite de le définir trop élevé dès le début, alors que 
        '  c'est seulement pour un zoom profond que l'on a besoin de 
        '  beaucoup d'itérations

        'Debug.WriteLine("m_iNbIterationsMin = " & m_iNbIterationsMin)
        'Debug.WriteLine("m_iNbIterationsMax = " & m_iNbIterationsMax)
        'Debug.WriteLine("m_iNbIterationsMaxDepart = " & m_prm.iNbIterationsMaxDepart)

        m_iMemNbIterationsMin = m_iNbIterationsMin
        ' 25/01/2015 En mode zoom - ne pas augmenter le nombre d'itération
        If Not m_bZoomMoins AndAlso m_iNbIterationsMin < iIntegerMax Then
            ' m_iNbIterationsMaxCible 
            Dim iNouvMax% = m_prm.iNbIterationsMaxDepart + m_iNbIterationsMin
            ' 25/01/2015 En mode cible, ne pas redescendre en dessous de la cible
            m_iNbIterationsMax = iNouvMax
        End If
        m_bZoomMoins = False

        m_iNbIterationsMin = iIntegerMax

        InitialiserTracerFract()

        Dim W As Decimal = m_cf.rCoordAbsXMax - m_cf.rCoordAbsXMin
        Dim H As Decimal = m_cf.rCoordAbsYMax - m_cf.rCoordAbsYMin
        Dim rLogW# = Math.Log10(W)
        Dim rLogH# = Math.Log10(H)
        'Debug.WriteLine("Nb iter.= " & m_iMemNbIterationsMin & " -> " & m_iNbIterationsMax & _
        '    ", " & W & ", " & H & ", " & rLogW & ", " & rLogH)
        'Debug.WriteLine("Ajout : " & rLogW & ", " & m_iMemNbIterationsMin)
        AjouterNivIter(New clsCoupleLogIter(rLogW, m_iMemNbIterationsMin))

        ' Pour algo. rapide
        ' -----------------
        Dim width% = m_szTailleEcran.Width
        Dim heigth% = m_szTailleEcran.Height
        Dim X2 As Decimal = (m_cf.rCoordAbsXMax + m_cf.rCoordAbsXMin) / 2
        Dim Y2 As Decimal = (m_cf.rCoordAbsYMax + m_cf.rCoordAbsYMin) / 2
        Dim xs As Decimal = X2 - (W / 2)
        Dim ys As Decimal = Y2 - (H / 2)
        m_prm.rXd = W / CDec(width)
        m_prm.rYd = H / CDec(heigth)
        ' -----------------

        ' Pour algo rapide, pas besoin d'effacer
        If m_bEffacerImg Then
            m_gr.Clear(couleurFondCyan)
            RaiseEvent EvMajBmp()
        End If

        Dim iPas% = iPasMax
        Do
            InitCoordFract(m_cf, iPas)

            ' Pour cacher les gros pixels hors zone
            If m_bEffacerImg Then m_gr.Clear(couleurFondCyan)

            TracerFract(iPas)

            If m_bQuitterTrace Then GoTo Fin

            iPas \= 2 ' \ : Antislash = Division entière
        Loop While iPas >= iPasMin

Fin:
        RaiseEvent EvFinTrace()

    End Sub

    Public Sub FinTrace()
        RaiseEvent EvFinTrace()
    End Sub

    Protected Overridable Sub InitialiserTracerFract()
        ' Utile pour initialiser les classes dérivées
    End Sub

    Protected Function bQuitter() As Boolean

        ' Traiter les messages (par ex. tracé d'un rectangle de sélection)
        Application.DoEvents()
        While Me.m_bPause
            Application.DoEvents()
            If Me.m_bQuitterTrace Then bQuitter = True : Exit Function
        End While
        If Me.m_bQuitterTrace Then bQuitter = True

    End Function

    Protected Overridable Sub TracerFract(iPas%)

        Dim penPixel As New Pen(Color.Black, 1)

        For iPaveY As Integer = 0 To m_cf.iPaveMaxY

            ' Pas possible avec Drawing.Graphics
            'Parallel.For(0, m_cf.iPaveMaxY, Sub(iPaveY)

            m_cf.rYAbs = (iPaveY + 0.5D) * m_cf.rHautPaveAbs + m_cf.rCoordAbsYMin

            For iPaveX As Integer = 0 To m_cf.iPaveMaxX

                If bQuitter() Then Exit Sub

                m_cf.rXAbs = (iPaveX + 0.5D) * m_cf.rLargPaveAbs + m_cf.rCoordAbsXMin
                Dim bFrontiere As Boolean = False
                Dim iNbIterations% = iCompterIterations(m_cf.rXAbs, m_cf.rYAbs,
                    iPaveX, iPaveY, iPas%, m_cf, bFrontiere)

                Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
                If bInterpoler AndAlso bFrontiere Then
                    If iPas > 1 Then
                        m_remplissage.Color = m_prm.coulInterpolee
                    Else
                        penPixel.Color = m_prm.coulInterpolee
                    End If
                Else
                    If iPas > 1 Then
                        m_remplissage.Color = CouleurPalette(iNbIterations, bFrontiere)
                    Else
                        penPixel.Color = CouleurPalette(iNbIterations, bFrontiere)
                    End If
                End If

                ' Si le pavé est de 1 pixel, un PixelSet (PSet) serait plus rapide
                '  qu'un Rectangle plein, mais PixelSet n'existe pas en VB .Net :
                '  il est probablement implémenté dans FillRectangle()
                If iPas > 1 Then
                    m_gr.FillRectangle(m_remplissage,
                        m_cf.iMargeX + iPaveX * iPas,
                        m_cf.iMargeY + iPaveY * iPas, iPas, iPas)
                Else
                    ' Pas de gain de temps constaté
                    m_gr.DrawLine(penPixel,
                        m_cf.iMargeX + iPaveX, m_cf.iMargeY + iPaveY,
                        m_cf.iMargeX + iPaveX + 1, m_cf.iMargeY + iPaveY)
                End If

                If m_bQuitterTrace Then Exit Sub

            Next iPaveX

            If m_szTailleEcran.Height > 1 Then
                Dim iAvancement% = CInt(100 * iPaveY / m_cf.iPaveMaxY)
                RaiseEvent EvMajAvancement(iAvancement)
            End If

            RaiseEvent EvMajBmp()

        Next iPaveY

    End Sub

    Public Sub CalculerNbCouleurs()

        If m_prmPalette.bPaletteSysteme Then
            m_iNbCouleurs = m_prmPalette.iNbCouleurs
            ReDim m_aiCouleurs%(0)
        Else
            m_iNbCouleurs = iNbCouleursPalette \ m_prmPalette.iNbCyclesDegrade
            ReDim m_aiCouleurs%(iNbCouleursReservees + m_iNbCouleurs)
        End If

    End Sub

    Public Sub InitPaletteCalc()

        m_aiCouleurs(iCodePixelNonExamine) = kcCouleurPixelNonExamine ' KnownColor.White
        m_aiCouleurs(iCodePixelFrontiere) = kcCouleurPixelFrontiere  ' KnownColor.LightGreen
        Dim iNumCouleur% = 0
        Dim iNumCouleurZone% = -1
        Dim iZonePalette% = 0
        Dim iTailleZonePalette% = m_iNbCouleurs \ m_prmPalette.iNbCyclesDegrade
        If iTailleZonePalette = 0 Then
            iTailleZonePalette = 1
        End If
        Dim iPas% = m_prmPalette.iNbCyclesDegrade

        Dim lst As List(Of Integer) = Nothing
        If m_prmPalette.bPaletteAleatoire Then lst = New List(Of Integer)

        Dim ht As New Dictionary(Of Integer, Integer)

        For i As Integer = 0 To iNbCouleursPalette - 1 Step iPas

            Dim colorValueR% = 0
            Dim colorValueG% = 0
            Dim colorValueB% = 0

            If i >= 1020 Then
                colorValueR = i - 1020 + 1
                colorValueG = colorValueR
                colorValueB = colorValueR
            ElseIf i >= 766 Then
                colorValueR = 255 + (766 - i) - 1
                colorValueG = 0
                colorValueB = 0
            ElseIf i >= 511 Then
                colorValueR = i - 511 + 1
                colorValueG = 255 - colorValueR
            ElseIf i >= 256 Then
                colorValueG = i - 255
                colorValueB = 255 - colorValueG
            Else
                colorValueB = i
            End If
            Dim color00 As Color = Color.FromArgb(colorValueR, colorValueG, colorValueB)
            Dim iCouleur% = color00.ToArgb

            If m_prmPalette.bPaletteAleatoire Then
                lst.Add(iCouleur)
            Else

                iZonePalette = iNumCouleur Mod m_prmPalette.iNbCyclesDegrade
                If iZonePalette = 0 Then iNumCouleurZone += 1
                Dim iIndice% = iNbCouleursReservees + iNumCouleurZone + iZonePalette * iTailleZonePalette
                'Debug.WriteLine(iNumCouleur.ToString("000") & " : " & iIndice.ToString("000") & _
                '    " -> " & iCouleur.ToString("000000000") & " R" & colorValueR.ToString("000") & _
                '    " G" & colorValueG.ToString("000") & " B" & colorValueB.ToString("000"))
                m_aiCouleurs(iIndice) = iCouleur

            End If

            iNumCouleur += 1

        Next i

        If m_prmPalette.bPaletteAleatoire Then

            Dim iIndice% = iNbCouleursReservees
            Do While lst.Count > 0
                Dim iNumColMax% = lst.Count - 1
                Dim iNumCol% = iRandomiser(0, iNumColMax)
                Dim iCouleur% = lst(iNumCol)
                lst.RemoveAt(iNumCol)
                m_aiCouleurs(iIndice) = iCouleur
                iIndice += 1
            Loop
        Else

            ' Vérification de la répartition
            For iNbIterations As Integer = 0 To iNumCouleur - 1
                Dim iCouleurFinale% = m_aiCouleurs(iNbIterations + iNbCouleursReservees)
                If Not ht.ContainsKey(iCouleurFinale) Then
                    ht.Add(iCouleurFinale, iNbIterations)
                Else

                    If iCouleurFinale = 0 Then
                        Debug.WriteLine("Couleur vide ! " & iCouleurFinale & " : " & iNbIterations)
                        'If bDebug Then Stop
                    End If

                    Dim iNbIterations0% = ht(iCouleurFinale)
                    If iNbIterations0 <> iNbIterations Then
                        Debug.WriteLine("Collision palette ! " & iCouleurFinale & " : " & iNbIterations0 & "<>" & iNbIterations)
                        'If bDebug Then Stop
                    End If
                End If
            Next

        End If

        'Debug.WriteLine("Nombre final de couleurs : " & iNumCouleur)

    End Sub

    Public Function CouleurPalette(iNbIterations%, bFrontiere As Boolean) As Color

        ' Détermination de la couleur de la palette standard
        '  à partir du modulo iCouleurMax

        Dim couleur As Color
        Dim iIndiceCouleur%
        If iNbIterations = iCodePixelNonExamine Then
            iIndiceCouleur = iCodePixelNonExamine
            Dim iCouleurFinaleKC% = kcCouleurPixelNonExamine 'KnownColor.White
            Dim kc As KnownColor = CType(iCouleurFinaleKC, Drawing.KnownColor)
            couleur = Color.FromKnownColor(kc)
        ElseIf bFrontiere Then 'iNbIterations = iCodePixelFrontiere Then
            iIndiceCouleur = iCodePixelFrontiere
            Dim iCouleurFinaleKC% = kcCouleurPixelFrontiere 'KnownColor.LightGreen
            Dim kc As KnownColor = CType(iCouleurFinaleKC, Drawing.KnownColor)
            couleur = Color.FromKnownColor(kc)
        Else

            iIndiceCouleur = iNbCouleursReservees +
                (m_prmPalette.iPremCouleur + iNbIterations) Mod m_iNbCouleurs

            If Not m_prmPalette.bPaletteSysteme Then
                Dim iCouleurFinale0% = m_aiCouleurs(iIndiceCouleur)
                couleur = Color.FromArgb(iCouleurFinale0)
            Else
                Dim kc As KnownColor = CType(iIndiceCouleur, Drawing.KnownColor)
                couleur = Color.FromKnownColor(kc)
            End If

        End If

        Dim iCouleurFinale% = couleur.ToArgb
        'Debug.WriteLine(iNbIterations & " -> " & iIndiceCouleur & " (" & iCouleurFinale & ")")

        ' Vérifier ssi le nombre d'itération est < au nombre de couleurs de la palette
        ' (sinon c'est normale d'avoir des colisions)
        'Static hs As New HashSet(Of Integer)
        'If Not hs.Contains(iNbIterations) Then
        '    hs.Add(iNbIterations)
        '    Debug.WriteLine(iNbIterations & " -> " & iIndiceCouleur & " (" & iCouleurFinale & ")")
        'End If
        'Static ht As New Dictionary(Of Integer, Integer)
        'If Not ht.ContainsKey(iNbIterations) Then
        '    ht.Add(iNbIterations, iCouleurFinale)
        'Else
        '    Dim iCouleurFinale0% = ht(iNbIterations)
        '    If iCouleurFinale0 <> iCouleurFinale Then
        '        Debug.WriteLine("Collision palette !")
        '        Stop
        '    End If
        'End If

        Return couleur

    End Function

    Protected Function iCalculerCouleur%(iNbIter%, bFrontiere As Boolean)

        Dim iIndiceCoul1%

        Const bAfficherFontiere As Boolean = True
        If bFrontiere AndAlso Not bAfficherFontiere Then
            ' Si on n'affiche pas la frontière, alors on applique le modulo même sur la frontière
            ' Mais comme on augmente le nombre d'itération max. de façon à ce que
            '  le nombre d'itération max. - min. soit constant, du coup la couleur max. n'est pas stable
            ' Conclusion : mieux vaux afficher une couleur de fontrière fixe
            iIndiceCoul1 = iNbCouleursReservees + (iNbIter Mod m_iNbCouleurs)
            Return iIndiceCoul1
        End If

        If bFrontiere Then
            iIndiceCoul1 = iCodePixelFrontiere
        Else
            iIndiceCoul1 = iNbCouleursReservees + (iNbIter Mod m_iNbCouleurs)
        End If
        Return iIndiceCoul1

    End Function

    Protected Function InterpolateColors%(s1%, s2%, weigth%)

        Dim c1 As Color = Color.FromArgb(s1)
        Dim c2 As Color = Color.FromArgb(s2)

        Dim lRed0& = (CLng(c2.R) - CLng(c1.R)) * weigth
        Dim lGreen0& = (CLng(c2.G) - CLng(c1.G)) * weigth
        Dim lBlue0& = (CLng(c2.B) - CLng(c1.B)) * weigth

        Dim lRed& = CLng(c1.R) + (lRed0 >> 8)
        Dim lGreen& = CLng(c1.G) + (lGreen0 >> 8)
        Dim lBlue& = CLng(c1.B) + (lBlue0 >> 8)

        Dim red As Byte = CByte(lRed And &HFF)
        Dim green As Byte = CByte(lGreen And &HFF)
        Dim blue As Byte = CByte(lBlue And &HFF)

        Return Color.FromArgb(red, green, blue).ToArgb

        'int InterpolateColors(int s1, int s2, int weigth){
        '    Color c1 = Color.FromArgb(s1); Color c2 = Color.FromArgb(s2);
        '    byte red   = (byte)(((int)c1.R + ((int)((c2.R - c1.R) * weigth) >> 8)) & 0xff);
        '    byte green = (byte)(((int)c1.G + ((int)((c2.G - c1.G) * weigth) >> 8)) & 0xff);
        '    byte blue  = (byte)(((int)c1.B + ((int)((c2.B - c1.B) * weigth) >> 8)) & 0xff);
        '    return Color.FromArgb(red, green, blue).ToArgb(); }

    End Function

    Public Sub SelectionnerPoint(pt As Point)
        InitCoordFract(iPasMin, pt)
        Dim bFrontiere As Boolean = False
        iCompterIterations(m_cf.rXAbs, m_cf.rYAbs, pt.X, pt.Y, iPasMin, m_cf, bFrontiere)
        RaiseEvent EvDetailIterations(m_oDetailIter.aptLirePoint)
    End Sub

    Private Sub InitCoordFract(iPas%, pt As Point)
        InitCoordFract(m_cf, iPas)
        m_cf.rXAbs = (pt.X \ iPas) * m_cf.rLargPaveAbs + m_cf.rCoordAbsXMin
        m_cf.rYAbs = (pt.Y \ iPas) * m_cf.rHautPaveAbs + m_cf.rCoordAbsYMin
    End Sub

    Protected Sub InitCoordFract(ByRef cf As TCoordFract, iPas%)

        cf.iPaveMaxX = m_szTailleEcran.Width \ iPas - 1
        cf.iPaveMaxY = m_szTailleEcran.Height \ iPas - 1
        cf.iMargeX = (m_szTailleEcran.Width - (cf.iPaveMaxX + 1) * iPas) \ 2
        cf.iMargeY = (m_szTailleEcran.Height - (cf.iPaveMaxY + 1) * iPas) \ 2
        cf.rLargPaveAbs = (cf.rCoordAbsXMax - cf.rCoordAbsXMin) / (cf.iPaveMaxX + 1)
        cf.rHautPaveAbs = (cf.rCoordAbsYMax - cf.rCoordAbsYMin) / (cf.iPaveMaxY + 1)

        'Debug.WriteLine("Pos.XYMinMax : " & cf.rCoordAbsXMin & ", " & cf.rCoordAbsXMax & ", " & _
        '                cf.rCoordAbsYMin & ", " & cf.rCoordAbsYMax)

    End Sub

    Protected Function iCompterIterations%(rX As Decimal, rY As Decimal,
        iPaveX%, iPaveY%, iPas%, cf As TCoordFract, ByRef bFrontiere As Boolean)

        ' Nombre complexe Z = a + ib avec i*i = -1 
        ' Equation : Z -> Z^degré + C

        If m_bDecimal Then
            Return iCompterIterations_Decimal(rX, rY,
                iPaveX, iPaveY, iPas, cf, bFrontiere)
        End If

        bFrontiere = False
        Dim iNbIterations% = 0
        Dim a, b, a2, b2, mem_a, mem_b As Double
        Dim rX1, rY1, r1, i1, r1pow2, i1pow2, rpow, rlastpow, rCount_f, rFactor As Double

        If m_prm.typeFract = TFractal.Mandelbrot Then
            m_prm.rRe = rX
            m_prm.rIm = rY
            a = 0D
            b = 0D
        Else ' Julia
            a = rX
            b = rY
            iNbIterations = 1
        End If

        If m_bModeDetailIterations Then m_oDetailIter.Initialiser()

        Dim bJulia0 As Boolean = False
        If m_prm.typeFract = TFractal.Julia Then bJulia0 = True

        Select Case m_prm.iDegre
            Case 2

                ' 2x plus rapide s'il y a 10000 itérations (mais sinon faible gain)
                If m_bAlgoRapide Then
                    rX1 = m_prm.rXd * iPaveX * iPas + cf.rCoordAbsXMin
                    rY1 = m_prm.rYd * iPaveY * iPas + cf.rCoordAbsYMin
                    r1 = 0
                    i1 = 0
                    r1pow2 = 0
                    i1pow2 = 0
                    If bJulia0 Then
                        a = m_prm.rRe
                        b = m_prm.rIm
                        r1 = rX1
                        i1 = rY1
                        r1pow2 = r1 * r1
                        i1pow2 = i1 * i1
                    End If
                    rpow = 0
                    rlastpow = 0
                    Do While iNbIterations <= m_iNbIterationsMax AndAlso rpow < 4
                        r1pow2 = r1 * r1
                        i1pow2 = i1 * i1
                        If bJulia0 Then
                            i1 = 2 * i1 * r1 + b
                            r1 = r1pow2 - i1pow2 + a
                        Else
                            i1 = (2 * i1) * r1 + rY1
                            r1 = r1pow2 - i1pow2 + rX1
                        End If
                        rlastpow = rpow
                        rpow = r1pow2 + i1pow2
                        iNbIterations += 1
                    Loop
                    If iNbIterations >= m_iNbIterationsMax Then bFrontiere = True
                    Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
                    If bInterpoler AndAlso bFrontiere Then
                        rCount_f = iNbIterations - 1 + (4 - rlastpow) / (rpow - rlastpow)
                        rFactor = (1D - (iNbIterations - rCount_f)) * 255
                        Dim iFactor% = 0
                        If rFactor >= Integer.MinValue AndAlso
                        rFactor <= Integer.MaxValue Then iFactor = CInt(rFactor)
                        Dim iCoul1% = iCalculerCouleur(iNbIterations - 1, bFrontiere:=False)
                        Dim iCoul2% = iCalculerCouleur(iNbIterations, bFrontiere:=False)
                        Dim iCoul% = InterpolateColors(iCoul1, iCoul2, iFactor)
                        m_prm.coulInterpolee = Color.FromArgb(iCoul)
                    End If

                Else
                    Do
                        If m_bModeDetailIterations Then _
                        m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                        a2 = a * a
                        b2 = b * b
                        ' On sort du cercle unitaire
                        If a2 + b2 > 4 Then Exit Do
                        b = 2 * a * b + m_prm.rIm
                        a = a2 - b2 + m_prm.rRe
                        iNbIterations += 1
                    Loop While iNbIterations <= m_iNbIterationsMax
                End If

            Case 3
                Do
                    If m_bModeDetailIterations Then _
                    m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    mem_a = a
                    mem_b = b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = mem_a * a
                    b2 = mem_b * b
                    b = mem_a * b + mem_b * a + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax

            Case 4
                Do
                    If m_bModeDetailIterations Then _
                    m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = a * a
                    b2 = b * b
                    b = 2 * a * b + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax

            Case 5
                Do
                    If m_bModeDetailIterations Then _
                    m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    mem_a = a : mem_b = b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = a * a
                    b2 = b * b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = mem_a * a
                    b2 = mem_b * b
                    b = mem_a * b + mem_b * a + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax
        End Select

        If iNbIterations >= m_iNbIterationsMax Then bFrontiere = True
        If iNbIterations < m_iNbIterationsMin Then
            m_iNbIterationsMin = iNbIterations
            'Debug.WriteLine("-> m_iNbIterationsMin = " & m_iNbIterationsMin)
        End If

        Return iNbIterations

    End Function

    Private Function iCompterIterations_Decimal%(rX As Decimal, rY As Decimal,
        iPaveX%, iPaveY%, iPas%, cf As TCoordFract, ByRef bFrontiere As Boolean)

        ' Nombre complexe Z = a + ib avec i*i = -1 
        ' Equation : Z -> Z^degré + C

        bFrontiere = False
        Dim iNbIterations% = 0
        Dim a, b, a2, b2, mem_a, mem_b As Decimal
        Dim rX1, rY1, r1, i1, r1pow2, i1pow2, rpow, rlastpow, rCount_f, rFactor As Decimal

        If m_prm.typeFract = TFractal.Mandelbrot Then
            m_prm.rRe = rX
            m_prm.rIm = rY
            a = 0D
            b = 0D
        Else ' Julia
            a = rX
            b = rY
            iNbIterations = 1
        End If

        If m_bModeDetailIterations Then m_oDetailIter.Initialiser()

        Select Case m_prm.iDegre
            Case 2

                ' 2x plus rapide s'il y a 10000 itérations (mais sinon faible gain)
                If m_bAlgoRapide Then
                    rX1 = m_prm.rXd * iPaveX * iPas + cf.rCoordAbsXMin
                    rY1 = m_prm.rYd * iPaveY * iPas + cf.rCoordAbsYMin
                    r1 = 0
                    i1 = 0
                    If m_prm.typeFract = TFractal.Julia Then
                        a = m_prm.rRe
                        b = m_prm.rIm
                        r1 = rX1
                        i1 = rY1
                    End If
                    r1pow2 = 0
                    i1pow2 = 0
                    rpow = 0
                    rlastpow = 0
                    Do While iNbIterations <= m_iNbIterationsMax AndAlso rpow < 4
                        r1pow2 = r1 * r1
                        i1pow2 = i1 * i1
                        i1 = (2 * i1) * r1 + rY1 + b
                        r1 = r1pow2 - i1pow2 + rX1 + a
                        rlastpow = rpow
                        rpow = r1pow2 + i1pow2
                        iNbIterations += 1
                    Loop
                    If iNbIterations >= m_iNbIterationsMax Then bFrontiere = True
                    Dim bInterpoler As Boolean = Not m_prmPalette.bFrontiereUnie
                    If bInterpoler AndAlso bFrontiere Then
                        rCount_f = iNbIterations - 1 + (4 - rlastpow) / (rpow - rlastpow)
                        rFactor = (1D - (iNbIterations - rCount_f)) * 255
                        Dim iFactor% = 0
                        If rFactor >= Integer.MinValue AndAlso
                        rFactor <= Integer.MaxValue Then iFactor = CInt(rFactor)
                        Dim iCoul1% = iCalculerCouleur(iNbIterations - 1, bFrontiere:=False)
                        Dim iCoul2% = iCalculerCouleur(iNbIterations, bFrontiere:=False)
                        Dim iCoul% = InterpolateColors(iCoul1, iCoul2, iFactor)
                        m_prm.coulInterpolee = Color.FromArgb(iCoul)
                    End If

                Else
                    Do
                        If m_bModeDetailIterations Then _
                            m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                        a2 = a * a
                        b2 = b * b
                        ' On sort du cercle unitaire
                        If a2 + b2 > 4 Then Exit Do
                        b = 2 * a * b + m_prm.rIm
                        a = a2 - b2 + m_prm.rRe
                        iNbIterations += 1
                    Loop While iNbIterations <= m_iNbIterationsMax
                End If

            Case 3
                Do
                    If m_bModeDetailIterations Then _
                        m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    mem_a = a
                    mem_b = b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = mem_a * a
                    b2 = mem_b * b
                    b = mem_a * b + mem_b * a + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax

            Case 4
                Do
                    If m_bModeDetailIterations Then _
                        m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = a * a
                    b2 = b * b
                    b = 2 * a * b + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax

            Case 5
                Do
                    If m_bModeDetailIterations Then _
                        m_oDetailIter.AjouterPointDetailIterations(a, b, m_cf)
                    a2 = a * a
                    b2 = b * b
                    If a2 + b2 > 4 Then Exit Do
                    mem_a = a : mem_b = b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = a * a
                    b2 = b * b
                    b = 2 * a * b
                    a = a2 - b2
                    a2 = mem_a * a
                    b2 = mem_b * b
                    b = mem_a * b + mem_b * a + m_prm.rIm
                    a = a2 - b2 + m_prm.rRe
                    iNbIterations += 1
                Loop While iNbIterations <= m_iNbIterationsMax
        End Select

        If iNbIterations >= m_iNbIterationsMax Then bFrontiere = True
        If iNbIterations < m_iNbIterationsMin Then m_iNbIterationsMin = iNbIterations
        Return iNbIterations

    End Function

#End Region

End Class