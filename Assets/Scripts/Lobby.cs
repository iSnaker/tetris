using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    public LobbyGameItem GameItemPrefab;
    public RectTransform RoomsRoot;

    public GameObject JoinFaildWindow;
    public Text JoinFaildWindowLabel;

    public GameObject CreateRoomButton;
    public GameObject LeaveRoomButton;

    public GameObject GamesList;
    public RectTransform IconImage;

    private void Awake()
    {
        if (!PlayerData.I.IsLoad || !PlayerData.I.IsInit)
        {
            
            SceneManager.LoadScene("Loader");
            return;
        }
        NetCore.I.OnConnectSuccessAction += OnConnectServerSuccess;
        NetCore.I.OnGetRoomsListAction += OnGetRoomsList;
        NetCore.I.OnJoinRoomFailedAction += OnJoinRoomFailed;
        NetCore.I.OnRoomJoinedAction += OnRoomJoin;
        NetCore.I.OnJoinedLobbyAction += OnJoinedLobby;
        NetCore.I.OnLeftRoomAction += OnLeftRoom;
        NetCore.I.OnPlayerEnteredRoomAction += OnPlayerEnteredRoom;

        JoinFaildWindow.SetActive(false);
        LeaveRoomButton.SetActive(false);
        CreateRoomButton.SetActive(false);
        GamesList.SetActive(false);
    }

    public void OnDestroy()
    {
        NetCore.I.OnConnectSuccessAction -= OnConnectServerSuccess;
        NetCore.I.OnGetRoomsListAction -= OnGetRoomsList;
        NetCore.I.OnJoinRoomFailedAction -= OnJoinRoomFailed;
        NetCore.I.OnRoomJoinedAction -= OnRoomJoin;
        NetCore.I.OnJoinedLobbyAction -= OnJoinedLobby;
        NetCore.I.OnLeftRoomAction -= OnLeftRoom;
        NetCore.I.OnPlayerEnteredRoomAction -= OnPlayerEnteredRoom;
    }

    public void Start()
    {
        NetCore.I.ConnectToServer();            
    }

    private void OnConnectServerSuccess()
    {
        NetCore.I.JoinLobby();
    }

    private void OnJoinedLobby()
    {
        CreateRoomButton.SetActive(true);
    }

    private void OnGetRoomsList(List<string> rooms)
    {
        while (RoomsRoot.childCount > 0) DestroyImmediate(RoomsRoot.GetChild(0).gameObject);

        var idx = 1;
        for (var i = 0; i < rooms.Count; ++i)
        {
            Instantiate(GameItemPrefab, RoomsRoot).Init(this, rooms[i], idx++);
        }

        GamesList.SetActive(true);
    }

    public void CreateRoom()
    {
        CreateRoomButton.SetActive(false);
        GamesList.SetActive(false);
        NetCore.I.CreateRoom();        
    }

    public void JoinRoom(string id)
    {
        CreateRoomButton.SetActive(false);
        GamesList.SetActive(false);
        NetCore.I.JoinRoom(id);
    }

    private void OnRoomJoin()
    {
        IconImage.eulerAngles = Vector3.zero;
        CreateRoomButton.SetActive(false);
        LeaveRoomButton.SetActive(true);

        if (NetCore.I.PlayersCount == 2)
            SceneManager.LoadScene("Game");
    }

    private void OnJoinRoomFailed(int returnCode, string message)
    {
        CreateRoomButton.SetActive(true);
        LeaveRoomButton.SetActive(false);
        JoinFaildWindow.SetActive(true);
        JoinFaildWindowLabel.text = message;
        GamesList.SetActive(true);
    }

    public void LeaveRoom()
    {
        LeaveRoomButton.SetActive(false);
        NetCore.I.LeaveRoom();        
    }   

    private void OnLeftRoom()
    {
        CreateRoomButton.SetActive(true);
        GamesList.SetActive(true);
    }

    public void OnPlayerEnteredRoom()
    {
        if (NetCore.I.PlayersCount == 2) 
            SceneManager.LoadScene("Game");
    }

    public void Update()
    {
        IconImage.eulerAngles += Vector3.down * Time.deltaTime * 90;
        if (Input.GetKey(KeyCode.Escape))
            SceneManager.LoadScene("Loader");
    }
}
