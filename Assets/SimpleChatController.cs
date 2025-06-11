using UnityEngine;
using Convai.Scripts.Runtime.UI;

public class SimpleChatController : MonoBehaviour
{
    [Header("간편 설정")]
    [SerializeField] private bool disableChat = true; // 채팅창 완전히 비활성화
    [SerializeField] private bool minimizeToCorner = false; // 좌측 하단 작게 축소
    
    [Header("축소 설정 (minimizeToCorner가 true일 때)")]
    [SerializeField] private float scale = 0.01f;
    [SerializeField] private Vector2 position = new Vector2(100, 100);
    
    void Start()
    {
        // 약간의 지연 후 실행 (ConvAI UI가 완전히 로드되기를 기다림)
        Invoke(nameof(ApplySettings), 1f);
    }
    
    void ApplySettings()
    {
        var chatHandler = ConvaiChatUIHandler.Instance;
        if (chatHandler == null)
        {
            Debug.LogWarning("ConvaiChatUIHandler를 찾을 수 없습니다.");
            return;
        }
        
        var currentUI = chatHandler.GetCurrentUI();
        if (currentUI == null)
        {
            Debug.LogWarning("현재 활성화된 Chat UI를 찾을 수 없습니다.");
            return;
        }
        
        var canvasGroup = currentUI.GetCanvasGroup();
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup을 찾을 수 없습니다.");
            return;
        }
        
        if (disableChat)
        {
            // 채팅창 완전히 숨기기
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Debug.Log("ConvAI 채팅창이 숨겨졌습니다.");
        }
        else if (minimizeToCorner)
        {
            // 좌측 하단으로 작게 이동
            var rectTransform = canvasGroup.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * scale;
                rectTransform.anchoredPosition = position;
                Debug.Log("ConvAI 채팅창이 좌측 하단으로 축소되었습니다.");
            }
        }
    }
} 