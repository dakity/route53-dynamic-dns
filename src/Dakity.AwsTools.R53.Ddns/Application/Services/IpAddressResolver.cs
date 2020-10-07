using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dakity.AwsTools.R53.Ddns.Application.Services
{
	public class IpAddressResolver : IIpAddressResolver
	{
		private const string IpAddressProviderEndpoint = "http://checkip.amazonaws.com";
		private readonly HttpClient _client;

		public IpAddressResolver(HttpClient client)
		{
			_client = client;
		}

		public async Task<IPAddress[]> GetMachineIpAddressAsync()
		{
			return await Dns.GetHostAddressesAsync(Dns.GetHostName());
		}

		public async Task<string> GetExternalIpAddressAsync()
		{
			try
			{
				_client.DefaultRequestHeaders.Clear();
				_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var path = new Uri(IpAddressProviderEndpoint);

				var response = await _client.GetAsync(path);
				response.EnsureSuccessStatusCode();
				var responseContent = await response.Content.ReadAsStringAsync();

				if (Regex.IsMatch(responseContent, "^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
				{
					return responseContent.TrimEnd(Environment.NewLine.ToCharArray());
					;
				}

				throw new InvalidIpAddressException();
			}
			catch (Exception ex) when (ex is HttpRequestException)
			{
				// Do some logging
				return "0.0.0.0";
			}
		}
	}

	public class InvalidIpAddressException : Exception
	{
	}
}