
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

Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace AviFile

    Public Class AviManager

        Private aviFile As Integer = 0
        Private streams As New ArrayList()

        ''' <summary>Open or create an AVI file</summary>
        ''' <param name="fileName">Name of the AVI file</param>
        ''' <param name="open">true: Open the file; false: Create or overwrite the file</param>
        Public Sub New(fileName As [String], open As Boolean)

            Avi.AVIFileInit()
            Dim result As Integer

            If open Then
                ' Open existing file
                result = Avi.AVIFileOpen(aviFile, fileName, Avi.OF_READWRITE, 0)
            Else
                ' Create empty file
                result = Avi.AVIFileOpen(aviFile, fileName, Avi.OF_WRITE Or Avi.OF_CREATE, 0)
            End If

            If result <> 0 Then
                Throw New Exception("Exception in AVIFileOpen: " & result.ToString())
            End If

        End Sub

        Private Sub New(aviFile As Integer)
            Me.aviFile = aviFile
        End Sub

        ''' <summary>Get the first video stream - usually there is only one video stream</summary>
        ''' <returns>VideoStream object for the stream</returns>
        Public Function GetVideoStream() As VideoStream

            Dim aviStream As IntPtr

            Dim result As Integer = Avi.AVIFileGetStream(aviFile, aviStream, Avi.streamtypeVIDEO_VIDS, 0)

            If result <> 0 Then
                Throw New Exception("Exception in AVIFileGetStream: " & result.ToString())
            End If

            Dim stream As New VideoStream(aviFile, aviStream)
            streams.Add(stream)
            Return stream

        End Function

        ''' <summary>Getthe first wave audio stream</summary>
        ''' <returns>AudioStream object for the stream</returns>
        Public Function GetWaveStream() As AudioStream

            Dim aviStream As IntPtr

            Dim result As Integer = Avi.AVIFileGetStream(aviFile, aviStream, Avi.streamtypeAUDIO, 0)

            If result <> 0 Then
                Throw New Exception("Exception in AVIFileGetStream: " & result.ToString())
            End If

            Dim stream As New AudioStream(aviFile, aviStream)
            streams.Add(stream)
            Return stream

        End Function

        ''' <summary>Get a stream from the internal list of opened streams</summary>
        ''' <param name="index">Index of the stream. The streams are not sorted, the first stream is the one that was opened first.</param>
        ''' <returns>VideoStream at position [index]</returns>
        ''' <remarks>
        ''' Use this method after DecompressToNewFile,
        ''' to get the copied stream from the new AVI file
        ''' </remarks>
        ''' <example>
        ''' //streams cannot be edited - copy to a new file
        '''	AviManager newManager = aviStream.DecompressToNewFile(@"..\..\testdata\temp.avi", true);
        ''' //there is only one stream in the new file - get it and add a frame
        '''	VideoStream aviStream = newManager.GetOpenStream(0);
        '''	aviStream.AddFrame(bitmap);
        ''' </example>
        Public Function GetOpenStream(index As Integer) As VideoStream
            Return DirectCast(streams(index), VideoStream)
        End Function

        ''' <summary>Add an empty video stream to the file</summary>
        ''' <param name="isCompressed">true: Create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="frameSize">Size of one frame in bytes</param>
        ''' <param name="width">Width of each image</param>
        ''' <param name="height">Height of each image</param>
        ''' <param name="format">PixelFormat of the images</param>
        ''' <returns>VideoStream object for the new stream</returns>
        Public Function AddVideoStream(isCompressed As Boolean, frameRate As Double,
                frameSize As Integer, width As Integer, height As Integer, format As PixelFormat) As VideoStream
            Dim stream As New VideoStream(aviFile, isCompressed, frameRate, frameSize, width, height,
                format)
            streams.Add(stream)
            Return stream
        End Function

        ''' <summary>Add an empty video stream to the file</summary>
        ''' <remarks>Compresses the stream without showing the codecs dialog</remarks>
        ''' <param name="compressOptions">Compression options</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="firstFrame">Image to write into the stream as the first frame</param>
        ''' <returns>VideoStream object for the new stream</returns>
        Public Function AddVideoStream(compressOptions As Avi.AVICOMPRESSOPTIONS,
                frameRate As Double, firstFrame As Bitmap) As VideoStream
            Dim stream As New VideoStream(aviFile, compressOptions, frameRate, firstFrame)
            streams.Add(stream)
            Return stream
        End Function

        ''' <summary>Add an empty video stream to the file</summary>
        ''' <param name="isCompressed">true: Create a compressed stream before adding frames</param>
        ''' <param name="frameRate">Frames per second</param>
        ''' <param name="firstFrame">Image to write into the stream as the first frame</param>
        ''' <returns>VideoStream object for the new stream</returns>
        Public Function AddVideoStream(isCompressed As Boolean, frameRate As Double,
                firstFrame As Bitmap) As VideoStream
            Dim stream As New VideoStream(aviFile, isCompressed, frameRate, firstFrame)
            streams.Add(stream)
            Return stream
        End Function

        ''' <summary>Add a wave audio stream from another file to this file</summary>
        ''' <param name="waveFileName">Name of the wave file to add</param>
        ''' <param name="startAtFrameIndex">Index of the video frame at which the sound is going to start</param>
        Public Sub AddAudioStream(waveFileName As [String], startAtFrameIndex As Integer)
            Dim audioManager As New AviManager(waveFileName, True)
            Dim newStream As AudioStream = audioManager.GetWaveStream()
            AddAudioStream(newStream, startAtFrameIndex)
            audioManager.Close()
        End Sub

        Private Function InsertSilence(countSilentSamples As Integer, waveData As IntPtr,
                lengthWave As Integer, ByRef streamInfo As Avi.AVISTREAMINFO) As IntPtr

            ' Initialize silence
            Dim lengthSilence As Integer = countSilentSamples * streamInfo.dwSampleSize
            Dim silence As Byte() = New Byte(lengthSilence - 1) {}

            ' Initialize new sound
            Dim lengthNewStream As Integer = lengthSilence + lengthWave
            Dim newWaveData As IntPtr = Marshal.AllocHGlobal(lengthNewStream)

            ' Copy silence
            Marshal.Copy(silence, 0, newWaveData, lengthSilence)

            ' Copy sound
            Dim sound As Byte() = New Byte(lengthWave - 1) {}
            Marshal.Copy(waveData, sound, 0, lengthWave)
            Dim startOfSound As New IntPtr(newWaveData.ToInt32() + lengthSilence)
            Marshal.Copy(sound, 0, startOfSound, lengthWave)

            Marshal.FreeHGlobal(newWaveData)

            streamInfo.dwLength = lengthNewStream

            Return newWaveData

        End Function

        ''' <summary>Add an existing wave audio stream to the file</summary>
        ''' <param name="newStream">The stream to add</param>
        ''' <param name="startAtFrameIndex">
        ''' The index of the video frame at which the sound is going to start.
        ''' '0' inserts the sound at the beginning of the video.
        ''' </param>
        Public Sub AddAudioStream(newStream As AudioStream, startAtFrameIndex As Integer)

            Dim streamInfo As New Avi.AVISTREAMINFO()
            Dim streamFormat As New Avi.PCMWAVEFORMAT()
            Dim streamLength As Integer = 0

            Dim rawData As IntPtr = newStream.GetStreamData(streamInfo, streamFormat, streamLength)
            Dim waveData As IntPtr = rawData

            If startAtFrameIndex > 0 Then

                ' Not supported
                ' streamInfo.dwStart = startAtFrameIndex;

                Dim framesPerSecond As Double = GetVideoStream().FrameRate
                Dim samplesPerSecond As Double = newStream.CountSamplesPerSecond
                Dim startAtSecond As Double = startAtFrameIndex / framesPerSecond
                Dim startAtSample As Integer = CInt(Math.Truncate(samplesPerSecond * startAtSecond))

                waveData = InsertSilence(startAtSample - 1, waveData, streamLength, streamInfo)
            End If

            Dim aviStream As IntPtr
            Dim result As Integer = Avi.AVIFileCreateStream(aviFile, aviStream, streamInfo)
            If result <> 0 Then
                Throw New Exception("Exception in AVIFileCreateStream: " & result.ToString())
            End If

            result = Avi.AVIStreamSetFormat(aviStream, 0, streamFormat, Marshal.SizeOf(streamFormat))
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamSetFormat: " & result.ToString())
            End If

            result = Avi.AVIStreamWrite(aviStream, 0, streamLength, waveData, streamLength,
                Avi.AVIIF_KEYFRAME, 0, 0)
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamWrite: " & result.ToString())
            End If

            result = Avi.AVIStreamRelease(aviStream)
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamRelease: " & result.ToString())
            End If

            Marshal.FreeHGlobal(waveData)

        End Sub

        ''' <summary>Add an existing wave audio stream to the file</summary>
        ''' <param name="waveData">The new stream's data</param>
        ''' <param name="streamInfo">Header info for the new stream</param>
        ''' <param name="streamFormat">The new stream' format info</param>
        ''' <param name="streamLength">Length of the new stream</param>
        Public Sub AddAudioStream(waveData As IntPtr, streamInfo As Avi.AVISTREAMINFO,
                streamFormat As Avi.PCMWAVEFORMAT, streamLength As Integer)

            Dim aviStream As IntPtr
            Dim result As Integer = Avi.AVIFileCreateStream(aviFile, aviStream, streamInfo)
            If result <> 0 Then
                Throw New Exception("Exception in AVIFileCreateStream: " & result.ToString())
            End If

            result = Avi.AVIStreamSetFormat(aviStream, 0, streamFormat, Marshal.SizeOf(streamFormat))
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamSetFormat: " & result.ToString())
            End If

            result = Avi.AVIStreamWrite(aviStream, 0, streamLength, waveData, streamLength,
                Avi.AVIIF_KEYFRAME, 0, 0)
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamWrite: " & result.ToString())
            End If

            result = Avi.AVIStreamRelease(aviStream)
            If result <> 0 Then
                Throw New Exception("Exception in AVIStreamRelease: " & result.ToString())
            End If

        End Sub

        ''' <summary>Copy a piece of video and wave sound int a new file</summary>
        ''' <param name="newFileName">File name</param>
        ''' <param name="startAtSecond">Start copying at second x</param>
        ''' <param name="stopAtSecond">Stop copying at second y</param>
        ''' <returns>AviManager for the new video</returns>
        Public Function CopyTo(newFileName As [String], startAtSecond As Single,
                stopAtSecond As Single) As AviManager

            Dim newFile As New AviManager(newFileName, False)

            Try
                ' Copy video stream

                Dim videoStream As VideoStream = GetVideoStream()

                Dim startFrameIndex As Integer = CInt(Math.Truncate(videoStream.FrameRate * startAtSecond))
                Dim stopFrameIndex As Integer = CInt(Math.Truncate(videoStream.FrameRate * stopAtSecond))

                videoStream.GetFrameOpen()
                Dim bmp As Bitmap = videoStream.GetBitmap(startFrameIndex)
                Dim newStream As VideoStream = newFile.AddVideoStream(False, videoStream.FrameRate, bmp)
                For n As Integer = startFrameIndex + 1 To stopFrameIndex
                    bmp = videoStream.GetBitmap(n)
                    newStream.AddFrame(bmp)
                Next
                videoStream.GetFrameClose()

                ' Copy audio stream

                Dim waveStream As AudioStream = GetWaveStream()

                Dim streamInfo As New Avi.AVISTREAMINFO()
                Dim streamFormat As New Avi.PCMWAVEFORMAT()
                Dim streamLength As Integer = 0
                Dim ptrRawData As IntPtr = waveStream.GetStreamData(streamInfo, streamFormat, streamLength)

                Dim startByteIndex As Integer =
                    CInt(Math.Truncate(startAtSecond * CSng(waveStream.CountSamplesPerSecond *
                        streamFormat.nChannels * waveStream.CountBitsPerSample) / 8))
                Dim stopByteIndex As Integer =
                    CInt(Math.Truncate(stopAtSecond * CSng(waveStream.CountSamplesPerSecond *
                        streamFormat.nChannels * waveStream.CountBitsPerSample) / 8))

                Dim ptrWavePart As New IntPtr(ptrRawData.ToInt32() + startByteIndex)

                Dim rawData As Byte() = New Byte(stopByteIndex - startByteIndex - 1) {}
                Marshal.Copy(ptrWavePart, rawData, 0, rawData.Length)
                Marshal.FreeHGlobal(ptrRawData)

                streamInfo.dwLength = rawData.Length
                streamInfo.dwStart = 0

                Dim unmanagedRawData As IntPtr = Marshal.AllocHGlobal(rawData.Length)
                Marshal.Copy(rawData, 0, unmanagedRawData, rawData.Length)
                newFile.AddAudioStream(unmanagedRawData, streamInfo, streamFormat, rawData.Length)
                Marshal.FreeHGlobal(unmanagedRawData)
            Catch ex As Exception
                newFile.Close()
                Throw ex
            End Try

            Return newFile

        End Function

        ''' <summary>Release all ressources</summary>
        Public Sub Close()
            For Each stream As AviStream In streams
                stream.Close()
            Next

            Avi.AVIFileRelease(aviFile)
            Avi.AVIFileExit()
        End Sub

        Public Shared Sub MakeFileFromStream(fileName As [String], stream As AviStream)

            Dim newFile As IntPtr = IntPtr.Zero
            Dim streamPointer As IntPtr = stream.StreamPointer

            Dim opts As New Avi.AVICOMPRESSOPTIONS_CLASS()
            opts.fccType = CUInt(Avi.streamtypeVIDEO_VIDS)
            opts.lpParms = IntPtr.Zero
            opts.lpFormat = IntPtr.Zero
            Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME Or Avi.ICMF_CHOOSE_DATARATE, 1,
                streamPointer, opts)
            Avi.AVISaveOptionsFree(1, opts)

            Avi.AVISaveV(fileName, 0, 0, 1, streamPointer, opts)

        End Sub

    End Class

End Namespace