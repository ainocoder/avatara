using UnityEngine;

public class AmbientLightHelper : MonoBehaviour
{
    [ContextMenu("Brighten Ambient Light")]
    public void BrightenAmbientLight()
    {
        // Ambient light 색상을 밝게 조정
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        RenderSettings.ambientIntensity = 1.5f;
        
        Debug.Log("Ambient light brightened to reduce shadows on character face");
    }
}