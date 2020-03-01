using System;
using System.IO;
using System.Linq;
using System.Text;
namespace TheBuild
{
    class BuildEventParameters
    {
        private BuildEventParameters() { }

        public BuildEventType Type { get; private set; }
        public string SolutionDirectoryPath { get; private set; }
        public string TargetPath { get; private set; }
        public string TargetDirectoryPath { get; private set; }
        public string ProjectPath { get; private set; }
        public string ProjectDirectoryPath { get; private set; }
        public string VisualStudioDirectoryPath { get; private set; }
        public string SolutionPath { get; private set; }
        public string OutDirectoryRelativePath { get; private set; }
        public string OutDirectory => Path.Combine(ProjectDirectoryPath ?? "", OutDirectoryRelativePath ?? "");
        public string ConfigurationName { get; private set; }
        public string ProjectName { get; private set; }
        public string TargetName { get; private set; }
        public string ProjectFileName { get; private set; }
        public string TargetExtension { get; private set; }
        public string TargetFileName { get; private set; }
        public string SolutionFileName { get; private set; }
        public string SolutionName { get; private set; }
        public string PlatformName { get; private set; }
        public string ProjectExtension { get; private set; }
        public string SolutionExtension { get; private set; }

        public static readonly (string Parameter, Action<BuildEventParameters, string> Setter)[] ParameterDefinitions =
             new (string, Action<BuildEventParameters, string>)[] {
                ("SolutionDir", (x, s) => x.SolutionDirectoryPath = s),
                ("TargetPath", (x, s) => x.TargetPath = s),
                ("ProjectPath", (x, s) => x.ProjectPath = s),
                ("DevEnvDir", (x, s) => x.VisualStudioDirectoryPath = s),
                ("TargetDir", (x, s) => x.TargetDirectoryPath = s),
                ("ProjectDir", (x, s) => x.ProjectDirectoryPath = s),
                ("SolutionPath", (x, s) => x.SolutionPath=s),
                ("OutDir", (x, s) => x.OutDirectoryRelativePath = s),
                ("ConfigurationName", (x, s) => x.ConfigurationName=s),
                ("ProjectName", (x, s) => x.ProjectName=s),
                ("TargetName", (x, s) => x.TargetName=s),
                ("ProjectFileName", (x, s) => x.ProjectFileName=s),
                ("TargetExt", (x, s) => x.TargetExtension=s),
                ("TargetFileName", (x, s) => x.TargetFileName=s),
                ("SolutionFileName", (x, s) => x.SolutionFileName=s),
                ("SolutionName", (x, s) => x.SolutionName=s),
                ("PlatformName", (x, s) => x.PlatformName=s),
                ("ProjectExt", (x, s) => x.ProjectExtension=s),
                ("SolutionExt", (x, s) => x.SolutionExtension=s),
                ("BuildEventType", (x, s)=>x.Type = Enum.Parse<BuildEventType>(s)),
             };

        public static string BuildExampleCommandLine(BuildEventType type)
        {
            var builder = new StringBuilder();

            //We will use forward slashes for the path here in order support build on Linix (They still work on Windows in this case)
            builder.Append($@"dotnet run -c Run --project ""$(ProjectDir.Replace('\', '/').TrimEnd('/'))/../{ProjectPathRelativeToSolution.Replace('\\', '/').TrimStart('/')}""");

            builder.Append($" ---BuildEventType {type}");

            foreach ((string parameter, _) in BuildEventParameters.ParameterDefinitions)
            {
                if (parameter == "BuildEventType")
                    continue;

                string passedParameter;
                if (parameter.ToLower().Contains("dir") || parameter.ToLower().Contains("path"))
                    passedParameter = $"$({parameter}.TrimEnd('\\'))";
                else passedParameter = $"$({parameter})";

                builder.Append($@" ---{parameter} ""{passedParameter}""");
            }

            string commandLine = builder.ToString().Trim();
            return commandLine;
        }

        static string ProjectPathRelativeToSolution => _projectPathRelativeToSolution?.Value;
        static readonly Lazy<string> _projectPathRelativeToSolution = new Lazy<string>(GetProjectPathRelativeToSolution);
        private static string GetProjectPathRelativeToSolution()
        {
            try
            {
                string fullPath, relativePath;
                DirectoryInfo info = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (!info.GetFiles("*.csproj").Any())
                {
                    info = info.Parent;
                }
                fullPath = info.GetFiles("*.csproj").SingleOrDefault().FullName;
                relativePath = Path.GetRelativePath(info.Parent.FullName, fullPath);
                return relativePath;
            }
            catch { throw; }
        }

        public static BuildEventParameters Current => _current.Value;
        static readonly Lazy<BuildEventParameters> _current = new Lazy<BuildEventParameters>(GetCurrent);
        private static BuildEventParameters GetCurrent()
        {
            string[] args = Environment.GetCommandLineArgs();

            var lookup = ParameterDefinitions.ToDictionary(x => x.Parameter, x => x.Setter);

            var parameters = new BuildEventParameters { };
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (!args[i].StartsWith("---"))
                    continue;

                string name = args[i].Substring(3);
                string value = args[i + 1];

                lookup.TryGetValue(name, out Action<BuildEventParameters, string> setter);
                setter?.Invoke(parameters, value);
            }

            return parameters;
        }
    }
}
