// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Tools.Internal;

namespace Microsoft.DotNet.OpenApi.Commands
{
    internal class AddFileCommand : BaseCommand
    {
        private const string CommandName = "file";

        public AddFileCommand(AddCommand parent)
            : base(parent, CommandName)
        {
            _classNameOpt = Option("-c|--class-name", "The name of the class to be generated", CommandOptionType.SingleValue);
            _outputFileOpt = Option("-o|--output-file", "The name of the file to output the swagger file to", CommandOptionType.SingleValue);
            _sourceFileArg = Argument(SourceProjectArgName, $"The openapi file to add. This can be a path to a local openapi file, " +
                   "a URI to a remote openapi file or a path to a *.csproj file containing openapi endpoints", multipleValues: true);
        }

        internal readonly CommandOption _classNameOpt;
        internal readonly CommandOption _outputFileOpt;

        internal readonly CommandArgument _sourceFileArg;

        private new AddCommand Parent => (AddCommand)base.Parent;

        protected override async Task<int> ExecuteCoreAsync()
        {
            var className = _classNameOpt.Value();
            var outputFile = _outputFileOpt.HasValue() ? _outputFileOpt.Value() : DefaultSwaggerFile;

            var projectFilePath = ResolveProjectFile(ProjectFileOption);

            Ensure.NotNullOrEmpty(_sourceFileArg.Value, SourceProjectArgName);

            foreach (var sourceFile in _sourceFileArg.Values)
            {
                var codeGenerator = CodeGenerator.NSwagCSharp;
                Parent.EnsurePackagesInProject(projectFilePath, codeGenerator);
                if (IsLocalFile(sourceFile))
                {
                    Parent.AddServiceReference(OpenApiReference, projectFilePath, sourceFile, className);
                }
                else if (IsUrl(sourceFile))
                {
                    var destination = Path.Combine(WorkingDirectory, outputFile);
                    // We have to download the file from that url, save it to a local file, then create a AddServiceLocalReference
                    // Use this task https://github.com/aspnet/AspNetCore/commit/91dcbd44c10af893374cfb36dc7a009caa4818d0#diff-ea7515a116529b85ad5aa8e06e4acc8e
                    await DownloadAndOverwriteAsync(sourceFile, destination, overwrite: false);

                    Parent.AddServiceReference(OpenApiReference, projectFilePath, destination, className, sourceFile);
                }
                else
                {
                    Error.Write($"{SourceProjectArgName} of '{sourceFile}' was not valid. Valid values are: a JSON file, a Project File or a Url");
                    throw new ArgumentException();
                }
            }

            return 0;
        }

        private bool IsLocalFile(string file)
        {
            var fullPath = Path.Join(WorkingDirectory, file);
            return File.Exists(fullPath) && file.EndsWith(".json");
        }

        protected override bool ValidateArguments()
        {
            Ensure.NotNullOrEmpty(_sourceFileArg.Value, SourceProjectArgName);
            return true;
        }
    }
}
