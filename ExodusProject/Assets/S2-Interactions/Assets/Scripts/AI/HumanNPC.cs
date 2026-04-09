using UnityEngine;

public class HumanNPC : MonoBehaviour
{
    public bool isRescued = false;

    public void GetPickedUp()
    {
        isRescued = true;
        gameObject.SetActive(false); // disappears from world
    }
}