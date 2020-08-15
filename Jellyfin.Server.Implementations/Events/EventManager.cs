﻿using System;
using System.Threading.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Server.Implementations.Events
{
    /// <summary>
    /// Handles the firing of events.
    /// </summary>
    public class EventManager : IEventManager
    {
        private readonly IServerApplicationHost _appHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManager"/> class.
        /// </summary>
        /// <param name="appHost">The application host.</param>
        public EventManager(IServerApplicationHost appHost)
        {
            _appHost = appHost;
        }

        /// <inheritdoc />
        public void Publish<T>(T eventArgs)
            where T : EventArgs
        {
            Task.WaitAll(PublishInternal(eventArgs));
        }

        /// <inheritdoc />
        public async Task PublishAsync<T>(T eventArgs)
            where T : EventArgs
        {
            await PublishInternal(eventArgs).ConfigureAwait(false);
        }

        private async Task PublishInternal<T>(T eventArgs)
            where T : EventArgs
        {
            using var scope = _appHost.ServiceProvider.CreateScope();
            foreach (var service in scope.ServiceProvider.GetServices<IEventConsumer<T>>())
            {
                await service.OnEvent(eventArgs).ConfigureAwait(false);
            }
        }
    }
}
