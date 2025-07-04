using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class SimpleTeethFix : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== SimpleTeethFix 시작 ===");
        
        // 2초 후에 자동 실행
        Invoke("AutoFixTeeth", 2f);
    }
    
    void AutoFixTeeth()
    {
        Debug.Log("이빨 문제 자동 해결 시작...");
        
        // missy1 또는 missyT 찾기
        GameObject missy = GameObject.Find("missy1");
        if (missy == null)
            missy = GameObject.Find("missyT");
        
        if (missy == null)
        {
            Debug.LogError("missy1 또는 missyT 캐릭터를 찾을 수 없습니다!");
            Debug.Log("씬에 있는 모든 GameObject 출력:");
            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (go.name.ToLower().Contains("missy"))
                    Debug.Log("발견된 Missy 관련 오브젝트: " + go.name);
            }
            return;
        }
        
        Debug.Log("캐릭터 발견: " + missy.name);
        
        // ConvaiLipSync 컴포넌트 확인
        var lipSync = missy.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 컴포넌트가 없습니다!");
            return;
        }
        
        Debug.Log("ConvaiLipSync 컴포넌트 발견!");
        
        // 현재 설정 출력
        var facialData = lipSync.FacialExpressionData;
        Debug.Log("현재 Head Renderer: " + (facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL"));
        Debug.Log("현재 Teeth Renderer: " + (facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL"));
        
        // 이빨 렌더러 찾기
        var renderers = missy.GetComponentsInChildren<SkinnedMeshRenderer>();
        Debug.Log($"총 {renderers.Length}개의 SkinnedMeshRenderer 발견:");
        
        SkinnedMeshRenderer teethRenderer = null;
        
        foreach (var renderer in renderers)
        {
            string info = $"  - {renderer.name}";
            if (renderer.sharedMesh != null)
            {
                info += $" (메시: {renderer.sharedMesh.name}, BlendShape: {renderer.sharedMesh.blendShapeCount}개)";
                
                // 이빨 관련 이름 확인
                if (renderer.name.ToLower().Contains("teeth") || 
                    renderer.name.ToLower().Contains("tooth") ||
                    renderer.sharedMesh.name.ToLower().Contains("teeth") ||
                    renderer.sharedMesh.name.ToLower().Contains("tooth"))
                {
                    info += " ← 이빨 후보!";
                    teethRenderer = renderer;
                }
            }
            Debug.Log(info);
        }
        
        if (teethRenderer != null)
        {
            Debug.Log($"이빨 렌더러 설정: {teethRenderer.name}");
            facialData.Teeth.Renderer = teethRenderer;
            facialData.Teeth.WeightBounds = new Vector2(0, 1);
            Debug.Log("이빨 렌더러 설정 완료!");
        }
        else
        {
            Debug.LogWarning("이빨 렌더러를 찾을 수 없습니다. Head Renderer를 Teeth Renderer로도 사용해보겠습니다.");
            if (facialData.Head.Renderer != null)
            {
                facialData.Teeth.Renderer = facialData.Head.Renderer;
                facialData.Teeth.WeightBounds = new Vector2(0, 1);
                Debug.Log("Head Renderer를 Teeth Renderer로 설정했습니다.");
            }
        }
        
        Debug.Log("=== 이빨 문제 해결 완료 ===");
    }
} 