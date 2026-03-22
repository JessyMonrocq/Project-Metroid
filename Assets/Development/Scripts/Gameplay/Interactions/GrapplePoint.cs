using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private bool isPowered;

    public bool IsPowered
    {
        get { return isPowered; }
        set { isPowered = value; }
    }

    private void Start()
    {
        visual.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>() && isPowered)
        {
            other.gameObject.GetComponent<PlayerMovement>().SetGrapplePoint(this.transform);
            visual.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            other.gameObject.GetComponent<PlayerMovement>().SetGrapplePoint(null);
            visual.SetActive(false);
        }
    }
}
