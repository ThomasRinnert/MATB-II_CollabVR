using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RESMANPumpButton : MonoBehaviour, Interactive
{
    [SerializeField] private MATBIISystem.RESMAN_Pump pumpID;
    [SerializeField] private GameObject arrow;
    private List<Material> materials = new List<Material>();
    private Outline outline;

    private bool pressing = false;
    public bool isPressed() { return pressing; }

    private Transform author;

    void Start()
    {
        materials.Add(GetComponent<Renderer>().material);
        Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
        for (int r = 0; r < renderers.Length; r++)
        {
            materials.Add(renderers[r].material);
        }

        outline = GetComponent<Outline>();
        outline.enabled = false;

        ChangeColor(new Color(0.7f, 0.7f, 0.7f, 1));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = true;
            author = other.transform;
            outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = false;
            outline.enabled = false;
        }
    }

    public void Activate() { ChangeColor(Color.green); }

    public void Restore() { ChangeColor(new Color(0.7f, 0.7f, 0.7f, 1)); }

    public void Fail() { ChangeColor(Color.red); }

    private void ChangeColor(Color c)
    {
        for (int m = 0; m < materials.Count; m++)
        {
            materials[m].color = c;
        }
    }

    public void Click()
    {
        if (!pressing) return;
        object[] content = new object[] {author.GetComponentInParent<PhotonView>().Owner.NickName, (int)pumpID};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(MATBIISystem.RESMAN_PumpChanged, content, raiseEventOptions, SendOptions.SendReliable);
        pressing = false;
    }
    public void Release() {}
}
