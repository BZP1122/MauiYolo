#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Extensions
 * 唯一标识：07159879-11c3-4813-aeb3-e21abc1d6da0
 * 文件名：ServiceProvider
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/6/26 15:51:03
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MauiLaser.Extensions
{
    /// <summary>
    /// ServiceProvider 的摘要说明
    /// </summary>
    public static class ServiceProvider
    {
        public static IServiceProvider Current()
        {
#if WINDOWS10_0_19041_0_OR_GREATER
            return MauiWinUIApplication.Current.Services;
#elif ANDROID
            return MauiApplication.Current.Services;
#elif IOS || MACCATALYST
            return MauiUIApplicationDelegate.Current.Services;
#else
            return null;
#endif
        }

        public static TService GetService<TService>()
        {
            return Current().GetService<TService>();
        }
    }
}
