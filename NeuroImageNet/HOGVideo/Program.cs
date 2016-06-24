using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroImageNet.IO;
using System.Threading;

namespace HOGVideo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Video file to open: ");
            string video_path = Std.OpenFile();

            Console.WriteLine("Video file to write to: ");
            string video_save_path = Std.SaveFile("*.avi");

            if (video_path == null || video_save_path == null) goto Wrong_Input;

            VideoConverter video_converter = new VideoConverter(video_path, video_save_path);
            video_converter.SetFrames(0,1000);
            video_converter.ConvertVideo();

            while (video_converter.is_running)
            {
                Console.Clear();
                Console.WriteLine("Converting_Video... " + video_converter.percent_done + "%" );
                Thread.Sleep(1000);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();

            goto Exit;

        Wrong_Input:
            {
                Console.WriteLine("Wront_Input, Try again.");
                Console.ReadLine();
                Environment.Exit(0);
            }

        Exit:
            Environment.Exit(0);
        }

    }
}
