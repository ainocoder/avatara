using UnityEngine;

public class ResolutionController : MonoBehaviour
{
    [Header("강제 해상도 설정")]
    public int targetWidth = 1152;  // 768 * 1.5 = 1152 (좌우 폭 1.5배 확장)
    public int targetHeight = 1344;
    public bool fullScreen = false;
    
    [Header("디버그 정보")]
    public bool showDebugInfo = true;

    void Awake()
    {
        // 게임 시작 시 해상도 강제 설정
        SetForceResolution();
    }

    void Start()
    {
        // Awake에서 설정되지 않았을 경우를 대비한 추가 호출
        SetForceResolution();
        
        if (showDebugInfo)
        {
            Debug.Log($"Current Resolution: {Screen.width}x{Screen.height}");
            Debug.Log($"Target Resolution: {targetWidth}x{targetHeight}");
        }
    }

    public void SetForceResolution()
    {
        // 현재 해상도가 목표 해상도와 다를 경우에만 변경
        if (Screen.width != targetWidth || Screen.height != targetHeight)
        {
            Screen.SetResolution(targetWidth, targetHeight, fullScreen);
            
            if (showDebugInfo)
            {
                Debug.Log($"Resolution forced to: {targetWidth}x{targetHeight}");
            }
        }
    }

    // Inspector에서 테스트할 수 있도록 하는 메서드
    [ContextMenu("Force Resolution Now")]
    public void ForceResolutionNow()
    {
        SetForceResolution();
    }

    // 해상도가 변경되었을 때 다시 강제 설정
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SetForceResolution();
        }
    }

    // 창 크기가 변경되었을 때도 강제 설정
    void Update()
    {
        // 매 프레임마다 체크하지 않고 필요시에만 체크
        if (Time.frameCount % 60 == 0) // 1초마다 체크
        {
            if (Screen.width != targetWidth || Screen.height != targetHeight)
            {
                SetForceResolution();
            }
        }
    }
} 