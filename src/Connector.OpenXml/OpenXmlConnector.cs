using Microsoft.AspNetCore.Mvc;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.OpenXml.OpenXmlConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>))]

namespace Ccs.Connector.OpenXml
{
    public class OpenXmlConnector : AbstractConnector
    {
        public override string Area => "Contest";

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Component(CcsDefaults.ComponentImportExport)
                .HasComponent<Components.JuryOpenXmlScoreboard.JuryOpenXmlScoreboardViewComponent>(100);
        }
    }
}
