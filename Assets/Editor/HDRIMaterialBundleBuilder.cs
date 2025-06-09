using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class HDRIMaterialBundleBuilder : EditorWindow
{
    private Vector2 scrollPos;
    private List<string> materialPaths = new List<string>();
    private List<bool> selectedMaterials = new List<bool>();
    private bool selectAll = true;
    private string hdriFolder = "Assets/HDRI";
    private string outputPath = "Assets/AssetBundles/HDRI";

    [MenuItem("Tools/HDRI Material Bundle Builder")]
    static void Init()
    {
        HDRIMaterialBundleBuilder window = (HDRIMaterialBundleBuilder)EditorWindow.GetWindow(typeof(HDRIMaterialBundleBuilder));
        window.titleContent = new GUIContent("HDRI Material Bundle Builder");
        window.Show();
    }

    void OnEnable()
    {
        FindHDRIMaterials();
    }

    void FindHDRIMaterials()
    {
        materialPaths.Clear();
        selectedMaterials.Clear();

        // HDRI 폴더가 존재하는지 확인
        if (!Directory.Exists(hdriFolder))
        {
            Debug.LogWarning($"폴더를 찾을 수 없습니다: {hdriFolder}");
            return;
        }

        // HDRI 폴더 내의 모든 머티리얼 찾기
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { hdriFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            materialPaths.Add(path);
            selectedMaterials.Add(true);
        }
    }

    void OnGUI()
    {
        GUILayout.Label("HDRI 머티리얼 번들 빌더", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // HDRI 폴더 설정
        EditorGUILayout.BeginHorizontal();
        hdriFolder = EditorGUILayout.TextField("HDRI 폴더", hdriFolder);
        if (GUILayout.Button("찾기", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("HDRI 폴더 선택", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 프로젝트 경로 상대 경로로 변환
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    hdriFolder = selectedPath;
                    FindHDRIMaterials();
                }
                else
                {
                    EditorUtility.DisplayDialog("경고", "프로젝트 폴더 내의 경로를 선택해주세요.", "확인");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // 출력 경로 설정
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("출력 경로", outputPath);
        if (GUILayout.Button("찾기", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.SaveFolderPanel("에셋 번들 출력 폴더 선택", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 프로젝트 경로 상대 경로로 변환
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    outputPath = selectedPath;
                }
                else
                {
                    EditorUtility.DisplayDialog("경고", "프로젝트 폴더 내의 경로를 선택해주세요.", "확인");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 머티리얼 새로고침 버튼
        if (GUILayout.Button("머티리얼 새로고침"))
        {
            FindHDRIMaterials();
        }

        EditorGUILayout.Space();

        // 전체 선택/해제
        selectAll = EditorGUILayout.ToggleLeft("모두 선택", selectAll);
        if (GUILayout.Button("선택 상태 적용"))
        {
            for (int i = 0; i < selectedMaterials.Count; i++)
                selectedMaterials[i] = selectAll;
        }

        EditorGUILayout.Space();

        // 머티리얼 목록
        if (materialPaths.Count == 0)
        {
            EditorGUILayout.HelpBox($"{hdriFolder} 폴더에 머티리얼이 없습니다.", MessageType.Info);
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            for (int i = 0; i < materialPaths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // 머티리얼 선택 토글
                selectedMaterials[i] = EditorGUILayout.ToggleLeft("", selectedMaterials[i], GUILayout.Width(20));

                // 머티리얼 미리보기
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPaths[i]);
                EditorGUILayout.ObjectField(material, typeof(Material), false);

                // 번들 이름 표시 (파일 이름 기반)
                string bundleName = Path.GetFileNameWithoutExtension(materialPaths[i]).ToLower();
                EditorGUILayout.LabelField("번들 이름: " + bundleName);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();

        // 빌드 버튼
        GUI.enabled = materialPaths.Count > 0;
        if (GUILayout.Button("에셋 번들 빌드", GUILayout.Height(30)))
        {
            BuildMaterialBundles();
        }
        GUI.enabled = true;
    }

    void BuildMaterialBundles()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        try
        {
            // 기존 어셋번들 이름 초기화
            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = null;
                }
            }

            // 선택된 머티리얼에 대한 처리
            List<AssetBundleBuild> bundleBuilds = new List<AssetBundleBuild>();
            Dictionary<string, List<string>> bundleAssets = new Dictionary<string, List<string>>();

            for (int i = 0; i < materialPaths.Count; i++)
            {
                if (selectedMaterials[i])
                {
                    string materialPath = materialPaths[i];
                    string materialName = Path.GetFileNameWithoutExtension(materialPath);
                    string bundleName = materialName.ToLower().Replace(" ", "_");

                    // 진행 상황 표시
                    EditorUtility.DisplayProgressBar(
                        "머티리얼 번들 빌드 중",
                        $"처리 중: {materialName}",
                        (float)i / materialPaths.Count
                    );

                    // 머티리얼 로드
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                    if (material != null)
                    {
                        // 머티리얼에 사용된 모든 텍스처 찾기
                        List<string> dependencyPaths = new List<string>();
                        dependencyPaths.Add(materialPath); // 머티리얼 자체 추가

                        // 셰이더 프로퍼티 검사
                        Shader shader = material.shader;
                        int propertyCount = ShaderUtil.GetPropertyCount(shader);

                        for (int j = 0; j < propertyCount; j++)
                        {
                            if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
                            {
                                string propertyName = ShaderUtil.GetPropertyName(shader, j);
                                Texture texture = material.GetTexture(propertyName);

                                if (texture != null)
                                {
                                    string texturePath = AssetDatabase.GetAssetPath(texture);
                                    if (!string.IsNullOrEmpty(texturePath))
                                    {
                                        dependencyPaths.Add(texturePath);
                                        Debug.Log($"번들 '{bundleName}'에 텍스처 추가: {texturePath}");
                                    }
                                }
                            }
                        }

                        // 번들 에셋 추가
                        if (!bundleAssets.ContainsKey(bundleName))
                        {
                            bundleAssets[bundleName] = new List<string>();
                        }

                        foreach (string dependencyPath in dependencyPaths)
                        {
                            if (!bundleAssets[bundleName].Contains(dependencyPath))
                            {
                                bundleAssets[bundleName].Add(dependencyPath);

                                // 어셋에 번들 이름 설정
                                AssetImporter importer = AssetImporter.GetAtPath(dependencyPath);
                                importer.assetBundleName = bundleName;
                            }
                        }

                        Debug.Log($"번들 '{bundleName}' 설정 완료: 머티리얼={materialPath}, 의존성 수={dependencyPaths.Count}");
                    }
                    else
                    {
                        Debug.LogWarning($"머티리얼을 로드할 수 없습니다: {materialPath}");
                    }
                }
            }

            // 번들 빌드 정보 생성
            foreach (var bundle in bundleAssets)
            {
                AssetBundleBuild bundleBuild = new AssetBundleBuild();
                bundleBuild.assetBundleName = bundle.Key;
                bundleBuild.assetNames = bundle.Value.ToArray();
                bundleBuilds.Add(bundleBuild);

                Debug.Log($"번들 '{bundle.Key}'에 포함된 에셋: {string.Join(", ", bundle.Value)}");
            }

            // 어셋번들 빌드
            if (bundleBuilds.Count > 0)
            {
                BuildPipeline.BuildAssetBundles(
                    outputPath,
                    bundleBuilds.ToArray(),
                    BuildAssetBundleOptions.ChunkBasedCompression,
                    EditorUserBuildSettings.activeBuildTarget
                );

                Debug.Log($"머티리얼 어셋번들 빌드 완료! 경로: {outputPath}");
                EditorUtility.DisplayDialog("빌드 완료", $"{bundleBuilds.Count}개의 머티리얼 어셋번들이 성공적으로 빌드되었습니다.", "확인");
            }
            else
            {
                Debug.LogWarning("빌드할 어셋번들이 없습니다.");
                EditorUtility.DisplayDialog("알림", "빌드할 어셋번들이 없습니다.", "확인");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"어셋번들 빌드 중 오류 발생: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("오류", $"어셋번들 빌드 중 오류가 발생했습니다: {e.Message}", "확인");
        }
        finally
        {
            // 진행 표시줄 제거
            EditorUtility.ClearProgressBar();

            // 에셋 데이터베이스 갱신
            AssetDatabase.Refresh();
        }
    }
}
