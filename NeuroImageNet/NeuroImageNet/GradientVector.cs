using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NeuroImageNet.Bmp;

namespace NeuroImageNet
{
    namespace HistogramofOrientedGradients
    {
        public class HOG
        {
            public ImageH image;
            public GradientVector gradient_vector;
            public CellHistograms cell_histograms;
            public Blocks blocks;

            public HOG(ImageH image)
            {
                this.image = image.Clone();
            }

            public void CreateGradientVector(bool inverted = false)
            {
                gradient_vector = new GradientVector(this.image, inverted);
            }

            public void CreateCellHistograms(int cell_size = 8, int bin_count = 9)
            {
                cell_histograms = new CellHistograms(gradient_vector, cell_size, bin_count);
            }

            public void CreateBlocks(int block_size = 2)
            {
                blocks = new Blocks(cell_histograms, block_size);
            }

            public void Dispose()
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                if (gradient_vector != null)
                {
                    gradient_vector.Dispose();
                    gradient_vector = null;
                }
                if (cell_histograms != null)
                {
                    cell_histograms.Dispose();
                    cell_histograms = null;
                }
                if (blocks != null)
                {
                    blocks.Dispose();
                    blocks = null;
                }
            }
        }

        public class GradientVector
        {
            public double[][] gradient_vector;
            public int width;
            public int height;

            public bool inverted;

            public GradientVector(ImageH image, bool inverted = false)
            {
                if (image == null) return;
                if (image.bmp == null) return;

                this.width = image.bmp.Width;
                this.height = image.bmp.Height;
                this.inverted = inverted;

                GetGradientVectors(image);
            }

            // int[length][0=x;1=y]
            // i = x_gradient_vector
            // i+1 = y_gradient_vector
            private void GetGradientVectors(ImageH image)
            {
                ImageH grey_scale = ImageFilter.GreyScale(image, inverted);
                int width = grey_scale.bmp.Width;
                int height = grey_scale.bmp.Height;
                int length = grey_scale.p_bmp.rgb.Length;
                gradient_vector = new double[length][];

                Parallel.For(0, length, i =>
                {
                    //for (int i = 0; i < length; i++)
                    //lock (gradient_vector)
                    {
                        int y = (int)Math.Floor(i / (double)width);
                        int x = i - (y * width);


                        int x_vec_sub = grey_scale.p_bmp.rgb[(y * width) + (x - 1 < 0 ? x : x - 1)] -
                                        grey_scale.p_bmp.rgb[(y * width) + (x + 1 >= width ? x : x + 1)];

                        int y_vec_sub = grey_scale.p_bmp.rgb[x + ((y - 1 < 0 ? y : y - 1) * width)] -
                                        grey_scale.p_bmp.rgb[x + ((y + 1 >= height ? y : y + 1) * width)];

                        gradient_vector[i] = new double[2];
                        gradient_vector[i][0] = Math.Abs(x_vec_sub);
                        gradient_vector[i][1] = Math.Abs(y_vec_sub);

                    }

                });

                grey_scale.Dispose();
            }

            public double GetAngle(int index)
            {
                double angle = Math.Atan((gradient_vector[index][1] == 0 ? 0 : gradient_vector[index][0] / gradient_vector[index][1])) * (180.0 / Math.PI) * 2;
                return angle;
            }

            public double GetAngle(double[] vector)
            {
                double angle = Math.Atan((vector[1] == 0 ? 0 : vector[0] / vector[1])) * (180.0 / Math.PI) * 2;
                return angle;
            }

            public double GetMagnitude(int index)
            {
                double magnitude = Math.Sqrt(Math.Pow(gradient_vector[index][0], 2) + Math.Pow(gradient_vector[index][1], 2));
                return magnitude;
            }

            public double GetMagnitude(double[] vector)
            {
                double magnitude = Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2));
                return magnitude;
            }

            public void NormalizeVectorGradient()
            {
                if (gradient_vector == null) return;

                int length = gradient_vector.Length;

                Parallel.For(0, length, i =>
                //for (int i = 0; i < length; i++)
                {
                    double magnitude = GetMagnitude(i);
                    gradient_vector[i][0] = (magnitude == 0 ? 0 : gradient_vector[i][0] / magnitude);
                    gradient_vector[i][1] = (magnitude == 0 ? 0 : gradient_vector[i][1] / magnitude);
                });
            }

            // 0 = X; 1 = Y
            public ImageH ConvertToImage(int gradient_index)
            {
                ImageH gradient_image = new ImageH();
                gradient_image.bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                gradient_image.Load_Parallel();

                int length = gradient_vector.Length;
                int depth = gradient_image.p_bmp.depth / 8;

                Parallel.For(0, length, i =>
                {
                    byte pixel = (byte)((255 + gradient_vector[i][gradient_index]) / 2);
                    gradient_image.p_bmp.rgb[i * depth] = pixel;
                    gradient_image.p_bmp.rgb[(i * depth) + 1] = pixel;
                    gradient_image.p_bmp.rgb[(i * depth) + 2] = pixel;
                });

                gradient_image.Write_Parallel_To_Bmp();

                return gradient_image;
            }

            public void Dispose()
            {
                if (gradient_vector != null)
                    gradient_vector = null;
                width = 0;
                height = 0;
            }
        }

        public class CellHistograms
        {
            public GradientVector gradient_vector;
            public CellHistogram[] cell_histograms;
            public int cell_size;
            public int width;
            public int height;
            public int bin_count;

            public CellHistograms(GradientVector gradient_vector, int cell_size = 8, int bin_count = 9)
            {
                if (gradient_vector == null || gradient_vector.gradient_vector == null)
                    return;

                this.gradient_vector = gradient_vector;
                this.cell_size = cell_size;
                this.bin_count = bin_count;

                width = (int)Math.Floor(gradient_vector.width / (float)cell_size);
                height = (int)Math.Floor(gradient_vector.height / (float)cell_size);

                GetCellHostograms();

            }

            private void GetCellHostograms()
            {
                int length = width * height;
                cell_histograms = new CellHistogram[length];

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        cell_histograms[(y * width) + x] = new CellHistogram(bin_count, x * cell_size, y * cell_size, cell_size, this.gradient_vector);
            }

            public ImageH ConvertToImage()
            {
                if (cell_histograms == null) return null;

                ImageH image = new ImageH();
                image.bmp = new Bitmap(width * cell_size, height * cell_size, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                double bin_degrees = 180.0 / bin_count;
                Pen pen = new Pen(new SolidBrush(Color.White));

                using (Graphics g = Graphics.FromImage(image.bmp))
                {
                    g.Clear(Color.Black);

                    double max_angle = 0;
                    double min_angle = 500;

                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            double angle = 0;
                            double ii = 0;
                            double mm = 0;

                            int l_length = cell_size / 4;
                            int x_offset = x * cell_size;
                            int y_offset = y * cell_size;

                            for (int i = 0; i < bin_count; i++)
                            {
                                //    [ mm                                   ] * [ ii           ]
                                ii += cell_histograms[(y * width) + x].bins[i] * ((i * bin_degrees) + (bin_degrees / 2));
                                mm += cell_histograms[(y * width) + x].bins[i];
                            }

                            angle = (mm == 0 ? 0 : ii / mm);

                            if (angle > max_angle) max_angle = angle;
                            else if (angle < min_angle) min_angle = angle;

                            Color back_color = ImageFilter.get_rgb_color_map_index(170, (int)angle);
                            Color fore_color = Color.FromArgb(255 - back_color.R, 255 - back_color.G, 255 - back_color.B);
                            g.FillRectangle(new SolidBrush(back_color), new Rectangle(x_offset, y_offset, cell_size, cell_size));


                            angle = (angle * Math.PI) / 180.0;


                            Point l_start = new Point(x_offset + l_length, y_offset + l_length);
                            Point l_plus = new Point((int)(x_offset + (Math.Cos(angle) * l_length)), (int)(y_offset + (Math.Sin(angle) * l_length)));
                            //Point l_minus = new Point((int)(x_offset - (Math.Cos(angle) * l_length)), (int)(y_offset - (Math.Sin(angle) * l_length)));

                            pen = new Pen(new SolidBrush(fore_color));
                            g.DrawLine(pen, l_start, l_plus);
                            //g.DrawLine(pen, l_start, l_minus);
                        }

                    //Console.WriteLine("Max_angle: " + max_angle.ToString());
                    //Console.WriteLine("Min_angle: " + min_angle.ToString());
                    g.Save();
                    g.Dispose();
                }

                return image;
            }

            public void Dispose()
            {
                if (cell_histograms != null)
                {
                    for (int i = 0; i < cell_histograms.Length; i++)
                        cell_histograms[i].Dispose();

                    cell_histograms = null;
                }
                if (gradient_vector != null)
                {
                    gradient_vector.Dispose();
                    gradient_vector = null;
                }
                width = 0;
                height = 0;
                bin_count = 0;
            }
        }

        public class CellHistogram
        {
            public int bin_count;
            public int x;
            public int y;
            public int width;
            public int height;
            public int size;
            public double[] bins;

            public CellHistogram(int bin_count, int x, int y, int size, GradientVector gv)
            {
                this.bin_count = bin_count;
                this.x = x;
                this.y = y;
                this.width = gv.width;
                this.height = gv.height;
                this.size = size;

                CalculateHistogram(gv);
            }

            private void CalculateHistogram(GradientVector gv)
            {
                bins = new double[bin_count];
                double bin_degrees = 180.0 / bin_count;

                int length = size * size;

                //Parallel.For(0, length, i =>

                for (int i = 0; i < length; i++)
                {
                    int inner_y = (int)Math.Floor(i / (float)size);
                    int inner_x = i - (inner_y * size);
                    int offset = ((y + inner_y) * width) + (x + inner_x);

                    // Calculate Histogram value
                    double[] vector = gv.gradient_vector[offset];
                    double magnitude = gv.GetMagnitude(vector);
                    double angle = gv.GetAngle(vector);

                    int bin_index = (int)Math.Floor((angle - (bin_degrees / 2)) / bin_degrees);
                    double bin_contribution = (angle - (bin_index * bin_degrees) - (bin_degrees / 2)) / bin_degrees;

                    if (bin_index >= 0)
                        bins[bin_index] += magnitude * (1 - bin_contribution);

                    if (bin_index + 1 < bin_count)
                        bins[bin_index + 1] += magnitude * bin_contribution;
                }//);

            }

            public void Dispose()
            {
                if (bins != null)
                    bins = null;
                width = 0;
                height = 0;
                bin_count = 0;
                x = 0;
                y = 0;
                size = 0;
            }
        }

        public class Blocks
        {
            public CellHistograms cell_histograms;
            public Block[] blocks;
            public int width;
            public int height;
            public int block_size;
            public int block_count;

            public Blocks(CellHistograms cell_histograms, int block_size)
            {
                if (cell_histograms == null && cell_histograms.cell_histograms == null)
                    return;

                this.cell_histograms = cell_histograms;
                this.block_size = block_size;
                this.width = cell_histograms.width - (block_size - 1);
                this.height = cell_histograms.height - (block_size - 1);

                block_count = width * height;

                CalculateBlocks();
            }

            private void CalculateBlocks()
            {
                blocks = new Block[block_count];
                int ch_width = cell_histograms.width;
                int ch_height = cell_histograms.height;
                int ch_length = ch_width * ch_height;

                Parallel.For(0, ch_length, i =>
                //for (int i = 0; i < ch_length; i++)
                {
                    int y = (int)Math.Floor(i / (float)ch_width);
                    int x = i - (y * ch_width);

                    // Inside the block boundries
                    if (x + block_size <= ch_width && y + block_size <= ch_height)
                    {
                        CellHistogram[] block_histogram = new CellHistogram[block_size * block_size];

                        for (int y_o = 0; y_o < block_size; y_o++)
                            for (int x_o = 0; x_o < block_size; x_o++)
                            {
                                int h_i = ((y + y_o) * ch_width) + (x + x_o);
                                int r_h_i = (y_o * block_size) + x_o;

                                block_histogram[r_h_i] = cell_histograms.cell_histograms[h_i];

                            }

                        lock (blocks)
                        {
                            int b_i = (y * width) + x;
                            blocks[b_i] = new Block(block_size, block_histogram,block_histogram[0].x, block_histogram[0].y);
                        }
                    }
                });
            }

            public ImageH ConvertToImage()
            {
                if (blocks == null) return null;

                ImageH image = new ImageH();
                int _1d_width = (width / block_size);
                int _1d_height = (height / block_size);
                int cell_size = cell_histograms.cell_size;

                image.bmp = new Bitmap(cell_histograms.width * cell_size, cell_histograms.height * cell_size, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int bin_count = cell_histograms.bin_count;
                double bin_degrees = 180.0 / bin_count;
                Pen pen = new Pen(new SolidBrush(Color.White));

                using (Graphics g = Graphics.FromImage(image.bmp))
                {
                    g.Clear(Color.Black);
                    double max_angle = 0;
                    double min_angle = 500;

                    for (int y = 0; y < height / block_size; y++)
                        for (int x = 0; x < width / block_size; x++)
                        {
                            int block_index = (y * width * block_size) + (x * block_size);
                            int l_length = cell_size / 4;
                            Block block = blocks[block_index];

                            for (int by = 0; by < block_size; by++)
                                for (int bx = 0; bx < block_size; bx++)
                                {
                                    int x_offset = (x * block_size * cell_size) + (cell_size * bx);
                                    int y_offset = (y * block_size * cell_size) + (cell_size * by);

                                    double angle = 0;
                                    double ii = 0;
                                    double mm = 0;

                                    for (int i = 0; i < bin_count; i++)
                                    {
                                        ii += block.block_vector[(((by * block_size) + bx) + (i * block_size * block_size))] * ((i * bin_degrees) + (bin_degrees / 2));
                                        mm += block.block_vector[(((by * block_size) + bx) + (i * block_size * block_size))];
                                    }

                                    angle = (mm == 0 ? 0 : ii / mm);

                                    if (angle > max_angle) max_angle = angle;
                                    else if (angle < min_angle) min_angle = angle;

                                    Color back_color = ImageFilter.get_rgb_color_map_index(170, (int)angle);
                                    Color fore_color = Color.FromArgb(255 - back_color.R, 255 - back_color.G, 255 - back_color.B);
                                    g.FillRectangle(new SolidBrush(back_color), new Rectangle(x_offset, y_offset, cell_size, cell_size));

                                    angle = (angle * Math.PI) / 180.0;

                                    Point l_start = new Point(x_offset + l_length, y_offset + l_length);
                                    Point l_plus = new Point((int)(x_offset + (Math.Cos(angle) * l_length)), (int)(y_offset + (Math.Sin(angle) * l_length)));
                                    //Point l_minus = new Point((int)(x_offset - (Math.Cos(angle) * l_length)), (int)(y_offset - (Math.Sin(angle) * l_length)));

                                    pen = new Pen(new SolidBrush(fore_color));
                                    g.DrawLine(pen, l_start, l_plus);
                                    //g.DrawLine(pen, l_start, l_minus);
                                }
                        }
                    Console.WriteLine("Max_angle: " + max_angle.ToString());
                    Console.WriteLine("Min_angle: " + min_angle.ToString());
                    g.Save();
                    g.Dispose();
                }

                return image;
            }

            public void Dispose()
            {
                if (cell_histograms != null)
                {
                    cell_histograms.Dispose();
                    cell_histograms = null;
                }

                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Length; i++)
                        blocks[i].Dispose();
                    blocks = null;
                }

                width = 0;
                height = 0;
                block_size = 0;
                block_count = 0;
            }
        }

        public class Block
        {
            public int x;
            public int y;
            public int block_size;
            public double[] block_vector;
            public CellHistogram[] histograms;

            public Block(int block_size, CellHistogram[] histograms, int x, int y)
            {
                this.histograms = histograms;
                this.block_size = block_size;
                this.x = x;
                this.y = y;

                CalculateBlock();
            }

            private void CalculateBlock()
            {
                int bin_count = histograms[0].bin_count;
                int block_count = (int)Math.Pow(block_size, 2);
                int block_vector_length = bin_count * block_count;

                block_vector = new double[block_vector_length];

                // Fill block_Vector / Calculate magnetude on the fly
                double magnitude = 0;

                for (int b = 0; b < bin_count; b++)
                    for (int i = 0; i < block_count; i++)
                    {
                        double bin_value = histograms[i].bins[b];

                        magnitude += Math.Pow(bin_value, 2);
                        block_vector[(b * block_count) + i] = bin_value;
                    }

                magnitude = Math.Sqrt(magnitude);

                // Divide block_vector through it's magnitude
                Parallel.For(0, block_vector_length, i =>
                //for (int i = 0; i < block_vector_length; i++)
                {
                    block_vector[i] = (magnitude == 0 ? 0 : block_vector[i] / magnitude);
                });

                // block_vector = normalized vector of all histograms submitted
            }

            public void Dispose()
            {
                if (block_vector != null)
                    block_vector = null;
                if (histograms != null)
                {
                    for (int i = 0; i < histograms.Length; i++)
                        histograms[i].Dispose();
                    histograms = null;
                }

                block_size = 0;
            }
        }
    }

    namespace Bmp
    {
        public static class ImageFilter
        {
            public static ImageH GreyScale(ImageH image, bool inverted = false)
            {
                ImageH grey_scale = new ImageH();
                grey_scale.bmp = new Bitmap(image.bmp.Width, image.bmp.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                grey_scale.Load_Parallel();

                if (image.p_bmp == null)
                    image.Load_Parallel();
                int depth = image.p_bmp.depth / 8;
                int length = image.p_bmp.rgb.Length / depth;

                Parallel.For(0, length, i =>
                //for (int i = 0; i < length; i++)
                {
                    int avg = 0;
                    for (int y = 0; y < depth; y++)
                        avg += image.p_bmp.rgb[(i * depth) + y];
                    avg /= depth;
                    grey_scale.p_bmp.rgb[i] = (inverted == false ? (byte)avg : (byte)(255-avg));
                });

                grey_scale.Write_Parallel_To_Bmp();
                return grey_scale;
            }

            public static Color get_rgb_color_map_index(int length, int index)
            {
                Color back = Color.Black;

                // from 1 to 0 exclude -1
                double[,] color_maps = new double[,] 
                {
                    { -1, 1, 0 },
                    { 1, 0, -1 },
                };

                index = Math.Abs(index);
                index %= length;    // Only get nums in that spectrum

                // Get if it is green-red, red-blue
                double spectrum_length = 2.0;
                double i_spectrum_val = (double)index / (double)length;
                double i_spectrum_length = 1.0 / spectrum_length;
                int i_spectrum_index = 0;

                for (int i = 0; i < spectrum_length; i++)
                    if (i_spectrum_val < i_spectrum_length * (i + 1))
                    {
                        i_spectrum_index = i;
                        i_spectrum_val -= i_spectrum_length * i;
                        break;
                    }

                // Create the actual color
                double inner_val = i_spectrum_val / i_spectrum_length;
                double[] rgb = new double[3];

                for (int i = 0; i < 3; i++)
                    if (color_maps[i_spectrum_index, i] != -1)
                        rgb[i] = Math.Abs(color_maps[i_spectrum_index, i] - inner_val);

                back = Color.FromArgb((int)(rgb[0] * 255.0), (int)(rgb[1] * 255.0), (int)(rgb[2] * 255.0));

                return back;
            }

            // Only odd window_sizes else window_size--
            public static ImageH MedianFilter(ImageH image, int window_size)
            {
                ImageH img = new ImageH();
                img.bmp = (Bitmap)image.bmp.Clone();
                img.Load_Parallel();

                int channels = img.p_bmp.depth/8;
                int length = img.p_bmp.rgb.Length / channels;

                int width = img.bmp.Width;
                int height = img.bmp.Height;
                int half_window_size =  (int)Math.Floor(window_size / 2.0);

                if (window_size % 2 == 0)
                    window_size--;

                Parallel.For(0, length, i =>
                {
                    byte[] w_array;
                    int cy = (int)Math.Floor((double) i / width);   // Center y (effected median pixel)
                    int cx = i - (cy * width);                       // Center x (effected median pixel)

                    int y = cy - half_window_size;
                    int x = cx - half_window_size;


                    for (int j = 0; j < channels; j++)
                    {
                        w_array = new byte[window_size*window_size];

                        for (int iy = 0; iy < window_size; iy++)
                            for (int ix = 0; ix < window_size; ix++)
                            {
                                lock (w_array)
                                {
                                    if (y + iy < 0 || x + ix < 0 || y + iy >= height || x + ix >= width)
                                        w_array[(iy * window_size) + ix] = 127;
                                    else
                                    {
                                        int offset = ((((y + iy) * width) + (x + ix)) * channels) + j;
                                        lock (img)
                                            w_array[(iy * window_size) + ix] = img.p_bmp.rgb[offset];
                                    }
                                }
                            }

                        lock (w_array)
                        {
                            Array.Sort(w_array);
                            int c_offset = (((cy * width) + cx) * channels) + j;

                            lock (img)
                                img.p_bmp.rgb[c_offset] = w_array[window_size * window_size / 2];
                        }
                    }
                });

                img.Write_Parallel_To_Bmp();
                return img;
            }
        }
    }
}