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
using System.Drawing;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace classifyBear
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

        private ObjectDetection model = null;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile modelFile =
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Assets/model.onnx"));
            string labelsString =
                await File.ReadAllTextAsync("Assets/labels.txt");
            var labels = labelsString.Split("\r\n");
            model = new ObjectDetection(labels);
            await model.Init(modelFile);
        }

        private async void TbRecognize_Click(object sender, RoutedEventArgs e)
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
                Uri uri = new Uri(tbUrl.Text);
                var response = await new HttpClient().GetAsync(uri);
                var stream = (await response.Content.ReadAsStreamAsync()).AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                //ImageFeatureValue imageFreatureValue =
                //    ImageFeatureValue.CreateFromVideoFrame(videoFrame);

                var output = await model.PredictImageAsync(videoFrame);

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


                var sbs = new SoftwareBitmapSource();
                var imagePreview = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied);
                await sbs.SetBitmapAsync(imagePreview);
                imgPreview.Source = sbs;
            }
            catch (Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("error: \n{0}", ex.ToString()));
                await a.ShowAsync();
            }
        }
    }
}
