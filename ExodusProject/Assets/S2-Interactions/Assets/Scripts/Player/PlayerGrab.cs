using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    public float grabDistance = 3f;
    public Transform carryPoint;

    private GameObject carriedObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedObject == null)
                TryGrab();
            else
                Drop();
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabDistance))
        {
            if (hit.collider.CompareTag("Human"))
            {
                carriedObject = hit.collider.transform.root.gameObject;

                carriedObject.GetComponent<Rigidbody>().isKinematic = true;
                carriedObject.transform.SetParent(carryPoint);
                carriedObject.transform.localPosition = Vector3.zero;
            }
        }
    }

    void Drop()
    {
        carriedObject.GetComponent<Rigidbody>().isKinematic = false;
        carriedObject.transform.SetParent(null);
        carriedObject = null;
    }
}