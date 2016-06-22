using ByteSizeLib;
using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.ConsoleTest {
    public class ConsoleProgress : IProgress<IWriteProgress> {

        public void Report(IWriteProgress value) {
            if(value.TotalBytes.HasValue) {
                DrawKnownLengthProgressBar(value.BytesWritten, value.TotalBytes.Value);
            } else {
                DrawUnknownLengthProgress(value.BytesWritten, "bytes");
            }
        }

        public static void DrawUnknownLengthProgress(long progress, string type) {
            Console.CursorLeft = 0;
            Console.Write($"{ByteSize.FromBytes(progress)} {type}");
        }

        public static void DrawKnownLengthProgressBar(long progress, long total) {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++) {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++) {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($"{ByteSize.FromBytes(progress)} of {ByteSize.FromBytes(total)}    "); //blanks at the end remove any excess
        }
    }
}
