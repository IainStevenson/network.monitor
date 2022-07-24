namespace Network.Graph.Cli
{
    internal static class ConsoleHandler
    {

        internal static void ResetLine(ConsoleColor forecolor, int column, int line)
        {
            Console.ForegroundColor = forecolor;
            Console.SetCursorPosition(column, line);

        }

        internal static int WriteTimestamp(int atLine)
        {
            Console.SetCursorPosition(0, atLine);
            Console.Write($"{DateTimeOffset.UtcNow:o}");
            return Console.CursorLeft;
        }

        internal static void PlotData(int startingAtLine, List<List<Tuple<bool, char>>> linesOfPlotpoints)
        {
            foreach (var linePlotPoints in linesOfPlotpoints)
            {

                var columnIndex = 0;
                foreach (var plotPoint in linePlotPoints)
                {
                    Console.SetCursorPosition(columnIndex, startingAtLine);
                    Console.ForegroundColor = plotPoint.Item1 ? ConsoleColor.Green : ConsoleColor.Blue;
                    Console.Write(plotPoint.Item2);
                    columnIndex++;
                }
                startingAtLine++;
            }
        }
    }

}