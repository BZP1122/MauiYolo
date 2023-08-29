#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.Models.Interfaces
 * 唯一标识：8977949d-6427-44b5-8f42-e58d5e4d3af4
 * 文件名：IYoloScorer
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/6/2 13:27:08
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
using MauiYoloV5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maui_YoloV5.Models.Interfaces
{
    /// <summary>
    /// IYoloScorer 的摘要说明
    /// </summary>
    public interface IYoloScorer : IDisposable
    {
        public Action OnStandChanged { get; set; }
        List<YoloPrediction> Predict(Mat image);
        void DrawMat(ref Mat src, List<YoloPrediction> Predictions, int thickness = 5);
    }
}
