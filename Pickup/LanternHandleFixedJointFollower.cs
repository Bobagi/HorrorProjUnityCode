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

    private Transform handSocketTransformToFollow;

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

        handleRigidbody.isKinematic = true;
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

        Quaternion desiredHandleWorldRotation =
            cachedHandSocketWorldRotation * Quaternion.Inverse(gripLocalRotationFromHandle);

        Vector3 desiredHandleWorldPosition =
            cachedHandSocketWorldPosition
            - (desiredHandleWorldRotation * gripLocalPositionFromHandle);

        handleRigidbody.MoveRotation(desiredHandleWorldRotation);
        handleRigidbody.MovePosition(desiredHandleWorldPosition);
    }

    private void CacheHandSocketPose()
    {
        cachedHandSocketWorldPosition = handSocketTransformToFollow.position;
        cachedHandSocketWorldRotation = handSocketTransformToFollow.rotation;
    }

    private bool OffsetsChanged()
    {
        return gripEulerAnglesRotationOffset != lastGripEulerAnglesRotationOffset
            || gripLocalPositionOffset != lastGripLocalPositionOffset;
    }

    private void RecalculateGripOffsets()
    {
        Vector3 gripLocalPositionWithoutOffset = handleRigidbody.transform.InverseTransformPoint(
            lanternHandleGripTransform.position
        );

        gripLocalPositionFromHandle = gripLocalPositionWithoutOffset + gripLocalPositionOffset;

        Quaternion gripWorldRotation =
            lanternHandleGripTransform.rotation * Quaternion.Euler(gripEulerAnglesRotationOffset);

        gripLocalRotationFromHandle =
            Quaternion.Inverse(handleRigidbody.transform.rotation) * gripWorldRotation;

        lastGripEulerAnglesRotationOffset = gripEulerAnglesRotationOffset;
        lastGripLocalPositionOffset = gripLocalPositionOffset;
    }

    private void ApplyHandlePoseImmediate()
    {
        Quaternion desiredHandleWorldRotation =
            cachedHandSocketWorldRotation * Quaternion.Inverse(gripLocalRotationFromHandle);

        Vector3 desiredHandleWorldPosition =
            cachedHandSocketWorldPosition
            - (desiredHandleWorldRotation * gripLocalPositionFromHandle);

        handleRigidbody.position = desiredHandleWorldPosition;
        handleRigidbody.rotation = desiredHandleWorldRotation;
    }
}
