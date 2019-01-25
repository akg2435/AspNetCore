// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.SignalR.Protocol;

namespace Microsoft.AspNetCore.SignalR.Common.Tests.Internal.Protocol
{
    public class StrawManHubProtocolTests : MessagePackHubProtocolTestsBase
    {
        protected override IHubProtocol HubProtocol { get; } = new StrawManHubProtocol();
    }
}