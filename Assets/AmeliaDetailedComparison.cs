using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;
using Convai.Scripts.Runtime.Features.LipSync.Visemes;

public class AmeliaDetailedComparison : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Amelia vs Amelia1 상세 분석 ===");
        
        // 두 캐릭터 찾기
        ConvaiLipSync ameliaLipSync = null;
        ConvaiLipSync amelia1LipSync = null;
        
        ConvaiLipSync[] lipSyncs = FindObjectsOfType<ConvaiLipSync>();
        
        foreach (var lipSync in lipSyncs)
        {
            string name = lipSync.gameObject.name;
            if (name.Contains("Amelia") && !name.Contains("1"))
            {
                ameliaLipSync = lipSync;
                Debug.Log($"Amelia 발견: {name}");
            }
            else if (name.Contains("Amelia1"))
            {
                amelia1LipSync = lipSync;
                Debug.Log($"Amelia1 발견: {name}");
            }
        }
        
        if (ameliaLipSync == null || amelia1LipSync == null)
        {
            Debug.LogError("Amelia 또는 Amelia1을 찾을 수 없습니다!");
            return;
        }
        
        CompareLipSyncSettings(ameliaLipSync, amelia1LipSync);
    }
    
    void CompareLipSyncSettings(ConvaiLipSync amelia, ConvaiLipSync amelia1)
    {
        Debug.Log("\n=== 기본 설정 비교 ===");
        Debug.Log($"Amelia WeightBlendingPower: {amelia.WeightBlendingPower}");
        Debug.Log($"Amelia1 WeightBlendingPower: {amelia1.WeightBlendingPower}");
        
        Debug.Log("\n=== Head Renderer 비교 ===");
        var ameliaHead = amelia.FacialExpressionData.Head;
        var amelia1Head = amelia1.FacialExpressionData.Head;
        
        Debug.Log($"Amelia Head: {(ameliaHead.Renderer != null ? ameliaHead.Renderer.name : "NULL")}");
        Debug.Log($"Amelia1 Head: {(amelia1Head.Renderer != null ? amelia1Head.Renderer.name : "NULL")}");
        Debug.Log($"Amelia Head BlendShapes: {(ameliaHead.Renderer?.sharedMesh?.blendShapeCount ?? 0)}");
        Debug.Log($"Amelia1 Head BlendShapes: {(amelia1Head.Renderer?.sharedMesh?.blendShapeCount ?? 0)}");
        
        Debug.Log("\n=== Teeth Renderer 비교 ===");
        var ameliaTeeth = amelia.FacialExpressionData.Teeth;
        var amelia1Teeth = amelia1.FacialExpressionData.Teeth;
        
        Debug.Log($"Amelia Teeth: {(ameliaTeeth.Renderer != null ? ameliaTeeth.Renderer.name : "NULL")}");
        Debug.Log($"Amelia1 Teeth: {(amelia1Teeth.Renderer != null ? amelia1Teeth.Renderer.name : "NULL")}");
        Debug.Log($"Amelia Teeth BlendShapes: {(ameliaTeeth.Renderer?.sharedMesh?.blendShapeCount ?? 0)}");
        Debug.Log($"Amelia1 Teeth BlendShapes: {(amelia1Teeth.Renderer?.sharedMesh?.blendShapeCount ?? 0)}");
        
        Debug.Log("\n=== VisemeEffectors 비교 ===");
        bool ameliaHeadHasEffectors = ameliaHead.VisemeEffectorsList != null;
        bool amelia1HeadHasEffectors = amelia1Head.VisemeEffectorsList != null;
        bool ameliaTeethHasEffectors = ameliaTeeth.VisemeEffectorsList != null;
        bool amelia1TeethHasEffectors = amelia1Teeth.VisemeEffectorsList != null;
        
        Debug.Log($"Amelia Head Effectors: {ameliaHeadHasEffectors}");
        Debug.Log($"Amelia1 Head Effectors: {amelia1HeadHasEffectors}");
        Debug.Log($"Amelia Teeth Effectors: {ameliaTeethHasEffectors}");
        Debug.Log($"Amelia1 Teeth Effectors: {amelia1TeethHasEffectors}");
        
        if (ameliaTeethHasEffectors && amelia1TeethHasEffectors)
        {
            string ameliaEffectorName = ameliaTeeth.VisemeEffectorsList.name;
            string amelia1EffectorName = amelia1Teeth.VisemeEffectorsList.name;
            Debug.Log($"Amelia Teeth Effector 이름: {ameliaEffectorName}");
            Debug.Log($"Amelia1 Teeth Effector 이름: {amelia1EffectorName}");
            
            if (ameliaEffectorName != amelia1EffectorName)
            {
                Debug.LogWarning("⚠️ 다른 Teeth Effector를 사용합니다!");
            }
        }
        
        Debug.Log("\n=== 문제 진단 ===");
        if (!amelia1TeethHasEffectors)
        {
            Debug.LogError("❌ Amelia1에 Teeth VisemeEffectorsList가 없습니다!");
        }
        
        if (amelia1Teeth.Renderer == null)
        {
            Debug.LogError("❌ Amelia1에 Teeth Renderer가 없습니다!");
        }
    }
} 