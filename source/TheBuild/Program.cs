using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TheBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args?.Any() != true)
            {
                Console.WriteLine("Pre-Build:");
                Console.WriteLine(BuildEventParameters.BuildExampleCommandLine(BuildEventType.PreBuild));
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                Console.WriteLine("Post-Build:");
                Console.WriteLine(BuildEventParameters.BuildExampleCommandLine(BuildEventType.PostBuild));
                return;
            }

            Task.Run(RunBuildEventAsync).Wait();
        }

        private static async Task RunBuildEventAsync()
        {
            await Task.Yield();

            string debugFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dbg.txt");
            try
            {
                var parameters = BuildEventParameters.Current;
                File.WriteAllText(debugFile, JsonConvert.SerializeObject(parameters, Formatting.Indented));
            }
            catch (Exception e)
            {
                File.WriteAllText(debugFile, JsonConvert.SerializeObject(e, Formatting.Indented));
                throw;
            }
        }
    }
}
