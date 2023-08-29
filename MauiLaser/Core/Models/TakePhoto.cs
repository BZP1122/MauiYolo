#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Core.Models
 * 唯一标识：14af29a5-c738-47d2-9e8a-d9007107ad40
 * 文件名：TakePhoto
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/4/24 13:56:25
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion << 版 本 注 释 >>

using MauiLaser.Core.Interfaces;
using MauiLaser.Core.PermissionsRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MauiLaser.Core.Models
{
    /// <summary>
    /// TakePhoto 的摘要说明
    /// </summary>
    public class TakePhoto : ITakePhoto
    {
        public async Task TakePhotoAsyn()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await Permissions.RequestAsync<CameraPermission>();
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        // save the file into local storage
                        string localFilePath = Path.Combine(
                            FileSystem.CacheDirectory,
                            photo.FileName
                        );

                        using Stream sourceStream = await photo.OpenReadAsync();
                        using FileStream localFileStream = File.OpenWrite(localFilePath);
                        await sourceStream.CopyToAsync(localFileStream);
                    }
                }
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "This Function Only Support Android Platform"
                );
            }
        }

        public async Task PickPhotoAsync()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await Permissions.RequestAsync<PickPhotoPermission>();
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult photo = await MediaPicker.Default.PickPhotoAsync();

                    if (photo != null)
                    {
                        string localFilePath = Path.Combine(
                            FileSystem.CacheDirectory,
                            photo.FileName
                        );
                        using Stream sourceStream = await photo.OpenReadAsync();
                        using FileStream localFileStream = File.OpenWrite(localFilePath);
                        await sourceStream.CopyToAsync(localFileStream);
                    }
                }
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "This Function Only Support Android Platform"
                );
            }
        }
    }
}
