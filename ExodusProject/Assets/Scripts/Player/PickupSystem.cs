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

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowEPrompt(false);
                    // We no longer add to rescued count here; we wait until they reach the Deposit Zone.
                }
                if (_hud != null)
                {
                    _hud.UpdatePassengerCount(currentPassengers);
                }
                Debug.Log($"Successfully picked up: {pos}");
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
                if (GameManager.Instance != null)
                {
                    Debug.Log("Showing Press E prompt via GameManager.");
                    GameManager.Instance.ShowEPrompt(true);
                }
                else
                {
                    Debug.LogError("GameManager.Instance is NULL! Cannot show prompt.");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HumanNPC"))
        {
            _nearbyHuman = null;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowEPrompt(false);
            }
        }
    }

    // Handle physical collisions as well (if NPC is not a trigger)
    void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }

    void OnCollisionExit(Collision collision)
    {
        OnTriggerExit(collision.collider);
    }

    public void DepositAll()
    {
        // Humans are officially 'rescued' only when they reach the deposit zone building.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHumansDeposited(currentPassengers);
        }
        
        Debug.Log($"[PickupSystem] Deposited {currentPassengers} humans at the building!");
        currentPassengers = 0;
        if (_hud != null) _hud.UpdatePassengerCount(0);
    }
}