// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.Tools;
using Xunit.Abstractions;

namespace Microsoft.DotNet.OpenApi.Tests
{
    public class OpenApiTestBase : IDisposable
    {
        protected readonly TemporaryDirectory _tempDir;
        protected readonly TextWriter _output = new StringWriter();
        protected readonly TextWriter _error = new StringWriter();
        protected readonly ITestOutputHelper _outputHelper;

        protected const string Content = @"{""x-generator"": ""NSwag""}";
        protected const string FakeSwaggerUrl = "https://contoso.com/swagger.json";

        public OpenApiTestBase(ITestOutputHelper output)
        {
            _tempDir = new TemporaryDirectory();
            _outputHelper = output;
        }

        public TemporaryNSwagProject CreateBasicProject(bool withSwagger)
        {
            var nswagJsonFile = "swagger.json";
            var project = _tempDir
                .WithCSharpProject("testproj")
                .WithTargetFrameworks("netcoreapp3.0");
            var tmp = project.Dir();

            if (withSwagger)
            {
                tmp = tmp.WithContentFile(nswagJsonFile);
            }

            tmp.WithContentFile("Startup.cs")
                .Create();

            return new TemporaryNSwagProject(project, nswagJsonFile);
        }

        internal Application GetApplication()
        {
            return new Application(
                _tempDir.Root,
                DownloadMock, _output, _error);
        }

        private Task<string> DownloadMock(string url)
        {
            return Task.FromResult(Content);
        }

        public void Dispose()
        {
            _tempDir.Dispose();
        }
    }

    public class TemporaryNSwagProject
    {
        public TemporaryNSwagProject(TemporaryCSharpProject project, string jsonFile)
        {
            Project = project;
            NSwagJsonFile = jsonFile;
        }

        public TemporaryCSharpProject Project { get; set; }
        public string NSwagJsonFile { get; set; }
    }
}
