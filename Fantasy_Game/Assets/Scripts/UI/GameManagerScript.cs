using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI eggText;

    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button quitButton;

    public TextMeshProUGUI winText;
    public TextMeshProUGUI finalScore;
    public Button quitButtonWin;

    public PlayerScriptNew playerScript;

    private int score;
    private int health;
    private int eggCount;

    public GameObject heartPrefab;
    public GameObject coinPrefab;

    void Start()
    {
        score = 0;
        eggCount = 0;
        UpdateScore(score);
    }



    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
        finalScore.text = "Final Score: " + score;
    }

    public void UpdateHealth(int newHealth)
    {
        if (playerScript.CurrentHealth() <= 0)
        {
            healthText.text = " 0/100";
        }
        else
        {
            healthText.text = " " + playerScript.CurrentHealth() + "/100";
        }
    }

    public void UpdateEggs()
    {
        eggCount++;
        eggText.text = "Eggs: " + eggCount + "/5";
    }

    public void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void Win()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winText.gameObject.SetActive(true);
        finalScore.gameObject.SetActive(true);
        quitButtonWin.gameObject.SetActive(true);
        StartCoroutine(WaitForWin());
    }

    private IEnumerator WaitForWin()
    {
        yield return new WaitForSeconds(0f);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public int EggCount()
    {
        return eggCount;
    }
}
