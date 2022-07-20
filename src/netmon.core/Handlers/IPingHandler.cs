using netmon.core.Configuration;
using netmon.core.Models;

namespace netmon.core.Handlers
{
    public interface IPingHandler
    {
        PingHandlerOptions Options {  get;}
        Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken);
    }
}