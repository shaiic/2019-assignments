using CustomVision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
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
        //private modelModel model = null;
        private ObjectDetection objDet = null;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/model.onnx"));
            //model = await modelModel.CreateFromStreamAsync(modelFile);

            StorageFile ObjClassFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/labels.txt"));
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/model.onnx"));
            var MAX_OBJS_NUM = 1;
            IList<string> data = await FileIO.ReadLinesAsync(ObjClassFile);
            List<string> ObjCls = data.ToList();

            objDet = new ObjectDetection(ObjCls, MAX_OBJS_NUM);
            await objDet.Init(modelFile);
        }

        private async void BtRecognize_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text = string.Empty;
            imgPreview.Source = null;
            //rectDect.Visibility = Visibility.Collapsed;
            //tbDect.Visibility = Visibility.Collapsed;


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
                //ImageFeatureValue imageFeatureValue = ImageFeatureValue.CreateFromVideoFrame(videoFrame);

                // 推理
                //var output = await model.EvaluateAsync(new modelInput() { data = imageFeatureValue });

                var output = await objDet.PredictImageAsync(videoFrame);


                // 解析结果、更新控件
                //var resultDescend = output.loss[0].OrderByDescending(p => p.Value);
                //var name = output.classLabel.GetAsVectorView()[0];

                BoundingBox boundingB = null;
                var stringResult = new StringBuilder();

                if (output.Count < 1)
                {
                    stringResult.AppendLine("Bear Region is not detected.");
                    tbResult.Text = stringResult.ToString();
                    return;
                }

                boundingB = output[0].BoundingBox;
                int Height = (int)Math.Max(0, boundingB.Height * softwareBitmap.PixelHeight);
                int Left = (int)Math.Max(0, boundingB.Left * softwareBitmap.PixelWidth);
                int Top = (int)Math.Max(0, boundingB.Top * softwareBitmap.PixelHeight);
                int Width = (int)Math.Max(0, boundingB.Width * softwareBitmap.PixelWidth);

                stringResult.AppendLine("Bear Region is as below:");
                stringResult.AppendLine("");
                stringResult.AppendLine("Height(pixel):" + Height.ToString());
                stringResult.AppendLine("Left(pixel):" + Left.ToString());
                stringResult.AppendLine("Top(pixel):" + Top.ToString());
                stringResult.AppendLine("Width(pixel):" + Width.ToString());
                tbResult.Text = stringResult.ToString();

                //tbResult.Text = stringResult.ToString();
                var sbs = new SoftwareBitmapSource();
                var imagePreview = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await sbs.SetBitmapAsync(imagePreview);
                imgPreview.Source = sbs;

                Thickness myThickness = new Thickness();
                myThickness.Bottom = 0;
                myThickness.Left = Left + imgPreview.Margin.Left;
                myThickness.Right = 0;
                myThickness.Top = Top + +imgPreview.Margin.Top;
                //rectDect.Margin = myThickness;
                //rectDect.Height = Height;
                //rectDect.Width = Width; 
                //rectDect.Visibility = Visibility.Visible;

                tbDect.Margin = myThickness;
                tbDect.Height = Height;
                tbDect.Width = Width;
                tbDect.Visibility = Visibility.Visible;


            }
            catch (Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("读取图片错误：\n{0}", ex.ToString()));
                await a.ShowAsync();
            }
        }
    }
}
