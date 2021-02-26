using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine;

public class RoomControl : MonoBehaviourPunCallbacks
{
    [SerializeField] int waitingRoomSceneIndex;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene(waitingRoomSceneIndex);
    }
}
