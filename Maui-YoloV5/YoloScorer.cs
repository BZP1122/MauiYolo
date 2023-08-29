using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Maui_YoloV5.Models.Abstract;
using Maui_YoloV5.Models.Interfaces;
using MauiYoloV5.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Mat = Emgu.CV.Mat;

namespace MauiYoloV5;

/// <summary>
/// Yolov5 scorer.
/// </summary>
public class YoloScorer<T> : IYoloScorer
    where T : YoloModel
{
    protected readonly T _model;
    protected IDisposableReadOnlyCollection<DisposableNamedOnnxValue> _result;
    protected readonly InferenceSession _inferenceSession;

    public Action OnStandChanged { get; set; }

    /// <summary>
    /// Outputs value between 0 and 1.
    /// </summary>
    protected float Sigmoid(float value)
    {
        return 1 / (1 + (float)Math.Exp(-value));
    }

    /// <summary>
    /// Converts xywh bbox format to xyxy.
    /// </summary>
    protected float[] Xywh2xyxy(float[] source)
    {
        var result = new float[4];

        result[0] = source[0] - (source[2] / 2f);
        result[1] = source[1] - (source[3] / 2f);
        result[2] = source[0] + (source[2] / 2f);
        result[3] = source[1] + (source[3] / 2f);

        return result;
    }

    /// <summary>
    /// Returns value clamped to the inclusive range of min and max.
    /// </summary>
    public float Clamp(float value, float min, float max)
    {
        return (value < min)
            ? min
            : (value > max)
                ? max
                : value;
    }

    /// <summary>
    /// Extracts pixels into tensor for net input.
    /// </summary>
    //private Tensor<float> ExtractPixels(Mat image)
    //{
    //    //var tensor = new DenseTensor<float>(new[] { 1, 3, _model.Height, _model.Width });
    //    int rows = image.Rows;
    //    int cols = image.Cols;
    //    int chs = image.NumberOfChannels;
    //    float[,,,] array = new float[1, 3, _model.Height, _model.Width];
    //    unsafe
    //    {
    //        byte* head = (byte*)image.GetDataPointer();
    //        for (int i = 0; i < rows * cols * chs; i++, head++)
    //        {
    //            array[0, i % chs, (i / chs) / cols, (i / chs) % rows] =
    //                *(head + (i % chs)) / 255.0F;
    //        }
    //    }
    //    return array.ToTensor();
    //}

    protected Tensor<float> ExtractPixels(Mat image)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        int rows = image.Rows;
        int cols = image.Cols;
        int chs = image.NumberOfChannels;
        var tensor = new DenseTensor<float>(new[] { 1, 3, _model.Height, _model.Width });
        //float[,,,] array = new float[1, 3, _model.Height, _model.Width];
        unsafe
        {
            fixed (float* ptr = tensor.Buffer.Span) // 获取array的指针
            {
                byte* head = (byte*)image.GetDataPointer();
                for (int i = 0; i < rows * cols * chs; i++, head++)
                {
                    // 计算当前元素的索引位置
                    int batch = 0;
                    int channel = i % chs;
                    int row = (i / chs) / cols;
                    int col = (i / chs) % cols;

                    float* p =
                        ptr
                        + (
                            batch * 3 * _model.Height * _model.Width
                            + channel * _model.Height * _model.Width
                            + row * _model.Width
                            + col
                        );

                    // 将像素值除以255后赋值给对应位置的元素
                    *p = *(head + channel) / 255.0F;
                }
            }
        }
        sw.Stop();
        Debug.WriteLine("ExtractPixels:" + sw.ElapsedMilliseconds + "ms");
        return tensor;
    }

    /// <summary>
    /// Runs inference session.
    /// </summary>
    protected DenseTensor<float>[] Inference(Mat image)
    {
        if (image.Width != _model.Width || image.Height != _model.Height)
            CvInvoke.Resize(image, image, new System.Drawing.Size(_model.Width, _model.Height));

        var inputs = new List<NamedOnnxValue> // add image as input
        {
            NamedOnnxValue.CreateFromTensor("images", ExtractPixels(image))
        };
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        _result = _inferenceSession.Run(inputs); // run inference
        sw.Stop();
        Debug.WriteLine("Inference:" + sw.ElapsedMilliseconds + "ms");
        var output = new List<DenseTensor<float>>();

        foreach (var item in _model.Outputs) // add outputs for processing
        {
            var tensor = _result.ToList().Find(x => x.Name == item);

            if (tensor != null)
            {
                output.Add(tensor.Value as DenseTensor<float>);
            }
        }
        return output.ToArray();
    }

    /// <summary>
    /// Parses net output (detect) to predictions.
    /// </summary>
    protected virtual List<YoloPrediction> ParseDetect(
        DenseTensor<float> output,
        (int Width, int Height) image
    )
    {
        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (
            _model.Width / (float)image.Width,
            _model.Height / (float)image.Height
        ); // x, y gains

        var (xPadding, yPadding) = (
            (_model.Width - (image.Width * xGain)) / 2,
            (_model.Height - (image.Height * yGain)) / 2
        ); // left, right pads
        for (int i = 0; i < (int)(output.Length / _model.Dimensions); i++)
        {
            var confidence = output[0, i, 4];
            if (confidence <= _model.Confidence)
                continue; // skip low obj_conf results
            for (int k = 5; k < _model.Dimensions; k++)
            {
                output[0, i, k] *= confidence;
                if (output[0, i, k] <= _model.MulConfidence)
                    continue; // skip low mul_conf results

                float cx = output[0, i, 0];
                float cy = output[0, i, 1];
                float cw = output[0, i, 2] / 2;
                float ch = output[0, i, 3] / 2;
                var xMin = (cx - cw - xPadding) / xGain; // unpad bbox tlx to original
                var yMin = (cy - ch - yPadding) / yGain; // unpad bbox tly to original
                var xMax = (cx + cw - xPadding) / xGain; // unpad bbox brx to original
                var yMax = (cy + ch - yPadding) / yGain; // unpad bbox bry to original

                xMin = Clamp(xMin, 0, image.Width - 0); // clip bbox tlx to boundaries
                yMin = Clamp(yMin, 0, image.Height - 0); // clip bbox tly to boundaries
                xMax = Clamp(xMax, 0, image.Width - 1); // clip bbox brx to boundaries
                yMax = Clamp(yMax, 0, image.Height - 1); // clip bbox bry to boundaries

                var label = _model.Labels[k - 5];

                var prediction = new YoloPrediction(
                    label,
                    output[0, i, k],
                    new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                );

                result.Add(prediction);
            }
        }
        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid) to predictions.
    /// </summary>
    protected virtual List<YoloPrediction> ParseSigmoid(
        DenseTensor<float>[] output,
        (int Width, int Height) image
    )
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (
            _model.Width / (float)image.Width,
            _model.Height / (float)image.Height
        ); // x, y gains

        var (xPadding, yPadding) = (
            (_model.Width - (image.Width * xGain)) / 2,
            (_model.Height - (image.Height * yGain)) / 2
        ); // left, right pads
        for (int i = 0; i < output.Length; i++)
        {
            var shapes = _model.Shapes[i];
            for (int a = 0; a < _model.Anchors[0].Length; a++)

                for (int y = 0; y < shapes; y++)
                {
                    for (int x = 0; x < shapes; x++)
                    {
                        {
                            var offset =
                                ((shapes * shapes * a) + (shapes * y) + x) * _model.Dimensions;

                            var buffer = output[i]
                                .Skip(offset)
                                .Take(_model.Dimensions)
                                .Select(Sigmoid)
                                .ToArray();

                            if (buffer[4] <= _model.Confidence)
                                continue; // skip low obj_conf results

                            var scores = buffer.Skip(5).Select(b => b * buffer[4]).ToList(); // mul_conf = obj_conf * cls_conf

                            var mulConfidence = scores.Max(); // max confidence score

                            if (mulConfidence <= _model.MulConfidence)
                                continue; // skip low mul_conf results

                            var rawX = ((buffer[0] * 2) - 0.5f + x) * _model.Strides[i]; // predicted bbox x (center)
                            var rawY = ((buffer[1] * 2) - 0.5f + y) * _model.Strides[i]; // predicted bbox y (center)

                            var rawW = (float)Math.Pow(buffer[2] * 2, 2) * _model.Anchors[i][a][0]; // predicted bbox w
                            var rawH = (float)Math.Pow(buffer[3] * 2, 2) * _model.Anchors[i][a][1]; // predicted bbox h

                            var xyxy = Xywh2xyxy(new[] { rawX, rawY, rawW, rawH });

                            var xMin = Clamp((xyxy[0] - xPadding) / xGain, 0, image.Width - 0); // unpad, clip tlx
                            var yMin = Clamp((xyxy[1] - yPadding) / yGain, 0, image.Height - 0); // unpad, clip tly
                            var xMax = Clamp((xyxy[2] - xPadding) / xGain, 0, image.Width - 1); // unpad, clip brx
                            var yMax = Clamp((xyxy[3] - yPadding) / yGain, 0, image.Height - 1); // unpad, clip bry

                            var label = _model.Labels[scores.IndexOf(mulConfidence)];

                            var prediction = new YoloPrediction(
                                label,
                                mulConfidence,
                                new(xMin, yMin, xMax - xMin, yMax - yMin)
                            );

                            result.Add(prediction);
                        }
                    }
                }
            { }
        }

        sw.Stop();
        Debug.WriteLine("ParseSigmoid:" + sw.ElapsedMilliseconds + "ms");
        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid or detect layer) to predictions.
    /// </summary>
    protected List<YoloPrediction> ParseOutput(
        DenseTensor<float>[] output,
        (int Width, int Height) image
    )
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        var result = _model.UseDetect ? ParseDetect(output[0], image) : ParseSigmoid(output, image);
        sw.Stop();
        Debug.WriteLine("ParseOutput:" + sw.ElapsedMilliseconds + "ms");
        return result;
    }

    /// <summary>
    /// Removes overlapped duplicates (nms).
    /// </summary>
    protected List<YoloPrediction> Suppress(List<YoloPrediction> items)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var result = new List<YoloPrediction>(items);
        foreach (var item in items) // iterate every prediction
        {
            foreach (var current in result.ToList().Where(current => current != item)) // make a copy for each iteration
            {
                var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                var intersection = RectangleF.Intersect(rect1, rect2);

                var intArea = intersection.Area(); // intersection area
                var unionArea = rect1.Area() + rect2.Area() - intArea; // union area
                var overlap = intArea / unionArea; // overlap ratio

                if (overlap >= _model.Overlap)
                {
                    if (item.Score >= current.Score)
                    {
                        result.Remove(current);
                    }
                }
            }
        }
        sw.Stop();
        Debug.WriteLine(
            "Suppress:" + sw.ElapsedMilliseconds + "ms" + "Count" + items.Count.ToString()
        );
        return result;
    }

    /// <summary>
    /// Runs object detection.
    /// </summary>
    public virtual List<YoloPrediction> Predict(Mat image)
    {
        var tensors = Inference(image.Clone());
        var rets = Suppress(ParseOutput(tensors, (image.Width, image.Height)));
        _result?.Dispose();
        return rets;
    }

    /// <summary>
    /// Creates new instance of YoloScorer.
    /// </summary>
    public YoloScorer()
    {
        _model = Activator.CreateInstance<T>();
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights path and options.
    /// </summary>
    public YoloScorer(string weights, SessionOptions opts = null)
        : this()
    {
        _inferenceSession = new InferenceSession(
            File.ReadAllBytes(weights),
            opts ?? new SessionOptions()
        );
    }

    public YoloScorer(string weights, T yoloModel, SessionOptions opts = null)
    {
        _model = yoloModel;
        _inferenceSession = new InferenceSession(
            File.ReadAllBytes(weights),
            opts ?? new SessionOptions()
        );
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights stream and options.
    /// </summary>
    public YoloScorer(Stream weights, SessionOptions opts = null)
        : this()
    {
        using (var reader = new BinaryReader(weights))
        {
            _inferenceSession = new InferenceSession(
                reader.ReadBytes((int)weights.Length),
                opts ?? new SessionOptions()
            );
        }
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights bytes and options.
    /// </summary>
    public YoloScorer(byte[] weights, SessionOptions opts = null)
    {
        _inferenceSession = new InferenceSession(weights, opts ?? new SessionOptions());
    }

    /// <summary>
    /// Disposes YoloScorer instance.
    /// </summary>
    public void Dispose()
    {
        _inferenceSession.Dispose();
    }

    public virtual void DrawMat(
        ref Emgu.CV.Mat src,
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
            CvInvoke.PutText(
                src,
                prediction.Label.Name,
                new System.Drawing.Point((int)x, (int)y),
                FontFace.HersheySimplex,
                1,
                color,
                thickness
            );
            CvInvoke.Rectangle(src, prediction.Rectangle.RectF2Rect(), color, thickness);
        }
    }

    //public Mat DrawSegMasks(Mat image, List<Mat> masks)
    //{
    //    Mat masked_img = new Mat();

    //    if (masks != null)
    //    {
    //        for (int i = 0; i < masks.Count; i++)
    //        {
    //            CvInvoke.AddWeighted(image, 0.5, masks[0], 0.5, 0, masked_img);
    //        }
    //    }
    //    return masked_img;
    //}
}
