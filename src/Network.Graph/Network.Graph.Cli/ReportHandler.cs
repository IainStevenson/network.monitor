using System.Text;

namespace Network.Graph.Cli
{
    public class ReportHandler
    {
        public StringBuilder PlotData(string title, List<List<Tuple<bool, char>>> linesOfPlotpoints)
        {
            var report = new StringBuilder();
            report.AppendLine(title);
            foreach (var linePlotPoints in linesOfPlotpoints)
            {
                foreach (var plotPoint in linePlotPoints)
                {                                       
                    report.Append(plotPoint.Item2);                    
                }
                report.AppendLine();
            }
            return report;
        }
    }
}