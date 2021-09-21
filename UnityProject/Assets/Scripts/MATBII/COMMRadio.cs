using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class COMMRadio : MonoBehaviour
{
    [SerializeField] private TextMesh display;
    [SerializeField] private COMMSlider channel;
    [SerializeField] private COMMSlider subchannel;
    [SerializeField] private MATBIISystem.COMM_Radio radio;

    private float frequency;
    public float GetFrequency() { return frequency; }

    public void SetFrequency(float f) { frequency = f; DisplayFrequency(); }
    
    public void ChangeFrequency(string author)
    {
        float oldfreq = frequency;
        frequency = channel.GetFrequency() + subchannel.GetFrequency();
        display.text = frequency.ToString("000.000", System.Globalization.CultureInfo.InvariantCulture);

        if (oldfreq != frequency)
        {
            object[] content = new object[] {author, (int)radio, frequency};
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(MATBIISystem.COMM_RadioChanged, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void DisplayFrequency()
    {
        channel.SetFrequency((float)System.Math.Truncate(frequency));
        subchannel.SetFrequency(frequency % 1.0f);
        display.text = frequency.ToString("000.000", System.Globalization.CultureInfo.InvariantCulture);
    }
}
