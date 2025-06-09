using UnityEngine;

public class AdvancedMaterialFixer : MonoBehaviour
{
    [ContextMenu("Force Fix All Materials")]
    public void ForceFixAllMaterials()
    {
        // shapeUnity 오브젝트 찾기
        GameObject shapeUnity = GameObject.Find("shapeUnity");
        if (shapeUnity == null)
        {
            Debug.LogError("shapeUnity object not found!");
            return;
        }
        
        // 모든 SkinnedMeshRenderer 찾기
        SkinnedMeshRenderer[] renderers = shapeUnity.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Debug.Log($"Processing renderer on: {renderer.gameObject.name}");
            
            // 모든 머티리얼을 새로운 기본 머티리얼로 교체
            Material[] newMaterials = new Material[renderer.materials.Length];
            
            for (int i = 0; i < newMaterials.Length; i++)
            {
                // Standard 머티리얼 생성
                Material newMat = new Material(Shader.Find("Standard"));
                newMat.color = Color.white;
                newMat.SetFloat("_Smoothness", 0.5f);
                newMat.SetFloat("_Metallic", 0f);
                newMat.name = $"Fixed_Material_{i}";
                
                newMaterials[i] = newMat;
            }
            
            renderer.materials = newMaterials;
            Debug.Log($"Fixed {newMaterials.Length} materials on {renderer.gameObject.name}");
        }
        
        Debug.Log("All materials have been reset to standard white materials!");
    }
    
    [ContextMenu("Apply Skin Material")]
    public void ApplySkinMaterial()
    {
        GameObject shapeUnity = GameObject.Find("shapeUnity");
        if (shapeUnity == null) return;
        
        SkinnedMeshRenderer[] renderers = shapeUnity.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    // 피부색으로 설정
                    Color skinColor = new Color(1f, 0.9f, 0.8f, 1f); // 자연스러운 피부색
                    materials[i].color = skinColor;
                    
                    if (materials[i].HasProperty("_Color"))
                        materials[i].SetColor("_Color", skinColor);
                        
                    if (materials[i].HasProperty("_BaseColor"))
                        materials[i].SetColor("_BaseColor", skinColor);
                }
            }
        }
        
        Debug.Log("Applied natural skin color to all materials!");
    }
}