using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.FFMPEG;
using AForge.Video.VFW;
using System.Drawing;
using NeuroImageNet.Bmp;
using NeuroImageNet.HistogramofOrientedGradients;
using NeuroImageNet.IO;
using System.IO;
using System.Threading;

namespace HOGVideo
{
    public class VideoConverter
    {
        public string video_path;
        public string video_save_path;
        public Thread conv_thread;
        public double percent_done=0;
        public int start = 0;
        public int length = 0;
        public int cell_size = 8;
        public int bin_count = 9;
        public bool is_running = false;

        public VideoFileReader video_reader;
        public VideoFileWriter video_writer;

        public VideoConverter(string video_path, string video_save_path)
        {
            this.video_path = video_path;
            this.video_save_path = video_save_path;
            if (!File.Exists(video_path)) return;

            Initialize();
        }

        private void Initialize()
        {
            percent_done = 0;

            video_reader = new VideoFileReader();
            video_writer = new VideoFileWriter();
            video_reader.Open(video_path);
            video_writer.Open(video_save_path, video_reader.Width, video_reader.Height, video_reader.FrameRate, VideoCodec.MPEG4, 50000000);// (VideoCodec)Enum.Parse(typeof(VideoCodec),video_reader.CodecName));

            Console.WriteLine("Video_File_Info:");
            Console.WriteLine("Width: {0}", video_reader.Width);
            Console.WriteLine("Height: {0}", video_reader.Height);
            Console.WriteLine("FPS: {0}", video_reader.FrameRate);
            Console.WriteLine("CODEC: {0}", video_reader.CodecName);
            Console.WriteLine("Frame_Count: {0}", video_reader.FrameCount + "\n");
        }

        public void ConvertVideo()
        {
            is_running = true;
            conv_thread = new Thread(new ThreadStart(convert_video));
            conv_thread.ApartmentState = ApartmentState.STA;
            conv_thread.Start();
        }

        private void convert_video()
        {
            for (int i = 0; i < start; i++)
                using (Bitmap bmp = video_reader.ReadVideoFrame()) ;
            
            
            ImageH image;
            HOG hog;
            ImageH hog_image;
            
            for (int i = start; i < length && i < video_reader.FrameCount; i++)
            {
                image = new ImageH();
                image.bmp = new Bitmap(1920, 1080);//video_reader.ReadVideoFrame();

                hog = new HOG(image.Clone());
                hog.CreateGradientVector();
                hog.CreateCellHistograms(cell_size, bin_count);

                hog_image = hog.cell_histograms.ConvertToImage().Clone();
                //video_writer.WriteVideoFrame(hog_image.bmp);

                hog_image.Dispose();
                hog_image = null;
                hog.Dispose();
                hog = null;
                image.Dispose();
                image = null;
                GC.Collect();

                percent_done = ((double)(i - start) / (double)(length - start)) * 100.0;
            }

            video_writer.Close();
            video_writer.Dispose();
            video_writer = null;

            video_reader.Close();
            video_reader.Dispose();
            video_reader = null;

            is_running = false;
        }

        public void SetFrames(int start, int length)
        {
            this.start = start;
            this.length = length;
        }
    }
}
