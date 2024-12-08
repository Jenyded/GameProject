using TMPro;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    private float elapsedTime = 0f; // Время, прошедшее с начала сессии
    private int hours, minutes, seconds;
    
    private int killCount = 0; // Счётчик убийств
    private int coinCount = 0; // Счётчик монет
    
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI coinCountText;

    private void Start()
    {
        // Запуск корутины
        StartCoroutine(UpdateGameSession());
    }

    private System.Collections.IEnumerator UpdateGameSession()
    {
        while (true)
        {
            elapsedTime += Time.deltaTime;
            hours = Mathf.FloorToInt(elapsedTime / 3600);
            minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
            seconds = Mathf.FloorToInt(elapsedTime % 60);

            timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";

            killCountText.text = $"Kills: {killCount}";
            coinCountText.text = $"Coins: {coinCount}";
            yield return null;
        }
    }

    public void AddKill()
    {
        killCount++;
    }
    
    public void AddCoin()
    {
        coinCount++;
    }
}