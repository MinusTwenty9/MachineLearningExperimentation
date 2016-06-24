using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroImageNet.Bmp;
using NeuroImageNet.HistogramofOrientedGradients;
using System.Diagnostics;

namespace NeuroImageNet_Test
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ImageH img = new ImageH(Environment.CurrentDirectory + "\\t2est.png");

            // Gradient Vector
            Stopwatch sw = new Stopwatch();
            sw.Start();

            HOG hog = new HOG(img);
            hog.CreateGradientVector();
            //ImageH cell_image = hog.gradient_vector.ConvertToImage(1);
            //cell_image.Save(Environment.CurrentDirectory + "\\test_cell.png",true);

            hog.gradient_vector.NormalizeVectorGradient();

            hog.CreateCellHistograms(8);
            ImageH hog_norm = hog.cell_histograms.ConvertToImage();
            hog_norm.Save(Environment.CurrentDirectory + "\\test_hog_norm.png", true);
            hog_norm.Dispose();

            hog.CreateBlocks(2);
            //ImageH blocks_image = hog.blocks.ConvertToImage();
            //blocks_image.Save(Environment.CurrentDirectory + "\\test_hog_blocks.png", true);
            //blocks_image.Dispose();

            sw.Stop();
            Console.WriteLine("HOG_Time: " + sw.ElapsedMilliseconds.ToString() + "ms");

            hog.Dispose();

            //GC.Collect();

        }
    }
}
