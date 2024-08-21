
' This class has been written by
' * Corinna John (Hannover, Germany)
' * cj@binary-universe.net
' * 
' * You may do with this code whatever you like,
' * except selling it or claiming any rights/ownership.
' * 
' * Please send me a little feedback about what you're
' * using this code for and what changes you'd like to
' * see in later versions. (And please excuse my bad english.)
' * 
' * WARNING: This is experimental code.
' * Please do not expect "Release Quality".

Imports System.Runtime.InteropServices

Namespace AviFile

    Public Class AudioStream : Inherits AviStream

        Public ReadOnly Property CountBitsPerSample() As Integer
            Get
                Return waveFormat.wBitsPerSample
            End Get
        End Property

        Public ReadOnly Property CountSamplesPerSecond() As Integer
            Get
                Return waveFormat.nSamplesPerSec
            End Get
        End Property

        Public ReadOnly Property CountChannels() As Integer
            Get
                Return waveFormat.nChannels
            End Get
        End Property

        ''' <summary>the stream's format</summary>
        Private waveFormat As New Avi.PCMWAVEFORMAT()

        ''' <summary>Initialize an AudioStream for an existing stream</summary>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="aviStream">An IAVISTREAM from [aviFile]</param>
        Public Sub New(aviFile As Integer, aviStream As IntPtr)
            Me.m_aviFile = aviFile
            Me.m_aviStream = aviStream

            Dim size As Integer = Marshal.SizeOf(waveFormat)
            Avi.AVIStreamReadFormat(aviStream, 0, waveFormat, size)
            Dim streamInfo As Avi.AVISTREAMINFO = GetStreamInfo(aviStream)
        End Sub

        ''' <summary>Read the stream's header information</summary>
        ''' <param name="aviStream">The IAVISTREAM to read from</param>
        ''' <returns>AVISTREAMINFO</returns>
        Private Function GetStreamInfo(aviStream As IntPtr) As Avi.AVISTREAMINFO
            Dim streamInfo As New Avi.AVISTREAMINFO()
            Dim result As Integer = Avi.AVIStreamInfo_(aviStream, streamInfo, Marshal.SizeOf(streamInfo))
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamInfo: " & result.ToString())
            End If
            Return streamInfo
        End Function

        ''' <summary>Read the stream's header information</summary>
        ''' <returns>AVISTREAMINFO</returns>
        Public Function GetStreamInfo() As Avi.AVISTREAMINFO
            If WriteCompressed Then
                Return GetStreamInfo(m_compressedStream)
            Else
                Return GetStreamInfo(m_aviStream)
            End If
        End Function

        ''' <summary>Read the stream's format information</summary>
        ''' <returns>PCMWAVEFORMAT</returns>
        Public Function GetFormat() As Avi.PCMWAVEFORMAT
            Dim format As New Avi.PCMWAVEFORMAT()
            Dim size As Integer = Marshal.SizeOf(format)
            Dim result As Integer = Avi.AVIStreamReadFormat(m_aviStream, 0, format, size)
            Return format
        End Function

        ''' <summary>Returns all data needed to copy the stream</summary>
        ''' <remarks>Do not forget to call Marshal.FreeHGlobal and release the raw data pointer</remarks>
        ''' <param name="streamInfo">Receives the header information</param>
        ''' <param name="format">Receives the format</param>
        ''' <param name="streamLength">Receives the length of the stream</param>
        ''' <returns>Pointer to the wave data</returns>
        Public Function GetStreamData(ByRef streamInfo As Avi.AVISTREAMINFO, ByRef format As Avi.PCMWAVEFORMAT, ByRef streamLength As Integer) As IntPtr
            streamInfo = GetStreamInfo()

            format = GetFormat()
            ' Length in bytes = length in samples * length of a sample
            streamLength = Avi.AVIStreamLength(m_aviStream.ToInt32()) * streamInfo.dwSampleSize
            Dim waveData As IntPtr = Marshal.AllocHGlobal(streamLength)

            Dim result As Integer = Avi.AVIStreamRead(m_aviStream, 0, streamLength, waveData,
                streamLength, 0, 0)
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamRead: " & result.ToString())
            End If

            Return waveData
        End Function

        ''' <summary>Copy the stream into a new file</summary>
        ''' <param name="fileName">Name of the new file</param>
        Public Overrides Sub ExportStream(fileName As [String])

            Dim opts As New Avi.AVICOMPRESSOPTIONS_CLASS()
            opts.fccType = CType(Avi.mmioStringToFOURCC("auds", 0), UInt32)
            opts.fccHandler = CType(Avi.mmioStringToFOURCC("CAUD", 0), UInt32)
            opts.dwKeyFrameEvery = 0
            opts.dwQuality = 0
            opts.dwFlags = 0
            opts.dwBytesPerSecond = 0
            opts.lpFormat = New IntPtr(0)
            opts.cbFormat = 0
            opts.lpParms = New IntPtr(0)
            opts.cbParms = 0
            opts.dwInterleaveEvery = 0

            Avi.AVISaveV(fileName, 0, 0, 1, m_aviStream, opts)

        End Sub

    End Class

End Namespace