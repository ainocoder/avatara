using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BlendShapeAnalyzer : MonoBehaviour
{
    [MenuItem("Tools/Analyze missy1 BlendShapes")]
    public static void AnalyzeMissy1BlendShapes()
    {
        GameObject missy1 = GameObject.Find("missy1");
        if (missy1 == null)
        {
            Debug.LogError("missy1 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Transform ccBaseBody = missy1.transform.Find("CC_Base_Body");
        if (ccBaseBody == null)
        {
            Debug.LogError("CC_Base_Body를 찾을 수 없습니다.");
            return;
        }

        SkinnedMeshRenderer smr = ccBaseBody.GetComponent<SkinnedMeshRenderer>();
        if (smr == null || smr.sharedMesh == null)
        {
            Debug.LogError("SkinnedMeshRenderer 또는 메시를 찾을 수 없습니다.");
            return;
        }

        Mesh mesh = smr.sharedMesh;
        Debug.Log($"=== {mesh.name} 블렌드셰이프 분석 ===");
        Debug.Log($"총 블렌드셰이프 개수: {mesh.blendShapeCount}");
        Debug.Log("");

        // 입/이빨과 관련된 블렌드셰이프 찾기
        List<string> mouthRelated = new List<string>();
        List<string> jawRelated = new List<string>();
        List<string> teethRelated = new List<string>();
        List<string> allBlendShapes = new List<string>();

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string name = mesh.GetBlendShapeName(i);
            allBlendShapes.Add($"{i}: {name}");

            string lowerName = name.ToLower();
            if (lowerName.Contains("mouth") || lowerName.Contains("lip"))
                mouthRelated.Add($"{i}: {name}");
            if (lowerName.Contains("jaw"))
                jawRelated.Add($"{i}: {name}");
            if (lowerName.Contains("teeth") || lowerName.Contains("tooth"))
                teethRelated.Add($"{i}: {name}");
        }

        Debug.Log("=== 입/입술 관련 블렌드셰이프 ===");
        foreach (string bs in mouthRelated)
            Debug.Log(bs);

        Debug.Log("");
        Debug.Log("=== 턱 관련 블렌드셰이프 ===");
        foreach (string bs in jawRelated)
            Debug.Log(bs);

        Debug.Log("");
        Debug.Log("=== 이빨 관련 블렌드셰이프 ===");
        foreach (string bs in teethRelated)
            Debug.Log(bs);

        Debug.Log("");
        Debug.Log("=== 전체 블렌드셰이프 목록 ===");
        foreach (string bs in allBlendShapes)
            Debug.Log(bs);
    }
}