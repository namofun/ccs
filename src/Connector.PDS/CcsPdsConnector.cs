using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.PlagiarismDetect.CcsPdsConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>),
    typeof(SatelliteSite.PlagModule.PlagModule<>))]

namespace Ccs.Connector.PlagiarismDetect
{
    public class CcsPdsConnector : AbstractConnector
    {
        public override string Area => "Contest";

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(CcsDefaults.JuryMenuAdmin, menu =>
            {
                menu.HasEntry(250)
                    .HasTitle(string.Empty, "Plagiarism Detect")
                    .HasLink("Contest", "JuryPlagiarism", "Index")
                    .RequireThat(c => c.HttpContext.Features.Get<IContestFeature>().Feature != CcsDefaults.KindProblemset);
            });
        }
    }
}
