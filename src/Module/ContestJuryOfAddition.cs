using Ccs.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    public class ContestJuryOfAddition : IAdditionalRole
    {
        private readonly int cid;

        public string Category => "Jury of";

        public string Title { get; }

        public string Text { get; }

        public string GetUrl(object urlHelper)
        {
            var url = urlHelper as IUrlHelper;
            return url.Action("Home", "Jury", new { area = "Contest", cid });
        }

        public ContestJuryOfAddition((int, string) item)
        {
            Title = item.Item2;
            cid = item.Item1;
            Text = "c" + item.Item1;
        }
    }

    public class CcsAdditionProvider : INotificationHandler<UserDetailModel>
    {
        private readonly IContestRepository _store;

        public CcsAdditionProvider(IContestRepository store)
            => _store = store;

        public async Task Handle(UserDetailModel notification, CancellationToken cancellationToken)
        {
            var list = await _store.ListPermissionAsync(notification.User.Id);
            notification.AddMore(list.Select(a => new ContestJuryOfAddition(a)).ToList());
        }
    }
}
