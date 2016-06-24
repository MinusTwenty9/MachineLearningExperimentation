namespace NeuroImageNet_Char_Learning
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.picture_box_display = new System.Windows.Forms.PictureBox();
            this.button_idx = new System.Windows.Forms.Button();
            this.button_load_run = new System.Windows.Forms.Button();
            this.button_learn = new System.Windows.Forms.Button();
            this.label_learning_itterations = new System.Windows.Forms.Label();
            this.label_average = new System.Windows.Forms.Label();
            this.button_compute_accuracy = new System.Windows.Forms.Button();
            this.timer_update = new System.Windows.Forms.Timer(this.components);
            this.label_run_info = new System.Windows.Forms.Label();
            this.label_accuracy = new System.Windows.Forms.Label();
            this.button_save = new System.Windows.Forms.Button();
            this.button_load = new System.Windows.Forms.Button();
            this.button_run_image = new System.Windows.Forms.Button();
            this.button_run_real_image = new System.Windows.Forms.Button();
            this.button_negative_learning = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picture_box_display)).BeginInit();
            this.SuspendLayout();
            // 
            // picture_box_display
            // 
            this.picture_box_display.Location = new System.Drawing.Point(2, 1);
            this.picture_box_display.Name = "picture_box_display";
            this.picture_box_display.Size = new System.Drawing.Size(254, 256);
            this.picture_box_display.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picture_box_display.TabIndex = 0;
            this.picture_box_display.TabStop = false;
            // 
            // button_idx
            // 
            this.button_idx.Location = new System.Drawing.Point(262, 1);
            this.button_idx.Name = "button_idx";
            this.button_idx.Size = new System.Drawing.Size(124, 23);
            this.button_idx.TabIndex = 1;
            this.button_idx.Text = "Load idx";
            this.button_idx.UseVisualStyleBackColor = true;
            this.button_idx.Click += new System.EventHandler(this.button_idx_Click);
            // 
            // button_load_run
            // 
            this.button_load_run.Location = new System.Drawing.Point(262, 330);
            this.button_load_run.Name = "button_load_run";
            this.button_load_run.Size = new System.Drawing.Size(124, 23);
            this.button_load_run.TabIndex = 2;
            this.button_load_run.Text = "Run";
            this.button_load_run.UseVisualStyleBackColor = true;
            this.button_load_run.Click += new System.EventHandler(this.button_load_image_Click);
            // 
            // button_learn
            // 
            this.button_learn.Location = new System.Drawing.Point(262, 30);
            this.button_learn.Name = "button_learn";
            this.button_learn.Size = new System.Drawing.Size(124, 23);
            this.button_learn.TabIndex = 3;
            this.button_learn.Text = "Start Learning";
            this.button_learn.UseVisualStyleBackColor = true;
            this.button_learn.Click += new System.EventHandler(this.button_learn_Click);
            // 
            // label_learning_itterations
            // 
            this.label_learning_itterations.AutoSize = true;
            this.label_learning_itterations.Location = new System.Drawing.Point(-1, 340);
            this.label_learning_itterations.Name = "label_learning_itterations";
            this.label_learning_itterations.Size = new System.Drawing.Size(103, 13);
            this.label_learning_itterations.TabIndex = 4;
            this.label_learning_itterations.Text = "Learning_Itterations:";
            // 
            // label_average
            // 
            this.label_average.AutoSize = true;
            this.label_average.Location = new System.Drawing.Point(-1, 365);
            this.label_average.Name = "label_average";
            this.label_average.Size = new System.Drawing.Size(53, 13);
            this.label_average.TabIndex = 5;
            this.label_average.Text = "Average: ";
            // 
            // button_compute_accuracy
            // 
            this.button_compute_accuracy.Location = new System.Drawing.Point(262, 88);
            this.button_compute_accuracy.Name = "button_compute_accuracy";
            this.button_compute_accuracy.Size = new System.Drawing.Size(124, 23);
            this.button_compute_accuracy.TabIndex = 6;
            this.button_compute_accuracy.Text = "Compute Accuracy";
            this.button_compute_accuracy.UseVisualStyleBackColor = true;
            this.button_compute_accuracy.Click += new System.EventHandler(this.button_compute_accuracy_Click);
            // 
            // timer_update
            // 
            this.timer_update.Interval = 250;
            this.timer_update.Tick += new System.EventHandler(this.timer_update_Tick);
            // 
            // label_run_info
            // 
            this.label_run_info.AutoSize = true;
            this.label_run_info.Location = new System.Drawing.Point(12, 269);
            this.label_run_info.Name = "label_run_info";
            this.label_run_info.Size = new System.Drawing.Size(79, 13);
            this.label_run_info.TabIndex = 7;
            this.label_run_info.Text = "Output / Label:";
            // 
            // label_accuracy
            // 
            this.label_accuracy.AutoSize = true;
            this.label_accuracy.Location = new System.Drawing.Point(-1, 393);
            this.label_accuracy.Name = "label_accuracy";
            this.label_accuracy.Size = new System.Drawing.Size(55, 13);
            this.label_accuracy.TabIndex = 8;
            this.label_accuracy.Text = "Accuracy:";
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(262, 117);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(124, 23);
            this.button_save.TabIndex = 9;
            this.button_save.Text = "Save Bpn";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_load
            // 
            this.button_load.Location = new System.Drawing.Point(262, 146);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(124, 23);
            this.button_load.TabIndex = 10;
            this.button_load.Text = "Load Bpn";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // button_run_image
            // 
            this.button_run_image.Location = new System.Drawing.Point(262, 388);
            this.button_run_image.Name = "button_run_image";
            this.button_run_image.Size = new System.Drawing.Size(124, 23);
            this.button_run_image.TabIndex = 11;
            this.button_run_image.Text = "Run Image";
            this.button_run_image.UseVisualStyleBackColor = true;
            this.button_run_image.Click += new System.EventHandler(this.button_run_image_Click);
            // 
            // button_run_real_image
            // 
            this.button_run_real_image.Location = new System.Drawing.Point(262, 359);
            this.button_run_real_image.Name = "button_run_real_image";
            this.button_run_real_image.Size = new System.Drawing.Size(124, 23);
            this.button_run_real_image.TabIndex = 12;
            this.button_run_real_image.Text = "Run Real Image";
            this.button_run_real_image.UseVisualStyleBackColor = true;
            this.button_run_real_image.Click += new System.EventHandler(this.button_run_real_image_Click);
            // 
            // button_negative_learning
            // 
            this.button_negative_learning.Location = new System.Drawing.Point(262, 59);
            this.button_negative_learning.Name = "button_negative_learning";
            this.button_negative_learning.Size = new System.Drawing.Size(124, 23);
            this.button_negative_learning.TabIndex = 13;
            this.button_negative_learning.Text = "Start N. Learning";
            this.button_negative_learning.UseVisualStyleBackColor = true;
            this.button_negative_learning.Click += new System.EventHandler(this.button_negative_learning_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 419);
            this.Controls.Add(this.button_negative_learning);
            this.Controls.Add(this.button_run_real_image);
            this.Controls.Add(this.button_run_image);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.label_accuracy);
            this.Controls.Add(this.label_run_info);
            this.Controls.Add(this.button_compute_accuracy);
            this.Controls.Add(this.label_average);
            this.Controls.Add(this.label_learning_itterations);
            this.Controls.Add(this.button_learn);
            this.Controls.Add(this.button_load_run);
            this.Controls.Add(this.button_idx);
            this.Controls.Add(this.picture_box_display);
            this.Name = "FormMain";
            this.Text = "NeuralImageNet_Char_Learning";
            ((System.ComponentModel.ISupportInitialize)(this.picture_box_display)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picture_box_display;
        private System.Windows.Forms.Button button_idx;
        private System.Windows.Forms.Button button_load_run;
        private System.Windows.Forms.Button button_learn;
        private System.Windows.Forms.Label label_learning_itterations;
        private System.Windows.Forms.Label label_average;
        private System.Windows.Forms.Button button_compute_accuracy;
        private System.Windows.Forms.Timer timer_update;
        private System.Windows.Forms.Label label_run_info;
        private System.Windows.Forms.Label label_accuracy;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.Button button_run_image;
        private System.Windows.Forms.Button button_run_real_image;
        private System.Windows.Forms.Button button_negative_learning;
    }
}

