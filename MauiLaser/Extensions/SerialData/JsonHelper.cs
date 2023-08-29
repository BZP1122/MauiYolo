#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Extensions.SerialData
 * 唯一标识：7b6b4851-8e35-447e-986a-68c18681a4ee
 * 文件名：JsonHelper
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/23 11:17:09
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
using System.Text;
using System.Web;

namespace MauiLaser.Extensions.SerialData
{
    /// <summary>
    /// JsonHelper 的摘要说明
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 将序列化的json字符串内容写入Json文件，并且保存
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="jsonConents">Json内容</param>
        public static void WriteJsonFile(string path, string jsonConents)
        {
            using (
                FileStream fs = new FileStream(
                    path,
                    FileMode.OpenOrCreate,
                    System.IO.FileAccess.ReadWrite,
                    FileShare.ReadWrite
                )
            )
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine(jsonConents);
                }
            }
        }

        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns>Json内容</returns>
        public static string GetJsonFile(string filepath)
        {
            string json = string.Empty;
            using (
                FileStream fs = new FileStream(
                    filepath,
                    FileMode.OpenOrCreate,
                    System.IO.FileAccess.ReadWrite,
                    FileShare.ReadWrite
                )
            )
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }
    }
}
