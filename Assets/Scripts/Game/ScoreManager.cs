using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public float score = 0;

    public GameObject ScorePanel;
    public Timer Timer_Text;
    public Text Score_text;
    public Text Your_score;
    public Text Enemy_score;
    public Text Coins;
    public Text WinLose_Text;
    public Button SettingsButton;
    public Button ExitButton;

    private bool check_send_data = true;
    private float enemy_score, your_coins, enemy_coins;

    private void Start()
    {
        Coins.text = Mathf.Round(PlayerData.I.CoinsCount).ToString();

        if (NetCore.I != null)
            NetCore.I.OnEndGame += EndGame;
    }

    public void OnDestroy()
    {
        if (NetCore.I != null)
            NetCore.I.OnEndGame -= EndGame;
    }

    public void AddScore(float score)
    {
        this.score += score;
        Score_text.text = this.score.ToString("0.0");
    }

    public void EndGame(object[] score_lose)
    {
        Timer_Text.timerIsRunning = false;
        ScorePanel.SetActive(true);
        ExitButton.interactable = false;
        SettingsButton.interactable = false;

        enemy_score = System.Convert.ToSingle(score_lose[0]);
        your_coins = PlayerData.I.CoinsCount;
        enemy_coins = System.Convert.ToSingle(score_lose[2]);

        Your_score.text = "Твой счет: " + score.ToString("0.0");
        Enemy_score.text = "Счет противника: " + enemy_score.ToString("0.0");
        
        if (check_send_data && ((score > enemy_score && (int)score_lose[1] == 0) || (int)score_lose[1] == 2))
        {
            WinLose_Text.text = "Ты победил!";
            PlayerPrefs.SetInt("WinsCount", PlayerPrefs.GetInt("WinsCount") + 1);
            NetCore.I.EndGame(new object[] { score, 1, PlayerPrefs.GetFloat("CoinsCount") });
        }
        else if (check_send_data)
        { 
            WinLose_Text.text = "Ты проиграл!";
            PlayerPrefs.SetInt("LosesCount", PlayerPrefs.GetInt("LosesCount") + 1);
            NetCore.I.EndGame(new object[] { score, 2, PlayerPrefs.GetFloat("CoinsCount") });
            your_coins *= 0.95f;
        }
        if (enemy_coins != 0 && (int)score_lose[1] == 2)
            your_coins += enemy_coins * 0.05f + 25;

        PlayerPrefs.SetFloat("CoinsCount", your_coins);
        Coins.text = Mathf.Round(your_coins).ToString();
        check_send_data = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }
}