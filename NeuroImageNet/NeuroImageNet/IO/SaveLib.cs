using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SaveLib
{
    #region FileLoader

    public class FileLoader
    {
        #region Variables

        public int length_bits;
        public string name;
        public TypeTables type_table;
        public string path;
        public bool end = false;

        private FileStream stream;
        private Encoding enc = Encoding.GetEncoding(850);

        #endregion

        #region Main Functions

        public FileLoader(string path)
        {
            this.path = path;

            if (load_file() == false)
                return;

            if (is_stream_end()) return;

            if (read_head() == false)
                return;
        }

        private bool load_file()
        {
            if (!File.Exists(this.path))
                return false;

            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            
            return true;
        }

        private bool read_head()
        {
            if (stream.Length < 8)
                return false;

            string file_idf = enc.GetString(read_stream(3));

            // Check for file identifyer
            if (file_idf != "[@]")
                return false;

            int tt_bits;
            int tt_index;
            int length_bits;
            string name;

            byte[] bit_info = read_stream(3);
            tt_bits = bit_info[0];
            tt_index = bit_info[1];
            length_bits = bit_info[2];

            name = read_stream();

            this.length_bits = length_bits;
            this.name = name;

            // Need's to be adjusted for more TT'S
            if (tt_bits == 4 && tt_index == 0 &&
                (length_bits == 8 || length_bits == 16 ||
                length_bits == 24))
                return true;
            else return false;
        }

        public int Get_TT_Index()
        {
            if (is_stream_end()) return -1;

            int index = stream.ReadByte();
            stream.Position--;

            return index;
        }

        public T[] Get_Var<T>()
        {
            if (is_stream_end()) return null;

            int tt_index = stream.ReadByte();

            // Var is a string, reset stream pos
            // return null
            if (tt_index == 6)
            {
                stream.Position--;
                return null;
            }

            int length = read_length();
            int bits = get_var_bits(tt_index);

            // Read raw_var from stream
            int byte_length = length * (bits / 8);
            byte[] bytes = new byte[byte_length];
            stream.Read(bytes,0,bytes.Length);

            T[] back = new T[length];
            Buffer.BlockCopy(bytes, 0, back, 0, bytes.Length);

            return back;
        }

        public string Get_Var()
        {
            if (is_stream_end()) return null;

            int tt_index = stream.ReadByte();
            if (tt_index != 6) return null;

            return read_stream();
        }

        public void Close()
        {
            if (stream != null)
                stream.Close();
        }

        public void Dispose()
        {
            length_bits = 0;
            name = null;
            path = null;
            end = false;

            Close();
            if (stream != null)
                stream.Dispose();
        }

        #endregion

        #region Helper Functions

        // Read#S a string
        private string read_stream()
        { 
            List<byte> bytes = new List<byte>();
            byte r_buffer;
            while ((r_buffer = (byte)stream.ReadByte()) != 0)
                bytes.Add(r_buffer);

            byte[] back_b = bytes.ToArray();
            string back = enc.GetString(back_b);

            return back;
        }

        // Read's a byte array
        private byte[] read_stream(int length)
        { 
            byte[] r_buffer = new byte[length];
            stream.Read(r_buffer,0,r_buffer.Length);

            return r_buffer;
        }

        // Read's the array length to read
        // From the stream
        private int read_length()
        { 
            byte[] bytes = new byte[length_bits/8];
            int[] length = new int[1];

            stream.Read(bytes,0,bytes.Length);
            Buffer.BlockCopy(bytes,0,length,0,bytes.Length);

            return length[0];
        }

        private int get_var_bits(int tt_index)
        {

            int bits = 0;

            // Calculating the bits
            // 8
            if (tt_index == 1) { bits = 8; }
            else if (tt_index == 8) { bits = 8; }
            else if (tt_index == 12) { bits = 8; }
            //16
            else if (tt_index == 3) { bits = 16; }
            else if (tt_index == 7) { bits = 16; }
            else if (tt_index == 11) { bits = 16; }
            // 32
            else if (tt_index == 0){ bits = 32; }
            else if (tt_index == 4) { bits = 32; }
            else if (tt_index == 9) { bits = 32; }
            // 64
            else if (tt_index == 2) { bits = 64; }
            else if (tt_index == 5) { bits = 64; }
            else if (tt_index == 10) { bits = 64; }

            return bits;
        }

        private bool is_stream_end()
        {
            if (stream.Position >= stream.Length)
            {
                end = true;
                return true;
            }
            else
            {
                end = false;
                return false;
            }
        }

        #endregion
    }

    #endregion

    #region FileSaver

    public class FileSaver
    {
        #region Variables

        public int length_bits = 24;
        public TypeTables type_tables;
        public string name="Default";
        public string path;

        // private int tt_bits;        // is equal to 4
        // private int tt_index;     // For the moment always = b4_i0
        private FileStream stream;
        private Encoding enc = Encoding.GetEncoding(850);

        #endregion

        #region Constructor

        public FileSaver(string path)
        {
            this.path = path;
            open();
            add_head();
        }

        public FileSaver(string path,int length_bits, string name)
        {
            this.path = path;
            this.length_bits = length_bits;
            this.name = name;

            open();
            add_head();
        }

        #endregion

        #region Main Functions

        private bool open()
        {
            // Check for existing file
            // del file 
            if (File.Exists(path))
            {
                File.Delete(path);
                while (File.Exists(path)) Thread.Sleep(15);
            }

            // Create Dir if it doesn't exist
            string dir_path = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir_path))
                Directory.CreateDirectory(dir_path);

            //Open File Stream
            stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

            return true;
        }

        private bool add_head()
        {
            string head_idf = "[@]";

            // Bits=4 Index=0
            byte[] tt_bit_length = new byte[]{ 4, 0 };             // Needs to be adjusted for multiple tt's
            byte length_bits = (byte)this.length_bits;
            string name = this.name+"\0";

            add_stream(head_idf);
            add_stream(tt_bit_length);
            add_stream(length_bits);
            add_stream(name);

            return true;
        }

        public bool Add_String(string str)
        {
            if (str == null) return false;

            str += "\0";

            byte tt_index = 6;      // String index
            byte[] str_b = enc.GetBytes(str);

            stream.WriteByte(tt_index);
            stream.Write(str_b,0,str_b.Length);

            return true;
        }

        public bool Add_Var_Array<T>(T[] vars_t)
        {
            if (vars_t.Length <= 0) return false;

            int length = vars_t.Length;
            int tt_index = -1;
            int bits = 0;

            // Calculating the bits
            // 8
            if (typeof(T) == typeof(byte)) { bits = 8; tt_index = 1; }
            else if (typeof(T) == typeof(sbyte)) { bits = 8; tt_index = 12; }
            else if (typeof(T) == typeof(bool)) { bits = 8; tt_index = 8; }
            //16
            else if (typeof(T) == typeof(short)) { bits = 16; tt_index = 3; }
            else if (typeof(T) == typeof(char)) { bits = 16; tt_index = 7; }
            else if (typeof(T) == typeof(ushort)) { bits = 16; tt_index = 11; }
            // 32
            else if (typeof(T) == typeof(int)) { bits = 32; tt_index = 0; }
            else if (typeof(T) == typeof(float)) { bits = 32; tt_index = 4; }
            else if (typeof(T) == typeof(uint)) { bits = 32; tt_index = 9; }
            // 64
            else if (typeof(T) == typeof(long)) { bits = 64; tt_index = 2; }
            else if (typeof(T) == typeof(double)) { bits = 64; tt_index = 5; }
            else if (typeof(T) == typeof(ulong)) { bits = 64; tt_index = 10; }
            // else
            else return false;

            add_length_type(length,(byte)tt_index);

            byte[] data = new byte[vars_t.Length * (bits / 8)];
            Buffer.BlockCopy(vars_t, 0, data, 0, data.Length);

            stream.Write(data, 0, data.Length);
            

            return true;
        }

        public void Close()
        {
            if (stream != null)
                stream.Close();
        }

        public void Dispose()
        {
            length_bits = 24;
            name = "Default";
            path = null;

            Close();
            if (stream != null)
                stream.Dispose();
        }

        #endregion

        #region Helper Functions

        // Add before new T[] data
        private void add_length_type(int length, byte tt_index)
        {
            byte[] length_b = new byte[(length_bits / 8)];
            Buffer.BlockCopy(new int[] { length }, 0, length_b, 0, length_b.Length);

            // First TT_INDEX
            // Second LENGTH

            stream.WriteByte(tt_index);
            stream.Write(length_b, 0, length_b.Length);

        }

        // Converts a string to it's bytes and write's it on the stream
        private void add_stream(string str)
        {
            byte[] w_buffer = enc.GetBytes(str);
            stream.Write(w_buffer,0,w_buffer.Length);
        }

        private void add_stream(byte[] w_buffer)
        {
            stream.Write(w_buffer, 0, w_buffer.Length);
        }

        private void add_stream(byte w_buffer)
        {
            stream.WriteByte(w_buffer);
        }

        #endregion

    }

    #endregion

    #region BitConvertEx (for decimal's)

    public static class BitConvertEx
    {
        public static byte[] GetBytes(decimal data)
        {
            byte[] back = null;

            Int32[] bits = decimal.GetBits(data);
            List<byte> bytes = new List<byte>();

            foreach (Int32 i in bits)
                bytes.AddRange(BitConverter.GetBytes(i));

            return bytes.ToArray();
        }


        public static decimal To_Decimal(byte[] data)
        {
            if (data.Length != 16)
                return -1;

            Int32[] bits = new Int32[4];

            for (int i = 0; i < 16; i += 4)
                bits[i / 4] = BitConverter.ToInt32(data,i);

            return new decimal(bits);
        }
    }

    #endregion

    public enum TypeTables
    {
        B4_I0
    }
}
