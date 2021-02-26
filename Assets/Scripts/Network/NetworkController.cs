using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public override void OnEnable()
    {
        ConnectToServer();
        base.OnEnable();
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        Debug.Log("You are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }

    void Update()
    {
        
    }
    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
}
