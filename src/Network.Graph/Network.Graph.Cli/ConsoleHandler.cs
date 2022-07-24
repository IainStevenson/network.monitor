namespace Network.Graph.Cli
{
    public static class ConsoleHandler
    {

        public static void ResetLine(ConsoleColor forecolor, int column, int line)
        {
            Console.ForegroundColor = forecolor;
            Console.SetCursorPosition(column, line);

        }

        public static int WriteTimestamp(int atLine)
        {
            Console.SetCursorPosition(0, atLine);
            Console.Write($"{DateTimeOffset.UtcNow:o}");
            return Console.CursorLeft;
        }
    }

}