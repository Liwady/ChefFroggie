/****************************************************/
// Filename: GameManager.cs
// Created by: Liwady Verbaendert
/****************************************************/

/*
 * GameManager.cs
 * 
 * This script manages the main game state transitions for this super awesome game. It handles
 * the navigation between the main menu, recipe selection, cooking interface, and feedback panel.
 */

/****************************************************/
// EOF: GameManager.cs
/****************************************************/

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables

    public static GameManager instance;
    public GameObject mainMenu;
    public GameObject recipeSelection;
    public GameObject cookingInterface;
    public GameObject feedbackPanel;

    #endregion

    #region Unity Methods

    /// <summary>
    ///     Awake is called when the script instance is being loaded.
    ///     Ensures a single instance of GameManager exists.
    /// </summary>
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Starts the game by showing the recipe selection screen and hiding the main menu.
    /// </summary>
    public void StartGame()
    {
        mainMenu.SetActive(false);
        recipeSelection.SetActive(true);
    }

    /// <summary>
    ///     Quits the application so we are not softlocked =)
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    ///     Transitions from the recipe selection screen to the cooking interface.
    /// </summary>
    public void SelectRecipe()
    {
        recipeSelection.SetActive(false);
        cookingInterface.SetActive(true);
    }

    /// <summary>
    ///     Completes the recipe process by showing the feedback panel and hiding the cooking interface.
    /// </summary>
    public void CompleteRecipe()
    {
        cookingInterface.SetActive(false);
        feedbackPanel.SetActive(true);
    }

    /// <summary>
    ///     Returns to the main menu from the feedback panel.
    /// </summary>
    public void ReturnToMenu()
    {
        feedbackPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    #endregion
}
