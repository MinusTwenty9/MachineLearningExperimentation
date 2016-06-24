using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeuroImageNet
{
    namespace IO
    {
        public class idxX_ubyte
        {
            public string image_path;
            public string label_path;

            public int image_count;
            public int width;
            public int height;

            private FileStream image_stream;
            private FileStream label_stream;

            private int image_stream_info_offset;
            private int label_stream_info_offset;

            public idxX_ubyte(string image_path, string lable_path)
            {
                this.image_path = image_path;
                this.label_path = lable_path;

                if (load_files() == false)
                    throw (new ArgumentException("idxX load file exception"));
            }

            private bool load_files()
            {
                if (!File.Exists(image_path)) return false;
                if (!File.Exists(label_path)) return false;

                image_stream = new FileStream(image_path, FileMode.Open, FileAccess.Read, FileShare.None);
                label_stream = new FileStream(label_path, FileMode.Open, FileAccess.Read, FileShare.None);

                load_image_info();
                load_label_info();

                return true;
            }

            private void load_image_info()
            {
                image_stream.Position = 0;

                int magic_number = read_int(ref image_stream);
                image_count = read_int(ref image_stream);
                width = read_int(ref image_stream);
                height = read_int(ref image_stream);

                image_stream_info_offset = (int)image_stream.Position;
            }

            private void load_label_info()
            {
                label_stream.Position = 0;

                int magic_number = read_int(ref label_stream);

                label_stream.Position = 8;
                label_stream_info_offset = (int)label_stream.Position;

            }

            public bool Get_Image(int index, ref byte[] pixel_data, ref byte label)
            {
                if (index < 0 || index >= image_count) return false;

                // Image pixel data
                int length = width * height;
                pixel_data = new byte[length];

                image_stream.Position = image_stream_info_offset + (length * index);
                image_stream.Read(pixel_data, 0, length);


                // Label info
                label_stream.Position = label_stream_info_offset + index;
                label = (byte)label_stream.ReadByte();

                return true;
            }

            // Read's a int from the given FileStream
            private int read_int(ref FileStream stream)
            {
                int[] back = new int[1];
                byte[] r_buffer = new byte[4];
                stream.Read(r_buffer, 0, 4);

                r_buffer = r_buffer.Reverse().ToArray();

                Buffer.BlockCopy(r_buffer, 0, back, 0, r_buffer.Length);

                return back[0];
            }

            private void Close()
            {
                if (image_stream != null)
                    image_stream.Close();
                if (label_stream != null)
                    label_stream.Close();
            }

            private void Dispose()
            {
                if (image_stream != null)
                    image_stream.Dispose();
                if (label_stream != null)
                    label_stream.Dispose();
            }
        }
    }
}
