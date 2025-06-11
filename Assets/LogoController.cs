using UnityEngine;
using UnityEngine.UI;

public class LogoController : MonoBehaviour
{
    [Header("로고 크기 설정")]
    [SerializeField] private float widthScale = 1.5f; // 가로 크기 배율
    [SerializeField] private float heightScale = 1f;  // 세로 크기 배율
    
    [Header("위치 설정")]
    [SerializeField] private Vector2 logoPosition = Vector2.zero;
    
    [Header("기타 설정")]
    [SerializeField] private bool maintainAspectRatio = false; // 비율 유지 여부
    
    private RectTransform rectTransform;
    private Image logoImage;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        logoImage = GetComponent<Image>();
        
        ApplyLogoSettings();
    }
    
    void ApplyLogoSettings()
    {
        if (rectTransform != null)
        {
            // 크기 조정
            if (maintainAspectRatio)
            {
                // 비율을 유지하면서 크기 조정
                float scale = Mathf.Max(widthScale, heightScale);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                // 가로/세로 개별 조정
                rectTransform.localScale = new Vector3(widthScale, heightScale, 1f);
            }
            
            // 위치 조정
            rectTransform.anchoredPosition = logoPosition;
        }
    }
    
    // Inspector에서 실시간으로 변경사항 적용
    void OnValidate()
    {
        if (Application.isPlaying && rectTransform != null)
        {
            ApplyLogoSettings();
        }
    }
    
    // 코드에서 호출할 수 있는 메서드들
    public void SetWidthScale(float scale)
    {
        widthScale = scale;
        ApplyLogoSettings();
    }
    
    public void SetHeightScale(float scale)
    {
        heightScale = scale;
        ApplyLogoSettings();
    }
    
    public void SetPosition(Vector2 position)
    {
        logoPosition = position;
        ApplyLogoSettings();
    }
    
    public void ResetToOriginal()
    {
        widthScale = 1f;
        heightScale = 1f;
        logoPosition = Vector2.zero;
        ApplyLogoSettings();
    }
} 