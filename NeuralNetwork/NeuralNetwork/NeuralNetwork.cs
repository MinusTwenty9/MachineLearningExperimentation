using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SaveLib;
using ZBitmap;
using System.Drawing;
using System.IO;

namespace NeuralNetwork
{

    #region Transferfunctions and their derivatives

    public enum TransferFunction
    {
        None=0,
        Sigmoid=1,
        Linear=2,
        Gaussian=3,
        RationalSigmoid=4,
        TanH=5
    }

    static class TransferFunctions
    {
        public static double Evaluate(TransferFunction t_func,double input)
        {
            switch (t_func)
            { 
                case TransferFunction.Sigmoid:
                    return sigmoid(input);

                case TransferFunction.Linear:
                    return linear(input);

                case TransferFunction.Gaussian:
                    return gaussian(input);

                case TransferFunction.RationalSigmoid:
                    return rational_sigmoid(input);

                case TransferFunction.TanH:
                    return tanh(input);

                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }

        public static double EvaluateDerivative(TransferFunction t_func, double input)
        {
            switch (t_func)
            { 
                case TransferFunction.Sigmoid:
                    return sigmoid_derivative(input);

                case TransferFunction.Linear:
                    return linear_derivative(input);

                case TransferFunction.Gaussian:
                    return gaussian_derivative(input);

                case TransferFunction.RationalSigmoid:
                    return rational_sigmoid_derivative(input);

                case TransferFunction.TanH:
                    return tanh_derivative(input);

                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }

        /* Transfer Functions destination */
        private static double sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        private static double sigmoid_derivative(double x)
        {
            return sigmoid(x) * (1 - sigmoid(x));
        }

        private static double linear(double x)
        {
            return x;
        }
        private static double linear_derivative(double x)
        {
            return 1;
        }

        private static double gaussian(double x)
        {
            return Math.Exp(-Math.Pow(x,2));
        }
        private static double gaussian_derivative(double x)
        {
            return -2.0 * x * gaussian(x);
        }

        private static double rational_sigmoid(double x)
        {
            return x / (1 + Math.Sqrt(1 + x * x));
        }
        private static double rational_sigmoid_derivative(double x)
        {
            double val = Math.Sqrt(1+x*x);
            return 1.0 / (val * (1 + val));
        }

        private static double tanh(double x)
        {
            return Math.Tanh(x);
        }
        private static double tanh_derivative(double x)
        {
            return Math.Pow(sech(x), 2);
        }

        private static double sech(double x)
        {
            return 1 / Math.Cosh(x);
        }
    }


    #endregion


    public class BackPropagationNetwork
    {
        #region Constructors

        public BackPropagationNetwork(int[] layer_sizes, TransferFunction[] transfer_functions)
        { 
            // Validate the input Data
            if (layer_sizes.Length != transfer_functions.Length || transfer_functions[0] != TransferFunction.None)
                throw new ArgumentException("Can not construct a Network with these parameters.");

            // Initialize Network layers
            layer_count = layer_sizes.Length - 1;
            input_size = layer_sizes[0];
            layer_size = new int[layer_count];

            for (int i = 0; i < layer_count; i++)
                layer_size[i] = layer_sizes[i + 1];

            transfer_function = new TransferFunction[layer_count];
            for (int i = 0; i < layer_count; i++)
                transfer_function[i] = transfer_functions[i + 1];

            // Start dimensioning our arrays
            bias = new double[layer_count][];
            previous_bias_delta = new double[layer_count][];
            delta = new double[layer_count][];
            layer_output = new double[layer_count][];
            layer_input = new double[layer_count][];

            weight = new double[layer_count][][];
            previous_weight_delta = new double[layer_count][][];

            // Fill 2d arrays
            for (int l = 0; l < layer_count; l++)
            {
                bias[l] = new double[layer_size[l]];
                previous_bias_delta[l] = new double[layer_size[l]];
                delta[l] = new double[layer_size[l]];

                layer_output[l] = new double[layer_size[l]];
                layer_input[l] = new double[layer_size[l]];

                weight[l] = new double[l == 0 ? input_size : layer_size[l-1]][];
                previous_weight_delta[l] = new double[l == 0 ? input_size : layer_size[l-1]][];

                for (int i = 0; i < (l == 0 ? input_size : layer_size[l-1]); i++)
                {
                    weight[l][i] = new double[layer_size[l]];
                    previous_weight_delta[l][i] = new double[layer_size[l]];
                }
            }


            ///////
            // I = Input Weights
            // J = Hidden Neurons (In weights for NJ)

            // Initialize the weights
            for (int l = 0; l < layer_count; l++)
            {
                for (int j = 0; j < layer_size[l]; j++)
                {
                    bias[l][j] = Gaussian.Get_Random_Gaussian();
                    previous_bias_delta[l][j] = 0.0;
                    layer_output[l][j] = 0.0;
                    layer_input[l][j] = 0.0;
                    delta[l][j] = 0.0;

                }

                for (int i = 0; i < (l == 0 ? input_size : layer_size[l-1]); i++)
                {
                    for (int j = 0; j < layer_size[l]; j++)
                    {
                        weight[l][i][j] = Gaussian.Get_Random_Gaussian();
                        previous_weight_delta[l][i][j] = 0.0;
                    }
                }
            }

        }

        public BackPropagationNetwork(string file_path)
        {
            loaded = false;

            Load(file_path);

            loaded = true;
        }

        #endregion

        #region Methods

        public void Run(ref double[] input, out double[] output)
        { 
            // Make sure we have enough data
            if (input.Length != input_size)
                throw new ArgumentException("Input data is not of currect dimension.");

            // Dimension
            output = new double[layer_size[layer_count-1]];


            /* Run the Network */
            for (int l = 0; l < layer_count; l++)
            {
                for (int j = 0; j < layer_size[l]; j++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < (l == 0 ? input_size : layer_size[l - 1]); i++)
                        sum += weight[l][i][j] * (l == 0 ? input[i] : layer_output[l - 1][i]);

                    sum += bias[l][j];
                    layer_input[l][j] = sum;

                    layer_output[l][j] = TransferFunctions.Evaluate(transfer_function[l],sum);
                }
            }

            // Copy the output to the output array
            for (int i = 0; i < layer_size[layer_count - 1]; i++)
                output[i] = layer_output[layer_count - 1][i];
        }

        public double Train(ref double[] input, ref double[] desired, double training_rate, double momentum)
        { 
            // Parameter Validation
            if (input.Length != input_size)
                throw new ArgumentException("Invaliud Input parameter", "input");
            if (desired.Length != layer_size[layer_count - 1])
                throw new ArgumentException("Invalid input parameter", "desired");

            // Local variables
            double error = 0.0, sum = 0.0, weight_delta = 0.0, bias_delta = 0.0;
            double[] output = new double[layer_size[layer_count-1]];

            // Run the network
            Run(ref input,out output);

            // Backpropagate the error
            for (int l = layer_count-1; l >=0; l--)
            {
                // Output layer
                if (l == layer_count - 1)
                {
                    for (int k = 0; k < layer_size[l]; k++)
                    {
                        delta[l][k] = output[k] - desired[k];
                        error += Math.Pow(delta[l][k],2);
                        delta[l][k] *= TransferFunctions.EvaluateDerivative(transfer_function[l],layer_input[l][k]);
                    }
                }
                else   // Hidden layer
                {
                    for (int i = 0; i < layer_size[l]; i++)
                    {
                        sum = 0.0;
                        for (int j = 0; j < layer_size[l + 1]; j++)
                        { 
                            sum += weight[l+1][i][j] * delta[l+1][j];
                        }
                        sum *= TransferFunctions.EvaluateDerivative(transfer_function[l],layer_input[l][i]);

                        delta[l][i] = sum;
                    }
                }
            }

            // Update the weights and biases
            for (int l = 0;l < layer_count; l++)
                for (int i = 0; i < (l == 0 ? input_size : layer_size[l-1]);i++)
                    for (int j = 0; j < layer_size[l]; j++)
                    {
                        weight_delta = training_rate * delta[l][j] * (l == 0 ? input[i] : layer_output[l - 1][i]);
                        weight[l][i][j] -= weight_delta + momentum * previous_weight_delta[l][i][j];

                        previous_weight_delta[l][i][j] = weight_delta;
                    }


            for (int l = 0; l < layer_count; l++)
                for (int i = 0; i < layer_size[l]; i++)
                {
                    bias_delta = training_rate * delta[l][i];
                    bias[l][i] -= bias_delta + momentum * previous_bias_delta[l][i];

                    previous_bias_delta[l][i] = bias_delta;
                }

            return error;
        }

        public void Save(string file_path)
        {
            if (file_path == null)
                return;

            FileSaver saver = new FileSaver(file_path,24, "bpn_file_" + name);

            // Head info
            saver.Add_String("BackPropagationNetwork");
            saver.Add_String(name);
            saver.Add_String(input_size.ToString());
            saver.Add_String(layer_count.ToString());

            // Add layer info
            saver.Add_Var_Array<int>(layer_size);
            byte[] tf = new byte[layer_count];
            for (int l = 0; l < layer_count; l++)
                tf[l] = (byte)(int)transfer_function[l];

            saver.Add_Var_Array<byte>(tf);


            // Weights and Biase's
            for (int l = 0; l < layer_count; l++)
            {
                // Bias
                saver.Add_Var_Array<double>(bias[l]);

                // Weights
                for (int i = 0; i < (l == 0 ? input_size : layer_size[l - 1]); i++)
                    saver.Add_Var_Array<double>(weight[l][i]);
            }

            saver.Close();
            saver.Dispose();
        }

        public void Load(string file_path)
        {
            if (file_path == null)
                return;

            FileLoader loader = new FileLoader(file_path);
            if (loader.Get_Var() != "BackPropagationNetwork") return;

            // Read head info
            name = loader.Get_Var();
            int.TryParse(loader.Get_Var(), out input_size);
            int.TryParse(loader.Get_Var(), out layer_count);

            // Read layer info
            layer_size = loader.Get_Var<int>();
            transfer_function = new TransferFunction[layer_count];
            byte[] tf = loader.Get_Var<byte>();
            for (int l = 0; l < layer_count; l++)
                transfer_function[l] = (TransferFunction)(int)tf[l];

            // Setup variables
            // Start dimensioning our arrays
            bias = new double[layer_count][];
            previous_bias_delta = new double[layer_count][];
            delta = new double[layer_count][];
            layer_output = new double[layer_count][];
            layer_input = new double[layer_count][];

            weight = new double[layer_count][][];
            previous_weight_delta = new double[layer_count][][];

            // Fill 2d arrays
            for (int l = 0; l < layer_count; l++)
            {
                bias[l] = new double[layer_size[l]];
                previous_bias_delta[l] = new double[layer_size[l]];
                delta[l] = new double[layer_size[l]];

                layer_output[l] = new double[layer_size[l]];
                layer_input[l] = new double[layer_size[l]];

                weight[l] = new double[l == 0 ? input_size : layer_size[l - 1]][];
                previous_weight_delta[l] = new double[l == 0 ? input_size : layer_size[l - 1]][];

                for (int i = 0; i < (l == 0 ? input_size : layer_size[l - 1]); i++)
                {
                    weight[l][i] = new double[layer_size[l]];
                    previous_weight_delta[l][i] = new double[layer_size[l]];
                }
            }


            // Weights and Biase's
            for (int l = 0; l < layer_count; l++)
            {
                bias[l] = loader.Get_Var<double>();

                for (int i = 0; i < (l == 0 ? input_size : layer_size[l - 1]); i++)
                    weight[l][i] = loader.Get_Var<double>();
            }

            loader.Dispose();
        }

        private string x_path_value(string x_path)
        {
            XmlNode node = doc.SelectSingleNode(x_path);

            if (node == null)
                throw new ArgumentException("Can not find specified node", x_path);

            return node.InnerText;
        }

        #endregion

        #region Public data

        public string name = "Default";

        #endregion

        #region Private Data

        public int layer_count;
        public int input_size;
        public int[] layer_size;
        private TransferFunction[] transfer_function;

        private double[][] layer_output;
        private double[][] layer_input;
        public double[][] bias;
        private double[][] delta;
        private double[][] previous_bias_delta;

        public double[][][] weight;
        private double[][][] previous_weight_delta;

        private XmlDocument doc = null;
        private bool loaded = true;

        #endregion
    }

    public static class BPNStatistics
    {
        public static void Create_Input_Weights_Grey_Scale_Images(BackPropagationNetwork bpn, string image_folder_path, int width, int height, string image_name = "bpn_input_weights_grey_scale_img_")
        {
            if (bpn == null) return;
            if (width * height != bpn.input_size) return;       // Wrong Pixel Size

            // Add's a Backslash to the image_folder_path if there is none
            if (image_folder_path.Substring(image_folder_path.Length - 1, 1) != "\\")
                image_folder_path += "\\";

            // Create Directory if it doesn't exist
            if (!Directory.Exists(image_folder_path))
                Directory.CreateDirectory(image_folder_path);

            // Create Image Data
            for (int j = 0; j < bpn.layer_size[0]; j++)
            {
                ImageH img = new ImageH();
                img.bmp = new Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                img.Load_Parallel();

                for (int i = 0; i < bpn.input_size; i++)
                {
                    byte pixel = (byte)(bpn.weight[0][i][j] * 256.0);
                    img.p_bmp.rgb[i * 3] = pixel;
                    img.p_bmp.rgb[i * 3+1] = pixel;
                    img.p_bmp.rgb[i * 3+2] = pixel;
                }

                img.Write_Parallel_To_Bmp();
                img.Save(image_folder_path+image_name + j.ToString() + ".png",true);
                img.Dispose();
            }
        }
    }

    public static class Gaussian
    {
        private static Random rand = new Random();

        public static double Get_Random_Gaussian()
        {
            return Get_Random_Gaussian(0.0, 1.0);
        }

        public static double Get_Random_Gaussian(double mean, double stddev)
        {
            double r_val1, r_val2;

            Get_Random_Gaussian(mean, stddev, out r_val1, out r_val2);

            return r_val1;
        }

        public static void Get_Random_Gaussian(double mean, double stddev, out double val1, out double val2)
        {
            double u, v, s, t;

            do
            {
                u = 2 * rand.NextDouble() - 1;
                v = 2 * rand.NextDouble() - 1;
            } while (u * u + v * v > 1 || (u == 0 && v == 0));

            s = u * u + v * v;
            t = Math.Sqrt((-2.0 * Math.Log(s)) / s);

            val1 = stddev * u * t + mean;
            val2 = stddev * v * t + mean;
        }
    }
}
