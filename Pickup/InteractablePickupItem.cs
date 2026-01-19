using UnityEngine;

public enum InteractablePickupItemType
{
    Lantern = 0,
    Revolver = 1
}

public sealed class InteractablePickupItem : MonoBehaviour
{
    [SerializeField] private InteractablePickupItemType itemType = InteractablePickupItemType.Lantern;
    [SerializeField] private string itemDisplayName = "Lantern";
    [SerializeField] private Transform pickupInteractionPoint;
    [SerializeField] private bool destroyGameObjectAfterPickup = true;

    private bool hasBeenPickedUp;

    public InteractablePickupItemType ItemType => itemType;
    public string ItemDisplayName => itemDisplayName;
    public Transform PickupInteractionPoint => pickupInteractionPoint != null ? pickupInteractionPoint : transform;

    public bool CanBePickedUp()
    {
        return !hasBeenPickedUp && gameObject.activeInHierarchy;
    }

    public void OnPickedUpBy(GameObject pickerGameObject)
    {
        if (hasBeenPickedUp)
        {
            return;
        }

        hasBeenPickedUp = true;

        if (destroyGameObjectAfterPickup)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }
}
