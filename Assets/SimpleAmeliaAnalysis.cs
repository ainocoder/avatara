using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class SimpleAmeliaAnalysis : MonoBehaviour
{
    void Start()
    {
        AnalyzeAmeliaCharacters();
    }
    
    void AnalyzeAmeliaCharacters()
    {
        Debug.Log("=== Amelia vs Amelia1 비교 분석 ===");
        
        ConvaiLipSync[] lipSyncs = FindObjectsByType<ConvaiLipSync>(FindObjectsSortMode.None);
        
        ConvaiLipSync amelia = null;
        ConvaiLipSync amelia1 = null;
        
        foreach (var lipSync in lipSyncs)
        {
            if (lipSync.name.Contains("Amelia"))
            {
                if (lipSync.name.Contains("1"))
                {
                    amelia1 = lipSync;
                    Debug.Log($"Amelia1 발견: {lipSync.name}");
                }
                else
                {
                    amelia = lipSync;
                    Debug.Log($"Amelia 발견: {lipSync.name}");
                }
            }
        }
        
        if (amelia == null || amelia1 == null)
        {
            Debug.LogError("Amelia 또는 Amelia1을 찾을 수 없습니다!");
            return;
        }
        
        CompareSettings(amelia, amelia1);
    }
    
    void CompareSettings(ConvaiLipSync amelia, ConvaiLipSync amelia1)
    {
        Debug.Log("\n=== Teeth Renderer 비교 ===");
        
        var ameliaTeeth = amelia.FacialExpressionData.Teeth;
        var amelia1Teeth = amelia1.FacialExpressionData.Teeth;
        
        string ameliaTeethName = ameliaTeeth.Renderer != null ? ameliaTeeth.Renderer.name : "NULL";
        string amelia1TeethName = amelia1Teeth.Renderer != null ? amelia1Teeth.Renderer.name : "NULL";
        
        int ameliaTeethBlends = ameliaTeeth.Renderer?.sharedMesh?.blendShapeCount ?? 0;
        int amelia1TeethBlends = amelia1Teeth.Renderer?.sharedMesh?.blendShapeCount ?? 0;
        
        Debug.Log($"Amelia Teeth Renderer: {ameliaTeethName} ({ameliaTeethBlends} BlendShapes)");
        Debug.Log($"Amelia1 Teeth Renderer: {amelia1TeethName} ({amelia1TeethBlends} BlendShapes)");
        
        bool ameliaHasTeethEffectors = ameliaTeeth.VisemeEffectorsList != null;
        bool amelia1HasTeethEffectors = amelia1Teeth.VisemeEffectorsList != null;
        
        Debug.Log($"Amelia Teeth Effectors: {ameliaHasTeethEffectors}");
        Debug.Log($"Amelia1 Teeth Effectors: {amelia1HasTeethEffectors}");
        
        if (ameliaHasTeethEffectors && amelia1HasTeethEffectors)
        {
            string ameliaEffectorName = ameliaTeeth.VisemeEffectorsList.name;
            string amelia1EffectorName = amelia1Teeth.VisemeEffectorsList.name;
            Debug.Log($"Amelia Effector: {ameliaEffectorName}");
            Debug.Log($"Amelia1 Effector: {amelia1EffectorName}");
            
            if (ameliaEffectorName != amelia1EffectorName)
            {
                Debug.LogWarning("⚠️ 다른 Teeth Effector 사용!");
            }
        }
        
        Debug.Log("\n=== 문제 진단 ===");
        if (amelia1TeethBlends == 0)
        {
            Debug.LogError("❌ Amelia1 Teeth에 BlendShape가 없습니다!");
        }
        
        if (!amelia1HasTeethEffectors)
        {
            Debug.LogError("❌ Amelia1에 Teeth Effectors가 없습니다!");
        }
    }
} 