using UnityEngine;

public class DepositZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 1. ALWAYS Log whenever ANYTHING touches the cube trigger
        Debug.Log($"[DepositZone] Trigger hit by: '{other.name}' (Tag: {other.tag})");

        // 2. Look for the PickupSystem script anywhere on the hitting object or its parents
        PickupSystem ps = other.GetComponentInParent<PickupSystem>();

        if (ps != null)
        {
            Debug.Log($"[DepositZone] Found PickupSystem on: {ps.gameObject.name}");
            
            if (ps.currentPassengers > 0)
            {
                Debug.Log($"[DepositZone] OK! Depositing {ps.currentPassengers} passengers.");
                ps.DepositAll();
            }
            else
            {
                Debug.Log("[DepositZone] Car is empty. No humans to deposit.");
            }
        }
    }
}