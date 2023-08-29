using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiLaser.Core.PermissionsRequest
{
    public class PermissionBaseRequest<T> : Permissions.BasePermission
        where T : Permissions.BasePermission, new()
    {
        public async override Task<PermissionStatus> CheckStatusAsync()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<T>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<T>();
            }
            return status;
        }

        public override void EnsureDeclared() { }

        public async override Task<PermissionStatus> RequestAsync()
        {
            return await Permissions.RequestAsync<T>();
        }

        public override bool ShouldShowRationale()
        {
            return true;
        }
    }
}
