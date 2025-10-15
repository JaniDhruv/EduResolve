using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EduResolve.Data;
using EduResolve.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduResolve.Services
{
    public class EscalationService : BackgroundService
    {
        private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(24);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(IServiceScopeFactory scopeFactory, ILogger<EscalationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEscalationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing complaint escalations.");
                }

                try
                {
                    await Task.Delay(ExecutionInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // swallow cancellation as it indicates shutdown
                }
            }
        }

        private async Task ProcessEscalationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var threshold = DateTime.UtcNow.AddHours(-72);

            var complaintsToEscalate = await context.Complaints
                .Include(c => c.AssignedTo)
                .Where(c => c.Status == ComplaintStatus.New
                    && !c.IsEscalated
                    && c.CreatedAt <= threshold)
                .ToListAsync(cancellationToken);

            if (!complaintsToEscalate.Any())
            {
                _logger.LogInformation("Escalation service run completed. No complaints required escalation.");
                return;
            }

            foreach (var complaint in complaintsToEscalate)
            {
                complaint.IsEscalated = true;
                complaint.EscalatedAt = DateTime.UtcNow;
                complaint.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Escalated {Count} complaints. Notify HOD users with a summary email in production implementation.", complaintsToEscalate.Count);

            // TODO: Inject and use an email notification service to send summaries to HOD stakeholders.
        }
    }
}
