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
using CustomVision;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClassifyBear
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObjectDetection model = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile lablesFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/labels.txt"));
            IList<string> lables = await FileIO.ReadLinesAsync(lablesFile);
            List<string> lablesList = lables.ToList();

            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/model.onnx"));
            var max_value = 1;
            model = new ObjectDetection(lablesList, max_value);
            await model.Init(modelFile);
        }

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
                //推理
                var output = await model.PredictImageAsync(videoFrame);

                //结果解析
                if (output.Count >= 1 && output[0].Probability > 0.5)
                {
                    tbResult.Text = "Hi,Bear!";
                    BoundingBox outputBox = output[0].BoundingBox;

                    int Height = (int)Math.Max(0, outputBox.Height * softwareBitmap.PixelHeight);
                    int Left = (int)Math.Max(0, outputBox.Left * softwareBitmap.PixelWidth);
                    int Top = (int)Math.Max(0, outputBox.Top * softwareBitmap.PixelHeight);
                    int Width = (int)Math.Max(0, outputBox.Width * softwareBitmap.PixelWidth);

                    var sbs = new SoftwareBitmapSource();
                    var imagePreview = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    await sbs.SetBitmapAsync(imagePreview);
                    imgPreview.Source = sbs;

                    Thickness myThickness = new Thickness();
                    myThickness.Bottom = 0;
                    myThickness.Left = Left + imgPreview.Margin.Left;
                    myThickness.Right = 0;
                    myThickness.Top = Top + +imgPreview.Margin.Top;

                    tbLable.Margin = myThickness;
                    tbLable.Height = Height;
                    tbLable.Width = Width;
                    tbLable.Visibility = Visibility.Visible;
                    tbLable.Text = "Bear";
                }
                else
                {
                    tbResult.Text = "No bear!";
                }
                
            }
            catch (Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("读取图片错误：\n{0}", ex.ToString()));
                await a.ShowAsync();
            }
        }
    }
}
