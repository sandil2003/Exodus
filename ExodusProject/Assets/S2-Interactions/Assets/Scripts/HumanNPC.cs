using UnityEngine;

public class HumanNPC : MonoBehaviour
{
    public bool isCarried = false;

    public void PickUp(Transform carryPoint)
    {
        isCarried = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        isCarried = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.SetParent(null);
    }
}