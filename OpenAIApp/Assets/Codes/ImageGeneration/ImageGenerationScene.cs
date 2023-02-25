using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImageGenerationScene : MonoBehaviour
{
    public RawImage ImageView;
    public TMP_InputField InputImageDesc;
    public Button ButtonShot;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnButtonBackClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    public async void OnButtonShotClicked()
    {
        this.ButtonShot.interactable = false;
        Texture2D image = null;
        try
        {
            var prompt = this.InputImageDesc.text.Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                return;
            }

            image = await GenerateImage(prompt);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            this.ButtonShot.interactable = true;
            this.RefreshImage(image);
        }
    }

    private void RefreshImage(Texture2D image)
    {
        if (this.ImageView.texture != null)
        {
            Destroy(this.ImageView.texture);
        }

        this.ImageView.texture = image;
        this.ImageView.SetNativeSize();
    }

    static async Task<Texture2D> GenerateImage(string prompt)
    {
        //     curl https://api.openai.com/v1/images/generations \
        //     -H 'Content-Type: application/json' \
        //     -H "Authorization: Bearer $OPENAI_API_KEY" \
        //     -d '{
        //     "prompt": "a white siamese cat",
        //     "n": 1,
        //     "size": "1024x1024"
        //     }'

        try
        {
            var url = "https://api.openai.com/v1/images/generations";
            string apiKey = OpenAIConfig.ApiKey;
        
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var textContent = new StringContent(GetGenerationPromptJson(prompt, 512, 512));
            textContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            var response = await httpClient.PostAsync(url, textContent);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //  { "created" : “1677286725”, "data" : [ { "url" : "imageUrl" } ] }
                var responseJson = await response.Content.ReadAsStringAsync();
                var imageGenerationResponseBody = JsonUtility.FromJson<ImageGenerationResponseBody>(responseJson);
                var imageUrl = imageGenerationResponseBody.data[0].url;
                var imageBytes = await DownloadImage(imageUrl);

                var image = new Texture2D(1, 1);
                image.LoadImage(imageBytes);
                return image;
            }
            
            UnityEngine.Debug.Log(response.StatusCode);
            UnityEngine.Debug.Log(await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        
        return null;
    }
    
    static async Task<byte[]> DownloadImage(string url)
    {
        using var httpClient = new HttpClient();
        var responseMessage = await httpClient.GetAsync(url);
        if (responseMessage.StatusCode == HttpStatusCode.OK)
        {
            return await responseMessage.Content.ReadAsByteArrayAsync();
        }

        return null;
    }

    static string GetGenerationPromptJson(string prompt, int imageWidth, int imageHeight)
    {
        return $"{{ \"prompt\": \"{prompt}\", \"n\": 1, \"size\": \"{imageWidth}x{imageHeight}\" }}";
    }

    [Serializable]
    public class ImageGenerationResponseBody
    {
        [Serializable]
        public class ImageData
        {
            public string url;
        }

        public string created;
        public ImageData[] data;
    }
}
