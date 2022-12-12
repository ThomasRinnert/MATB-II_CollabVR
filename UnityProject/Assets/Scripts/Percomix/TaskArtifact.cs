using UnityEngine;
using Valve.VR.InteractionSystem;

public class TaskArtifact : MonoBehaviour
{
    public MATBIISystem.MATBII_TASK task = MATBIISystem.MATBII_TASK.SYSMON;

    Interactable interactable = null;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.onAttachedToHand   += onAttachedToHand;
            interactable.onDetachedFromHand += onDetachedFromHand;
        }
    }

    void onAttachedToHand(Hand hand) { HandAttachment(hand, true); }
    void onDetachedFromHand(Hand hand) { HandAttachment(hand, false); }
    void HandAttachment(Hand hand, bool b)
    {
        if(hand.handType == Valve.VR.SteamVR_Input_Sources.LeftHand)
        {
            ExperimentControlsMATBIITeam.Instance.setLeftHand(b);
        }
        if(hand.handType == Valve.VR.SteamVR_Input_Sources.RightHand)
        {
            ExperimentControlsMATBIITeam.Instance.setRightHand(b);
        }
    }
}
