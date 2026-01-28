using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedButtons : MonoBehaviour
{
    //[SerializeField] private Button speed1xButton;
    [SerializeField] private Button speed2xButton;
    [SerializeField] private Button speed3xButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI pauseButtonText;

    private void Start()
    {
        //speed1xButton.onClick.AddListener(() => SetSpeed(0));
        speed2xButton.onClick.AddListener(() => SetSpeed(1));
        speed3xButton.onClick.AddListener(() => SetSpeed(2));
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
    }

    private void SetSpeed(int index)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameSpeed(index);
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
            pauseButtonText.text = isPaused ? ">" : "||";
        }
    }
} 