using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleAIBubble : MonoBehaviour
{
    [Header("말풍선 설정")]
    [SerializeField] private string bubbleText = "AI도우미 나우에게 궁금한걸 질문해주세요!";
    [SerializeField] private Color bubbleColor = new Color(0.2f, 0.7f, 1f, 0.9f); // 하늘색
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float bubbleWidth = 450f;
    [SerializeField] private float bubbleHeight = 90f;
    [SerializeField] private int fontSize = 18;
    
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
    private TextMeshProUGUI textComponent;
    private Image backgroundImage;
    private Vector3 originalPosition;
    private float animationTimer;
    private Button bubbleButton;
    
    void Start()
    {
        CreateBubbleUI();
        
        if (autoHide)
        {
            StartCoroutine(AutoHideCoroutine());
        }
    }
    
    void CreateBubbleUI()
    {
        // Canvas 찾기 또는 생성
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Main Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 최상단에 표시
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
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
        backgroundImage.sprite = CreateRoundedRectSprite();
        
        // 텍스트 컴포넌트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubbleObject.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(20, 15);
        textRect.offsetMax = new Vector2(-20, -15);
        
        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = bubbleText;
        textComponent.color = textColor;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
        
        // 말풍선 꼬리 추가
        CreateBubbleTail();
        
        // 나타나는 애니메이션
        StartCoroutine(ShowAnimation());
    }
    
    Sprite CreateRoundedRectSprite()
    {
        int width = 100;
        int height = 60;
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        
        float cornerRadius = 15f;
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = 0f;
                
                // 모서리 거리 계산
                float left = cornerRadius;
                float right = width - cornerRadius;
                float bottom = cornerRadius;
                float top = height - cornerRadius;
                
                if (x >= left && x <= right && y >= bottom && y <= top)
                {
                    // 내부 영역
                    distance = 0f;
                }
                else if (x < left && y < bottom)
                {
                    // 좌하단 모서리
                    distance = Vector2.Distance(pos, new Vector2(left, bottom)) - cornerRadius;
                }
                else if (x > right && y < bottom)
                {
                    // 우하단 모서리
                    distance = Vector2.Distance(pos, new Vector2(right, bottom)) - cornerRadius;
                }
                else if (x < left && y > top)
                {
                    // 좌상단 모서리
                    distance = Vector2.Distance(pos, new Vector2(left, top)) - cornerRadius;
                }
                else if (x > right && y > top)
                {
                    // 우상단 모서리
                    distance = Vector2.Distance(pos, new Vector2(right, top)) - cornerRadius;
                }
                else
                {
                    // 가장자리 영역
                    distance = 0f;
                }
                
                if (distance <= 0f)
                {
                    colors[y * width + x] = Color.white;
                }
                else
                {
                    float alpha = Mathf.Clamp01(1f - distance / 2f);
                    colors[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
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
        tailImage.sprite = CreateTriangleSprite();
    }
    
    Sprite CreateTriangleSprite()
    {
        Texture2D texture = new Texture2D(20, 15);
        Color[] colors = new Color[texture.width * texture.height];
        
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float normalizedX = (float)x / texture.width;
                float normalizedY = (float)y / texture.height;
                
                // 삼각형 모양 만들기
                if (normalizedY > (1f - normalizedX) * 0.5f + normalizedX * 0.5f)
                {
                    colors[y * texture.width + x] = Color.white;
                }
                else
                {
                    colors[y * texture.width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 1f));
    }
    
    void Update()
    {
        if (useBouncyAnimation && bubbleRect != null)
        {
            animationTimer += Time.deltaTime * bounceSpeed;
            float yOffset = Mathf.Sin(animationTimer) * bounceHeight;
            bubbleRect.anchoredPosition = originalPosition + new Vector3(0, yOffset, 0);
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
            float scale = Mathf.Lerp(0f, 1f, EaseOutBounce(progress));
            bubbleRect.localScale = Vector3.one * scale;
            yield return null;
        }
        
        bubbleRect.localScale = Vector3.one;
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
    }
    
    IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HideBubble();
    }
    
    float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        }
        else
        {
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
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