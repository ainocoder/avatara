using UnityEngine;
using UnityEngine.UI;

public class BoothLocationManager : MonoBehaviour
{
    [Header("UI References")]
    public Button boothLocationButton;
    public GameObject boothLocationPanel;
    public Button closeButton;
    
    void Start()
    {
        // 버튼 이벤트 설정
        if (boothLocationButton != null)
        {
            boothLocationButton.onClick.AddListener(ShowBoothLocation);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideBoothLocation);
        }
        
        // 시작 시 패널 숨김
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(false);
        }
    }
    
    public void ShowBoothLocation()
    {
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(true);
        }
    }
    
    public void HideBoothLocation()
    {
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        // 이벤트 해제
        if (boothLocationButton != null)
        {
            boothLocationButton.onClick.RemoveListener(ShowBoothLocation);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HideBoothLocation);
        }
    }
}