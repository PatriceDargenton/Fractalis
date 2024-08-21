

Module modUtil

    Public m_sTitreMsg$ = sTitreMsg

    Public Sub AfficherMsgErreur2(ByRef Ex As Exception, Optional sTitreFct$ = "", Optional sInfo$ = "",
            Optional sDetailMsgErr$ = "", Optional bCopierMsgPressePapier As Boolean = True,
            Optional ByRef sMsgErrFinal$ = "")

        If Not Cursor.Current.Equals(Cursors.Default) Then _
            Cursor.Current = Cursors.Default
        Dim sMsg$ = ""
        If sTitreFct <> "" Then sMsg = "Fonction : " & sTitreFct
        If sInfo <> "" Then sMsg &= vbCrLf & sInfo
        If sDetailMsgErr <> "" Then sMsg &= vbCrLf & sDetailMsgErr
        If Ex.Message <> "" Then
            sMsg &= vbCrLf & Ex.Message.Trim
            If Not IsNothing(Ex.InnerException) Then _
                sMsg &= vbCrLf & Ex.InnerException.Message
        End If
        If bCopierMsgPressePapier Then CopierPressePapier(sMsg)
        sMsgErrFinal = sMsg
        MsgBox(sMsg, MsgBoxStyle.Critical)

    End Sub

    Public Sub CopierPressePapier(sInfo$)

        ' Copier des informations dans le presse-papier de Windows
        ' (elles resteront jusqu'à ce que l'application soit fermée)

        Try
            Dim dataObj As New DataObject
            dataObj.SetData(DataFormats.Text, sInfo)
            Clipboard.SetDataObject(dataObj)
        Catch ex As Exception
            ' Le presse-papier peut être indisponible
            AfficherMsgErreur2(ex, "CopierPressePapier", bCopierMsgPressePapier:=False)
        End Try

    End Sub

    Public Function Is64BitProcess() As Boolean
        Return (IntPtr.Size = 8)
    End Function

    Public Sub LibererRessourceDotNet()

        ' 19/01/2011 Il faut appeler 2x :
        '  cf. All-In-One Code Framework\Visual Studio 2008\VBAutomateWord

        ' Clean up the unmanaged Word COM resources by forcing a garbage 
        ' collection as soon as the calling function is off the stack (at 
        ' which point these objects are no longer rooted).
        GC.Collect()
        GC.WaitForPendingFinalizers()
        ' GC needs to be called twice in order to get the Finalizers called 
        ' - the first time in, it simply makes a list of what is to be 
        ' finalized, the second time in, it actually the finalizing. Only 
        ' then will the object do its automatic ReleaseComObject.
        GC.Collect()
        GC.WaitForPendingFinalizers()

        TraiterMsgSysteme_DoEvents()

    End Sub

    Public Sub TraiterMsgSysteme_DoEvents()

        'Try
        Application.DoEvents() ' Peut planter avec OWC : Try Catch nécessaire
        'Threading.Thread.Sleep(0) ' Pas totalement équivalent à DoEvents()
        'Catch
        'End Try

    End Sub

    Public Function bFichierExiste(sCheminFichier$, Optional bPrompt As Boolean = False) As Boolean

        ' Retourne True si un fichier correspondant est trouvé
        ' Ne fonctionne pas avec un filtre, par ex. du type C:\*.txt
        Dim bFichierExiste0 As Boolean = IO.File.Exists(sCheminFichier)

        If Not bFichierExiste0 AndAlso bPrompt Then _
            MsgBox("Impossible de trouver le fichier :" & vbLf & sCheminFichier,
                MsgBoxStyle.Critical, m_sTitreMsg & " - Fichier introuvable")

        Return bFichierExiste0

    End Function

    Public Const sCauseErrPoss$ =
        "Le fichier est peut-être protégé en écriture ou bien verrouillé par une autre application"
    Public Const sCauseErrPossDossier$ =
        "Le dossier est peut-être protégé en écriture" & vbLf &
        "ou bien un fichier est verrouillé par une autre application"

    ' Attribut pour éviter que l'IDE s'interrompt en cas d'exception
    <System.Diagnostics.DebuggerStepThrough()>
    Public Function bFichierAccessible(sCheminFichier$,
            Optional bPrompt As Boolean = False,
            Optional bPromptFermer As Boolean = False,
            Optional bInexistOk As Boolean = False,
            Optional bPromptRetenter As Boolean = False,
            Optional bLectureSeule As Boolean = False,
            Optional bEcriture As Boolean = True) As Boolean

        ' Vérifier si un fichier est accessible en écriture (non verrouillé par Excel par exemple)
        ' bEcriture = True par défaut (pour la rétrocompatibilité de la fct bFichierAccessible)
        ' Nouveau : Simple lecture : Mettre bEcriture = False
        ' On conserve l'option bLectureSeule pour alerter qu'un fichier doit être fermé
        '  par l'utilisateur (par exemple un classeur Excel ouvert)

        bFichierAccessible = False

        If bInexistOk Then
            ' Avec cette option, ne pas renvoyer Faux si le fichier n'existe pas
            If Not bFichierExiste(sCheminFichier) Then ' Et ne pas alerter non plus
                bFichierAccessible = True
                Exit Function
            End If
        Else
            If Not bFichierExiste(sCheminFichier, bPrompt) Then
                Exit Function
            End If
        End If

Retenter:
        Dim reponse As MsgBoxResult = MsgBoxResult.Cancel
        Try
            ' Si Excel a verrouillé le fichier, une simple ouverture en lecture
            '  est permise à condition de préciser aussi IO.FileShare.ReadWrite
            Dim mode As IO.FileMode = IO.FileMode.Open
            Dim access As IO.FileAccess = IO.FileAccess.ReadWrite
            If Not bEcriture Then access = IO.FileAccess.Read
            Using fs As New IO.FileStream(sCheminFichier, mode, access, IO.FileShare.ReadWrite)
                fs.Close()
            End Using
            bFichierAccessible = True
        Catch ex As Exception
            Dim msgbs As MsgBoxStyle = MsgBoxStyle.Exclamation
            If bPrompt Then
                AfficherMsgErreur2(ex, "bFichierAccessible",
                    "Impossible d'accéder au fichier :" & vbLf &
                    sCheminFichier, sCauseErrPoss)
            ElseIf bPromptFermer Then
                Dim sQuestion$ = ""
                If bPromptRetenter Then
                    msgbs = msgbs Or MsgBoxStyle.RetryCancel
                    sQuestion = vbLf & "Voulez-vous réessayer ?"
                End If
                ' Attention : le fichier peut aussi être en lecture seule pour diverses raisons !
                ' Certains fichiers peuvent aussi être inaccessibles pour une simple lecture
                '  par ex. certains fichiers du dossier 
                '  \Documents and Settings\All Users\Application Data\Microsoft\Crypto\RSA\MachineKeys\
                If bLectureSeule Then
                    ' Le verrouillage Excel peut ralentir une lecture ODBC,
                    '  mais sinon la lecture directe n'est pas possible, même avec
                    '  IO.FileMode.Open, IO.FileAccess.Read et IO.FileShare.Read ?
                    '  (sauf si le fichier a l'attribut lecture seule) 
                    ' En fait si, à condition de préciser IO.FileShare.ReadWrite
                    reponse = MsgBox(
                        "Veuillez fermer S.V.P. le fichier :" & vbLf &
                        sCheminFichier & sQuestion, msgbs, m_sTitreMsg)
                Else
                    reponse = MsgBox("Le fichier n'est pas accessible en écriture :" & vbLf &
                        sCheminFichier & vbLf &
                        "Le cas échéant, veuillez le fermer, ou bien changer" & vbLf &
                        "ses attributs de protection, ou alors les droits d'accès." &
                        sQuestion, msgbs, m_sTitreMsg)
                End If
            End If
        End Try

        If Not bFichierAccessible And reponse = MsgBoxResult.Retry Then GoTo Retenter

    End Function

    Public Function bSupprimerFichier(sCheminFichier$, Optional bPromptErr As Boolean = False) As Boolean

        ' Vérifier si le fichier existe
        If Not bFichierExiste(sCheminFichier) Then Return True

        If Not bFichierAccessible(sCheminFichier,
            bPromptFermer:=bPromptErr, bPromptRetenter:=bPromptErr) Then Return False

        ' Supprimer le fichier
        Try
            IO.File.Delete(sCheminFichier)
            Return True

        Catch ex As Exception
            If bPromptErr Then _
                AfficherMsgErreur2(ex, "Impossible de supprimer le fichier :" & vbLf & sCheminFichier, sCauseErrPoss)
            Return False
        End Try

    End Function

#Region "Rnd"

    Private Const bRndClassique As Boolean = False

    Public Sub InitRnd()
        ' 20/11/2011 Utile seulement avec bRndClassique
        If bRndClassique Then VBMath.Randomize()
    End Sub

    Public Function iRandomiser%(iMin%, iMax%)

        Dim iRes As Integer = 0
        If iMin = iMax Then Return iMin

        Dim rRnd!
        If bRndClassique Then
            rRnd = VBMath.Rnd()
        Else
            Static rRndGenerateur As New Random
            Dim rRndDouble As Double = rRndGenerateur.NextDouble
            rRnd = CSng(rRndDouble)
            'rRnd = 1
        End If

        ' On atteint jamais la borne max.
        'Dim rVal! = iMin + rRnd * (iMax - iMin)
        ' 13/11/2011 
        Dim rVal! = iMin + rRnd * (iMax + 1 - iMin)
        ' Fix : Partie entière sans arrondir à l'entier le plus proche
        iRes = iFix(rVal)
        ' Au cas où Rnd() renverrait 1.0 et qq
        If iRes > iMax Then iRes = iMax

        'Debug.WriteLine("Tirage entier entre " & iMin & " et " & iMax & " = " & iRes)

        Return iRes

    End Function

#End Region

    Public Function iFix%(rVal!)

        ' Fix : Partie entière sans arrondir à l'entier le plus proche
        iFix = CInt(IIf(rVal > 0, Math.Floor(rVal), Math.Ceiling(rVal)))

    End Function

End Module