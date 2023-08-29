#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.Models.Interfaces
 * 唯一标识：8cd1c3bc-e493-4e10-a150-6664ad9dc7ca
 * 文件名：YoloV8PoseModel
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/7/26 9:04:25
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

using Maui_YoloV5.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maui_YoloV5.Models
{
    /// <summary>
    /// YoloV8PoseModel 的摘要说明
    /// Dimensions x,y,w,h+ 人类别 confidence + pt(17(关节点数量) *3)-> x,y,v 位置 x,y 点位置信度 v
    /// </summary>
    public record YoloV8PoseModel()
        : YoloModel(
            640,
            640,
            3,
            Dimensions: 56,
            new[] { 8, 16, 32, 64 },
            new[]
            {
                new[] { new[] { 010, 13 }, new[] { 016, 030 }, new[] { 033, 023 } },
                new[] { new[] { 030, 61 }, new[] { 062, 045 }, new[] { 059, 119 } },
                new[] { new[] { 116, 90 }, new[] { 156, 198 }, new[] { 373, 326 } }
            },
            new[] { 80, 40, 20 },
            0.6f,
            0.25f,
            0.45f,
            new[] { "output0", "output1" },
            new() { new(1, "person"), },
            true
        )
    {
        public float PoseConfidence { get; init; } = 0.5f;
    }
}
