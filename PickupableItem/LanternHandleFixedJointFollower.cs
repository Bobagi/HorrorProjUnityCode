using UnityEngine;

public sealed class LanternHandleFixedJointFollower : MonoBehaviour
{
    [SerializeField]
    private Rigidbody handleRigidbody;

    private Transform handSocketTransformToFollow;
    private Vector3 cachedHandSocketWorldPosition;
    private Quaternion cachedHandSocketWorldRotation;

    public void BindToHandSocket(Transform handSocketTransform)
    {
        if (handSocketTransform == null || handleRigidbody == null)
        {
            return;
        }

        handSocketTransformToFollow = handSocketTransform;

        handleRigidbody.isKinematic = true;
        handleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

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

        handleRigidbody.MoveRotation(cachedHandSocketWorldRotation);
        handleRigidbody.MovePosition(cachedHandSocketWorldPosition);
    }

    private void CacheHandSocketPose()
    {
        cachedHandSocketWorldPosition = handSocketTransformToFollow.position;
        cachedHandSocketWorldRotation = handSocketTransformToFollow.rotation;
    }

    private void ApplyHandlePoseImmediate()
    {
        handleRigidbody.position = cachedHandSocketWorldPosition;
        handleRigidbody.rotation = cachedHandSocketWorldRotation;
    }
}
