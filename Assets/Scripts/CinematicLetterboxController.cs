using UnityEngine;
using UnityEngine.UI;

public class CinematicLetterboxController : MonoBehaviour
{
    [Header("레터박스 설정")]
    [SerializeField] private float contentRatio = 0.6f; // 3/5 = 0.6
    [SerializeField] private Color letterboxColor = Color.black;
    [SerializeField] private bool enableLetterbox = true;
    
    [Header("디버그 정보")]
    [SerializeField] private bool showDebugInfo = true;
    
    private Canvas letterboxCanvas;
    private GameObject topLetterbox;
    private GameObject bottomLetterbox;
    private Camera mainCamera;
    
    void Awake()
    {
        CreateLetterboxUI();
        FindMainCamera();
    }
    
    void Start()
    {
        ApplyLetterboxSettings();
    }
    
    void FindMainCamera()
    {
        // 메인 카메라 찾기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (showDebugInfo && mainCamera != null)
        {
            Debug.Log($"CinematicLetterbox: 메인 카메라 찾음 - {mainCamera.name}");
        }
    }
    
    void CreateLetterboxUI()
    {
        // 레터박스 전용 캔버스 생성
        GameObject canvasGO = new GameObject("Letterbox Canvas");
        canvasGO.transform.SetParent(transform);
        
        letterboxCanvas = canvasGO.AddComponent<Canvas>();
        letterboxCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        letterboxCanvas.sortingOrder = 1000; // 가장 위에 표시
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2560f, 1440f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // 상단 레터박스 생성
        topLetterbox = CreateLetterboxPanel("Top Letterbox", canvasGO.transform);
        
        // 하단 레터박스 생성
        bottomLetterbox = CreateLetterboxPanel("Bottom Letterbox", canvasGO.transform);
        
        if (showDebugInfo)
        {
            Debug.Log("CinematicLetterbox: 레터박스 UI 생성 완료");
        }
    }
    
    GameObject CreateLetterboxPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        // Image 컴포넌트 추가
        Image image = panel.AddComponent<Image>();
        image.color = letterboxColor;
        image.raycastTarget = false; // 마우스 이벤트 차단 방지
        
        // RectTransform 설정
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    void ApplyLetterboxSettings()
    {
        if (!enableLetterbox || topLetterbox == null || bottomLetterbox == null)
        {
            SetLetterboxActive(false);
            return;
        }
        
        SetLetterboxActive(true);
        UpdateLetterboxSizes();
        
        if (showDebugInfo)
        {
            float letterboxHeight = (1f - contentRatio) / 2f;
            Debug.Log($"CinematicLetterbox: 적용 완료");
            Debug.Log($"- 컨텐츠 비율: {contentRatio * 100}%");
            Debug.Log($"- 레터박스 높이: {letterboxHeight * 100}% (위/아래 각각)");
            Debug.Log($"- 실제 컨텐츠 영역: {Screen.width}x{Screen.height * contentRatio}");
        }
    }
    
    void UpdateLetterboxSizes()
    {
        // 레터박스 높이 계산 (1/5 = 0.2, 즉 (1-0.6)/2 = 0.2)
        float letterboxHeight = (1f - contentRatio) / 2f;
        
        // 상단 레터박스 설정
        RectTransform topRect = topLetterbox.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0f, 1f - letterboxHeight);
        topRect.anchorMax = new Vector2(1f, 1f);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;
        
        // 하단 레터박스 설정
        RectTransform bottomRect = bottomLetterbox.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0f, 0f);
        bottomRect.anchorMax = new Vector2(1f, letterboxHeight);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;
    }
    
    void SetLetterboxActive(bool active)
    {
        if (letterboxCanvas != null)
        {
            letterboxCanvas.gameObject.SetActive(active);
        }
    }
    
    // 카메라 뷰포트 조정 (선택적 사용)
    void AdjustCameraViewport()
    {
        if (mainCamera == null) return;
        
        if (enableLetterbox)
        {
            // 카메라가 중앙 3/5 영역만 렌더링하도록 설정
            float letterboxHeight = (1f - contentRatio) / 2f;
            Rect viewport = new Rect(0f, letterboxHeight, 1f, contentRatio);
            mainCamera.rect = viewport;
            
            if (showDebugInfo)
            {
                Debug.Log($"CinematicLetterbox: 카메라 뷰포트 조정 - {viewport}");
            }
        }
        else
        {
            // 원래 뷰포트로 복원
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
    
    // Public 메서드들
    public void SetContentRatio(float ratio)
    {
        contentRatio = Mathf.Clamp01(ratio);
        ApplyLetterboxSettings();
    }
    
    public void SetLetterboxColor(Color color)
    {
        letterboxColor = color;
        
        if (topLetterbox != null)
        {
            topLetterbox.GetComponent<Image>().color = letterboxColor;
        }
        
        if (bottomLetterbox != null)
        {
            bottomLetterbox.GetComponent<Image>().color = letterboxColor;
        }
    }
    
    public void EnableLetterbox(bool enable)
    {
        enableLetterbox = enable;
        ApplyLetterboxSettings();
    }
    
    // 컨텍스트 메뉴 (테스트용)
    [ContextMenu("적용하기")]
    public void ApplySettings()
    {
        ApplyLetterboxSettings();
    }
    
    [ContextMenu("레터박스 토글")]
    public void ToggleLetterbox()
    {
        EnableLetterbox(!enableLetterbox);
    }
    
    [ContextMenu("3/5 비율 설정")]
    public void SetThreeFifthsRatio()
    {
        SetContentRatio(0.6f);
    }
    
    [ContextMenu("4/5 비율 설정")]  
    public void SetFourFifthsRatio()
    {
        SetContentRatio(0.8f);
    }
    
    [ContextMenu("전체 화면")]
    public void SetFullScreen()
    {
        SetContentRatio(1.0f);
    }
    
    // 해상도 변경 감지
    void Update()
    {
        // 해상도가 변경되었을 때 레터박스 크기 재조정
        if (Time.frameCount % 60 == 0) // 1초마다 체크
        {
            if (enableLetterbox)
            {
                UpdateLetterboxSizes();
            }
        }
    }
    
    // Inspector에서 설정 변경시 실시간 적용
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyLetterboxSettings();
        }
    }
    
    void OnDestroy()
    {
        // 카메라 뷰포트 원복
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
} 