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
}