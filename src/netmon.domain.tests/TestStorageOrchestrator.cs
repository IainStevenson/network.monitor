using netmon.domain.Interfaces;
using netmon.domain.Models;

namespace netmon.domain.tests
{
    /// <summary>
    ///  Implements a test specific null orchestrator pattern that eliminates muching about with mocking.
    /// </summary>
    public class TestStorageOrchestrator : IStorageOrchestrator<PingResponseModel>
    {

        public int StorageRequestCount {  get;set;}
        public Task StoreAsync(PingResponseModel item)
        {
            this.StorageRequestCount ++;
            return Task.FromResult(0);
        }
    }
}