using UnityEngine;

public sealed class LanternHandleFixedJointFollower : MonoBehaviour
{
    [SerializeField]
    private Rigidbody handleRigidbody;

    [SerializeField]
    private Transform handleGripPoint;

    [SerializeField]
    private FixedJoint handleFixedJoint;

    private Transform handSocketTransformToFollow;
    private Rigidbody handSocketRigidbody;
    private Vector3 cachedHandSocketWorldPosition;
    private Quaternion cachedHandSocketWorldRotation;

    public void BindToHandSocket(Transform handSocketTransform)
    {
        if (handSocketTransform == null || handleRigidbody == null || handleGripPoint == null)
        {
            return;
        }

        handSocketTransformToFollow = handSocketTransform;

        handleRigidbody.isKinematic = false;
        handleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        EnsureHandSocketRigidbody();
        EnsureHandleJoint();

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
        if (handSocketTransformToFollow == null || handSocketRigidbody == null)
        {
            return;
        }

        handSocketRigidbody.MoveRotation(cachedHandSocketWorldRotation);
        handSocketRigidbody.MovePosition(cachedHandSocketWorldPosition);
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

        Quaternion gripToHandleRotation =
            Quaternion.Inverse(handleGripPoint.rotation) * handleRigidbody.rotation;
        Vector3 gripToHandleOffset = handleRigidbody.position - handleGripPoint.position;

        handleRigidbody.position = cachedHandSocketWorldPosition + gripToHandleOffset;
        handleRigidbody.rotation = cachedHandSocketWorldRotation * gripToHandleRotation;
    }

    private void EnsureHandSocketRigidbody()
    {
        if (handSocketRigidbody != null)
        {
            return;
        }

        GameObject handSocketAnchor = new GameObject("LanternHandSocketAnchor");
        handSocketAnchor.transform.position = handSocketTransformToFollow.position;
        handSocketAnchor.transform.rotation = handSocketTransformToFollow.rotation;

        handSocketRigidbody = handSocketAnchor.AddComponent<Rigidbody>();
        handSocketRigidbody.isKinematic = true;
        handSocketRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void EnsureHandleJoint()
    {
        if (handleFixedJoint == null)
        {
            handleFixedJoint = handleRigidbody.GetComponent<FixedJoint>();
        }

        if (handleFixedJoint == null)
        {
            handleFixedJoint = handleRigidbody.gameObject.AddComponent<FixedJoint>();
        }

        handleFixedJoint.connectedBody = handSocketRigidbody;
        handleFixedJoint.autoConfigureConnectedAnchor = false;
        handleFixedJoint.anchor =
            handleRigidbody.transform.InverseTransformPoint(handleGripPoint.position);
        handleFixedJoint.connectedAnchor = Vector3.zero;
    }

    private void OnDisable()
    {
        if (handSocketRigidbody != null)
        {
            Destroy(handSocketRigidbody.gameObject);
            handSocketRigidbody = null;
        }
    }
}
