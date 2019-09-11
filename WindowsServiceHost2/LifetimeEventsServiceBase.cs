using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceHost2
{
    internal class LifetimeEventsServiceBase : ServiceBase, IHostLifetime
    {
        private readonly ILogger _logger;
        private readonly IApplicationLifetime _appLifetime;

        public LifetimeEventsServiceBase(
            ILogger<LifetimeEventsHostedService> logger,
            IApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            new Thread(Run).Start();

            return Task.CompletedTask;
        }

        private void Run()
        {
            try
            {
                _logger.LogInformation("Run");

                Run(this); // This blocks until the service is stopped.
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            _logger.LogInformation("sto");
            return Task.CompletedTask;
        }

        // Called by base.Run when the service is ready to start.
        protected override void OnStart(string[] args)
        {
            _logger.LogInformation("OnStart");

            base.OnStart(args);

            _logger.LogInformation("OnStartFinish");

        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync");

            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        //public Task StopAsync(CancellationToken cancellationToken)
        //{
        //    return Task.CompletedTask;
        //}

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called 1.");

            string Path = @"C:\Logs\TestApplication.txt";
            if (!File.Exists(Path))
            {
                using (var sw = File.CreateText(Path))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
            }
            else
            {
                using (var sw = File.AppendText(Path))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
            }
            // Perform post-startup activities here
        }

        // Called by base.Stop. This may be called multiple times by service Stop, ApplicationStopping, and StopAsync.
        // That's OK because StopApplication uses a CancellationTokenSource and prevents any recursion.
        protected override void OnStop()
        {
            _logger.LogInformation("Stop");
            _appLifetime.StopApplication();
            base.OnStop();
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called 1.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called 1.");

            // Perform post-stopped activities here
        }
    }
}
