using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UserID : MonoBehaviourPun
{
    [SerializeField] public TextMeshPro textID;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        UserBoard board = GetComponentsInParent<UserBoard>(true)[0];
        if (board == null) return;
        if (board.player_manager == null) return;
        textID.text = board.player_manager.NickName;
    }
}