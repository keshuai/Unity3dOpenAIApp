using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatBotScene : MonoBehaviour
{
    public TMP_InputField InputAskAnsawerList;
    public TMP_InputField InputAsk;
    public Button ButtonShot;

    private readonly StringBuilder _askAnswers = new StringBuilder();
    
    ChatBot.Conversation _conversation = new ChatBot.Conversation();

    public void OnButtonBackClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    public async void OnButtonShotClicked()
    {
        this.ButtonShot.interactable = false;
        try
        {
            var ask = this.InputAsk.text.Trim();
            if (string.IsNullOrEmpty(ask))
            {
                return;
            }

            _askAnswers.AppendLine("[问]");
            _askAnswers.AppendLine(ask);
            this.RefreshAskAnswersView();
            
            var answer = await _conversation.Ask(ask);
            UnityEngine.Debug.Log(answer);
            _askAnswers.AppendLine("[答]");
            _askAnswers.AppendLine(answer);
            _askAnswers.AppendLine();
            
            this.RefreshAskAnswersView();
        }
        catch
        {
        }
        finally
        {
            this.ButtonShot.interactable = true;
            this.RefreshAskAnswersView();
        }
    }

    private void RefreshAskAnswersView()
    {
        this.InputAskAnsawerList.text = this._askAnswers.ToString();
        this.InputAskAnsawerList.stringPosition = this.InputAskAnsawerList.text.Length - 1;
    }
}
