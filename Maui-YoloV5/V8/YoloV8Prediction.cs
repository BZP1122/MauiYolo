#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.V8
 * 唯一标识：f01c53e6-7933-4e4d-b72c-14376e60db68
 * 文件名：YoloV8Prediction
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/29 13:51:11
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

using NcnnDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using Yolov5Net.Scorer;

namespace Maui_YoloV5.V8
{
    /// <summary>
    /// YoloV8Prediction 的摘要说明
    /// </summary>
    public class YoloV8Prediction : YoloPrediction
    {
        public YoloV8Prediction(
            YoloLabel label,
            float score,
            RectangleF rectangle,
            List<float> Masks = null
        )
            : base(label, score, rectangle)
        {
            this.Masks = Masks;
        }

        public List<float> Masks { get; set; }
        public Emgu.CV.Mat SegMask { get; set; }
    }
}
