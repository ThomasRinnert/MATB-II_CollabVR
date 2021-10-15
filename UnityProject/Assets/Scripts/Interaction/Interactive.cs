using Photon.Pun;
using UnityEngine;

public abstract class Interactive: MonoBehaviourPun
{
    abstract public void Click();
    abstract public void Release();
}