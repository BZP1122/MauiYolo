using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiLaser.Core.Interfaces
{
    public interface ITakePhoto
    {
        /// <summary>
        /// 获取图像
        /// </summary>
        /// <returns></returns>
        Task TakePhotoAsyn();

        /// <summary>
        /// 打开相册
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        Task PickPhotoAsync();
    }
}
