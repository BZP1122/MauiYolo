#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：$bzp$
 * 公司名称：
 * 命名空间：MauiLaser.ViewModels
 * 唯一标识：0c8d25a8-d3e2-46da-8973-c8d339d74233
 * 文件名：SettingPageViewModel
 * 当前用户域：LAPTOP-6EGECOUF
 *
 * 创建者：$bzp$
 * 电子邮箱：738771920@qq.com
 * 创建时间：2023/5/23 10:37:10
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Emgu.CV.Ocl;
using MauiLaser.Core.Models.Dtos;
using MauiLaser.Extensions.SerialData;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace MauiLaser.ViewModels
{
    /// <summary>
    /// SettingPageViewModel 的摘要说明
    /// </summary>
    public partial class SettingPageViewModel : ObservableObject
    {
        IMessenger message = WeakReferenceMessenger.Default;

        [ObservableProperty]
        DeviceConfig config = new DeviceConfig();

        [ObservableProperty]
        ObservableCollection<string> yoloModels;

        public SettingPageViewModel()
        {
            Config = Config.ReadDeviceConfig() ?? new DeviceConfig();
            yoloModels = new ObservableCollection<string>();
            YoloModels.Add("YoloV8-Seg");
            YoloModels.Add("YoloV8-Defect");
            YoloModels.Add("YoloV8-Pose");
            if (
                string.IsNullOrWhiteSpace(Config.SelectYoloModel)
                || YoloModels.ToList().FindIndex(t => t == Config.SelectYoloModel) == -1
            )
            {
                YoloModels?.First();
            }
        }

        [RelayCommand]
        private async Task SelectParamsAsync(string cmdParams)
        {
            var fileResult = await FilePicker.PickAsync();
            if (fileResult == null)
                return;
            if (cmdParams == "onnx")
            {
                if (fileResult.FileName.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
                {
                    Config.OnnxFilePath = fileResult.FullPath;
                }
            }
            else if (cmdParams == "clsFile")
            {
                if (fileResult.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    Config.ClsFilePath = fileResult.FullPath;
                }
            }
        }

        [RelayCommand]
        private async Task SaveDeviceConfigAsync()
        {
            try
            {
                Config?.SaveDeviceConfig();
                message?.Send<DeviceConfig>(Config);
                await Application.Current.MainPage.DisplayAlert("tip", "保存成功", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"保存异常：{ex.Message}", "OK");
            }
        }
    }
}
