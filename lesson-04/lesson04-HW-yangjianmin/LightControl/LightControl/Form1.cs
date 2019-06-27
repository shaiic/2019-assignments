using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightControl
{
    [Flags]
    enum Light {
        None = 0,
        Table = 1,
        Top = 2,
        Bed = 3,
        All = 99
    }
    enum Place
    {
        None = 0,
        Bedroom = 1,
        Livingroom = 2,
        All = 99
    }
    struct LightMap
    {
        public Light light;
        public Place place;
    };

    public partial class Form1 : Form
    {
        private void SwitchLight(LightMap lightMap, string pic)
        {
            MakesureRunInUI(() =>
            {            
                if(lightMap.place == Place.All && lightMap.light == Light.All)
                {
                    Toplightbox.LoadAsync(pic);
                    Bedlightbox.LoadAsync(pic);
                    Tablelightbox.LoadAsync(pic);
                }
                else if(lightMap.place == Place.Bedroom && lightMap.light == Light.All)
                {
                    Bedlightbox.LoadAsync(pic);
                    Tablelightbox.LoadAsync(pic);
                }
                else if (lightMap.place == Place.Livingroom && lightMap.light == Light.All)
                {
                    Toplightbox.LoadAsync(pic);
                }
                else if (lightMap.light == Light.Top)
                {
                    Toplightbox.LoadAsync(pic);
                }

                else if (lightMap.light == Light.Bed)
                {
                    Bedlightbox.LoadAsync(pic);
                }

                else if (lightMap.light == Light.Table)
                {
                    Tablelightbox.LoadAsync(pic);
                }
            });            
        }

        private void TurnOn(LightMap lightMap)
        {
            SwitchLight(lightMap, "LightOn.png");
        }

        private void TurnOff(LightMap lightMap)
        {
            SwitchLight(lightMap, "LightOff.png");
        }

        public Form1()
        {
            InitializeComponent();
            Switch.Text = "开始";

            LightMap lightMap;
            lightMap.light = Light.All;
            lightMap.place = Place.All;

            TurnOff(lightMap);
        }

        // 设置服务信息
        //宋老师的
        //const string speechKey = "ae9be3ac79434d5e828ea0b443400170";
        //const string speechRegion = "westus";
        //const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/99ba0b18-fdb9-4773-a908-9c1811037694?verbose=true&timezoneOffset=-360&subscription-key=171ecbac277142d7bd1ba1dc316eb976&q=";

        //我的
        const string speechKey = "3f476ec680884eb5b7a71df28f1fc90e";
        const string speechRegion = "westus";
        const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/6021af12-0bd9-4fba-a088-77730ccf99bf?verbose=true&timezoneOffset=-360&subscription-key=5872e34bcb2f4af9ab970f61b5959320&q=";

        //// 语音识别器
        SpeechRecognizer recognizer;
        bool isRecording = false;

        // 获得音频分析后的文本内容
        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Log("最终结果: " + e.Result.Text);
                ProcessSttResult(e.Result.Text);
            }
        }

        // 识别过程中的中间结果
        private void Recognizer_Recognizing(object sender, SpeechRecognitionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result.Text))
            {
                Log("中间结果: " + e.Result.Text);
            }
        }

        private async void ProcessSttResult(string text)
        {
            Log("ProcessSttResult():"+text);
            // 调用语言理解服务取得用户意图
            var json = await GetLuisResult(text);
            if(json == null)
            {
                return;
            }

            var intent = (string)json.topScoringIntent.intent;

            // 按照意图控制灯
            if (!string.IsNullOrEmpty(intent))
            {
                List<LightMap> lightMapList = ParseJsonLights(json);
                if (intent.Equals("TurnOn", StringComparison.OrdinalIgnoreCase))
                {
                    lightMapList.ForEach(lightMap =>
                    {
                        TurnOn(lightMap);
                    });
                    
                }
                else if (intent.Equals("TurnOff", StringComparison.OrdinalIgnoreCase))
                {
                    lightMapList.ForEach(lightMap =>
                    {
                        TurnOff(lightMap);
                    });
                }
            }
        }

        private async Task<dynamic> GetLuisResult(string text)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string luisJson = await httpClient.GetStringAsync(luisEndpoint + text);

                try
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(luisJson);
                    if (result.topScoringIntent == null)
                    {
                        return null;
                    }
                    string intent = (string)result.topScoringIntent.intent;
                    double score = (double)result.topScoringIntent.score;
                    Log("意图: " + intent + "\r\n得分: " + score + "\r\n");

                    return result;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    return null;
                }
            }
        }

        private async Task Initialize()
        {
            try
            {
                Log("Initialize() start-----------------------");
                var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig.SpeechRecognitionLanguage = "zh-CN";

                // 设置识别中文
                recognizer = new SpeechRecognizer(speechConfig);

                // 挂载识别中的事件

                // 收到中间结果
                recognizer.Recognizing += Recognizer_Recognizing;
                // 收到最终结果
                recognizer.Recognized += Recognizer_Recognized;
                
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(true);
                Log("Initialize() end-----------------------");
            }
            catch (Exception ex)
            {
                if (ex is System.TypeInitializationException)
                {
                    Log("类型初始化时遇到异常：{0}\r\n请确认项目在x64模式下编译，而不是Any CPU。", ex.ToString());
                }
                else
                {
                    Log("异常：{0}\r\n初始化出错，请确认麦克风正常。", ex.ToString());
                    Log("已降级到文本语言理解模式");

                    TextBox inputBox = new TextBox();
                    inputBox.Text = "";
                    inputBox.Size = new Size(300, 26);
                    inputBox.Location = new Point(10, 10);
                    inputBox.KeyDown += inputBox_KeyDown;
                    Controls.Add(inputBox);

                    Switch.Visible = false;
                }
            }

        }

        private async Task UnloadRecognizer()
        {
            await recognizer.StopContinuousRecognitionAsync();
            // 收到中间结果
            recognizer.Recognizing -= Recognizer_Recognizing;
            // 收到最终结果
            recognizer.Recognized -= Recognizer_Recognized;
            recognizer.Dispose();
            recognizer = null;
        }

        #region 界面操作

        private void Log(string message, params string[] parameters)
        {
            MakesureRunInUI(() =>
            {
                if (parameters != null && parameters.Length > 0)
                {
                    message = string.Format(message + "\r\n", parameters);
                }
                else
                {
                    message += "\r\n";
                }
                this.Message.AppendText(message);
            });
        }         

        private void MakesureRunInUI(Action action)
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(action);
                Invoke(action, null);
            }
            else
            {
                action();
            }
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && sender is TextBox)
            {
                TextBox textBox = sender as TextBox;
                e.Handled = true;
                Log(textBox.Text);
                ProcessSttResult(textBox.Text);
                textBox.Text = string.Empty;
            }
        }

        #endregion        

        private List<LightMap> ParseJsonLights(dynamic json) 
        {
            Log("ParseJsonLights():" + json);
            LightMap lightMap;
            List<LightMap> lightMapList = new List<LightMap>();

            Light light = Light.None;
            Place place = Place.None;
            var entities = (JArray)json.entities;
            Log("entities:"+ entities.ToString());
            if(entities != null)
            {
                entities.ToList().ForEach(ele =>
                {

                    var item = (dynamic)ele;
                    var value = (string)item.entity;
                    var type = (string)item.type;
                    if(type == "light" && value.ToString().Length>1)
                    {
                        var resolvedValues = (JArray)item.resolution.values;
                        resolvedValues.ToList().ForEach(r =>
                        {
                            var v = (dynamic)r;
                            switch (v.ToString().ToUpperInvariant())
                            {
                                case "TOPLIGHT":
                                    light = Light.Top;
                                    place = Place.Livingroom;
                                    break;
                                case "BEDLIGHT":
                                    light = Light.Bed;
                                    place = Place.Bedroom;
                                    break;
                                case "TABLELIGHT":
                                    light = Light.Table;
                                    place = Place.Bedroom;
                                    break;
                            }
                            lightMap.light = light;
                            lightMap.place = place;
                            lightMapList.Add(lightMap);
                        });

                    }
                    if (type == "place")
                    {
                        var resolvedValues = (JArray)item.resolution.values;
                        resolvedValues.ToList().ForEach(r =>
                        {
                            var v = (dynamic)r;
                            switch (v.ToString().ToUpperInvariant())
                            {
                                case "BEDROOM":
                                    light = Light.All;
                                    place = Place.Bedroom;
                                    break;
                                case "LIVINGROOM":
                                    light = Light.All;
                                    place = Place.Livingroom;
                                    break;
                            }
                            lightMap.light = light;
                            lightMap.place = place;
                            lightMapList.Add(lightMap);
                        });
                    }
                });
            }
            
            return lightMapList;
        }

        private async void testBtn_Click(object sender, EventArgs e)
        {
            Switch.Enabled = false;

            isRecording = !isRecording;
            if (isRecording)
            {
                // 启动识别器
                await Initialize();                
                Switch.Text = "停止";
            }
            else
            {
                // 停止识别器
                await UnloadRecognizer();
                Log("end------------------");
                Switch.Text = "开始";
            }

            Switch.Enabled = true;
        }
    }
}
