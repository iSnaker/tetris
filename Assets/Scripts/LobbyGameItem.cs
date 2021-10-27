using UnityEngine;
using UnityEngine.UI;

public class LobbyGameItem : MonoBehaviour
{
    public string RoomId;
    public Text RoomLabel;
    public Lobby Lobby;

    public void OnClick()
    {
        Lobby.JoinRoom(RoomId);
    }

    internal void Init(Lobby lobby, string name, int idx)
    {
        var roomUserName = name.Split('#')[0];
        RoomLabel.text = idx + ". " + roomUserName;
        RoomId = name;
        Lobby = lobby;
    }
}
