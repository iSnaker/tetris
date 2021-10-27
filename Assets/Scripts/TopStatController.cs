using UnityEngine;
using UnityEngine.UI;

public class TopStatController : MonoBehaviour
{
    public Text WinsCount;
    public Text CoinsCount;
    public Text LosesCount;

    private void Start()
    {
        WinsCount.text = PlayerData.I.WinsCount.ToString();
        CoinsCount.text = Mathf.Round(PlayerData.I.CoinsCount).ToString();
        LosesCount.text = PlayerData.I.LosesCount.ToString();
    }
}
