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
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Maui_YoloV5.Models;
using Maui_YoloV5.Models.Interfaces;
using Maui_YoloV5.V8;
using MauiYoloV5;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Mat = Emgu.CV.Mat;

namespace Maui_YoloV5.V8
{
    /// <summary>
    /// YoloV8SegScorer 的摘要说明
    /// </summary>
    public class YoloV8SegScorer : YoloScorer<YoloV8SegModel>
    {
        private YoloV8SegMasks segRets;

        public YoloV8SegScorer(string weights, SessionOptions opts = null)
            : base(weights, opts) { }

        public YoloV8SegScorer(string weights, YoloV8SegModel yoloModel, SessionOptions opts = null)
            : base(weights, yoloModel, opts) { }

        #region <方法>

        public override List<YoloPrediction> Predict(Mat image)
        {
            var tensors = Inference(image.Clone());
            var rets = Suppress(ParseOutput(tensors, (image.Width, image.Height)));
            segRets = PredictSeg(image, tensors, rets);
            _result?.Dispose();
            return rets;
        }

        public override void DrawMat(
            ref Mat src,
            List<YoloPrediction> Predictions,
            int thickness = 5
        )
        {
            var clone = src.Clone();
            base.DrawMat(ref src, Predictions, thickness);
            Mat ret = new Mat(src.Size, DepthType.Cv8U, 3);
            if (segRets != null)
            {
                foreach (var mask in segRets.SegMasks)
                {
                    CvInvoke.AddWeighted(src, 1, mask, 1, 0, src);
                }
                foreach (var item in segRets.BinMasks)
                {
                    CvInvoke.Add(clone, ret, ret, item);
                }
                var maskImgPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "mask.png"
                );
                CvInvoke.Imwrite(maskImgPath, ret);
            }
        }

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
            );
            Span<float> outputData = output.Buffer.Span;
            unsafe
            {
                fixed (float* outputPtr = outputData)
                {
                    int itearNums = outputData.Length / _model.Dimensions;
                    //遍历每一个结果
                    for (int i = 0; i < itearNums - 32; i++)
                    {
                        // 遍历每一个类别
                        for (int k = 4; k < _model.Dimensions - 32; k++)
                        {
                            float[] mask = new float[32];
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
                            int maskStartNum = _model.Dimensions - 32;
                            for (int r = maskStartNum; r < _model.Dimensions; r++)
                            {
                                mask[r - maskStartNum] = output[0, r, i];
                            }
                            var prediction = new YoloPrediction(
                                label,
                                confidence,
                                new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin),
                                mask
                            );

                            result.Add(prediction);
                        }
                    }
                }
            }
            return result.ToList();
        }

        public YoloV8SegMasks PredictSeg(
            Mat image,
            DenseTensor<float>[] tensors,
            List<YoloPrediction> rets
        )
        {
            float[] factors = new float[4];
            factors[0] = (float)image.Cols / _model.Width;
            factors[1] = (float)image.Rows / _model.Width;
            factors[2] = image.Rows;
            factors[3] = image.Cols;
            YoloV8SegMasks segMats = ParseSegOutPut(tensors, rets, factors);
            return segMats;
        }

        /// <summary>
        /// 实例分割输出解析
        /// </summary>
        /// <param name="output">网络输出</param>
        /// <param name="nmsedItems">经过NMS的预测结果</param>
        /// <param name="scales">缩放比例</param>
        /// <returns></returns>
        private YoloV8SegMasks ParseSegOutPut(
            DenseTensor<float>[] output,
            List<YoloPrediction> nmsedItems,
            float[] scales
        )
        {
            if (output.Length != 2)
                return null;
            YoloV8SegMasks yoloV8SegMasks = new YoloV8SegMasks();
            DenseTensor<float> segInference = output[1];
            List<Mat> masks = new List<Mat>();
            var arrayNp = np.array(segInference.ToArray(), np.float32).reshape(32, 160 * 160);
            NDArray arrayOutput;
            Random rd = new Random(); // 产生随机数
            nmsedItems.ForEach(ret =>
            {
                arrayOutput = (np.array(ret.OutMask, typeof(float))).reshape(1, 32);
                NDArray multMask = np.matmul(arrayOutput, arrayNp);
                int box_x1 = (int)ret.Rectangle.Left;
                int box_y1 = (int)ret.Rectangle.Top;
                int box_x2 = (int)ret.Rectangle.Right;
                int box_y2 = (int)ret.Rectangle.Bottom;
                Mat rgb_mask = Mat.Zeros((int)scales[2], (int)scales[3], DepthType.Cv8U, 3);
                int mx1 = Math.Max(0, (int)((box_x1 / scales[0]) * 0.25));
                int mx2 = Math.Max(0, (int)((box_x2 / scales[0]) * 0.25));
                int my1 = Math.Max(0, (int)((box_y1 / scales[1]) * 0.25));
                int my2 = Math.Max(0, (int)((box_y2 / scales[1]) * 0.25));
                Mat src = ArrayToMat(multMask.ToArray<float>(), new int[] { 160, 160 });
                Mat mask_roi = new Mat(
                    src,
                    new Emgu.CV.Structure.Range(my1, my2),
                    new Emgu.CV.Structure.Range(mx1, mx2)
                );
                Mat actual_maskm = new Mat();
                CvInvoke.Resize(
                    mask_roi,
                    actual_maskm,
                    new System.Drawing.Size(box_x2 - box_x1, box_y2 - box_y1)
                );
                unsafe
                {
                    ret.Area = 0;
                    float* head = (float*)actual_maskm.GetDataPointer();
                    for (int i = 0; i < actual_maskm.Rows * actual_maskm.Cols; i++)
                    {
                        var value = (*head);
                        if (value > (_model as YoloV8SegModel).SegConfidence)
                        {
                            ret.Area++;
                            *head = 1.0f;
                        }
                        else
                        {
                            *head = 0f;
                        }
                        head++;
                    }
                }
                Mat bin_mask = new Mat();
                actual_maskm = actual_maskm * 200;
                actual_maskm.ConvertTo(bin_mask, DepthType.Cv8U);
                if ((box_y1 + bin_mask.Rows) >= scales[2])
                {
                    box_y2 = (int)scales[2] - 1;
                }
                if ((box_x1 + bin_mask.Cols) >= scales[3])
                {
                    box_x2 = (int)scales[3] - 1;
                }
                Mat mask = Mat.Zeros((int)scales[2], (int)scales[3], DepthType.Cv8U, 1);
                bin_mask = new Mat(
                    bin_mask,
                    new Emgu.CV.Structure.Range(0, box_y2 - box_y1),
                    new Emgu.CV.Structure.Range(0, box_x2 - box_x1)
                );
                Rectangle roi = new Rectangle(box_x1, box_y1, box_x2 - box_x1, box_y2 - box_y1);
                bin_mask.CopyTo(new Mat(mask, roi));
                CvInvoke.Add(
                    rgb_mask,
                    new ScalarArray(
                        new MCvScalar(rd.Next(0, 255), rd.Next(0, 255), rd.Next(0, 255))
                    ),
                    rgb_mask,
                    mask
                );
                masks.Add(rgb_mask);
                yoloV8SegMasks.BinMasks.Add(mask);
            });
            yoloV8SegMasks.SegMasks = masks;
            return yoloV8SegMasks;
        }

        private Mat ArrayToMat(float[] array, int[] arrSize)
        {
            Mat src = new Mat(arrSize[0], arrSize[1], DepthType.Cv32F, 1);
            src.SetTo(array);
            return src;
        }

        private Mat ArrayToMat(Matrix<float> array, int[] arrSize)
        {
            Mat src = new Mat(arrSize[0], arrSize[1], DepthType.Cv32F, 1);
            src.SetTo(array);
            return src;
        }

        float[,] Row2VecD(float[] src)
        {
            float[,] dst = new float[src.Length, 1];
            Buffer.BlockCopy(src, 0, dst, 0, sizeof(float) * src.Length);
            return dst;
        }
        #endregion <方法>
    }
}
