using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LanternRaiseAndCameraZoomController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private PickedUpItemTypeHandler pickedUpItemTypeHandler;

    [SerializeField]
    private StarterAssets.ThirdPersonController thirdPersonController;

    [Header("Input")]
    [SerializeField]
    private bool keepHandRaised = false;

    [SerializeField]
    private InputActionReference raiseLanternInputActionReference;

    [Header("Hold IK Transforms")]
    [SerializeField]
    private Transform holdRightHandIkTargetTransform;

    [SerializeField]
    private Transform holdLeftHandIkTargetTransform;

    [SerializeField]
    private Transform holdNormalReferenceTransform;

    [SerializeField]
    private Transform holdRaisedReferenceTransform;

    [Header("Force max IK weight")]
    [SerializeField]
    private bool forceMaxWeight = false;

    [Header("Cinemachine")]
    [SerializeField]
    private CinemachineCamera playerFollowCinemachineCamera;

    [SerializeField]
    private float zoomedFieldOfView = 32f;

    [SerializeField]
    private float zoomedCameraDistance = 3.2f;

    [SerializeField]
    private float cameraZoomSmoothingSeconds = 0.10f;

    [Header("Hold IK Transition")]
    [SerializeField]
    private float holdTargetTransitionSeconds = 0.12f;

    [Header("Aim Rotation")]
    [SerializeField]
    private bool forceLookAtCameraTarget = true;

    [SerializeField]
    private float aimRotationSmoothingSeconds = 0.06f;

    private bool isRaiseActive;

    private float defaultCameraFieldOfView;
    private float defaultCameraDistance;

    private float cameraFieldOfViewVelocity;
    private float cameraDistanceVelocity;
    private float aimRotationVelocity;

    private CinemachineThirdPersonFollow cachedThirdPersonFollow;

    private void Awake()
    {
        CacheCameraDefaults();
    }

    private void OnEnable()
    {
        if (raiseLanternInputActionReference != null)
        {
            raiseLanternInputActionReference.action.Enable();
            raiseLanternInputActionReference.action.started += HandleRaiseStarted;
            raiseLanternInputActionReference.action.canceled += HandleRaiseCanceled;
        }
    }

    private void OnDisable()
    {
        if (raiseLanternInputActionReference != null)
        {
            raiseLanternInputActionReference.action.started -= HandleRaiseStarted;
            raiseLanternInputActionReference.action.canceled -= HandleRaiseCanceled;
            raiseLanternInputActionReference.action.Disable();
        }

        isRaiseActive = false;
        MoveHoldTargetsToNormal();
        UpdateCameraZoom(false);
    }

    private void Update()
    {
        bool isLanternEquipped = pickedUpItemTypeHandler != null
            && (pickedUpItemTypeHandler.IsRightHandItemEquipped
                || pickedUpItemTypeHandler.IsLeftHandItemEquipped);

        if (!isLanternEquipped)
        {
            isRaiseActive = false;
            MoveHoldTargetsToNormal();
            UpdateCameraZoom(false);
            return;
        }

        if (forceMaxWeight)
        {
            pickedUpItemTypeHandler.setWeight(1f);
        }

        if (isRaiseActive)
        {
            MoveHoldTargetsToRaised();
        }
        else
        {
            MoveHoldTargetsToNormal();
        }

        UpdateCameraZoom(isRaiseActive);
    }

    private void LateUpdate()
    {
        if (!isRaiseActive || thirdPersonController == null)
        {
            return;
        }

        RotatePlayerTowardsCameraYaw();
    }

    private void HandleRaiseStarted(InputAction.CallbackContext _)
    {
        isRaiseActive = true;
    }

    private void HandleRaiseCanceled(InputAction.CallbackContext _)
    {
        if (!keepHandRaised)
            isRaiseActive = false;
    }

    private void MoveHoldTargetsToNormal()
    {
        MoveHoldTargetTowards(holdRightHandIkTargetTransform, holdNormalReferenceTransform);
        MoveHoldTargetTowards(holdLeftHandIkTargetTransform, holdNormalReferenceTransform);
    }

    private void MoveHoldTargetsToRaised()
    {
        MoveHoldTargetTowards(holdRightHandIkTargetTransform, holdRaisedReferenceTransform);
        MoveHoldTargetTowards(holdLeftHandIkTargetTransform, holdRaisedReferenceTransform);
    }

    private void MoveHoldTargetTowards(Transform holdTargetTransform, Transform referenceTransform)
    {
        if (holdTargetTransform == null || referenceTransform == null)
        {
            return;
        }

        float smoothing = Mathf.Max(0.01f, holdTargetTransitionSeconds);
        float t = 1f - Mathf.Exp(-Time.deltaTime / smoothing);

        holdTargetTransform.position = Vector3.Lerp(
            holdTargetTransform.position,
            referenceTransform.position,
            t
        );

        holdTargetTransform.rotation = Quaternion.Slerp(
            holdTargetTransform.rotation,
            referenceTransform.rotation,
            t
        );
    }

    private void CacheCameraDefaults()
    {
        if (playerFollowCinemachineCamera == null)
        {
            return;
        }

        defaultCameraFieldOfView = playerFollowCinemachineCamera.Lens.FieldOfView;

        cachedThirdPersonFollow =
            playerFollowCinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();

        if (cachedThirdPersonFollow != null)
        {
            defaultCameraDistance = cachedThirdPersonFollow.CameraDistance;
        }
    }

    private void UpdateCameraZoom(bool isZoomed)
    {
        if (playerFollowCinemachineCamera == null)
        {
            return;
        }

        float desiredFieldOfView = isZoomed ? zoomedFieldOfView : defaultCameraFieldOfView;

        float smoothing = Mathf.Max(0.01f, cameraZoomSmoothingSeconds);

        float blendedFov = Mathf.SmoothDamp(
            playerFollowCinemachineCamera.Lens.FieldOfView,
            desiredFieldOfView,
            ref cameraFieldOfViewVelocity,
            smoothing
        );

        LensSettings lens = playerFollowCinemachineCamera.Lens;
        lens.FieldOfView = blendedFov;
        playerFollowCinemachineCamera.Lens = lens;

        if (cachedThirdPersonFollow == null)
        {
            return;
        }

        float desiredDistance = isZoomed ? zoomedCameraDistance : defaultCameraDistance;

        float blendedDistance = Mathf.SmoothDamp(
            cachedThirdPersonFollow.CameraDistance,
            desiredDistance,
            ref cameraDistanceVelocity,
            smoothing
        );

        cachedThirdPersonFollow.CameraDistance = blendedDistance;
    }

    private void RotatePlayerTowardsCameraYaw()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 planarForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up);

        if (planarForward.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float desiredYaw = Quaternion.LookRotation(planarForward, Vector3.up).eulerAngles.y;

        float currentYaw = thirdPersonController.transform.eulerAngles.y;

        if (forceLookAtCameraTarget)
        {
            float smoothedYaw = Mathf.SmoothDampAngle(
                currentYaw,
                desiredYaw,
                ref aimRotationVelocity,
                Mathf.Max(0.01f, aimRotationSmoothingSeconds)
            );

            thirdPersonController.transform.rotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }
    }
}
