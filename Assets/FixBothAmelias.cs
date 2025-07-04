using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class FixBothAmelias : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 두 Amelia 캐릭터 모두 수정 ===");
        
        ConvaiLipSync[] lipSyncs = FindObjectsByType<ConvaiLipSync>(FindObjectsSortMode.None);
        
        foreach (var lipSync in lipSyncs)
        {
            if (lipSync.name.Contains("Amelia"))
            {
                FixAmeliaCharacter(lipSync);
            }
        }
    }
    
    void FixAmeliaCharacter(ConvaiLipSync amelia)
    {
        string charName = amelia.name;
        Debug.Log($"\n--- {charName} 수정 중 ---");
        
        // CC_Base_Body renderer 찾기
        SkinnedMeshRenderer bodyRenderer = null;
        SkinnedMeshRenderer[] renderers = amelia.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (var renderer in renderers)
        {
            if (renderer.name.Contains("CC_Base_Body"))
            {
                bodyRenderer = renderer;
                break;
            }
        }
        
        if (bodyRenderer == null)
        {
            Debug.LogError($"❌ {charName}: CC_Base_Body를 찾을 수 없습니다!");
            return;
        }
        
        // Teeth Renderer 수정
        var teethData = amelia.FacialExpressionData.Teeth;
        teethData.Renderer = bodyRenderer;
        teethData.WeightBounds = new Vector2(0, 100);
        
        Debug.Log($"✅ {charName}: Teeth Renderer를 CC_Base_Body로 설정 ({bodyRenderer.sharedMesh.blendShapeCount} BlendShapes)");
        
        // Head의 VisemeEffectorsList를 Teeth에도 복사
        var headEffectors = amelia.FacialExpressionData.Head.VisemeEffectorsList;
        if (headEffectors != null)
        {
            teethData.VisemeEffectorsList = headEffectors;
            Debug.Log($"✅ {charName}: Head의 VisemeEffectors를 Teeth에 복사");
        }
        else
        {
            Debug.LogWarning($"⚠️ {charName}: Head에도 VisemeEffectors가 없습니다!");
        }
        
        Debug.Log($"✅ {charName} 수정 완료!");
    }
} 