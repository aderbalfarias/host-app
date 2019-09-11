using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceHost2
{
    public class ServiceBaseLifetime : ServiceBase, IHostLifetime
    {
        private readonly ILogger _logger;

        private readonly TaskCompletionSource<object> _delayStart = new TaskCompletionSource<object>();

        public ServiceBaseLifetime(IApplicationLifetime applicationLifetime, ILogger<ServiceBaseLifetime> logger)
        {
            ApplicationLifetime = applicationLifetime 
                ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _logger = logger;
        }

        private IApplicationLifetime ApplicationLifetime { get; }

        public Task WaitForStartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("WaitForStartAsync");

            cancellationToken.Register(() => _delayStart.TrySetCanceled());
            ApplicationLifetime.ApplicationStopping.Register(Stop);

            new Thread(Run).Start(); // Otherwise this would block and prevent IHost.StartAsync from finishing.
            return _delayStart.Task;
        }

        private void Run()
        {
            try
            {
                _logger.LogInformation("Run");

                Run(this); // This blocks until the service is stopped.
                _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
            }
            catch (Exception ex)
            {
                _delayStart.TrySetException(ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        // Called by base.Run when the service is ready to start.
        protected override void OnStart(string[] args)
        {
            _logger.LogInformation("OnStart");

            _delayStart.TrySetResult(null);
            base.OnStart(args);

            _logger.LogInformation("OnStartFinish");

        }

        // Called by base.Stop. This may be called multiple times by service Stop, ApplicationStopping, and StopAsync.
        // That's OK because StopApplication uses a CancellationTokenSource and prevents any recursion.
        protected override void OnStop()
        {
            _logger.LogInformation("Stop");
            ApplicationLifetime.StopApplication();
            base.OnStop();
        }
    }
}
