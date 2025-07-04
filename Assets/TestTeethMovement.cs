using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class TestTeethMovement : MonoBehaviour
{
    [Header("이빨 움직임 테스트")]
    public GameObject testCharacter;
    public bool isAnimating = false;
    
    [ContextMenu("이빨 움직임 테스트")]
    public void TestTeethAnimation()
    {
        if (testCharacter == null)
        {
            testCharacter = GameObject.Find("missy1") ?? GameObject.Find("missyT");
        }
        
        if (testCharacter == null)
        {
            Debug.LogError("테스트할 캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        var lipSync = testCharacter.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 컴포넌트가 없습니다!");
            return;
        }
        
        var teethRenderer = lipSync.FacialExpressionData.Teeth.Renderer;
        if (teethRenderer == null)
        {
            Debug.LogError("이빨 렌더러가 설정되지 않았습니다! 먼저 MissyTeethFixer를 실행하세요.");
            return;
        }
        
        Debug.Log("이빨 움직임 테스트 시작...");
        
        // 간단한 이빨 애니메이션 테스트
        StartCoroutine(AnimateTeeth(teethRenderer));
    }
    
    private System.Collections.IEnumerator AnimateTeeth(SkinnedMeshRenderer renderer)
    {
        isAnimating = true;
        var mesh = renderer.sharedMesh;
        
        Debug.Log("입 벌리기/닫기 애니메이션 테스트 중...");
        
        for (int cycle = 0; cycle < 3; cycle++)
        {
            // 입 벌리기
            for (float t = 0; t <= 1; t += 0.1f)
            {
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string shapeName = mesh.GetBlendShapeName(i);
                    if (shapeName.ToLower().Contains("jaw") || 
                        shapeName.ToLower().Contains("open") ||
                        shapeName.ToLower().Contains("mouth"))
                    {
                        renderer.SetBlendShapeWeight(i, t * 100f);
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
            
            // 입 닫기
            for (float t = 1; t >= 0; t -= 0.1f)
            {
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string shapeName = mesh.GetBlendShapeName(i);
                    if (shapeName.ToLower().Contains("jaw") || 
                        shapeName.ToLower().Contains("open") ||
                        shapeName.ToLower().Contains("mouth"))
                    {
                        renderer.SetBlendShapeWeight(i, t * 100f);
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        Debug.Log("이빨 움직임 테스트 완료!");
        isAnimating = false;
    }
    
    [ContextMenu("BlendShape 목록 출력")]
    public void ListBlendShapes()
    {
        if (testCharacter == null)
        {
            testCharacter = GameObject.Find("missy1") ?? GameObject.Find("missyT");
        }
        
        if (testCharacter == null)
        {
            Debug.LogError("캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        var lipSync = testCharacter.GetComponent<ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync 컴포넌트가 없습니다!");
            return;
        }
        
        var teethRenderer = lipSync.FacialExpressionData.Teeth.Renderer;
        if (teethRenderer == null)
        {
            Debug.LogError("이빨 렌더러가 설정되지 않았습니다!");
            return;
        }
        
        var mesh = teethRenderer.sharedMesh;
        Debug.Log($"=== {teethRenderer.name}의 BlendShape 목록 ===");
        Debug.Log($"총 BlendShape 개수: {mesh.blendShapeCount}");
        
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            Debug.Log($"  {i}: {mesh.GetBlendShapeName(i)}");
        }
    }
} 