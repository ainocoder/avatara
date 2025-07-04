using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;
using Convai.Scripts.Runtime.Features.LipSync.Visemes;

public class MissyTeethFixer : MonoBehaviour
{
    [Header("자동 이빨 설정 도구")]
    public GameObject missyCharacter;
    
    [ContextMenu("Missy1 이빨 문제 해결")]
    public void FixMissyTeeth()
    {
        Debug.Log("=== MISSY1 이빨 문제 해결 시작 ===");
        
        // missy1 캐릭터 찾기
        if (missyCharacter == null)
        {
            missyCharacter = GameObject.Find("missy1");
            if (missyCharacter == null)
            {
                missyCharacter = GameObject.Find("missyT");
            }
        }
        
        if (missyCharacter == null)
        {
            Debug.LogError("missy1 또는 missyT 캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"캐릭터 발견: {missyCharacter.name}");
        
        // ConvaiLipSync 컴포넌트 가져오기
        var lipSync = missyCharacter.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError($"ConvaiLipSync 컴포넌트가 {missyCharacter.name}에 없습니다!");
            return;
        }
        
        Debug.Log("ConvaiLipSync 컴포넌트 발견!");
        
        // 이빨 렌더러 찾기
        SkinnedMeshRenderer teethRenderer = FindTeethRenderer(missyCharacter);
        
        if (teethRenderer == null)
        {
            Debug.LogError("이빨 렌더러를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"이빨 렌더러 발견: {teethRenderer.name}");
        
        // Facial Expression Data 설정
        var facialData = lipSync.FacialExpressionData;
        
        // Teeth Renderer 설정
        facialData.Teeth.Renderer = teethRenderer;
        
        // Teeth VisemeEffectorsList 설정 (ARKit 기준)
        var arkitTeethVisemes = Resources.Load<VisemeEffectorsList>("Convai/Visemes/ARKit/Teeth_VisemeEffectors_ARKit");
        if (arkitTeethVisemes != null)
        {
            facialData.Teeth.VisemeEffectorsList = arkitTeethVisemes;
            Debug.Log("ARKit Teeth VisemeEffectors 설정 완료");
        }
        else
        {
            // 다른 경로들도 시도해보기
            Debug.LogWarning("ARKit Teeth VisemeEffectors를 찾을 수 없습니다.");
            Debug.Log("대안으로 Reallusion 또는 다른 Viseme 설정을 시도합니다...");
            
            // Reallusion 시도
            var reallusion = Resources.Load<VisemeEffectorsList>("Convai/Visemes/Reallusion/Teeth_VisemeEffectors_Reallusion");
            if (reallusion != null)
            {
                facialData.Teeth.VisemeEffectorsList = reallusion;
                Debug.Log("Reallusion Teeth VisemeEffectors 설정 완료");
            }
                         else
             {
                 Debug.LogWarning("Teeth VisemeEffectors를 찾을 수 없습니다.");
                 Debug.Log("VisemeEffectors 없이도 기본 설정으로 진행합니다...");
                 Debug.Log("나중에 Inspector에서 수동으로 VisemeEffectors를 설정하면 더 나은 결과를 얻을 수 있습니다.");
             }
        }
        
        // Weight Bounds 설정
        facialData.Teeth.WeightBounds = new Vector2(0, 1);
        
        Debug.Log("=== MISSY1 이빨 설정 완료! ===");
        Debug.Log("이제 missy1이 말할 때 이빨이 정상적으로 움직일 것입니다.");
        
        // 변경사항 저장
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(lipSync);
        if (missyCharacter.scene.name != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(missyCharacter.scene);
        }
        #endif
    }
    
    private SkinnedMeshRenderer FindTeethRenderer(GameObject character)
    {
        Debug.Log("이빨 렌더러 검색 중...");
        
        // 모든 하위 SkinnedMeshRenderer 검색
        var renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (var renderer in renderers)
        {
            // 이름에 "teeth", "tooth", "Teeth", "Tooth" 포함된 것 찾기
            if (renderer.name.ToLower().Contains("teeth") || 
                renderer.name.ToLower().Contains("tooth"))
            {
                Debug.Log($"이빨 렌더러 후보 발견: {renderer.name}");
                
                // BlendShape가 있는지 확인
                if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0)
                {
                    Debug.Log($"BlendShape 개수: {renderer.sharedMesh.blendShapeCount}");
                    
                    // 첫 번째 BlendShape 이름 출력
                    for (int i = 0; i < Mathf.Min(5, renderer.sharedMesh.blendShapeCount); i++)
                    {
                        Debug.Log($"  BlendShape {i}: {renderer.sharedMesh.GetBlendShapeName(i)}");
                    }
                    
                    return renderer;
                }
            }
        }
        
        // 이름으로 못 찾으면 메시 이름으로 검색
        foreach (var renderer in renderers)
        {
            if (renderer.sharedMesh != null)
            {
                string meshName = renderer.sharedMesh.name.ToLower();
                if (meshName.Contains("teeth") || meshName.Contains("tooth"))
                {
                    Debug.Log($"메시 이름으로 이빨 렌더러 발견: {renderer.name} (메시: {renderer.sharedMesh.name})");
                    return renderer;
                }
            }
        }
        
                 Debug.Log("이름으로 이빨 렌더러를 찾을 수 없습니다.");
         Debug.Log("BlendShape가 있는 렌더러 중에서 선택해보겠습니다...");
         
         // BlendShape가 많은 렌더러를 우선적으로 선택 (얼굴/머리 관련일 가능성이 높음)
         SkinnedMeshRenderer bestCandidate = null;
         int maxBlendShapes = 0;
         
         foreach (var renderer in renderers)
         {
             if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > maxBlendShapes)
             {
                 // 확실히 몸통이 아닌 것들만 선택
                 string name = renderer.name.ToLower();
                 if (!name.Contains("body") && !name.Contains("arm") && !name.Contains("leg"))
                 {
                     maxBlendShapes = renderer.sharedMesh.blendShapeCount;
                     bestCandidate = renderer;
                 }
             }
         }
         
         if (bestCandidate != null)
         {
             Debug.Log($"BlendShape가 가장 많은 렌더러를 이빨 렌더러로 선택: {bestCandidate.name} (BlendShape: {maxBlendShapes}개)");
             Debug.Log("이것이 올바른 선택인지 확인하고, 필요시 수동으로 조정하세요.");
             return bestCandidate;
         }
         
         Debug.Log("모든 렌더러 목록:");
         foreach (var renderer in renderers)
         {
             string meshInfo = renderer.sharedMesh != null ? $" (메시: {renderer.sharedMesh.name}, BlendShape: {renderer.sharedMesh.blendShapeCount}개)" : " (메시 없음)";
             Debug.Log($"  - {renderer.name}{meshInfo}");
         }
         
         return null;
    }
    
    [ContextMenu("현재 설정 확인")]
    public void CheckCurrentSettings()
    {
        if (missyCharacter == null)
        {
            missyCharacter = GameObject.Find("missy1") ?? GameObject.Find("missyT");
        }
        
        if (missyCharacter == null)
        {
            Debug.LogError("캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        var lipSync = missyCharacter.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 컴포넌트가 없습니다!");
            return;
        }
        
        var facialData = lipSync.FacialExpressionData;
        
        Debug.Log("=== 현재 립싱크 설정 ===");
        Debug.Log($"Head Renderer: {(facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL")}");
        Debug.Log($"Teeth Renderer: {(facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL")}");
        Debug.Log($"Teeth VisemeEffectors: {(facialData.Teeth.VisemeEffectorsList != null ? "설정됨" : "NULL")}");
        Debug.Log($"Teeth WeightBounds: {facialData.Teeth.WeightBounds}");
    }
} 