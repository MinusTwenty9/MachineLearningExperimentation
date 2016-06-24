using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NeuroImageNet
{
    namespace IO
    {
        public static class Std
        {
            public static string OpenFile(string ie_file_name = "")
            {
                string back = null;
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.FileName = ie_file_name;
                    if (ofd.ShowDialog() == DialogResult.OK)
                        back = ofd.FileName;
                    ofd.Dispose();
                }

                return back;
            }

            public static string SaveFile(string ie_file_name = "")
            {
                string back = null;
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.FileName = ie_file_name;
                    if (sfd.ShowDialog() == DialogResult.OK)
                        back = sfd.FileName;
                    sfd.Dispose();
                }

                return back;
            }

            public static string OpenFolder()
            {
                string back = null;

                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                        back = fbd.SelectedPath;
                    fbd.Dispose();
                }

                return back;

            }

            public static int ConsoleInputIndex(string[] strs)
            {
                for (int i = 0; i < strs.Length; i++)
                    Console.WriteLine(strs[i]);
                Console.Write("> ");

                int back = -1;
                int.TryParse(Console.ReadLine(), out back);

                return back;
            }

            public static int ConsoleInputIndex(string str)
            {
                Console.WriteLine(str);
                Console.Write("> ");

                int back = -1;
                int.TryParse(Console.ReadLine(), out back);

                return back;
            }

            public static string ConsoleInputString(string[] strs)
            {
                for (int i = 0; i < strs.Length; i++)
                    Console.WriteLine(strs[i]);
                Console.Write("> ");

                string read = Console.ReadLine();

                return read;
            }

        }
    }
}
