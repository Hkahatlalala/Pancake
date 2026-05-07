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

    // BIẾN BẤT TỬ: Nhớ trạng thái để biết người chơi vừa chết hay mới mở app
    public static bool isRestarting = false;

    // --- CỤC ÂM THANH TUI MỚI THÊM VÀO ---
    public AudioClip spawnSound; // Băng chứa tiếng đẻ bánh
    public AudioClip dropSound;  // Băng chứa tiếng rơi bánh
    private AudioSource audioSource; // Cái Loa

    void Awake()
    {
        instance = this;
        // Bắt thằng GameManager đi kiếm cái Loa đang đeo trên người nó
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (isRestarting)
        {
            // NẾU VỪA RESTART: Dẹp Menu, vô thẳng game quất luôn!
            mainMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            scoreText.gameObject.SetActive(true);

            Time.timeScale = 1f; // Rã đông
            isGameActive = true;

            // Xóa cờ để lỡ mài có thoát game vào lại thì nó vẫn hiện Menu
            isRestarting = false;
        }
        else
        {
            // NẾU MỚI MỞ GAME LẦN ĐẦU: Hiện Menu vàng khè
            mainMenuPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            scoreText.gameObject.SetActive(false);

            Time.timeScale = 0f; // Đóng băng
            isGameActive = false;
        }
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);

        Time.timeScale = 1f;
        isGameActive = true;
    }

    // --- 2 HÀM NÀY ĐỂ THẰNG XẺNG GỌI MỖI KHI CẦN KÊU ---
    public void PlaySpawnSound()
    {
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    public void PlayDropSound()
    {
        if (dropSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }

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

    public void Restart()
    {
        isRestarting = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}