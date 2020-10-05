// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal
{
    internal class HttpConnectionMiddleware<TContext>
    {
        private readonly ServiceContext _serviceContext;
        private readonly IHttpApplication<TContext> _application;
        private readonly HttpProtocols _endpointDefaultProtocols;

        public HttpConnectionMiddleware(ServiceContext serviceContext, IHttpApplication<TContext> application, HttpProtocols protocols)
        {
            _serviceContext = serviceContext;
            _application = application;
            _endpointDefaultProtocols = protocols;
        }

        public Task OnConnectionAsync(ConnectionContext connectionContext)
        {
            var memoryPoolFeature = connectionContext.Features.Get<IMemoryPoolFeature>();

            var httpConnectionContext = new HttpConnectionContext
            {
                ConnectionId = connectionContext.ConnectionId,
                ConnectionContext = connectionContext,
                Protocols = connectionContext.Features.Get<HttpProtocolsFeature>()?.HttpProtocols ?? _endpointDefaultProtocols,
                ServiceContext = _serviceContext,
                ConnectionFeatures = connectionContext.Features,
                MemoryPool = memoryPoolFeature?.MemoryPool ?? System.Buffers.MemoryPool<byte>.Shared,
                Transport = connectionContext.Transport,
                LocalEndPoint = connectionContext.LocalEndPoint as IPEndPoint,
                RemoteEndPoint = connectionContext.RemoteEndPoint as IPEndPoint,
                InitialExecutionContext = ExecutionContext.Capture(),
            };

            var connection = new HttpConnection(httpConnectionContext);

            return connection.ProcessRequestsAsync(_application);
        }
    }
}
