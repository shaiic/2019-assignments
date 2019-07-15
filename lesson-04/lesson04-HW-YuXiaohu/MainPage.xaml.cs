using System;
using System.Collections.Generic;
using System.Drawing;
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
        //private bearModel model = null;
        private ObjectDetection model = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/bear.onnx"));
            //model = await bearModel.CreateFromStreamAsync(modelFile);
            string labelsString = await File.ReadAllTextAsync("Assets/labels.txt");
            var labels = labelsString.Split("\r\n");
            model = new ObjectDetection(labels);
            await model.Init(modelFile);
        }

        private async void BtRecognize_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text = string.Empty;
            imgPreview.Source = null;


            List<UIElement> removedItems = new List<UIElement>();
            foreach (var item in gridMain.Children)
            {
                if (item is Border || (item is TextBlock && item != tbResult))
                {
                    removedItems.Add(item);
                }
            }
            foreach (var item in removedItems)
            {
                gridMain.Children.Remove(item);
            }

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
                //var output = await model.EvaluateAsync(new bearInput() { data = imageFeatureValue });
                var output = await model.PredictImageAsync(videoFrame);

                // 解析结果、更新控件
                var stringResult = new StringBuilder();
                var pbWidth = imgPreview.ActualWidth;
                var pbHeight = imgPreview.ActualHeight;
                Random rand = new Random();

                foreach (var item in output)
                {
                    if (item.Probability > 0.5)
                    {
                        int x = (int)Math.Round(item.BoundingBox.Left * pbWidth);
                        int y = (int)Math.Round(item.BoundingBox.Top * pbHeight);
                        int w = (int)Math.Round(item.BoundingBox.Width * pbWidth);
                        int h = (int)Math.Round(item.BoundingBox.Height * pbHeight);
                        stringResult.AppendLine(string.Format("{0}: {1}, [{2},{3},{4},{5}]",
                            item.TagName, item.Probability, x, y, w, h));

                        Rectangle rect = new Rectangle(x, y, w, h);
                        Color color = Color.FromKnownColor((KnownColor)rand.Next(1, 175));
                        Windows.UI.Color uiColor = Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
                        Border b = new Border();
                        b.BorderBrush = new SolidColorBrush(uiColor);
                        b.BorderThickness = new Thickness(2);
                        b.Width = w;
                        b.Height = h;
                        b.Margin = new Thickness(x + imgPreview.Margin.Left, y + imgPreview.Margin.Top, 0, 0);
                        b.HorizontalAlignment = HorizontalAlignment.Left;
                        b.VerticalAlignment = VerticalAlignment.Top;
                        gridMain.Children.Add(b);

                        TextBlock tb = new TextBlock();
                        tb.Margin = new Thickness(b.Margin.Left + 2, b.Margin.Top + 2, 0, 0);
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.Text = item.TagName;
                        tb.IsColorFontEnabled = true;
                        tb.Foreground = b.BorderBrush;
                        gridMain.Children.Add(tb);
                    }
                }
                tbResult.Text = stringResult.ToString();



                //以下老师原代码
                /*var resultDescend = output.loss[0].OrderByDescending(p => p.Value);
                var name = output.classLabel.GetAsVectorView()[0];

                var stringResult = new StringBuilder();
                stringResult.AppendLine(name);
                stringResult.AppendLine();
                foreach (var kvp in resultDescend)
                {
                    stringResult.AppendLine(string.Format("{0}\t: {1}%", kvp.Key, kvp.Value));
                }
                */
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
    }
}
