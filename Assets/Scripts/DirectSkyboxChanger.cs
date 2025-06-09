using UnityEngine;
using UnityEditor;

public class DirectSkyboxChanger : MonoBehaviour
{
    [ContextMenu("Load and Set Wood HDRI")]
    public void SetWoodHDRIDirect()
    {
        // 직접 Asset Database를 통해 머티리얼 로드
        #if UNITY_EDITOR
        string assetPath = "Assets/HDRI/wood.mat";
        Material woodMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        
        if (woodMaterial != null)
        {
            RenderSettings.skybox = woodMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Wood HDRI skybox applied successfully!");
        }
        else
        {
            Debug.LogError("Wood HDRI material not found at: " + assetPath);
        }
        #endif
    }
    
    [ContextMenu("Load and Set Room HDRI")]
    public void SetRoomHDRIDirect()
    {
        #if UNITY_EDITOR
        string assetPath = "Assets/HDRI/room.mat";
        Material roomMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        
        if (roomMaterial != null)
        {
            RenderSettings.skybox = roomMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Room HDRI skybox applied successfully!");
        }
        else
        {
            Debug.LogError("Room HDRI material not found at: " + assetPath);
        }
        #endif
    }
    
    [ContextMenu("Load and Set Street HDRI")]
    public void SetStreetHDRIDirect()
    {
        #if UNITY_EDITOR
        string assetPath = "Assets/HDRI/street.mat";
        Material streetMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        
        if (streetMaterial != null)
        {
            RenderSettings.skybox = streetMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Street HDRI skybox applied successfully!");
        }
        else
        {
            Debug.LogError("Street HDRI material not found at: " + assetPath);
        }
        #endif
    }
    
    [ContextMenu("Load and Set Seaside HDRI")]
    public void SetSeasideHDRIDirect()
    {
        #if UNITY_EDITOR
        string assetPath = "Assets/UnityTechnologies/UnityHDRI/Seaside/SeasideWhiteBalanced.mat";
        Material seasideMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        
        if (seasideMaterial != null)
        {
            RenderSettings.skybox = seasideMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Seaside HDRI skybox applied successfully!");
        }
        else
        {
            Debug.LogError("Seaside HDRI material not found at: " + assetPath);
        }
        #endif
    }
    
    [ContextMenu("Check Current Skybox")]
    public void CheckCurrentSkybox()
    {
        if (RenderSettings.skybox != null)
        {
            Debug.Log("Current skybox: " + RenderSettings.skybox.name);
        }
        else
        {
            Debug.Log("No skybox currently set");
        }
    }
}