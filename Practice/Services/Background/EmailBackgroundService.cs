using Microsoft.Extensions.Hosting;
using Practice.Services.Background;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(IEmailQueue queue, ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var email))
            {
                // simulate email sending
                _logger.LogInformation($"📧 Sending email to: {email}");

                await Task.Delay(2000); // simulate delay

                _logger.LogInformation($"✅ Email sent to: {email}");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}