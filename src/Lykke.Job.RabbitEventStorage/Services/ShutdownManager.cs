﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Sdk;

namespace Lykke.Job.RabbitEventStorage.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    public class ShutdownManager : IShutdownManager
    {
        private readonly ILog _log;
        private readonly IEnumerable<IStopable> _items;
        //private readonly TriggerHost _triggerHost;

        public ShutdownManager(
            ILogFactory logFactory, 
            //TriggerHost triggerHost,
            IEnumerable<IStopable> items)
        {
            _log = logFactory.CreateLog(this);
            _items = items;
            //_triggerHost = triggerHost;
        }

        public async Task StopAsync()
        {
            // TODO: Implement your shutdown logic here. Good idea is to log every step
            foreach (var item in _items)
            {
                try
                {
                    item.Stop();
                }
                catch (Exception ex)
                {
                    _log.Warning($"Unable to stop {item.GetType().Name}", ex);
                }
            }
            
            //_triggerHost.Cancel();
            await Task.CompletedTask;
        }
    }
}
