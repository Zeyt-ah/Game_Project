using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Keybinds()
    {
        SceneManager.LoadScene(2);
    }
    public void Instructions()
    {
        SceneManager.LoadScene(3);
    }
    public void Settings()
    {
        SceneManager.LoadScene(5);
    }

    public void QuitGame()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }
}
