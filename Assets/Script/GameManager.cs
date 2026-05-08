using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public GameObject gameOverPanel;
    public GameObject mainMenuPanel;
    public GameCamera gameCamera;
    public static GameManager instance;

    public bool isGameActive = false;

    public static bool isRestarting = false;

    public AudioClip spawnSound;
    public AudioClip dropSound;
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    // Kiểm tra có phải Restart hay ko
    void Start()
    {
        if (isRestarting)
        {
            mainMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            scoreText.gameObject.SetActive(true);

            Time.timeScale = 1f;
            isGameActive = true;
            isRestarting = false;
        }
        else
        {
            mainMenuPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            scoreText.gameObject.SetActive(false);

            Time.timeScale = 0f;
            isGameActive = false;
        }
    }

    // Vào Game từ Menu
    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);

        Time.timeScale = 1f;
        isGameActive = true;
    }

    // Phát âm thanh xuất hiện bánh
    public void PlaySpawnSound()
    {
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    // Phát âm thanh khi bánh rơi chạm đĩa hoặc tháp bánh
    public void PlayDropSound()
    {
        if (dropSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }

    // Tăng điểm và kiểm tra điều kiện scale Camera
    public void AddScore()
    {
        if (!isGameActive) return;

        score++;
        scoreText.text = score.ToString();

        if (score > 10 && ((score - 11) % 5 == 0))
        {
            if (gameCamera != null)
            {
                gameCamera.ZoomOut();
            }
        }
    }

    // Xử lý thua cuộc
    public void GameOver()
    {
        if (gameCamera != null)
        {
            gameCamera.TriggerShake(0.5f, 0.3f);
        }

        gameOverPanel.SetActive(true);
        finalScoreText.text = "You stacked: " + score + " pancakes!";

        Time.timeScale = 0f;
        isGameActive = false;
    }

    // Tải lại Scene từ đầu bật isRestarting để game vào thẳng màn chơi mà không qua Menu
    public void Restart()
    {
        isRestarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}