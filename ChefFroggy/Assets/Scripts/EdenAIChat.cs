using EdenAI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EdenAIChat : MonoBehaviour
{
    public Text dialogueText;
    public InputField playerInput;
    public Button submitButton;

    private EdenAIApi edenAI;

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
        edenAI = new EdenAIApi("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNTlmYmYwOWYtMGI4NC00ZWE1LWE1NTktODZhMWEyMGEyMzU2IiwidHlwZSI6ImFwaV90b2tlbiJ9.nIjfmCKwG-uGyOdEzpgHZsexPbHsdF3BCEkOH2ZPuTQ");
    }

    void OnSubmit()
    {
        string playerResponse = playerInput.text;
        StartCoroutine(SendChatRequest(playerResponse));
    }

    IEnumerator SendChatRequest(string prompt)
    {
        Task<ChatResponse> chatTask = edenAI.SendChatRequest("openai", prompt);
        yield return new WaitUntil(() => chatTask.IsCompleted);

        if (chatTask.Exception != null)
            Debug.LogError(chatTask.Exception);
        else
        {
            ChatResponse response = chatTask.Result;
            dialogueText.text = response.generated_text;
        }
    }
}
