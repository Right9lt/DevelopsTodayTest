using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    static class InputHelper
    {
        public static int InputInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var str = Console.ReadLine();
                if (int.TryParse(str, out int result))
                {
                    return result;
                }
            }
        }

        public static string InputFile(string prompt, string[] extensions)
        {
            while (true)
            {
                Console.Write(prompt);
                var str = Console.ReadLine();
                var extension = Path.GetExtension(str);
                if (!extensions.Contains(extension))
                {
                    Console.WriteLine("Unsupported file type");
                    continue;
                }
                if (!File.Exists(str))
                {
                    Console.WriteLine("File doesn't exists");
                    continue;
                }

                return str;
            }
        }
    }
}
