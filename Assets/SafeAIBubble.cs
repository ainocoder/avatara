using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SafeAIBubble : MonoBehaviour
{
    [Header("말풍선 설정")]
    [SerializeField] private string bubbleText = "AI도우미 나우에게 궁금한걸 질문해주세요!";
    [SerializeField] private Color bubbleColor = new Color(0.2f, 0.7f, 1f, 0.9f); // 하늘색
    [SerializeField] private Color textColor = Color.black;
    [SerializeField] private float bubbleWidth = 450f;
    [SerializeField] private float bubbleHeight = 90f;
    [SerializeField] private int fontSize = 18;
    
    [Header("텍스트 디버깅")]
    [SerializeField] private bool forceTextVisible = true; // 텍스트 강제 표시
    
    [Header("위치 설정")]
    [SerializeField] private Vector2 topOffset = new Vector2(0, -60);
    [SerializeField] private bool autoHide = true;
    [SerializeField] private float autoHideDelay = 8f;
    
    [Header("애니메이션")]
    [SerializeField] private bool useBouncyAnimation = true;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float bounceHeight = 5f;
    
    private Canvas canvas;
    private GameObject bubbleObject;
    private RectTransform bubbleRect;
    private Text textComponent;
    private Image backgroundImage;
    private Vector3 originalPosition;
    private float animationTimer;
    private Button bubbleButton;
    
    void Start()
    {
        Debug.Log("SafeAIBubble: 시작!");
        CreateBubbleUI();
        
        if (autoHide)
        {
            StartCoroutine(AutoHideCoroutine());
        }
    }
    
    void CreateBubbleUI()
    {
        Debug.Log("SafeAIBubble: UI 생성 중...");
        
        // Canvas 찾기 또는 생성
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("SafeAIBubble: 새 Canvas 생성");
            GameObject canvasObj = new GameObject("Main Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 최상단에 표시
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            Debug.Log("SafeAIBubble: 기존 Canvas 사용: " + canvas.name);
        }
        
        // 메인 말풍선 오브젝트
        bubbleObject = new GameObject("AI Assistant Bubble");
        bubbleObject.transform.SetParent(canvas.transform, false);
        
        // RectTransform 설정 (화면 상단 중앙에 고정)
        bubbleRect = bubbleObject.AddComponent<RectTransform>();
        bubbleRect.sizeDelta = new Vector2(bubbleWidth, bubbleHeight);
        bubbleRect.anchorMin = new Vector2(0.5f, 1f);
        bubbleRect.anchorMax = new Vector2(0.5f, 1f);
        bubbleRect.pivot = new Vector2(0.5f, 1f);
        bubbleRect.anchoredPosition = topOffset;
        originalPosition = bubbleRect.anchoredPosition;
        
        // 버튼 컴포넌트 (클릭 가능하게)
        bubbleButton = bubbleObject.AddComponent<Button>();
        bubbleButton.onClick.AddListener(OnBubbleClick);
        
        // 배경 이미지
        backgroundImage = bubbleObject.AddComponent<Image>();
        backgroundImage.color = bubbleColor;
        backgroundImage.sprite = CreateSimpleSprite();
        
        // 텍스트 컴포넌트 (Unity 기본 Text 사용)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubbleObject.transform, false);
        textObj.transform.SetAsLastSibling(); // 텍스트를 맨 앞으로
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(20, 15);
        textRect.offsetMax = new Vector2(-20, -15);
        
        textComponent = textObj.AddComponent<Text>();
        textComponent.text = bubbleText;
        textComponent.color = textColor;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        // 기본 폰트 설정
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (textComponent.font == null)
        {
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        // 텍스트 렌더링 확인
        Debug.Log("SafeAIBubble: 텍스트 설정 - 색상: " + textComponent.color + ", 폰트: " + textComponent.font.name + ", 텍스트: " + textComponent.text);
        
        // 텍스트가 확실히 보이도록 추가 설정
        textComponent.raycastTarget = false; // UI 클릭 차단 방지
        textComponent.supportRichText = true;
        
        // 말풍선 꼬리 추가
        CreateBubbleTail();
        
        // 텍스트 렌더링 강제 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 한 프레임 후 텍스트 확인
        StartCoroutine(CheckTextAfterFrame());
        
        // 나타나는 애니메이션
        StartCoroutine(ShowAnimation());
        
        Debug.Log("SafeAIBubble: UI 생성 완료!");
    }
    
    Sprite CreateSimpleSprite()
    {
        // 간단한 둥근 사각형 스프라이트 생성
        Texture2D texture = new Texture2D(100, 60);
        Color[] colors = new Color[texture.width * texture.height];
        
        float cornerRadius = 15f;
        
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Vector2 center = new Vector2(texture.width * 0.5f, texture.height * 0.5f);
                
                // 모서리 거리 계산 (간단한 버전)
                float distance = Vector2.Distance(pos, center);
                float maxDistance = Mathf.Min(texture.width, texture.height) * 0.4f;
                
                if (distance <= maxDistance)
                {
                    colors[y * texture.width + x] = Color.white;
                }
                else
                {
                    float alpha = Mathf.Clamp01(1f - (distance - maxDistance) / 5f);
                    colors[y * texture.width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    
    void CreateBubbleTail()
    {
        // 말풍선 꼬리 (작은 삼각형)
        GameObject tailObj = new GameObject("Bubble Tail");
        tailObj.transform.SetParent(bubbleObject.transform, false);
        
        RectTransform tailRect = tailObj.AddComponent<RectTransform>();
        tailRect.sizeDelta = new Vector2(20, 15);
        tailRect.anchorMin = new Vector2(0.5f, 0f);
        tailRect.anchorMax = new Vector2(0.5f, 0f);
        tailRect.anchoredPosition = new Vector2(0, -7);
        
        Image tailImage = tailObj.AddComponent<Image>();
        tailImage.color = bubbleColor;
        
        // 간단한 사각형 꼬리
        Texture2D tailTexture = new Texture2D(10, 10);
        Color[] tailColors = new Color[tailTexture.width * tailTexture.height];
        for (int i = 0; i < tailColors.Length; i++)
        {
            tailColors[i] = Color.white;
        }
        tailTexture.SetPixels(tailColors);
        tailTexture.Apply();
        
        tailImage.sprite = Sprite.Create(tailTexture, new Rect(0, 0, tailTexture.width, tailTexture.height), new Vector2(0.5f, 1f));
    }
    
    void Update()
    {
        if (useBouncyAnimation && bubbleRect != null)
        {
            animationTimer += Time.deltaTime * bounceSpeed;
            float yOffset = Mathf.Sin(animationTimer) * bounceHeight;
            bubbleRect.anchoredPosition = originalPosition + new Vector3(0, yOffset, 0);
        }
        
        // 텍스트 강제 표시 (디버깅용)
        if (forceTextVisible && textComponent != null && textComponent.color.a < 0.9f)
        {
            textComponent.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        }
    }
    
    IEnumerator ShowAnimation()
    {
        // 처음에는 보이지 않게
        bubbleRect.localScale = Vector3.zero;
        
        // 크기 애니메이션
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(0f, 1f, progress);
            bubbleRect.localScale = Vector3.one * scale;
            yield return null;
        }
        
        bubbleRect.localScale = Vector3.one;
        Debug.Log("SafeAIBubble: 나타나기 애니메이션 완료!");
    }
    
    IEnumerator HideAnimation()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startScale = bubbleRect.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1f, 0f, progress);
            bubbleRect.localScale = startScale * scale;
            yield return null;
        }
        
        if (bubbleObject != null)
            bubbleObject.SetActive(false);
            
        Debug.Log("SafeAIBubble: 숨기기 애니메이션 완료!");
    }
    
    IEnumerator CheckTextAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        
        if (textComponent != null)
        {
            Debug.Log("SafeAIBubble: 텍스트 확인 - 활성화: " + textComponent.gameObject.activeSelf + 
                     ", 텍스트: '" + textComponent.text + "', 색상: " + textComponent.color + 
                     ", 폰트: " + (textComponent.font != null ? textComponent.font.name : "null") +
                     ", Canvas: " + (textComponent.canvas != null ? textComponent.canvas.name : "null"));
                     
            // 텍스트가 보이지 않는 경우 강제로 다시 설정
            if (textComponent.color.a < 0.1f)
            {
                Debug.Log("SafeAIBubble: 텍스트 투명도 문제 발견, 수정 중...");
                textComponent.color = new Color(0f, 0f, 0f, 1f); // 검은색, 완전 불투명
            }
        }
        else
        {
            Debug.LogError("SafeAIBubble: 텍스트 컴포넌트가 null입니다!");
        }
    }
    
    IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(autoHideDelay);
        Debug.Log("SafeAIBubble: 자동 숨김 실행");
        HideBubble();
    }
    
    public void ShowBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
            StartCoroutine(ShowAnimation());
        }
    }
    
    public void HideBubble()
    {
        if (bubbleObject != null)
        {
            StartCoroutine(HideAnimation());
        }
    }
    
    public void OnBubbleClick()
    {
        Debug.Log("SafeAIBubble: 말풍선 클릭됨!");
        HideBubble();
    }
    
    public void UpdateBubbleText(string newText)
    {
        bubbleText = newText;
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }
} 