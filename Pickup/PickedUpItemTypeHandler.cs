using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class PickedUpItemTypeHandler : MonoBehaviour
{
    [Header("Hand Socket")]
    [SerializeField]
    private Transform rightHandSocketTransform;

    [Header("Lantern")]
    [SerializeField]
    private GameObject lanternEquippedPrefab;

    [SerializeField]
    private Vector3 lanternLocalPositionOffset;

    [SerializeField]
    private Vector3 lanternLocalEulerAnglesOffset;

    [Header("Hold IK")]
    [SerializeField]
    private Rig holdRigLayer;

    [SerializeField]
    private TwoBoneIKConstraint holdRightArmTwoBoneIkConstraint;

    [SerializeField]
    private float holdRigWeightWhenActive = 1f;

    [Header("Hold IK Weight Smoothing")]
    [SerializeField]
    private StarterAssets.ThirdPersonController thirdPersonController;

    [SerializeField]
    private float holdIkWeightWhenGrounded = 0.85f;

    [SerializeField]
    private float holdIkWeightWhenAirborne = 0.55f;

    [SerializeField]
    private float holdIkWeightSmoothingSeconds = 0.12f;

    private GameObject currentlyEquippedRightHandItemGameObject;
    private float holdIkWeightVelocity;
    private float currentHoldIkWeight;
    private bool isRightHandItemEquipped;

    public bool IsRightHandItemEquipped => isRightHandItemEquipped;

    private void Awake()
    {
        isRightHandItemEquipped = false;
        holdIkWeightVelocity = 0f;
        currentHoldIkWeight = 0f;

        if (holdRigLayer != null)
        {
            holdRigLayer.weight = 0f;
        }

        if (holdRightArmTwoBoneIkConstraint != null)
        {
            holdRightArmTwoBoneIkConstraint.weight = 0f;
        }
    }

    public void setWeight(float weight)
    {
        if (holdRigLayer != null)
        {
            holdRigLayer.weight = weight;
        }

        if (holdRightArmTwoBoneIkConstraint != null)
        {
            holdRightArmTwoBoneIkConstraint.weight = weight;
        }
    }

    private void Update()
    {
        if (!isRightHandItemEquipped)
        {
            return;
        }

        float desiredHoldIkWeight = ResolveDesiredHoldIkWeight();
        currentHoldIkWeight = Mathf.SmoothDamp(
            currentHoldIkWeight,
            desiredHoldIkWeight,
            ref holdIkWeightVelocity,
            Mathf.Max(0.01f, holdIkWeightSmoothingSeconds)
        );

        if (holdRigLayer != null)
        {
            holdRigLayer.weight = currentHoldIkWeight;
        }

        if (holdRightArmTwoBoneIkConstraint != null)
        {
            holdRightArmTwoBoneIkConstraint.weight = currentHoldIkWeight;
        }
    }

    public void HandlePickedUpItem(InteractablePickupItem pickedUpItem)
    {
        if (pickedUpItem == null)
        {
            return;
        }

        if (rightHandSocketTransform == null)
        {
            return;
        }

        if (pickedUpItem.ItemType == InteractablePickupItemType.Lantern)
        {
            EquipLantern();
        }
    }

    private void EquipLantern()
    {
        if (lanternEquippedPrefab == null)
        {
            return;
        }

        if (currentlyEquippedRightHandItemGameObject != null)
        {
            Destroy(currentlyEquippedRightHandItemGameObject);
            currentlyEquippedRightHandItemGameObject = null;
        }

        GameObject spawnedLanternGameObject = Instantiate(lanternEquippedPrefab);

        BoxCollider spawnedLanternBoxCollider =
            spawnedLanternGameObject.GetComponent<BoxCollider>();
        if (spawnedLanternBoxCollider != null)
        {
            spawnedLanternBoxCollider.enabled = false;
        }

        spawnedLanternGameObject.transform.position = rightHandSocketTransform.TransformPoint(
            lanternLocalPositionOffset
        );
        spawnedLanternGameObject.transform.rotation =
            rightHandSocketTransform.rotation * Quaternion.Euler(lanternLocalEulerAnglesOffset);

        currentlyEquippedRightHandItemGameObject = spawnedLanternGameObject;

        isRightHandItemEquipped = true;
        holdIkWeightVelocity = 0f;
        currentHoldIkWeight = 0f;

        float desiredHoldIkWeight = ResolveDesiredHoldIkWeight();

        if (holdRigLayer != null)
        {
            holdRigLayer.weight = desiredHoldIkWeight;
        }

        if (holdRightArmTwoBoneIkConstraint != null)
        {
            holdRightArmTwoBoneIkConstraint.weight = desiredHoldIkWeight;
        }

        currentHoldIkWeight = desiredHoldIkWeight;

        LanternHandleFixedJointFollower handleFollower =
            spawnedLanternGameObject.GetComponentInChildren<LanternHandleFixedJointFollower>();

        if (handleFollower != null)
        {
            handleFollower.BindToHandSocket(rightHandSocketTransform);
        }
    }

    private float ResolveDesiredHoldIkWeight()
    {
        if (thirdPersonController == null)
        {
            return Mathf.Clamp01(holdRigWeightWhenActive);
        }

        return thirdPersonController.Grounded ? holdIkWeightWhenGrounded : holdIkWeightWhenAirborne;
    }
}
