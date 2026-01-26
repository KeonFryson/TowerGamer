using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private TextMeshProUGUI MoneyText;
    [SerializeField] private TextMeshProUGUI LivesText;
    [SerializeField] private TextMeshProUGUI WaveText;

 
    // Update is called once per frame
    void Update()
    {
        
        if (GameManager.Instance != null)
        {
            MoneyText.text = "Money: $" + GameManager.Instance.GetMoney();
            LivesText.text = "Lives: " + GameManager.Instance.GetLives();
            WaveText.text = "Wave: " + GameManager.Instance.GetWave();
        }


    }
}
