/**
CommandLineParameter.cs

Copyright (c) 2017 Palmtree Software

This software is released under the MIT License.
https://opensource.org/licenses/MIT
*/

using System;
using System.IO;
using System.Linq;

namespace NugetPackager
{
    internal class CommandLineParameter
    {
        #region コンストラクタ

        public CommandLineParameter(string[] args)
        {
            RepositoryDir = null;
            PackageDir = null;
            LicenseUrl = null;
            NugetVerbosity = null;
            var index = 0;
            while (index < args.Length)
            {
                switch (args[index])
                {
                    case "-repository":
                        if (RepositoryDir != null)
                            throw new ArgumentException("-repositoryオプションの指定が重複しています。");
                        ++index;
                        if (index >= args.Length)
                            throw new ArgumentException("-repositoryオプションのパラメタがありません。");
                        try
                        {
                            RepositoryDir = new DirectoryInfo(args[index]);
                            ++index;
                            if (!RepositoryDir.Exists)
                                throw new ArgumentException("-repositoryオプションで与えられたディレクトリがありません。");
                        }
                        catch
                        {
                            throw new ArgumentException("-repositoryオプションで与えられたディレクトリがありません。");
                        }
                        break;
                    case "-package":
                        if (PackageDir != null)
                            throw new ArgumentException("-packageオプションの指定が重複しています。");
                        ++index;
                        if (index >= args.Length)
                            throw new ArgumentException("-packageオプションのパラメタがありません。");
                        try
                        {
                            PackageDir = new DirectoryInfo(args[index]);
                            ++index;
                            if (!PackageDir.Exists)
                                throw new ArgumentException("-packageオプションで与えられたディレクトリがありません。");
                        }
                        catch
                        {
                            throw new ArgumentException("-packageオプションで与えられたディレクトリがありません。");
                        }
                        break;
                    case "-license":
                        if (LicenseUrl != null)
                            throw new ArgumentException("-licenseオプションの指定が重複しています。");
                        ++index;
                        if (index >= args.Length)
                            throw new ArgumentException("-licenseオプションのパラメタがありません。");
                        try
                        {
                            LicenseUrl = new Uri(args[index]);
                            ++index;
                            if (!LicenseUrl.IsAbsoluteUri && LicenseUrl.IsFile || LicenseUrl.IsLoopback || LicenseUrl.IsUnc)
                                throw new ArgumentException("-licenseオプションで与えられたURLが正しくありません。");
                        }
                        catch
                        {
                            throw new ArgumentException("-licenseオプションで与えられたURLが正しくありません。");
                        }
                        break;
                    case "-nugetverbosity":
                        if (NugetVerbosity != null)
                            throw new ArgumentException("-nugetverbosityオプションの指定が重複しています。");
                        ++index;
                        if (index >= args.Length)
                            throw new ArgumentException("-nugetverbosityオプションのパラメタがありません。");
                        NugetVerbosity = args[index];
                        ++index;
                        if (new[] { "normal", "quiet", "detailed" }.All(s => s != NugetVerbosity))
                            throw new ArgumentException("-nugetverbosityオプションの値がnormal/quiet/detailedのいずれでもありません。");
                        break;
                    default:
                        throw new ArgumentException("未知のオプションです。: '" + args[index] + "'");
                }
            }
            if (RepositoryDir == null)
                throw new ArgumentException("-repositoryオプションが指定されていません。");
            if (PackageDir == null)
                throw new ArgumentException("-packageオプションが指定されていません。");
            if (LicenseUrl == null)
                throw new ArgumentException("-licenseオプションが指定されていません。");
            //if (NugetVerbosity == null)
            //    NugetVerbosity = "normal";
        }

        #endregion

        #region パブリックプロパティ

        public DirectoryInfo RepositoryDir { get; private set; }
        public DirectoryInfo PackageDir { get; private set; }
        public Uri LicenseUrl { get; private set; }
        public string NugetVerbosity { get; private set; }

        #endregion
    }
}