using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class QuickTeethFix : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 빠른 이빨 문제 해결 ===");
        Invoke("FixTeethNow", 1f);
    }
    
    void FixTeethNow()
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
        
        // CC_Base_Body 렌더러 찾기
        var renderers = missy.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer bodyRenderer = null;
        
        foreach (var renderer in renderers)
        {
            if (renderer.name == "CC_Base_Body")
            {
                bodyRenderer = renderer;
                break;
            }
        }
        
        if (bodyRenderer == null)
        {
            Debug.LogError("CC_Base_Body 렌더러를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"CC_Base_Body 발견! BlendShape 개수: {bodyRenderer.sharedMesh.blendShapeCount}");
        
        // 이빨 렌더러를 CC_Base_Body로 변경
        var facialData = lipSync.FacialExpressionData;
        facialData.Teeth.Renderer = bodyRenderer;
        facialData.Teeth.WeightBounds = new Vector2(0, 100); // 0-100 범위
        
        Debug.Log("✅ 이빨 렌더러가 CC_Base_Body로 설정되었습니다!");
        Debug.Log("✅ Weight Bounds: 0-100으로 설정");
        
        // 테스트용 BlendShape 확인
        var mesh = bodyRenderer.sharedMesh;
        int jawOpenIndex = -1;
        int mouthOpenIndex = -1;
        
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i);
            if (shapeName == "Jaw_Open")
                jawOpenIndex = i;
            else if (shapeName == "V_Open")
                mouthOpenIndex = i;
        }
        
        if (jawOpenIndex >= 0)
        {
            Debug.Log($"✅ Jaw_Open BlendShape 발견! (인덱스: {jawOpenIndex})");
            StartCoroutine(TestJawMovement(bodyRenderer, jawOpenIndex));
        }
        else if (mouthOpenIndex >= 0)
        {
            Debug.Log($"✅ V_Open BlendShape 발견! (인덱스: {mouthOpenIndex})");
            StartCoroutine(TestJawMovement(bodyRenderer, mouthOpenIndex));
        }
        else
        {
            Debug.Log("테스트용 BlendShape를 찾지 못했지만, 립싱크는 작동할 것입니다.");
        }
        
        Debug.Log("=== 🎉 이빨 문제 해결 완료! 🎉 ===");
        Debug.Log("이제 missy1과 대화해보세요. 이빨이 입과 함께 움직일 것입니다!");
    }
    
    System.Collections.IEnumerator TestJawMovement(SkinnedMeshRenderer renderer, int blendShapeIndex)
    {
        Debug.Log("이빨 움직임 테스트 시작...");
        
        for (int i = 0; i < 3; i++)
        {
            // 입 벌리기
            for (float weight = 0; weight <= 100; weight += 10)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, weight);
                yield return new WaitForSeconds(0.05f);
            }
            
            // 입 닫기
            for (float weight = 100; weight >= 0; weight -= 10)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, weight);
                yield return new WaitForSeconds(0.05f);
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // 원래대로 되돌리기
        renderer.SetBlendShapeWeight(blendShapeIndex, 0);
        
        Debug.Log("✅ 이빨 움직임 테스트 완료!");
        Debug.Log("이빨이 움직였다면 문제가 해결된 것입니다!");
    }
} 