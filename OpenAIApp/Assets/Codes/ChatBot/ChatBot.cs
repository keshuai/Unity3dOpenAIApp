using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;

namespace ChatBot
{
    // 请求数据结构
    public class OpenAiRequestBody
    {
        public string model = "text-davinci-003";
        public string prompt;
        public float temperature = 0.9f;
        public int max_tokens = 2048;
        public float top_p = 1;
        public float frequency_penalty = 0;
        public float presence_penalty = 0.6f;
        public string stop = "[\" User:\", \" AI:\"]";
    }
    
    //  响应数据结构
    public class OpenAiResponseBody
    {
        [System.Serializable] // 在使用Unity自带的Json工具必须进行标识
        public class Choice
        {
            public string text;
            public int index;
            public string logprobs;
            public string finish_reason;
        }

        public string id;
        public string @object;
        public int created;
        public string model;
        public Choice[] choices;
    }
    
    public class AskAnswerList
    {
        public class AskAnswerPair
        {
            public string UserAsk;
            public string AiAnswer;
        }
     
        private string _title = "我是嘿哈，一只小熊猫，来自银河火箭研发基地的2号黑科技机器人，可以回答各种问题和提供语言交互服务。";
        private readonly List<AskAnswerPair> _historys = new List<AskAnswerPair>();
        private AskAnswerPair _currentAskAnswer = null;
        private StringBuilder _stringBuilder = new StringBuilder();

        public void AddHistory(string userAsk, string aiAnswer)
        {
            _historys.Add(new AskAnswerPair()
            {
                UserAsk = userAsk,
                AiAnswer = aiAnswer,
            });
        }

        public void SetCurrentUserAsk(string userAsk)
        {
            if (_currentAskAnswer == null)
            {
                _currentAskAnswer = new AskAnswerPair();
            }

            _currentAskAnswer.UserAsk = userAsk;
        }
        
        public void SetCurrentAiAnswer(string aiAnswer)
        {
            if (_currentAskAnswer == null)
            {
                throw new Exception("aiAnswer null");
            }

            _currentAskAnswer.AiAnswer = aiAnswer;
        }

        public void FinishCurrentSay()
        {
            if (_currentAskAnswer == null)
            {
                return;
            }

            _historys.Add(_currentAskAnswer);
            _currentAskAnswer = null;
        }

        public string ToPrompt()
        {
            _stringBuilder.Clear();
            _stringBuilder.AppendLine(_title);
            _stringBuilder.AppendLine();
            foreach (var pair in _historys)
            {
                _stringBuilder.AppendLine("User:");
                _stringBuilder.AppendLine(pair.UserAsk);
                _stringBuilder.AppendLine("Ai:");
                _stringBuilder.AppendLine(pair.AiAnswer);
            }

            if (_currentAskAnswer != null)
            {
                _stringBuilder.AppendLine("User:");
                _stringBuilder.AppendLine(_currentAskAnswer.UserAsk);
                _stringBuilder.AppendLine("Ai:");
            }

            return _stringBuilder.ToString();
        }
    }

    public class Conversation
    {
        // https://platform.openai.com/docs/api-reference/edits/create
        const string url = "https://api.openai.com/v1/completions";
        string apiKey = OpenAIConfig.ApiKey;

        private HttpClient _httpClient;

        private readonly AskAnswerList _askAnswerList = new AskAnswerList();

        public Conversation()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> Ask(string ask)
        {
            var answer = "";

            try
            {
                _askAnswerList.SetCurrentUserAsk(ask);
                answer = await this.GetAnswer(ask);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                answer = e.Message;
                UnityEngine.Debug.LogException(e);
            }

            _askAnswerList.SetCurrentAiAnswer(answer);
            _askAnswerList.FinishCurrentSay();
            return answer;
        }

        private async Task<string> GetAnswer(string ask)
        {
            var prompt = _askAnswerList.ToPrompt();
            //Console.WriteLine($"网络发送: {prompt}");

            var openAiRequestBody = new OpenAiRequestBody() { prompt = prompt };
            //var openAiRequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(openAiRequestBody);
            var openAiRequestJson = UnityEngine.JsonUtility.ToJson(openAiRequestBody);
            var httpContent = new StringContent(openAiRequestJson);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var httpResponseMessage = await _httpClient.PostAsync(url, httpContent); // 如果这里报错，请将计算机名改为英语，Unity自带的BUG

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
                //Console.WriteLine($"收到网络回复：{responseJson}");

                //var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenAiResponseBody>(responseJson);
                var responseObj = UnityEngine.JsonUtility.FromJson<OpenAiResponseBody>(responseJson);

                // 结果为空
                if (responseObj.choices == null || responseObj.choices.Length == 0)
                {
                    return "";
                }

                // 设返回结果
                return responseObj.choices[0].text;
            }

            // 处理错误信息
            var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();
            Console.WriteLine($"网络请求异常，状态码：{httpResponseMessage.StatusCode}，错误信息：{errorMessage}");
            throw new Exception(errorMessage);
        }
    }
}

