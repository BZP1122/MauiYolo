#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Core.Models.Dtos
 * 唯一标识：e75a5a41-9341-417c-82a6-a4b7c16aec0f
 * 文件名：YoloCocoP5ModelDto
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/23 10:55:06
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

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MauiLaser.Core.Models.Dtos
{
    /// <summary>
    /// YoloCocoP5ModelDto 的摘要说明
    /// </summary>
    public partial class DeviceConfig : ObservableObject
    {
        [ObservableProperty]
        string onnxFilePath;

        [ObservableProperty]
        string clsFilePath;

        [ObservableProperty]
        float confidence = 0.45f;

        [ObservableProperty]
        string ip = "192.168.0.1";

        [ObservableProperty]
        int port = 80;

        [ObservableProperty]
        string rtspAddress;

        [ObservableProperty]
        string httpAddress;

        [ObservableProperty]
        string selectYoloModel;

        public IEnumerable<string> ReadLablesFromFile()
        {
            if (File.Exists(ClsFilePath))
                return File.ReadLines(ClsFilePath);
            return Enumerable.Empty<string>();
        }
    }
}
