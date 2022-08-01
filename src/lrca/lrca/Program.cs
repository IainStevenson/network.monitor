using Microsoft.AspNetCore.Hosting;

namespace lrca
{
    internal class Program
    {
        public static ManualResetEventSlim Done = new ManualResetEventSlim(false);
        public static void Main(string[] args)
        {
            Task.Factory.StartNew(async () => {
            
                while(true)
                {
                    Console.WriteLine("Running!");
                    await Task.Delay(1000);
                }
            });
            var keyNum = Console.In.Peek();
        }
    }
}