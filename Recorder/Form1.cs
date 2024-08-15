using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using Accord.Video.FFMPEG;
using System.Drawing;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Recorder
{
    public enum RecordStatus
    {
        Voice = 0,
        Video = 1
    }

    public partial class Recorder : Form
    {
        Stopwatch stopwatch;

        private WasapiLoopbackCapture audioCapture;
        private WaveInEvent micCapture;
        private WaveFileWriter micWriter;
        private WaveFileWriter audioWriter;
        private float volumeGain = 3.0f;

        private string micPath = "microphone_output.wav";
        private string sysAudioPath = "audio_output.wav";
        private string audioPath = "combined_audio.wav";
        private string videoPath = "video.avi";
        private string AVPath = "result.avi";
        private RecordStatus recordStatus = RecordStatus.Voice;

        private VideoFileWriter videoWriter;
        private Rectangle screenBounds;
        private Timer captureTimer;

        private Form2 waitProcess;
        private bool closeEvent = false;
        

        public Recorder()
        {
            InitializeComponent();
            waitProcess = new Form2("Wait, Please!");
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            stopwatch = new Stopwatch();

            notifyIcon.BalloonTipTitle = "AVRecorder";
            notifyIcon.BalloonTipText = "AVRecorder is processing";
            notifyIcon.Text = "AVRecorder";

            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", onCloseEvent);

            notifyIcon.ContextMenu = trayMenu;
        }

        private void onCloseEvent(Object sender, EventArgs e)
        {
            closeEvent = true;
            this.Close();
        }
        private void Btn_record_Click(object sender, EventArgs e)
        {
            if (WaveIn.DeviceCount == 0)
            {
                MessageBox.Show($"No MicroPhone Detected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(sysAudioPath))
                File.Create(sysAudioPath).Dispose();

            if (!File.Exists(micPath))
                File.Create(micPath).Dispose();

            
            //Mic Record
            try
            {
                micCapture = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(44100, 1) // 44100Hz, 16-bit, Mono
                };
                micWriter = new WaveFileWriter(micPath, new WaveFormat(44100, 1));

                micCapture.DataAvailable += async (s, t) =>
                {
                    if (micWriter != null)
                    {
                        byte[] adjustedBuffer = ApplyVolumeGain(t.Buffer, t.BytesRecorded, volumeGain);
                        await micWriter.WriteAsync(adjustedBuffer, 0, t.BytesRecorded);
                        await micWriter.FlushAsync();
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mic Record Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Audio Record
            try
            {
                audioCapture = new WasapiLoopbackCapture();
                audioWriter = new WaveFileWriter(sysAudioPath, audioCapture.WaveFormat);

                audioCapture.DataAvailable += async (s, t) =>
                {
                    if (audioWriter != null)
                    {
                        await audioWriter.WriteAsync(t.Buffer, 0, t.BytesRecorded);
                        await audioWriter.FlushAsync();

                        long currentPosition = audioWriter.Position;
                        long durationInBytes = stopwatch.ElapsedMilliseconds * (audioCapture.WaveFormat.AverageBytesPerSecond / 1000);

                        if (currentPosition < durationInBytes)
                        {
                            audioWriter.Write(new byte[durationInBytes - currentPosition], 0, (int)(durationInBytes - currentPosition));
                        }
                    }
                };

                audioCapture.RecordingStopped += async (s, t) =>
                {
                    if (audioWriter != null)
                    {
                        audioWriter.Dispose();
                        audioWriter = null;
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SysAudio Record Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            audioCapture.StartRecording();
            micCapture.StartRecording();

            if(recordStatus == RecordStatus.Video)
            {
                try
                {
                    screenBounds = Screen.PrimaryScreen.Bounds;
                    videoWriter = new VideoFileWriter();
                    videoWriter.Open(videoPath, screenBounds.Width, screenBounds.Height, 25, Accord.Video.FFMPEG.VideoCodec.MPEG4);

                    captureTimer = new Timer();
                    captureTimer.Interval = 1000 / 25;
                    captureTimer.Tick += CaptureFrame;
                    captureTimer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Screen Capture Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            stopwatch.Start();

            btn_record.Enabled = false;
            btn_stop.Enabled = true;
        }

        private void Btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                audioCapture.StopRecording();
                micCapture.StopRecording();
                stopwatch.Stop();

                audioWriter.Close();
                micWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stop Audio Record Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MixAudioStreams(micPath, sysAudioPath, audioPath);

            if (recordStatus == RecordStatus.Video)
            {
                try
                {
                    captureTimer.Stop();
                    captureTimer.Dispose();
                    videoWriter.Close();
                    videoWriter.Dispose();

                    if (File.Exists(AVPath))
                        File.Delete(AVPath);

                    CombineAV();                

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Stop Video Record Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var sendForm = new SendDialog();
            sendForm.setStatus(recordStatus);

            var str = stopwatch.Elapsed.TotalSeconds;
            sendForm.setTime((int)stopwatch.Elapsed.TotalSeconds);

            sendForm.ShowDialog();

            btn_record.Enabled = true;
            btn_stop.Enabled = false;

            
            stopwatch.Reset();
        }

        static byte[] ApplyVolumeGain(byte[] buffer, int bytesRecorded, float gain)
        {
            if (gain < 0)
                throw new ArgumentException("Gain must be non-negative");

            byte[] adjustedBuffer = new byte[bytesRecorded];
            Buffer.BlockCopy(buffer, 0, adjustedBuffer, 0, bytesRecorded);

            for (int i = 0; i < bytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(adjustedBuffer, i);
                int newSample = (int)(sample * gain);

                if (newSample > short.MaxValue)
                    newSample = short.MaxValue;
                else if (newSample < short.MinValue)
                    newSample = short.MinValue;

                sample = (short)newSample;
                BitConverter.GetBytes(sample).CopyTo(adjustedBuffer, i);
            }

            return adjustedBuffer;
        }

        public async void CombineAV()
        {
            try
            {
                waitProcess.Show();

                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

                var ffmpeg = FFmpeg.Conversions.New()
                         .AddParameter($"-i {videoPath}")    // Input video
                         .AddParameter($"-i {audioPath}")    // Input audio
                         .AddParameter("-c:v copy")              // Copy video codec
                         .AddParameter("-c:a aac")               // Set audio codec
                         .AddParameter("-strict experimental")   // Experimental codecs
                         .AddParameter("-shortest")              // Shortest duration
                         .SetOutput(AVPath);

                await ffmpeg.Start();

                waitProcess.Hide();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"A + V Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }
        public void MixAudioStreams(string filePath1, string filePath2, string outputFilePath)
        {
            var standardFormat = new WaveFormat(44100, 16, 2);

            var tempFilePath1 = "temp1.wav";
            var tempFilePath2 = "temp2.wav";
            File.Create(tempFilePath1).Dispose();
            File.Create(tempFilePath2).Dispose();

            // Convert both files to the standard format
            AudioFormatConverter.ConvertToStandardFormat(filePath1, "temp1.wav", standardFormat);
            AudioFormatConverter.ConvertToStandardFormat(filePath2, "temp2.wav", standardFormat);

            // Mix the converted files
            using (var reader1 = new AudioFileReader("temp1.wav"))
            using (var reader2 = new AudioFileReader("temp2.wav"))
            using (var waveFileWriter = new WaveFileWriter(outputFilePath, standardFormat))
            {
                var bufferSize = 4096;
                var buffer1 = new float[bufferSize];
                var buffer2 = new float[bufferSize];
                var mixedBuffer = new float[bufferSize];

                int samplesRead1, samplesRead2;

                while ((samplesRead1 = reader1.Read(buffer1, 0, bufferSize)) > 0 &&
                       (samplesRead2 = reader2.Read(buffer2, 0, bufferSize)) > 0)
                {
                    for (int i = 0; i < bufferSize; i++)
                    {
                        mixedBuffer[i] = (buffer1[i] + buffer2[i]) / 2; // Simple averaging
                    }

                    waveFileWriter.WriteSamples(mixedBuffer, 0, samplesRead1);
                }
            }

            // Clean up temporary files
            System.IO.File.Delete("temp1.wav");
            System.IO.File.Delete("temp2.wav");
        }        

        private void TimerTick(object sender, EventArgs e)
        {
            lbl_Timer.Text = string.Format("{0:hh\\:mm\\:ss}", stopwatch.Elapsed);
        }

        private void CaptureFrame(object sender, EventArgs e)
        {
            using (Bitmap screenshot = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                }

                videoWriter.WriteVideoFrame(screenshot);
            }
        }

        private void Voice_radio_CheckedChanged(object sender, EventArgs e)
        {
            recordStatus = RecordStatus.Voice;
        }

        private void Video_radio_CheckedChanged(object sender, EventArgs e)
        {
            recordStatus = RecordStatus.Video;
        }

        private void Recorder_Resize(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
            else
            {
                notifyIcon.Visible = false;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon.Visible = false;
            this.WindowState = FormWindowState.Normal;
        }

        private void Recorder_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closeEvent)
            {
                this.Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);

                e.Cancel = true;
            }
        }
    }

    public class AudioFormatConverter
    {
        public static void ConvertToStandardFormat(string inputFile, string outputFile, WaveFormat standardFormat)
        {
            using (var reader = new AudioFileReader(inputFile))
            {
                using (var resampler = new MediaFoundationResampler(reader, standardFormat))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, resampler);
                }
            }
        }
    }
}
