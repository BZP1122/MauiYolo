#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Converters
 * 唯一标识：231c993f-15ed-4ec9-af24-4c4e68329dde
 * 文件名：FilePath2NameConverter
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/24 12:59:16
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MauiLaser.Converters
{
    /// <summary>
    /// FilePath2NameConverter 的摘要说明
    /// </summary>
    public class FilePath2NameConverter : IValueConverter
    {
        #region <常量>
        #endregion <常量>

        #region <变量>
        #endregion <变量>

        #region <属性>
        #endregion <属性>

        #region <构造方法和析构方法>
        #endregion <构造方法和析构方法>

        #region <方法>
        #endregion <方法>

        #region <事件>
        #endregion <事件>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Path.GetFileName(value.ToString());
            }
            return string.Empty;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            return value;
        }
    }
}
