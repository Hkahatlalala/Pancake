using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;      
    public TextMeshProUGUI finalScoreText; 
    public GameObject gameOverPanel;
    public GameCamera gameCamera;
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    public void AddScore()
    {
        score++;
        scoreText.text = score + " pieces";

        
        if (score > 10 && ((score-11) % 5 == 0))
        {
            if (gameCamera != null)
            {
                gameCamera.ZoomOut();
            }
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);

        finalScoreText.text = "You stacked: " + score + " pancakes!";

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}