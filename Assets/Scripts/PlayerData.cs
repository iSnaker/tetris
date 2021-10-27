using System;
using UnityEngine;

public class PlayerData
{
    private static PlayerData _this;

    public static PlayerData I
    {
        get
        {
            if (_this != null) return _this;
            _this = new PlayerData();
            return _this;
        }
    }

    public bool IsInit;
    public bool IsLoad;
    public string NickName;
    public string UserId;

    private PlayerData()
    {
        IsInit = true;
        if (!PlayerPrefs.HasKey("Player"))
        {
            IsLoad = false;
            NickName = "";
            return;
        }

        IsLoad = true;
        NickName = PlayerPrefs.GetString("NickName");
    }

    internal void SetNickName(string v)
    {
        NickName = v;
        PlayerPrefs.SetInt("Player", 1);
        PlayerPrefs.SetString("NickName", v);
        IsLoad = true;
    }

    internal void Logout()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    internal float CoinsCount => PlayerPrefs.GetFloat("CoinsCount", 100);
    internal int WinsCount => PlayerPrefs.GetInt("WinsCount", 0);
    internal int LosesCount => PlayerPrefs.GetInt("LosesCount", 0);
}
