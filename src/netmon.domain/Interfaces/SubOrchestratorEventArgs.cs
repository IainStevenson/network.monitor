namespace netmon.domain.Interfaces
{
    public class SubOrchestratorEventArgs: EventArgs
    {
        public Type OrchestratorType {  get;set;}
        public SubOrchestratorEventArgs(Type orchestratorType)
        {
            OrchestratorType = orchestratorType;    
        }
    }
}