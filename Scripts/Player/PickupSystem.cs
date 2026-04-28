using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public static PickupSystem Instance { get; private set; }

    public int maxCapacity = 2;
    public int currentPassengers = 0;

    public event System.Action<int, Vector3> HumanPickedUp;

    private HumanNPC _nearbyHuman = null;
    private HUDManager _hud;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() 
    {
        FindHUD();
    }

    private void FindHUD()
    {
        if (_hud == null)
        {
            _hud = FindObjectOfType<HUDManager>();
            if (_hud == null)
            {
                Debug.LogWarning("HUDManager not found in scene! Pickup UI will not work.");
            }
        }
    }

    void Update()
    {
        if (_nearbyHuman != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E pressed. Nearby human: {_nearbyHuman.name}, Passengers: {currentPassengers}/{maxCapacity}");
            
            if (currentPassengers < maxCapacity)
            {
                currentPassengers++;
                Vector3 pos = _nearbyHuman.transform.position;
                _nearbyHuman.GetPickedUp();
                _nearbyHuman = null;

                HumanPickedUp?.Invoke(-1, pos);

                if (_hud != null)
                {
                    _hud.UpdatePassengerCount(currentPassengers);
                    _hud.ShowEPrompt(false);
                }
            }
            else
            {
                Debug.Log("Car is full!");
            }
        }
    }

    // Called by child SphereCollider (trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HumanNPC"))
        {
            // We use GetComponentInParent in case the Tag is on a child object (like a collider)
            _nearbyHuman = other.GetComponentInParent<HumanNPC>();
            
            if (_nearbyHuman == null)
            {
                Debug.LogError($"Object tagged 'HumanNPC' ({other.name}) is missing the HumanNPC script!");
                return;
            }

            Debug.Log($"Detected Human: {_nearbyHuman.name}");

            if (currentPassengers < maxCapacity)
            {
                FindHUD();
                if (_hud != null) _hud.ShowEPrompt(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HumanNPC"))
        {
            _nearbyHuman = null;
            if (_hud != null) _hud.ShowEPrompt(false);
        }
    }

    public void DepositAll()
    {
        FindObjectOfType<GameManager>().OnHumansDeposited(currentPassengers);
        currentPassengers = 0;
        _hud.UpdatePassengerCount(0);
    }
}