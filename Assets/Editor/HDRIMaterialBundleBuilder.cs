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

        // HDRI ������ �����ϴ��� Ȯ��
        if (!Directory.Exists(hdriFolder))
        {
            Debug.LogWarning($"������ ã�� �� �����ϴ�: {hdriFolder}");
            return;
        }

        // HDRI ���� ���� ��� ��Ƽ���� ã��
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
        GUILayout.Label("HDRI ��Ƽ���� ���� ����", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // HDRI ���� ����
        EditorGUILayout.BeginHorizontal();
        hdriFolder = EditorGUILayout.TextField("HDRI ����", hdriFolder);
        if (GUILayout.Button("ã��", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("HDRI ���� ����", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // ������Ʈ ��� ��� ��η� ��ȯ
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    hdriFolder = selectedPath;
                    FindHDRIMaterials();
                }
                else
                {
                    EditorUtility.DisplayDialog("���", "������Ʈ ���� ���� ��θ� �������ּ���.", "Ȯ��");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // ��� ��� ����
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("��� ���", outputPath);
        if (GUILayout.Button("ã��", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.SaveFolderPanel("���� ���� ��� ���� ����", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // ������Ʈ ��� ��� ��η� ��ȯ
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    outputPath = selectedPath;
                }
                else
                {
                    EditorUtility.DisplayDialog("���", "������Ʈ ���� ���� ��θ� �������ּ���.", "Ȯ��");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // ��Ƽ���� ���ΰ�ħ ��ư
        if (GUILayout.Button("��Ƽ���� ���ΰ�ħ"))
        {
            FindHDRIMaterials();
        }

        EditorGUILayout.Space();

        // ��ü ����/����
        selectAll = EditorGUILayout.ToggleLeft("��� ����", selectAll);
        if (GUILayout.Button("���� ���� ����"))
        {
            for (int i = 0; i < selectedMaterials.Count; i++)
                selectedMaterials[i] = selectAll;
        }

        EditorGUILayout.Space();

        // ��Ƽ���� ���
        if (materialPaths.Count == 0)
        {
            EditorGUILayout.HelpBox($"{hdriFolder} ������ ��Ƽ������ �����ϴ�.", MessageType.Info);
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            for (int i = 0; i < materialPaths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // ��Ƽ���� ���� ���
                selectedMaterials[i] = EditorGUILayout.ToggleLeft("", selectedMaterials[i], GUILayout.Width(20));

                // ��Ƽ���� �̸�����
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPaths[i]);
                EditorGUILayout.ObjectField(material, typeof(Material), false);

                // ���� �̸� ǥ�� (���� �̸� ���)
                string bundleName = Path.GetFileNameWithoutExtension(materialPaths[i]).ToLower();
                EditorGUILayout.LabelField("���� �̸�: " + bundleName);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();

        // ���� ��ư
        GUI.enabled = materialPaths.Count > 0;
        if (GUILayout.Button("���� ���� ����", GUILayout.Height(30)))
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
            // ���� ��¹��� �̸� �ʱ�ȭ
            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = null;
                }
            }

            // ���õ� ��Ƽ���� ���� ó��
            List<AssetBundleBuild> bundleBuilds = new List<AssetBundleBuild>();
            Dictionary<string, List<string>> bundleAssets = new Dictionary<string, List<string>>();

            for (int i = 0; i < materialPaths.Count; i++)
            {
                if (selectedMaterials[i])
                {
                    string materialPath = materialPaths[i];
                    string materialName = Path.GetFileNameWithoutExtension(materialPath);
                    string bundleName = materialName.ToLower().Replace(" ", "_");

                    // ���� ��Ȳ ǥ��
                    EditorUtility.DisplayProgressBar(
                        "��Ƽ���� ���� ���� ��",
                        $"ó�� ��: {materialName}",
                        (float)i / materialPaths.Count
                    );

                    // ��Ƽ���� �ε�
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                    if (material != null)
                    {
                        // ��Ƽ���� ���� ��� �ؽ�ó ã��
                        List<string> dependencyPaths = new List<string>();
                        dependencyPaths.Add(materialPath); // ��Ƽ���� ��ü �߰�

                        // ���̴� ������Ƽ �˻�
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
                                        Debug.Log($"���� '{bundleName}'�� �ؽ�ó �߰�: {texturePath}");
                                    }
                                }
                            }
                        }

                        // ���� ���� �߰�
                        if (!bundleAssets.ContainsKey(bundleName))
                        {
                            bundleAssets[bundleName] = new List<string>();
                        }

                        foreach (string dependencyPath in dependencyPaths)
                        {
                            if (!bundleAssets[bundleName].Contains(dependencyPath))
                            {
                                bundleAssets[bundleName].Add(dependencyPath);

                                // ��¿� ���� �̸� ����
                                AssetImporter importer = AssetImporter.GetAtPath(dependencyPath);
                                importer.assetBundleName = bundleName;
                            }
                        }

                        Debug.Log($"���� '{bundleName}' ���� �Ϸ�: ��Ƽ����={materialPath}, ������ ��={dependencyPaths.Count}");
                    }
                    else
                    {
                        Debug.LogWarning($"��Ƽ������ �ε��� �� �����ϴ�: {materialPath}");
                    }
                }
            }

            // ���� ���� ���� ����
            foreach (var bundle in bundleAssets)
            {
                AssetBundleBuild bundleBuild = new AssetBundleBuild();
                bundleBuild.assetBundleName = bundle.Key;
                bundleBuild.assetNames = bundle.Value.ToArray();
                bundleBuilds.Add(bundleBuild);

                Debug.Log($"���� '{bundle.Key}'�� ���Ե� ����: {string.Join(", ", bundle.Value)}");
            }

            // ��¹��� ����
            if (bundleBuilds.Count > 0)
            {
                BuildPipeline.BuildAssetBundles(
                    outputPath,
                    bundleBuilds.ToArray(),
                    BuildAssetBundleOptions.ChunkBasedCompression,
                    EditorUserBuildSettings.activeBuildTarget
                );

                Debug.Log($"��Ƽ���� ��¹��� ���� �Ϸ�! ���: {outputPath}");
                EditorUtility.DisplayDialog("���� �Ϸ�", $"{bundleBuilds.Count}���� ��Ƽ���� ��¹����� ���������� ����Ǿ����ϴ�.", "Ȯ��");
            }
            else
            {
                Debug.LogWarning("������ ��¹����� �����ϴ�.");
                EditorUtility.DisplayDialog("�˸�", "������ ��¹����� �����ϴ�.", "Ȯ��");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"��¹��� ���� �� ���� �߻�: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("����", $"��¹��� ���� �� ������ �߻��߽��ϴ�: {e.Message}", "Ȯ��");
        }
        finally
        {
            // ���� ǥ���� ����
            EditorUtility.ClearProgressBar();

            // ���� �����ͺ��̽� ����
            AssetDatabase.Refresh();
        }
    }
}
