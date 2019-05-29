// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Tools.Internal;

namespace Microsoft.DotNet.OpenApi.Commands
{
    internal class RemoveCommand : BaseCommand
    {
        private const string CommandName = "remove";

        public RemoveCommand(Application parent) : base(parent, CommandName)
        {
        }

        protected override Task<int> ExecuteCoreAsync()
        {
            var projectFile = ResolveProjectFile(ProjectFileOption);

            var sourceFile = Ensure.NotNullOrEmpty(SourceFileArg.Value, SourceFileArgName);

            if (IsProjectFile(sourceFile))
            {
                RemoveServiceReference(OpenApiProjectReference, projectFile, sourceFile);
            }
            else
            {
                RemoveServiceReference(OpenApiReference, projectFile, sourceFile);

                if (!Path.IsPathRooted(sourceFile))
                {
                    sourceFile = Path.Combine(WorkingDir, sourceFile);
                }
                File.Delete(sourceFile);
            }

            return Task.FromResult(0);
        }

        private void RemoveServiceReference(string tagName, FileInfo projectFile, string sourceFile)
        {
            var project = LoadProject(projectFile);
            var openApiReferenceItems = project.GetItems(tagName);

            foreach (ProjectItem item in openApiReferenceItems)
            {
                var include = item.EvaluatedInclude;
                if (string.Equals(include, sourceFile, StringComparison.Ordinal))
                {
                    project.RemoveItem(item);
                    project.Save();
                    return;
                }
            }

            Out.Write("No openapi reference was found with the given source file");
        }
    }
}
