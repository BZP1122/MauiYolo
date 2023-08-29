#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.Views.Common
 * 唯一标识：cb9911c2-06ed-4a94-9851-640b87df525c
 * 文件名：WeChatHook
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/7/26 17:18:38
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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;

namespace MauiLaser.Views.Common
{
    /// <summary>
    /// WeChatHook 的摘要说明
    /// </summary>
    public class WeChatHook
    {
        public string createMarkdownParam(string content)
        {
            string param =
                "{\"msgtype\":\"markdown\",\"markdown\":{\"content\":\"" + content + "\"}}";
            return param;
        }

        public string getDemoMarkdownMsg()
        {
            string cttStr = "";
            cttStr += "# <font color=\\\"warning\\\">带黄色的标题</font> 警告提醒\n";
            cttStr += "> ## 加粗信息，显示特别的信息\n";
            cttStr += "> <font color=\\\"warning\\\">黄色</font>消息，一般用于警告\n";
            cttStr += "> <font color=\\\"warning\\\">黄色</font>消息，一般用于警告\n";
            cttStr += "> <font color=\\\"info\\\">绿色</font>消息，一般用于安全\n";
            cttStr += "> <font color=\\\"comment\\\">灰色</font>消息，一般用于忽略\n";
            cttStr += "> 正常消息，一般用于普通文本\n";
            return createMarkdownParam(cttStr);
        }

        public async Task Post<T>(string postUrl, T paramObj)
            where T : class
        {
            Uri uri = new Uri(string.Format(postUrl, string.Empty));
            var _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            using (var client = new HttpClient())
            {
                string json = JsonSerializer.Serialize<T>(paramObj, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var Rsp = await client.PostAsync(uri, content);
            }
        }

        public void Test()
        {
            string hook = "";
            string content = getDemoMarkdownMsg();
            string webhookUrl = $"https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key={hook}";
            var rsp = Post<string>(webhookUrl, content);
        }
    }
}
