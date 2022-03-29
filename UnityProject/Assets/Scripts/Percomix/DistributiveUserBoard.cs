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
        if (player_manager != null) player = player_manager.player;
        GetComponentInChildren<UserID>().Init();
    }

    void Update()
    {
        if (player_manager == null)
        {
            player_manager = PlayerManager.LocalPlayerInstance;
            player = player_manager.player;
            GetComponentInChildren<UserID>().Init();
        }
        if (!photonView) return;
        if (!photonView.IsMine && Camera.current != null)
        {
            transform.LookAt(transform.position + Camera.current.transform.rotation * Vector3.forward, Camera.current.transform.rotation * Vector3.up);
        }
    }

    public void Activate(bool active, bool force = false)
    {
        if (player_manager == null)
        {
            player_manager = PlayerManager.LocalPlayerInstance;
            if (player_manager != null) player = player_manager.player;
            GetComponentInChildren<UserID>().Init();
        }
        if (!force && player_manager.photonView.IsMine) active = false;
        if (!force && player_manager.NickName.Contains("XP")) active = false;
        gameObject.SetActive(active);
    }

    public void Init()
    {
        player_manager = GetComponentInParent<PlayerManager>();
        if (player_manager == null) player_manager = PlayerManager.LocalPlayerInstance;
        if (player_manager != null) player = player_manager.player;
        GetComponentInChildren<UserID>().Init();
    }
}
