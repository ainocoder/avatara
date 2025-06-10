using UnityEngine;
using UnityEngine.UI;

public class ConvaiBoothLocationManager : MonoBehaviour
{
    [Header("Booth UI References - Assign in Inspector")]
    public Button boothLocationButton;
    public GameObject boothLocationPanel;
    public Button closeButton;
    
    void Start()
    {
        // 시작 시 참조 설정 및 이벤트 연결
        SetupReferences();
        SetupEvents();
        
        // 패널을 확실히 비활성화
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(false);
            Debug.Log("Booth panel deactivated on start");
        }
    }
    
    void SetupReferences()
    {
        // 수동 참조가 없다면 자동으로 찾기
        if (boothLocationButton == null)
        {
            GameObject buttonObj = GameObject.Find("BoothLocationButton_Convai");
            if (buttonObj != null)
                boothLocationButton = buttonObj.GetComponent<Button>();
        }
        
        if (boothLocationPanel == null)
        {
            boothLocationPanel = GameObject.Find("BoothLocationPanel_Convai");
        }
        
        if (closeButton == null)
        {
            GameObject closeObj = GameObject.Find("CloseButton_Convai");
            if (closeObj != null)
                closeButton = closeObj.GetComponent<Button>();
        }
        
        Debug.Log($"References found - Button: {boothLocationButton != null}, Panel: {boothLocationPanel != null}, Close: {closeButton != null}");
    }
    
    void SetupEvents()
    {
        // 부스 위치 버튼 이벤트
        if (boothLocationButton != null)
        {
            boothLocationButton.onClick.RemoveAllListeners();
            boothLocationButton.onClick.AddListener(ShowBoothLocation);
            Debug.Log("Booth location button event connected!");
        }
        
        // 닫기 버튼 이벤트
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HideBoothLocation);
            Debug.Log("Close button event connected!");
        }
    }
    
    [ContextMenu("Show Booth Location")]
    public void ShowBoothLocation()
    {
        Debug.Log("=== ShowBoothLocation Called ===");
        
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(true);
            Debug.Log("Booth location panel SHOWN!");
        }
        else
        {
            Debug.LogError("Booth location panel is NULL!");
        }
    }
    
    [ContextMenu("Hide Booth Location")]
    public void HideBoothLocation()
    {
        Debug.Log("=== HideBoothLocation Called ===");
        
        if (boothLocationPanel != null)
        {
            boothLocationPanel.SetActive(false);
            Debug.Log("Booth location panel HIDDEN!");
        }
        else
        {
            Debug.LogError("Booth location panel is NULL!");
        }
    }
    
    // 디버깅용 메서드
    void Update()
    {
        // 키보드 테스트 (H키로 패널 숨기기)
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideBoothLocation();
        }
    }
}