using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
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

            var store = notification.Context.RequestServices.GetRequiredService<IProblemStore>();
            var level = await store.CheckPermissionAsync(cid, userid);
            notification.Handled = level.HasValue && level.Value >= Polygon.Entities.AuthorLevel.Writer;
        }
    }
}
