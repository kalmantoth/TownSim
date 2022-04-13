using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuHandlerScript : MonoBehaviour
{
     public Button StartGameButton;
     public Button ContinueGameButton;
     public Button ExitGameButton;

     // Start is called before the first frame update
     void Start()
     {
          if (SceneManager.GetSceneByName("PauseMenu").isLoaded) Time.timeScale = 0f;

          // Button event listeners
          if (StartGameButton != null) StartGameButton.onClick.AddListener(StartGame);
          if (ContinueGameButton != null) ContinueGameButton.onClick.AddListener(ContinueGame);
          if (ExitGameButton != null) ExitGameButton.onClick.AddListener(ExitGame);
          
     }

     // Update is called once per frame
     void Update()
     {
          if (Input.GetKeyDown(KeyCode.KeypadEnter))
          {
               StartGame();
          }

          if (Input.GetKeyDown(KeyCode.Escape))
          {
               if (SceneManager.GetSceneByName("PauseMenu").isLoaded) ContinueGame();
               else ExitGame();
          }
     }

     private void StartGame()
     {
          if (SceneManager.GetSceneByName("PauseMenu").isLoaded)
          {
               Time.timeScale = 1;
               SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
               SceneManager.UnloadSceneAsync("PauseMenu");
               GlobVars.InitGlobVars();
               SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
          }
          else
          {
               Debug.Log("MainScene loading...");
               GlobVars.InitGlobVars();
               SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
          }
     }

     private void ContinueGame()
     {
          Time.timeScale = 1;           // Resume the game by clicking continue from the Pause menu
          SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));
          SceneManager.UnloadSceneAsync("PauseMenu");
     }

     private void ExitGame()
     {
          if (SceneManager.GetSceneByName("PauseMenu").isLoaded)
          {
               Time.timeScale = 1;
               SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
          }
          else
               Application.Quit();
          
     }



}
