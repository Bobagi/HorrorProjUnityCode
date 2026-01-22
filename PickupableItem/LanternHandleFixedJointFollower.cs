using UnityEngine;

public sealed class LanternHandleFixedJointFollower : MonoBehaviour
{
    [SerializeField]
    private Rigidbody handleRigidbody;

    [SerializeField]
    private Transform handleGripPoint;

    private Transform handSocketTransformToFollow;
    private Vector3 cachedHandSocketWorldPosition;
    private Quaternion cachedHandSocketWorldRotation;
    private Vector3 gripPointLocalPosition;
    private Quaternion gripPointLocalRotation;

    public void BindToHandSocket(Transform handSocketTransform)
    {
        if (handSocketTransform == null || handleRigidbody == null || handleGripPoint == null)
        {
            return;
        }

        handSocketTransformToFollow = handSocketTransform;

        handleRigidbody.isKinematic = true;
        handleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        CacheGripPointLocalPose();

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
    }

    private void FixedUpdate()
    {
        if (handSocketTransformToFollow == null || handleRigidbody == null)
        {
            return;
        }

        Quaternion targetHandleRotation =
            cachedHandSocketWorldRotation * Quaternion.Inverse(gripPointLocalRotation);
        Vector3 targetHandlePosition =
            cachedHandSocketWorldPosition - targetHandleRotation * gripPointLocalPosition;

        handleRigidbody.MoveRotation(targetHandleRotation);
        handleRigidbody.MovePosition(targetHandlePosition);
    }

    private void CacheHandSocketPose()
    {
        cachedHandSocketWorldPosition = handSocketTransformToFollow.position;
        cachedHandSocketWorldRotation = handSocketTransformToFollow.rotation;
    }

    private void ApplyHandlePoseImmediate()
    {
        if (handleRigidbody == null || handSocketTransformToFollow == null)
        {
            return;
        }

        Quaternion targetHandleRotation =
            cachedHandSocketWorldRotation * Quaternion.Inverse(gripPointLocalRotation);
        Vector3 targetHandlePosition =
            cachedHandSocketWorldPosition - targetHandleRotation * gripPointLocalPosition;

        handleRigidbody.position = targetHandlePosition;
        handleRigidbody.rotation = targetHandleRotation;
    }

    private void CacheGripPointLocalPose()
    {
        gripPointLocalPosition = handleRigidbody.transform.InverseTransformPoint(
            handleGripPoint.position
        );
        gripPointLocalRotation =
            Quaternion.Inverse(handleRigidbody.rotation) * handleGripPoint.rotation;
    }
}
