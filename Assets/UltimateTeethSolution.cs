using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class UltimateTeethSolution : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 궁극의 이빨 문제 해결 ===");
        Invoke("SolveCompleteTeethProblem", 2f);
    }
    
    void SolveCompleteTeethProblem()
    {
        // missy1 찾기
        GameObject missy = GameObject.Find("missy1");
        if (missy == null)
            missy = GameObject.Find("missyT");
            
        if (missy == null)
        {
            Debug.LogError("missy 캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        var lipSync = missy.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 컴포넌트가 없습니다!");
            return;
        }
        
        var facialData = lipSync.FacialExpressionData;
        
        Debug.Log("=== 현재 설정 확인 ===");
        Debug.Log($"Head Renderer: {facialData.Head.Renderer?.name ?? "NULL"}");
        Debug.Log($"Teeth Renderer: {facialData.Teeth.Renderer?.name ?? "NULL"}");
        
        // 렌더러들 찾기
        var renderers = missy.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer bodyRenderer = null;
        SkinnedMeshRenderer teethRenderer = null;
        
        foreach (var renderer in renderers)
        {
            Debug.Log($"발견된 렌더러: {renderer.name} (BlendShape: {renderer.sharedMesh?.blendShapeCount ?? 0}개)");
            
            if (renderer.name == "CC_Base_Body")
                bodyRenderer = renderer;
            else if (renderer.name == "CC_Base_Teeth")
                teethRenderer = renderer;
        }
        
        if (bodyRenderer == null)
        {
            Debug.LogError("CC_Base_Body를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log("=== 문제 해결 방법 선택 ===");
        
        // 방법 1: Head를 다른 렌더러로, Teeth를 Body로 설정
        if (teethRenderer != null)
        {
            Debug.Log("방법 1: Head=CC_Base_Teeth, Teeth=CC_Base_Body로 설정");
            facialData.Head.Renderer = teethRenderer;
            facialData.Teeth.Renderer = bodyRenderer;
            facialData.Teeth.WeightBounds = new Vector2(0, 100);
            
            Debug.Log("✅ Head를 CC_Base_Teeth로, Teeth를 CC_Base_Body로 설정 완료");
        }
        else
        {
            Debug.Log("방법 2: Teeth 기능 비활성화, Head만 CC_Base_Body 사용");
            facialData.Head.Renderer = bodyRenderer;
            facialData.Teeth.Renderer = null; // 이빨 렌더러 비활성화
            
            Debug.Log("✅ Head만 CC_Base_Body 사용, Teeth 비활성화");
        }
        
        // 방법 3: 실제 이빨 GameObject 가시성 조작
        Debug.Log("=== 실제 이빨 모델 가시성 제어 ===");
        
        GameObject teethObject = null;
        foreach (Transform child in missy.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains("teeth") || 
                child.name.ToLower().Contains("tooth") ||
                child.name == "CC_Base_Teeth")
            {
                teethObject = child.gameObject;
                Debug.Log($"이빨 오브젝트 발견: {child.name}");
                break;
            }
        }
        
        if (teethObject != null)
        {
            var teethMeshRenderer = teethObject.GetComponent<SkinnedMeshRenderer>();
            if (teethMeshRenderer != null)
            {
                Debug.Log("이빨 MeshRenderer 발견 - 투명도 조작 시도");
                StartCoroutine(AnimateTeethVisibility(teethMeshRenderer));
            }
        }
        
        // 방법 4: BlendShape 직접 조작으로 이빨 효과 시뮬레이션
        Debug.Log("=== BlendShape 직접 조작 테스트 ===");
        StartCoroutine(DirectBlendShapeTeethAnimation(bodyRenderer));
        
        Debug.Log("=== 🎯 완전한 해결 시도 완료 🎯 ===");
    }
    
    System.Collections.IEnumerator AnimateTeethVisibility(SkinnedMeshRenderer teethRenderer)
    {
        Debug.Log("이빨 가시성 애니메이션 시작...");
        
        var materials = teethRenderer.materials;
        var originalColors = new Color[materials.Length];
        
        // 원래 색상 저장
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].HasProperty("_Color"))
                originalColors[i] = materials[i].color;
            else if (materials[i].HasProperty("_BaseColor"))
                originalColors[i] = materials[i].GetColor("_BaseColor");
        }
        
        // 이빨 깜빡임 효과 (가시성 확인)
        for (int cycle = 0; cycle < 3; cycle++)
        {
            // 투명하게
            for (int i = 0; i < materials.Length; i++)
            {
                Color transparentColor = originalColors[i];
                transparentColor.a = 0.3f;
                
                if (materials[i].HasProperty("_Color"))
                    materials[i].color = transparentColor;
                else if (materials[i].HasProperty("_BaseColor"))
                    materials[i].SetColor("_BaseColor", transparentColor);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // 원래대로
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].HasProperty("_Color"))
                    materials[i].color = originalColors[i];
                else if (materials[i].HasProperty("_BaseColor"))
                    materials[i].SetColor("_BaseColor", originalColors[i]);
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        Debug.Log("이빨 가시성 테스트 완료 - 이빨이 깜빡였다면 렌더링은 정상입니다.");
    }
    
    System.Collections.IEnumerator DirectBlendShapeTeethAnimation(SkinnedMeshRenderer bodyRenderer)
    {
        Debug.Log("직접 BlendShape 이빨 애니메이션 시작...");
        
        var mesh = bodyRenderer.sharedMesh;
        int[] teethShapes = new int[10];
        int shapeCount = 0;
        
        // 이빨/턱 관련 BlendShape 찾기
        for (int i = 0; i < mesh.blendShapeCount && shapeCount < 10; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i).ToLower();
            if (shapeName.Contains("jaw") || shapeName.Contains("open") || 
                shapeName.Contains("mouth") && (shapeName.Contains("upper") || shapeName.Contains("lower")))
            {
                teethShapes[shapeCount] = i;
                Debug.Log($"이빨 효과용 BlendShape: {mesh.GetBlendShapeName(i)} (인덱스: {i})");
                shapeCount++;
            }
        }
        
        if (shapeCount == 0)
        {
            Debug.Log("적절한 BlendShape를 찾지 못했습니다.");
            yield break;
        }
        
        // 말하는 효과 시뮬레이션
        Debug.Log($"{shapeCount}개의 BlendShape로 말하기 효과 시뮬레이션...");
        
        for (int cycle = 0; cycle < 5; cycle++)
        {
            // 입 벌리기 (이빨 보이기)
            for (int i = 0; i < shapeCount; i++)
            {
                float weight = Random.Range(20f, 80f);
                bodyRenderer.SetBlendShapeWeight(teethShapes[i], weight);
            }
            
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            
            // 입 닫기
            for (int i = 0; i < shapeCount; i++)
            {
                bodyRenderer.SetBlendShapeWeight(teethShapes[i], 0);
            }
            
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }
        
        Debug.Log("직접 BlendShape 애니메이션 완료!");
        Debug.Log("위 테스트에서 이빨이 보였다면 BlendShape 방식으로 작동 가능합니다.");
    }
    
    [ContextMenu("Reset All Renderers")]
    public void ResetAllRenderers()
    {
        Debug.Log("모든 렌더러 설정 초기화...");
        
        GameObject missy = GameObject.Find("missy1") ?? GameObject.Find("missyT");
        if (missy == null) return;
        
        var lipSync = missy.GetComponent<ConvaiLipSync>();
        if (lipSync == null) return;
        
        var facialData = lipSync.FacialExpressionData;
        var renderers = missy.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (var renderer in renderers)
        {
            if (renderer.name == "CC_Base_Body")
            {
                facialData.Head.Renderer = renderer;
                Debug.Log("Head 렌더러를 CC_Base_Body로 재설정");
            }
            else if (renderer.name == "CC_Base_Teeth")
            {
                facialData.Teeth.Renderer = renderer;
                Debug.Log("Teeth 렌더러를 CC_Base_Teeth로 재설정");
            }
        }
        
        facialData.Teeth.WeightBounds = new Vector2(0, 1);
        Debug.Log("렌더러 초기화 완료");
    }
} 