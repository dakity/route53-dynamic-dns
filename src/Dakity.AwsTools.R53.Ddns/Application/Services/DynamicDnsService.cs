using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Route53;
using Amazon.Route53.Model;
using Dakity.AwsTools.R53.Ddns.Application.Dto;
using Microsoft.Extensions.Options;

namespace Dakity.AwsTools.R53.Ddns.Application.Services
{
	public class DynamicDnsService : IDynamicDnsService
	{
		private readonly IAmazonRoute53 _client;
		private readonly List<HostedZoneDomain> _hostedZoneDomains;
		private readonly IIpAddressResolver _ipAddressResolver;

		public DynamicDnsService(
			IOptions<HostedZonesConfig> config,
			IAmazonRoute53 client,
			IIpAddressResolver ipAddressResolver)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_ipAddressResolver = ipAddressResolver ?? throw new ArgumentNullException(nameof(ipAddressResolver));
			_hostedZoneDomains = config.Value.HostedZoneDomains;
		}

		public async Task UpdateDnsAsync(CancellationToken cancellationToken)
		{
			try
			{
				var externalIpAddress = await _ipAddressResolver.GetExternalIpAddressAsync();

				var hostedZonesResponse = await _client.ListHostedZonesAsync(cancellationToken);

				if (!hostedZonesResponse.HostedZones.Any()) return;

				foreach (var hostedZone in hostedZonesResponse.HostedZones.Where(x => _hostedZoneDomains.Any(d => d.DomainName.Contains(x.Name))).ToList())
				{
					var targetHostedZone = _hostedZoneDomains.FirstOrDefault(d => hostedZone.Name.Contains(d.DomainName));

					if (targetHostedZone == null) continue;

					var listResourceRecordSetsResponse = await _client.ListResourceRecordSetsAsync(new ListResourceRecordSetsRequest(hostedZone.Id), cancellationToken);

					if (listResourceRecordSetsResponse.HttpStatusCode != HttpStatusCode.OK) throw new ApplicationException($"Unable to retrieve the list of record sets for {targetHostedZone.DomainName}");

					var changeBatch = new ChangeBatch();
					foreach (var domainName in targetHostedZone.RecordNames)
					{
						var domainRecordSet = listResourceRecordSetsResponse.ResourceRecordSets.Single(x => x.Name == domainName && x.Type == "A");

						if (domainRecordSet.ResourceRecords.Any(x => x.Value.Contains(externalIpAddress))) continue;

						domainRecordSet.ResourceRecords = new List<ResourceRecord>
						{
							new ResourceRecord {Value = externalIpAddress}
						};

						changeBatch.Changes.Add(new Change
						{
							Action = ChangeAction.UPSERT,
							ResourceRecordSet = domainRecordSet
						});
					}

					if (!changeBatch.Changes.Any()) continue;

					var recordSetsResponse = await _client.ChangeResourceRecordSetsAsync(new ChangeResourceRecordSetsRequest(hostedZone.Id, changeBatch), cancellationToken);

					// Monitor the change status
					var changeRequest = new GetChangeRequest
					{
						Id = recordSetsResponse.ChangeInfo.Id
					};

					ChangeStatus status;
					do
					{
						var change = await _client.GetChangeAsync(changeRequest, cancellationToken);
						status = change.ChangeInfo.Status;

						Console.WriteLine("Change is pending.");
						Thread.Sleep(15000);
					} while (status == ChangeStatus.PENDING);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}
	}
}