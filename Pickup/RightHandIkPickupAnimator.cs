using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class RightHandIkPickupAnimator : MonoBehaviour
{
    [Header("Rigging")]
    [SerializeField]
    private Transform rightHandIkTargetTransform;

    [SerializeField]
    private Rig reachRigLayer;

    [SerializeField]
    private TwoBoneIKConstraint rightArmReachTwoBoneIkConstraint;

    [Header("Reach Settings")]
    [SerializeField]
    private float reachRigWeightWhenActive = 1f;

    [SerializeField]
    private float returnToOriginalSeconds = 0.12f;

    [Header("Target Smoothing")]
    [SerializeField]
    private float targetFollowSmoothingSeconds = 0.06f;

    private Coroutine currentReachCoroutine;
    private Vector3 rightHandIkTargetPositionVelocity;

    public IEnumerator PlayReachToTargetCoroutine(
        Transform worldTargetTransform,
        float animationSeconds
    )
    {
        if (worldTargetTransform == null)
        {
            yield break;
        }

        if (rightHandIkTargetTransform == null)
        {
            yield break;
        }

        if (reachRigLayer == null || rightArmReachTwoBoneIkConstraint == null)
        {
            yield break;
        }

        float clampedAnimationSeconds = Mathf.Max(0.01f, animationSeconds);

        if (currentReachCoroutine != null)
        {
            StopCoroutine(currentReachCoroutine);
        }

        currentReachCoroutine = StartCoroutine(
            ReachCoroutine(worldTargetTransform, clampedAnimationSeconds)
        );
        yield return currentReachCoroutine;
        currentReachCoroutine = null;
    }

    private IEnumerator ReachCoroutine(Transform worldTargetTransform, float animationSeconds)
    {
        Vector3 originalTargetPosition = rightHandIkTargetTransform.position;
        Quaternion originalTargetRotation = rightHandIkTargetTransform.rotation;

        reachRigLayer.weight = reachRigWeightWhenActive;
        rightArmReachTwoBoneIkConstraint.weight = 1f;

        Vector3 reachEndPosition = worldTargetTransform.position;
        Quaternion reachEndRotation = worldTargetTransform.rotation;

        float elapsedSeconds = 0f;

        while (elapsedSeconds < animationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedSeconds / animationSeconds);
            float smoothedTime = normalizedTime * normalizedTime * (3f - (2f * normalizedTime));

            Vector3 desiredPosition = Vector3.Lerp(
                originalTargetPosition,
                reachEndPosition,
                smoothedTime
            );
            Quaternion desiredRotation = Quaternion.Slerp(
                originalTargetRotation,
                reachEndRotation,
                smoothedTime
            );

            float clampedFollowSeconds = Mathf.Max(0.01f, targetFollowSmoothingSeconds);
            rightHandIkTargetTransform.position = Vector3.SmoothDamp(
                rightHandIkTargetTransform.position,
                desiredPosition,
                ref rightHandIkTargetPositionVelocity,
                clampedFollowSeconds
            );

            rightHandIkTargetTransform.rotation = Quaternion.Slerp(
                rightHandIkTargetTransform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-Time.deltaTime / clampedFollowSeconds)
            );

            yield return null;
        }

        rightHandIkTargetTransform.position = reachEndPosition;
        rightHandIkTargetTransform.rotation = reachEndRotation;

        float clampedReturnSeconds = Mathf.Max(0.01f, returnToOriginalSeconds);
        float returnElapsedSeconds = 0f;
        rightHandIkTargetPositionVelocity = Vector3.zero;

        while (returnElapsedSeconds < clampedReturnSeconds)
        {
            returnElapsedSeconds += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(returnElapsedSeconds / clampedReturnSeconds);
            float smoothedTime = normalizedTime * normalizedTime * (3f - (2f * normalizedTime));

            Vector3 desiredPosition = Vector3.Lerp(
                reachEndPosition,
                originalTargetPosition,
                smoothedTime
            );
            Quaternion desiredRotation = Quaternion.Slerp(
                reachEndRotation,
                originalTargetRotation,
                smoothedTime
            );

            float clampedFollowSeconds = Mathf.Max(0.01f, targetFollowSmoothingSeconds);
            rightHandIkTargetTransform.position = Vector3.SmoothDamp(
                rightHandIkTargetTransform.position,
                desiredPosition,
                ref rightHandIkTargetPositionVelocity,
                clampedFollowSeconds
            );

            rightHandIkTargetTransform.rotation = Quaternion.Slerp(
                rightHandIkTargetTransform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-Time.deltaTime / clampedFollowSeconds)
            );

            yield return null;
        }

        rightHandIkTargetTransform.position = originalTargetPosition;
        rightHandIkTargetTransform.rotation = originalTargetRotation;

        rightArmReachTwoBoneIkConstraint.weight = 0f;
        reachRigLayer.weight = 0f;
    }
}
