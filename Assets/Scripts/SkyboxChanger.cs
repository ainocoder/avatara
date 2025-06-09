using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
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