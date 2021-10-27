using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class Loader : MonoBehaviour
{
    public InputField NickName;

    public RectTransform CompButtonTransform;
    public RectTransform OnlineButtonTransform;

    
    public GameObject NickGroup;


    public void Start()
    {
        NickName.text = "";
        CompButtonTransform.gameObject.SetActive(true);
        OnlineButtonTransform.gameObject.SetActive(true);
        NickGroup.SetActive(false);
        if (PlayerData.I.IsInit && PlayerData.I.IsLoad)
        {
            NickName.text = PlayerData.I.NickName;
        }
    }

    public void OnOnlineClick()
    {
        if (PlayerData.I.IsInit && PlayerData.I.IsLoad)
        {
            SceneManager.LoadSceneAsync("Lobby");
        }
        else
        {
            NickGroup.SetActive(true);
            OnlineButtonTransform.gameObject.SetActive(false);
            CompButtonTransform.gameObject.SetActive(false);            
        }
    }

    public void OnPlayClick()
    {
        if (NickName.text.IsNullOrEmpty())
            return;
        NickGroup.SetActive(false);
        PlayerData.I.SetNickName(NickName.text.Replace("#", ""));
        NickName.text = PlayerData.I.NickName;
        SceneManager.LoadSceneAsync("Lobby");
    }
}
