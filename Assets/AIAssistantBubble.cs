using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AIAssistantBubble : MonoBehaviour
{
    [Header("말풍선 설정")]
    [SerializeField] private string bubbleText = "AI도우미 나우에게 궁금한걸 질문해주세요!";
    [SerializeField] private Color bubbleColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private float bubbleWidth = 400f;
    [SerializeField] private float bubbleHeight = 80f;
    
    [Header("위치 설정")]
    [SerializeField] private Vector2 topOffset = new Vector2(0, -50); // 화면 상단에서의 오프셋
    [SerializeField] private bool autoHide = true; // 자동으로 숨김
    [SerializeField] private float autoHideDelay = 10f; // 자동 숨김 시간
    
    [Header("애니메이션 설정")]
    [SerializeField] private bool useFloatingAnimation = true;
    [SerializeField] private float floatingSpeed = 1f;
    [SerializeField] private float floatingHeight = 10f;
    
    private Canvas canvas;
    private GameObject bubbleObject;
    private RectTransform bubbleRect;
    private TextMeshProUGUI bubbleTextComponent;
    private Image bubbleBackground;
    private Vector3 originalPosition;
    private float floatingTimer;
    
    void Start()
    {
        CreateBubbleUI();
        
        if (autoHide)
        {
            Invoke(nameof(HideBubble), autoHideDelay);
        }
    }
    
    void CreateBubbleUI()
    {
        // Canvas 생성 또는 찾기
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 말풍선 오브젝트 생성
        bubbleObject = new GameObject("AI Assistant Bubble");
        bubbleObject.transform.SetParent(canvas.transform, false);
        
        // RectTransform 설정
        bubbleRect = bubbleObject.AddComponent<RectTransform>();
        bubbleRect.sizeDelta = new Vector2(bubbleWidth, bubbleHeight);
        bubbleRect.anchorMin = new Vector2(0.5f, 1f); // 화면 상단 중앙
        bubbleRect.anchorMax = new Vector2(0.5f, 1f);
        bubbleRect.anchoredPosition = topOffset;
        originalPosition = bubbleRect.anchoredPosition;
        
        // 배경 이미지 (구름 모양 효과)
        bubbleBackground = bubbleObject.AddComponent<Image>();
        bubbleBackground.color = bubbleColor;
        bubbleBackground.sprite = CreateCloudSprite();
        
        // 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("Bubble Text");
        textObj.transform.SetParent(bubbleObject.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(15, 10); // 여백
        textRect.offsetMax = new Vector2(-15, -10);
        
        // TextMeshPro 컴포넌트
        bubbleTextComponent = textObj.AddComponent<TextMeshProUGUI>();
        bubbleTextComponent.text = bubbleText;
        bubbleTextComponent.color = textColor;
        bubbleTextComponent.fontSize = 16;
        bubbleTextComponent.fontStyle = FontStyles.Bold;
        bubbleTextComponent.alignment = TextAlignmentOptions.Center;
        bubbleTextComponent.enableWordWrapping = true;
        
        // 그림자 효과 추가
        AddDropShadow();
    }
    
    Sprite CreateCloudSprite()
    {
        // 간단한 둥근 사각형 스프라이트 생성
        Texture2D texture = new Texture2D(100, 60);
        Color[] colors = new Color[texture.width * texture.height];
        
        Vector2 center = new Vector2(texture.width * 0.5f, texture.height * 0.5f);
        float radiusX = texture.width * 0.4f;
        float radiusY = texture.height * 0.4f;
        
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedX = (x - center.x) / radiusX;
                float normalizedY = (y - center.y) / radiusY;
                float ellipseDistance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                
                if (ellipseDistance <= 1f)
                {
                    float alpha = 1f - Mathf.Pow(ellipseDistance, 0.5f);
                    colors[y * texture.width + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[y * texture.width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    
    void AddDropShadow()
    {
        // 그림자 효과를 위한 두 번째 배경
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(bubbleObject.transform, false);
        shadowObj.transform.SetAsFirstSibling(); // 뒤로 보내기
        
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.sizeDelta = Vector2.zero;
        shadowRect.anchoredPosition = new Vector2(3, -3); // 그림자 오프셋
        
        Image shadowImage = shadowObj.AddComponent<Image>();
        shadowImage.sprite = bubbleBackground.sprite;
        shadowImage.color = new Color(0, 0, 0, 0.3f);
    }
    
    void Update()
    {
        if (useFloatingAnimation && bubbleRect != null)
        {
            floatingTimer += Time.deltaTime * floatingSpeed;
            float yOffset = Mathf.Sin(floatingTimer) * floatingHeight;
            bubbleRect.anchoredPosition = originalPosition + new Vector3(0, yOffset, 0);
        }
    }
    
    public void ShowBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }
    
    public void HideBubble()
    {
        if (bubbleObject != null)
        {
            StartCoroutine(FadeOut());
        }
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        CanvasGroup canvasGroup = bubbleObject.GetComponent<CanvasGroup>() ?? bubbleObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = bubbleObject.GetComponent<CanvasGroup>() ?? bubbleObject.AddComponent<CanvasGroup>();
        
        float elapsed = 0f;
        float duration = 0.5f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        if (bubbleObject != null)
            bubbleObject.SetActive(false);
    }
    
    public void UpdateText(string newText)
    {
        bubbleText = newText;
        if (bubbleTextComponent != null)
        {
            bubbleTextComponent.text = newText;
        }
    }
    
    // 클릭했을 때 숨기기
    public void OnBubbleClick()
    {
        HideBubble();
    }
    
    void OnDestroy()
    {
        if (bubbleObject != null)
        {
            Destroy(bubbleObject);
        }
    }
} 