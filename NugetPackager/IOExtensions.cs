using System.IO;
using System;
using System.Linq;

namespace NugetPackager
{
    internal static class IOExtensions
    {
        #region パブリックメソッド

        public static FileInfo GetFile(this DirectoryInfo dir, params string[] file_name)
        {
            if (file_name == null)
                throw new ArgumentNullException();
            if (file_name.Length <= 0)
                throw new ArgumentException();
            return (new FileInfo(Path.Combine(new[] { dir.FullName }.Concat(file_name).ToArray())));
        }

        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, params string[] directory_name)
        {
            if (directory_name == null)
                throw new ArgumentNullException();
            if (directory_name.Length <= 0)
                throw new ArgumentException();
            return (new DirectoryInfo(Path.Combine(new[] { dir.FullName }.Concat(directory_name).ToArray())));
        }

        #endregion
    }
}