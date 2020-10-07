using System.Collections.Generic;

namespace Dakity.AwsTools.R53.Ddns.Application.Dto
{
	/// <summary>
	///     Holds the target records configuration.
	/// </summary>
	public class HostedZonesConfig
	{
		/// <summary>
		///     A list of <see cref="HostedZoneDomain" /> containing the hosted zones domain names and the collection of records in
		///     that zone.
		/// </summary>
		public List<HostedZoneDomain> HostedZoneDomains { get; set; }
	}
}