#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：Maui_YoloV5.V8
 * 唯一标识：3bc52c77-061f-4a8c-b8f5-3130dd5a9259
 * 文件名：PoseInfo
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/7/26 11:39:37
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Net.WebRequestMethods;

namespace Maui_YoloV5.V8
{
    /// <summary>
    /// PoseInfo 的摘要说明
    /// </summary>
    public class PoseInfo
    {
        public readonly static string[] PoseNames = new string[]
        {
            "nose",
            "left_eye",
            "right_eye",
            "left_ear",
            "right_ear",
            "left_shoulder",
            "right_shoulder",
            "left_elbow",
            "right_elbow",
            "left_wrist",
            "right_wrist",
            "left_hip",
            "right_hip",
            "left_knee",
            "right_knee",
            "left_ankle",
            "right_ankle"
        };

        public PoseInfo(string name, System.Drawing.Point pt, float confidece)
        {
            this.Name = name;
            Pt = pt;
            this.Confidece = confidece;
        }

        public string Name { get; set; }
        public System.Drawing.Point Pt { get; set; }
        public float Confidece { get; set; }

        //public static Mat DrawLines(Mat src, List<PoseInfo> poses)
        //{

        //}
    }
}
