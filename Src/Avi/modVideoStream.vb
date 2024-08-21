
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

Imports System.IO
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace AviFile

    Public Class VideoStream : Inherits AviStream

        Public Class typeFlux
            Public Const sTypeAudio$ = "auds"
            Public Const sTypeText$ = "txts"
            Public Const sTypeVideo$ = "vids"
            Public Const sTypeMidi$ = "mids"
        End Class

        ''' <summary>handle for AVIStreamGetFrame</summary>
        Private getFrameObject As Integer

        ''' <summary>size of an imge in bytes, stride*height</summary>
        Private m_frameSize As Integer
        Public ReadOnly Property FrameSize() As Integer
            Get
                Return m_frameSize
            End Get
        End Property

        Protected m_frameRate As Double
        Public ReadOnly Property FrameRate() As Double
            Get
                Return m_frameRate
            End Get
        End Property

        Private m_width As Integer
        Public ReadOnly Property Width() As Integer
            Get
                Return m_width
            End Get
        End Property

        Private m_height As Integer
        Public ReadOnly Property Height() As Integer
            Get
                Return m_height
            End Get
        End Property

        Private m_countBitsPerPixel As Int16
        Public ReadOnly Property CountBitsPerPixel() As Int16
            Get
                Return m_countBitsPerPixel
            End Get
        End Property

        ''' <summary>count of frames in the stream</summary>
        Protected m_countFrames As Integer = 0
        Public ReadOnly Property CountFrames() As Integer
            Get
                Return m_countFrames
            End Get
        End Property

        ''' <summary>Palette for indexed frames</summary>
        Protected m_palette As Avi.RGBQUAD()
        Public ReadOnly Property Palette() As Avi.RGBQUAD()
            Get
                Return m_palette
            End Get
        End Property

        ''' <summary>initial frame index</summary>
        ''' <remarks>Added by M. Covington</remarks>
        Protected m_firstFrame As Integer = 0
        Public ReadOnly Property FirstFrame() As Integer
            Get
                Return m_firstFrame
            End Get
        End Property

        Private m_compressOptions As Avi.AVICOMPRESSOPTIONS
        Public ReadOnly Property CompressOptions() As Avi.AVICOMPRESSOPTIONS
            Get
                Return m_compressOptions
            End Get
        End Property

        Public ReadOnly Property StreamInfo() As Avi.AVISTREAMINFO
            Get
                Return GetStreamInfo(m_aviStream)
            End Get
        End Property

        ''' <summary>Initialize an empty VideoStream</summary>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="writeCompressed">true: Create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="frameSize">Size of one frame in bytes</param>
        ''' <param name="width">Width of each image</param>
        ''' <param name="height">Height of each image</param>
        ''' <param name="format">PixelFormat of the images</param>
        Public Sub New(aviFile As Integer, writeCompressed As Boolean, frameRate As Double,
                frameSize As Integer, width As Integer, height As Integer, format As PixelFormat)

            Me.m_aviFile = aviFile
            Me.m_writeCompressed = writeCompressed
            Me.m_frameRate = frameRate
            Me.m_frameSize = frameSize
            Me.m_width = width
            Me.m_height = height
            Me.m_countBitsPerPixel = ConvertPixelFormatToBitCount(format)
            Me.m_firstFrame = 0

            CreateStream()

        End Sub

        ''' <summary>Initialize a new VideoStream and add the first frame</summary>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="writeCompressed">true: create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="firstFrame">Image to write into the stream as the first frame</param>
        Public Sub New(aviFile As Integer, writeCompressed As Boolean, frameRate As Double,
                firstFrame As Bitmap)
            Initialize(aviFile, writeCompressed, frameRate, firstFrame)
            CreateStream()
            AddFrame(firstFrame)
        End Sub

        ''' <summary>Initialize a new VideoStream and add the first frame</summary>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="compressOptions">true: create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="firstFrame">Image to write into the stream as the first frame</param>
        Public Sub New(aviFile As Integer, compressOptions As Avi.AVICOMPRESSOPTIONS, frameRate As Double,
                firstFrame As Bitmap)
            Initialize(aviFile, True, frameRate, firstFrame)
            CreateStream(compressOptions)
            AddFrame(firstFrame)
        End Sub

        ''' <summary>Initialize a VideoStream for an existing stream</summary>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="aviStream">An IAVISTREAM from [aviFile]</param>
        Public Sub New(aviFile As Integer, aviStream As IntPtr)

            Me.m_aviFile = aviFile
            Me.m_aviStream = aviStream
            Dim streamInfo As Avi.AVISTREAMINFO = GetStreamInfo(aviStream)

            'Avi.BITMAPINFOHEADER bih = new Avi.BITMAPINFOHEADER();
            'int size = Marshal.SizeOf(bih);
            Dim bih As New Avi.BITMAPINFO()
            Dim size As Integer = Marshal.SizeOf(bih.bmiHeader)
            Avi.AVIStreamReadFormat(aviStream, 0, bih, size)

            If bih.bmiHeader.biBitCount < 24 Then
                size = Marshal.SizeOf(bih.bmiHeader) + Avi.PALETTE_SIZE
                Avi.AVIStreamReadFormat(aviStream, 0, bih, size)
                CopyPalette(bih.bmiColors)
            End If

            Me.m_frameRate = CSng(streamInfo.dwRate) / CSng(streamInfo.dwScale)
            Me.m_width = CInt(streamInfo.rcFrame.right)
            Me.m_height = CInt(streamInfo.rcFrame.bottom)
            Me.m_frameSize = bih.bmiHeader.biSizeImage
            Me.m_countBitsPerPixel = bih.bmiHeader.biBitCount
            Me.m_firstFrame = Avi.AVIStreamStart(aviStream.ToInt32())
            Me.m_countFrames = Avi.AVIStreamLength(aviStream.ToInt32())

        End Sub

        ''' <summary>Copy all properties from one VideoStream to another one</summary>
        ''' <remarks>Used by EditableVideoStream</remarks>
        ''' <param name="frameSize"></param><param name="frameRate"></param>
        ''' <param name="width"></param><param name="height"></param>
        ''' <param name="countBitsPerPixel"></param>
        ''' <param name="countFrames"></param><param name="compressOptions"></param>
        Friend Sub New(frameSize As Integer, frameRate As Double, width As Integer, height As Integer,
                countBitsPerPixel As Int16, countFrames As Integer,
                compressOptions As Avi.AVICOMPRESSOPTIONS, writeCompressed As Boolean)

            Me.m_frameSize = frameSize
            Me.m_frameRate = frameRate
            Me.m_width = width
            Me.m_height = height
            Me.m_countBitsPerPixel = countBitsPerPixel
            Me.m_countFrames = countFrames
            Me.m_compressOptions = compressOptions
            Me.m_writeCompressed = writeCompressed
            Me.m_firstFrame = 0

        End Sub

        ''' <summary>Copy a palette</summary>
        ''' <param name="template">Original palette</param>
        Private Sub CopyPalette(template As ColorPalette)

            Me.m_palette = New Avi.RGBQUAD(template.Entries.Length - 1) {}

            For n As Integer = 0 To Me.m_palette.Length - 1
                If n < template.Entries.Length Then
                    Me.m_palette(n).rgbRed = template.Entries(n).R
                    Me.m_palette(n).rgbGreen = template.Entries(n).G
                    Me.m_palette(n).rgbBlue = template.Entries(n).B
                Else
                    Me.m_palette(n).rgbRed = 0
                    Me.m_palette(n).rgbGreen = 0
                    Me.m_palette(n).rgbBlue = 0
                End If
            Next

        End Sub

        ''' <summary>Copy a palette</summary>
        ''' <param name="template">Original palette</param>
        Private Sub CopyPalette(template As Avi.RGBQUAD())

            Me.m_palette = New Avi.RGBQUAD(template.Length - 1) {}

            For n As Integer = 0 To Me.m_palette.Length - 1
                If n < template.Length Then
                    Me.m_palette(n).rgbRed = template(n).rgbRed
                    Me.m_palette(n).rgbGreen = template(n).rgbGreen
                    Me.m_palette(n).rgbBlue = template(n).rgbBlue
                Else
                    Me.m_palette(n).rgbRed = 0
                    Me.m_palette(n).rgbGreen = 0
                    Me.m_palette(n).rgbBlue = 0
                End If
            Next

        End Sub

        ''' <summary>Initialize a new VideoStream</summary>
        ''' <remarks>Used only by constructors</remarks>
        ''' <param name="aviFile">The file that contains the stream</param>
        ''' <param name="writeCompressed">true: create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="firstFrameBitmap">Image to write into the stream as the first frame</param>
        Private Sub Initialize(aviFile As Integer, writeCompressed As Boolean, frameRate As Double,
                firstFrameBitmap As Bitmap)

            Me.m_aviFile = aviFile
            Me.m_writeCompressed = writeCompressed
            Me.m_frameRate = frameRate
            Me.m_firstFrame = 0

            CopyPalette(firstFrameBitmap.Palette)

            Dim bmpData As BitmapData = firstFrameBitmap.LockBits(New Rectangle(0, 0, firstFrameBitmap.Width, firstFrameBitmap.Height), ImageLockMode.[ReadOnly], firstFrameBitmap.PixelFormat)

            Me.m_frameSize = bmpData.Stride * bmpData.Height
            Me.m_width = firstFrameBitmap.Width
            Me.m_height = firstFrameBitmap.Height
            Me.m_countBitsPerPixel = ConvertPixelFormatToBitCount(firstFrameBitmap.PixelFormat)

            firstFrameBitmap.UnlockBits(bmpData)

        End Sub

        ''' <summary>Get the count of bits per pixel from a PixelFormat value</summary>
        ''' <param name="format">One of the PixelFormat members beginning with "Format..." - all others are not supported</param>
        ''' <returns>bit count</returns>
        Private Function ConvertPixelFormatToBitCount(format As PixelFormat) As Int16

            Dim formatName As [String] = format.ToString()
            If formatName.Substring(0, 6) <> "Format" Then
                Throw New Exception("Unknown pixel format: " & formatName)
            End If

            formatName = formatName.Substring(6, 2)
            Dim bitCount As Int16 = 0
            If [Char].IsNumber(formatName(1)) Then
                ' 16, 32, 48
                bitCount = Int16.Parse(formatName)
            Else
                ' 4, 8
                bitCount = Int16.Parse(formatName(0).ToString())
            End If

            Return bitCount

        End Function

        ''' <summary>Returns a PixelFormat value for a specific bit count</summary>
        ''' <param name="bitCount">count of bits per pixel</param>
        ''' <returns>A PixelFormat value for [bitCount]</returns>
        Private Function ConvertBitCountToPixelFormat(bitCount As Integer) As PixelFormat

            Dim formatName As [String]
            If bitCount > 16 Then
                formatName = [String].Format("Format{0}bppRgb", bitCount)
            ElseIf bitCount = 16 Then
                formatName = "Format16bppRgb555"
            Else
                ' < 16
                formatName = [String].Format("Format{0}bppIndexed", bitCount)
            End If

            Return CType([Enum].Parse(GetType(PixelFormat), formatName), PixelFormat)

        End Function

        Private Function GetStreamInfo(aviStream As IntPtr) As Avi.AVISTREAMINFO
            Dim streamInfo As New Avi.AVISTREAMINFO()
            Dim result As Integer = Avi.AVIStreamInfo_(StreamPointer, streamInfo, Marshal.SizeOf(streamInfo))
            If result <> 0 Then
                Throw New Exception("Exception in VideoStreamInfo: " & result.ToString())
            End If
            Return streamInfo
        End Function

        Private Sub GetRateAndScale(ByRef frameRate As Double, ByRef scale As Integer)
            scale = 1
            While frameRate <> CLng(Math.Truncate(frameRate))
                frameRate = frameRate * 10
                scale *= 10
            End While
        End Sub

        ''' <summary>Create a new stream</summary>
        Private Sub CreateStreamWithoutFormat()

            Dim scale As Integer = 1
            Dim rate As Double = m_frameRate
            GetRateAndScale(rate, scale)

            Dim strhdr As New Avi.AVISTREAMINFO()
            strhdr.fccType = Avi.mmioStringToFOURCC("vids", 0)
            strhdr.fccHandler = Avi.mmioStringToFOURCC("CVID", 0)
            strhdr.dwFlags = 0
            strhdr.dwCaps = 0
            strhdr.wPriority = 0
            strhdr.wLanguage = 0
            strhdr.dwScale = CInt(scale)
            strhdr.dwRate = CInt(Math.Truncate(rate))
            ' Frames per Second
            strhdr.dwStart = 0
            strhdr.dwLength = 0
            strhdr.dwInitialFrames = 0
            strhdr.dwSuggestedBufferSize = m_frameSize
            'height_ * stride_;
            strhdr.dwQuality = -1
            ' Default
            strhdr.dwSampleSize = 0
            strhdr.rcFrame.top = 0
            strhdr.rcFrame.left = 0
            strhdr.rcFrame.bottom = CUInt(m_height)
            strhdr.rcFrame.right = CUInt(m_width)
            strhdr.dwEditCount = 0
            strhdr.dwFormatChangeCount = 0
            strhdr.szName = New UInt16(63) {}

            Dim result As Integer = Avi.AVIFileCreateStream(m_aviFile, m_aviStream, strhdr)

            If result <> 0 Then
                Throw New Exception("Exception in AVIFileCreateStream: " & result.ToString())
            End If

        End Sub

        ''' <summary>Create a new stream</summary>
        Private Sub CreateStream()
            CreateStreamWithoutFormat()

            If WriteCompressed Then
                CreateCompressedStream()
                'SetFormat(aviStream, 0);
            Else
            End If
        End Sub

        ''' <summary>Create a new stream</summary>
        Private Sub CreateStream(options As Avi.AVICOMPRESSOPTIONS)
            CreateStreamWithoutFormat()
            CreateCompressedStream(options)
        End Sub

        ''' <summary>Create a compressed stream from an uncompressed stream</summary>
        Private Sub CreateCompressedStream()

            ' Display the compression options dialog...
            Dim options As New Avi.AVICOMPRESSOPTIONS_CLASS()

            ' http://www.fourcc.org/codecs.php
            ' Formats compatibles YouTube :
            ' http://support.google.com/youtube/troubleshooter/2888402

            ' vids = flux vidéo, quel que soit le codec, ne pas changer
            options.fccType = CUInt(Avi.mmioStringToFOURCC(typeFlux.sTypeVideo, 0))
            'Debug.WriteLine("options.fccType=" & options.fccType)
            ' Equivalent :
            'options.fccType = CUInt(Avi.streamtypeVIDEO_VIDS)
            'Debug.WriteLine("options.fccType=" & options.fccType)

            ' Ok, mais avec YouTube, ne marche plus à 100% :
            options.fccHandler = CUInt(Avi.mmioStringToFOURCC("CVID", 0)) ' Cinépack codec : ok !
            ' ->
            'options.fccHandler = 1145656899

            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("DIVX", 0)) ' Ok !

            ' Angelpotion Codec - vids:mp4, vids:mp42, mpeg4v3 and vids:mp43
            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("MP42", 0)) ' BUG

            'options.fccHandler = 1668707181 ' MS vidéo 1 : Ok, mais très compressé !
            'options.fccHandler = 1987410281 ' Intel 1 N&B !
            'options.fccHandler = 808596585  ' Intel 2 N&B !

            'Debug.WriteLine("Cinépack par le code :")
            'Debug.WriteLine("options.fccHandler=" & options.fccHandler)

            ' Cinépack de radius, via l'interface, pas le même codec que CVID ? Pareil !
            'options.fccHandler = 1684633187

            'options.dwQuality = 10000 ' 28/03/2015 Pareil !
            ' Utiliser les prm de cette struct. au lieu de la valeur par défaut
            'options.dwFlags = Avi.AVICOMPRESSF_VALID ' Pareil
            'options.dwFlags = _
            '    Avi.AVICOMPRESSF_INTERLEAVE Or _
            '    Avi.AVICOMPRESSF_DATARATE Or _
            '    Avi.AVICOMPRESSF_KEYFRAMES Or _
            '    Avi.AVICOMPRESSF_VALID()

            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("CVID", 0))
            'Debug.WriteLine("options.fccHandler=" & options.fccHandler)
            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("MPG4", 0))
            'Debug.WriteLine("options.fccHandler=" & options.fccHandler)
            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("MRLE", 0)) ' BUG
            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("XVID", 0)) ' BUG
            'options.fccHandler = CUInt(Avi.mmioStringToFOURCC("VIDS", 0)) ' BUG

            options.lpParms = IntPtr.Zero
            options.lpFormat = IntPtr.Zero

            ' Affiche la boite de dlg
            'Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME Or _
            '    Avi.ICMF_CHOOSE_DATARATE, 1, m_aviStream, options)
            'Debug.WriteLine("Cinépack par l'interface :")
            'Debug.WriteLine("options.fccHandler=" & options.fccHandler)

            ' get the compressed stream
            Me.m_compressOptions = options.ToStruct()
            Dim result As Integer = Avi.AVIMakeCompressedStream(m_compressedStream,
                m_aviStream, m_compressOptions, 0)
            If result <> 0 Then
                Throw New Exception("Exception in AVIMakeCompressedStream: " & result.ToString())
            End If

            Avi.AVISaveOptionsFree(1, options)
            SetFormat(m_compressedStream, 0)

        End Sub

        ''' <summary>Create a compressed stream from an uncompressed stream</summary>
        Private Sub CreateCompressedStream(options As Avi.AVICOMPRESSOPTIONS)

            Dim result As Integer = Avi.AVIMakeCompressedStream(m_compressedStream, m_aviStream, options, 0)
            If result <> 0 Then
                Throw New Exception("Exception in AVIMakeCompressedStream: " & result.ToString())
            End If

            Me.m_compressOptions = options

            SetFormat(m_compressedStream, 0)

        End Sub

        ''' <summary>Add one frame to a new stream</summary>
        ''' <param name="bmp"></param>
        ''' <remarks>
        ''' This works only with uncompressed streams,
        ''' and compressed streams that have not been saved yet.
        ''' Use DecompressToNewFile to edit saved compressed streams.
        ''' </remarks>
        Public Sub AddFrame(bmp As Bitmap)

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY)

            ' NEW 2012-11-10
            If m_countFrames = 0 Then
                CopyPalette(bmp.Palette)
                SetFormat(If(WriteCompressed, m_compressedStream, StreamPointer), m_countFrames)
            End If

            Dim bmpDat As BitmapData = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.[ReadOnly], bmp.PixelFormat)

            Dim result As Integer = Avi.AVIStreamWrite(
                If(WriteCompressed, m_compressedStream, StreamPointer),
                m_countFrames, 1, bmpDat.Scan0,
                CType(bmpDat.Stride * bmpDat.Height, Int32), 0, 0, 0)

            If result <> 0 Then
                Throw New Exception("Exception in VideoStreamWrite: " & result.ToString())
            End If

            bmp.UnlockBits(bmpDat)

            m_countFrames += 1

        End Sub

        ''' <summary>Apply a format to a new stream</summary>
        ''' <param name="aviStream">The IAVISTREAM</param>
        ''' <remarks>
        ''' The format must be set before the first frame can be written,
        ''' and it cannot be changed later.
        ''' </remarks>
        Private Sub SetFormat(aviStream As IntPtr, writePosition As Integer)

            Dim bi As New Avi.BITMAPINFO()
            bi.bmiHeader.biWidth = m_width
            bi.bmiHeader.biHeight = m_height
            bi.bmiHeader.biPlanes = 1
            bi.bmiHeader.biBitCount = m_countBitsPerPixel
            bi.bmiHeader.biSizeImage = m_frameSize
            bi.bmiHeader.biSize = Marshal.SizeOf(bi.bmiHeader)

            If m_countBitsPerPixel < 24 Then
                bi.bmiHeader.biClrUsed = Me.m_palette.Length
                bi.bmiHeader.biClrImportant = Me.m_palette.Length
                bi.bmiColors = New Avi.RGBQUAD(Me.m_palette.Length - 1) {}
                Me.m_palette.CopyTo(bi.bmiColors, 0)
                bi.bmiHeader.biSize += bi.bmiColors.Length * Avi.RGBQUAD_SIZE
            End If

            Dim result As Integer = Avi.AVIStreamSetFormat(aviStream, writePosition, bi, bi.bmiHeader.biSize)
            If result <> 0 Then
                Throw New Exception("Error in VideoStreamSetFormat: " & result.ToString("X"))
            End If

        End Sub

        ''' <summary>Prepare for decompressing frames</summary>
        ''' <remarks>
        ''' This method has to be called before GetBitmap and ExportBitmap.
        ''' Release ressources with GetFrameClose.
        ''' </remarks>
        Public Sub GetFrameOpen()

            Dim streamInfo As Avi.AVISTREAMINFO = GetStreamInfo(StreamPointer)

            ' Open frames

            Dim bih As New Avi.BITMAPINFOHEADER()
            bih.biBitCount = m_countBitsPerPixel
            bih.biClrImportant = 0
            bih.biClrUsed = 0
            bih.biCompression = 0
            bih.biPlanes = 1
            bih.biSize = Marshal.SizeOf(bih)
            bih.biXPelsPerMeter = 0
            bih.biYPelsPerMeter = 0

            ' Corrections by M. Covington:
            ' If these are pre-set, interlaced video is not handled correctly.
            ' Better to give zeroes and let Windows fill them in.
            bih.biHeight = 0
            ' was (Int32)streamInfo.rcFrame.bottom;
            bih.biWidth = 0
            ' was (Int32)streamInfo.rcFrame.right;
            ' Corrections by M. Covington:
            ' Validate the bit count, because some AVI files give a bit count
            ' that is not one of the allowed values in a BitmapInfoHeader.
            ' Here 0 means for Windows to figure it out from other information.
            If bih.biBitCount > 24 Then
                bih.biBitCount = 32
            ElseIf bih.biBitCount > 16 Then
                bih.biBitCount = 24
            ElseIf bih.biBitCount > 8 Then
                bih.biBitCount = 16
            ElseIf bih.biBitCount > 4 Then
                bih.biBitCount = 8
            ElseIf bih.biBitCount > 0 Then
                bih.biBitCount = 4
            End If

            getFrameObject = Avi.AVIStreamGetFrameOpen(StreamPointer, bih)

            If getFrameObject = 0 Then
                Throw New Exception("Exception in VideoStreamGetFrameOpen!")
            End If

        End Sub

        ''' <summary>Export a frame into a bitmap file</summary>
        ''' <param name="position">Position of the frame</param>
        ''' <param name="dstFileName">Name of the file to store the bitmap</param>
        Public Sub ExportBitmap(position As Integer, dstFileName As [String])
            Dim bmp As Bitmap = GetBitmap(position)
            bmp.Save(dstFileName, ImageFormat.Bmp)
            bmp.Dispose()
        End Sub

        ''' <summary>Export a frame into a bitmap</summary>
        ''' <param name="position">Position of the frame</param>
        Public Function GetBitmap(position As Integer) As Bitmap

            If position > m_countFrames Then
                Throw New Exception("Invalid frame position: " & position)
            End If

            Dim streamInfo As Avi.AVISTREAMINFO = GetStreamInfo(StreamPointer)

            Dim bih As New Avi.BITMAPINFO()
            Dim headerSize As Integer = Marshal.SizeOf(bih.bmiHeader)

            ' Decompress the frame and return a pointer to the DIB
            Dim dib As Integer = Avi.AVIStreamGetFrame(getFrameObject, m_firstFrame + position)

            ' Copy the bitmap header into a managed struct
            bih.bmiColors = Me.m_palette
            bih.bmiHeader = DirectCast(Marshal.PtrToStructure(New IntPtr(dib), bih.bmiHeader.[GetType]()), Avi.BITMAPINFOHEADER)

            If bih.bmiHeader.biSizeImage < 1 Then
                Throw New Exception("Exception in VideoStreamGetFrame")
            End If

            ' Copy the image			
            Dim framePaletteSize As Integer = bih.bmiHeader.biClrUsed * Avi.RGBQUAD_SIZE
            Dim bitmapData As Byte() = New Byte(bih.bmiHeader.biSizeImage - 1) {}
            Dim dibPointer As New IntPtr(dib + Marshal.SizeOf(bih.bmiHeader) + framePaletteSize)
            Marshal.Copy(dibPointer, bitmapData, 0, bih.bmiHeader.biSizeImage)

            ' Copy bitmap info
            Dim bitmapInfo As Byte() = New Byte(Marshal.SizeOf(bih) - 1) {}
            Dim ptr As IntPtr = Marshal.AllocHGlobal(bitmapInfo.Length)
            Marshal.StructureToPtr(bih, ptr, False)
            Marshal.Copy(ptr, bitmapInfo, 0, bitmapInfo.Length)
            Marshal.FreeHGlobal(ptr)

            ' Create file header
            Dim bfh As New Avi.BITMAPFILEHEADER()
            bfh.bfType = Avi.BMP_MAGIC_COOKIE
            bfh.bfSize = CType(55 + bih.bmiHeader.biSizeImage, Int32)
            ' Size of file as written to disk
            bfh.bfReserved1 = 0
            bfh.bfReserved2 = 0
            bfh.bfOffBits = Marshal.SizeOf(bih) + Marshal.SizeOf(bfh)
            If bih.bmiHeader.biBitCount < 8 Then
                ' There is a palette between header and pixel data
                'Avi.PALETTE_SIZE;
                bfh.bfOffBits += bih.bmiHeader.biClrUsed * Avi.RGBQUAD_SIZE
            End If

            ' Write a bitmap stream
            Dim bw As New BinaryWriter(New MemoryStream())

            ' Write header
            bw.Write(bfh.bfType)
            bw.Write(bfh.bfSize)
            bw.Write(bfh.bfReserved1)
            bw.Write(bfh.bfReserved2)
            bw.Write(bfh.bfOffBits)
            ' Write bitmap info
            bw.Write(bitmapInfo)
            ' Write bitmap data
            bw.Write(bitmapData)

            Dim bmp As Bitmap = DirectCast(Image.FromStream(bw.BaseStream), Bitmap)
            Dim saveableBitmap As New Bitmap(bmp.Width, bmp.Height)
            Dim g As Graphics = Graphics.FromImage(saveableBitmap)
            g.DrawImage(bmp, 0, 0)
            g.Dispose()
            bmp.Dispose()

            bw.Close()
            Return saveableBitmap

        End Function

        ''' <summary>Free ressources that have been used by GetFrameOpen</summary>
        Public Sub GetFrameClose()
            If getFrameObject <> 0 Then
                Avi.AVIStreamGetFrameClose(getFrameObject)
                getFrameObject = 0
            End If
        End Sub

        ''' <summary>Copy all frames into a new file</summary>
        ''' <param name="fileName">Name of the new file</param>
        ''' <param name="recompress">true: Compress the new stream</param>
        ''' <returns>AviManager for the new file</returns>
        ''' <remarks>Use this method if you want to append frames to an existing, compressed stream</remarks>
        Public Function DecompressToNewFile(fileName As [String], recompress As Boolean,
                ByRef newStream2 As VideoStream) As AviManager

            Dim newFile As New AviManager(fileName, False)

            Me.GetFrameOpen()

            Dim frame As Bitmap = GetBitmap(0)
            Dim newStream As VideoStream = newFile.AddVideoStream(recompress, m_frameRate, frame)
            frame.Dispose()

            For n As Integer = 1 To m_countFrames - 1
                frame = GetBitmap(n)
                newStream.AddFrame(frame)
                frame.Dispose()
            Next

            Me.GetFrameClose()

            newStream2 = newStream
            Return newFile

        End Function

        ''' <summary>Copy the stream into a new file</summary>
        ''' <param name="fileName">Name of the new file</param>
        Public Overrides Sub ExportStream(fileName As [String])

            Dim opts As New Avi.AVICOMPRESSOPTIONS_CLASS()
            opts.fccType = CUInt(Avi.streamtypeVIDEO_VIDS)
            opts.lpParms = IntPtr.Zero
            opts.lpFormat = IntPtr.Zero
            Dim streamPointer__1 As IntPtr = StreamPointer
            Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME Or Avi.ICMF_CHOOSE_DATARATE, 1,
                streamPointer__1, opts)
            Avi.AVISaveOptionsFree(1, opts)

            Avi.AVISaveV(fileName, 0, 0, 1, m_aviStream, opts)

        End Sub

    End Class

End Namespace