using netmon.core.Models;

namespace netmon.core.Interfaces
{
    public interface IPingResponseModelStorageOrchestrator
    {
        Task Store(PingResponseModel item);
    }
}