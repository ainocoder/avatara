using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkyboxSetter : MonoBehaviour
{
    void Start()
    {
        SetV1SkyboxDirect();
    }
    
    [ContextMenu("Set V1 HDRI")]
    public void SetV1SkyboxDirect()
    {
        #if UNITY_EDITOR
        string assetPath = "Assets/HDRI/v1.mat";
        Material v1Material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        
        if (v1Material != null)
        {
            RenderSettings.skybox = v1Material;
            DynamicGI.UpdateEnvironment();
            Debug.Log("V1 HDRI skybox applied successfully!");
        }
        else
        {
            Debug.LogError("V1 HDRI material not found at: " + assetPath);
        }
        #else
        // 런타임에서는 Resources를 통해 로드 시도
        Material v1Material = Resources.Load<Material>("HDRI/v1");
        if (v1Material != null)
        {
            RenderSettings.skybox = v1Material;
            DynamicGI.UpdateEnvironment();
            Debug.Log("V1 HDRI skybox applied successfully from Resources!");
        }
        else
        {
            Debug.LogError("V1 HDRI material not found in Resources!");
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