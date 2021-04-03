using Ccs.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Substrate.Dashboards;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    public class ContestJuryUploadPermissionHandler : INotificationHandler<ImageUploadPermission>
    {
        public async Task Handle(ImageUploadPermission notification, CancellationToken cancellationToken)
        {
            if (notification.Type != "c") return;

            if (!(notification.Id is int cid) ||
                !int.TryParse(notification.Context.User.GetUserId(), out int userid))
            {
                notification.Handled = false;
                return;
            }

            if (notification.Context.User.IsInRole("Administrator"))
            {
                notification.Handled = true;
                return;
            }

            notification.Handled = false;
            var store = notification.Context.RequestServices.GetRequiredService<ScopedContestContextFactory>();
            var c = await store.CreateAsync(cid, false);
            if (c != null)
            {
                var jury = await c.ListJuriesAsync();
                if (jury.TryGetValue(userid, out var val))
                {
                    notification.Handled = val.Item2 >= Ccs.Entities.JuryLevel.Jury;
                }
            }
        }
    }
}
