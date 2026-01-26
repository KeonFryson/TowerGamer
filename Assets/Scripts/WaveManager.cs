using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float timeBetweenEnemies = 0.5f;

    [Header("Wave Progression")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private float enemyIncreasePerWave = 1.5f;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private int enemiesAlive = 0;

    private void Awake()
    {
        waveInProgress = false;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        while (true)
        {
            currentWave++;
            waveInProgress = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentWave(currentWave);
            }

            int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyIncreasePerWave, currentWave - 1));

            yield return StartCoroutine(SpawnWave(enemyCount));

            waveInProgress = false;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenEnemies);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null && PathManager.Instance != null)
        {
            Vector3 spawnPos = PathManager.Instance.GetWaypoint(0);
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemiesAlive++;
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
}