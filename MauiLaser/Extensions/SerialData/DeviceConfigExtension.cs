#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Extensions.SerialData
 * 唯一标识：b43ad77f-cd6f-4f42-97e3-6a40295dcd30
 * 文件名：DeviceConfigExtension
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/23 11:06:27
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

using MauiLaser.Core.Models.Dtos;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MauiLaser.Extensions.SerialData
{
    /// <summary>
    /// DeviceConfigExtension 的摘要说明
    /// </summary>
    public static class DeviceConfigExtension
    {
        static string DeviceConfigPath = FileSystem.AppDataDirectory + @"/DeviceConfig.json";

        public static void SaveDeviceConfig(this DeviceConfig config)
        {
            if (config != null)
            {
                string serialStr = JsonConvert.SerializeObject(config);
                JsonHelper.WriteJsonFile(DeviceConfigPath, serialStr);
            }
        }

        public static DeviceConfig ReadDeviceConfig(this DeviceConfig config)
        {
            if (File.Exists(DeviceConfigPath))
            {
                var serialStr = JsonHelper.GetJsonFile(DeviceConfigPath);
                if (!string.IsNullOrWhiteSpace(serialStr))
                {
                    return JsonConvert.DeserializeObject<DeviceConfig>(serialStr);
                }
            }

            return null;
        }
    }
}
