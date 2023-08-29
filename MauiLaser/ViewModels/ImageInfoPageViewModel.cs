using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using Honor_Maui.Core.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Converters;
using Communication = Microsoft.Maui.ApplicationModel.Communication;

using System.Windows.Input;
using System.IO;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Mvvm.Messaging;
using MauiLaser.Core.Models.Dtos;
using MauiLaser.Extensions.SerialData;
using Microsoft.Maui;
using Camera.MAUI;
using ZXing;
using static Microsoft.Maui.ApplicationModel.Permissions;
using CommunityToolkit.Maui.Views;
using Camera.MAUI.ZXingHelper;
using System.Threading;
using ZXing.Common;
using Maui_YoloV5.Models.Interfaces;
using MauiYoloV5;
using Maui_YoloV5.Models;
using MauiYoloV5.V8;
using Maui_YoloV5.V8;
using MauiLaser.Core.PermissionsRequest;
using CommunityToolkit.Maui.Core.Views;
using System.Runtime.InteropServices;
using Microsoft.Maui.Dispatching;

namespace MauiLaser.ViewModels
{
    internal partial class TaskPageViewModel : ObservableObject
    {
        Socket _socket;
        byte[] _temp = new byte[0]; //存放一帧图像的数据

        [ObservableProperty]
        string ip = "192.168.191.2";

        [ObservableProperty]
        string tipInfo;

        [ObservableProperty]
        int port = 1234;

        private CameraInfo camera = null;

        public CameraInfo Camera
        {
            get { return camera; }
            set
            {
                SetProperty(ref camera, value);
                AutoStartPreview = false;
                AutoStartPreview = true;
            }
        }

        [ObservableProperty]
        private ObservableCollection<CameraInfo> cameras = new();

        [ObservableProperty]
        private MicrophoneInfo microphone = null;

        [ObservableProperty]
        private ObservableCollection<MicrophoneInfo> microphones = new();

        [ObservableProperty]
        private BarcodeDecodeOptions barCodeOptions;

        [ObservableProperty]
        private Result[] barCodeResults;

        [ObservableProperty]
        string barcodeText;

        [ObservableProperty]
        private bool takeSnapshot = false;

        [ObservableProperty]
        private bool autoStartPreview = false;

        [ObservableProperty]
        private MediaSource videoSource;

        [ObservableProperty]
        private bool autoStartRecording = false;

        [ObservableProperty]
        private float snapshotSeconds = 0.3f;

        [ObservableProperty]
        string recordingFile;

        [ObservableProperty]
        bool isDefectYolo;

        public int NumCameras
        {
            set
            {
                if (value > 0)
                    Camera = Cameras.First();
            }
        }

        public int NumMicrophones
        {
            set
            {
                if (value > 0)
                    Microphone = Microphones.First();
            }
        }

        int thickness = 5;

        IYoloScorer scorer;

        string imagePath = "";

        IMessenger message = WeakReferenceMessenger.Default;

        [ObservableProperty]
        ImageSource imageDefect;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenRtspCommand))]
        bool isCanOpenRtsp = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCameraCommand))]
        bool isCanOpenCamera = true;

        [ObservableProperty]
        List<YoloPrediction> predictions;

        [ObservableProperty]
        bool isDownloadingImage = false;

        [ObservableProperty]
        ImageSource snapImage;

        MemoryStream _imgStream;

        private DeviceConfig deviceConfig = null;

        public TaskPageViewModel()
        {
            BarCodeOptions = new BarcodeDecodeOptions
            {
                AutoRotate = true,
                PossibleFormats = { BarcodeFormat.QR_CODE },
                ReadMultipleCodes = false,
                TryHarder = true,
                TryInverted = true
            };
#if IOS
            RecordingFile = Path.Combine(FileSystem.Current.CacheDirectory, "Video.mov");
#else
            RecordingFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Video.mp4"
            );

            RecordingFile = Path.Combine(FileSystem.Current.CacheDirectory, "Video.mp4");
#endif
            deviceConfig = new DeviceConfig();
            deviceConfig = deviceConfig.ReadDeviceConfig();
            SelectedModel();
            scorer.OnStandChanged += () =>
            {
                Task.Run(async () =>
                {
                    //await Microsoft.Maui.Devices.Flashlight.Default.TurnOnAsync();
                    //await Task.Delay(1000);
                    //await Microsoft.Maui.Devices.Flashlight.Default.TurnOffAsync();
                    IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();

                    SpeechOptions options = new SpeechOptions()
                    {
                        Pitch = 0f, // 0.0 - 2.0
                        Volume = 0f, // 0.0 - 1.0
                        Locale = locales.FirstOrDefault()
                    };

                    await TextToSpeech.Default.SpeakAsync("Warning");
                });
            };

            message.Register<DeviceConfig>(
                this,
                (o, config) =>
                {
                    if (config != null)
                    {
                        this.deviceConfig = config;
                    }
                }
            );
        }

        partial void OnSnapImageChanged(ImageSource oldValue, ImageSource newValue)
        {
            SnapCallBackAsync();
        }

        public async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }
            return null;
        }

        private Mat ConterImageSoureToMat(ImageSource value)
        {
            Stream result = (
                (value as StreamImageSource)
                ?? throw new ArgumentException(
                    "Expected value to be of type StreamImageSource.",
                    "value"
                )
            )
                .Stream(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            if (result == null)
            {
                return null;
            }
            _imgStream?.Dispose();
            _imgStream = new MemoryStream();
            result.CopyTo(_imgStream);
            var buffer = _imgStream.ToArray();
            Mat src = new Mat();
            CvInvoke.Imdecode(buffer, ImreadModes.Color, src);
            return src;
        }

        private ImageSource ConvertMatToImageSource(Emgu.CV.Mat mat)
        {
            Image<Bgr, byte> image = mat.ToImage<Bgr, byte>();
            byte[] bytes = image.ToJpegData();
            MemoryStream memoryStream = new MemoryStream(bytes);
            return ImageSource.FromStream(() => memoryStream);
        }

        private ImageSource ConvertBytesToImageSource(byte[] bytes)
        {
            byte[] mat = (byte[])bytes.Clone();

            _imgStream?.Dispose();
            _imgStream = new MemoryStream(mat);
            // 创建 ImageSource 对象并返回
            return ImageSource.FromStream(() => _imgStream);
        }

        #region Command


        [RelayCommand]
        private async Task OpenTcpClientAsync()
        {
            //await ReceiveImageBuffer();
        }

        [RelayCommand]
        private void SelectedModel()
        {
            if (scorer != null)
            {
                scorer.Dispose();
            }
            if (this.deviceConfig != null)
            {
                try
                {
                    var cls = deviceConfig.ReadLablesFromFile()?.ToArray();
                    List<YoloLabel> labels = new List<YoloLabel>();
                    for (int i = 1; i < cls.Length + 1; i++)
                    {
                        labels.Add(new YoloLabel(i, cls[i - 1]));
                    }
                    switch (deviceConfig.SelectYoloModel)
                    {
                        case "YoloV8-Defect":
                        {
                            YoloV8DefectModel defectModel = new YoloV8DefectModel();
                            scorer = new YoloV8DefectScorer(
                                deviceConfig.OnnxFilePath,
                                defectModel with
                                {
                                    Confidence = Math.Max(0.3f, deviceConfig.Confidence),
                                    Labels = labels
                                }
                            );

                            break;
                        }
                        case "YoloV8-Seg":
                        {
                            //SessionOptions session = new SessionOptions();
                            //session = SessionOptions.MakeSessionOptionWithCudaProvider(1);
                            YoloV8SegModel yoloV8SegModel = new YoloV8SegModel();
                            scorer = new YoloV8SegScorer(
                                deviceConfig.OnnxFilePath,
                                yoloV8SegModel with
                                {
                                    SegConfidence = 0.5f,
                                    Confidence = Math.Max(0.3f, deviceConfig.Confidence),
                                    Labels = labels
                                }
                            );
                            break;
                        }
                        case "YoloV8-Pose":
                        {
                            YoloV8PoseModel yoloV8PoseModel = new YoloV8PoseModel();
                            scorer = new YoloV8PoseScorer(
                                deviceConfig.OnnxFilePath,
                                yoloV8PoseModel with
                                {
                                    PoseConfidence = 0.5f,
                                    Confidence = Math.Max(0.1f, deviceConfig.Confidence),
                                    Labels = new List<YoloLabel>() { new(0, "Person") }
                                }
                            );
                            ;
                            break;
                        }
                        default:
                            break;
                    }
                }
                catch
                {
                    scorer = null;
                }
            }
        }

        [RelayCommand]
        private async Task SelectedImageAsync()
        {
            var ret = await PickAndShow(PickOptions.Images);
            imagePath = ret?.FullPath;
            try
            {
                IsDownloadingImage = true;
                ImageDefect = ImageSource.FromFile(imagePath);
            }
            catch (Exception) { }
            finally
            {
                IsDownloadingImage = false;
            }
        }

        [RelayCommand]
        private async void ScreenShot()
        {
            TakeSnapshot = false;
            TakeSnapshot = true;
            foreach (var contact in await GetContactNames())
            {
                Debug.WriteLine($"{DateTime.Now}: number={contact.DisplayName}");
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task DefectImageAsync()
        {
            if (
                !File.Exists(deviceConfig?.OnnxFilePath)
                || !File.Exists(imagePath)
                || scorer == null
            )
            {
                await Application.Current.MainPage.DisplayAlert("提示", "请先加载模型并选择图像？", "OK");
                return;
            }
            try
            {
                IsDownloadingImage = true;
                await Task.Run(() =>
                    {
                        Mat src = new Mat(imagePath, ImreadModes.AnyColor);
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Predictions = scorer.Predict(src);
                        sw.Stop();
                        Debug.WriteLine($"CPU 识别时间：{sw.ElapsedMilliseconds}ms");
                        scorer.DrawMat(ref src, Predictions, thickness);
                        ImageDefect = ConvertMatToImageSource(src);
                        src.Dispose();
                    })
                    .ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            throw t.Exception;
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
            finally
            {
                IsDownloadingImage = false;
            }
        }
        #endregion

        [RelayCommand]
        private void StartCamera()
        {
            AutoStartPreview = true;
        }

        [RelayCommand]
        private void StopCamera()
        {
            AutoStartPreview = false;
        }

        [RelayCommand]
        private void StartRecording()
        {
            AutoStartRecording = true;
        }

        [RelayCommand]
        private void StopRecording()
        {
            AutoStartRecording = false;
            try
            {
                VideoSource = MediaSource.FromFile(RecordingFile);
            }
            catch (Exception) { }
        }

        [RelayCommand]
        public async Task SendEmailAsync()
        {
            if (Email.Default.IsComposeSupported)
            {
                string subject = "Warning!";
                string body = "Warning Warning Warning!!!!";
                string[] recipients = new[] { "738771920@qq.com", "738771920@qq.com" };

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>(recipients)
                };

                await Email.Default.ComposeAsync(message);
            }
        }

        #region Ignore

        public async Task<IEnumerable<Contact>> GetContactNames()
        {
            await Permissions.RequestAsync<ContactPermission>();
            IEnumerable<Contact> contacts = await Communication.Contacts.Default.GetAllAsync();
            return contacts;
        }

        [RelayCommand(
            CanExecute = nameof(IsCanOpenRtsp),
            AllowConcurrentExecutions = false,
            IncludeCancelCommand = true
        )]
        public async Task OpenRtspAsync(CancellationToken token)
        {
            IsCanOpenRtsp = false;
            Mat frame = new Mat();
            try
            {
                await Task.Run(() =>
                    {
                        VideoCapture capture = new VideoCapture(deviceConfig.RtspAddress);
                        if (!capture.IsOpened)
                        {
                            throw new Exception("Open Rtsp Faid");
                        }
                        while (capture.IsOpened)
                        {
                            if (token.IsCancellationRequested)
                                break;
                            var ret = capture.Read(frame);
                            if (ret)
                                ImageDefect = ConvertMatToImageSource(frame);
                        }
                    })
                    .ContinueWith(t =>
                    {
                        IsCanOpenRtsp = true;
                        frame.Dispose();
                        if (!t.IsCompletedSuccessfully)
                        {
                            throw t.Exception;
                        }
                    });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("error", ex.Message, "OK");
            }
        }

        Mat src;

        private void SnapCallBackAsync()
        {
            if (IsDefectYolo && Camera != null && AutoStartPreview)
            {
                TakeSnapshot = false;
                TakeSnapshot = true;
                if (SnapImage != null)
                {
                    try
                    {
                        src = ConterImageSoureToMat(SnapImage);
                        Predictions = scorer?.Predict(src);
                        scorer?.DrawMat(ref src, Predictions, thickness);
                        ImageDefect = ConvertMatToImageSource(src);
                    }
                    catch (Exception) { }
                }
            }
        }

        private async Task ReceiveImageBuffer()
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

                var from = new IPEndPoint(0, 0);

                await Task.Factory.StartNew(
                    () =>
                    {
                        while (true)
                        {
                            //await Task.Delay(100);
                            var recvBuffer = udpClient.Receive(ref from);
                            _temp = byteMerger(_temp, b: recvBuffer);
                            if (recvBuffer.Length != 1430)
                            {
                                ImageDefect = ConvertBytesToImageSource(_temp);
                                Debug.WriteLine("frame length:" + recvBuffer.Length);

                                _temp = new byte[0];
                            }
                        }
                    },
                    TaskCreationOptions.LongRunning
                );

                //var data = Encoding.UTF8.GetBytes("on");
                //udpClient.Send(data, data.Length, Ip, Port);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "error",
                    "tcp 连接异常" + ex.Message,
                    "OK"
                );
            }
        }

        //合并一帧图像数据  a 全局变量 temp   b  接受的一个数据包 RevBuff
        public byte[] byteMerger(byte[] a, byte[] b)
        {
            int i = a.Length + b.Length;
            byte[] t = new byte[i]; //定义一个长度为 全局变量temp  和 数据包RevBuff 一起大小的字节数组 t
            Array.Copy(a, 0, t, 0, a.Length); //先将 temp（先传过来的数据包）放进  t
            Array.Copy(b, 0, t, a.Length, b.Length); //然后将后进来的这各数据包放进t
            return t; //返回t给全局变量 temp
        }
        #endregion
    }
}
