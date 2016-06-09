using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildNugets
{
    class Program
    {
        private static Dictionary<string, List<string>> _Switches;
        static void Main(string[] args)
        {
            BuildSwitchwes(args);

            BuildNugetPackages();
        }

        private static void BuildSwitchwes(string[] args)
        {
            _Switches = new Dictionary<string, List<string>>(new CasensensitiveStringComparer());
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length)
                {
                    break;
                }

                string switchKey = args[i];
                string switchValue = args[i + 1];

                if (!_Switches.ContainsKey(switchKey))
                {
                    _Switches[switchKey] = new List<string>();
                }
                _Switches[switchKey].Add(switchValue);
            }
        }

        private static void BuildNugetPackages()
        {
            //string version = _Switches["-version"];
            //string nugetPath = _Switches["-nugetPath"];
            string rootDir = _Switches["-solutionDir"].First();
            string outputPath = _Switches["-outputPath"].First();
            //string configuration = _Switches["-configuration"];
            //string exclude = _Switches.ContainsKey("-exclude") ? _Switches["-exclude"] : Path.Combine(rootDir, "packages");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!ValidateSwitches())
            {
                Console.Error.WriteLine("Usage : BuildNugets -Outputpath \"path/to/nugetpackages\" -nugetPath \"path/to/nugetExecutables\" -version versionforallnugets -solutionDir \"path/to/solutionDir\" -exclude \"path/to/excludeddirecrtory");

                return;
            }

            RecursivelyBuild(rootDir, new NugetSettings
            {
                Configuration = _Switches["-configuration"].First(),
                Version = _Switches["-version"].First(),
                NugetPath = _Switches["-nugetPath"].First(),
                Exclude = _Switches.ContainsKey("-exclude") && _Switches["-exclude"].Any()
                        ? _Switches["-exclude"] : new List<string> { Path.Combine(rootDir, "packages") }
            });

        }

        private static bool ValidateSwitches()
        {
            return _Switches.ContainsKey("-configuration") &&

                 _Switches.ContainsKey("-version") &&

                 _Switches.ContainsKey("-nugetPath");


        }

        private static void RecursivelyBuild(string directory, NugetSettings settings)
        {
         //   Console.WriteLine("Getting files from directory: {0}", directory);
            string[] projFiles = Directory.GetFiles(directory, "*.csproj");
            string[] nugetSpec = Directory.GetFiles(directory, "*.nuspec");
            string processStart = Path.Combine(settings.NugetPath, "nuget");

            if (nugetSpec.Any())
            {
                foreach (var fileName in projFiles)
                {
                    string args = string.Format("pack \"{0}\" -Build -Symbols -Version {1}  -Properties Configuration={2} ", Path.Combine(directory, fileName)
                        , settings.Version, settings.Configuration, Path.Combine(directory, "bin", settings.Configuration));

                    Logger.WriteLine(args);

                    var startinfo = new ProcessStartInfo
                    {
                        Arguments = args,
                        WorkingDirectory = _Switches["-outputPath"].First(),
                        FileName = processStart,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        RedirectStandardOutput = false,
                        UseShellExecute = false

                    };
                    Console.WriteLine("Packing {0}", fileName);
                    var task = Task.Run(() =>
                    {
                        Process process = new Process();
                        process.StartInfo = startinfo;
                        process.Start();
                        process.Exited += Process_Exited;
                        process.WaitForExit();

                    });

                    task.Wait();





                }
               
            }
             if (projFiles.Any())
            {
                return;
            }

            string[] subDirs = Directory.GetDirectories(directory);

            foreach (var dirName in subDirs)
            {
                if (!settings.Exclude.Any(exclude =>  dirName.ToLowerInvariant().Contains(exclude.ToLowerInvariant()))
)                    RecursivelyBuild(dirName, settings);
            }





        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            var process = sender as Process;
            if (process == null)
            {
                return;
            }
            var stdErr = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(stdErr))
                Console.WriteLine(string.Format("Log:({0}) {1} ", process.ExitCode, stdErr));


        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }


}
