﻿namespace Network.Graph.Cli
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
       private MicroGraphHandler _graphHandler = new MicroGraphHandler();

        public void Execute(string[] args)
        {

            ConsoleColor defaultConsoleForecolor = Console.ForegroundColor;
            Tuple<List<int>, List<int>> data;

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {

                    var currentLine = 1;

                    var returnCursorToColumn = ConsoleHandler.WriteTimestamp(currentLine);

                    currentLine++;

                    data = _graphHandler.GetRandomTestData();

                    _graphHandler.DrawGraphElements(currentLine, data);

                    ConsoleHandler.ResetLine(defaultConsoleForecolor, returnCursorToColumn, currentLine - 1);

                    Thread.Sleep(1000);

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }


        static void Main(string[] args)
        {

            var instance = new Program();
            instance.Execute(args);          

        }
    }
}