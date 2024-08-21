
Imports System.Runtime.InteropServices

Namespace AviFile

    Public Class Avi

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure RGBQUAD
            Public rgbBlue As Byte
            Public rgbGreen As Byte
            Public rgbRed As Byte
            Public rgbReserved As Byte
        End Structure

        Public Shared RGBQUAD_SIZE As Integer = 4
        Public Shared PALETTE_SIZE As Integer = 4 * 256 ' RGBQUAD * 256 colours

        Public Shared ReadOnly streamtypeVIDEO_VIDS As Integer = mmioFOURCC("v"c, "i"c, "d"c, "s"c)
        Public Shared ReadOnly streamtypeAUDIO As Integer = mmioFOURCC("a"c, "u"c, "d"c, "s"c)
        Public Shared ReadOnly streamtypeMIDI As Integer = mmioFOURCC("m"c, "i"c, "d"c, "s"c)
        Public Shared ReadOnly streamtypeTEXT As Integer = mmioFOURCC("t"c, "x"c, "t"c, "s"c)

        Public Const OF_SHARE_DENY_WRITE As Integer = 32
        Public Const OF_WRITE As Integer = 1
        Public Const OF_READWRITE As Integer = 2
        Public Const OF_CREATE As Integer = 4096

        Public Const BMP_MAGIC_COOKIE As Integer = 19778      ' ascii string "BM"
        Public Const AVICOMPRESSF_INTERLEAVE As Integer = &H1 ' interleave
        Public Const AVICOMPRESSF_DATARATE As Integer = &H2   ' use a data rate
        Public Const AVICOMPRESSF_KEYFRAMES As Integer = &H4  ' use keyframes
        Public Const AVICOMPRESSF_VALID As Integer = &H8      ' has valid data
        Public Const AVIIF_KEYFRAME As Integer = &H10

        Public Const ICMF_CHOOSE_KEYFRAME As UInt32 = &H1     ' show KeyFrame Every box
        Public Const ICMF_CHOOSE_DATARATE As UInt32 = &H2     ' show DataRate box
        Public Const ICMF_CHOOSE_PREVIEW As UInt32 = &H4      ' allow expanded preview dialog

        ' macro mmioFOURCC
        Public Shared Function mmioFOURCC(ch0 As Char, ch1 As Char, ch2 As Char, ch3 As Char) As Int32

            Dim i0% = CType(CByte(AscW(ch0)), Int32)
            Dim i1% = CType(CByte(AscW(ch1)), Int32) << 8
            Dim i2% = CType(CByte(AscW(ch2)), Int32) << 16
            Dim i3% = CType(CByte(AscW(ch3)), Int32) << 24
            Dim iRet% = (
                 CType(CByte(AscW(ch0)), Int32) Or
                (CType(CByte(AscW(ch1)), Int32) << 8) Or
                (CType(CByte(AscW(ch2)), Int32) << 16) Or
                (CType(CByte(AscW(ch3)), Int32) << 24))
            'Debug.WriteLine("mmioFOURCC : " & ch0 & ch1 & ch2 & ch3 & " -> " & iRet)
            Return iRet

        End Function

#Region "structure declarations"

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure RECT
            Public left As UInt32
            Public top As UInt32
            Public right As UInt32
            Public bottom As UInt32
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure BITMAPINFOHEADER
            Public biSize As Int32
            Public biWidth As Int32
            Public biHeight As Int32
            Public biPlanes As Int16
            Public biBitCount As Int16
            Public biCompression As Int32
            Public biSizeImage As Int32
            Public biXPelsPerMeter As Int32
            Public biYPelsPerMeter As Int32
            Public biClrUsed As Int32
            Public biClrImportant As Int32
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure BITMAPINFO
            Public bmiHeader As BITMAPINFOHEADER
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
            Public bmiColors As RGBQUAD()
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure PCMWAVEFORMAT
            Public wFormatTag As Short
            Public nChannels As Short
            Public nSamplesPerSec As Integer
            Public nAvgBytesPerSec As Integer
            Public nBlockAlign As Short
            Public wBitsPerSample As Short
            Public cbSize As Short
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure AVISTREAMINFO
            Public fccType As Int32
            Public fccHandler As Int32
            Public dwFlags As Int32
            Public dwCaps As Int32
            Public wPriority As Int16
            Public wLanguage As Int16
            Public dwScale As Int32
            Public dwRate As Int32
            Public dwStart As Int32
            Public dwLength As Int32
            Public dwInitialFrames As Int32
            Public dwSuggestedBufferSize As Int32
            Public dwQuality As Int32
            Public dwSampleSize As Int32
            Public rcFrame As RECT
            Public dwEditCount As Int32
            Public dwFormatChangeCount As Int32
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public szName As UInt16()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure BITMAPFILEHEADER
            Public bfType As Int16
            ' "magic cookie" - must be "BM"
            Public bfSize As Int32
            Public bfReserved1 As Int16
            Public bfReserved2 As Int16
            Public bfOffBits As Int32
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure AVIFILEINFO
            Public dwMaxBytesPerSecond As Int32
            Public dwFlags As Int32
            Public dwCaps As Int32
            Public dwStreams As Int32
            Public dwSuggestedBufferSize As Int32
            Public dwWidth As Int32
            Public dwHeight As Int32
            Public dwScale As Int32
            Public dwRate As Int32
            Public dwLength As Int32
            Public dwEditCount As Int32
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
            Public szFileType As Char()
        End Structure

        ' https://msdn.microsoft.com/en-us/library/windows/desktop/dd756791%28v=vs.85%29.aspx
        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Structure AVICOMPRESSOPTIONS

            '#define streamtypeVIDEO FCC('vids') ' Indicates a video stream
            '#define streamtypeAUDIO FCC('auds') ' Indicates an audio stream
            '#define streamtypeMIDI  FCC('mids') ' Indicates a MIDI stream
            '#define streamtypeTEXT  FCC('txts') ' Indicates a text stream
            Public fccType As UInt32

            Public fccHandler As UInt32
            Public dwKeyFrameEvery As UInt32
            ' only used with AVICOMRPESSF_KEYFRAMES
            Public dwQuality As UInt32
            Public dwBytesPerSecond As UInt32
            ' only used with AVICOMPRESSF_DATARATE
            Public dwFlags As UInt32
            Public lpFormat As IntPtr
            Public cbFormat As UInt32
            Public lpParms As IntPtr
            Public cbParms As UInt32
            Public dwInterleaveEvery As UInt32
        End Structure

        ''' <summary>AviSaveV needs a pointer to a pointer to an AVICOMPRESSOPTIONS structure</summary>
        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Public Class AVICOMPRESSOPTIONS_CLASS
            Public fccType As UInt32
            Public fccHandler As UInt32
            Public dwKeyFrameEvery As UInt32
            ' only used with AVICOMRPESSF_KEYFRAMES
            Public dwQuality As UInt32
            Public dwBytesPerSecond As UInt32
            ' only used with AVICOMPRESSF_DATARATE
            Public dwFlags As UInt32
            Public lpFormat As IntPtr
            Public cbFormat As UInt32
            Public lpParms As IntPtr
            Public cbParms As UInt32
            Public dwInterleaveEvery As UInt32

            Public Function ToStruct() As AVICOMPRESSOPTIONS
                Dim returnVar As New AVICOMPRESSOPTIONS()
                returnVar.fccType = Me.fccType
                returnVar.fccHandler = Me.fccHandler
                returnVar.dwKeyFrameEvery = Me.dwKeyFrameEvery
                returnVar.dwQuality = Me.dwQuality
                returnVar.dwBytesPerSecond = Me.dwBytesPerSecond
                returnVar.dwFlags = Me.dwFlags
                returnVar.lpFormat = Me.lpFormat
                returnVar.cbFormat = Me.cbFormat
                returnVar.lpParms = Me.lpParms
                returnVar.cbParms = Me.cbParms
                returnVar.dwInterleaveEvery = Me.dwInterleaveEvery
                Return returnVar
            End Function
        End Class
#End Region

#Region "method declarations"

        ' Initialize the AVI library
        <DllImport("avifil32.dll")>
        Public Shared Sub AVIFileInit()
        End Sub

        ' Open an AVI file
        <DllImport("avifil32.dll", PreserveSig:=True)>
        Public Shared Function AVIFileOpen(ByRef ppfile As Integer, szFile As [String],
            uMode As Integer, pclsidHandler As Integer) As Integer
        End Function

        ' Get a stream from an open AVI file
        <DllImport("avifil32.dll")>
        Public Shared Function AVIFileGetStream(pfile As Integer, ByRef ppavi As IntPtr,
            fccType As Integer, lParam As Integer) As Integer
        End Function

        ' Get the start position of a stream
        <DllImport("avifil32.dll", PreserveSig:=True)>
        Public Shared Function AVIStreamStart(pavi As Integer) As Integer
        End Function

        ' Get the length of a stream in frames
        <DllImport("avifil32.dll", PreserveSig:=True)>
        Public Shared Function AVIStreamLength(pavi As Integer) As Integer
        End Function

        ' Get information about an open stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamInfo_(pAVIStream As IntPtr, ByRef psi As AVISTREAMINFO,
            lSize As Integer) As Integer
        End Function

        ' Get a pointer to a GETFRAME object (returns 0 on error)
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamGetFrameOpen(pAVIStream As IntPtr,
            ByRef bih As BITMAPINFOHEADER) As Integer
        End Function

        ' Get a pointer to a packed DIB (returns 0 on error)
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamGetFrame(pGetFrameObj As Integer, lPos As Integer) As Integer
        End Function

        ' Create a new stream in an open AVI file
        <DllImport("avifil32.dll")>
        Public Shared Function AVIFileCreateStream(pfile As Integer, ByRef ppavi As IntPtr,
            ByRef ptr_streaminfo As AVISTREAMINFO) As Integer
        End Function

        ' Create an editable stream
        <DllImport("avifil32.dll")>
        Public Shared Function CreateEditableStream(ByRef ppsEditable As IntPtr,
            psSource As IntPtr) As Integer
        End Function

        ' Cut samples from an editable stream
        <DllImport("avifil32.dll")>
        Public Shared Function EditStreamCut(pStream As IntPtr, ByRef plStart As Int32,
            ByRef plLength As Int32, ByRef ppResult As IntPtr) As Integer
        End Function

        ' Copy a part of an editable stream
        <DllImport("avifil32.dll")>
        Public Shared Function EditStreamCopy(pStream As IntPtr, ByRef plStart As Int32,
            ByRef plLength As Int32, ByRef ppResult As IntPtr) As Integer
        End Function

        ' Paste an editable stream into another editable stream
        <DllImport("avifil32.dll")>
        Public Shared Function EditStreamPaste(pStream__1 As IntPtr, ByRef plPos As Int32,
            ByRef plLength As Int32, pstream__2 As IntPtr, lStart As Int32, lLength As Int32) As Integer
        End Function

        ' Change a stream's header values
        <DllImport("avifil32.dll")>
        Public Shared Function EditStreamSetInfo(pStream As IntPtr, ByRef lpInfo As AVISTREAMINFO,
            cbInfo As Int32) As Integer
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVIMakeFileFromStreams(ByRef ppfile As IntPtr, nStreams As Integer,
            ByRef papStreams As IntPtr) As Integer
        End Function

        ' Set the format for a new stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamSetFormat(aviStream As IntPtr, lPos As Int32,
            ByRef lpFormat As BITMAPINFO, cbFormat As Int32) As Integer
        End Function

        ' Set the format for a new stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamSetFormat(aviStream As IntPtr, lPos As Int32,
            ByRef lpFormat As PCMWAVEFORMAT, cbFormat As Int32) As Integer
        End Function

        ' Read the format for a stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamReadFormat(aviStream As IntPtr, lPos As Int32,
            ByRef lpFormat As BITMAPINFO, ByRef cbFormat As Int32) As Integer
        End Function

        ' Read the size of the format for a stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamReadFormat(aviStream As IntPtr, lPos As Int32,
            empty As Integer, ByRef cbFormat As Int32) As Integer
        End Function

        ' Read the format for a stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamReadFormat(aviStream As IntPtr, lPos As Int32,
            ByRef lpFormat As PCMWAVEFORMAT, ByRef cbFormat As Int32) As Integer
        End Function

        ' Write a sample to a stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamWrite(aviStream As IntPtr, lStart As Int32,
            lSamples As Int32, lpBuffer As IntPtr, cbBuffer As Int32, dwFlags As Int32,
            dummy1 As Int32, dummy2 As Int32) As Integer
        End Function

        ' Release the GETFRAME object
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamGetFrameClose(pGetFrameObj As Integer) As Integer
        End Function

        ' Release an open AVI stream
        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamRelease(aviStream As IntPtr) As Integer
        End Function

        ' Release an open AVI file
        <DllImport("avifil32.dll")>
        Public Shared Function AVIFileRelease(pfile As Integer) As Integer
        End Function

        ' Close the AVI library
        <DllImport("avifil32.dll")>
        Public Shared Sub AVIFileExit()
        End Sub

        <DllImport("avifil32.dll")>
        Public Shared Function AVIMakeCompressedStream(ByRef ppsCompressed As IntPtr,
            aviStream As IntPtr, ByRef ao As AVICOMPRESSOPTIONS, dummy As Integer) As Integer
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVISaveOptions(hwnd As IntPtr, uiFlags As UInt32, nStreams As Int32,
            ByRef ppavi As IntPtr, ByRef plpOptions As AVICOMPRESSOPTIONS_CLASS) As Boolean
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVISaveOptionsFree(nStreams As Integer,
            ByRef plpOptions As AVICOMPRESSOPTIONS_CLASS) As Long
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVIFileInfo_(pfile As Integer, ByRef pfi As AVIFILEINFO,
            lSize As Integer) As Integer
        End Function

        <DllImport("winmm.dll", EntryPoint:="mmioStringToFOURCCA")>
        Public Shared Function mmioStringToFOURCC(sz As [String], uFlags As Integer) As Integer
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVIStreamRead(pavi As IntPtr, lStart As Int32, lSamples As Int32,
            lpBuffer As IntPtr, cbBuffer As Int32, plBytes As Int32,
            plSamples As Int32) As Integer
        End Function

        <DllImport("avifil32.dll")>
        Public Shared Function AVISaveV(szFile As [String], empty As Int16, lpfnCallback As Int16,
            nStreams As Int16, ByRef ppavi As IntPtr, ByRef plpOptions As AVICOMPRESSOPTIONS_CLASS) As Integer
        End Function

#End Region

    End Class

End Namespace