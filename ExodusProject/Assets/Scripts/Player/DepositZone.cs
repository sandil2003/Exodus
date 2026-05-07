using UnityEngine;

public class DepositZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        ProcessDeposit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcessDeposit(collision.gameObject);
    }

    private void ProcessDeposit(GameObject otherGo)
    {
        // Look for the PickupSystem script anywhere on the hitting object or its parents
        PickupSystem ps = otherGo.GetComponentInParent<PickupSystem>();

        if (ps != null)
        {
            if (ps.currentPassengers > 0)
            {
                Debug.Log($"[DepositZone] Depositing {ps.currentPassengers} humans at {gameObject.name}.");
                ps.DepositAll();
            }
            else
            {
                Debug.Log("[DepositZone] Car is empty. No humans to deposit.");
            }
        }
    }
}