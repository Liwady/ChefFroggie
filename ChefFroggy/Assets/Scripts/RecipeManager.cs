/****************************************************/
// Filename: RecipeManager.cs
// Created by: Liwady Verbaendert
/****************************************************/

/*
 * RecipeManager.cs
 * 
 * This script manages the interaction between the player and Chef Froggy, an AI-powered chef assistant.
 * The player is guided through various recipes step by step, with Chef Froggy providing instructions and 
 * responding to player input. This script utilizes the EdenAI API to generate AI responses based on player choices.
 */

/****************************************************/
// EOF: RecipeManager.cs
/****************************************************/

using EdenAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Recipe
{
    public string name;
    public List<RecipeStep> steps;
}

[System.Serializable]
public class RecipeStep
{
    public string instruction;
}

public class RecipeManager : MonoBehaviour
{
    #region Variables
    public List<Recipe> recipes;
    public Text chatText;
    public InputField playerInput;
    public Text recapText;
    public Button submitButton;
    public Button recapButton;

    private int currentStep;
    private Recipe currentRecipe;
    private EdenAIApi edenAI;
    private List<ChatMessage> conversationHistory;
    private List<string> playerChoices;

    private const string CONTEXT = "You are Chef Froggy, a friendly frog who is a skilled chef in a charming forest restaurant. Your goal is to guide players through various recipes and cooking steps, providing helpful instructions and ensuring a delightful culinary experience. Keep your responses short and engaging, focusing on the player's input and moving to the next step. You also try to refrain from using Great Choice or Let's get cooking! You NEVER give full recipes/direct instructions or ask the customer questions!";
    #endregion

    #region Unity Methods

    /// <summary>
    ///     Start is called before the first frame update.
    ///     Initializes the AI, recipes, and conversation history.
    /// </summary>
    void Start()
    {
        edenAI = new EdenAIApi("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNTlmYmYwOWYtMGI4NC00ZWE1LWE1NTktODZhMWEyMGEyMzU2IiwidHlwZSI6ImFwaV90b2tlbiJ9.nIjfmCKwG-uGyOdEzpgHZsexPbHsdF3BCEkOH2ZPuTQ");
        InitializeRecipes();
        InitializeConversationHistory();
        playerChoices = new List<string>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    ///     Sets the current recipe based on the selected index and initiates the first step.
    /// </summary>
    /// <param name="recipeIndex">
    ///     Index of the selected recipe.
    /// </param>
    public void SetCurrentRecipe(int recipeIndex)
    {
        currentRecipe = recipes[recipeIndex];
        currentStep = 0;
        chatText.text = "";
        recapText.text = "";
        conversationHistory = new List<ChatMessage>();
        playerChoices = new List<string>();
        ShowNextStep();
    }

    /// <summary>
    ///     Initiates the recap process after the recipe steps are completed.
    /// </summary>
    public void GoToRecap()
    {
        recapButton.gameObject.SetActive(false);
        StartCoroutine(ShowRecap());
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     Initializes the list of recipes.
    /// </summary>
    private void InitializeRecipes()
    {
        recipes = new List<Recipe>
        {
            lilyPadPancakes,
            flyLiciousFruitSalad,
            tadpoleTacos,
            frogLeggedSpaghetti
        };
    }

    /// <summary>
    ///     Initializes the conversation history with the initial context message.
    /// </summary>
    private void InitializeConversationHistory()
    {
        conversationHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Message = CONTEXT }
        };
    }

    /// <summary>
    ///     Displays the next step in the current recipe.
    /// </summary>
    private void ShowNextStep()
    {
        if (currentStep < currentRecipe.steps.Count)
            ChefFroggyQuestion(currentRecipe.steps[currentStep]);
        else
            StartRecap();
    }

    /// <summary>
    ///     Prepares the UI for the recap process.
    /// </summary>
    private void StartRecap()
    {
        recapButton.gameObject.SetActive(true);
        playerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
    }

    /// <summary>
    ///     Displays Chef Froggy's question for the current recipe step.
    /// </summary>
    /// <param name="step">
    ///     The current recipe step.
    /// </param>
    private void ChefFroggyQuestion(RecipeStep step)
    {
        AppendChatMessage($"Chef Froggy: {step.instruction}");
        playerInput.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
        submitButton.onClick.AddListener(OnSubmit);
    }

    /// <summary>
    ///     Handles the player's response submission.
    /// </summary>
    private void OnSubmit()
    {
        string playerResponse = playerInput.text;
        playerInput.text = "";
        playerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        submitButton.onClick.RemoveListener(OnSubmit);
        playerChoices.Add(playerResponse);
        AppendChatMessage($"Player: {playerResponse}");
        StartCoroutine(ProcessResponse(playerResponse));
    }

    /// <summary>
    ///     Processes the player's response and generates Chef Froggy's next response.
    /// </summary>
    /// <param name="playerResponse">
    ///     The player's response text.
    /// </param>
    private IEnumerator ProcessResponse(string playerResponse)
    {
        var userMessage = new ChatMessage { Role = "user", Message = playerResponse };
        conversationHistory.Add(userMessage);

        string currentInstruction = currentRecipe.steps[currentStep].instruction;
        string fullPrompt = $"{CONTEXT}\n\nChef Froggy: {currentInstruction}\nPlayer: {playerResponse}\nChef Froggy:";

        Task<ChatResponse> chatTask = edenAI.SendChatRequest("openai", fullPrompt);
        yield return new WaitUntil(() => chatTask.IsCompleted);

        if (chatTask.Exception != null)
            Debug.LogError(chatTask.Exception);
        else
        {
            ChatResponse response = chatTask.Result;
            var aiMessage = new ChatMessage { Role = "assistant", Message = response.generated_text };
            conversationHistory.Add(aiMessage);

            AppendChatMessage($"Chef Froggy: {response.generated_text}");

            currentStep++;
            if (currentStep < currentRecipe.steps.Count)
                ChefFroggyQuestion(currentRecipe.steps[currentStep]);
            else
                StartRecap();
        }
    }

    /// <summary>
    ///     Generates and displays a recap of the player's choices.
    /// </summary>
    private IEnumerator ShowRecap()
    {
        string summaryPrompt = $"{CONTEXT}\n\nHere are the choices the player made:\n";

        for (int i = 0; i < currentRecipe.steps.Count; i++)
            summaryPrompt += $"{currentRecipe.steps[i].instruction} Player chose: {playerChoices[i]}. ";

        summaryPrompt += "\nChef Froggy, please provide a friendly and engaging summary of the player's choices.";

        Task<ChatResponse> summaryTask = edenAI.SendChatRequest("openai", summaryPrompt);
        yield return new WaitUntil(() => summaryTask.IsCompleted);

        if (summaryTask.Exception != null)
            Debug.LogError(summaryTask.Exception);
        else
        {
            ChatResponse summaryResponse = summaryTask.Result;
            AppendChatMessage($"Chef Froggy: {summaryResponse.generated_text}\n", true);
            GameManager.instance.CompleteRecipe();
        }
    }

    /// <summary>
    ///     Appends a new message to the chat display.
    /// </summary>
    /// <param name="message">
    ///     The message to display.
    /// </param>
    /// <param name="recap">
    ///     Whether the message is part of the recap.
    /// </param>
    private void AppendChatMessage(string message, bool recap = false)
    {
        if (recap)
            recapText.text += message + "\n";
        else
            chatText.text += message + "\n";
    }

    #endregion

    #region Recipe Definitions

    // Recipe definitions
    private Recipe lilyPadPancakes = new Recipe
    {
        name = "Lily Pad Pancakes",
        steps = new List<RecipeStep>
        {
            new RecipeStep { instruction = "Let's start with the flour. Do you prefer a lighter or denser pancake?" },
            new RecipeStep { instruction = "Would you like your pancakes sweet? How much sugar should we add?" },
            new RecipeStep { instruction = "For texture, do you prefer fluffy or thin pancakes?" },
            new RecipeStep { instruction = "Should we add a splash of green food coloring for fun?" },
            new RecipeStep { instruction = "For garnishing, would you prefer kiwi slices or blueberries, or both?" }
        }
    };

    private Recipe flyLiciousFruitSalad = new Recipe
    {
        name = "Fly-licious Fruit Salad",
        steps = new List<RecipeStep>
        {
            new RecipeStep { instruction = "Which fruits do you enjoy the most for a fruit salad?" },
            new RecipeStep { instruction = "Should we mix all the fruits together or layer them?" },
            new RecipeStep { instruction = "Would you like a zesty lime and honey dressing?" },
            new RecipeStep { instruction = "Should I toss the salad gently to keep the fruits intact?" },
            new RecipeStep { instruction = "Do you prefer your fruit salad served chilled or at room temperature?" }
        }
    };

    private Recipe tadpoleTacos = new Recipe
    {
        name = "Tadpole Tacos",
        steps = new List<RecipeStep>
        {
            new RecipeStep { instruction = "Which protein would you like for your tacos? (e.g., ground beef, vegetarian crumbles)" },
            new RecipeStep { instruction = "Do you prefer your tacos mildly seasoned or spicy?" },
            new RecipeStep { instruction = "What toppings would you like? (e.g., lettuce, tomatoes, cheese, olives)" },
            new RecipeStep { instruction = "Should I fill the taco shells with the prepared ingredients?" },
            new RecipeStep { instruction = "Would you like any additional toppings or sides?" }
        }
    };

    private Recipe frogLeggedSpaghetti = new Recipe
    {
        name = "Frog Legged Spaghetti",
        steps = new List<RecipeStep>
        {
            new RecipeStep { instruction = "Do you prefer your spaghetti with a lot of garlic or just a hint?" },
            new RecipeStep { instruction = "Should we add some red pepper flakes for a bit of heat?" },
            new RecipeStep { instruction = "Do you want cherry tomatoes in the spaghetti?" },
            new RecipeStep { instruction = "Should I toss the spaghetti with the sauce now?" },
            new RecipeStep { instruction = "Would you like basil and Parmesan cheese on top?" },
            new RecipeStep { instruction = "Do you want to add cooked chicken or tofu for a protein boost?" }
        }
    };

    #endregion
}