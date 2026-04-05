using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public int passengerCount = 0;
    public int maxPassengers = 2;
    public int rescuedHumans = 0;

    private HumanPickup currentHuman;

    private void OnTriggerEnter(Collider other)
    {
        HumanPickup human = other.GetComponent<HumanPickup>();
        if(human != null)
        {
            currentHuman = human;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HumanPickup human = other.GetComponent<HumanPickup>();
        if(human != null)
        {
            currentHuman = null;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if(currentHuman != null && passengerCount < maxPassengers)
        {
            passengerCount++;
            Destroy(currentHuman.gameObject);
            Debug.Log("Picked up human. Passengers: " + passengerCount);
        }
    }

    public void DepositPassengers()
    {
        if(passengerCount > 0)
        {
            rescuedHumans += passengerCount;
            Debug.Log("Humans Rescued: " + rescuedHumans);

            passengerCount = 0;
            Debug.Log("Passengers emptied");
        }
    }
}