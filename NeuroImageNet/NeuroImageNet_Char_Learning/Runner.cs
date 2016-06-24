using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroImageNet.Bmp;
using NeuroImageNet.HistogramofOrientedGradients;
using System.Drawing;

namespace NeuroImageNet_Char_Learning
{
    public class Runner
    {
        public CharLearner trainer;
        public ImageH image;
        public HOG hog;

        public int cell_size;
        public int block_size;
        public int bin_count;

        public double threshold = 0.9;

        public Runner(CharLearner trainer, ImageH image)
        {
            this.trainer = trainer;
            this.image = image;

            //this.cell_size = trainer.cell_size;
            //this.block_size = trainer.block_size;
            //this.bin_count = trainer.bin_count;
            this.cell_size = 8;
            this.block_size = trainer.block_size;
            this.bin_count = trainer.bin_count;
        }

        public void Load_HOG()
        {
            if (hog != null)
            {
                hog.Dispose();
                hog = null;
            }

            hog = new HOG(image.Clone());
            hog.CreateGradientVector(true);
            hog.gradient_vector.NormalizeVectorGradient();
            hog.CreateCellHistograms(cell_size, bin_count);
            hog.CreateBlocks(block_size);
        }

        public ImageH Run()
        {
            ImageH back = image.Clone();
            Font font = new Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel);
            Brush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(brush, 2);

            using (Graphics g = Graphics.FromImage(back.bmp))
            {
                for (int b = 0; b < hog.blocks.block_count; b++)
                {
                    Block block = hog.blocks.blocks[b];
                    int output_num = 0;
                    double[] input = block.block_vector;
                    double[] output = null;
                    trainer.Run(ref output, ref output_num, block);

                    if (output[output_num] >= threshold)
                    {
                        bool all_under_threshold = false;
                        for (int i = 0; i < output.Length; i++ )
                            if (i != output_num && output[i] >= 0.2)
                            {
                                all_under_threshold = true;
                                break;
                            }

                        if (all_under_threshold == false)
                        {
                            // Draw label to block x,y
                            g.DrawRectangle(pen, new Rectangle(block.x, block.y, block.block_size * block.histograms[0].size, block.block_size * block.histograms[0].size));
                            g.DrawString(output_num.ToString(), font, brush, block.x, block.y);
                        }
                    }
                }

                g.Save();
                g.Dispose();
            }

            return back;
        }
    }
}
