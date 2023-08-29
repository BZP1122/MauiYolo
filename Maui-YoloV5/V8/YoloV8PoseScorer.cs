#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.V8
 * 唯一标识：97140e8b-67b9-4367-b222-95e4c45608f1
 * 文件名：YoloV8Posecorer
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/7/26 9:10:29
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
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Maui_YoloV5.Models;
using MauiYoloV5;
using MauiYoloV5.Extensions;
using Microsoft.Maui.Graphics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Web;
using Point = System.Drawing.Point;

namespace Maui_YoloV5.V8
{
    /// <summary>
    /// YoloV8Posecorer 的摘要说明
    /// </summary>
    public class YoloV8PoseScorer : YoloScorer<YoloV8PoseModel>
    {
        public YoloV8PoseScorer(
            string weights,
            YoloV8PoseModel yoloModel,
            SessionOptions opts = null
        )
            : base(weights, yoloModel, opts) { }

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
                    //coco 8400
                    for (int i = 0; i < itearNums; i++)
                    {
                        // 【0-3】 xywh  4 confidence  【5-55】
                        float confidence = outputPtr[4 * itearNums + i];
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
                        var label = _model.Labels[0];
                        var prediction = new YoloPrediction(
                            label,
                            confidence,
                            new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                        );
                        prediction.OutMask = new float[51];
                        for (int k = 5; k < _model.Dimensions; k++)
                        {
                            float x = outputPtr[k * itearNums + i];
                            if ((k - 5) % 3 == 0)
                            {
                                x = (x - xPadding) / xGain;
                            }
                            else if (((k - 5) % 3 == 1))
                            {
                                x = (x - yPadding) / yGain;
                            }
                            prediction.OutMask[k - 5] = x;
                        }
                        result.Add(prediction);
                    }
                }
            }

            return result.ToList();
        }

        public override void DrawMat(
            ref Mat src,
            List<YoloPrediction> Predictions,
            int thickness = 5
        )
        {
            foreach (var prediction in Predictions) // draw predictions
            {
                var score = Math.Round(prediction.Score, 2);
                var (x, y) = (prediction.Rectangle.Left - 3, prediction.Rectangle.Top - 23);
                var color = new MCvScalar(
                    new Random().Next(0, 255),
                    new Random().Next(0, 255),
                    new Random().Next(0, 255)
                );
                string label = prediction.Label.Name + score.ToString();
                CvInvoke.Rectangle(src, prediction.Rectangle.RectF2Rect(), color, thickness);
                List<PoseInfo> poses = new List<PoseInfo>();
                for (int i = 0; i < _model.Dimensions - 5; i += 3)
                {
                    Point pt1 = new Point(
                        (int)prediction.OutMask[i],
                        (int)prediction.OutMask[i + 1]
                    );
                    CvInvoke.Circle(src, pt1, 5, color, thickness);
                    poses.Add(
                        new PoseInfo(PoseInfo.PoseNames[i / 3], pt1, prediction.OutMask[i + 2])
                    );
                }
                DrawLines(src, 5, poses);
                var angleLKnee = Angle(poses[13].Pt, poses[11].Pt, poses[15].Pt);
                var angleLhip = Angle(poses[11].Pt, poses[5].Pt, poses[13].Pt);
                var angleRKnee = Angle(poses[14].Pt, poses[12].Pt, poses[16].Pt);
                var angleRhip = Angle(poses[12].Pt, poses[6].Pt, poses[14].Pt);
                var kneeAngle = Math.Max(angleLKnee, angleRKnee);
                var kneeHip = Math.Max(angleLhip, angleRhip);
                bool isStand = kneeAngle > 150d && kneeHip > 150d && poses[13].Confidece < 0.9f;
                if (isStand)
                {
                    OnStandChanged?.Invoke();
                }
                CvInvoke.PutText(
                    src,
                    label + "-" + (isStand ? "stand" : "other"),
                    new Point((int)x, (int)y),
                    FontFace.HersheySimplex,
                    1,
                    isStand ? new MCvScalar(0, 0, 255) : new MCvScalar(0, 255, 0),
                    thickness
                );
            }
        }

        private void DrawLines(Mat src, int thickness, List<PoseInfo> poses)
        {
            CvInvoke.Line(
                src,
                poses.ElementAt(0).Pt,
                poses.ElementAt(1).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(1).Pt,
                poses.ElementAt(3).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(3).Pt,
                poses.ElementAt(5).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(5).Pt,
                poses.ElementAt(7).Pt,
                new MCvScalar(255, 0, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(7).Pt,
                poses.ElementAt(9).Pt,
                new MCvScalar(255, 0, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(5).Pt,
                poses.ElementAt(11).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(11).Pt,
                poses.ElementAt(13).Pt,
                new MCvScalar(0, 255, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(13).Pt,
                poses.ElementAt(15).Pt,
                new MCvScalar(0, 255, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(0).Pt,
                poses.ElementAt(2).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(2).Pt,
                poses.ElementAt(4).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(4).Pt,
                poses.ElementAt(6).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(6).Pt,
                poses.ElementAt(8).Pt,
                new MCvScalar(255, 0, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(8).Pt,
                poses.ElementAt(10).Pt,
                new MCvScalar(255, 0, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(6).Pt,
                poses.ElementAt(12).Pt,
                new MCvScalar(0, 0, 255),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(12).Pt,
                poses.ElementAt(14).Pt,
                new MCvScalar(0, 255, 0),
                thickness
            );
            CvInvoke.Line(
                src,
                poses.ElementAt(14).Pt,
                poses.ElementAt(16).Pt,
                new MCvScalar(0, 255, 0),
                thickness
            );
        }

        private double Angle(Point cen, Point first, Point second)
        {
            const double M_PI = 3.1415926535897;

            double ma_x = first.X - cen.X;
            double ma_y = first.Y - cen.Y;
            double mb_x = second.X - cen.X;
            double mb_y = second.Y - cen.Y;
            double v1 = (ma_x * mb_x) + (ma_y * mb_y);
            double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
            double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
            double cosM = v1 / (ma_val * mb_val);
            double angleAMB = Math.Acos(cosM) * 180 / M_PI;

            return angleAMB;
        }
    }
}
