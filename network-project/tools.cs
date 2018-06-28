using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace network_project {
    class tools {

        public static string read() {
            return Console.ReadLine();
        }

        public static void print(Object o) {
            Console.WriteLine(o.ToString());
        }

        public static void print(Object o, bool line) {
            Console.Write(o.ToString());
        }

        public static void clearLine() {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            print("");
        }
    }
}
