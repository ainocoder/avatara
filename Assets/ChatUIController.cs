using UnityEngine;
using Convai.Scripts.Runtime.UI;

public class ChatUIController : MonoBehaviour
{
    [Header("채팅창 설정")]
    [SerializeField] private bool hideChat = false;
    [SerializeField] private bool minimizeChat = true;
    [SerializeField] private float chatScale = 0.01f;
    [SerializeField] private Vector2 chatPosition = new Vector2(150, 100);
    [SerializeField] private Vector2 chatSize = new Vector2(300, 200);
    
    [Header("키보드 단축키")]
    [SerializeField] private KeyCode toggleChatKey = KeyCode.F1;
    [SerializeField] private KeyCode minimizeKey = KeyCode.F2;
    
    private ConvaiChatUIHandler chatUIHandler;
    private GameObject currentChatCanvas;
    private RectTransform chatRectTransform;
    private CanvasGroup chatCanvasGroup;
    private bool isMinimized = false;
    private bool isHidden = false;
    
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Vector2 originalSize;
    
    void Start()
    {
        // ConvaiChatUIHandler 찾기
        chatUIHandler = ConvaiChatUIHandler.Instance;
        if (chatUIHandler == null)
        {
            Debug.LogError("ConvaiChatUIHandler를 찾을 수 없습니다!");
            return;
        }
        
        // 잠시 기다린 후 UI 초기화 (ChatUI가 완전히 로드될 때까지)
        Invoke(nameof(InitializeChatUI), 0.5f);
    }
    
    void InitializeChatUI()
    {
        var currentUI = chatUIHandler.GetCurrentUI();
        if (currentUI != null)
        {
            chatCanvasGroup = currentUI.GetCanvasGroup();
            if (chatCanvasGroup != null)
            {
                currentChatCanvas = chatCanvasGroup.gameObject;
                chatRectTransform = currentChatCanvas.GetComponent<RectTransform>();
                
                // 원본 값 저장
                originalScale = chatRectTransform.localScale;
                originalPosition = chatRectTransform.anchoredPosition;
                originalSize = chatRectTransform.sizeDelta;
                
                // 초기 설정 적용
                ApplyInitialSettings();
            }
        }
    }
    
    void ApplyInitialSettings()
    {
        if (hideChat)
        {
            HideChat();
        }
        else if (minimizeChat)
        {
            MinimizeChat();
        }
    }
    
    void Update()
    {
        if (chatRectTransform == null) return;
        
        // 키보드 입력 처리
        if (Input.GetKeyDown(toggleChatKey))
        {
            ToggleChatVisibility();
        }
        
        if (Input.GetKeyDown(minimizeKey))
        {
            ToggleChatSize();
        }
    }
    
    public void ToggleChatVisibility()
    {
        if (isHidden)
        {
            ShowChat();
        }
        else
        {
            HideChat();
        }
    }
    
    public void ToggleChatSize()
    {
        if (isMinimized)
        {
            RestoreChat();
        }
        else
        {
            MinimizeChat();
        }
    }
    
    public void HideChat()
    {
        if (chatCanvasGroup != null)
        {
            chatCanvasGroup.alpha = 0f;
            chatCanvasGroup.interactable = false;
            chatCanvasGroup.blocksRaycasts = false;
            isHidden = true;
        }
    }
    
    public void ShowChat()
    {
        if (chatCanvasGroup != null)
        {
            chatCanvasGroup.alpha = 1f;
            chatCanvasGroup.interactable = true;
            chatCanvasGroup.blocksRaycasts = true;
            isHidden = false;
        }
    }
    
    public void MinimizeChat()
    {
        if (chatRectTransform != null)
        {
            // 좌측 하단으로 이동하고 크기 축소
            chatRectTransform.localScale = Vector3.one * chatScale;
            chatRectTransform.anchoredPosition = chatPosition;
            
            // 메인 패널 크기도 조정
            Transform backgroundPanel = chatRectTransform.Find("Background");
            if (backgroundPanel != null)
            {
                RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
                if (bgRect != null)
                {
                    bgRect.sizeDelta = chatSize;
                }
            }
            
            isMinimized = true;
        }
    }
    
    public void RestoreChat()
    {
        if (chatRectTransform != null)
        {
            // 원본 크기와 위치로 복원
            chatRectTransform.localScale = originalScale;
            chatRectTransform.anchoredPosition = originalPosition;
            
            // 메인 패널 크기도 원본으로 복원
            Transform backgroundPanel = chatRectTransform.Find("Background");
            if (backgroundPanel != null)
            {
                RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
                if (bgRect != null)
                {
                    bgRect.sizeDelta = originalSize;
                }
            }
            
            isMinimized = false;
        }
    }
    
    // Inspector에서 값이 변경될 때 실시간으로 적용
    void OnValidate()
    {
        if (Application.isPlaying && chatRectTransform != null)
        {
            if (hideChat)
            {
                HideChat();
            }
            else
            {
                ShowChat();
                if (minimizeChat)
                {
                    MinimizeChat();
                }
                else
                {
                    RestoreChat();
                }
            }
        }
    }
} 