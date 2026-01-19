using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class PickedUpItemTypeHandler : MonoBehaviour
{
    [Header("Hand Socket")]
    [SerializeField]
    private Transform rightHandSocketTransform;

    [SerializeField]
    private Transform leftHandSocketTransform;

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
    private Rig leftHoldRigLayer;

    [SerializeField]
    private TwoBoneIKConstraint holdLeftArmTwoBoneIkConstraint;

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
    private GameObject currentlyEquippedLeftHandItemGameObject;
    private float rightHoldIkWeightVelocity;
    private float rightCurrentHoldIkWeight;
    private float leftHoldIkWeightVelocity;
    private float leftCurrentHoldIkWeight;
    private bool isRightHandItemEquipped;
    private bool isLeftHandItemEquipped;

    public bool IsRightHandItemEquipped => isRightHandItemEquipped;
    public bool IsLeftHandItemEquipped => isLeftHandItemEquipped;

    private void Awake()
    {
        isRightHandItemEquipped = false;
        isLeftHandItemEquipped = false;
        rightHoldIkWeightVelocity = 0f;
        rightCurrentHoldIkWeight = 0f;
        leftHoldIkWeightVelocity = 0f;
        leftCurrentHoldIkWeight = 0f;

        if (holdRigLayer != null)
        {
            holdRigLayer.weight = 0f;
        }

        if (holdRightArmTwoBoneIkConstraint != null)
        {
            holdRightArmTwoBoneIkConstraint.weight = 0f;
        }

        if (leftHoldRigLayer != null)
        {
            leftHoldRigLayer.weight = 0f;
        }

        if (holdLeftArmTwoBoneIkConstraint != null)
        {
            holdLeftArmTwoBoneIkConstraint.weight = 0f;
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

        if (leftHoldRigLayer != null)
        {
            leftHoldRigLayer.weight = weight;
        }

        if (holdLeftArmTwoBoneIkConstraint != null)
        {
            holdLeftArmTwoBoneIkConstraint.weight = weight;
        }
    }

    private void Update()
    {
        if (!isRightHandItemEquipped && !isLeftHandItemEquipped)
        {
            return;
        }

        if (isRightHandItemEquipped)
        {
            UpdateHoldIkWeight(
                ref rightCurrentHoldIkWeight,
                ref rightHoldIkWeightVelocity,
                holdRigLayer,
                holdRightArmTwoBoneIkConstraint
            );
        }

        if (isLeftHandItemEquipped)
        {
            UpdateHoldIkWeight(
                ref leftCurrentHoldIkWeight,
                ref leftHoldIkWeightVelocity,
                leftHoldRigLayer,
                holdLeftArmTwoBoneIkConstraint
            );
        }
    }

    public void HandlePickedUpItem(InteractablePickupItem pickedUpItem)
    {
        HandlePickedUpItem(pickedUpItem, PickupHandSide.Right);
    }

    public void HandlePickedUpItem(InteractablePickupItem pickedUpItem, PickupHandSide pickupHandSide)
    {
        if (pickedUpItem == null)
        {
            return;
        }

        if (pickedUpItem.ItemType == InteractablePickupItemType.Lantern)
        {
            EquipLantern(pickupHandSide);
        }
    }

    private void EquipLantern(PickupHandSide pickupHandSide)
    {
        if (lanternEquippedPrefab == null)
        {
            return;
        }

        if (pickupHandSide == PickupHandSide.Left)
        {
            EquipLanternToLeftHand();
            return;
        }

        EquipLanternToRightHand();
    }

    private void EquipLanternToRightHand()
    {
        if (rightHandSocketTransform == null)
        {
            return;
        }

        GameObject spawnedLanternGameObject = SpawnEquippedLantern(rightHandSocketTransform);

        if (currentlyEquippedRightHandItemGameObject != null)
        {
            Destroy(currentlyEquippedRightHandItemGameObject);
        }

        isRightHandItemEquipped = true;
        rightHoldIkWeightVelocity = 0f;
        rightCurrentHoldIkWeight = 0f;

        ApplyImmediateHoldIkWeight(
            ref rightCurrentHoldIkWeight,
            holdRigLayer,
            holdRightArmTwoBoneIkConstraint
        );

        LanternHandleFixedJointFollower handleFollower =
            spawnedLanternGameObject.GetComponentInChildren<LanternHandleFixedJointFollower>();

        if (handleFollower != null)
        {
            handleFollower.BindToHandSocket(rightHandSocketTransform);
        }

        currentlyEquippedRightHandItemGameObject = spawnedLanternGameObject;
    }

    private void EquipLanternToLeftHand()
    {
        if (leftHandSocketTransform == null)
        {
            return;
        }

        GameObject spawnedLanternGameObject = SpawnEquippedLantern(leftHandSocketTransform);

        if (currentlyEquippedLeftHandItemGameObject != null)
        {
            Destroy(currentlyEquippedLeftHandItemGameObject);
        }

        isLeftHandItemEquipped = true;
        leftHoldIkWeightVelocity = 0f;
        leftCurrentHoldIkWeight = 0f;

        ApplyImmediateHoldIkWeight(
            ref leftCurrentHoldIkWeight,
            leftHoldRigLayer,
            holdLeftArmTwoBoneIkConstraint
        );

        LanternHandleFixedJointFollower handleFollower =
            spawnedLanternGameObject.GetComponentInChildren<LanternHandleFixedJointFollower>();

        if (handleFollower != null)
        {
            handleFollower.BindToHandSocket(leftHandSocketTransform);
        }

        currentlyEquippedLeftHandItemGameObject = spawnedLanternGameObject;
    }

    private GameObject SpawnEquippedLantern(Transform handSocketTransform)
    {
        GameObject spawnedLanternGameObject = Instantiate(lanternEquippedPrefab);

        BoxCollider spawnedLanternBoxCollider =
            spawnedLanternGameObject.GetComponent<BoxCollider>();
        if (spawnedLanternBoxCollider != null)
        {
            spawnedLanternBoxCollider.enabled = false;
        }

        spawnedLanternGameObject.transform.position = handSocketTransform.TransformPoint(
            lanternLocalPositionOffset
        );
        spawnedLanternGameObject.transform.rotation =
            handSocketTransform.rotation * Quaternion.Euler(lanternLocalEulerAnglesOffset);

        return spawnedLanternGameObject;
    }

    private void UpdateHoldIkWeight(
        ref float currentHoldIkWeight,
        ref float holdIkWeightVelocity,
        Rig handHoldRigLayer,
        TwoBoneIKConstraint holdArmTwoBoneIkConstraint
    )
    {
        float desiredHoldIkWeight = ResolveDesiredHoldIkWeight();
        currentHoldIkWeight = Mathf.SmoothDamp(
            currentHoldIkWeight,
            desiredHoldIkWeight,
            ref holdIkWeightVelocity,
            Mathf.Max(0.01f, holdIkWeightSmoothingSeconds)
        );

        if (handHoldRigLayer != null)
        {
            handHoldRigLayer.weight = currentHoldIkWeight;
        }

        if (holdArmTwoBoneIkConstraint != null)
        {
            holdArmTwoBoneIkConstraint.weight = currentHoldIkWeight;
        }
    }

    private void ApplyImmediateHoldIkWeight(
        ref float currentHoldIkWeight,
        Rig handHoldRigLayer,
        TwoBoneIKConstraint holdArmTwoBoneIkConstraint
    )
    {
        float desiredHoldIkWeight = ResolveDesiredHoldIkWeight();

        if (handHoldRigLayer != null)
        {
            handHoldRigLayer.weight = desiredHoldIkWeight;
        }

        if (holdArmTwoBoneIkConstraint != null)
        {
            holdArmTwoBoneIkConstraint.weight = desiredHoldIkWeight;
        }

        currentHoldIkWeight = desiredHoldIkWeight;
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

public enum PickupHandSide
{
    Right,
    Left
}
