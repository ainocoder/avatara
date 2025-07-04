using UnityEngine;

public class QuickAnalyzer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 === 빠른 분석 시작 ===");
        
        // CC_Base_Body 메시들 찾기
        SkinnedMeshRenderer[] renderers = FindObjectsOfType<SkinnedMeshRenderer>();
        
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.name == "CC_Base_Body")
            {
                string characterName = FindCharacterName(renderer.transform);
                
                Debug.Log($"\n--- {characterName} ---");
                Debug.Log($"  V_Open: {renderer.GetBlendShapeWeight(0):F3}");
                Debug.Log($"  V_Tight: {renderer.GetBlendShapeWeight(4):F3}");
                Debug.Log($"  Mesh Hash: {renderer.sharedMesh.GetHashCode()}");
                Debug.Log($"  Position: {renderer.transform.position}");
                Debug.Log($"  Local Scale: {renderer.transform.localScale}");
                
                // Animator 확인
                Animator animator = renderer.GetComponentInParent<Animator>();
                if (animator != null)
                {
                    Debug.Log($"  Animator Enabled: {animator.enabled}");
                    Debug.Log($"  Layers: {animator.layerCount}");
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        Debug.Log($"    Layer[{i}] Weight: {animator.GetLayerWeight(i):F3}");
                    }
                }
            }
        }
    }
    
    string FindCharacterName(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (current.name.Contains("Amelia"))
                return current.name;
            current = current.parent;
        }
        return "Unknown";
    }
} 