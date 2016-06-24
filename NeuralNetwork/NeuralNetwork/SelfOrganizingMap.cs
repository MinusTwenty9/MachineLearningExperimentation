using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using SaveLib;

namespace NeuralNetwork
{
    public class SelfOrganizingMap
    {
        #region Initialization

        public SelfOrganizingMap(int input_size, int[] dimension_sizes)
        { 
            // Set variables
            this.input_size = input_size;
            this.dimension_sizes = dimension_sizes;
            this.dimension_count = dimension_sizes.Length;
            this.node_count = 1;

            for (int i = 0; i < dimension_count; i++)
                node_count *= dimension_sizes[i];

            // Setup lattice
            this.lattice = new SOMNode[node_count];
            int[] dimension_counter = new int[dimension_count];
            for (int i = 0; i < node_count; i++)
            {
                lattice[i] = new SOMNode(input_size, dimension_counter);
                add_to_dimension_counter(ref dimension_counter, dimension_sizes);
            }
        }

        public SelfOrganizingMap(string path)
        {
            Load(path);
        }

        #endregion

        #region Variables 

        public string name = "Default";

        private int input_size;
        private int node_count;
        private int dimension_count;
        private int[] dimension_sizes;
        private int[] bmu_pos;

        public SOMNode[] lattice;

        #endregion

        #region Methods

        public void Create_SOM(double[][] inputs, int itterations, double learning_rate, bool parallel = false)
        {
            double map_radius = 0;
            double map_time_const = 0;
            double map_neighbourhood_radius = 0;
            double map_learning_rate = 0;

            double[] input;

            for (int i = 0; i < dimension_count; i++)
                if (dimension_sizes[i] > map_radius)
                    map_radius = dimension_sizes[i];

            map_radius /= 2;
            map_time_const = itterations / Math.Log(map_radius);


            int itteration_count = 0;
            while (--itterations >= 0)
            {
                input = inputs[itteration_count];

                // Calculate map parameters
                map_neighbourhood_radius = map_radius * Math.Exp(-(double)itteration_count / map_time_const);

                // Find the BMU
                int bmu_index = find_best_matching_node_index(input);
                bmu_pos = lattice[bmu_index].d_pos;

                double radius_sq = map_neighbourhood_radius * map_neighbourhood_radius;

                // Adjustin weights of the nodes

                for (int n = 0; n < node_count; n++)
                {
                    double dist_to_node = lattice[n].Get_Pos_Distance(bmu_pos);

                    if (dist_to_node < (radius_sq))
                    {
                        double node_influence = Math.Exp(-(dist_to_node) / (2 * radius_sq));
                        lattice[n].Adjust_Weights(input, learning_rate, node_influence);
                    }
                }


                map_learning_rate = learning_rate * Math.Exp(-(double)itteration_count / (double)itterations);
                itteration_count++;

                if (itterations % 100 == 0)
                {
                    //Console.Clear();
                    //Console.WriteLine(itterations.ToString());
                }
            }
        }

        public SOMNode Run(double[] input)
        {
            return (SOMNode)lattice[find_best_matching_node_index(input)];
        }

        public void Save(string file_path)
        {
            if (file_path == null)
                return;

            // SaveLib FileSaver 
            FileSaver saver = new FileSaver(file_path,24, "som_file_" + name);

            // Head Info
            saver.Add_String("SelfOrganizingMap");
            saver.Add_String(name);
            saver.Add_String(input_size.ToString());
            saver.Add_String(node_count.ToString());
            saver.Add_String(dimension_count.ToString());

            // Dimension Sizes
            saver.Add_Var_Array<int>(dimension_sizes);

            // Node Loop
            for (int n = 0; n < node_count; n++)
            {
                saver.Add_Var_Array<double>(lattice[n].weights);
                saver.Add_Var_Array<int>(lattice[n].d_pos);
            }


            // Save
            saver.Close();
            saver.Dispose();
        }

        public void Load(string file_path)
        {
            if (file_path == null)
                return;

            FileLoader loader = new FileLoader(file_path);

            // Not the right file_structure
            if (loader.Get_Var() != "SelfOrganizingMap") return;
            
            // Read head info
            name = loader.Get_Var();
            int.TryParse(loader.Get_Var(), out input_size);
            int.TryParse(loader.Get_Var(), out node_count);
            int.TryParse(loader.Get_Var(), out dimension_count);
            
            // Dimension Sizes
            dimension_sizes = loader.Get_Var<int>();

            // Node Loop
            lattice = new SOMNode[node_count];

            for (int n = 0; n < node_count; n++)
            {
                double[] weights = loader.Get_Var<double>();
                int[] d_pos = loader.Get_Var<int>();

                lattice[n] = new SOMNode(weights,d_pos);
            }

            loader.Dispose();
        }

        private int find_best_matching_node_index(double[] input)
        {
            double min_distance = -1;
            int index = 0;

            for (int i = 0; i < node_count; i++)
            {
                double distance = lattice[i].Get_Distance_Sq(input);
                if (distance < min_distance || min_distance == -1)
                {
                    min_distance = distance;
                    index = i;
                }
            }

            return index;
        }

        // Adds 1 to the dimension_counter
        private void add_to_dimension_counter(ref int[] dimension_counter, int[] dimension_sizes)
        {
            for (int i = 0; i < dimension_count; i++)
                if (!add_dimension_p1(ref dimension_counter[i], dimension_sizes[i]))
                    break;
        }
        // Adds 1 to a dimension index
        private bool add_dimension_p1(ref int index, int length)
        {
            index++;

            if (index >= length) { index = 0; return true; }
            else return false;
        }

        #endregion
    }

    public class SOMNode
    {
        #region Initialization

        public SOMNode(int input_size, int[] d_pos)
        { 
            weights = new double[input_size];

            for (int i = 0; i < input_size; i++)
                weights[i] = Gaussian.Get_Random_Gaussian();

            this.d_pos = (int[])d_pos.Clone();
        }

        public SOMNode(double[] weights, int[] d_pos)
        {
            this.weights = weights;
            this.d_pos = d_pos;
        }

        #endregion

        #region Variables

        public double[] weights;
        public int[] d_pos;

        #endregion

        #region Methods

        // Distance between input and node weights sq (x^2)
        public double Get_Distance_Sq(double[] input)
        {
            double distance = 0;

            for (int i = 0; i < weights.Length; i++)
                distance += (input[i] - weights[i]) * (input[i] - weights[i]);

            return Math.Sqrt(distance);
        }

        // Distance between input and node weights
        public double Get_Pos_Distance(int[] pos)
        {
            double distance = 0;

            for (int i = 0; i < d_pos.Length; i++)
                distance += (pos[i] - d_pos[i]) * (pos[i] - d_pos[i]);

            return distance;
        }

        public void Adjust_Weights(double[] input, double learning_rate, double influence)
        {
            for (int w = 0; w < input.Length; w++)
                weights[w] += learning_rate * influence * (input[w] - weights[w]);
        }


        #endregion
    }
}
