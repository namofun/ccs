#nullable disable
using Microsoft.AspNetCore.Mvc.DataTables;

namespace Xylab.Contesting.Registration.BatchByTeamName
{
    public class TeamAccount
    {
        [DtDisplay(0, "#")]
        public int Id { get; set; }

        [DtDisplay(1, "team")]
        public string TeamName { get; set; }

        [DtDisplay(2, "username")]
        [DtCellCss(Class = "text-monospace")]
        public string UserName { get; set; }

        [DtDisplay(3, "password")]
        [DtCellCss(Class = "text-monospace")]
        public string Password { get; set; }
    }
}
