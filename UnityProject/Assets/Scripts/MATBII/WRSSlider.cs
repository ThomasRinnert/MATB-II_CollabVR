using Photon.Pun;
using UnityEngine;

public class WRSSlider : Interactive
{
    public enum WorkloadType {cognitive, physical}
    [SerializeField] private WorkloadType type;
    [SerializeField] private Collider minBorder;
    [SerializeField] private Collider maxBorder;
    [SerializeField] private int Min = 0;
    [SerializeField] private int Max = 9;
    [SerializeField] public int value = 5;

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
        Compute(author.NickName);
    }

    private Photon.Realtime.Player author = null;
    private Transform interactor = null;

    void Start()
    {
        value = Min;
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void Update()
    {
        if(!clicked || author != PhotonNetwork.LocalPlayer) return;
        this.transform.Translate(interactor.position.x - this.transform.position.x, 0, 0);
        //Compute(author.NickName);
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
            author = other.GetComponentInParent<Photon.Pun.PhotonView>().Owner;
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

    private void Compute(string author)
    {
        float x = transform.position.x - minBorder.transform.position.x;

        x = (x * (Max - Min) / (maxBorder.transform.position.x - minBorder.transform.position.x) + Min);
        value = Mathf.RoundToInt(x);

        if (value < Min) { value = Min; }
        if (value > Max) { value = Max; }

        AlignSlider();
    }

    private void AlignSlider()
    {
        float x = minBorder.transform.position.x + (maxBorder.transform.position.x - minBorder.transform.position.x) * (value - Min) / (Max - Min);
        
        if (x < minBorder.transform.position.x) x = minBorder.transform.position.x; 
        if (x > maxBorder.transform.position.x) x = maxBorder.transform.position.x; 
        
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
