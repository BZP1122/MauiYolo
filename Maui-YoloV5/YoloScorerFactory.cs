#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5
 * 唯一标识：a32c677d-9cae-4964-af11-452b085e7bc5
 * 文件名：YoloScorerFactory
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/7/27 9:44:09
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

using Maui_YoloV5.Models;
using Maui_YoloV5.Models.Interfaces;
using Maui_YoloV5.V8;
using MauiYoloV5;
using MauiYoloV5.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maui_YoloV5
{
    /// <summary>
    /// YoloScorerFactory 的摘要说明
    /// </summary>
    public class YoloScorerFactory
    {
        public enum YoloScorerEnums
        {
            V5Dectet,
            V8Dectet,
            V8Seg,
            V8Pose
        }

        private static IYoloScorer _scorer = null;

        public static IYoloScorer Factory(
            YoloScorerEnums type,
            string onnxPath,
            float confidence,
            List<YoloLabel> labels
        )
        {
            if (_scorer != null)
            {
                _scorer.Dispose();
            }
            try
            {
                switch (type)
                {
                    case YoloScorerEnums.V8Dectet:
                    {
                        YoloV8DefectModel defectModel = new YoloV8DefectModel();
                        _scorer = new YoloV8DefectScorer(
                            onnxPath,
                            defectModel with
                            {
                                Confidence = Math.Max(0.3f, confidence),
                                Labels = labels
                            }
                        );

                        break;
                    }
                    case YoloScorerEnums.V8Seg:
                    {
                        //SessionOptions session = new SessionOptions();
                        //session = SessionOptions.MakeSessionOptionWithCudaProvider(1);
                        YoloV8SegModel yoloV8SegModel = new YoloV8SegModel();
                        _scorer = new YoloV8SegScorer(
                            onnxPath,
                            yoloV8SegModel with
                            {
                                SegConfidence = 0.5f,
                                Confidence = Math.Max(0.3f, confidence),
                                Labels = labels
                            }
                        );
                        break;
                    }
                    case YoloScorerEnums.V8Pose:
                    {
                        YoloV8PoseModel yoloV8PoseModel = new YoloV8PoseModel();
                        _scorer = new YoloV8PoseScorer(
                            onnxPath,
                            yoloV8PoseModel with
                            {
                                PoseConfidence = 0.5f,
                                Confidence = Math.Max(0.1f, confidence),
                                Labels = new List<YoloLabel>() { new(0, "Person") }
                            }
                        );
                        break;
                    }
                    case YoloScorerEnums.V5Dectet:
                        YoloCocoP5Model yoloModel = new YoloCocoP5Model();
                        _scorer = new YoloScorer<YoloCocoP5Model>(
                            onnxPath,
                            yoloModel with
                            {
                                Confidence = Math.Max(0.1f, confidence),
                                Labels = new List<YoloLabel>() { new(0, "Person") }
                            }
                        );
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                _scorer = null;
                throw new Exception("实例化失败");
            }
            return _scorer;
        }
    }
}
