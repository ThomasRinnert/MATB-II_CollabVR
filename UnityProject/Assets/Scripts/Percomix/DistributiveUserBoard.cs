using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DistributiveUserBoard : UserBoard
{
    void Start()
    {
        gameObject.SetActive(false);
        player_manager = GetComponentInParent<PlayerManager>();
    }

    void LateUpdate()
    {
        if (player_manager == null)
        {
            player_manager = PlayerManager.LocalPlayerInstance;
            GetComponentInChildren<UserID>().Init();
        }
        
        if (!photonView) return;
        if (!photonView.IsMine && Camera.current != null)
        {
            transform.LookAt(transform.position + Camera.current.transform.rotation * Vector3.forward, Camera.current.transform.rotation * Vector3.up);
        }
    }

    public void Activate(bool active)
    {
        gameObject.SetActive(active);
    }
}
