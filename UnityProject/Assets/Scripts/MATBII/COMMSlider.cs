using Photon.Pun;
using UnityEngine;

public class COMMSlider : Interactive
{
    [SerializeField] public COMMRadio radio;
    [SerializeField] public float freqMin = 108.000f;
    [SerializeField] public float freqMax = 135.975f;
    [SerializeField] public float increment = 0.025f;

    [SerializeField] private float frequency = 0.0f;
    public float GetFrequency() { return frequency; }
    public void SetFrequency(float f) { frequency = f; AlignSlider(); }

    [SerializeField] private Collider minBorder;
    [SerializeField] private Collider maxBorder;
    private bool outOfBounds = false;
    public bool isOutOfBounds() { return outOfBounds; }
    
    private Outline outline;
    private bool pressing = false;
    public bool isPressed() { return pressing; }

    private bool clicked = false;
    override public void Click()
    {
        if(pressing) clicked = true;
    }
    override public void Release()
    {
        clicked = false;
        ComputeFrequency(author != null ? author.NickName : "LocalUser");
    }

    private Photon.Realtime.Player author = null;
    private Transform interactor = null;

    void Start()
    {
        frequency = freqMin;
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void Update()
    {
        if(!clicked || author != PhotonNetwork.LocalPlayer) return;
        this.transform.Translate(interactor.position.x - this.transform.position.x, 0, 0);
        ComputeFrequency(author != null ? author.NickName : "LocalUser");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bounds")
        {
            outOfBounds = true;
        }
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            pressing = true;
            interactor = other.transform;
            var photon = other.GetComponentInParent<Photon.Pun.PhotonView>();
            if (photon != null) author = photon.Owner;
            outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bounds")
        {
            outOfBounds = false;
        }
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            pressing = false;
            outline.enabled = false;
        }
    }

    private void ComputeFrequency(string author)
    {
        float x = transform.position.x - minBorder.transform.position.x;

        frequency = x * (freqMax - freqMin) / (maxBorder.transform.position.x - minBorder.transform.position.x) + freqMin;
        frequency -= frequency % increment;

        if (frequency < freqMin) { frequency = freqMin; AlignSlider(); }
        if (frequency > freqMax) { frequency = freqMax - increment; AlignSlider(); }

        radio.ChangeFrequency(author);
    }

    private void AlignSlider()
    {
        float x = minBorder.transform.position.x + (maxBorder.transform.position.x - minBorder.transform.position.x) * (frequency - freqMin) / (freqMax - freqMin);
        
        if (x < minBorder.transform.position.x) x = minBorder.transform.position.x; 
        if (x > maxBorder.transform.position.x) x = maxBorder.transform.position.x; 
        
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
