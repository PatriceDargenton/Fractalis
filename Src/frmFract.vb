
' Fractalis : Traceur de fractales de type Mandelbrot et Julia
' ------------------------------------------------------------

' Fichier FrmFract.vb
' -------------------

' Conventions de nommage des variables :
' b pour Boolean (booléen vrai ou faux)
' i pour Integer (%) et Short (System.Int16)
' l pour Long : &
' r pour nombre Réel (Single!, Double# ou Decimal : D)
' a pour Array (tableau) : ()
' o pour Object (objet ou classe)
' m_ pour variable Membre de la classe (mais pas pour les constantes)

Public Class frmFractalis : Inherits Form

#Region "Configuration"

    ' Désactiver pour partir de la zone standard
    Private Const bDefinirCible As Boolean = False 'True

    ' 1 560 x 878 : Max. possible WinForm en 16/9 (alors que l'écran est en 1600 x 1900)
    ' 1 280 x 720
    ' Taille parfaite : 16/9
    ' https://support.google.com/youtube/answer/1722171?hl=fr
    Private Const rRatioImg! = 16 / 9 ''4 / 3 '1.5
    'Private Const rRatioImg! = 2.0
    'Private Const rRatioImg! = 1.0
    'Private Const rRatioImg! = 1.5
    'Private Const rRatioImg! = 0.5
    Private Const iTailleImgDebug% = 480 '1080 
    Private Const iTailleImgRelease% = 480 '1080

    ' Fabriquation d'une vidéo : mettre True ici (et presser v pour interrompre la vidéo)
    ' ------------------------------------------
    Private bVideo As Boolean = False
    Private Const iNbImg% = 100
    Private Const rIncJuliaRe! = 0
    Private Const rZoomVideo As Decimal = 0.9995D ' 1D pour désactivé
    Private Const bDeplacerPtJulia As Boolean = True
    Private Const sCarFinVideo$ = "v"
    Private Const sCheminAVI$ = "\Tmp\Test.avi" ' Chemin relatif où doit être créé la vidéo

    ' Debug de la création vidéo, le cas échéant
    Private Const bDebugCompress As Boolean = False
    Private Const iNumImgDepartDebug% = 4500
    Private Const bNumeroterImg As Boolean = False
    ' ------------------------------------------

    Private m_vdo As clsVideo
    Private m_iNumImg% = 0
    Private m_sCheminAVI$ = ""
    Private m_dTpsDebVideo As Date
    Private bDesactiverRafraichissementPdtVideo As Boolean = bVideo

#End Region

#Region "Déclarations"

    ' Déclaration de la feuille de configuration :
    '  avec la gestion des événements, il faut utiliser As au lieu de =
    'Private m_frmConfig = New frmConfig()
    Private WithEvents m_frmConfig As New frmConfig()

#Const iMethodeCalculSimple = 0
#Const iMethodeCalculRemplissage = 1
#Const iMethodeCalculQuadTree = 2 ' il fonctionne bien au centre de Mandelbrot, mais pas à l'extérieur de l'ensemble : il reste des gros pavés
#Const iMethodeCalculRapide = 3

#Const iMethodeCalcul = iMethodeCalculRapide
    '#Const iMethodeCalcul = iMethodeCalculQuadTree
    '#Const iMethodeCalcul = iMethodeCalculRemplissage
    '#Const iMethodeCalcul = iMethodeCalculSimple

#If iMethodeCalcul = iMethodeCalculSimple Then
    Private WithEvents m_clsFract As New ClsFract()
#ElseIf iMethodeCalcul = iMethodeCalculRemplissage Then
    Private WithEvents m_clsFract As New ClsFractRemplissage()
#ElseIf iMethodeCalcul = iMethodeCalculRapide Then
    Private WithEvents m_clsFract As New clsFractRapide()
#Else
    Private WithEvents m_clsFract As New ClsFractQuadTreeR()
#End If

    ' Ne pas commencer à tracer si la feuille n'est pas initialisée
    Private m_bInitApp As Boolean ' Application initialisée ?
    ' Suspendre le tracé quelque temps pendant le redimensionnement 
    '  (resize) de l'application
    Private m_bSuspendreTracePdtResize As Boolean
    Private m_bReTracer As Boolean ' Les paramètres ont changés : retracer
    Private m_gr As Graphics ' Graphique de la feuille

    Private m_szTailleEcran As New Size() ' Dimension du tracé en pixels
    ' Mémorisation pour retracer le bitmap au retour d'une iconisation
    Private m_szMemTailleEcran As New Size()

    ' Pour gérer le bitmap de cache (buffering) : c'est 15% + rapide
    Private m_bmpCache As Bitmap
    Private m_bTraceEnCours As Boolean

    ' Coordonnées en pixels dans l'ensemble de Mandelbrot ou Julia
    Private m_rectCoordPixels As New Rectangle()

    Private m_sTitreFrm$

#End Region

#Region " Windows Form Designer generated code "

    Public Sub New()

        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        DimensionnerFenetre()

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents TimerResize As System.Windows.Forms.Timer
    Friend WithEvents TimerVideo As System.Windows.Forms.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFractalis))
        Me.TimerResize = New System.Windows.Forms.Timer(Me.components)
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.TimerVideo = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'TimerResize
        '
        Me.TimerResize.Interval = 1000
        '
        'TimerVideo
        '
        Me.TimerVideo.Interval = 1
        '
        'frmFractalis
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(392, 392)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmFractalis"
        Me.Text = "Fractalis"
        Me.ToolTip1.SetToolTip(Me, "Bouton droit pour configurer (v pour vidéo)")
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Initialisations"

    Private Sub DimensionnerFenetre()

        ' Fonction appellée depuis le constructeur, après InitializeComponent()
        '  afin de fixer la taille intérieure de la fenêtre (Me.ClientRectangle)
        '  de façon à ce qu'elle corresponde exactement à la taille du bitmap
        '  qui va constituer la taille de la vidéo : ratio 16/9
        '  avec des tailles standards, par ex.: 1280 x 720 pixels

        Dim iTailleImg% = iTailleImgRelease
        If bDebug Then
            iTailleImg = iTailleImgDebug
            'Me.StartPosition = FormStartPosition.CenterScreen
            'Me.StartPosition = FormStartPosition.Manual
            'Me.Location = New System.Drawing.Point(10, 10)
        End If

        Dim iMarge% = 0

        'Debug.WriteLine("Taille demandée : " & iTailleImg & " : Ratio demandé = " & rRatioImg)

        Dim rRatioEcran As Decimal = 0
        If rRatioImg >= 1 Then

            Dim iLargTot% = CInt(iTailleImg * rRatioImg)

            Dim iInc% = 0
            Do ' Augmenter la hauteur jusqu'à la taille voulue
                Dim memSize As Drawing.Size = Me.Size
                Me.Size = New System.Drawing.Size(iLargTot + iInc, iTailleImg + iInc)
                'Dim x% = Me.ClientRectangle.Width
                Dim y% = Me.ClientRectangle.Height
                If y >= iTailleImg Then Exit Do
                ' On ne peut pas dépasser la taille de l'écran !
                If Me.Size = memSize Then Exit Do
                iInc += 1
            Loop While True

            Do ' Diminuer la largeur jusqu'au ratio voulu
                Me.Size = New System.Drawing.Size(iLargTot + iInc - iMarge, iTailleImg + iInc)
                Dim x% = Me.ClientRectangle.Width
                Dim y% = Me.ClientRectangle.Height
                rRatioEcran = CDec(x / y)
                'Debug.WriteLine("   iMarge=" & iMarge & " : ratio = " & rRatioEcran)
                If rRatioEcran <= rRatioImg Then Exit Do
                iMarge += 1
            Loop While True

        Else

            Dim iHautTot% = CInt(iTailleImg / rRatioImg)

            Dim iInc% = 0
            Do ' Augmenter la largeur jusqu'à la taille voulue
                Dim memSize As Drawing.Size = Me.Size
                Me.Size = New System.Drawing.Size(iTailleImg + iInc, iHautTot + iInc)
                Dim x% = Me.ClientRectangle.Width
                'Dim y% = Me.ClientRectangle.Height
                If x >= iTailleImg Then Exit Do
                ' On ne peut pas dépasser la taille de l'écran !
                If Me.Size = memSize Then Exit Do
                iInc += 1
            Loop While True

            Do ' Augmenter la hauteur jusqu'au ratio voulu
                Me.Size = New System.Drawing.Size(iTailleImg + iInc, iHautTot + iInc + iMarge)
                Dim x% = Me.ClientRectangle.Width
                Dim y% = Me.ClientRectangle.Height
                rRatioEcran = CDec(x / y)
                'Debug.WriteLine("   iMarge=" & iMarge & " : ratio = " & rRatioEcran)
                If rRatioEcran <= rRatioImg Then Exit Do
                iMarge += 1
            Loop While True
        End If

        'Debug.WriteLine(Me.ClientRectangle.Width & "x" & Me.ClientRectangle.Height & _
        '    " : Ratio = " & rRatioEcran)
        If bDebug Then
            'Me.StartPosition = FormStartPosition.CenterScreen
            Me.Location = New System.Drawing.Point(100, 100)
        End If

    End Sub

    Private Sub frmFractalis_FormClosing(sender As Object, e As FormClosingEventArgs) _
            Handles Me.FormClosing

        StopTrace()

        If bVideo AndAlso m_vdo.m_bVideoEnCours AndAlso Not m_vdo.m_bVideoTerminee Then
            m_vdo.bTerminer()
            m_iNumImg = iNbImg  ' Arreter la vidéo
        End If

    End Sub

    Private Sub frmFractalis_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        If Not (e.Alt OrElse e.Control OrElse e.Shift) Then
            StopTrace()
            Select Case e.KeyCode
                Case Keys.Left : m_clsFract.Deplacer(-clsFract.rPetitDeplacement, 0)
                Case Keys.Right : m_clsFract.Deplacer(clsFract.rPetitDeplacement, 0)
                Case Keys.Up : m_clsFract.Deplacer(0, -clsFract.rPetitDeplacement)
                Case Keys.Down : m_clsFract.Deplacer(0, clsFract.rPetitDeplacement)
            End Select
            Retracer()
            Exit Sub
        End If
        If e.Control AndAlso Not (e.Alt OrElse e.Shift) Then
            Select Case e.KeyCode
                Case Keys.Up : EvZoomPlus(clsFract.rFactPetitZoomPlus)
                Case Keys.Down : EvZoomMoins(clsFract.rFactPetitZoomMoins)
            End Select
            Exit Sub
        End If
        If e.Alt AndAlso Not e.Control Then
            StopTrace()
            Dim rDep As Decimal = clsFract.rPetitDeplacementJulia
            If e.Shift Then rDep = clsFract.rTresPetitDeplacementJulia
            Select Case e.KeyCode
                Case Keys.Left : m_clsFract.DeplacerPtJulia(-rDep, 0)
                Case Keys.Right : m_clsFract.DeplacerPtJulia(rDep, 0)
            ' Déplacer dans l'autre sens, pour être cohérent avec 
            '  le point affiché dans la config.
                Case Keys.Up : m_clsFract.DeplacerPtJulia(0, rDep)
                Case Keys.Down : m_clsFract.DeplacerPtJulia(0, -rDep)
            End Select
            Retracer()
            MajJuliaFrmConfig()
            Exit Sub
        End If
        If e.Alt AndAlso e.Control AndAlso Not e.Shift Then
            StopTrace()
            m_clsFract.InitPtJulia()
            Retracer()
            MajJuliaFrmConfig()
            Exit Sub
        End If

    End Sub

    Private Sub frmFractalis_KeyPress(sender As Object, e As Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress

        'Debug.WriteLine(e.KeyChar)
        If bVideo AndAlso m_vdo.m_bVideoEnCours AndAlso e.KeyChar = sCarFinVideo Then
            e.Handled = True
            If Is64BitProcess() Then MsgBox("Rappel : Pas de vidéo en 64 bits !", MsgBoxStyle.Information, sTitreMsg)
            If m_vdo.bTerminer() Then
                Me.Text = m_sTitreFrm & " : vidéo !" '" : vidéo enregistrée."
                'Beep(400, 20)
            End If
            Exit Sub
        End If

        ' Touche entrée :
        'If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
        '    e.Handled = True
        '    Exit Sub
        'End If

    End Sub

    Private Sub frmFractalis_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim sVersionAppli$ = " - V" & My.Application.Info.Version.Major &
            "." & My.Application.Info.Version.Minor &
            My.Application.Info.Version.Build & " (" & sDateVersionAppli & ")"
        Dim sTxt$ = Me.Text & sVersionAppli
        If bDebug Then sTxt &= " - Debug"
        If Is64BitProcess() Then sTxt &= " - 64 bits" Else sTxt &= " - 32 bits"
        Me.Text = sTxt
        m_sTitreFrm = sTxt

        Me.AddOwnedForm(m_frmConfig) ' Gestion de l'iconisation des 2 feuilles

    End Sub

    Private Sub frmFractalis_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated

        If Not m_bInitApp Then

            If bVideo Then
                m_vdo = New clsVideo
                m_sCheminAVI = Application.StartupPath & sCheminAVI

                ' Création du dossier, le cas échéant
                Dim sDossier$ = IO.Path.GetDirectoryName(m_sCheminAVI)
                If Not IO.Directory.Exists(sDossier) Then
                    Dim di As New IO.DirectoryInfo(sDossier)
                    Try
                        di.Create()
                    Catch
                    End Try
                    If Not IO.Directory.Exists(sDossier) Then
                        MsgBox("Impossible de créer le dossier :" & vbLf & sDossier,
                            MsgBoxStyle.Critical, sTitreMsg)
                        Exit Sub
                    End If
                End If

                If m_vdo.bInitialiser(m_sCheminAVI) Then
                    m_iNumImg = 1
                    m_dTpsDebVideo = Now
                End If
            End If

            InitFract()
            m_bReTracer = True
        End If
        m_bInitApp = True ' On est prêt à tracer maintenant

        m_bSuspendreTracePdtResize = False

    End Sub

    Private Sub frmFractalis_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        LibererRessourceDotNet() ' 25/01/2015 Pb d'instance qui reste longtemps en ram
    End Sub

    Private Sub InitFract()

        m_frmConfig.iDegre = clsFract.iDegreAlgoDef
        m_frmConfig.iNbIterationsMax = clsFract.iNbIterationsMaxDepartDef
        m_frmConfig.bJulia = (clsFract.typeFractDef = TFractal.Julia)
        m_frmConfig.ptfJulia = clsFract.ptfJuliaDef

        m_frmConfig.bMire = False
        m_frmConfig.bPaletteSysteme = clsFract.bPaletteSystemeDef
        m_frmConfig.bPaletteAleatoire = clsFract.bPaletteAleatoireDef
        m_frmConfig.iNbCouleurs = clsFract.iCouleurMaxDef
        m_frmConfig.iPremCouleur = clsFract.iPremCouleurDef
        m_frmConfig.iNbCyclesDegrade = clsFract.iNbCyclesPaletteDef
        m_frmConfig.bFrontiereUnie = clsFract.bFrontiereUnieDef
        m_frmConfig.bAlgoRapide = clsFract.bAlgoRapideDef
        m_frmConfig.bDecimal = clsFract.bDecimalDef
        m_frmConfig.bLisser = clsFract.bLisserDef

        m_clsFract.InitConfig()
        m_frmConfig.bEffacerImg = m_clsFract.m_bEffacerImgDef

        m_clsFract.m_prmPalette.bPaletteSysteme = m_frmConfig.bPaletteSysteme
        m_clsFract.m_prmPalette.iNbCouleurs = m_frmConfig.iNbCouleurs
        m_clsFract.m_prmPalette.iPremCouleur = m_frmConfig.iPremCouleur
        m_clsFract.m_prmPalette.iNbCyclesDegrade = m_frmConfig.iNbCyclesDegrade
        m_clsFract.m_prmPalette.bPaletteAleatoire = m_frmConfig.bPaletteAleatoire
        m_clsFract.m_prmPalette.bFrontiereUnie = m_frmConfig.bFrontiereUnie
        m_clsFract.m_prmPalette.bLisser = m_frmConfig.bLisser

        m_clsFract.InitialiserPrmFract()
        m_clsFract.InitPalette()

        If bDefinirCible Then InitCible()

        'If clsFract.typeFractDef = TFractal.Julia Then
        '    Dim x% = Me.ClientRectangle.Width
        '    Dim y% = Me.ClientRectangle.Height
        '    Dim rRatioEcran# = x / y
        '    'Dim rDep! = CSng(-rRatioEcran / 10)
        '    'm_clsFract.Deplacer(rDep, 0)
        '    m_clsFract.Zoomer(clsFract.rZoomDef)
        'End If

    End Sub

    Private Sub InitCible()

        Dim X, Y, Z, ZCible As Decimal
        Dim IMin%

        X = 0 : Y = 0 : Z = 4 : IMin = 0

        'X = 0.3583868675873765381477125472D
        'Y = 0.6468847308973626478270010484D
        'Z = 0.0003169718035602269918059889D
        'IMin = 2280

        If bVideo Then Z = 4 : IMin = 0

        m_clsFract.ViserPoint(X, Y, Z, IMin, ZCible)
        m_frmConfig.iNbIterationsMax = IMin + m_clsFract.iNbIterationsMaxDepart
        m_clsFract.m_bModeCible = True

        MajCoordZoomFrmConfig()

    End Sub

    Private Function bIconisation() As Boolean
        bIconisation = False
        ' Iconisation, autre poss.: tester WindowState
        If Me.ClientSize.Width = 0 And Me.ClientSize.Height = 0 Then bIconisation = True
    End Function

    Private Sub frmFractalis_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        If bIconisation() Then GoTo Fin

        StopTrace()
        InitialiserGraphique() ' Il faut recréer le graphique

        ' Retour d'une iconisation
        If m_szTailleEcran.Width = m_szMemTailleEcran.Width And
           m_szTailleEcran.Height = m_szMemTailleEcran.Height Then
            ' Dans ce cas, on peut réafficher le bitmap pendant le thread !
            m_bSuspendreTracePdtResize = False
            ' Pas besoin du timer invalidate dans ce cas
            If Not m_bReTracer Then Exit Sub
        End If
        m_szMemTailleEcran = m_szTailleEcran

        ' Si l'application est initialisée, on attend que l'utilisateur
        '  ait finit de redimensionner la feuille avant de recommencer à tracer
        If m_bInitApp Then Me.TimerResize.Enabled = True

Fin:
        m_bSuspendreTracePdtResize = True

    End Sub

    Private Sub TimerResize_Tick(sender As Object, e As EventArgs) Handles TimerResize.Tick

        Me.TimerResize.Enabled = False
        m_bSuspendreTracePdtResize = False
        Retracer()

    End Sub

    Private Sub InitialiserGraphique()

        If bIconisation() Then Exit Sub

        m_szTailleEcran = Me.ClientSize
        m_clsFract.szTailleEcran = m_szTailleEcran

        m_gr = Me.CreateGraphics
        ' Au retour d'une iconisation, garder le bitmap
        If m_szTailleEcran.Width <> m_szMemTailleEcran.Width Or
            m_szTailleEcran.Height <> m_szMemTailleEcran.Height Then
            m_bmpCache = New Bitmap(
                m_szTailleEcran.Width, m_szTailleEcran.Height,
                Imaging.PixelFormat.Format32bppArgb) ' Test frc rapide 07/08/2014
            ' Tracer dans le buffer
            m_clsFract.Gr = Graphics.FromImage(m_bmpCache)
        End If
        m_clsFract.RespecterRatioZoneAbs()
        MajCoordZoomFrmConfig()

        If bVideo AndAlso Not IsNothing(m_vdo) AndAlso
           Not String.IsNullOrEmpty(m_sCheminAVI) Then
            ' Si une vidéo est commencée, la terminer et arreter le mode vidéo
            If m_vdo.m_bVideoEnCours Then m_vdo.bTerminer() : bVideo = False
            ' On peut aussi redémmarer le mode vidéo, mais il faut réinit.
            '  le nbre d'images restantes, le zoom, ...
            'If m_vdo.bInitialiser(m_sCheminAVI) Then InitFract()
        End If

    End Sub

#End Region

#Region "Gestion de l'interface"

    Private Sub frmFractalis_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint

        If m_frmConfig.bMire() Then
            AfficherPalette(e)
            Exit Sub
        End If

        RetracerPaint()

    End Sub

    Private Sub RetracerPaint()

        If m_bSuspendreTracePdtResize Then Exit Sub

        ' Si on a bufferisé et qu'il s'agit d'une simple m.a.j.,
        '  on affiche le bitmap
        If Not m_clsFract.bQuitterTrace AndAlso
            (Not m_bReTracer OrElse bTraceEnCours()) Then
            MajEcranBmpCache()
            Exit Sub
        End If

        StopTrace()

        If bVideo AndAlso bDeplacerPtJulia Then

            If m_iNumImg = 1 Then
                m_clsFract.m_prm.rDeltaAngle = CDec(Math.PI / 1500) '500)
                m_clsFract.m_prm.rIm = 0.9D
                m_clsFract.m_prm.rRe = -0.9D
            Else
                m_clsFract.ZoomerFacteur(rZoomVideo, bZoomMoins:=False)
                m_clsFract.m_prm.rIm -= 0.0001D
                m_clsFract.m_prm.rRe += 0.00005D
            End If

        End If

        If m_frmConfig.bMire() Then Exit Sub

        ' Pour l'algo. rapide, réafficher d'emblée la précédente image (sauf en mode vidéo)
        '  il y a parfois un léger scintillement en gris :
        ' 18/01/2015 Soluce contre le scintillement : retracer directement
        If Not m_clsFract.bEffacerImg() AndAlso
           Not bDesactiverRafraichissementPdtVideo Then
            MajEcranBmpCache()
        End If

        ' Ne pas réinit la palette si c'est la palette aléatoire et qu'elle n'a pas été modifiée
        If Not m_frmConfig.bPaletteSysteme() AndAlso m_frmConfig.bPaletteModifiee() Then
            m_clsFract.CalculerNbCouleurs()
        End If
        If Not m_frmConfig.bPaletteSysteme() AndAlso m_frmConfig.bPaletteModifiee() Then
            m_clsFract.InitPaletteCalc()
            MajCoordZoomFrmConfig()
            m_frmConfig.bPaletteModifiee = False
        End If

        Dim dTpsDeb As Date = Now
        Me.m_bTraceEnCours = True
        If Not bVideo Then Me.Text = m_sTitreFrm & "..."

        ' Mode simulation pour debug pb compression sur youtube
        Dim bSimul As Boolean = False
        If bDebugCompress AndAlso bVideo Then
            If m_iNumImg < iNumImgDepartDebug Then ' m_iNumImg > 1 AndAlso 
                bSimul = True
                m_clsFract.FinTrace()
                GoTo Suite
            End If
        End If

        m_clsFract.TracerFractDepart(m_bmpCache)

Suite:

        If Not bVideo Then
            Dim dTpsFin As Date = Now
            Dim ts As TimeSpan = dTpsFin - dTpsDeb
            'Debug.WriteLine("Temps de tracé : " & ts.TotalSeconds.ToString("0.00") & " sec.")
            Dim sTps$ = "(" & ts.TotalSeconds.ToString("0.000") & " sec.)"
            Me.Text = m_sTitreFrm & " " & sTps
            Exit Sub
        End If

        If Not m_vdo.m_bVideoTerminee AndAlso m_iNumImg <= iNbImg Then

            If Not bSimul Then m_vdo.bAjouterImage(m_bmpCache)

            Dim sAvancement$ = m_iNumImg & "/" & iNbImg
            Dim dTpsFin As Date = Now
            Dim ts As TimeSpan = dTpsFin - m_dTpsDebVideo
            Dim sTps$ = "(" & ts.TotalSeconds.ToString("0.0") & " sec.)"
            Me.Text = m_sTitreFrm & " : vidéo... " & sAvancement & " " & sTps

        End If

    End Sub

    Private Sub AfficherPalette(e As PaintEventArgs)

        m_clsFract.CalculerNbCouleurs()
        If Not m_frmConfig.bPaletteSysteme Then _
            m_clsFract.InitPaletteCalc() : m_frmConfig.bPaletteModifiee = False

        Me.Size = New Size(1500, 850)
        Dim fs As New FontStyle()
        Dim font1 As Font = New Font(Me.Font, fs)
        Dim brushBlanc As New SolidBrush(Color.White)
        Dim brushNoir As New SolidBrush(Color.Black)
        Dim brush0 As New SolidBrush(Color.Black)

        Dim iPas% = 32 '35
        Dim iNumPave% = 0
        For y As Integer = 0 To Me.Size.Height - iPas Step iPas
            For x As Integer = 0 To Me.Size.Width - iPas Step iPas

                Dim couleur As Color = m_clsFract.CouleurPalette(iNumPave, bFrontiere:=False)
                brush0.Color = couleur
                e.Graphics.FillRectangle(brush0, x, y, iPas, iPas)
                Dim sVal$ = iNumPave.ToString
                e.Graphics.FillRectangle(brushBlanc, x, y, 28, 13)
                e.Graphics.DrawString(sVal, font1, brushNoir, x, y)

                iNumPave += 1
            Next
        Next

    End Sub

    Private Sub DisplayKnownColors(e As PaintEventArgs)

        Me.Size = New Size(650, 550)
        Dim i As Integer

        ' Get all the values from the KnownColor enumeration.
        Dim colorsArray As System.Array =
            [Enum].GetValues(GetType(KnownColor))
        Dim allColors(colorsArray.Length) As KnownColor

        Array.Copy(colorsArray, allColors, colorsArray.Length)

        ' Loop through printing out the value's name in the colors 
        ' they represent.
        Dim y As Single
        Dim x As Single = 10.0F

        For i = 0 To allColors.Length - 1

            ' If x is a multiple of 30, start a new column.
            If (i > 0 And i Mod 30 = 0) Then
                x += 105.0F
                y = 15.0F
            Else
                ' Otherwise increment y by 15.
                y += 15.0F
            End If

            ' Create a custom brush from the color and use it to draw
            ' the brush's name.
            'Me.Font.Bold = True
            Dim fs As New FontStyle()
            'fs.Bold = True
            Dim font1 As Font = New Font(Me.Font, fs)
            Dim aBrush As New SolidBrush(Color.FromName(
                allColors(i).ToString()))
            Dim kc As KnownColor = allColors(i)
            Dim couleur As Color = Color.FromKnownColor(kc)
            Dim brush0 As New SolidBrush(couleur) 'Color.Black
            e.Graphics.FillRectangle(brush0, x - 12, y, 13, 13)
            e.Graphics.DrawString(allColors(i).ToString(),
                font1, aBrush, x, y)

            ' Dispose of the custom brush.
            aBrush.Dispose()
        Next

    End Sub

    Private Sub MajEcranBmpCache()

        If bVideo AndAlso bNumeroterImg AndAlso m_iNumImg <= iNbImg Then
            Dim fs As New FontStyle()
            Dim font1 As Font = New Font(Me.Font, fs)
            Dim width% = m_bmpCache.Width
            Dim heigth% = m_bmpCache.Height
            Const iMargeHaut% = 30 ' Ne pas mettre 0, car WMP cache le haut de l'img !
            Dim rectBmp As New Rectangle(0, iMargeHaut, width, heigth)
            Dim g As Graphics = Graphics.FromImage(Me.m_bmpCache)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.DrawString(m_iNumImg.ToString, font1, Brushes.White, rectBmp)
            g.Flush()
        End If

        m_gr.DrawImage(m_bmpCache, 0, 0)

    End Sub

    Private Sub frmFractalis_Closing(sender As Object, e As _
            System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        StopTrace()
    End Sub

    Private Sub frmFractalis_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown

        If e.Button = MouseButtons.Left Then
            StopTrace()
            If m_clsFract.bModeDetailIterations Then _
                m_clsFract.SelectionnerPoint(New Point(e.X, e.Y)) : Exit Sub
            m_rectCoordPixels = New Rectangle(e.X, e.Y, 0, 0)
        End If

        If e.Button <> MouseButtons.Right Then Exit Sub

        ' Affichage du panneau de configuration de Fractalis

        If bTraceEnCours() Then Me.m_clsFract.m_bPause = True ': m_thrdFract.Suspend()
        ' Note : Pour pouvoir fixer la position de la feuille avant d'appeler
        '  Show(), il faut que m_frmConfig.StartPosition soit sur manual
        If Me.WindowState = FormWindowState.Normal Then _
            m_frmConfig.Location = New Point(Me.Left + Me.Width, Me.Top)

        m_frmConfig.Show()

        MajCoordZoomFrmConfig()
        If bTraceEnCours() Then
            ' Petit défaut : il faudrait laisser au formulaire le temps 
            '  de s'afficher, mais le Suspend et le Resume ne suffisent pas
            'm_thrdFract.Sleep(1000) ' inutile : ne suffit pas
            ' Attention au problème de synchronisation avec la mise à jour 
            '  du bitmap : cela provoque parfois un blocage !
            'Application.DoEvents() 
            ' Dans la prochaine version, je vais peut être enlever le thread
            'm_thrdFract.Resume()
            Me.m_clsFract.m_bPause = False
        End If

    End Sub

    Private Sub frmFractalis_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove

        If e.Button <> MouseButtons.Left Then Exit Sub
        If m_clsFract.bModeDetailIterations Then _
            m_clsFract.SelectionnerPoint(New Point(e.X, e.Y)) : Exit Sub
        MajEcranBmpCache()
        rectTracerSelection(e)

    End Sub

    Private Sub frmFractalis_MouseUp(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp

        If e.Button <> MouseButtons.Left Then Exit Sub
        If m_clsFract.bModeDetailIterations Then Exit Sub
        Dim rectNorm As Rectangle = rectTracerSelection(e)
        If rectNorm.Width = 0 Or rectNorm.Height = 0 Then Exit Sub
        m_rectCoordPixels = rectNorm
        m_clsFract.ZoomerZonePixels(m_rectCoordPixels)
        Retracer()

    End Sub

    Private Function rectTracerSelection(e As MouseEventArgs) As Rectangle

        ' Tracer le cadre de sélection d'une zone à zoomer et
        '  renvoyer le rectangle normalisé et recadré selon le
        '  ratio de l'écran, en pixels

        ' Idée : utiliser le mode XOR pour ne pas effacer le tracé :
        '  je n'ai pas trouvé comment faire en VB .NET
        ' Autre solution : retracer le bitmap de cache avant l'appel 
        '  à cette fonction

        ' Méthode la plus simple s'il n'y a pas besoin de respecter le ratio
        'Dim m_pt2 As New Point(e.X, e.Y)
        'm_rectCoordPixels = New Rectangle( _
        '    Math.Min(m_pt1.X, m_pt2.X), _
        '    Math.Min(m_pt1.Y, m_pt2.Y), _
        '    Math.Abs(m_pt2.X - m_pt1.X), _
        '    Math.Abs(m_pt2.Y - m_pt1.Y))

        m_rectCoordPixels.Width = e.X - m_rectCoordPixels.Left
        m_rectCoordPixels.Height = e.Y - m_rectCoordPixels.Top
        Dim rectNorm As Rectangle = m_rectCoordPixels
        rectNorm = rectRespecterRatioZonePixels(rectNorm)
        rectNorm = rectNormaliserRectangle(rectNorm)
        m_gr.DrawRectangle(Pens.Black, rectNorm)
        rectTracerSelection = rectNorm

    End Function

    Private Function rectRespecterRatioZonePixels(
        rectCoordPixels As Rectangle) As Rectangle

        ' Attention : la zone de sélection doit être proportionnelle
        '  au ratio de l'écran
        If m_szTailleEcran.Height >= m_szTailleEcran.Width Then
            ' \ : Antislash = Division entière
            If m_szTailleEcran.Width <> 0 Then _
            rectCoordPixels.Height = rectCoordPixels.Width *
                m_szTailleEcran.Height \ m_szTailleEcran.Width
        Else
            If m_szTailleEcran.Height <> 0 Then _
            rectCoordPixels.Width = rectCoordPixels.Height *
                m_szTailleEcran.Width \ m_szTailleEcran.Height
        End If

        rectRespecterRatioZonePixels = rectCoordPixels

    End Function

    Private Function rectNormaliserRectangle(rect As Rectangle) As Rectangle

        ' Traiter les rectangles à l'envers

        rectNormaliserRectangle = rect

        If rect.Width < 0 And rect.Height < 0 Then
            rectNormaliserRectangle = New Rectangle(
                rect.Left + rect.Width, rect.Top + rect.Height, -rect.Width, -rect.Height)
            Exit Function
        End If

        If rect.Width < 0 Then _
            rectNormaliserRectangle = New Rectangle(
                rect.Left + rect.Width, rect.Top, -rect.Width, rect.Height)
        If rect.Height < 0 Then _
            rectNormaliserRectangle = New Rectangle(
                rect.Left, rect.Top + rect.Height, rect.Width, -rect.Height)

    End Function

    Private Sub MajCoordZoomFrmConfig()

        ' Centre et amplitude du zoom en coordonnées absolues
        ' 28 décimales : on a besoin du max. de précision 
        'Const sFormat$ = "0.0000000000000000000000000000"
        Dim sCoordZoom$ = "Coordonnées du zoom (" & m_clsFract.m_iNbCouleurs & " couleurs) :"
        sCoordZoom &= vbLf & "X = " & m_clsFract.rCentreX & "D" '.ToString(sFormat)
        sCoordZoom &= vbLf & "Y = " & m_clsFract.rCentreY & "D" '.ToString(sFormat)
        sCoordZoom &= vbLf & "Z = " & m_clsFract.rAmplitX & "D" '.ToString(sFormat)

        Dim rLogAct# = Math.Log10(m_clsFract.rAmplitX)
        Dim sCible$ = ""
        Dim sType$ = " dbl"
        Dim rLimite As Decimal = CDec(clsFract.rAmplitudeMinOkDouble)
        If m_frmConfig.bDecimal Then rLimite = clsFract.rAmplitudeMinOkDecimal : sType = " dec."
        Dim rLogLimite# = Math.Log10(rLimite)
        Dim rPCLimite# = rLogAct / rLogLimite
        Dim sPCLimite$ = rPCLimite.ToString("0.00%")
        Dim sLimite$ = "Limite " & sType & " : " & sPCLimite
        If m_clsFract.m_bModeCible Then
            Dim rLogCible# = Math.Log10(m_clsFract.m_cf.rZoomCible)
            Dim rPC# = rLogAct / rLogCible
            Dim sPC$ = rPC.ToString("0.00%")
            sCible = ", Cible : " & sPC
        End If
        sCoordZoom &= vbLf & sLimite & sCible
        m_frmConfig.sCoordZoom = sCoordZoom

        'sCoordZoom &= vbLf & "IMin = " & m_clsFract.m_iMemNbIterationsMin 'iNbIterationsMin
        'sCoordZoom &= vbLf & "IMax = " & m_clsFract.iNbIterationsMax

        m_frmConfig.iNbIterationsMax = m_clsFract.iNbIterationsMax
        sCoordZoom &= vbLf & "IMin = " & m_clsFract.iNbIterationsMax

        sCoordZoom = sCoordZoom.Replace(",", ".")
        'Debug.WriteLine("")
        'Debug.WriteLine(sCoordZoom)
        sCoordZoom = sCoordZoom.Replace(vbLf, vbCrLf)
        If Not bVideo Then CopierPressePapier(sCoordZoom)

        'Dim sCoord$ = _
        '    "m_clsFract.m_cf.rCoordAbsXMin=" & m_clsFract.m_cf.rCoordAbsXMin & _
        '    "D: m_clsFract.m_cf.rCoordAbsXMax=" & m_clsFract.m_cf.rCoordAbsXMax & _
        '    "D: m_clsFract.m_cf.rCoordAbsYMin=" & m_clsFract.m_cf.rCoordAbsYMin & _
        '    "D: m_clsFract.m_cf.rCoordAbsYMax=" & m_clsFract.m_cf.rCoordAbsYMax & "D"
        'sCoord = sCoord.Replace(",", ".")
        'Debug.WriteLine("")
        'Debug.WriteLine(sCoord)

    End Sub

    Private Sub MajJuliaFrmConfig()
        Dim ptfJulia As New PointF(m_clsFract.rLirePointJuliaX, m_clsFract.rLirePointJuliaY)
        m_frmConfig.ptfJulia = ptfJulia
    End Sub

    Public Sub LireConfig()

        StopTrace()

        ' Si les paramètres ont changés, réinitaliser le nombre d'itération
        Dim bInit As Boolean = False
        If m_frmConfig.bJulia <> m_clsFract.bJulia Then bInit = True
        If m_frmConfig.bJulia And
            (m_frmConfig.ptfJulia.X <> m_clsFract.ptfJulia.X Or
             m_frmConfig.ptfJulia.Y <> m_clsFract.ptfJulia.Y) Then bInit = True
        If m_frmConfig.iDegre <> m_clsFract.iDegre Then bInit = True
        ' Pour m_clsFract.m_iNbIterationsMin = 0
        If bInit Then m_clsFract.InitialiserIterations()

        m_clsFract.typeFrac = m_frmConfig.typeFrac ' Pas utilisé : à vérifier
        m_clsFract.bJulia = m_frmConfig.bJulia
        If m_frmConfig.bJulia Then m_clsFract.ptfJulia = m_frmConfig.ptfJulia
        m_clsFract.iDegre = m_frmConfig.iDegre
        m_clsFract.iNbIterationsMaxDepart = m_frmConfig.iNbIterationsMax
        m_clsFract.bEffacerImg = m_frmConfig.bEffacerImg
        m_clsFract.bModeDetailIterations = m_frmConfig.bModeDetailIterations
        m_clsFract.bModeTranslation = m_frmConfig.bModeTranslation
        m_clsFract.m_bAlgoRapide = m_frmConfig.bAlgoRapide
        m_clsFract.m_bDecimal = m_frmConfig.bDecimal
        m_clsFract.m_bLisser = m_frmConfig.bLisser

        m_clsFract.m_prmPalette.bPaletteSysteme = m_frmConfig.bPaletteSysteme
        m_clsFract.m_prmPalette.iNbCouleurs = m_frmConfig.iNbCouleurs
        m_clsFract.m_prmPalette.iPremCouleur = m_frmConfig.iPremCouleur
        m_clsFract.m_prmPalette.iNbCyclesDegrade = m_frmConfig.iNbCyclesDegrade
        m_clsFract.m_prmPalette.bPaletteAleatoire = m_frmConfig.bPaletteAleatoire
        m_clsFract.m_prmPalette.bFrontiereUnie = m_frmConfig.bFrontiereUnie
        m_clsFract.m_prmPalette.bLisser = m_frmConfig.bLisser

        m_clsFract.CalculerNbCouleurs()
        If Not m_clsFract.m_prmPalette.bPaletteSysteme Then m_clsFract.InitPaletteCalc()
        m_clsFract.InitPalette()

    End Sub

    Public Sub Retracer()

        m_bReTracer = True

        If bVideo Then
            Me.TimerVideo.Enabled = True
            Me.TimerVideo.Start()
        Else
            ' 18/01/2015 Retracer directement : mode navigation
            RetracerPaint()
        End If

        MajCoordZoomFrmConfig()

    End Sub

    Private Sub TimerVideo_Tick(sender As Object, e As EventArgs) Handles TimerVideo.Tick
        Me.TimerVideo.Stop()
        RetracerPaint()
    End Sub

    Public Sub PauseReprendreTrace()

        ' Faire une pause du thread ou bien reprendre le thread suspendu

        If Not bTraceEnCours() Then Exit Sub
        Me.m_clsFract.m_bPause = Not Me.m_clsFract.m_bPause

    End Sub

    Public Sub StopTrace()
        If Not bTraceEnCours() Then Exit Sub
        m_clsFract.bQuitterTrace = True
        m_frmConfig.iAvancement = 0
        m_clsFract.bModeDetailIterations = m_frmConfig.bModeDetailIterations
        m_clsFract.bModeTranslation = m_frmConfig.bModeTranslation
        Me.m_bTraceEnCours = False
    End Sub

    Public Function bTraceEnCours() As Boolean
        Return Me.m_bTraceEnCours
    End Function

#End Region

#Region "Gestion des événements particuliers"

    Private Sub m_frmConfig_EvAppliquer() Handles m_frmConfig.EvAppliquer
        LireConfig()
        If m_frmConfig.bMire() Then
            Invalidate()
            Exit Sub
        End If
        Retracer()
    End Sub

    Private Sub m_frmConfig_EvPause() Handles m_frmConfig.EvPause
        PauseReprendreTrace()
    End Sub

    Private Sub m_frmConfig_EvStop() Handles m_frmConfig.EvStop
        StopTrace()
        m_iNumImg = iNbImg ' Arreter la vidéo
    End Sub

    Private Sub m_frmConfig_EvZoomInit() Handles m_frmConfig.EvZoomInit
        StopTrace()
        m_clsFract.InitialiserPrmFract()
        LireConfig()
        Retracer()
    End Sub

    Private Sub m_frmConfig_EvZoomMoins() Handles m_frmConfig.EvZoomMoins
        EvZoomMoins(clsFract.rFacteurZoomMoins)
    End Sub

    Private Sub EvZoomPlus(rFacteurZoomPlus As Decimal)
        StopTrace()
        m_clsFract.ZoomerFacteur(rFacteurZoomPlus, bZoomMoins:=False)
        Retracer()
    End Sub

    Private Sub EvZoomMoins(rFacteurZoomMoins As Decimal)

        StopTrace()

        ' 21/08/2014 Avant de réinit. le nombre d'itération min.
        '  d'abord augmenter le nombre d'itération de départ
        m_clsFract.iNbIterationsMaxDepart = m_clsFract.iNbIterationsMax
        m_clsFract.InitialiserIterations()

        ' Ne pas utiliser le zoom fenêtre (pour éviter de modifier la position actuelle) :
        'm_clsFract.Zoomer(rFacteurZoomPlus)
        ' Mais le zoom depuis la position actuelle
        m_clsFract.ZoomerFacteur(rFacteurZoomMoins, bZoomMoins:=True)
        Retracer()

    End Sub

    Private Sub m_clsFract_EvMajBmp() Handles m_clsFract.EvMajBmp
        MajEcranBmpCache()
    End Sub

    Private Sub m_frmConfig_EvModeTranslation() Handles m_frmConfig.EvModeTranslation
        m_clsFract.bModeTranslation = m_frmConfig.bModeTranslation
        If Not m_clsFract.bModeTranslation Then MajEcranBmpCache()
    End Sub

    Private Sub m_frmConfig_EvDetailIterations() Handles m_frmConfig.EvDetailIterations
        m_clsFract.bModeDetailIterations = m_frmConfig.bModeDetailIterations
        If Not m_clsFract.bModeDetailIterations Then MajEcranBmpCache()
    End Sub

    Private Sub m_clsFract_EvDetailIterations(aPt() As Drawing.Point) Handles m_clsFract.EvDetailIterations
        If IsNothing(aPt) Then Exit Sub
        MajEcranBmpCache()
        Const iEpaisseurTrait% = 2
        Dim penBlanc As New Pen(Color.White, iEpaisseurTrait)
        For i As Integer = 1 To aPt.GetUpperBound(0)
            m_gr.DrawLine(penBlanc, aPt(i - 1), aPt(i))
        Next i
    End Sub

    Private Sub m_clsFract_EvMajAvancement(iAvancement%) Handles m_clsFract.EvMajAvancement
        m_frmConfig.iAvancement = iAvancement
    End Sub

    Private Sub m_clsFract_EvFinTrace() Handles m_clsFract.EvFinTrace

        m_bReTracer = False
        m_frmConfig.iAvancement = 0
        m_clsFract.bModeDetailIterations = m_frmConfig.bModeDetailIterations
        m_clsFract.bModeTranslation = m_frmConfig.bModeTranslation

        If Not m_clsFract.m_bQuitterTrace Then

            If bVideo Then
                m_iNumImg += 1
                ' Rajouter une image à la fin pour avoir le bon compte dans la vidéo 28/03/2015
                If m_iNumImg <= iNbImg + 1 Then
                    Me.m_bTraceEnCours = False
                    Retracer()
                    If bDeplacerPtJulia Then MajJuliaFrmConfig()
                    Exit Sub
                End If
            End If

            Dim W As Decimal = m_clsFract.m_cf.rCoordAbsXMax - m_clsFract.m_cf.rCoordAbsXMin
            Dim H As Decimal = m_clsFract.m_cf.rCoordAbsYMax - m_clsFract.m_cf.rCoordAbsYMin
            ' Ne marche pas : on n'obtient pas exactement le même point de zoom
            ' (il faudrait commencer en décimal, mais 20x + lent)
            'Const rPaveMin3# = 0.00000000000002 ' 2E-14 
            'Const rPaveMin2# = 0.0000000000001  ' 1E-13
            'Const rPaveMin1# = 0.0000000000005  ' 5E-13
            'Const rPaveMin1# = 0.000000000002  ' 1E-12
            Dim rPaveMin3 As Decimal = CDec(clsFract.rAmplitudeMinOkDouble)
            If m_frmConfig.bDecimal Then rPaveMin3 = clsFract.rAmplitudeMinOkDecimal
            Dim rPaveMin2 As Decimal = rPaveMin3 * 5
            Dim rPaveMin1 As Decimal = rPaveMin2 * 5
            Dim iFreqBeep% = 600
            Dim iDuree% = 20
            If W < rPaveMin3 OrElse H < rPaveMin3 Then
                iFreqBeep = 200 : iDuree = 50
            ElseIf W < rPaveMin2 OrElse H < rPaveMin2 Then
                iFreqBeep = 400
            ElseIf W < rPaveMin1 OrElse H < rPaveMin1 Then
                iFreqBeep = 500
            End If
            Beep(iFreqBeep, iDuree)

        End If

    End Sub

#End Region

End Class