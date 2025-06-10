#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConvaiCameraLockManager))]
public class ConvaiCameraLockManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ConvaiCameraLockManager manager = (ConvaiCameraLockManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("카메라 고정 관리자", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("이 컴포넌트는 관리자가 카메라 이동을 제어하는 데 사용됩니다. " +
                               "재생 중에는 체험자가 이 설정을 볼 수 없습니다.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        // 기본 인스펙터 그리기
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // 버튼들
        EditorGUILayout.LabelField("빠른 제어", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("카메라 고정 토글"))
        {
            manager.ToggleCameraLock();
        }
        
        if (GUILayout.Button("완전 고정"))
        {
            manager.SetCameraLock(true);
            manager.SetLookAroundAllowed(false);
        }
        
        if (GUILayout.Button("둘러보기만 허용"))
        {
            manager.SetCameraLock(true);
            manager.SetLookAroundAllowed(true);
        }
        
        if (GUILayout.Button("모든 제한 해제"))
        {
            manager.SetCameraLock(false);
        }
        
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 현재 상태 표시
        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);
        GUI.enabled = false;
        EditorGUILayout.Toggle("카메라 고정됨", manager.IsCameraLocked());
        GUI.enabled = true;
        
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("게임이 실행 중입니다. 설정을 변경하여 실시간으로 테스트해보세요!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("게임을 실행하여 카메라 고정 기능을 테스트하세요.", MessageType.Warning);
        }
    }
}
#endif 