using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnButtonChatBotClicked()
    {
        SceneManager.LoadScene("ChatBot");
    }
    
    public void OnButtonImageGenerationClicked()
    {
        SceneManager.LoadScene("ImageGeneration");
    }
}
