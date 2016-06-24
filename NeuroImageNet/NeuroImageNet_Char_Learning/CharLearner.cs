using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroImageNet.HistogramofOrientedGradients;
using NeuroImageNet.Bmp;
using NeuralNetwork;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace NeuroImageNet_Char_Learning
{
    public class CharLearner
    {
        public HOG[] hog;
        public BackPropagationNetwork bpn;
        public NeuralNetwork.idxX_ubyte idx;
        public ImageH image;
        public int hog_index = -1;
        public int training_ittr = 0;
        public int training_index = -1;
        private double t_err = 0;
        public bool training = false;

        public int[] bpn_layer_size;
        public int hidden_size = 15;
        public int output_size = 10;
        public TransferFunction[] transfer_functions;
        public double training_rate = 0.1;
        public double momentum = 0.025;

        public int bin_count = 9;
        public int block_size = 3;
        public int cell_size = 9;

        public string[] negative_image_paths;

        private Random rand = new Random();

        public CharLearner()
        { }

        public void Load_Bpn(string bpn_path)
        {
            LoadBpn(bpn_path);
        }

        public void Load_Idx(string image_path, string label_path)
        {
            idx = new NeuralNetwork.idxX_ubyte(image_path, label_path);
            hog = new HOG[idx.image_count];
            
            LoadHog(0);
            LoadBpn();
        }


        public void LoadHog(int index)
        {
            if (hog[index] != null) return;

            ImageH image = new ImageH();
            image.bmp = new Bitmap(idx.width,idx.height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            image.Load_Parallel();

            int length = idx.width * idx.height;
            byte[] pixels = new byte[length];
            byte label = 0;
            if (!idx.Get_Image(index,ref pixels,ref label))return;

            Parallel.For(0, length, i =>
            {
                image.p_bmp.rgb[(i * 3)] = pixels[i];
                image.p_bmp.rgb[(i * 3)+1] = pixels[i];
                image.p_bmp.rgb[(i * 3)+2] = pixels[i];
            });

            image.Write_Parallel_To_Bmp();
            hog[index] = new HOG(image);
            hog[index].CreateGradientVector();
            hog[index].gradient_vector.NormalizeVectorGradient();
            hog[index].CreateCellHistograms(cell_size,bin_count);
            hog[index].CreateBlocks(block_size);
        }

        private void LoadBpn()
        {
            if (bpn != null) return;

            int input_size = hog[0].blocks.block_count * hog[0].blocks.blocks[0].block_vector.Length;
            bpn_layer_size = new int[]
            {
                input_size,
                hidden_size,
                output_size
            };

            transfer_functions = new TransferFunction[] 
            {
                TransferFunction.None,
                TransferFunction.Sigmoid,
                TransferFunction.Sigmoid
            };

            bpn = new BackPropagationNetwork(bpn_layer_size, transfer_functions);
        }

        private void LoadBpn(string path)
        {
            bpn = new BackPropagationNetwork(path);

            bpn_layer_size = new int[]
            {
                bpn.input_size,
                bpn.layer_size[0],
                bpn.layer_size[1]
            };

            transfer_functions = new TransferFunction[] 
            {
                TransferFunction.None,
                TransferFunction.Sigmoid,
                TransferFunction.Sigmoid
            };
        }

        public void StartTrain()
        {
            if (training) return;

            training = true;
            Thread train_thread = new Thread(new ThreadStart(train_thread_handler));
            train_thread.ApartmentState = ApartmentState.STA;
            train_thread.Start();
        }

        public void StartNegativeTraining()
        {
            if (training) return;

            training = true;
            Thread train_thread = new Thread(new ThreadStart(training_negative_thread_handler));
            train_thread.ApartmentState = ApartmentState.STA;
            train_thread.Start();
        }

        public void StopTraining()
        {
            training = false;
        }

        private void train_thread_handler()
        {
            int index_offset = 0;
            int index = 0;
            int index_range = 10000;
            int itterations = 5;
            int c_itteration = 0;

            while (training)
            {

                training_index = index_offset + index;
                
                LoadHog(training_index);

                double[] input;
                double[] dessired;
                byte[] pixels = null;
                byte label = 0;

                input = new double[bpn_layer_size[0]];
                dessired = new double[bpn_layer_size[bpn_layer_size.Length - 1]];

                idx.Get_Image(training_index, ref pixels, ref label);
                dessired[label] = 1.0;

                //for (int b = 0; b < hog[training_index].blocks.block_count; b++)
                //{
                //    for (int i = 0; i < input.Length; i++)
                //        input[i] = hog[training_index].blocks.blocks[b].block_vector[i];
                //    t_err += bpn.Train(ref input, ref dessired, training_rate, momentum);
                //}
                
                for (int b = 0; b < hog[training_index].blocks.block_count; b++)
                {
                    for (int i = 0; i < hog[0].blocks.blocks[0].block_vector.Length; i++)
                        input[(b * hog[0].blocks.blocks[0].block_vector.Length) + i] = hog[training_index].blocks.blocks[b].block_vector[i];
                    
                }
                t_err += bpn.Train(ref input, ref dessired, training_rate, momentum);

                training_ittr++;


                index++;
                if (index_offset + index >= idx.image_count || index >= index_range)
                {
                    index = 0;
                    c_itteration++;

                    if (c_itteration >= itterations)
                    {
                        // Dispose of HOG
                        for (int i = 0; i < index_range; i++)
                        {
                            if (i + index_offset >= idx.image_count) break;

                            if (index_offset + i != 0)
                            {
                                hog[index_offset + i].Dispose();
                                hog[index_offset + i] = null;
                            }
                        }

                        c_itteration = 0;
                        index_offset += index_range;

                        if (index_offset >= idx.image_count)
                            index_offset = 0;
                    }
                }
            }
        }

        private void training_negative_thread_handler()
        {
            for (int i = 0; i < negative_image_paths.Length; i++)
            {
                if (!File.Exists(negative_image_paths[i])) continue;

                HOG n_hog = new HOG(new ImageH(negative_image_paths[i]));
                n_hog.CreateGradientVector(true);
                n_hog.gradient_vector.NormalizeVectorGradient();
                n_hog.CreateCellHistograms(cell_size, bin_count);
                n_hog.CreateBlocks(block_size);

                for (int y = 0; y < 1 && training; y++)
                    for (int b = 0; b < n_hog.blocks.block_count && training; b++)
                    {
                        double[] input;
                        double[] dessired;

                        input = n_hog.blocks.blocks[b].block_vector;
                        dessired = new double[bpn_layer_size[bpn_layer_size.Length - 1]];
                        bpn.Train(ref input, ref dessired, training_rate/10, momentum/10);
                        training_ittr++;
                    }

                n_hog.Dispose();
            }
        }

        public void Run(ref ImageH image, ref int output_num, ref int label)
        {
            int index = rand.Next(idx.image_count);
            LoadHog(index);

            image = hog[index].image.Clone();

            double[] input;
            double[] output;
            byte[] pixels = null;
            byte b_label = 0;

            input = new double[bpn_layer_size[0]];
            idx.Get_Image(index, ref pixels, ref b_label);

            for (int b = 0; b < hog[index].blocks.block_count; b++)
            {
                for (int i = 0; i < hog[0].blocks.blocks[0].block_vector.Length; i++)
                    input[(b * hog[0].blocks.blocks[0].block_vector.Length) + i] = hog[index].blocks.blocks[b].block_vector[i];
            }

            bpn.Run(ref input, out output);

            double max_val = 0;
            int max_index = 0;

            for (int i = 0; i < output.Length; i++)
                if (output[i] > max_val)
                {
                    max_val = output[i];
                    max_index = i;
                }

            output_num = max_index;
            label = b_label;
        }

        public void Run(ref int output_num, ref int label)
        {
            int index = rand.Next(idx.image_count);
            LoadHog(index);

            double[] input;
            double[] output;
            byte[] pixels = null;
            byte b_label = 0;

            input = new double[bpn_layer_size[0]];
            idx.Get_Image(index, ref pixels, ref b_label);

            for (int b = 0; b < hog[index].blocks.block_count; b++)
            {
                for (int i = 0; i < hog[0].blocks.blocks[0].block_vector.Length; i++)
                    input[(b * hog[0].blocks.blocks[0].block_vector.Length) + i] = hog[index].blocks.blocks[b].block_vector[i];
            }

            if (index != 0)
            {
                hog[index].Dispose();
                hog[index] = null;
            }

            bpn.Run(ref input, out output);

            double max_val = 0;
            int max_index = 0;

            for (int i = 0; i < output.Length; i++)
                if (output[i] > max_val)
                {
                    max_val = output[i];
                    max_index = i;
                }

            output_num = max_index;
            label = b_label;

            if (label != output_num)
            { }
        }

        public void Run(ref double[] output, ref int output_num, Block block )
        {
            double[] input = block.block_vector;
            bpn.Run(ref input, out output);

            double max_val = 0;
            int max_index = 0;

            for (int i = 0; i < output.Length; i++)
                if (output[i] > max_val)
                {
                    max_val = output[i];
                    max_index = i;
                }

            output_num = max_index;
        }

        public void Run(ImageH image, ref int output_num)
        {
            HOG hog = new HOG(image);
            hog.CreateGradientVector();
            hog.gradient_vector.NormalizeVectorGradient();
            hog.CreateCellHistograms(cell_size, bin_count);
            hog.CreateBlocks(block_size);

            double[] input;
            double[] output;
            byte[] pixels = null;
            byte b_label = 0;

            input = new double[bpn_layer_size[0]];

            for (int b = 0; b < hog.blocks.block_count; b++)
            {
                for (int i = 0; i < hog.blocks.blocks[0].block_vector.Length; i++)
                    input[(b * hog.blocks.blocks[0].block_vector.Length) + i] = hog.blocks.blocks[b].block_vector[i];
            }
            
            hog.Dispose();
            hog = null;

            bpn.Run(ref input, out output);

            double max_val = 0;
            int max_index = 0;

            for (int i = 0; i < output.Length; i++)
                if (output[i] > max_val)
                {
                    max_val = output[i];
                    max_index = i;
                }

            output_num = max_index;
        }

        public double GetAverageError()
        {
            return t_err / (training_ittr/* * bpn_layer_size[0]*/);
        }

        public void Dispose()
        {
            if (hog != null)
            {
                for (int i = 0; i < hog.Length; i++)
                    if (hog[i] != null)
                        hog[i].Dispose();
                hog = null;
            }
            if (bpn != null)
                bpn = null;
            if (idx != null)
                idx = null;
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }
    }
}
