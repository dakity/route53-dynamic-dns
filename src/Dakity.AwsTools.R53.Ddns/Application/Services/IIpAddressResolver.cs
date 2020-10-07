using System.Net;
using System.Threading.Tasks;

namespace Dakity.AwsTools.R53.Ddns.Application.Services
{
	public interface IIpAddressResolver
	{
		Task<string> GetExternalIpAddressAsync();
		Task<IPAddress[]> GetMachineIpAddressAsync();
	}
}