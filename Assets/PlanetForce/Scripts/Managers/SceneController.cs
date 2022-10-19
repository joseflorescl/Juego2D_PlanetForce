using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public enum SceneName { Welcome = 0, Game}

    public void LoadGame()
    {
        // La escena Game debe estar en la lista de escenas en la ventana de Build Settings
        SceneManager.LoadScene((int)SceneName.Game);
    }

    public void LoadWelcome()
    {
        // La escena Game debe estar en la lista de escenas en la ventana de Build Settings
        SceneManager.LoadScene((int)SceneName.Welcome);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex == (int)SceneName.Welcome)
                Application.Quit();
            else
                LoadWelcome();
        }
    }
}
