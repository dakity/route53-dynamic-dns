using System;
using System.Threading;
using System.Threading.Tasks;
using Dakity.AwsTools.R53.Ddns.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dakity.AwsTools.R53.Ddns
{
	public class Worker : BackgroundService
	{
		private readonly IDynamicDnsService _dnsService;
		private readonly ILogger<Worker> _logger;

		public Worker(ILogger<Worker> logger, IDynamicDnsService dnsService)
		{
			_logger = logger;
			_dnsService = dnsService;
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await _dnsService.UpdateDnsAsync(cancellationToken);
				await Task.Delay(60000, cancellationToken);
			}

			_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
			await Task.Delay(60000, cancellationToken);
		}
	}
}