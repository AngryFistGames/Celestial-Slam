using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject previousMenu;
    [SerializeField] GameObject loadScreen;
    public int roomSize;

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        
    }

    public void DelayStart()
    {
        previousMenu.SetActive(false);
        loadScreen.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Online Start");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    void CreateRoom()
    {
        Debug.Log("Creating Galaxy");
        int randomGalaxyNumber = Random.Range(0, 10000);
        RoomOptions GalaxySpecs = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize};
        PhotonNetwork.CreateRoom("Galaxy" + randomGalaxyNumber, GalaxySpecs);
        Debug.Log(randomGalaxyNumber);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("The Gods seem to have failed.... trying again");
        CreateRoom();
    }

    public void DelayCancel()
    {
        loadScreen.SetActive(false);
        previousMenu.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }
}
