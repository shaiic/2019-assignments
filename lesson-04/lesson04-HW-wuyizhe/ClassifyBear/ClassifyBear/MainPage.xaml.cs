using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClassifyBear
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// 分类熊的模型
        /// </summary>
        private bearModel classificationModel = null;
        /// <summary>
        /// 检测熊的模型
        /// </summary>
        private ObjectDetection detectionModel = null;
        /// <summary>
        /// 保存的原始图片名称
        /// </summary>
        private string tempIn = "temp_in.jpg";
        /// <summary>
        /// 处理后的图片名称
        /// </summary>
        private string tempOut = "temp_out.jpg";
        /// <summary>
        /// 应用的文件路径
        /// </summary>
        private StorageFolder localFolder
        {
            get
            {
                return Windows.Storage.ApplicationData.Current.LocalFolder;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/bear.onnx"));
            classificationModel = await bearModel.CreateFromStreamAsync(modelFile);

            StorageFile detectionFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/bear-detection.onnx"));
            detectionModel = new ObjectDetection(new List<string> { "black bear", "brown bear", "polar bear" });

            await detectionModel.Init(detectionFile);
        }

        /// <summary>
        /// 识别图像分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtRecognize_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text = string.Empty;
            imgPreview.Source = null;

            try
            {
                // 下载图片
                Uri uri = new Uri(tbUrl.Text);
                var response = await new HttpClient().GetAsync(uri);
                var stream = (await response.Content.ReadAsStreamAsync()).AsRandomAccessStream();

                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // 构造模型需要的输入格式
                VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                ImageFeatureValue imageFeatureValue = ImageFeatureValue.CreateFromVideoFrame(videoFrame);

                // 推理
                var output = await classificationModel.EvaluateAsync(new bearInput() { data = imageFeatureValue });

                // 解析结果、更新控件
                var resultDescend = output.loss[0].OrderByDescending(p => p.Value);
                var name = output.classLabel.GetAsVectorView()[0];

                var stringResult = new StringBuilder();
                stringResult.AppendLine(name);
                stringResult.AppendLine();
                foreach (var kvp in resultDescend)
                {
                    stringResult.AppendLine(string.Format("{0}\t: {1}%", kvp.Key, kvp.Value));
                }

                tbResult.Text = stringResult.ToString();
                var sbs = new SoftwareBitmapSource();
                var imagePreview = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await sbs.SetBitmapAsync(imagePreview);

                imgPreview.Source = sbs;
            }
            catch (Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("读取图片错误：\n{0}", ex.ToString()));
                await a.ShowAsync();
            }
        }

        /// <summary>
        /// 检测图片中熊的位子并标识
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtDetection_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text = string.Empty;
            imgPreview.Source = null;
            btDetection.IsEnabled = false;

            try
            {
                // 下载图片
                Uri uri = new Uri(tbUrl.Text);
                var response = await new HttpClient().GetAsync(uri);
                var stream = (await response.Content.ReadAsStreamAsync()).AsRandomAccessStream();

                var decoder = await BitmapDecoder.CreateAsync(stream);                
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // 构造模型需要的输入格式
                VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap); 
                

                var stringResult = new StringBuilder();
                var output = await detectionModel.PredictImageAsync(videoFrame);
                var resultDescend = output.OrderByDescending(m => m.Probability);
                foreach (var r in resultDescend)
                {
                    stringResult.AppendLine(string.Format("{0}\t: {1}%", r.TagName, r.Probability * 100));
                }

                tbResult.Text = stringResult.ToString();

                if (output.Count > 0)
                {
                    softwareBitmap = await Process(softwareBitmap, output.First().BoundingBox);
                }

                imgPreview.Source = await LoadTempOutImage();
            }
            catch (Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("读取图片错误：\n{0}", ex.ToString()));
                await a.ShowAsync();
            }
            btDetection.IsEnabled = true;
        }

        /// <summary>
        /// 加载读取加好标识的图片
        /// </summary>
        /// <returns></returns>
        private async Task<BitmapImage> LoadTempOutImage()
        {
            var tmpFile = await localFolder.GetFileAsync(tempOut);
            var image = new BitmapImage(new Uri(tmpFile.Path));

            return image;
        }

        /// <summary>
        /// 处理原始网络获取的图片
        /// </summary>
        /// <param name="softwareBitmap"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>

        private async Task<SoftwareBitmap> Process(SoftwareBitmap softwareBitmap, BoundingBox boundingBox)
        {
            var tmpFile = await localFolder.CreateFileAsync(tempIn, CreationCollisionOption.ReplaceExisting);
            await SaveToTempFile(softwareBitmap, tmpFile);
            await DrawFrame(boundingBox);
            return softwareBitmap;
        }

        /// <summary>
        /// 按照传入的坐标信息在图片上画红色的框
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        private async Task DrawFrame(BoundingBox boundingBox)
        {            
            var inputFile = await localFolder.GetFileAsync(tempIn);
            BitmapDecoder imagedecoder;
            using (var imagestream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                imagedecoder = await BitmapDecoder.CreateAsync(imagestream);
            }
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, imagedecoder.PixelWidth, imagedecoder.PixelHeight, 96);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96);
                var imgWidth = (float)image.Bounds.Width;
                var imgHeight = (float)image.Bounds.Height;

                ds.DrawImage(image);
                //ds.DrawText("Test", new System.Numerics.Vector2(150, 150), Colors.Black);
                ds.DrawRectangle(imgWidth * boundingBox.Left, imgHeight * boundingBox.Top, imgWidth * boundingBox.Width, imgHeight * boundingBox.Height, Colors.Red);
            }
            
            
            var file = await localFolder.CreateFileAsync(tempOut, CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);                
            }
        }

        /// <summary>
        /// 保存图片到临时文件中
        /// <param name="softwareBitmap"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        private async Task SaveToTempFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {            
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }
            }
        }
    }
}
