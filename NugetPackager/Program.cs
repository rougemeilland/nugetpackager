/**
Program.cs

Copyright (c) 2017 Palmtree Software

This software is released under the MIT License.
https://opensource.org/licenses/MIT
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace NugetPackager
{
    class Program
    {
        private static Regex _condition_property_pattern = new Regex("^ ?'\\$\\(Configuration\\)\\|\\$\\(Platform\\)'? == ?'Release\\|AnyCPU' ?$", RegexOptions.Compiled);

        static void Main(string[] args)
        {
            var project_file_etension_pattern = new Regex(@"\.csproj$", RegexOptions.Compiled);
            var package_file_pattern_text = @"^{0}\.[0-9]+\.[0-9]+\.[0-9]+\.nupgk$";
            CommandLineParameter parameter;
            try
            {
                parameter = new CommandLineParameter(args);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            while (true)
            {
                SearchRepositoryDir(parameter.RepositoryDir, (repository_dir, repository_name) =>
                {
                    SearchProjectFile(parameter.RepositoryDir, project_file =>
                    {
                        var nuspec_file = new FileInfo(project_file_etension_pattern.Replace(project_file.FullName, ".nuspec"));
                        if (!nuspec_file.Exists)
                            CreateNuspecFile(nuspec_file, repository_name, parameter.LicenseUrl);
                        try
                        {
                            string assembly_name = GetAssemblyName(project_file);
                            var binary_file = new[] { assembly_name + ".exe", ".dll" }.Select(file_name => project_file.Directory.GetFile("bin", "Release", file_name)).Where(file => file.Exists == true).FirstOrDefault();
                            var package_file_pattern = new Regex(string.Format(package_file_pattern_text, assembly_name), RegexOptions.Compiled);
                            var package_file = parameter.PackageDir.EnumerateFiles("*").Where(file => package_file_pattern.IsMatch(file.Name) == true).OrderByDescending(file => file.LastWriteTimeUtc).FirstOrDefault();
                            if (binary_file != null && binary_file.Exists && (package_file == null || package_file.Exists == false || package_file.LastWriteTimeUtc < binary_file.LastWriteTimeUtc))
                            {
                                if (package_file != null)
                                    package_file.Delete();
                                ExecuteNuget(parameter, project_file);
                            }
                        }
                        catch
                        {
                        }
                    });
                });
                Console.WriteLine("ENTERキーを押すと再実行します。");
                Console.ReadLine();
            }
        }

        private static string GetAssemblyName(FileInfo project_file)
        {
            var doc = new XmlDocument();
            doc.Load(project_file.FullName);
            var node_project = doc.ChildNodes
                               .Cast<XmlNode>()
                               .Where(node => node.Name == "Project")
                               .FirstOrDefault();
            if (node_project == null)
                throw new ApplicationException();
            var project_properties = node_project.ChildNodes
                                     .Cast<XmlNode>()
                                     .Select(node => new { node = node, condition = node.Attributes["Condition"] })
                                     .Where(item => item.node.Name == "PropertyGroup" && (item.condition == null || _condition_property_pattern.IsMatch(item.condition.Value)))
                                     .SelectMany(item => item.node.ChildNodes.Cast<XmlNode>());
            var assembly_name = project_properties
                                      .Where(node => node.Name == "AssemblyName")
                                      .Select(node => node.InnerText)
                                      .FirstOrDefault();
            if (assembly_name == null)
                throw new ApplicationException();
            var output_path = project_properties
                              .Where(node => node.Name == "OutputPath")
                              .Select(node => node.InnerText)
                              .FirstOrDefault();
            return (assembly_name);
        }

        private static void ExecuteNuget(CommandLineParameter parameter, FileInfo project_file)
        {
            var process = new Process();
            process.StartInfo.FileName = "nuget.exe";
            process.StartInfo.Arguments = string.Join(" ", new[]
            {
                "pack",
                project_file.Name,
                "-OutputDirectory", string.Format( "\"{0}\"", parameter.PackageDir.FullName),
                "-Build",
                "-Properties","Configuration=Release",
            }
            .Concat(parameter.NugetVerbosity != null ? new[] { "-Verbosity", parameter.NugetVerbosity } : new string[0])
            .Where(s => s != null));
            //process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = project_file.Directory.FullName;
            if (!process.Start())
                Console.WriteLine("nugetプロセスの起動に失敗しました。");
            else
                process.WaitForExit();
        }

        private static void CreateNuspecFile(FileInfo nuspec_file, string repository_name, Uri license_url)
        {
            using (var resource_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NugetPackager.Resources.DefaultNuspecFile.txt"))
            using (var reader = new StreamReader(resource_stream, Encoding.UTF8))
            using (var file_stream = new FileStream(nuspec_file.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(file_stream, Encoding.UTF8))
            {
                writer.Write(string.Format(reader.ReadToEnd(), license_url.AbsoluteUri, repository_name));
                writer.Flush();
            }
        }

        private static void SearchRepositoryDir(DirectoryInfo dir, Action<DirectoryInfo, string> found)
        {
            foreach (var sub_dir in dir.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                if (sub_dir.GetFile(".gitattributes").Exists && sub_dir.GetFile(".gitignore").Exists)
                    found(sub_dir, sub_dir.Name);
            }
        }

        private static void SearchProjectFile(DirectoryInfo dir, Action<FileInfo> found)
        {
            foreach (var file in dir.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                found(file);
        }
    }
}