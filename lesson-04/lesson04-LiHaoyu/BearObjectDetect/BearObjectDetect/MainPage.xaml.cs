using CustomVision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BearObjectDetect
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TbRun_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tbBearType.Text = string.Empty;

            Uri imageUri = null;
            try
            {
                imageUri = new Uri(tbImageUrl.Text);
            }
            catch (Exception)
            {
                tbBearType.Text = "URL不合法";
                return;
            }

            tbBearType.Text = "加载图片...";
            imgBear.Source = new BitmapImage(imageUri);

        }

        private void ImgBear_ImageOpened(object sender, RoutedEventArgs e)
        {
            RecognizeBear();
        }

        private void ImgBear_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            tbBearType.Text = "图片加载失败";
        }


        private async void RecognizeBear()
        {
            // 加载模型
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(imgBear);

            // 取得所有像素值
            var pixelBuffer = await rtb.GetPixelsAsync();

            // 构造模型需要的输入格式
            SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, rtb.PixelWidth, rtb.PixelHeight);
            VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

            //Create writealbe Bitmap
            WriteableBitmap writeableBmp = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
            softwareBitmap.CopyToBuffer(writeableBmp.PixelBuffer);

            //导入模型
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/model.onnx"));
            List<string> labels = new List<string>();
            labels.Add("bear");
            labels.Add("pig");
            ObjectDetection model = new ObjectDetection(labels);
            await model.Init(modelFile);

            // 推理
            //BearModelOutput output = await model.EvaluateAsync(bearModelInput);
            List<PredictionModel> listPredictionModel;
            listPredictionModel = await model.PredictImageAsync(videoFrame) as List<PredictionModel>;

            PredictionModel modelResult = null;
            float probability = 0;
            foreach (PredictionModel pItem in listPredictionModel)
            {

                if (pItem.Probability > probability)
                {
                    modelResult = pItem;
                    probability = pItem.Probability;
                }

            }
            // writeableBmp.DrawRectangle(254, 39, 275, 242, Colors.Red);
            if (probability > 0)
            {
                int left, top, width, height;
                left = (int)(modelResult.BoundingBox.Left * writeableBmp.PixelWidth);
                top = (int)(modelResult.BoundingBox.Top * writeableBmp.PixelHeight);
                width = (int)(modelResult.BoundingBox.Width * writeableBmp.PixelWidth);
                height = (int)(modelResult.BoundingBox.Height * writeableBmp.PixelHeight);

                writeableBmp.DrawRectangle(left, top, left + width, top + height, Colors.Red);
                tbBearType.Text = modelResult.TagName;
                //writeableBmp.DrawRectangle(111, 189, (int) (232*1.4), (int) (299*1.4), Colors.Red);
                imgBear.Source = writeableBmp;
            }

        }
    }
}
