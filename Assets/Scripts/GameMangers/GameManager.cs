using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingMoney = 650;
    [SerializeField] private int startingLives = 100;

    private int currentMoney;
    private int currentLives;
    private int currentWave = 0;

    public UnityEvent<int> OnMoneyChanged;
    public UnityEvent<int> OnLivesChanged;
    public UnityEvent<int> OnWaveChanged;
    public UnityEvent OnGameOver;

    [Header("Game Speed")]
    [SerializeField] private float[] gameSpeeds = { 1f, 2f, 4f };
    private int currentSpeedIndex = 0;
    public UnityEvent<float> OnGameSpeedChanged;

    // --- Pause Support ---
    private bool isPaused = false;
    private float previousTimeScale = 1f;
    public UnityEvent<bool> OnPauseStateChanged;
    // ---------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentMoney = startingMoney;
        currentLives = startingLives;

        // Set default game speed
        SetGameSpeed(0);
    }

    private void Start()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnLivesChanged?.Invoke(currentLives);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }

    public void LoseLife(int amount)
    {
        currentLives -= amount;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        OnWaveChanged?.Invoke(currentWave);
    }

    private void GameOver()
    {
        OnGameOver?.Invoke();
        Debug.Log("Game Over!");
    }

    public int GetWave()
    {
        return currentWave;
    }
    public int GetMoney()
    {
        return currentMoney;
    }

    public int GetLives()
    {
        return currentLives;
    }

    // --- Game Speed Methods ---
    public void SetGameSpeed(int speedIndex)
    {
        if (isPaused)
        {
            currentSpeedIndex = speedIndex;
            return;
        }

        if (speedIndex < 0 || speedIndex >= gameSpeeds.Length)
            return;

        currentSpeedIndex = speedIndex;
        Time.timeScale = gameSpeeds[speedIndex];
        previousTimeScale = gameSpeeds[speedIndex];
        OnGameSpeedChanged?.Invoke(gameSpeeds[speedIndex]);
    }

    public float GetCurrentGameSpeed()
    {
        return gameSpeeds[currentSpeedIndex];
    }

    // --- Pause Methods ---
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            OnPauseStateChanged?.Invoke(true);
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = gameSpeeds[currentSpeedIndex];
            OnPauseStateChanged?.Invoke(false);
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}