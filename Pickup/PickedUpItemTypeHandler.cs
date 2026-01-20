using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class PickedUpItemTypeHandler : MonoBehaviour
{
    [Header("Hand Socket")]
    [SerializeField]
    private Transform rightHandSocketTransform;

    [SerializeField]
    private Transform leftHandSocketTransform;

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

    public void HandlePickedUpItem(
        InteractablePickupItem pickedUpItem,
        PickupHandSide pickupHandSide
    )
    {
        if (pickedUpItem == null)
        {
            return;
        }

        EquipItem(pickedUpItem, pickupHandSide);
    }

    private void EquipItem(InteractablePickupItem pickedUpItem, PickupHandSide pickupHandSide)
    {
        if (pickedUpItem == null || pickedUpItem.EquippedPrefab == null)
        {
            return;
        }

        if (pickupHandSide == PickupHandSide.Left)
        {
            EquipItemToLeftHand(pickedUpItem);
            return;
        }

        EquipItemToRightHand(pickedUpItem);
    }

    private void EquipItemToRightHand(InteractablePickupItem pickedUpItem)
    {
        if (rightHandSocketTransform == null)
        {
            return;
        }

        GameObject spawnedItemGameObject = SpawnEquippedItem(
            pickedUpItem,
            rightHandSocketTransform
        );

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

        BindFollowerIfPresent(spawnedItemGameObject, pickedUpItem, rightHandSocketTransform);
        currentlyEquippedRightHandItemGameObject = spawnedItemGameObject;
    }

    private void EquipItemToLeftHand(InteractablePickupItem pickedUpItem)
    {
        if (leftHandSocketTransform == null)
        {
            return;
        }

        GameObject spawnedItemGameObject = SpawnEquippedItem(pickedUpItem, leftHandSocketTransform);

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

        BindFollowerIfPresent(spawnedItemGameObject, pickedUpItem, leftHandSocketTransform);
        currentlyEquippedLeftHandItemGameObject = spawnedItemGameObject;
    }

    private GameObject SpawnEquippedItem(
        InteractablePickupItem pickedUpItem,
        Transform handSocketTransform
    )
    {
        if (pickedUpItem == null || pickedUpItem.EquippedPrefab == null)
        {
            return null;
        }

        GameObject spawnedItemGameObject = Instantiate(pickedUpItem.EquippedPrefab);
        DisableItemColliders(spawnedItemGameObject);

        if (pickedUpItem.EquippedItemFollowMode == EquippedItemFollowMode.AttachToSocket)
        {
            spawnedItemGameObject.transform.SetParent(handSocketTransform, false);
            spawnedItemGameObject.transform.localPosition = Vector3.zero;
            spawnedItemGameObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            spawnedItemGameObject.transform.position = handSocketTransform.position;
            spawnedItemGameObject.transform.rotation = handSocketTransform.rotation;
        }

        return spawnedItemGameObject;
    }

    private static void DisableItemColliders(GameObject spawnedItemGameObject)
    {
        if (spawnedItemGameObject == null)
        {
            return;
        }

        Collider[] colliders = spawnedItemGameObject.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private static void BindFollowerIfPresent(
        GameObject spawnedItemGameObject,
        InteractablePickupItem pickedUpItem,
        Transform handSocketTransform
    )
    {
        if (
            spawnedItemGameObject == null
            || pickedUpItem == null
            || pickedUpItem.EquippedItemFollowMode != EquippedItemFollowMode.PhysicsFollower
            || handSocketTransform == null
        )
        {
            return;
        }

        LanternHandleFixedJointFollower handleFollower =
            spawnedItemGameObject.GetComponentInChildren<LanternHandleFixedJointFollower>();

        if (handleFollower != null)
        {
            handleFollower.BindToHandSocket(handSocketTransform);
        }
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
    Left,
}
