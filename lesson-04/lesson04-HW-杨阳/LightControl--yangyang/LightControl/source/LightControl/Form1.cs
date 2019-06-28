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

namespace LightControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.Text = "开始";
        }

        // 设置服务信息

        // 设置语音服务密钥及区域
        // 可在 https://azure.microsoft.com/zh-cn/try/cognitive-services/my-apis/?api=speech-services 中找到
        // 密钥示例: 5ee7ba6869f44321a40751967accf7b9
        // 区域示例: westus
        const string speechKey = "ae9be3ac79434d5e828ea0b443400170";
        const string speechRegion = "westus";

        // 设置语言理解服务地址
        // 示例: https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/102f6255-0c32-4f46-9c79-fe12fea4d6c4?subscription-key=8004421650254a74875cf3c888b1d11f&verbose=true&timezoneOffset=0&q=
        // 可在 https://www.luis.ai 中进入app右上角publish中找到
        const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/99ba0b18-fdb9-4773-a908-9c1811037694?verbose=true&timezoneOffset=-360&subscription-key=171ecbac277142d7bd1ba1dc316eb976&q=";

        const string off = "关闭";
        const string on = "打开";

        //// 语音识别器
        SpeechRecognizer recognizer;
        bool isRecording = false;

        SortedDictionary<string, string> lightStates = new SortedDictionary<string, string>();
        Dictionary<string, string[]> placeMapping = new Dictionary<string, string[]>();

        private void Form1_Load(object sender, EventArgs e)
        {
            lightStates["台灯"] = off;
            lightStates["床头灯"] = off;
            lightStates["顶灯"] = off;

            placeMapping["卧室"] = new string[] { "台灯", "床头灯" };
            placeMapping["客厅"] = new string[] { "顶灯" };

            UpdateLightStatesUI();
        }


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
            using (HttpClient httpClient = new HttpClient())
            {
                string luisJson = await httpClient.GetStringAsync(luisEndpoint + text);

                try
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(luisJson);
                    if (result.topScoringIntent == null)
                    {
                        return;
                    }
                    string intent = (string)result.topScoringIntent.intent;
                    double score = (double)result.topScoringIntent.score;
                    Log(result.ToString());

                    string state = "Off";
                    bool needEntity = false;

                    // 按照意图控制灯
                    if (!string.IsNullOrEmpty(intent))
                    {
                        switch (intent.ToUpperInvariant())
                        {
                            case "TURNON":
                                state = on;
                                needEntity = true;
                                break;
                            case "TURNOFF":
                                state = off;
                                needEntity = true;
                                break;
                            case "TURNOFFALL":
                                state = off;
                                var keys = lightStates.Keys.ToArray();
                                foreach (var key in keys)
                                {
                                    lightStates[key] = state;
                                }
                                break;
                            default:
                                Log(string.Format("unknown intent {0}", intent));
                                break;
                        }
                    }

                    if (needEntity && result.entities != null)
                    {
                        List<string> light = new List<string>();
                        List<string> place = new List<string>();
                        foreach (var item in result.entities)
                        {
                            switch (item.type.ToString().ToUpperInvariant())
                            {
                                case "LIGHT":
                                    light.Add(item.entity.ToString());
                                    break;
                                case "PLACE":
                                    place.Add(item.entity.ToString());
                                    break;
                                default:
                                    Log("unknown engity type: {0}", item.type);
                                    break;
                            }
                        }

                        if (light.Count > 0)
                        {
                            foreach (var item in light)
                            {
                                lightStates[item] = state;
                            }
                        }
                        else if (place.Count > 0)
                        {
                            foreach (var itemPlace in place)
                            {
                                foreach (var itemLight in placeMapping[itemPlace])
                                {
                                    lightStates[itemLight] = state;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
            }
            UpdateLightStatesUI();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            isRecording = !isRecording;
            if (isRecording)
            {
                // 启动识别器
                await Initialize();
                button1.Text = "停止";
            }
            else
            {
                // 停止识别器
                await UnloadRecognizer();
                button1.Text = "开始";
            }

            button1.Enabled = true;
        }

        private async Task Initialize()
        {
            try
            {
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

                    button1.Visible = false;
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
                textBox1.AppendText(message);
            });
        }

        private void UpdateLightStatesUI()
        {
            MakesureRunInUI(() =>
            {
                StringBuilder sbStates = new StringBuilder();
                foreach (var light in lightStates)
                {
                    sbStates.AppendLine(string.Format("{0}: {1}", light.Key, light.Value));
                }
                lbState.Text = sbStates.ToString();
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
    }
}
