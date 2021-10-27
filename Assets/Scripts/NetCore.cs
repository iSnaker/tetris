using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetCore : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static NetCore I;

    private TypedLobby _customLobby = new TypedLobby("tetrisLobby", LobbyType.Default);
    private Dictionary<string, RoomInfo> _cachedRoomList = new Dictionary<string, RoomInfo>();


    public void Awake()
    {
        if (I != null && I != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(this);
    }

    public bool InRoom => PhotonNetwork.InRoom;

    public Action OnConnectSuccessAction;
    public Action OnConnectFailedAction;
    public Action OnJoinedLobbyAction;
    public Action<List<string>> OnGetRoomsListAction;
    public Action OnRoomCreateAction;
    public Action OnRoomJoinedAction;
    public Action<int, string> OnJoinRoomFailedAction;
    public Action OnLeftRoomAction;
    public Action OnPlayerEnteredRoomAction;

    public Action<bool> OnClearPrint;
    public Action<object[]> OnEndGame;
    public Action<bool> OnLoseGame;
    public Action<bool> OnEnableDraw;

    public Action<int> OnGetMoney;

    public Action<byte[]> OnTransCam;

    public Action<FigurePacket> OnFigureRecieveAction;

    public int PlayersCount => PhotonNetwork.PlayerList.Length;

    public void ConnectToServer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            OnConnectedToMaster();
            return;
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN.");
        PlayerData.I.UserId = PhotonNetwork.AuthValues.UserId;
        OnConnectSuccessAction?.Invoke();        
    }    

    public void JoinLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            OnJoinedLobby();
            return;
        }
        PhotonNetwork.JoinLobby(_customLobby);
    }

    public override void OnJoinedLobby()
    {
        OnJoinedLobbyAction?.Invoke();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                _cachedRoomList.Remove(info.Name);
            }
            else
            {
                _cachedRoomList[info.Name] = info;
            }
        }

        OnGetRoomsListAction?.Invoke(_cachedRoomList.Keys.ToList());
    }

    public void CreateRoom()
    {
        var roomName = PlayerData.I.NickName + "#" + PlayerData.I.UserId;
        Debug.Log("Create room: " + roomName);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions
        {
            MaxPlayers = 2
        });
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        OnRoomJoinedAction?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"OnJoinRoomFailed[{returnCode}]: {message}");
        OnJoinRoomFailedAction?.Invoke(returnCode, message);        
    }

    internal void LeaveRoom() {
        if (PhotonNetwork.CurrentRoom != null)
        {
            Debug.Log("LeaveRoom...");
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            OnLeftRoom();
        }
    }

    public override void OnLeftRoom()
    {
        OnLeftRoomAction?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerEnteredRoomAction?.Invoke();
    }

    public void EndGame(object[] data)
    {
            var ro = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(2, data, ro, SendOptions.SendReliable);
    }

    public void GetImage(byte[] tex_bytes)
    {
        var ro = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(3, tex_bytes, ro, SendOptions.SendReliable);
    }

    public void ClearPrint(bool enable_draw)
    {
            var ro = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(1, enable_draw, ro, SendOptions.SendReliable);
    }

    public void SendFigure(FigurePacket packet)
    {
        if (PhotonNetwork.InRoom)
        {
            var bytes = packet.GetData();
            Debug.Log("Net send data: " + bytes.Length);
            var otherPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => !p.IsLocal);
            if (otherPlayer == null)
                return;

            var ro = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(0, bytes, ro, SendOptions.SendReliable);
        }        
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == 0)
        {
            var data = (byte[])photonEvent.CustomData;

            var f = new FigurePacket(data);
            OnFigureRecieveAction?.Invoke(f);
        }
        if (eventCode == 1)
        {
            OnClearPrint?.Invoke((bool)photonEvent.CustomData);
        }
        if (eventCode == 2)
        {
            OnEndGame?.Invoke((object[])photonEvent.CustomData);
        }
        if (eventCode == 3)
        {
            OnTransCam?.Invoke((byte[])photonEvent.CustomData);
        }
    }
}
