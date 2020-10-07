using System.Collections.Generic;

namespace Dakity.AwsTools.R53.Ddns.Application.Dto
{
	/// <summary>
	///     Holds a hosted zone domain name and the collection of records in that zone.
	/// </summary>
	public class HostedZoneDomain
	{
		/// <summary>
		///     The AWS hosted zone domain name.
		/// </summary>
		public string DomainName { get; set; }

		/// <summary>
		///     The list of 'Type A' records that the dynamic dns process will maintain.
		/// </summary>
		public List<string> RecordNames { get; set; }
	}
}