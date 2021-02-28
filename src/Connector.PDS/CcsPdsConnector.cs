using Microsoft.AspNetCore.Mvc;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.PlagiarismDetect.CcsPdsConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>))]

namespace Ccs.Connector.PlagiarismDetect
{
    public class CcsPdsConnector : AbstractConnector
    {
        public override string Area => "Contest";
    }
}
