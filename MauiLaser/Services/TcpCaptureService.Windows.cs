#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Services
 * 唯一标识：76baf5c6-8803-4021-9752-90c2317419f8
 * 文件名：RtspCaptureService
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/6/26 15:32:41
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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace MauiLaser.Services
{
    /// <summary>
    /// RtspCaptureService 的摘要说明
    /// </summary>
    public class TcpCaptureService : ITcpCaptureService
    {
        public Stream GetImageSourceFromRtsp()
        {
            return null;
        }
    }
}
