namespace Network.Graph.Cli
{
    public class MicroGraphHandler
    {
        public const int _xRange = 24; // number of hours in the day
        public const int _yRange = 100; // 100% divided by 10 increments, values are not raw seepd but % of max.
        public const int _yRangeIncrements = 10; // 10 * 10% increments

        public readonly static char[] _indicators = new char[] { ' ', '_', '|', '.', '|', '.' };

        public MicroGraphCellIndications GetCellIndication(int value, int lineValueLow, int lineValueHigh, bool isDownload)
        {



            var midwayRange = lineValueLow + ((int)(lineValueHigh - lineValueLow) / 2);

            MicroGraphCellIndications charIndex;

            // value higher than range -> full char
            // value exceeds half range -> full char
            if (value >= midwayRange)
            {
                charIndex = isDownload ? MicroGraphCellIndications.WholeIncrementDownload : MicroGraphCellIndications.WholeIncrementUpload;
            }
            else if (value > lineValueLow)
            {
                // value exceeds low -> half char
                charIndex = isDownload ? MicroGraphCellIndications.HalfIncrementDownload : MicroGraphCellIndications.HalfIncrementUpload;

            }
            else
            {
                // value too low -> null char or zero if this is the lowest value range and its zero
                charIndex = (lineValueLow == 0 && value == 0) ? MicroGraphCellIndications.Zero : MicroGraphCellIndications.None;
            }


            return charIndex;
        }


        /// <summary>
        /// Cycle through the array elements as a 10 * 24 grid and draw a graph of the values.
        /// </summary>
        /// <param name="startLine">The line on which to start the graph.</param>
        /// <param name="data">The data to graph.</param>
        public List<List<Tuple<bool, char>>> DrawGraphElements(int startLine, Tuple<List<int>, List<int>> data)
        {

            // Tuple<bool, char[]>[]

            var lines = new List<List<Tuple<bool, char>>>();

            

            // this is done in the sequence that might be written to a text stream or be displayed on a screen.
            //0---------------------23                       
            //          ::           : -> first line
            //   .  .  ::::  :    :  :
            //   :  :  ::::  :  :::  :
            //  :::::  ::::  :  :::  :
            //  ::::: :::::  : ::::  :
            // :::::::::::::::::::::::
            // :::::::::::::::::::::::
            // :::::::::::::::::::::::
            // o:::::::::::::::::::::o -> last line

            // TODO Abstract out startline to cope with media type

            // Y starts from 0 to _yRangeIncrements -1 to allow for text writing.
            for (var valueRangeLine = 0; valueRangeLine < _yRangeIncrements; valueRangeLine++)
            {
                List< Tuple<bool, char>> line =   new List<Tuple<bool, char>>();
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

                    MicroGraphCellIndications charIndex = MicroGraphCellIndications.None;
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
                    var cellItem = new Tuple<bool, char> (downLoadHasPriority, _indicators[(int)charIndex]);
                    line.Add(cellItem);

                    //ConsoleColor forecolor = downLoadHasPriority ? ConsoleColor.Green : ConsoleColor.Blue;
                    //Console.ForegroundColor = forecolor;
                    //Console.Write(_indicators[(int)charIndex]);
                }
                lines.Add(line);
            }
            return lines;
        }


        public Tuple<List<int>, List<int>> GetRandomTestData()
        {
            Random rnd = new();
            List<int> downloads = Enumerable.Range(0, _yRange).OrderBy(r => rnd.Next()).Take(_xRange).ToList();
            List<int> uploads = Enumerable.Range(0, _yRange / 2).OrderBy(r => rnd.Next()).Take(_xRange).ToList();
            return new Tuple<List<int>, List<int>>(downloads, uploads);
        }
    }

}