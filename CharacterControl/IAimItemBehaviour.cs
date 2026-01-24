public interface IAimItemBehaviour
{
    void OnAimStarted(InteractablePickupItemType itemType, PickupHandSide handSide);
    void OnAimCanceled(InteractablePickupItemType itemType, PickupHandSide handSide);
}
