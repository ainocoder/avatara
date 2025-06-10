using Convai.Scripts.Runtime.Addons;
using UnityEngine;

/// <summary>
/// 관리자용 카메라 고정 설정 컴포넌트
/// Unity Inspector에서 쉽게 카메라 이동을 제어할 수 있습니다.
/// </summary>
[AddComponentMenu("Convai/Camera Lock Manager")]
[HelpURL("https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/scripts-overview")]
public class ConvaiCameraLockManager : MonoBehaviour
{
    [Header("카메라 제어 설정 (관리자 전용)")]
    [SerializeField]
    [Tooltip("체크하면 카메라가 고정되어 플레이어가 이동할 수 없습니다.")]
    public bool lockCameraMovement = false;
    
    [SerializeField]
    [Tooltip("체크하면 마우스 룩(회전)은 허용하되 위치 이동만 제한합니다.")]
    public bool allowLookAround = true;
    
    [Header("정보")]
    [SerializeField]
    [Tooltip("현재 카메라 고정 상태를 표시합니다.")]
    private bool isCurrentlyLocked = false;
    
    [SerializeField]
    [Tooltip("연결된 플레이어 움직임 컴포넌트")]
    private ConvaiPlayerMovement playerMovement;

    //Singleton Instance
    public static ConvaiCameraLockManager Instance { get; private set; }

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
        // 플레이어 움직임 컴포넌트 찾기
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<ConvaiPlayerMovement>();
            
        if (playerMovement == null)
            Debug.LogWarning("[ConvaiCameraLockManager] ConvaiPlayerMovement 컴포넌트를 찾을 수 없습니다!");
    }

    private void Update()
    {
        // 상태 변경 체크 및 적용
        CheckAndApplyLockState();
    }
    
    private void CheckAndApplyLockState()
    {
        if (playerMovement == null) return;
        
        // 상태가 변경되었는지 확인
        if (isCurrentlyLocked != lockCameraMovement)
        {
            isCurrentlyLocked = lockCameraMovement;
            ApplyLockSettings();
        }
        
        // 둘러보기 설정도 실시간으로 업데이트
        if (lockCameraMovement)
        {
            playerMovement.SetLookAroundAllowed(allowLookAround);
        }
    }
    
    private void ApplyLockSettings()
    {
        if (playerMovement == null) return;
        
        // ConvaiPlayerMovement의 공개 메서드를 사용하여 설정 적용
        playerMovement.SetMovementLock(lockCameraMovement);
        playerMovement.SetLookAroundAllowed(allowLookAround);
        
        string status = lockCameraMovement ? "활성화" : "비활성화";
        string lookStatus = allowLookAround ? "허용" : "제한";
        
        Debug.Log($"[ConvaiCameraLockManager] 카메라 이동 고정: {status}, 둘러보기: {lookStatus}");
    }
    
    /// <summary>
    /// 외부에서 카메라 고정 상태를 토글할 수 있는 공개 메서드
    /// </summary>
    public void ToggleCameraLock()
    {
        lockCameraMovement = !lockCameraMovement;
        Debug.Log($"[ConvaiCameraLockManager] 카메라 고정 토글: {(lockCameraMovement ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 현재 카메라 고정 상태를 반환하는 메서드
    /// </summary>
    public bool IsCameraLocked()
    {
        return lockCameraMovement;
    }
    
    /// <summary>
    /// 카메라 고정 상태를 직접 설정하는 메서드
    /// </summary>
    /// <param name="locked">고정 여부</param>
    public void SetCameraLock(bool locked)
    {
        lockCameraMovement = locked;
        Debug.Log($"[ConvaiCameraLockManager] 카메라 고정 상태 설정: {(locked ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 둘러보기 허용 여부를 설정하는 메서드
    /// </summary>
    /// <param name="allow">허용 여부</param>
    public void SetLookAroundAllowed(bool allow)
    {
        allowLookAround = allow;
        if (playerMovement != null && lockCameraMovement)
        {
            playerMovement.SetLookAroundAllowed(allow);
        }
        Debug.Log($"[ConvaiCameraLockManager] 둘러보기 설정: {(allow ? "허용" : "제한")}");
    }
    
    private void OnDestroy()
    {
        // 컴포넌트 제거 시 원본 설정 복원
        if (playerMovement != null && isCurrentlyLocked)
        {
            playerMovement.SetMovementLock(false);
            Debug.Log("[ConvaiCameraLockManager] 컴포넌트 제거로 인한 카메라 고정 해제");
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때마다 호출됨
        if (Application.isPlaying && playerMovement != null)
        {
            CheckAndApplyLockState();
        }
    }
#endif
} 