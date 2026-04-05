using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPickup pickup = other.GetComponent<PlayerPickup>();

            int passengers = pickup.passengerCount;
            pickup.passengerCount = 0;

            GameManager.Instance.AddRescuedHumans(passengers);
        }
    }
}