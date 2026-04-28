using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class llm : MonoBehaviour
{
    [Header("GLM 配置")]
    [Tooltip("在 Inspector 手动填写你的 GLM API Key")]
    public string apiKey = "";

    [Tooltip("免费模型默认使用 glm-4-flash")]
    public string modelName = "glm-4-flash";

    [Header("人设配置")]
    [TextArea(2, 6)]
    [Tooltip("这里填写 chatbot 人格，比如：毒舌女王 / 温柔偶像 / 病娇 AI")]
    public string personaPrompt = "";

    [Header("生成参数")]
    [Range(0f, 1.2f)]
    public float temperature = 0.9f;

    private const string GlmEndpoint = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    public void OnBallEliminated(int scoreValue, float intervalSeconds)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.LogWarning("[Chatbot] 尚未填写 GLM API Key（Inspector 中 llm.apiKey）。");
            return;
        }

        string moodInstruction = BuildMoodByInterval(intervalSeconds);
        string persona = string.IsNullOrWhiteSpace(personaPrompt) ? "你是一个会根据玩家表现变换语气的虚拟陪伴者。" : personaPrompt.Trim();

        string systemPrompt =
            "你是一个球消除游戏里的实时语音搭子（但这里只输出文本到控制台）。" +
            "每次收到玩家消除信息，都回复一句短评（8~24字），必须中文，风格鲜明，禁止解释规则。" +
            "人设：" + persona + "。" +
            "语气要求：" + moodInstruction;

        string userPrompt =
            $"玩家刚消除了一个球。当前累计得分：{scoreValue}。距离上一次消除间隔：{intervalSeconds:F2} 秒。请给一句回应。";

        StartCoroutine(SendChat(systemPrompt, userPrompt));
    }

    private string BuildMoodByInterval(float intervalSeconds)
    {
        if (intervalSeconds >= 8f)
        {
            return "偏嘲讽、挖苦但别过线，像嘴硬教练。";
        }

        if (intervalSeconds >= 4f)
        {
            return "带一点调侃和催促，半鼓励半嫌弃。";
        }

        if (intervalSeconds >= 2f)
        {
            return "积极鼓励、认可玩家状态。";
        }

        return "明显偏爱慕和夸夸，语气热烈。";
    }

    private IEnumerator SendChat(string systemPrompt, string userPrompt)
    {
        string mergedUserPrompt = systemPrompt + "\n\n" + userPrompt;
        ChatCompletionRequest requestData = new ChatCompletionRequest
        {
            model = string.IsNullOrWhiteSpace(modelName) ? "glm-4-flash" : modelName.Trim(),
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "user", content = mergedUserPrompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(GlmEndpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + apiKey.Trim());

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[Chatbot] GLM 请求失败: " + req.error + "\n" + req.downloadHandler.text + "\nRequest: " + json);
                yield break;
            }

            string raw = req.downloadHandler.text;
            ChatCompletionResponse response = null;
            try
            {
                response = JsonUtility.FromJson<ChatCompletionResponse>(raw);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Chatbot] 解析响应失败: " + ex.Message + "\nRaw: " + raw);
                yield break;
            }

            if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0].message == null)
            {
                Debug.LogWarning("[Chatbot] 响应为空或结构不匹配。\nRaw: " + raw);
                yield break;
            }

            string text = response.choices[0].message.content;
            Debug.Log("[Chatbot] " + text);
        }
    }

    [Serializable]
    private class ChatCompletionRequest
    {
        public string model;
        public ChatMessage[] messages;
    }

    [Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatCompletionResponse
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    private class ChatChoice
    {
        public ChatMessage message;
    }
}