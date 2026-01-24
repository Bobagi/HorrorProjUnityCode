using UnityEngine;

public sealed class RevolverAimBehaviour : MonoBehaviour, IAimItemBehaviour, IAimZoomOverrideProvider
{
    [Header("Revolver Aim Settings")]
    [SerializeField]
    private InteractablePickupItemType revolverItemType = InteractablePickupItemType.Revolver;

    [SerializeField]
    private PickupHandSide requiredHandSide = PickupHandSide.Right;

    [SerializeField]
    private float zoomedFieldOfView = 24f;

    [SerializeField]
    private float zoomedCameraDistance = 2.4f;

    [Header("Reticle")]
    [SerializeField]
    private GameObject reticleGameObject;

    private bool isReticleActive;

    private void OnDisable()
    {
        SetReticleActive(false);
    }

    public void OnAimStarted(InteractablePickupItemType itemType, PickupHandSide handSide)
    {
        if (!IsMatchingAim(itemType, handSide))
        {
            return;
        }

        SetReticleActive(true);
    }

    public void OnAimCanceled(InteractablePickupItemType itemType, PickupHandSide handSide)
    {
        if (!isReticleActive)
        {
            return;
        }

        SetReticleActive(false);
    }

    public bool TryGetZoomOverride(
        InteractablePickupItemType itemType,
        PickupHandSide handSide,
        out float zoomOverrideFieldOfView,
        out float zoomOverrideCameraDistance
    )
    {
        if (IsMatchingAim(itemType, handSide))
        {
            zoomOverrideFieldOfView = zoomedFieldOfView;
            zoomOverrideCameraDistance = zoomedCameraDistance;
            return true;
        }

        zoomOverrideFieldOfView = 0f;
        zoomOverrideCameraDistance = 0f;
        return false;
    }

    private bool IsMatchingAim(InteractablePickupItemType itemType, PickupHandSide handSide)
    {
        return itemType == revolverItemType && handSide == requiredHandSide;
    }

    private void SetReticleActive(bool isActive)
    {
        isReticleActive = isActive;
        if (reticleGameObject != null)
        {
            reticleGameObject.SetActive(isActive);
        }
    }
}
