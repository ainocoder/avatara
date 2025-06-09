using UnityEngine;

public class MaterialFixer : MonoBehaviour
{
    [ContextMenu("Fix Body Materials")]
    public void FixBodyMaterials()
    {
        // Body 오브젝트의 SkinnedMeshRenderer 찾기
        SkinnedMeshRenderer bodyRenderer = GameObject.Find("Body")?.GetComponent<SkinnedMeshRenderer>();
        
        if (bodyRenderer != null)
        {
            // 올바른 머티리얼 로드 시도
            Material faceMaterial = Resources.Load<Material>("Assets/Characters/cc4ex/Materials/shapeUnity/face_001");
            Material headMaterial = Resources.Load<Material>("Assets/Characters/cc4ex/Materials/shapeUnity/head_001");
            Material lipsMaterial = Resources.Load<Material>("Assets/Characters/cc4ex/Materials/shapeUnity/black_Lips_001");
            
            // 머티리얼 배열 생성
            Material[] materials = new Material[3];
            
            if (faceMaterial != null)
                materials[0] = faceMaterial;
            else
                Debug.LogWarning("Face material not found, using default");
                
            if (headMaterial != null)  
                materials[1] = headMaterial;
            else
                Debug.LogWarning("Head material not found, using default");
                
            if (lipsMaterial != null)
                materials[2] = lipsMaterial;
            else
                Debug.LogWarning("Lips material not found, using default");
            
            // 머티리얼 적용
            bodyRenderer.materials = materials;
            
            Debug.Log("Body materials fixed!");
        }
        else
        {
            Debug.LogError("Body SkinnedMeshRenderer not found!");
        }
    }
    
    [ContextMenu("Reset Material Colors")]
    public void ResetMaterialColors()
    {
        SkinnedMeshRenderer bodyRenderer = GameObject.Find("Body")?.GetComponent<SkinnedMeshRenderer>();
        
        if (bodyRenderer != null)
        {
            foreach (Material mat in bodyRenderer.materials)
            {
                if (mat != null)
                {
                    // 기본 색상을 흰색으로 설정
                    mat.color = Color.white;
                    
                    // 만약 _Color 프로퍼티가 있다면
                    if (mat.HasProperty("_Color"))
                        mat.SetColor("_Color", Color.white);
                        
                    // 만약 _BaseColor 프로퍼티가 있다면 (URP)
                    if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", Color.white);
                }
            }
            
            Debug.Log("Material colors reset to white!");
        }
    }
}