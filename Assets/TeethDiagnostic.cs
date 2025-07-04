using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class TeethDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 이빨 진단 시작 ===");
        Invoke("DiagnoseTeeth", 2f);
    }
    
    void DiagnoseTeeth()
    {
        Debug.Log("이빨 문제 상세 진단 중...");
        
        // missy1 찾기
        GameObject missy = GameObject.Find("missy1");
        if (missy == null)
            missy = GameObject.Find("missyT");
            
        if (missy == null)
        {
            Debug.LogError("missy 캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"캐릭터: {missy.name}");
        
        var lipSync = missy.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 없음!");
            return;
        }
        
        var facialData = lipSync.FacialExpressionData;
        
        Debug.Log("=== 현재 설정 확인 ===");
        Debug.Log($"Head Renderer: {(facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL")}");
        Debug.Log($"Teeth Renderer: {(facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL")}");
        Debug.Log($"Teeth Weight Bounds: {facialData.Teeth.WeightBounds}");
        Debug.Log($"Teeth VisemeEffectors: {(facialData.Teeth.VisemeEffectorsList != null ? "있음" : "NULL")}");
        
        if (facialData.Teeth.Renderer == null)
        {
            Debug.LogError("Teeth Renderer가 여전히 NULL입니다!");
            return;
        }
        
        var teethRenderer = facialData.Teeth.Renderer;
        var mesh = teethRenderer.sharedMesh;
        
        Debug.Log("=== 이빨 렌더러 상세 정보 ===");
        Debug.Log($"렌더러 이름: {teethRenderer.name}");
        Debug.Log($"메시 이름: {mesh.name}");
        Debug.Log($"BlendShape 개수: {mesh.blendShapeCount}");
        Debug.Log($"렌더러 활성화: {teethRenderer.enabled}");
        Debug.Log($"GameObject 활성화: {teethRenderer.gameObject.activeInHierarchy}");
        
        if (mesh.blendShapeCount == 0)
        {
            Debug.LogError("이빨 렌더러에 BlendShape가 없습니다! 이것이 문제의 원인입니다.");
            FindAlternativeRenderer(missy);
            return;
        }
        
        Debug.Log("=== BlendShape 목록 ===");
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i);
            float currentWeight = teethRenderer.GetBlendShapeWeight(i);
            Debug.Log($"  {i}: {shapeName} (현재 가중치: {currentWeight})");
        }
        
        Debug.Log("=== 이빨 BlendShape 테스트 시작 ===");
        StartCoroutine(TestTeethBlendShapes(teethRenderer));
    }
    
    void FindAlternativeRenderer(GameObject missy)
    {
        Debug.Log("=== 대안 렌더러 검색 중 ===");
        
        var renderers = missy.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer bestCandidate = null;
        int maxBlendShapes = 0;
        
        foreach (var renderer in renderers)
        {
            if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
            {
                Debug.Log($"렌더러: {renderer.name}, 메시: {renderer.sharedMesh.name}, BlendShape: {renderer.sharedMesh.blendShapeCount}개");
                
                // 입/얼굴 관련 BlendShape 찾기
                bool hasJawShapes = false;
                for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                {
                    string shapeName = renderer.sharedMesh.GetBlendShapeName(i).ToLower();
                    if (shapeName.Contains("jaw") || shapeName.Contains("mouth") || 
                        shapeName.Contains("open") || shapeName.Contains("teeth"))
                    {
                        hasJawShapes = true;
                        Debug.Log($"    입/턱 관련 BlendShape 발견: {renderer.sharedMesh.GetBlendShapeName(i)}");
                    }
                }
                
                if (hasJawShapes && renderer.sharedMesh.blendShapeCount > maxBlendShapes)
                {
                    maxBlendShapes = renderer.sharedMesh.blendShapeCount;
                    bestCandidate = renderer;
                }
            }
        }
        
        if (bestCandidate != null)
        {
            Debug.Log($"더 나은 이빨 렌더러 후보 발견: {bestCandidate.name}");
            Debug.Log("이 렌더러로 다시 설정해보겠습니다...");
            
            var lipSync = missy.GetComponent<ConvaiLipSync>();
            var facialData = lipSync.FacialExpressionData;
            facialData.Teeth.Renderer = bestCandidate;
            facialData.Teeth.WeightBounds = new Vector2(0, 100); // 0-100으로 시도
            
            Debug.Log("새로운 이빨 렌더러 설정 완료!");
            StartCoroutine(TestTeethBlendShapes(bestCandidate));
        }
        else
        {
            Debug.LogError("적절한 이빨 렌더러를 찾을 수 없습니다!");
        }
    }
    
    System.Collections.IEnumerator TestTeethBlendShapes(SkinnedMeshRenderer renderer)
    {
        var mesh = renderer.sharedMesh;
        
        Debug.Log("입/턱 BlendShape를 직접 테스트합니다...");
        
        // 입/턱 관련 BlendShape 찾기
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i).ToLower();
            
            if (shapeName.Contains("jaw") || shapeName.Contains("mouth") || 
                shapeName.Contains("open") || shapeName.Contains("teeth"))
            {
                Debug.Log($"테스트 중: {mesh.GetBlendShapeName(i)}");
                
                // BlendShape 애니메이션
                for (float weight = 0; weight <= 100; weight += 20)
                {
                    renderer.SetBlendShapeWeight(i, weight);
                    Debug.Log($"  가중치 {weight} 적용");
                    yield return new WaitForSeconds(0.3f);
                }
                
                // 원래대로 되돌리기
                renderer.SetBlendShapeWeight(i, 0);
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        Debug.Log("BlendShape 테스트 완료!");
        Debug.Log("이빨이 움직였다면 이 렌더러가 올바른 선택입니다.");
        Debug.Log("움직이지 않았다면 다른 문제가 있을 수 있습니다.");
    }
    
    [ContextMenu("강제 이빨 애니메이션")]
    public void ForceTeethAnimation()
    {
        GameObject missy = GameObject.Find("missy1") ?? GameObject.Find("missyT");
        if (missy == null) return;
        
        var lipSync = missy.GetComponent<ConvaiLipSync>();
        if (lipSync == null) return;
        
        var teethRenderer = lipSync.FacialExpressionData.Teeth.Renderer;
        if (teethRenderer == null) return;
        
        Debug.Log("강제 이빨 애니메이션 시작!");
        StartCoroutine(ForceTeethMove(teethRenderer));
    }
    
    System.Collections.IEnumerator ForceTeethMove(SkinnedMeshRenderer renderer)
    {
        var mesh = renderer.sharedMesh;
        
        for (int cycle = 0; cycle < 5; cycle++)
        {
            // 모든 BlendShape를 조금씩 움직여보기
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                renderer.SetBlendShapeWeight(i, 50f);
                yield return new WaitForSeconds(0.1f);
                renderer.SetBlendShapeWeight(i, 0f);
            }
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("강제 애니메이션 완료!");
    }
} 