using UnityEngine;
using UnityEditor;

public class BlendShapeLister : EditorWindow
{
    [MenuItem("Tools/BlendShape Lister")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeLister>("BlendShape Lister");
    }

    private SkinnedMeshRenderer smr;

    void OnGUI()
    {
        smr = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("SkinnedMeshRenderer", smr, typeof(SkinnedMeshRenderer), true);

        if (smr != null && GUILayout.Button("Print BlendShapes"))
        {
            var mesh = smr.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                Debug.Log($"{smr.gameObject.name} - {i}: {mesh.GetBlendShapeName(i)}");
            }
        }
    }
}