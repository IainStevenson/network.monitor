namespace Network.Graph.Cli
{
    /// <summary>
    /// Display a simulation of % network speeds over a day as a line graph.
    /// Display upload and download values as a vertical bar each with a separate colour and or char set.
    /// Display both values on the line. The lower of the two values for a cell dictates the char /color used.
    /// If any value is 0 then it is shown as a * in its colour
    /// Download speed is shown if values are in same range and are equal.
    /// </summary>
    internal class Program
    {
        private enum Indications
        {
            None,
            Zero,
            WholeIncrementDownload,
            HalfIncrementDownload,
            WholeIncrementUpload,
            HalfIncrementUpload,
        }
        private const int _xRange = 24; // number of hours in the day
        private const int _yRange = 100; // 100% divided by 10 increments, values are not raw seepd but % of max.
        private const int _yRangeIncrements = 10; // 10 * 10% increments

        private readonly static char[] _indicators = new char[] { ' ', '_', '|', '.', '|', '.' };       


        static void Main(string[] args)
        {
            ConsoleColor defaultConsoleForecolor = Console.ForegroundColor;
            Tuple<List<int>, List<int>> data;

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {

                    var currentLine = 1;

                    var returnCursorToColumn = WriteTimestamp(currentLine);

                    currentLine++;

                    data = GetRandomData();

                    DrawGraphElements(currentLine, data);

                    ResetLine(defaultConsoleForecolor, returnCursorToColumn, currentLine - 1);

                    Thread.Sleep(1000);

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }

        private static void ResetLine(ConsoleColor forecolor, int column, int line)
        {
            Console.ForegroundColor = forecolor;
            Console.SetCursorPosition(column, line);

        }


        /// <summary>
        /// Cycle through the array elements as a 10 * 24 grid and draw a graph of the values.
        /// </summary>
        /// <param name="startLine">The line on which to start the graph.</param>
        /// <param name="data">The data to graph.</param>
        private static void DrawGraphElements(int startLine, Tuple<List<int>, List<int>> data)
        {
            // this is done in the sequence that might be written to a text stream or be displayed on a screen.
            // 0 -> x+                 0
            //                         |
            //          ::           : V
            //   .  .  ::::  :    :  : y+
            //   :  :  ::::  :  :::  :
            //  :::::  ::::  :  :::  :
            //  ::::: :::::  : ::::  :
            // :::::::::::::::::::::::
            // :::::::::::::::::::::::
            // :::::::::::::::::::::::
            // o:::::::::::::::::::::o

            // TODO Abstract out startline to cope with media type

            // Y starts from 0 to _yRangeIncrements -1 to allow for text writing.
            for (var valueRangeLine = 0; valueRangeLine < _yRangeIncrements; valueRangeLine++)
            {
                // X starts from left to allow for text writing.
                for (var hourAsColumnValue = 0; hourAsColumnValue < _xRange; hourAsColumnValue++)
                {
                    var downValue = data.Item1[hourAsColumnValue];
                    var upValue = data.Item2[hourAsColumnValue];

                    // TO DO: abstract this out to cope with media type.
                    Console.SetCursorPosition(hourAsColumnValue, valueRangeLine + startLine);
                  
                    var lineValueHigh = (_yRange - _yRangeIncrements) - (valueRangeLine * _yRangeIncrements) + _yRangeIncrements; // 0 -> 100, 1 -> 90 ...
                    var lineValueLow = lineValueHigh - _yRangeIncrements; // 0 -> 90, 1 -> 80 ...


                    var downloadIsInThisRange = downValue >= lineValueHigh || downValue >= lineValueLow;

                    var uploadIsInThisRange = upValue >= lineValueHigh || upValue >= lineValueLow;

                    Indications charIndex = Indications.None;
                    bool downLoadHasPriority = false;

                    // need to get the char tp print and the colour
                    if (downloadIsInThisRange && uploadIsInThisRange)
                    {
                        if (downValue <= upValue)
                        {
                            downLoadHasPriority = true;
                            charIndex = GetCellIndication(downValue, lineValueLow, lineValueHigh, downLoadHasPriority);
                        }
                        else
                        {
                            charIndex = GetCellIndication(upValue, lineValueLow, lineValueHigh, downLoadHasPriority);
                        }
                    }
                    else
                    {
                        if (downloadIsInThisRange)
                        {
                            downLoadHasPriority = true;
                            charIndex = GetCellIndication(downValue, lineValueLow, lineValueHigh, downLoadHasPriority);
                        }
                        else if (uploadIsInThisRange)
                        {
                            charIndex = GetCellIndication(upValue, lineValueLow, lineValueHigh, downLoadHasPriority);
                        }
                    }

                    ConsoleColor forecolor = downLoadHasPriority ? ConsoleColor.Green : ConsoleColor.Blue;
                    Console.ForegroundColor = forecolor;
                    Console.Write(_indicators[(int)charIndex]);
                }

            }
        }

        private static Tuple<List<int>, List<int>> GetRandomData()
        {
            Random rnd = new();
            List<int> downloads = Enumerable.Range(0, _yRange).OrderBy(r => rnd.Next()).Take(_xRange).ToList();
            List<int> uploads = Enumerable.Range(0, _yRange / 2).OrderBy(r => rnd.Next()).Take(_xRange).ToList();
            return new Tuple<List<int>, List<int>>(downloads, uploads);
        }

        private static int WriteTimestamp(int atLine)
        {
            Console.SetCursorPosition(0, atLine);
            Console.Write($"{DateTimeOffset.UtcNow:o}");
            return Console.CursorLeft;
        }
        private static Indications GetCellIndication(int value, int lineValueLow, int lineValueHigh, bool isDownload)
        {
           


            var midwayRange = lineValueLow + ((int)(lineValueHigh - lineValueLow) / 2);

            Indications charIndex ;
            
            // value higher than range -> full char
            // value exceeds half range -> full char
            if (value >= midwayRange)
            {
                charIndex = isDownload ? Indications.WholeIncrementDownload: Indications.WholeIncrementUpload; 
            }
            else if (value > lineValueLow)
            {
                // value exceeds low -> half char
                charIndex = isDownload ? Indications.HalfIncrementDownload: Indications.HalfIncrementUpload;

            }
            else
            {
                // value too low -> null char or zero if this is the lowest value range and its zero
                charIndex = (lineValueLow == 0 && value == 0) ? Indications.Zero: Indications.None;
            }

            
            return charIndex;
        }
    }
}