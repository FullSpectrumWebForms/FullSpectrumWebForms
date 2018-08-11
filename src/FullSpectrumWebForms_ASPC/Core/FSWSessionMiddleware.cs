using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace FSW_ASPC.Core
{


    public class FSWSessionCleanerService : BackgroundService
    {
        private readonly ILogger<FSWSessionCleanerService> _logger;
    
        public FSWSessionCleanerService(Microsoft.AspNetCore.SignalR.IHubContext<CommunicationHub> hub, ILogger<FSWSessionCleanerService> logger)
        {
            _logger = logger;
            CommunicationHub.Hub = hub;
        }
    
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"FSWSessionCleanerService is starting.");
    
            stoppingToken.Register(() => _logger.LogDebug($" FSWSessionCleanerService background task is stopping."));
    
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"FSWSessionCleanerService task doing background work.");
    
                FSW.Core.Session.ClearTimedOutSessions();
    
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
    
            _logger.LogDebug($"FSWSessionCleanerService background task is stopping.");
        }
    
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), stoppingToken);
        }
    }
}
