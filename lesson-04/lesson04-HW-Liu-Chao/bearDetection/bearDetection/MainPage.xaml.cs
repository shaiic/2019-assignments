using CustomVision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
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
using Windows.UI.Xaml.Shapes;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace bearDetection
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

        private ObjectDetection od = null;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var maximumObjects = 5; //maximum number of Objects that can be detected

            StorageFile labelsFile =
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Assets/labels.txt"));
            IList<string> data = await FileIO.ReadLinesAsync(labelsFile);
            List<string> labelsList = data.ToList(); 

            od = new ObjectDetection(labelsList, maximumObjects);
            StorageFile modelFile =
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Assets/model.onnx"));
            await od.Init(modelFile);
        }

        private void DrawBoxes(IList<PredictionModel> detectedBears, SoftwareBitmap source)
        {
            if (detectedBears != null)
            {
                SolidColorBrush lineBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                double lineThickness = 2.0;
                SolidColorBrush fillBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);

                foreach (PredictionModel bear in detectedBears)
                {
                    var x = Math.Max(0, bear.BoundingBox.Left * source.PixelWidth);
                    var y = Math.Max(0, bear.BoundingBox.Top * source.PixelHeight);
                    var w = Math.Min(bdCanvas.Width - x, bear.BoundingBox.Width * source.PixelWidth);
                    var h = Math.Min(bdCanvas.Height - y, bear.BoundingBox.Height * source.PixelHeight);
                    var box = new Rectangle
                    { 
                        Width = w,
                        Height = h,
                        Fill = fillBrush,
                        Stroke = lineBrush,
                        StrokeThickness = lineThickness,
                        Margin = new Thickness((uint)(x), (uint)(y), 0, 0)
                    };
                    var tag = new TextBlock
                    {
                        Text = bear.TagName,
                        Margin = new Thickness((uint)(bear.BoundingBox.Left * source.PixelWidth), (uint)(bear.BoundingBox.Top * source.PixelHeight), 0, 0),
                        Foreground = new SolidColorBrush(Colors.Yellow)
                    };
                    this.bdCanvas.Children.Add(box);
                    this.bdCanvas.Children.Add(tag);
                }
            }
        }
        private async void BdRecognize_Click(object sender, RoutedEventArgs e)
        {
            bdResult.Text = string.Empty;
            bdCanvas.Background = null;

            try
            {
                Uri uri = new Uri(bdUri.Text);
                var response = await new HttpClient().GetAsync(uri);
                var stream = (await response.Content.ReadAsStreamAsync()).AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                WriteableBitmap dispalySource = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                softwareBitmap.CopyToBuffer(dispalySource.PixelBuffer);

                var brush = new ImageBrush
                {
                    ImageSource = dispalySource,
                    AlignmentX = 0,
                    AlignmentY = 0,
                    Stretch = Stretch.None
                };
                bdCanvas.Background = brush;
                bdCanvas.Children.Clear();
                VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                var prediction = await od.PredictImageAsync(videoFrame);
                var acceptablePredictions = prediction.Where(p => p.Probability >= 0.5).ToList(); //Only show the ones with more than 50% probability
                DrawBoxes(acceptablePredictions, softwareBitmap);
            }
            catch(Exception ex)
            {
                MessageDialog a = new MessageDialog(String.Format("error: \n{0}", ex.ToString()));
                await a.ShowAsync();
            }
            
        }
    }
}
