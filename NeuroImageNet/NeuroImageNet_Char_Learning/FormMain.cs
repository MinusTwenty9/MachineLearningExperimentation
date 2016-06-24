using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NeuroImageNet.IO;
using NeuroImageNet.Bmp;
using System.Threading.Tasks;
using System.IO;

namespace NeuroImageNet_Char_Learning
{
    public partial class FormMain : Form
    {
        public CharLearner char_learner;

        public FormMain()
        {
            InitializeComponent();
        }

        private void button_idx_Click(object sender, EventArgs e)
        {
            string label_path = Std.OpenFile("*.idx1-ubyte");
            string image_path = Std.OpenFile("*.idx3-ubyte");

            if (label_path == null || image_path == null) return;

            if (char_learner == null)
                char_learner = new CharLearner();
            char_learner.Load_Idx(image_path, label_path);
        }

        private void button_load_image_Click(object sender, EventArgs e)
        {
            ImageH display_image = null;
            int label = 0;
            int output_name = 0;

            char_learner.Run(ref display_image, ref output_name, ref label);

            label_run_info.Text = "Output / Label:\n" + output_name + "\n" + label;
            if (picture_box_display.Image != null)
                picture_box_display.Image.Dispose();
            picture_box_display.Image = (Bitmap)display_image.bmp.Clone();
            
            display_image.Dispose();
        }

        private void button_learn_Click(object sender, EventArgs e)
        {
            if (char_learner.training == false)
            {
                char_learner.StartTrain();
                button_learn.Text = "Stop Learning";
                timer_update.Enabled = true;
            }
            else
            {
                char_learner.StopTraining();
                button_learn.Text = "Start Learning";
                timer_update.Enabled = false;
            }
        }

        private void button_compute_accuracy_Click(object sender, EventArgs e)
        {
            int ittr_percent = 0;
            double avg_percent = 0;

            for (int y = 0; y < 10; y++)
            {
                int ittr = 0;
                double avg = 0;

                int label = 0;
                int output = 0;

                for (int i = 0; i < 1000; i++)
                {
                    char_learner.Run(ref output, ref label);
                    if (label == output) avg++;
                    ittr++;
                }

                avg /= ittr; 
                avg_percent += avg;
                ittr_percent++;

                label_accuracy.Text = "Accuracy: Loading... " + (y * 10) + "% done";
                Application.DoEvents();
            }

            avg_percent /= ittr_percent;
            avg_percent *= 100;

            label_accuracy.Text = "Accuracy: " + avg_percent + "%";
        }

        private void timer_update_Tick(object sender, EventArgs e)
        {
            //ImageH image = new ImageH();
            //image.bmp = new Bitmap(char_learner.idx.width, char_learner.idx.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //image.Load_Parallel();

            int length = char_learner.idx.width * char_learner.idx.height;
            int training_index = char_learner.training_index;

        //    if (char_learner.hog[training_index] == null || char_learner.hog[training_index].image == null) goto Dispose;

        //    image = char_learner.hog[training_index].image.Clone();
            
        //    //image.Write_Parallel_To_Bmp();

        //    if (picture_box_display.Image != null)
        //        picture_box_display.Image.Dispose();
        //    picture_box_display.Image = (Bitmap)image.bmp.Clone();

        //    goto Dispose;

        //Dispose:
        //    image.Dispose();

            label_learning_itterations.Text = "Learning_Itterations: " + char_learner.training_ittr.ToString();
            label_average.Text = "Average: " + char_learner.GetAverageError();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            string save_path = Std.SaveFile("*.ats");
            if (save_path == null) return;

            char_learner.bpn.Save(save_path);
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            string bpn_path = Std.OpenFile("*.ats");

            if (bpn_path == null) return;

            if (char_learner == null)
                char_learner = new CharLearner();
            char_learner.Load_Bpn(bpn_path);
        }

        private void button_run_image_Click(object sender, EventArgs e)
        {
            string image_path = Std.OpenFile();
            if (image_path == null) return;

            ImageH image = new ImageH(image_path);

            if (image.bmp.Width != char_learner.idx.width ||
                image.bmp.Height != char_learner.idx.height)
            {
                Bitmap c_bmp = new Bitmap(char_learner.idx.width, char_learner.idx.height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(c_bmp))
                {
                    g.DrawImage(image.bmp, new Rectangle(0,0,c_bmp.Width, c_bmp.Height));
                    g.Save();
                    g.Dispose();
                }
                image.bmp = (Bitmap)c_bmp.Clone();
                c_bmp.Dispose();
                c_bmp = null;
            }

            int output = 0;

            char_learner.Run(image, ref output);

            label_run_info.Text = "Output / Label:\n" + output + "\n-";
            if (picture_box_display.Image != null)
                picture_box_display.Image.Dispose();
            picture_box_display.Image = (Bitmap)image.bmp.Clone();

            image.Dispose();
        }
        
        private void button_run_real_image_Click(object sender, EventArgs e)
        {
            
            string image_path = Std.OpenFile();
            if (image_path == null) return;

            ImageH image = new ImageH(image_path);
            Runner runner = new Runner(char_learner, image);
            runner.Load_HOG();
            ImageH back = runner.Run();

            back.Save(Environment.CurrentDirectory + "\\runner_image.png",true);
        }

        private void button_negative_learning_Click(object sender, EventArgs e)
        {
            if (char_learner.training == false)
            {
                string folder_path = Std.OpenFolder();
                if (folder_path == null) return;

                string[] n_image_paths = Directory.GetFiles(folder_path);
                char_learner.negative_image_paths = n_image_paths;

                char_learner.StartNegativeTraining();
                button_negative_learning.Text = "Stop N. Learning";
                timer_update.Enabled = true;
            }
            else
            {
                char_learner.StopTraining();
                button_negative_learning.Text = "Start N. Learning";
                timer_update.Enabled = false;
            }
        }
    }
}
