using System;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading;

namespace ZBitmap
{
    /************************************/
    /*           Made by Zachi          */
    /*         ImageHandler Class       */
    /************************************/
    public class ImageH
    {
        public string path;                         // Path of Bmp
        public PBitmap p_bmp;                       // PBitmap for Multithreaded bmp operations
        public Bitmap bmp;                          // Bitmap loaded by Load_Image
        public bool ofd = true;                     // Open FileDialog if path doesn't exist.
        
        public ImageH()
        { }
        
        public ImageH(string path)
        {
            this.path = path;
            Load_Image(path);
        }

        // Load the Bmp from path || 
        // Load from OpenFileDialog
        public void Load_Image(string path)
        {
            this.path = path;

            if (File.Exists(path))
            {
                Load_Bmp();
                return;
            }

            if (this.ofd == true)
            {
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.Filter = "Image files ; Jpg, Jpeg, Png, Bmp|*.jpg;*.jpeg;*.png;*.bmp| All files (*.*)|*.*";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.path = ofd.FileName;
                        Load_Bmp();
                    }

                    ofd.Dispose();
                }
            }
        }

        // Load the Bmp from this.path || 
        // Load from OpenFileDialog
        public void Load_Image()
        {
            if (File.Exists(path))
            {
                Load_Bmp();
                return;
            }

            if (this.ofd == true)
            {
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.Filter = "Image files ; Jpg, Jpeg, Png, Bmp|*.jpg;*.jpeg;*.png;*.bmp| All files (*.*)|*.*";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.path = ofd.FileName;
                        Load_Bmp();
                    }

                    ofd.Dispose();
                }
            }
        }

        // Load the Bitmap Bmp from this.path
        private void Load_Bmp()
        {
            if (this.bmp != null)
                this.bmp.Dispose();
            try
            {
                this.bmp = new Bitmap(this.path);
            }
            catch { }
        }

        // Load the PBmp from Bmp
        public void Load_Parallel()
        {
            if (bmp == null) return;

            Load_PBmp();
        }

        // Load the PBmp from Bmp
        private void Load_PBmp()
        {
            if (p_bmp != null)
                p_bmp.Dispose();
            try
            {
                p_bmp = new PBitmap((Bitmap)this.bmp.Clone());
            }
            catch { }
        }

        // Save Bmp
        public void Save(string path, bool ovr_existing = false)
        {
            System.Drawing.Imaging.ImageFormat img_format = System.Drawing.Imaging.ImageFormat.Png;

            if (File.Exists(path))
                if (!ovr_existing)
                    return;
                else
                {
                    try
                    {
                        File.Delete(path);
                        while (File.Exists(path)) Thread.Sleep(15);
                    }
                    catch { }
                }

            if (bmp == null) return;

            try
            {
                bmp.Save(path, img_format);
            }
            catch { }
        }

        // Save Bmp
        public void Save(string path, System.Drawing.Imaging.ImageFormat img_format, bool ovr_existing = false)
        {
            if (File.Exists(path))
                if (!ovr_existing)
                    return;
                else
                {
                    try
                    {
                        File.Delete(path);
                        while (File.Exists(path)) Thread.Sleep(15);
                    }
                    catch { }
                }

            if (bmp == null) return;

            try
            {
                bmp.Save(path, img_format);
            }
            catch { }
        }

        // Save PBmp to Bmp
        public void Write_Parallel_To_Bmp()
        {
            if (p_bmp == null) return;
            if (bmp != null) bmp.Dispose();

            try
            {
                bmp = p_bmp.Get_Bitmap();
            }
            catch { }
        }

        // Returns the File-Type/Ending from this.path
        public string Get_Image_Type()
        {
            string back = null;

            if (String.IsNullOrEmpty(path)) return back;

            string[] split = path.Split('.');
            back = split[split.Length - 1];

            return back;
        }

        // Clone This
        public ImageH Clone()
        {
            ImageH imgH = new ImageH();
            imgH.ofd = ofd;
            imgH.path = path;
            imgH.bmp = (Bitmap)bmp.Clone();
            imgH.p_bmp = (PBitmap)p_bmp.Clone();

            return imgH;
        }

        public void Dispose()
        {
            if (bmp != null) bmp.Dispose();
            if (p_bmp != null) p_bmp.Dispose();
            ofd = true;
            path = null;
        }
    }
}
