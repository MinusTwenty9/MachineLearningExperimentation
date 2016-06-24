using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ZBitmap
{
    /************************************/
    /*           Made by Zachi          */
    /*      ParallelBitmap.PBitmap      */
    /************************************/
    public class PBitmap
    {
        private IntPtr ptr;                 // Memory Pointer to RGB
        private BitmapData bmp_data;          // Bmp data
        private Bitmap bmp;                // Initial Bitmap
        public byte[] rgb;                  // Pixel data (i+0=r; i+1=g; i+2=b;)
        public int depth;                   // 24 Bit (0-8bit r, 8-16bit g, 16-24bit b)

        public PBitmap(Bitmap bitmap)
        {
            this.Dispose();

            this.bmp = (Bitmap)bitmap.Clone();
            this.ptr = new IntPtr();

            Lock_Bitmap();
        }

        // Returns the RGB pixel array a Bitmap
        public Bitmap Get_Bitmap()
        {
            return Unlock_Bitmap();
        }

        // Locks the pixels to the RGB pixel array
        private void Lock_Bitmap()
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int pixel_count = width * height;

            Rectangle rec = new Rectangle(0, 0, width, height);
            depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);

            if ( depth != 24) // && depth != 32 && depth != 8 &&)       // You can modify it to include other information (rgba, ...)
            {
                throw new ArgumentException("Only 24 bpp images are supported.");
            }

            bmp_data = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);

            int step = depth / 8;
            rgb = new byte[pixel_count * step];

            ptr = bmp_data.Scan0;

            Marshal.Copy(ptr, rgb, 0, pixel_count*step);
        }

        // Write RGB pixel array back in bmp
        private Bitmap Unlock_Bitmap()
        {
            Marshal.Copy(rgb, 0, ptr, rgb.Length);
            bmp.UnlockBits(bmp_data);

            return (Bitmap)bmp.Clone();
        }

        public void Dispose()
        {
            if (bmp != null)
                bmp.Dispose();
            if (bmp_data != null)
                bmp_data = null;
            if (rgb != null)
                rgb = null;
        }

        public PBitmap Clone()
        {
            PBitmap p_bmp = new PBitmap((Bitmap)this.bmp.Clone());
            p_bmp.rgb = (byte[])this.rgb.Clone();

            return p_bmp;
        }
    }
}
