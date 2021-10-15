using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

public class AvatarScale : MonoBehaviour
{
    [SerializeField] public PlayerManagerVR player;

    public SteamVR_Action_Boolean action = null;
    
    [SerializeField] public Transform LeftShoulder;
    [SerializeField] public Transform LeftHand;
    [SerializeField] public Transform LeftTarget;

    [SerializeField] public Transform RightShoulder;
    [SerializeField] public Transform RightHand;
    [SerializeField] public Transform RightTarget;

    [SerializeField] public Transform Head;
    [SerializeField] public Transform HeadTarget;

    private bool left;
    private bool right;

    // Start is called before the first frame update
    void Start()
    {
        if (!player.photonView.IsMine) return;

        action.AddOnStateDownListener(l1, SteamVR_Input_Sources.LeftHand);
        action.AddOnStateDownListener(r1, SteamVR_Input_Sources.RightHand);
        action.AddOnStateUpListener(l2, SteamVR_Input_Sources.LeftHand);
        action.AddOnStateUpListener(r2, SteamVR_Input_Sources.RightHand);
    }
    
    void OnDestroy()
    {
        if (!player.photonView.IsMine) return;
        
        action.RemoveOnStateDownListener(l1, SteamVR_Input_Sources.LeftHand);
        action.RemoveOnStateDownListener(r1, SteamVR_Input_Sources.RightHand);
        action.RemoveOnStateUpListener(l2, SteamVR_Input_Sources.LeftHand);
        action.RemoveOnStateUpListener(r2, SteamVR_Input_Sources.RightHand);
    }

    void l1(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) { left  = true; if (left && right) StartCoroutine(Resize(false)); }
    void r1(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) { right = true; if (left && right) StartCoroutine(Resize(true)); }
    void l2(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) { left  = false; }
    void r2(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) { right = false; }

    IEnumerator Resize(bool upscale)
    {
        float factor = (upscale) ? 1.01f: 0.99f;
    
        while ((upscale && ((LeftTarget.position - LeftHand.position).magnitude > 0.01f || (RightTarget.position - RightHand.position).magnitude > 0.01f))
            ||(!upscale && ((LeftTarget.position - LeftHand.position).magnitude < 0.01f || (RightTarget.position - RightHand.position).magnitude < 0.01f)))
        {
            this.transform.localScale = this.transform.localScale * factor;
            player.scale = this.transform.localScale;
            yield return new WaitForFixedUpdate();
        }
    }

    public void Resize(Vector3 scale) { this.transform.localScale = scale; }
    /*
    void Resize()
    {
        Vector3 l = LeftHand.position - LeftShoulder.position;
        Vector3 tl = LeftTarget.position - LeftShoulder.position;
        float factor_l = tl.magnitude / l.magnitude;

        Vector3 r = RightHand.position - RightShoulder.position;
        Vector3 tr = RightTarget.position - RightShoulder.position;
        float factor_r = tr.magnitude / r.magnitude;

        Vector3 h = Head.position - transform.position;
        Vector3 th = HeadTarget.position - transform.position;
        float factor_h = th.magnitude / h.magnitude;

        float factor = Mathf.Max(factor_l, factor_r, factor_h);
        this.transform.localScale = Vector3.one * factor;
        
    }
    */
}
