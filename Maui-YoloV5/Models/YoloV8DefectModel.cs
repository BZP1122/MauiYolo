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
    /// YoloV8DefectModel 的摘要说明
    ///  Dimensions = 4(x,y,w,h)+80(cls)
    /// </summary>
    public record YoloV8DefectModel()
        : YoloModel(
            640,
            640,
            3,
            84,
            new[] { 8, 16, 32, 64 },
            new[]
            {
                new[] { new[] { 010, 13 }, new[] { 016, 030 }, new[] { 033, 023 } },
                new[] { new[] { 030, 61 }, new[] { 062, 045 }, new[] { 059, 119 } },
                new[] { new[] { 116, 90 }, new[] { 156, 198 }, new[] { 373, 326 } }
            },
            new[] { 80, 40, 20 },
            0.20f,
            0.25f,
            0.45f,
            new[] { "output0" },
            new()
            {
                new(1, "AIQIYI"),
                new(2, "BaiDuMap"),
                new(3, "BaiDuSearch"),
                new(4, "bilibili"),
                new(5, "Camera"),
                new(6, "Contanct"),
                new(7, "DouYin"),
                new(8, "DouYuTV"),
                new(9, "GaoDeMap"),
                new(10, "HePinJingYin"),
                new(11, "HUAWEIRead"),
                new(12, "KaiXinXiaoXiaoLe"),
                new(13, "HuYaTv"),
                new(14, "KuaiShou"),
                new(15, "KuGouMusic"),
                new(16, "QiMaoBook"),
                new(17, "QQ"),
                new(18, "QQBrowser"),
                new(19, "Setting"),
                new(20, "SysBrowser"),
                new(21, "TenceVideo"),
                new(22, "TouTiao"),
                new(23, "UCBrowser"),
                new(24, "WangYiYun"),
                new(25, "WangZheRongYao"),
                new(26, "WeChat"),
                new(27, "WeiBo"),
                new(28, "YouKu"),
                new(29, "TaoBao"),
                new(30, "TaoBao"),
            },
            true
        );
}
