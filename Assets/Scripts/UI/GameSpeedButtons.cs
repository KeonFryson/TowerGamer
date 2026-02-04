using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedButtons : MonoBehaviour
{
    [SerializeField] private Button speedButton;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI pauseButtonText;

    private readonly int[] speedIndices = { 0, 1, 2 }; // 0: 1x, 1: 2x, 2: 3x
    private int currentSpeedIndex = 0;

    private void Start()
    {
        if (speedButton != null)
        {
            speedButton.onClick.AddListener(CycleSpeed);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }

        // Subscribe to pause state changes if available
        if (GameManager.Instance != null && GameManager.Instance.OnPauseStateChanged != null)
        {
            GameManager.Instance.OnPauseStateChanged.AddListener(UpdatePauseButtonText);
            UpdatePauseButtonText(GameManager.Instance.IsPaused());
        }
        else
        {
            // Fallback: set initial text
            UpdatePauseButtonText(false);
        }

        UpdateSpeedButtonText();
    }

    private void CycleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedIndices.Length;
        SetSpeed(currentSpeedIndex);
        UpdateSpeedButtonText();
    }

    private void SetSpeed(int index)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameSpeed(index);
        }
    }

    private void UpdateSpeedButtonText()
    {
        if (speedButtonText != null)
        {
            string speedLabel = speedIndices[currentSpeedIndex] switch
            {
                0 => ">",
                1 => ">>",
                2 => ">>>",
                _ => ">"
            };
            speedButtonText.text = speedLabel;
        }
    }

    private void TogglePause()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    private void UpdatePauseButtonText(bool isPaused)
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "Play" : "Pause";
        }
    }
}