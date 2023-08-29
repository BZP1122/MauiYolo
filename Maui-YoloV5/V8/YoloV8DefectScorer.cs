#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5
 * 唯一标识：80160f9e-f302-42b7-93ff-8a0e6776a4a8
 * 文件名：YoloV8Scorer
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/23 15:09:16
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
using Emgu.CV.Flann;
using Maui_YoloV5.Models;
using Maui_YoloV5.Models.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MauiYoloV5.V8
{
    /// <summary>
    /// YoloV8Scorer 的摘要说明
    /// </summary>
    public class YoloV8DefectScorer : YoloScorer<YoloV8DefectModel>
    {
        public YoloV8DefectScorer(
            string weights,
            YoloV8DefectModel yoloModel,
            SessionOptions opts = null
        )
            : base(weights, yoloModel, opts) { }

        #region <方法>

        protected override List<YoloPrediction> ParseDetect(
            DenseTensor<float> output,
            (int Width, int Height) image
        )
        {
            ConcurrentBag<YoloPrediction> result = new ConcurrentBag<YoloPrediction>();

            var (xGain, yGain) = (
                _model.Width / (float)image.Width,
                _model.Height / (float)image.Height
            ); // x, y gains

            var (xPadding, yPadding) = (
                (_model.Width - (image.Width * xGain)) / 2,
                (_model.Height - (image.Height * yGain)) / 2
            ); // left, right pads
            Span<float> outputData = output.Buffer.Span;
            unsafe
            {
                fixed (float* outputPtr = outputData)
                {
                    int itearNums = outputData.Length / _model.Dimensions;
                    //遍历每一个结果
                    for (int i = 0; i < itearNums; i++)
                    {
                        // 遍历每一个类别
                        for (int k = 4; k < _model.Dimensions; k++)
                        {
                            float confidence = outputPtr[k * itearNums + i];
                            if (confidence < _model.Confidence)
                                continue;

                            float cx = outputPtr[i];
                            float cy = outputPtr[itearNums + i];
                            float cw = outputPtr[2 * itearNums + i] / 2;
                            float ch = outputPtr[3 * itearNums + i] / 2;
                            var xMin = (cx - cw - xPadding) / xGain; // unpad bbox tlx to original
                            var yMin = (cy - ch - yPadding) / yGain; // unpad bbox tly to original
                            var xMax = (cx + cw - xPadding) / xGain; // unpad bbox brx to original
                            var yMax = (cy + ch - yPadding) / yGain; // unpad bbox bry to original
                            xMin = Clamp(xMin, 0, image.Width - 0); // clip bbox tlx to boundaries
                            yMin = Clamp(yMin, 0, image.Height - 0); // clip bbox tly to boundaries
                            xMax = Clamp(xMax, 0, image.Width - 1); // clip bbox brx to boundaries
                            yMax = Clamp(yMax, 0, image.Height - 1); // clip bbox bry to boundaries
                            var label = _model.Labels[k - 4];
                            var prediction = new YoloPrediction(
                                label,
                                confidence,
                                new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                            );

                            result.Add(prediction);
                        }
                    }
                }
            }

            return result.ToList();
        }

        #endregion <方法>
    }
}
