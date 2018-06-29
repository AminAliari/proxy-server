using System;

namespace network_project {
    class tools {

        public static string read() {
            return Console.ReadLine();
        }

        public static char readKey() {
            return Console.ReadKey(true).KeyChar;
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

        public static string getTimestamp(DateTime value) {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
