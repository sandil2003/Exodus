using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public int maxCapacity = 2;
    public int currentPassengers = 0;

    private HumanNPC _nearbyHuman = null;
    private HUDManager _hud;

    void Start() => _hud = FindObjectOfType<HUDManager>();

    void Update()
    {
        if (_nearbyHuman != null && Input.GetKeyDown(KeyCode.E))
        {
            if (currentPassengers < maxCapacity)
            {
                currentPassengers++;
                _nearbyHuman.GetPickedUp();
                _nearbyHuman = null;
                _hud.UpdatePassengerCount(currentPassengers);
                _hud.ShowEPrompt(false);
            }
        }
    }

    // Called by child SphereCollider (trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HumanNPC"))
        {
            _nearbyHuman = other.GetComponent<HumanNPC>();
            if (currentPassengers < maxCapacity)
                _hud.ShowEPrompt(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HumanNPC"))
        {
            _nearbyHuman = null;
            _hud.ShowEPrompt(false);
        }
    }

    public void DepositAll()
    {
        FindObjectOfType<GameManager>().OnHumansDeposited(currentPassengers);
        currentPassengers = 0;
        _hud.UpdatePassengerCount(0);
    }
}