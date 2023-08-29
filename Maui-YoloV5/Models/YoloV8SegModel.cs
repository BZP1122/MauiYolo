#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.Models
 * 唯一标识：709a56d0-90c4-4bf3-b932-d03441e42d0a
 * 文件名：IocnP5Models
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/19 12:59:47
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
    /// YoloV8SegModel
    /// yoloV8 实例分割
    /// Dimensions = 4(x,y,w,h)+80(cls)+32(seg)
    /// </summary>
    public record YoloV8SegModel()
        : YoloModel(
            640,
            640,
            3,
            Dimensions: 116,
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
            new()
            {
                new(1, "person"),
                new(2, "bicycle"),
                new(3, "car"),
                new(4, "motorcycle"),
                new(5, "airplane"),
                new(6, "bus"),
                new(7, "train"),
                new(8, "truck"),
                new(9, "boat"),
                new(10, "traffic light"),
                new(11, "fire hydrant"),
                new(12, "stop sign"),
                new(13, "parking meter"),
                new(14, "bench"),
                new(15, "bird"),
                new(16, "cat"),
                new(17, "dog"),
                new(18, "horse"),
                new(19, "sheep"),
                new(20, "cow"),
                new(21, "elephant"),
                new(22, "bear"),
                new(23, "zebra"),
                new(24, "giraffe"),
                new(25, "backpack"),
                new(26, "umbrella"),
                new(27, "handbag"),
                new(28, "tie"),
                new(29, "suitcase"),
                new(30, "frisbee"),
                new(31, "skis"),
                new(32, "snowboard"),
                new(33, "sports ball"),
                new(34, "kite"),
                new(35, "baseball bat"),
                new(36, "baseball glove"),
                new(37, "skateboard"),
                new(38, "surfboard"),
                new(39, "tennis racket"),
                new(40, "bottle"),
                new(41, "wine glass"),
                new(42, "cup"),
                new(43, "fork"),
                new(44, "knife"),
                new(45, "spoon"),
                new(46, "bowl"),
                new(47, "banana"),
                new(48, "apple"),
                new(49, "sandwich"),
                new(50, "orange"),
                new(51, "broccoli"),
                new(52, "carrot"),
                new(53, "hot dog"),
                new(54, "pizza"),
                new(55, "donut"),
                new(56, "cake"),
                new(57, "chair"),
                new(58, "couch"),
                new(59, "potted plant"),
                new(60, "bed"),
                new(61, "dining table"),
                new(62, "toilet"),
                new(63, "tv"),
                new(64, "laptop"),
                new(65, "mouse"),
                new(66, "remote"),
                new(67, "keyboard"),
                new(68, "cell phone"),
                new(69, "microwave"),
                new(70, "oven"),
                new(71, "toaster"),
                new(72, "sink"),
                new(73, "refrigerator"),
                new(74, "book"),
                new(75, "clock"),
                new(76, "vase"),
                new(77, "scissors"),
                new(78, "teddy bear"),
                new(79, "hair drier"),
                new(80, "toothbrush")
            },
            true
        )
    {
        public float SegConfidence { get; init; } = 0.5f;
    }
}
