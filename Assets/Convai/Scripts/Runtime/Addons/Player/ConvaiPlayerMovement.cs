using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Convai.Scripts.Runtime.Addons
{
    /// <summary>
    ///     Class for handling player movement including walking, running, jumping, and looking around.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Convai/Player Movement")]
    [HelpURL("https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/scripts-overview")]
    public class ConvaiPlayerMovement : MonoBehaviour
    {
        [Header("Movement Parameters")] [SerializeField] [Tooltip("The speed at which the player walks.")] [Range(1, 10)]
        private float walkingSpeed = 3f;

        [SerializeField] [Tooltip("The speed at which the player runs.")] [Range(1, 10)]
        private float runningSpeed = 8f;

        [SerializeField] [Tooltip("The speed at which the player jumps.")] [Range(1, 10)]
        private float jumpSpeed = 4f;

        [Header("Gravity & Grounding")] [SerializeField] [Tooltip("The gravity applied to the player.")] [Range(1, 10)]
        private float gravity = 9.8f;

        [Header("Camera Parameters")] [SerializeField] [Tooltip("The main camera the player uses.")]
        private Camera playerCamera;

        [SerializeField] [Tooltip("Speed at which the player can look around.")] [Range(0, 1)]
        private float lookSpeedMultiplier = 0.5f;

        [SerializeField] [Tooltip("Limit of upwards and downwards look angles.")] [Range(1, 90)]
        private float lookXLimit = 45.0f;

        [Header("Movement Lock Settings (Admin Only)")]
        [SerializeField] [Tooltip("관리자 전용: 카메라 이동을 고정합니다. 체험자는 이동할 수 없게 됩니다.")]
        private bool lockMovement = false;

        [SerializeField] [Tooltip("관리자 전용: 카메라 회전(둘러보기)은 허용하되 위치 이동만 제한합니다.")]
        private bool allowLookAround = true;

        private CharacterController _characterController;
        private Vector3 _moveDirection = Vector3.zero;
        private float _rotationX;

        //Singleton Instance
        public static ConvaiPlayerMovement Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern to ensure only one instance exists
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // Check for running state and move the player
            MovePlayer();

            // Handle the player and camera rotation
            RotatePlayerAndCamera();
        }

        private void OnEnable()
        {
            ConvaiInputManager.Instance.jumping += Jump;
        }


        private void MovePlayer()
        {
            Vector3 horizontalMovement = Vector3.zero;

            // 관리자가 이동을 고정했는지 확인
            if (!lockMovement && !EventSystem.current.IsPointerOverGameObject() && !UIUtilities.IsAnyInputFieldFocused())
            {
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);

                float speed = ConvaiInputManager.Instance.isRunning ? runningSpeed : walkingSpeed;

                Vector2 moveVector = ConvaiInputManager.Instance.moveVector;
                float curSpeedX = speed * moveVector.x;
                float curSpeedY = speed * moveVector.y;

                horizontalMovement = forward * curSpeedY + right * curSpeedX;
            }

            if (!_characterController.isGrounded)
                // Apply gravity only when canMove is true
                _moveDirection.y -= gravity * Time.deltaTime;

            // Move the character only if movement is not locked
            if (!lockMovement)
            {
                _characterController.Move((_moveDirection + horizontalMovement) * Time.deltaTime);
            }
        }

        private void Jump()
        {
            // 이동이 고정되어 있으면 점프도 비활성화
            if (!lockMovement && _characterController.isGrounded && !UIUtilities.IsAnyInputFieldFocused()) 
                _moveDirection.y = jumpSpeed;
        }

        private void RotatePlayerAndCamera()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            // 이동이 고정되어 있고 둘러보기도 허용하지 않는 경우 회전 비활성화
            if (lockMovement && !allowLookAround) return;

            // Vertical rotation
            _rotationX -= ConvaiInputManager.Instance.lookVector.y * lookSpeedMultiplier;
            _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

            // Horizontal rotation
            float rotationY = ConvaiInputManager.Instance.lookVector.x * lookSpeedMultiplier;
            transform.rotation *= Quaternion.Euler(0, rotationY, 0);
        }

        /// <summary>
        /// 관리자용: 카메라 이동 고정 상태를 설정합니다.
        /// </summary>
        /// <param name="locked">고정 여부</param>
        public void SetMovementLock(bool locked)
        {
            lockMovement = locked;
            Debug.Log($"[ConvaiPlayerMovement] 이동 고정 상태: {(locked ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 관리자용: 둘러보기 허용 여부를 설정합니다.
        /// </summary>
        /// <param name="allow">허용 여부</param>
        public void SetLookAroundAllowed(bool allow)
        {
            allowLookAround = allow;
            Debug.Log($"[ConvaiPlayerMovement] 둘러보기: {(allow ? "허용" : "제한")}");
        }

        /// <summary>
        /// 관리자용: 현재 이동 고정 상태를 반환합니다.
        /// </summary>
        /// <returns>고정 여부</returns>
        public bool IsMovementLocked()
        {
            return lockMovement;
        }

        /// <summary>
        /// 관리자용: 현재 둘러보기 허용 상태를 반환합니다.
        /// </summary>
        /// <returns>허용 여부</returns>
        public bool IsLookAroundAllowed()
        {
            return allowLookAround;
        }

        /// <summary>
        /// 관리자용: 이동 고정 상태를 토글합니다.
        /// </summary>
        public void ToggleMovementLock()
        {
            SetMovementLock(!lockMovement);
        }
    }
}