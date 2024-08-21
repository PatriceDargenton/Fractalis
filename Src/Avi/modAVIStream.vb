
Namespace AviFile

    Public MustInherit Class AviStream

        Protected m_aviFile As Integer
        Protected m_aviStream As IntPtr
        Protected m_compressedStream As IntPtr
        Protected m_writeCompressed As Boolean

        ''' <summary>Pointer to the unmanaged AVI file</summary>
        Friend ReadOnly Property FilePointer() As Integer
            Get
                Return m_aviFile
            End Get
        End Property

        ''' <summary>Pointer to the unmanaged AVI Stream</summary>
        Friend Overridable ReadOnly Property StreamPointer() As IntPtr
            Get
                Return m_aviStream
            End Get
        End Property

        ''' <summary>Flag: The stream is compressed/uncompressed</summary>
        Friend ReadOnly Property WriteCompressed() As Boolean
            Get
                Return m_writeCompressed
            End Get
        End Property

        ''' <summary>Close the stream</summary>
        Public Overridable Sub Close()
            If m_writeCompressed Then
                Avi.AVIStreamRelease(m_compressedStream)
            End If
            Avi.AVIStreamRelease(StreamPointer)
        End Sub

        ''' <summary>Export the stream into a new file</summary>
        ''' <param name="fileName"></param>
        Public MustOverride Sub ExportStream(fileName As [String])

    End Class

End Namespace