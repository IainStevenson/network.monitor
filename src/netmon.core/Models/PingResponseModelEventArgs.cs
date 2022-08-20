namespace netmon.core.Models
{
    public class PingResponseModelEventArgs : EventArgs
    {
        public PingResponseModel Model { get; set; }
        public PingResponseModelEventArgs(PingResponseModel item)
        {
            this.Model = item;
        }
    }
}