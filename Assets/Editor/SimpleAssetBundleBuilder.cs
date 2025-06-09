using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SimpleAssetBundleBuilder : EditorWindow
{
    private Vector2 scrollPos;
    private List<string> prefabPaths;
    private List<bool> selectedPrefabs;
    private bool selectAll = true;

    [MenuItem("Tools/Simple AssetBundle Builder")]
    static void Init()
    {
        SimpleAssetBundleBuilder window = (SimpleAssetBundleBuilder)EditorWindow.GetWindow(typeof(SimpleAssetBundleBuilder));
        window.titleContent = new GUIContent("Simple AssetBundle Builder");
        window.Show();
    }

    void OnEnable()
    {
        LoadPrefabs();
    }

    void LoadPrefabs()
    {
        string prefabFolder = "Assets/Characters";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });

        prefabPaths = new List<string>();
        selectedPrefabs = new List<bool>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            prefabPaths.Add(path);
            selectedPrefabs.Add(true);
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Characters 폴더 내 프리팹 목록", EditorStyles.boldLabel);

        selectAll = EditorGUILayout.ToggleLeft("모두 선택", selectAll);
        if (GUILayout.Button("선택 상태 적용"))
        {
            for (int i = 0; i < selectedPrefabs.Count; i++)
                selectedPrefabs[i] = selectAll;
        }

        GUILayout.Space(10);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        for (int i = 0; i < prefabPaths.Count; i++)
        {
            selectedPrefabs[i] = EditorGUILayout.ToggleLeft(prefabPaths[i], selectedPrefabs[i]);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("어셋번들 빌드"))
        {
            BuildAssetBundles();
        }
    }

    void BuildAssetBundles()
    {
        string outputPath = "Assets/Assetbundles/Characters";
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        // 기존 어셋번들 이름 초기화
        foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = null;
            }
        }

        // 선택된 프리팹에 어셋번들 이름 설정 (확장자 없이)
        for (int i = 0; i < prefabPaths.Count; i++)
        {
            if (selectedPrefabs[i])
            {
                AssetImporter importer = AssetImporter.GetAtPath(prefabPaths[i]);
                string prefabName = Path.GetFileNameWithoutExtension(prefabPaths[i]);
                // .bundle 확장자 제거
                importer.assetBundleName = prefabName.ToLower();
            }
        }

        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        // 빌드 후 어셋번들 이름 초기화
        foreach (var assetPath in prefabPaths)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = null;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("어셋번들 빌드 완료! 경로: " + outputPath);
    }
}
