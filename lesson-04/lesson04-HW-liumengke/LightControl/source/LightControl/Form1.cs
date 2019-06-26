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
            pictureBox1.Load("LightOff.png");
        }

        // 设置服务信息

        // 设置语音服务密钥及区域
        // 可在 https://azure.microsoft.com/zh-cn/try/cognitive-services/my-apis/?api=speech-services 中找到
        // 密钥示例: 5ee7ba6869f44321a40751967accf7b9
        // 区域示例: westus
        const string speechKey = "9cca1a5d82f84080958fe4abc0174762";
        const string speechRegion = "westus";

        // 设置语言理解服务地址
        // 示例: https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/102f6255-0c32-4f46-9c79-fe12fea4d6c4?subscription-key=8004421650254a74875cf3c888b1d11f&verbose=true&timezoneOffset=0&q=
        // 可在 https://www.luis.ai 中进入app右上角publish中找到
        //const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/820a7679-62fb-4dc8-8f5f-d5509c42c0a5?verbose=true&timezoneOffset=-360&subscription-key=90087c81ce6e42418dce90bc85c5f44c&q=";
        const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/4ab296c8-41dd-4c4e-a827-555ccdf4b5e6?verbose=true&timezoneOffset=-360&subscription-key=90087c81ce6e42418dce90bc85c5f44c&q=";
        //// 语音识别器
        SpeechRecognizer recognizer;
        bool isRecording = false;

        private void Form1_Load(object sender, EventArgs e)
        {
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
            // 调用语言理解服务取得用户意图
            Dictionary<string, string >  res = await GetLuisResult(text);



            // 按照意图控制灯
            if (res != null && res.ContainsKey("intent") && res.ContainsKey("fangjian") && res.ContainsKey("light")) {
                if (res["intent"] == "TurnOn")
                {
                    OpenLightSpec(res["fangjian"], res["light"]);
                }
                else if (res["intent"] == "TurnOff") {
                    CloseLightSpec(res["fangjian"], res["light"]);
                }

            }


        }

        private async Task<Dictionary<string, string>> GetLuisResult(string text)
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

                    Dictionary<string, string> ret = new Dictionary<string, string>();

                    string intent = (string)result.topScoringIntent.intent;
                    double score = (double)result.topScoringIntent.score;
                    Log("意图: " + intent + "\r\n得分: " + score + "\r\n");

                    ret.Add("intent", intent);

                    if (result.entities != null)
                    {
                        string type1 = (string)result.entities[0].type;
                        string val1 = (string)result.entities[0].entity;

                        ret.Add(type1, val1);                       
                        if (result.entities.Count >= 1) {
                            string type2 = (string)result.entities[1].type;
                            string val2 = (string)result.entities[1].entity;
                            ret.Add(type2, val2);
                        }
                    }
                    //return intent;
                    return ret;
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    return null;
                }
            }
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

        private void OpenLightSpec(string fangjian, string light)
        {
            MakesureRunInUI(() =>
            {
                string outputMesg = "房间" + fangjian + "的" + light + "开了";
                textBox2.Text = outputMesg;
                pictureBox1.Load("LightOn.png");
            });
        }

        private void CloseLightSpec(string fangjian, string light)
        {
            MakesureRunInUI(() =>
            {
                string outputMesg = "房间" + fangjian + "的" + light + "关了";
                textBox2.Text = outputMesg;
                pictureBox1.Load("LightOff.png");
            });
        }

        private void OpenLight()
        {
            MakesureRunInUI(() =>
            {
                pictureBox1.Load("LightOn.png");
            });
        }

        private void CloseLight()
        {
            MakesureRunInUI(() =>
            {
                pictureBox1.Load("LightOff.png");
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
