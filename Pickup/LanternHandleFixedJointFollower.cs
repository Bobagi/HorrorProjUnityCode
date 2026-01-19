using UnityEngine;

public sealed class LanternHandleFixedJointFollower : MonoBehaviour
{
    [SerializeField]
    private Rigidbody handleRigidbody;

    [SerializeField]
    private Transform lanternHandleGripTransform;

    [SerializeField]
    private Vector3 gripEulerAnglesRotationOffset;

    [SerializeField]
    private Vector3 gripLocalPositionOffset;

    [Header("Rotation Lock")]
    [SerializeField]
    private bool lockHandleRotationToHand = true;

    [Header("Left Hand Overrides")]
    [SerializeField]
    private Vector3 leftHandGripEulerAnglesRotationOffset = new Vector3(0f, 0f, 180f);

    [SerializeField]
    private Vector3 leftHandGripLocalPositionOffset;

    private Transform handSocketTransformToFollow;
    private bool isFollowingLeftHandSocket;

    private Vector3 gripLocalPositionFromHandle;
    private Quaternion gripLocalRotationFromHandle;

    private Vector3 cachedHandSocketWorldPosition;
    private Quaternion cachedHandSocketWorldRotation;

    private Vector3 lastGripEulerAnglesRotationOffset;
    private Vector3 lastGripLocalPositionOffset;

    public void BindToHandSocket(Transform handSocketTransform)
    {
        if (
            handSocketTransform == null
            || handleRigidbody == null
            || lanternHandleGripTransform == null
        )
        {
            return;
        }

        handSocketTransformToFollow = handSocketTransform;
        isFollowingLeftHandSocket = IsLeftHandSocket(handSocketTransform);

        handleRigidbody.isKinematic = lockHandleRotationToHand;
        handleRigidbody.useGravity = false;
        handleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        RecalculateGripOffsets();
        CacheHandSocketPose();
        ApplyHandlePoseImmediate();
    }

    private void LateUpdate()
    {
        if (handSocketTransformToFollow == null)
        {
            return;
        }

        CacheHandSocketPose();

        if (OffsetsChanged())
        {
            RecalculateGripOffsets();
            ApplyHandlePoseImmediate();
        }
    }

    private void FixedUpdate()
    {
        if (handSocketTransformToFollow == null || handleRigidbody == null)
        {
            return;
        }

        if (lockHandleRotationToHand)
        {
            Quaternion desiredHandleWorldRotation =
                cachedHandSocketWorldRotation * Quaternion.Inverse(gripLocalRotationFromHandle);

            Vector3 desiredHandleWorldPosition =
                cachedHandSocketWorldPosition
                - (desiredHandleWorldRotation * gripLocalPositionFromHandle);

            handleRigidbody.MoveRotation(desiredHandleWorldRotation);
            handleRigidbody.MovePosition(desiredHandleWorldPosition);
            return;
        }

        Quaternion currentHandleWorldRotation = handleRigidbody.rotation;
        Vector3 desiredPositionWithFreeRotation =
            cachedHandSocketWorldPosition
            - (currentHandleWorldRotation * gripLocalPositionFromHandle);

        handleRigidbody.MovePosition(desiredPositionWithFreeRotation);
    }

    private void CacheHandSocketPose()
    {
        cachedHandSocketWorldPosition = handSocketTransformToFollow.position;
        cachedHandSocketWorldRotation = handSocketTransformToFollow.rotation;
    }

    private bool OffsetsChanged()
    {
        Vector3 effectiveRotationOffset = ResolveGripEulerAnglesRotationOffset();
        Vector3 effectivePositionOffset = ResolveGripLocalPositionOffset();

        return effectiveRotationOffset != lastGripEulerAnglesRotationOffset
            || effectivePositionOffset != lastGripLocalPositionOffset;
    }

    private void RecalculateGripOffsets()
    {
        Vector3 gripLocalPositionWithoutOffset = handleRigidbody.transform.InverseTransformPoint(
            lanternHandleGripTransform.position
        );

        gripLocalPositionFromHandle =
            gripLocalPositionWithoutOffset + ResolveGripLocalPositionOffset();

        Vector3 effectiveRotationOffset = ResolveGripEulerAnglesRotationOffset();
        Quaternion gripWorldRotation =
            lanternHandleGripTransform.rotation * Quaternion.Euler(effectiveRotationOffset);

        gripLocalRotationFromHandle =
            Quaternion.Inverse(handleRigidbody.transform.rotation) * gripWorldRotation;

        lastGripEulerAnglesRotationOffset = effectiveRotationOffset;
        lastGripLocalPositionOffset = ResolveGripLocalPositionOffset();
    }

    private void ApplyHandlePoseImmediate()
    {
        Quaternion desiredHandleWorldRotation =
            cachedHandSocketWorldRotation * Quaternion.Inverse(gripLocalRotationFromHandle);

        if (lockHandleRotationToHand)
        {
            Vector3 desiredHandleWorldPosition =
                cachedHandSocketWorldPosition
                - (desiredHandleWorldRotation * gripLocalPositionFromHandle);

            handleRigidbody.position = desiredHandleWorldPosition;
            handleRigidbody.rotation = desiredHandleWorldRotation;
            return;
        }

        Quaternion currentHandleWorldRotation = handleRigidbody.rotation;
        Vector3 desiredPositionWithFreeRotation =
            cachedHandSocketWorldPosition
            - (currentHandleWorldRotation * gripLocalPositionFromHandle);

        handleRigidbody.position = desiredPositionWithFreeRotation;
    }

    private Vector3 ResolveGripEulerAnglesRotationOffset()
    {
        if (!isFollowingLeftHandSocket)
        {
            return gripEulerAnglesRotationOffset;
        }

        return gripEulerAnglesRotationOffset + leftHandGripEulerAnglesRotationOffset;
    }

    private Vector3 ResolveGripLocalPositionOffset()
    {
        if (!isFollowingLeftHandSocket)
        {
            return gripLocalPositionOffset;
        }

        return gripLocalPositionOffset + leftHandGripLocalPositionOffset;
    }

    private static bool IsLeftHandSocket(Transform handSocketTransform)
    {
        if (handSocketTransform == null)
        {
            return false;
        }

        string socketName = handSocketTransform.name;
        return !string.IsNullOrEmpty(socketName)
            && socketName.IndexOf("left", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
