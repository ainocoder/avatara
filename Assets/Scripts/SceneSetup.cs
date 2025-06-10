using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    // 유니티 에디터에서 여기에 스카이박스 재질을 연결합니다.
    public Material skyboxMaterial;

    void Start()
    {
        // 스카이박스 재질이 설정되어 있는지 확인
        if (skyboxMaterial != null)
        {
            // 씬의 스카이박스를 지정된 재질로 설정
            RenderSettings.skybox = skyboxMaterial;
            // 환경 조명을 업데이트하여 변경사항을 즉시 반영
            DynamicGI.UpdateEnvironment();
        }

        // 메인 카메라의 배경을 스카이박스로 설정 (안전장치)
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
