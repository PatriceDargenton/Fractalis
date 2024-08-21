
' Fichier FrmConfig.vb : Configuration de Fractalis
' --------------------

Public Class frmConfig : Inherits Form

    Public Event EvZoomMoins()
    Public Event EvZoomInit()
    Public Event EvAppliquer()
    Public Event EvPause()
    Public Event EvStop()
    Public Event EvDetailIterations()
    Public Event EvModeTranslation()

    ' Amplitude max. (en coord. absolue) que l'on peut fixer 
    '  dans la petite fenêtre pour le paramètre de Julia 
    Private Const iAmplitPrmJulia% = 10
    Private m_ptfJulia As PointF
    Private m_bPaletteModifiee As Boolean ' Vérifier si la palette calculée est modifiée
    Private m_typeFract As TFractal
    Private m_bJulia As Boolean

    Friend WithEvents chkModeTranslation As System.Windows.Forms.CheckBox
    Friend WithEvents rbMandelbrotEtJulia As System.Windows.Forms.RadioButton
    Friend WithEvents pnlVideo As System.Windows.Forms.Panel

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
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
    Friend WithEvents grpbTypeFract As System.Windows.Forms.GroupBox
    Friend WithEvents rbJulia As System.Windows.Forms.RadioButton
    Friend WithEvents rbMandelbrot As System.Windows.Forms.RadioButton
    Friend WithEvents nudDegre As System.Windows.Forms.NumericUpDown
    Friend WithEvents LblDegre As System.Windows.Forms.Label
    Friend WithEvents cmdPause As System.Windows.Forms.Button
    Friend WithEvents cmdStop As System.Windows.Forms.Button
    Friend WithEvents cmdAppliquer As System.Windows.Forms.Button
    Friend WithEvents pbAvancement As System.Windows.Forms.ProgressBar
    Friend WithEvents nudNbIterationsMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblIterationMax As System.Windows.Forms.Label
    Friend WithEvents cmdZoomInit As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents cmdZoomMoins As System.Windows.Forms.Button
    Friend WithEvents lblZoom As System.Windows.Forms.Label
    Friend WithEvents chkEffacerImg As System.Windows.Forms.CheckBox
    Friend WithEvents chkModeDetailIterations As System.Windows.Forms.CheckBox
    Friend WithEvents pbxJulia As System.Windows.Forms.PictureBox
    Friend WithEvents lblPrmJulia As System.Windows.Forms.Label
    Friend WithEvents txtJuliaX As System.Windows.Forms.TextBox
    Friend WithEvents txtJuliaY As System.Windows.Forms.TextBox
    Friend WithEvents panelJulia As System.Windows.Forms.Panel
    Friend WithEvents pnlPalette As System.Windows.Forms.Panel
    Friend WithEvents chkPaletteSysteme As System.Windows.Forms.CheckBox
    Friend WithEvents nudPremCouleur As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents nudNbCouleurs As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents nudNbCyclesDegrade As System.Windows.Forms.NumericUpDown
    Friend WithEvents pbxVerif As System.Windows.Forms.PictureBox
    Friend WithEvents chkPaletteAleatoire As System.Windows.Forms.CheckBox
    Friend WithEvents chkMire As System.Windows.Forms.CheckBox
    Friend WithEvents chkFrontiereUnie As System.Windows.Forms.CheckBox
    Friend WithEvents chkLisser As System.Windows.Forms.CheckBox
    Friend WithEvents chkDecimal As System.Windows.Forms.CheckBox
    Friend WithEvents chkAlgoRapide As System.Windows.Forms.CheckBox

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.grpbTypeFract = New System.Windows.Forms.GroupBox()
        Me.rbMandelbrotEtJulia = New System.Windows.Forms.RadioButton()
        Me.rbJulia = New System.Windows.Forms.RadioButton()
        Me.rbMandelbrot = New System.Windows.Forms.RadioButton()
        Me.nudDegre = New System.Windows.Forms.NumericUpDown()
        Me.LblDegre = New System.Windows.Forms.Label()
        Me.cmdPause = New System.Windows.Forms.Button()
        Me.cmdStop = New System.Windows.Forms.Button()
        Me.cmdAppliquer = New System.Windows.Forms.Button()
        Me.cmdZoomInit = New System.Windows.Forms.Button()
        Me.chkEffacerImg = New System.Windows.Forms.CheckBox()
        Me.pbAvancement = New System.Windows.Forms.ProgressBar()
        Me.nudNbIterationsMax = New System.Windows.Forms.NumericUpDown()
        Me.lblIterationMax = New System.Windows.Forms.Label()
        Me.cmdZoomMoins = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.chkModeDetailIterations = New System.Windows.Forms.CheckBox()
        Me.txtJuliaY = New System.Windows.Forms.TextBox()
        Me.txtJuliaX = New System.Windows.Forms.TextBox()
        Me.pbxJulia = New System.Windows.Forms.PictureBox()
        Me.pnlPalette = New System.Windows.Forms.Panel()
        Me.chkLisser = New System.Windows.Forms.CheckBox()
        Me.chkFrontiereUnie = New System.Windows.Forms.CheckBox()
        Me.chkPaletteAleatoire = New System.Windows.Forms.CheckBox()
        Me.pbxVerif = New System.Windows.Forms.PictureBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.nudNbCyclesDegrade = New System.Windows.Forms.NumericUpDown()
        Me.nudPremCouleur = New System.Windows.Forms.NumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.nudNbCouleurs = New System.Windows.Forms.NumericUpDown()
        Me.chkMire = New System.Windows.Forms.CheckBox()
        Me.chkPaletteSysteme = New System.Windows.Forms.CheckBox()
        Me.chkDecimal = New System.Windows.Forms.CheckBox()
        Me.chkAlgoRapide = New System.Windows.Forms.CheckBox()
        Me.chkModeTranslation = New System.Windows.Forms.CheckBox()
        Me.pnlVideo = New System.Windows.Forms.Panel()
        Me.lblZoom = New System.Windows.Forms.Label()
        Me.panelJulia = New System.Windows.Forms.Panel()
        Me.lblPrmJulia = New System.Windows.Forms.Label()
        Me.grpbTypeFract.SuspendLayout()
        CType(Me.nudDegre, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudNbIterationsMax, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pbxJulia, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlPalette.SuspendLayout()
        CType(Me.pbxVerif, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudNbCyclesDegrade, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudPremCouleur, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudNbCouleurs, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlVideo.SuspendLayout()
        Me.panelJulia.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpbTypeFract
        '
        Me.grpbTypeFract.Controls.Add(Me.rbMandelbrotEtJulia)
        Me.grpbTypeFract.Controls.Add(Me.rbJulia)
        Me.grpbTypeFract.Controls.Add(Me.rbMandelbrot)
        Me.grpbTypeFract.Location = New System.Drawing.Point(8, 8)
        Me.grpbTypeFract.Name = "grpbTypeFract"
        Me.grpbTypeFract.Size = New System.Drawing.Size(112, 90)
        Me.grpbTypeFract.TabIndex = 0
        Me.grpbTypeFract.TabStop = False
        Me.grpbTypeFract.Text = "Ensemble de type"
        Me.ToolTip1.SetToolTip(Me.grpbTypeFract, "Types d'ensemble fractal à dessiner")
        '
        'rbMandelbrotEtJulia
        '
        Me.rbMandelbrotEtJulia.Location = New System.Drawing.Point(16, 70)
        Me.rbMandelbrotEtJulia.Name = "rbMandelbrotEtJulia"
        Me.rbMandelbrotEtJulia.Size = New System.Drawing.Size(90, 16)
        Me.rbMandelbrotEtJulia.TabIndex = 2
        Me.rbMandelbrotEtJulia.Text = "Mdb + Julia"
        '
        'rbJulia
        '
        Me.rbJulia.Location = New System.Drawing.Point(16, 48)
        Me.rbJulia.Name = "rbJulia"
        Me.rbJulia.Size = New System.Drawing.Size(80, 16)
        Me.rbJulia.TabIndex = 1
        Me.rbJulia.Text = "Julia"
        '
        'rbMandelbrot
        '
        Me.rbMandelbrot.Checked = True
        Me.rbMandelbrot.Location = New System.Drawing.Point(16, 24)
        Me.rbMandelbrot.Name = "rbMandelbrot"
        Me.rbMandelbrot.Size = New System.Drawing.Size(80, 16)
        Me.rbMandelbrot.TabIndex = 0
        Me.rbMandelbrot.TabStop = True
        Me.rbMandelbrot.Text = "Mandelbrot"
        '
        'nudDegre
        '
        Me.nudDegre.Location = New System.Drawing.Point(176, 16)
        Me.nudDegre.Maximum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudDegre.Minimum = New Decimal(New Integer() {2, 0, 0, 0})
        Me.nudDegre.Name = "nudDegre"
        Me.nudDegre.Size = New System.Drawing.Size(32, 20)
        Me.nudDegre.TabIndex = 1
        Me.nudDegre.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'LblDegre
        '
        Me.LblDegre.Location = New System.Drawing.Point(128, 16)
        Me.LblDegre.Name = "LblDegre"
        Me.LblDegre.Size = New System.Drawing.Size(48, 16)
        Me.LblDegre.TabIndex = 5
        Me.LblDegre.Text = "Degré :"
        Me.ToolTip1.SetToolTip(Me.LblDegre, "Degré de l'équation Z -> Z^degré + C")
        '
        'cmdPause
        '
        Me.cmdPause.Location = New System.Drawing.Point(120, 296)
        Me.cmdPause.Name = "cmdPause"
        Me.cmdPause.Size = New System.Drawing.Size(96, 24)
        Me.cmdPause.TabIndex = 8
        Me.cmdPause.Text = "Pause / Reprise"
        Me.ToolTip1.SetToolTip(Me.cmdPause, "Faire une pause / reprendre le tracé")
        '
        'cmdStop
        '
        Me.cmdStop.Location = New System.Drawing.Point(120, 328)
        Me.cmdStop.Name = "cmdStop"
        Me.cmdStop.Size = New System.Drawing.Size(96, 24)
        Me.cmdStop.TabIndex = 9
        Me.cmdStop.Text = "Stop"
        Me.ToolTip1.SetToolTip(Me.cmdStop, "Arrêter le tracé")
        '
        'cmdAppliquer
        '
        Me.cmdAppliquer.Location = New System.Drawing.Point(8, 264)
        Me.cmdAppliquer.Name = "cmdAppliquer"
        Me.cmdAppliquer.Size = New System.Drawing.Size(208, 24)
        Me.cmdAppliquer.TabIndex = 5
        Me.cmdAppliquer.Text = "Appliquer"
        Me.ToolTip1.SetToolTip(Me.cmdAppliquer, "Appliquer ces paramètres et retracer")
        '
        'cmdZoomInit
        '
        Me.cmdZoomInit.Location = New System.Drawing.Point(8, 328)
        Me.cmdZoomInit.Name = "cmdZoomInit"
        Me.cmdZoomInit.Size = New System.Drawing.Size(96, 24)
        Me.cmdZoomInit.TabIndex = 7
        Me.cmdZoomInit.Tag = ""
        Me.cmdZoomInit.Text = "Zoom Init."
        Me.ToolTip1.SetToolTip(Me.cmdZoomInit, "Ré-initialiser le zoom")
        '
        'chkEffacerImg
        '
        Me.chkEffacerImg.Location = New System.Drawing.Point(8, 232)
        Me.chkEffacerImg.Name = "chkEffacerImg"
        Me.chkEffacerImg.Size = New System.Drawing.Size(104, 24)
        Me.chkEffacerImg.TabIndex = 3
        Me.chkEffacerImg.Text = "Effacer l'image"
        Me.ToolTip1.SetToolTip(Me.chkEffacerImg, "Effacer l'image à chaque affinage des pixels (en mode remplissage)")
        '
        'pbAvancement
        '
        Me.pbAvancement.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pbAvancement.Location = New System.Drawing.Point(8, 454)
        Me.pbAvancement.Name = "pbAvancement"
        Me.pbAvancement.Size = New System.Drawing.Size(412, 21)
        Me.pbAvancement.TabIndex = 13
        '
        'nudNbIterationsMax
        '
        Me.nudNbIterationsMax.Location = New System.Drawing.Point(131, 56)
        Me.nudNbIterationsMax.Maximum = New Decimal(New Integer() {32767, 0, 0, 0})
        Me.nudNbIterationsMax.Name = "nudNbIterationsMax"
        Me.nudNbIterationsMax.Size = New System.Drawing.Size(88, 20)
        Me.nudNbIterationsMax.TabIndex = 2
        Me.nudNbIterationsMax.Value = New Decimal(New Integer() {166, 0, 0, 0})
        '
        'lblIterationMax
        '
        Me.lblIterationMax.Location = New System.Drawing.Point(128, 39)
        Me.lblIterationMax.Name = "lblIterationMax"
        Me.lblIterationMax.Size = New System.Drawing.Size(80, 16)
        Me.lblIterationMax.TabIndex = 16
        Me.lblIterationMax.Text = "Itération max.:"
        Me.ToolTip1.SetToolTip(Me.lblIterationMax, "Itération maximum de l'équation Z -> Z^degré + C")
        '
        'cmdZoomMoins
        '
        Me.cmdZoomMoins.Location = New System.Drawing.Point(8, 296)
        Me.cmdZoomMoins.Name = "cmdZoomMoins"
        Me.cmdZoomMoins.Size = New System.Drawing.Size(96, 24)
        Me.cmdZoomMoins.TabIndex = 6
        Me.cmdZoomMoins.Tag = ""
        Me.cmdZoomMoins.Text = "Zoom -"
        Me.ToolTip1.SetToolTip(Me.cmdZoomMoins, "Reculer le zoom")
        '
        'chkModeDetailIterations
        '
        Me.chkModeDetailIterations.Location = New System.Drawing.Point(120, 232)
        Me.chkModeDetailIterations.Name = "chkModeDetailIterations"
        Me.chkModeDetailIterations.Size = New System.Drawing.Size(104, 24)
        Me.chkModeDetailIterations.TabIndex = 4
        Me.chkModeDetailIterations.Text = "Détail itérations"
        Me.ToolTip1.SetToolTip(Me.chkModeDetailIterations, "Afficher le détail des itérations sur un pixel pointé à la souris")
        '
        'txtJuliaY
        '
        Me.txtJuliaY.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtJuliaY.Location = New System.Drawing.Point(112, 88)
        Me.txtJuliaY.Name = "txtJuliaY"
        Me.txtJuliaY.Size = New System.Drawing.Size(96, 20)
        Me.txtJuliaY.TabIndex = 1
        Me.txtJuliaY.Text = "0"
        Me.ToolTip1.SetToolTip(Me.txtJuliaY, "Saisissez directement la valeur Y du paramètre de Julia")
        '
        'txtJuliaX
        '
        Me.txtJuliaX.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtJuliaX.Location = New System.Drawing.Point(112, 56)
        Me.txtJuliaX.Name = "txtJuliaX"
        Me.txtJuliaX.Size = New System.Drawing.Size(96, 20)
        Me.txtJuliaX.TabIndex = 0
        Me.txtJuliaX.Text = "0"
        Me.ToolTip1.SetToolTip(Me.txtJuliaX, "Saisissez directement la valeur X du paramètre de Julia")
        '
        'pbxJulia
        '
        Me.pbxJulia.BackColor = System.Drawing.Color.Aqua
        Me.pbxJulia.Location = New System.Drawing.Point(8, 8)
        Me.pbxJulia.Name = "pbxJulia"
        Me.pbxJulia.Size = New System.Drawing.Size(100, 100)
        Me.pbxJulia.TabIndex = 20
        Me.pbxJulia.TabStop = False
        Me.ToolTip1.SetToolTip(Me.pbxJulia, "Sélectionner le paramètre de Julia en cliquant dans la zone")
        '
        'pnlPalette
        '
        Me.pnlPalette.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.pnlPalette.Controls.Add(Me.chkLisser)
        Me.pnlPalette.Controls.Add(Me.chkFrontiereUnie)
        Me.pnlPalette.Controls.Add(Me.chkPaletteAleatoire)
        Me.pnlPalette.Controls.Add(Me.pbxVerif)
        Me.pnlPalette.Controls.Add(Me.Label3)
        Me.pnlPalette.Controls.Add(Me.nudNbCyclesDegrade)
        Me.pnlPalette.Controls.Add(Me.nudPremCouleur)
        Me.pnlPalette.Controls.Add(Me.Label2)
        Me.pnlPalette.Controls.Add(Me.Label1)
        Me.pnlPalette.Controls.Add(Me.nudNbCouleurs)
        Me.pnlPalette.Controls.Add(Me.chkMire)
        Me.pnlPalette.Controls.Add(Me.chkPaletteSysteme)
        Me.pnlPalette.Location = New System.Drawing.Point(230, 16)
        Me.pnlPalette.Name = "pnlPalette"
        Me.pnlPalette.Size = New System.Drawing.Size(155, 320)
        Me.pnlPalette.TabIndex = 11
        Me.ToolTip1.SetToolTip(Me.pnlPalette, "Options de la palette de couleur")
        '
        'chkLisser
        '
        Me.chkLisser.Location = New System.Drawing.Point(87, 145)
        Me.chkLisser.Name = "chkLisser"
        Me.chkLisser.Size = New System.Drawing.Size(61, 24)
        Me.chkLisser.TabIndex = 24
        Me.chkLisser.Text = "Lisser"
        Me.ToolTip1.SetToolTip(Me.chkLisser, "Lisser les couleurs avec un effet de flou (pour l'algorithme rapide seulement)")
        '
        'chkFrontiereUnie
        '
        Me.chkFrontiereUnie.Location = New System.Drawing.Point(87, 202)
        Me.chkFrontiereUnie.Name = "chkFrontiereUnie"
        Me.chkFrontiereUnie.Size = New System.Drawing.Size(61, 24)
        Me.chkFrontiereUnie.TabIndex = 23
        Me.chkFrontiereUnie.Text = "Front."
        Me.ToolTip1.SetToolTip(Me.chkFrontiereUnie, "Afficher la frontière sous une couleur unie, ou sinon interpoler les couleurs (po" &
            "ur l'algorithme rapide seulement)")
        '
        'chkPaletteAleatoire
        '
        Me.chkPaletteAleatoire.Location = New System.Drawing.Point(12, 241)
        Me.chkPaletteAleatoire.Name = "chkPaletteAleatoire"
        Me.chkPaletteAleatoire.Size = New System.Drawing.Size(104, 24)
        Me.chkPaletteAleatoire.TabIndex = 8
        Me.chkPaletteAleatoire.Text = "Palette alétoire"
        Me.ToolTip1.SetToolTip(Me.chkPaletteAleatoire, "Répartir les dégradés de couleur aléatoirement")
        '
        'pbxVerif
        '
        Me.pbxVerif.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pbxVerif.BackColor = System.Drawing.Color.LimeGreen
        Me.pbxVerif.Location = New System.Drawing.Point(37, 271)
        Me.pbxVerif.Name = "pbxVerif"
        Me.pbxVerif.Size = New System.Drawing.Size(79, 25)
        Me.pbxVerif.TabIndex = 22
        Me.pbxVerif.TabStop = False
        Me.ToolTip1.SetToolTip(Me.pbxVerif, "L'indicateur est rouge si le nombre des 1024 couleurs n'est pas divisible par le " &
            "nombre de cycles (jusqu'à 32 cycles), il indique le risque d'avoir des défauts v" &
            "isibles dans la palette.")
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(9, 183)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(80, 16)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Nb. cycles :"
        Me.ToolTip1.SetToolTip(Me.Label3, "Nombre de cycles de dégradées de couleur dans la palette à 1024 couleurs")
        '
        'nudNbCyclesDegrade
        '
        Me.nudNbCyclesDegrade.Location = New System.Drawing.Point(12, 202)
        Me.nudNbCyclesDegrade.Maximum = New Decimal(New Integer() {512, 0, 0, 0})
        Me.nudNbCyclesDegrade.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudNbCyclesDegrade.Name = "nudNbCyclesDegrade"
        Me.nudNbCyclesDegrade.Size = New System.Drawing.Size(52, 20)
        Me.nudNbCyclesDegrade.TabIndex = 7
        Me.nudNbCyclesDegrade.Value = New Decimal(New Integer() {32, 0, 0, 0})
        '
        'nudPremCouleur
        '
        Me.nudPremCouleur.Location = New System.Drawing.Point(12, 145)
        Me.nudPremCouleur.Maximum = New Decimal(New Integer() {167, 0, 0, 0})
        Me.nudPremCouleur.Name = "nudPremCouleur"
        Me.nudPremCouleur.Size = New System.Drawing.Size(52, 20)
        Me.nudPremCouleur.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(9, 126)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(80, 16)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "1ère couleur :"
        Me.ToolTip1.SetToolTip(Me.Label2, "Numéro de la 1ère couleur")
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(9, 67)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(80, 16)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Nb. couleurs :"
        Me.ToolTip1.SetToolTip(Me.Label1, "Nombre de couleurs")
        '
        'nudNbCouleurs
        '
        Me.nudNbCouleurs.Location = New System.Drawing.Point(12, 86)
        Me.nudNbCouleurs.Maximum = New Decimal(New Integer() {167, 0, 0, 0})
        Me.nudNbCouleurs.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudNbCouleurs.Name = "nudNbCouleurs"
        Me.nudNbCouleurs.Size = New System.Drawing.Size(52, 20)
        Me.nudNbCouleurs.TabIndex = 3
        Me.nudNbCouleurs.Value = New Decimal(New Integer() {167, 0, 0, 0})
        '
        'chkMire
        '
        Me.chkMire.Location = New System.Drawing.Point(12, 8)
        Me.chkMire.Name = "chkMire"
        Me.chkMire.Size = New System.Drawing.Size(52, 24)
        Me.chkMire.TabIndex = 0
        Me.chkMire.Text = "Mire"
        Me.ToolTip1.SetToolTip(Me.chkMire, "Afficher la palette")
        '
        'chkPaletteSysteme
        '
        Me.chkPaletteSysteme.Location = New System.Drawing.Point(12, 38)
        Me.chkPaletteSysteme.Name = "chkPaletteSysteme"
        Me.chkPaletteSysteme.Size = New System.Drawing.Size(104, 24)
        Me.chkPaletteSysteme.TabIndex = 1
        Me.chkPaletteSysteme.Text = "Palette système"
        Me.ToolTip1.SetToolTip(Me.chkPaletteSysteme, "Utiliser la palette système prédéfinie (167 couleurs max.), ou sinon la palette d" &
            "e 1024 couleurs dégradées en cycle(s)")
        '
        'chkDecimal
        '
        Me.chkDecimal.Location = New System.Drawing.Point(8, 358)
        Me.chkDecimal.Name = "chkDecimal"
        Me.chkDecimal.Size = New System.Drawing.Size(73, 24)
        Me.chkDecimal.TabIndex = 17
        Me.chkDecimal.Text = "Décimal"
        Me.ToolTip1.SetToolTip(Me.chkDecimal, "Calcul en Decimal au lieu de Double (plus lent, mais le zoom peut aller plus loin" &
            ")")
        '
        'chkAlgoRapide
        '
        Me.chkAlgoRapide.Location = New System.Drawing.Point(120, 358)
        Me.chkAlgoRapide.Name = "chkAlgoRapide"
        Me.chkAlgoRapide.Size = New System.Drawing.Size(73, 24)
        Me.chkAlgoRapide.TabIndex = 18
        Me.chkAlgoRapide.Text = "Rapide"
        Me.ToolTip1.SetToolTip(Me.chkAlgoRapide, "Utiliser l'algorithme rapide (degré 2 seulement)")
        '
        'chkModeTranslation
        '
        Me.chkModeTranslation.Location = New System.Drawing.Point(24, 23)
        Me.chkModeTranslation.Name = "chkModeTranslation"
        Me.chkModeTranslation.Size = New System.Drawing.Size(104, 24)
        Me.chkModeTranslation.TabIndex = 19
        Me.chkModeTranslation.Text = "Translation"
        Me.ToolTip1.SetToolTip(Me.chkModeTranslation, "Définir un chemin de translation à la souris")
        '
        'pnlVideo
        '
        Me.pnlVideo.Controls.Add(Me.chkModeTranslation)
        Me.pnlVideo.Location = New System.Drawing.Point(402, 16)
        Me.pnlVideo.Name = "pnlVideo"
        Me.pnlVideo.Size = New System.Drawing.Size(22, 320)
        Me.pnlVideo.TabIndex = 20
        Me.ToolTip1.SetToolTip(Me.pnlVideo, "Configuration du mode vidéo")
        '
        'lblZoom
        '
        Me.lblZoom.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblZoom.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblZoom.Location = New System.Drawing.Point(8, 385)
        Me.lblZoom.Name = "lblZoom"
        Me.lblZoom.Size = New System.Drawing.Size(412, 66)
        Me.lblZoom.TabIndex = 12
        Me.lblZoom.Text = "Prm Zoom"
        '
        'panelJulia
        '
        Me.panelJulia.Controls.Add(Me.txtJuliaY)
        Me.panelJulia.Controls.Add(Me.txtJuliaX)
        Me.panelJulia.Controls.Add(Me.lblPrmJulia)
        Me.panelJulia.Controls.Add(Me.pbxJulia)
        Me.panelJulia.Enabled = False
        Me.panelJulia.Location = New System.Drawing.Point(8, 104)
        Me.panelJulia.Name = "panelJulia"
        Me.panelJulia.Size = New System.Drawing.Size(216, 120)
        Me.panelJulia.TabIndex = 5
        '
        'lblPrmJulia
        '
        Me.lblPrmJulia.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPrmJulia.Location = New System.Drawing.Point(120, 16)
        Me.lblPrmJulia.Name = "lblPrmJulia"
        Me.lblPrmJulia.Size = New System.Drawing.Size(88, 32)
        Me.lblPrmJulia.TabIndex = 21
        Me.lblPrmJulia.Text = "Paramètres de Julia : X et Y"
        '
        'frmConfig
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(428, 480)
        Me.Controls.Add(Me.pnlVideo)
        Me.Controls.Add(Me.chkAlgoRapide)
        Me.Controls.Add(Me.chkDecimal)
        Me.Controls.Add(Me.pnlPalette)
        Me.Controls.Add(Me.panelJulia)
        Me.Controls.Add(Me.chkModeDetailIterations)
        Me.Controls.Add(Me.lblZoom)
        Me.Controls.Add(Me.cmdZoomMoins)
        Me.Controls.Add(Me.lblIterationMax)
        Me.Controls.Add(Me.nudNbIterationsMax)
        Me.Controls.Add(Me.pbAvancement)
        Me.Controls.Add(Me.chkEffacerImg)
        Me.Controls.Add(Me.cmdZoomInit)
        Me.Controls.Add(Me.cmdAppliquer)
        Me.Controls.Add(Me.cmdStop)
        Me.Controls.Add(Me.cmdPause)
        Me.Controls.Add(Me.LblDegre)
        Me.Controls.Add(Me.nudDegre)
        Me.Controls.Add(Me.grpbTypeFract)
        Me.Name = "frmConfig"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Configuration de Fractalis"
        Me.grpbTypeFract.ResumeLayout(False)
        CType(Me.nudDegre, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudNbIterationsMax, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pbxJulia, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlPalette.ResumeLayout(False)
        CType(Me.pbxVerif, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudNbCyclesDegrade, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudPremCouleur, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudNbCouleurs, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlVideo.ResumeLayout(False)
        Me.panelJulia.ResumeLayout(False)
        Me.panelJulia.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Propriétés"

    Public Property bPaletteModifiee() As Boolean
        Get
            Return m_bPaletteModifiee
        End Get
        Set(value As Boolean)
            m_bPaletteModifiee = value
        End Set
    End Property

    Public Property bMire() As Boolean
        Get
            Return chkMire.Checked
        End Get
        Set(bVal As Boolean)
            chkMire.Checked = bVal
        End Set
    End Property

    Public Property bFrontiereUnie() As Boolean
        Get
            Return chkFrontiereUnie.Checked
        End Get
        Set(bVal As Boolean)
            chkFrontiereUnie.Checked = bVal
        End Set
    End Property

    Public Property bLisser() As Boolean
        Get
            Return chkLisser.Checked
        End Get
        Set(bVal As Boolean)
            chkLisser.Checked = bVal
        End Set
    End Property

    Public Property bPaletteSysteme() As Boolean
        Get
            Return chkPaletteSysteme.Checked
        End Get
        Set(bVal As Boolean)
            chkPaletteSysteme.Checked = bVal
            m_bPaletteModifiee = True
        End Set
    End Property

    Public Property iNbCouleurs%()
        Get
            Return CInt(nudNbCouleurs.Value) ' nud : NumericUpDown
        End Get
        Set(iVal%)
            nudNbCouleurs.Value = iVal
        End Set
    End Property

    Public Property iPremCouleur%()
        Get
            Return CInt(nudPremCouleur.Value) ' nud : NumericUpDown
        End Get
        Set(iVal%)
            nudPremCouleur.Value = iVal
        End Set
    End Property

    Public Property iNbCyclesDegrade%()
        Get
            VerifierPalette()
            Dim iNbCyclesDegrade0% = CInt(nudNbCyclesDegrade.Value) ' nud : NumericUpDown
            Return iNbCyclesDegrade0
        End Get
        Set(iVal%)
            nudNbCyclesDegrade.Value = iVal
            m_bPaletteModifiee = True
        End Set
    End Property

    Public Property bPaletteAleatoire() As Boolean
        Get
            Return chkPaletteAleatoire.Checked
        End Get
        Set(bVal As Boolean)
            chkPaletteAleatoire.Checked = bVal
            m_bPaletteModifiee = True
        End Set
    End Property

    Public Property iDegre%()
        Get
            Return CInt(nudDegre.Value) ' nud : NumericUpDown
        End Get
        Set(iVal%)
            nudDegre.Value = iVal
        End Set
    End Property

    Public Property iNbIterationsMax%()
        Get
            Return CInt(nudNbIterationsMax.Value)
        End Get
        Set(iVal%)
            nudNbIterationsMax.Value = iVal
        End Set
    End Property

    Public Property ptfJulia() As PointF
        Get
            Return m_ptfJulia
        End Get
        Set(rVal As PointF)
            m_ptfJulia = rVal
            MajTxtJulia(bLimiterPrecision:=False)
            pbxJulia.Invalidate()
        End Set
    End Property

    Public Property typeFrac As TFractal
        Get
            Return m_typeFract
        End Get
        Set(bVal As TFractal)
            m_typeFract = bVal
            Select Case bVal
                Case TFractal.Mandelbrot : rbMandelbrot.Checked = True
                Case TFractal.Julia : rbJulia.Checked = True
                Case TFractal.MandelbrotEtJulia : rbMandelbrotEtJulia.Checked = True
            End Select
        End Set
    End Property

    Public Property bJulia() As Boolean
        Get
            Return m_bJulia
        End Get
        Set(bVal As Boolean)
            m_bJulia = bVal
        End Set
    End Property

    Public Property bEffacerImg() As Boolean
        Get
            Return chkEffacerImg.Checked ' chk : CheckBox
        End Get
        Set(bVal As Boolean)
            chkEffacerImg.Checked = bVal
        End Set
    End Property

    Public Property bDecimal() As Boolean
        Get
            Return chkDecimal.Checked
        End Get
        Set(bVal As Boolean)
            chkDecimal.Checked = bVal
        End Set
    End Property

    Public Property bAlgoRapide() As Boolean
        Get
            Return chkAlgoRapide.Checked
        End Get
        Set(bVal As Boolean)
            chkAlgoRapide.Checked = bVal
        End Set
    End Property

    Public WriteOnly Property iAvancement%()
        Set(iVal%)
            pbAvancement.Value = Math.Min(iVal, 100) ' pb : ProgressBar
        End Set
    End Property

    Public WriteOnly Property sCoordZoom$()
        Set(sVal$)
            lblZoom.Text = sVal ' lbl : Label
        End Set
    End Property

    Public ReadOnly Property bModeDetailIterations() As Boolean
        Get
            Return chkModeDetailIterations.Checked
        End Get
    End Property

    Public Property bModeTranslation() As Boolean
        Get
            Return chkModeTranslation.Checked
        End Get
        Set(bVal As Boolean)
            chkModeTranslation.Checked = bVal
        End Set
    End Property

#End Region

#Region "Gestion de l'interface"

    Private Sub Activation()

        If chkPaletteSysteme.Checked Then

            nudNbCouleurs.Enabled = True
            nudPremCouleur.Enabled = True

            nudNbCyclesDegrade.Enabled = False
            chkPaletteAleatoire.Enabled = False
            pbxVerif.Enabled = False

        Else

            nudNbCouleurs.Enabled = False
            nudPremCouleur.Enabled = False

            nudNbCyclesDegrade.Enabled = True
            chkPaletteAleatoire.Enabled = True
            pbxVerif.Enabled = True

        End If

    End Sub

    Private Sub FrmConfig_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) _
            Handles MyBase.Closing
        ' Ne pas fermer ce formulaire, le cacher seulement
        e.Cancel = True : Me.Hide()
    End Sub

    Private Sub cmdZoomMoins_Click(sender As Object, e As EventArgs) Handles cmdZoomMoins.Click
        RaiseEvent EvZoomMoins()
    End Sub

    Private Sub cmdZoomInit_Click(sender As Object, e As EventArgs) Handles cmdZoomInit.Click
        RaiseEvent EvZoomInit()
    End Sub

    Private Sub cmdAppliquer_Click(sender As Object, e As EventArgs) Handles cmdAppliquer.Click
        RaiseEvent EvAppliquer()
    End Sub

    Private Sub cmdPause_Click(sender As Object, e As EventArgs) Handles cmdPause.Click
        RaiseEvent EvPause()
    End Sub

    Private Sub cmdStop_Click(sender As Object, e As EventArgs) Handles cmdStop.Click
        RaiseEvent EvStop()
    End Sub

    Private Sub pbxJulia_Paint(sender As Object, e As PaintEventArgs) Handles pbxJulia.Paint ' pbx : PictureBox

        ' Tracer le cercle unitaire
        Dim iDiamCercleUnitaire% = 2 * pbxJulia.Width \ iAmplitPrmJulia
        e.Graphics.DrawEllipse(Pens.Red,
            pbxJulia.Width \ 2 - iDiamCercleUnitaire \ 2,
            pbxJulia.Height \ 2 - iDiamCercleUnitaire \ 2,
            iDiamCercleUnitaire, iDiamCercleUnitaire)

        ' Tracer la cible représentant le paramètre de Julia
        Const iDiamCercleCible% = 5
        e.Graphics.DrawEllipse(Pens.Black,
            pbxJulia.Width \ 2 + CInt(pbxJulia.Width *
                m_ptfJulia.X / iAmplitPrmJulia) - iDiamCercleCible \ 2,
            pbxJulia.Height \ 2 + CInt(pbxJulia.Height *
                -m_ptfJulia.Y / iAmplitPrmJulia - iDiamCercleCible \ 2),
            iDiamCercleCible, iDiamCercleCible)

    End Sub

    Private Sub pbPrmJulia_MouseDown(sender As Object, e As MouseEventArgs) Handles pbxJulia.MouseDown
        m_ptfJulia = New PointF(
            CSng(iAmplitPrmJulia *
                (e.X - pbxJulia.Width / 2) / pbxJulia.Width),
            CSng(iAmplitPrmJulia *
                (-e.Y + pbxJulia.Height / 2) / pbxJulia.Height))
        MajTxtJulia(bLimiterPrecision:=True)
        pbxJulia.Invalidate()
    End Sub

    Private Sub MajTxtJulia(bLimiterPrecision As Boolean)
        Dim sFormat$ = ""
        If bLimiterPrecision Then sFormat = "0.00"
        txtJuliaX.Text = m_ptfJulia.X.ToString(sFormat) ' txt : TextBox
        txtJuliaY.Text = m_ptfJulia.Y.ToString(sFormat)
    End Sub

    Private Sub txtJuliaX_TextChanged(sender As Object, e As EventArgs) Handles txtJuliaX.TextChanged
        If bConvTxtToSng(txtJuliaX.Text, m_ptfJulia.X) Then pbxJulia.Invalidate()
    End Sub

    Private Sub txtJuliaY_TextChanged(sender As Object, e As EventArgs) Handles txtJuliaY.TextChanged
        If bConvTxtToSng(txtJuliaY.Text, m_ptfJulia.Y) Then pbxJulia.Invalidate()
    End Sub

    Private Function bConvTxtToSng(sTxt$, ByRef rVal!) As Boolean
        Try
            Dim rVal0! = CSng(sTxt)
            rVal = rVal0
            bConvTxtToSng = True
        Catch
        End Try
    End Function

    Private Sub nudDegre_ValueChanged(sender As Object, e As EventArgs) _
            Handles nudDegre.ValueChanged
        If nudDegre.Value > 2 AndAlso chkAlgoRapide.Checked Then chkAlgoRapide.Checked = False
    End Sub

    Private Sub chkDecimal_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkDecimal.CheckedChanged
    End Sub

    Private Sub chkAlgoRapide_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkAlgoRapide.CheckedChanged
        If chkAlgoRapide.Checked AndAlso nudDegre.Value > 2 Then nudDegre.Value = 2
        If chkAlgoRapide.Checked AndAlso chkModeDetailIterations.Checked Then chkModeDetailIterations.Checked = False
        ' Pour le moment il y a encore un bug : mal centré : on désactive
        'If chkAlgoRapide.Checked AndAlso rbJulia.Checked Then rbJulia.Checked = False
    End Sub

    Private Sub GestionTypeFract()
        m_bJulia = rbJulia.Checked OrElse rbMandelbrotEtJulia.Checked
        panelJulia.Enabled = m_bJulia
        If rbMandelbrot.Checked Then
            m_typeFract = TFractal.Mandelbrot
        ElseIf rbJulia.Checked Then
            m_typeFract = TFractal.Julia
        ElseIf rbMandelbrotEtJulia.Checked Then
            m_typeFract = TFractal.MandelbrotEtJulia
        End If
    End Sub

    Private Sub rbMandelbrot_CheckedChanged(sender As Object, e As System.EventArgs) _
            Handles rbMandelbrot.CheckedChanged
        GestionTypeFract()
    End Sub

    Private Sub rbJulia_CheckedChanged(sender As Object, e As EventArgs) Handles rbJulia.CheckedChanged
        GestionTypeFract()
    End Sub

    Private Sub rbMandelbrotEtJulia_CheckedChanged(sender As Object, e As EventArgs) _
            Handles rbMandelbrotEtJulia.CheckedChanged
        GestionTypeFract()
    End Sub

    Private Sub chkModeDetailIterations_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkModeDetailIterations.CheckedChanged
        If chkModeDetailIterations.Checked AndAlso chkAlgoRapide.Checked Then chkAlgoRapide.Checked = False
        If chkModeDetailIterations.Checked Then RaiseEvent EvAppliquer()
        RaiseEvent EvDetailIterations()
    End Sub

    Private Sub chkModeTranslation_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkModeTranslation.CheckedChanged
        If chkModeTranslation.Checked Then RaiseEvent EvAppliquer()
        RaiseEvent EvModeTranslation()
    End Sub

    Private Sub chkMire_CheckChanged(sender As Object, e As EventArgs) Handles chkMire.CheckedChanged
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub chkPaletteSysteme_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkPaletteSysteme.CheckedChanged
        Activation()
        m_bPaletteModifiee = True
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub nudPremCouleur_ValueChanged(sender As Object, e As EventArgs) _
            Handles nudPremCouleur.ValueChanged
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub nudNbCouleurs_ValueChanged(sender As Object, e As EventArgs) _
            Handles nudNbCouleurs.ValueChanged
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub nudNbCyclesDegrade_ValueChanged(sender As Object, e As EventArgs) _
            Handles nudNbCyclesDegrade.ValueChanged
        m_bPaletteModifiee = True
        VerifierPalette()
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub chkPaletteAleatoire_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkPaletteAleatoire.CheckedChanged
        m_bPaletteModifiee = True
        If chkMire.Checked Then RaiseEvent EvAppliquer()
    End Sub

    Private Sub chkLisser_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkLisser.CheckedChanged
    End Sub

    Private Sub chkFrontiereUnie_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkFrontiereUnie.CheckedChanged
        If Not chkFrontiereUnie.Checked AndAlso chkAlgoRapide.Checked Then chkAlgoRapide.Checked = False
    End Sub

    Private Sub VerifierPalette()
        pbxVerif.Invalidate()
    End Sub

    Private Sub pbxVerif_paint(sender As Object, e As PaintEventArgs) Handles pbxVerif.Paint ' pbx : PictureBox

        Const sTip$ = "L'indicateur est rouge si le nombre des 1024 couleurs n'est pas divisible" &
            " par le nombre de cycles (jusqu'à 32 cycles), il indique le risque d'avoir des défauts" &
            " visibles dans la palette."

        Dim iNbCyclesDegrade% = CInt(nudNbCyclesDegrade.Value)
        Dim iNbCouleurs% = iNbCouleursPalette \ iNbCyclesDegrade
        Dim bErrNbZonesPalette As Boolean = CBool(IIf(iNbCyclesDegrade > 32, False,
            (iNbCouleurs Mod iNbCyclesDegrade) > 0))
        If bErrNbZonesPalette Then
            Dim iReste% = iNbCouleurs Mod iNbCyclesDegrade
            'Debug.WriteLine("Reste : " & iReste)
            ToolTip1.SetToolTip(pbxVerif, sTip & " : 1024\" & iNbCyclesDegrade & "=" & iNbCouleurs & " mod " & iNbCyclesDegrade & " = " & iReste)
            e.Graphics.FillRectangle(New SolidBrush(Color.Red), 0, 0, pbxVerif.Width, pbxVerif.Height)
        Else
            e.Graphics.FillRectangle(New SolidBrush(Color.LimeGreen), 0, 0, pbxVerif.Width, pbxVerif.Height)
            ToolTip1.SetToolTip(pbxVerif, sTip)
        End If

    End Sub

#End Region

End Class