using netmon.core.Models;

namespace netmon.core.Handlers
{
    public interface IPingHandler
    {
        Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken);
    }
}