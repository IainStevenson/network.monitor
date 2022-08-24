using System.Diagnostics.CodeAnalysis;

namespace netmon.core.Models
{
    [ExcludeFromCodeCoverage]
    public class PingResponseModelEventArgs : EventArgs
    {
        public PingResponseModel Model { get; set; }
        public PingResponseModelEventArgs(PingResponseModel item)
        {
            this.Model = item;
        }
    }
}