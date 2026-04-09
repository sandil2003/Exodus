using UnityEngine;

public class DepositZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickupSystem ps = other.GetComponent<PickupSystem>();
            if (ps != null && ps.currentPassengers > 0)
                ps.DepositAll();
        }
    }
}