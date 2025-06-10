using UnityEngine;
using System.Collections;

public class AdvancedResolutionController : MonoBehaviour
{
    [Header("해상도 설정")]
    public int targetWidth = 768;
    public int targetHeight = 1344;
    public bool forceFullscreen = false;
    
    [Header("다중 디스플레이 설정")]
    public bool applyToAllDisplays = true;
    public int targetDisplay = 0; // 0 = 주 디스플레이
    
    [Header("고급 옵션")]
    public bool maintainAspectRatio = true;
    public bool preventResolutionChange = true;
    public float checkInterval = 1.0f; // 해상도 체크 간격
    
    [Header("디버그")]
    public bool enableLogging = true;
    
    private Coroutine resolutionCheckCoroutine;

    void Awake()
    {
        // DontDestroyOnLoad로 설정하여 씬 전환 시에도 유지
        DontDestroyOnLoad(gameObject);
        
        // 즉시 해상도 설정
        ApplyResolution();
    }

    void Start()
    {
        if (preventResolutionChange)
        {
            // 주기적으로 해상도 체크하는 코루틴 시작
            resolutionCheckCoroutine = StartCoroutine(CheckResolutionPeriodically());
        }
        
        LogDisplayInfo();
    }

    void OnDestroy()
    {
        if (resolutionCheckCoroutine != null)
        {
            StopCoroutine(resolutionCheckCoroutine);
        }
    }

    public void ApplyResolution()
    {
        if (applyToAllDisplays)
        {
            // 모든 디스플레이에 적용
            for (int i = 0; i < Display.displays.Length; i++)
            {
                if (Display.displays[i] != null)
                {
                    SetResolutionForDisplay(i);
                }
            }
        }
        else
        {
            // 지정된 디스플레이에만 적용
            SetResolutionForDisplay(targetDisplay);
        }
    }

    private void SetResolutionForDisplay(int displayIndex)
    {
        try
        {
            if (displayIndex < Display.displays.Length)
            {
                // 주 디스플레이인 경우
                if (displayIndex == 0)
                {
                    Screen.SetResolution(targetWidth, targetHeight, forceFullscreen);
                    
                    if (enableLogging)
                    {
                        Debug.Log($"Primary display resolution set to: {targetWidth}x{targetHeight}");
                    }
                }
                else
                {
                    // 보조 디스플레이 활성화 및 설정
                    Display.displays[displayIndex].Activate();
                    
                    if (enableLogging)
                    {
                        Debug.Log($"Display {displayIndex} activated");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableLogging)
            {
                Debug.LogError($"Failed to set resolution for display {displayIndex}: {e.Message}");
            }
        }
    }

    private IEnumerator CheckResolutionPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            // 현재 해상도가 목표와 다르면 다시 설정
            if (Screen.width != targetWidth || Screen.height != targetHeight)
            {
                if (enableLogging)
                {
                    Debug.Log($"Resolution changed detected. Current: {Screen.width}x{Screen.height}, Target: {targetWidth}x{targetHeight}");
                }
                
                ApplyResolution();
            }
        }
    }

    private void LogDisplayInfo()
    {
        if (!enableLogging) return;
        
        Debug.Log($"Number of displays: {Display.displays.Length}");
        Debug.Log($"Current screen resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"Target resolution: {targetWidth}x{targetHeight}");
        
        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];
            Debug.Log($"Display {i}: {display.systemWidth}x{display.systemHeight}");
        }
    }

    // 런타임에서 해상도 변경을 위한 public 메서드들
    public void SetTargetResolution(int width, int height)
    {
        targetWidth = width;
        targetHeight = height;
        ApplyResolution();
    }

    public void ToggleFullscreen()
    {
        forceFullscreen = !forceFullscreen;
        ApplyResolution();
    }

    // Inspector에서 테스트용
    [ContextMenu("Apply Resolution Now")]
    public void ApplyResolutionNow()
    {
        ApplyResolution();
    }

    [ContextMenu("Log Display Info")]
    public void LogDisplayInfoNow()
    {
        LogDisplayInfo();
    }

    // 애플리케이션 포커스 변경 시
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && preventResolutionChange)
        {
            ApplyResolution();
        }
    }

    // 애플리케이션 일시정지 시
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && preventResolutionChange)
        {
            ApplyResolution();
        }
    }
} 