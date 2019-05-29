// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.OpenApi.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.OpenApi.Refresh.Tests
{
    public class OpenApiRefreshTests : OpenApiTestBase
    {
        public OpenApiRefreshTests(ITestOutputHelper output) : base(output){}

        [Fact]
        public async Task OpenApi_Refresh_Basic()
        {
            CreateBasicProject(withSwagger: false);

            var app = GetApplication();
            var run = app.Execute(new[] { "add", FakeSwaggerUrl });

            Assert.True(string.IsNullOrEmpty(_error.ToString()), $"Threw error: {_error.ToString()}");
            Assert.Equal(0, run);

            var expectedJsonPath = Path.Combine(_tempDir.Root, "swagger.v1.json");
            var json = await File.ReadAllTextAsync(expectedJsonPath);
            json += "trash";
            await File.WriteAllTextAsync(expectedJsonPath, json);

            var jsonInfo = new FileInfo(expectedJsonPath);
            var firstWriteTime = jsonInfo.LastWriteTime;

            app = GetApplication();
            run = app.Execute(new[] { "refresh", FakeSwaggerUrl });

            Assert.True(string.IsNullOrEmpty(_error.ToString()), $"Threw error: {_error.ToString()}");
            Assert.Equal(0, run);

            var secondWriteTime = new FileInfo(expectedJsonPath).LastWriteTime;
            Assert.True(firstWriteTime < secondWriteTime, $"File wasn't updated! ${firstWriteTime} ${secondWriteTime}");
        }
    }
}
