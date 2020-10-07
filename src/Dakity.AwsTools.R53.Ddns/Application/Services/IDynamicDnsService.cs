using System.Threading;
using System.Threading.Tasks;

namespace Dakity.AwsTools.R53.Ddns.Application.Services
{
	public interface IDynamicDnsService
	{
		Task UpdateDnsAsync(CancellationToken cancellationToken);
	}
}