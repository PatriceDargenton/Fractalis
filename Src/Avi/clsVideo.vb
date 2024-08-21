
Public Class clsVideo

    ' Frame Rate: 30 is preferred. 23.98, 24, 25, 29.97 are also acceptable.
    ' https://support.google.com/youtube/answer/58134?hl=en&ref_topic=2888603
    ' Pareil !
    Private Const iFrameRate% = 30

    Public m_bCompression As Boolean = True

    Public m_bVideoEnCours As Boolean = False
    Public m_bVideoTerminee As Boolean = False

    ' AVI : « Imbrication Audio Vidéo »
    Private m_aviManager As AviFile.AviManager
    Private m_aviStream As AviFile.VideoStream

    Public Function bInitialiser(sCheminVideo$) As Boolean

        If Is64BitProcess() Then
            m_bVideoEnCours = False
            m_bVideoTerminee = False
            Return True
        End If

        Try
            If Not IsNothing(m_aviManager) Then m_aviManager.Close()

            If IO.File.Exists(sCheminVideo) Then
                If Not bSupprimerFichier(sCheminVideo, bPromptErr:=True) Then Exit Function
            End If

            m_aviManager = New AviFile.AviManager(sCheminVideo, open:=False)
            m_aviStream = Nothing
            Return True
        Catch ex As Exception
            AfficherMsgErreur2(ex, "clsVideo.bInitialiser")
            Return False
        Finally
            m_bVideoEnCours = False
            m_bVideoTerminee = False
        End Try

    End Function

    Public Function bAjouterImage(bmp As Bitmap) As Boolean

        If Is64BitProcess() Then m_bVideoEnCours = True : Return True

        If IsNothing(m_aviStream) Then
            If IsNothing(m_aviManager) Then
                MsgBox("clsVideo.bInitialiser oublié !")
                Return False
            End If
            Try
                m_aviStream = m_aviManager.AddVideoStream(isCompressed:=m_bCompression,
                    frameRate:=iFrameRate, firstFrame:=bmp)
                m_bVideoEnCours = True
                Return True
            Catch ex As Exception
                AfficherMsgErreur2(ex, "clsVideo.bAjouterImage", ,
                    "Cause possible : forcer 32 bits")
                m_bVideoEnCours = False
                Return False
            End Try
        Else
            Try
                m_aviStream.AddFrame(bmp)
                Return True
            Catch ex As Exception
                AfficherMsgErreur2(ex, "clsVideo.bAjouterImage")
                Return False
            End Try
        End If

    End Function

    Public Function bTerminer() As Boolean

        If Is64BitProcess() Then
            m_bVideoTerminee = True
            m_bVideoEnCours = False
            Return True
        End If

        Try
            If m_bVideoEnCours Then m_aviManager.Close()
            m_bVideoTerminee = True
            'Beep(600, 20)
            Return True
        Catch ex As Exception
            'Beep(400, 20)
            AfficherMsgErreur2(ex, "clsVideo.bTerminer")
            Return False
        Finally
            m_aviManager = Nothing
            m_aviStream = Nothing
            m_bVideoEnCours = False
        End Try

    End Function

End Class