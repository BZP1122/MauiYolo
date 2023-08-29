#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.V8
 * 唯一标识：eb534fb7-15bb-4f9b-86e3-8c1d70416b78
 * 文件名：YoloV8SegMasks
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/30 15:01:21
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

using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maui_YoloV5.V8
{
    /// <summary>
    /// YoloV8SegMasks 的摘要说明
    /// </summary>
    public class YoloV8SegMasks
    {
        public List<Mat> SegMasks { get; set; } = new List<Mat>();

        public List<Mat> BinMasks { get; set; } = new List<Mat>();
    }
}
