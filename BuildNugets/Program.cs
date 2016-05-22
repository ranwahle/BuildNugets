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
        private static Dictionary<string, string> _Switches;
        static void Main(string[] args)
        {
            BuildSwitchwes(args);

            BuildNugetPackages();
        }

        private static void BuildSwitchwes(string[] args)
        {
            _Switches = new Dictionary<string, string>(new CasensensitiveStringComparer());
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length)
                {
                    break;
                }

                string switchKey = args[i];
                string switchValue = args[i + 1];

                _Switches[switchKey] = switchValue;
            }
        }

        private static void BuildNugetPackages()
        {
            //string version = _Switches["-version"];
            //string nugetPath = _Switches["-nugetPath"];
            string rootDir = _Switches["-solutionDir"];
            string outputPath = _Switches["-outputPath"];
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
                Configuration = _Switches["-configuration"],
                Version = _Switches["-version"],
                NugetPath = _Switches["-nugetPath"],
                Exclude = _Switches.ContainsKey("-exclude") ? _Switches["-exclude"] : Path.Combine(rootDir, "packages")
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
            string[] nugetSpecs = Directory.GetFiles(directory, "*.nuspec");
            string processStart = Path.Combine(settings.NugetPath, "nuget");

            foreach (var fileName in nugetSpecs)
            {
                string args = string.Format("pack \"{0}\" -Build -Symbols -Version {1} -Properties Configuration={2} -IncludeReferencedProjects", Path.Combine(directory, fileName)
                    , settings.Version, settings.Configuration);


                var startinfo = new ProcessStartInfo
                {
                    Arguments = args,
                    WorkingDirectory = _Switches["-outputPath"],
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
            string[] csProj = Directory.GetFiles(directory, "*.csproj");
            if (csProj.Any())
            {
                return;
            }

            string[] subDirs = Directory.GetDirectories(directory);

            foreach (var dirName in subDirs)
            {
                if (!dirName.Equals(settings.Exclude, StringComparison.InvariantCultureIgnoreCase))
                    RecursivelyBuild(dirName, settings);
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
