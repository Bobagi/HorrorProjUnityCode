public interface IAimZoomOverrideProvider
{
    bool TryGetZoomOverride(
        InteractablePickupItemType itemType,
        PickupHandSide handSide,
        out float zoomedFieldOfView,
        out float zoomedCameraDistance
    );
}
