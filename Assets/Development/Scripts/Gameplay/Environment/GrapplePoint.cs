using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private void Start()
    {
        visual.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>())
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
