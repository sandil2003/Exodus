using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerPickup player = other.GetComponent<PlayerPickup>();

            if(player != null)
            {
                player.DepositPassengers();
            }
        }
    }
}