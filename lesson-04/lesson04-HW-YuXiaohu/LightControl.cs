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
        const string speechKey = "5f6a0de33f3a4f2093a368ff810cf46e";
        const string speechRegion = "westus";

        // 设置语言理解服务地址
        // 示例: https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/102f6255-0c32-4f46-9c79-fe12fea4d6c4?subscription-key=8004421650254a74875cf3c888b1d11f&verbose=true&timezoneOffset=0&q=
        // 可在 https://www.luis.ai 中进入app右上角publish中找到
        const string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/f3243736-e422-440a-9c1a-285f037cc076?verbose=true&timezoneOffset=-360&subscription-key=68b13988244e41cf996be7f8ddf93c7c&q=";

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
            string intent = await GetLuisResult(text);

            // 按照意图控制灯
            if (!string.IsNullOrEmpty(intent))
            {
                if (intent.Equals("TurnOnDL", StringComparison.OrdinalIgnoreCase))
                {
                    OpenLight();
                    Log("台灯开 " + "\r\n");
                }
                else if (intent.Equals("TurnOnBL", StringComparison.OrdinalIgnoreCase))
                {
                    OpenLight();
                    Log("床头灯开 " + "\r\n");
                }
                else if (intent.Equals("TurnOnRL", StringComparison.OrdinalIgnoreCase))
                {
                    OpenLight();
                    Log("顶灯开 " + "\r\n");
                }
                else if (intent.Equals("TurnOnDBL", StringComparison.OrdinalIgnoreCase))
                {
                    OpenLight();
                    Log("台灯、床头灯开 " + "\r\n");
                }
                else if (intent.Equals("AllTurnOn", StringComparison.OrdinalIgnoreCase))
                {
                    OpenLight();
                    Log("三灯全开 " + "\r\n");
                }
                else if (intent.Equals("TurnOffDL", StringComparison.OrdinalIgnoreCase))
                {
                    CloseLight();
                    Log("台灯关 " + "\r\n");
                }
                else if (intent.Equals("TurnOffBL", StringComparison.OrdinalIgnoreCase))
                {
                    CloseLight();
                    Log("床头灯关 " + "\r\n");
                }
                else if (intent.Equals("TurnOffRL", StringComparison.OrdinalIgnoreCase))
                {
                    CloseLight();
                    Log("顶灯关 " + "\r\n");
                }
                else if (intent.Equals("TurnOffDBL", StringComparison.OrdinalIgnoreCase))
                {
                    CloseLight();
                    Log("台灯、床头灯关 " + "\r\n");
                }
                else if (intent.Equals("AllTurnOff", StringComparison.OrdinalIgnoreCase))
                {
                    CloseLight();
                    Log("三灯全关 " + "\r\n");
                }
                else if (intent.Equals("Recall", StringComparison.OrdinalIgnoreCase))
                {
                    Log("对不起，请重新下达命令 " + "\r\n");
                }
            }
        }

        private async Task<string> GetLuisResult(string text)
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
                    string intentTmp = (string)result.topScoringIntent.intent;
                    double score = (double)result.topScoringIntent.score;
                    Log("意图: " + intentTmp + "\r\n得分: " + score + "\r\n");
                    string intent = null;
                    
                   // if (result.entities < 0)
                   // {
                     //   Log("chenglie: " + intentTmp + "\r\n得分: " + score + "\r\n");
                   // }

                    if ((intentTmp == "TurnOn" | intentTmp == "TurnOff") && result.entities != null && result.entities.Count > 0)
                    {
                        string role = (string)result.entities[0].role;
                        // string type = (string)entity_dict["type"];
                        string type = (string)result.entities[0].type;
                        if (role == "台灯")
                        {
                            intent = intentTmp + "DL";
                        }
                        else if (role == "床头灯")
                        {
                            intent = intentTmp + "BL";
                        }
                        else if (role == "顶灯")
                        {
                            intent = intentTmp + "RL";
                        }
                        else if (role == "客厅")
                        {
                            intent = intentTmp + "RL";
                        }
                        else if (role == "卧室")
                        {
                            intent = intentTmp + "DBL";
                        }
                        else
                        {
                            intent = "Recall";
                        }
                    }
                    else if ((intentTmp == "AllTurnOn" | intentTmp == "AllTurnOff")  && score > 0.6)
                    {
                        intent = intentTmp;
                    }
                    else
                    {
                        intent = "Recall";
                    }


                        //Log("entities: " + result + "\r\n得分: " + score + "\r\n");
                        return intent;
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

        private void OpenLight()
        {
            MakesureRunInUI(() =>
            {
                pictureBox1.Load("LightOn.png");
            });
            //Log("开灯 ");
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
    }
}
