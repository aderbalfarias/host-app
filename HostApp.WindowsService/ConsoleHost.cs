    public class ConsoleHost : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHandleResponseService _handleResponseService;

        public ConsoleHost(ILogger<ConsoleHost> logger, IHandleResponseService handleResponseService)
        {
            _logger = logger;
            _handleResponseService = handleResponseService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Core module started");

            try
            {
                _handleResponseService.ReceiveData();
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception at ConsoleHost, Error: {e}");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Core module stopped");

            return Task.CompletedTask;
        }
    }
