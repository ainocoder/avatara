using UnityEngine;
using UnityEditor;

public class BlendShapeViewerWindow : EditorWindow
{
    private Vector2 scrollPos;
    private SkinnedMeshRenderer selectedRenderer;

    [MenuItem("Tools/Blend Shape Viewer")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeViewerWindow>("Blend Shape Viewer");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        selectedRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", selectedRenderer, typeof(SkinnedMeshRenderer), true);

        if (selectedRenderer != null && selectedRenderer.sharedMesh != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Blend Shapes: {selectedRenderer.sharedMesh.blendShapeCount}");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < selectedRenderer.sharedMesh.blendShapeCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"#{i}: {selectedRenderer.sharedMesh.GetBlendShapeName(i)}");
                float weight = selectedRenderer.GetBlendShapeWeight(i);
                float newWeight = EditorGUILayout.Slider(weight, 0, 100);
                if (weight != newWeight)
                {
                    selectedRenderer.SetBlendShapeWeight(i, newWeight);
                    EditorUtility.SetDirty(selectedRenderer);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}

