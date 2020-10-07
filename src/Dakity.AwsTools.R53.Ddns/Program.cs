using Amazon.Route53;
using Dakity.AwsTools.R53.Ddns.Application.Dto;
using Dakity.AwsTools.R53.Ddns.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dakity.AwsTools.R53.Ddns
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<HostedZonesConfig>(x => hostContext.Configuration.GetSection("HostedZonesConfig").Bind(x));
					services.TryAddSingleton<IAmazonRoute53, AmazonRoute53Client>();
					services.TryAddSingleton<IDynamicDnsService, DynamicDnsService>();
					services.AddHttpClient<IIpAddressResolver, IpAddressResolver>();
					services.AddHostedService<Worker>();
				});
		}
	}
}