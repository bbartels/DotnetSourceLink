using System;
using System.Threading.Tasks;

namespace DotnetSourceLink.Console
{
    class Program
    {
        private static RepositoryManager _manager ;

        public static async Task Main(string[] args)
        {

            System.Console.Write("Enter CoreFX directory: ");
            string input = System.Console.ReadLine();
            string directory = input == string.Empty ? "" : input;

            _manager = new RepositoryManager(Indexing.Repository.CoreFx, directory);
            await _manager.Scan();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.Console.Write("Ready\n");

            do
            {
                System.Console.Write("Enter Id: ");
                input = System.Console.ReadLine();
                try
                {
                    GetType(input);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            } while (input != string.Empty);

            System.Console.ReadLine();
        }

        static void GetType(string type)
        {
            System.Console.WriteLine($"Getting Type: {type}");
            foreach (var location in _manager.Get(type).Item1)
            {
                System.Console.WriteLine("github.com/" + location);
            }
        }
    }
}
