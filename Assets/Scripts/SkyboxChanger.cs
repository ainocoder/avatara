using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    public Material[] skyboxes; // 스카이박스 메테리얼 배열
    private int currentIndex = 0; // 현재 스카이박스 인덱스

    void Start()
    {
        // 처음 시작 시 첫 번째 스카이박스로 설정
        if (skyboxes.Length > 0)
        {
            RenderSettings.skybox = skyboxes[currentIndex];
            DynamicGI.UpdateEnvironment(); // 환경 조명 업데이트
        }
        Camera.main.clearFlags = CameraClearFlags.Skybox;
    }

    void Update()
    {
        // ... existing code ...
    }

    [ContextMenu("Set Default Sky")]
    public void SetDefaultSky()
    {
        Material skyMat = Resources.Load<Material>("Assets/defaultSky");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Default Sky");
        }
    }
    
    [ContextMenu("Set Room HDRI")]
    public void SetRoomHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/HDRI/room");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Room HDRI");
        }
    }
    
    [ContextMenu("Set Street HDRI")]
    public void SetStreetHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/HDRI/street");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Street HDRI");
        }
    }
    
    [ContextMenu("Set Wood HDRI")]
    public void SetWoodHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/HDRI/wood");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Wood HDRI");
        }
    }
    
    [ContextMenu("Set Beach/Seaside")]
    public void SetSeasideHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/UnityTechnologies/UnityHDRI/Seaside/SeasideWhiteBalanced");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Seaside");
        }
    }
    
    [ContextMenu("Set Forest/MuirWood")]
    public void SetMuirWoodHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/UnityTechnologies/UnityHDRI/MuirWood/MuirWoodWhiteBalanced");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: MuirWood Forest");
        }
    }
    
    [ContextMenu("Set Treasure Island")]
    public void SetTreasureIslandHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/UnityTechnologies/UnityHDRI/TreasureIsland/TreasureIslandWhiteBalanced");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Treasure Island");
        }
    }
    
    [ContextMenu("Set Church Interior")]
    public void SetChurchHDRI()
    {
        Material skyMat = Resources.Load<Material>("Assets/UnityTechnologies/UnityHDRI/Trinitatis Church/TrinitatisChurchWhiteBalanced");
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox changed to: Church Interior");
        }
    }
}