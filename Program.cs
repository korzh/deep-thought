using System;

namespace Korzh.DeepThought
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Using: deepth <command> <KB_ID> <XML file> [other options]");
                return;
            }

            var command = args[0];
            var kbId = args[1];
            var xmlFileName = args[2];

            if (command == "train") {
            }
            else if (command == "test") {

            }

        }
    }
}
