using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;

public class TeethDebugger : MonoBehaviour
{
    [Header("Debug Teeth Configuration")]
    public GameObject targetCharacter;
    
    void Start()
    {
        // Auto-find missyT if not assigned
        if (targetCharacter == null)
        {
            targetCharacter = GameObject.Find("missyT");
        }
        
        // Auto-run debug on start
        Invoke("DebugTeethSetup", 1f); // Wait 1 second for initialization
    }
    
    [ContextMenu("Debug Teeth Setup")]
    public void DebugTeethSetup()
    {
        Debug.Log("=== STARTING TEETH DEBUG ===");
        
        if (targetCharacter == null)
        {
            Debug.LogError("Target character not found! Looking for missyT...");
            targetCharacter = GameObject.Find("missyT");
            if (targetCharacter == null)
            {
                Debug.LogError("missyT GameObject not found in scene!");
                return;
            }
        }
        
        Debug.Log($"Found target character: {targetCharacter.name}");
        
        var convaiLipSync = targetCharacter.GetComponent<ConvaiLipSync>();
        if (convaiLipSync == null)
        {
            Debug.LogError("ConvaiLipSync component not found on " + targetCharacter.name);
            return;
        }
        
        Debug.Log("ConvaiLipSync component found!");
        
        var facialData = convaiLipSync.FacialExpressionData;
        
        Debug.Log("=== FACIAL EXPRESSION DATA ===");
        Debug.Log($"Head Renderer: {(facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL")}");
        Debug.Log($"Head WeightBounds: {facialData.Head.WeightBounds}");
        Debug.Log($"Head VisemeEffectorsList: {(facialData.Head.VisemeEffectorsList != null ? "Assigned" : "NULL")}");
        
        Debug.Log($"Teeth Renderer: {(facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL")}");
        Debug.Log($"Teeth WeightBounds: {facialData.Teeth.WeightBounds}");
        Debug.Log($"Teeth VisemeEffectorsList: {(facialData.Teeth.VisemeEffectorsList != null ? "Assigned" : "NULL")}");
        
        Debug.Log($"Tongue Renderer: {(facialData.Tongue.Renderer != null ? facialData.Tongue.Renderer.name : "NULL")}");
        Debug.Log($"Tongue WeightBounds: {facialData.Tongue.WeightBounds}");
        Debug.Log($"Tongue VisemeEffectorsList: {(facialData.Tongue.VisemeEffectorsList != null ? "Assigned" : "NULL")}");
        
        // Check if teeth renderer has BlendShapes
        if (facialData.Teeth.Renderer != null)
        {
            var mesh = facialData.Teeth.Renderer.sharedMesh;
            if (mesh != null)
            {
                Debug.Log($"Teeth Mesh BlendShape Count: {mesh.blendShapeCount}");
                for (int i = 0; i < Mathf.Min(mesh.blendShapeCount, 10); i++) // Show first 10 only
                {
                    Debug.Log($"  BlendShape {i}: {mesh.GetBlendShapeName(i)}");
                }
            }
            else
            {
                Debug.LogError("Teeth renderer mesh is null!");
            }
        }
        else
        {
            Debug.LogError("Teeth Renderer is NULL - This is likely the problem!");
        }
        
        // Check jaw bones
        Debug.Log($"Jaw Bone: {(facialData.JawBone != null ? facialData.JawBone.name : "NULL")}");
        Debug.Log($"Tongue Bone: {(facialData.TongueBone != null ? facialData.TongueBone.name : "NULL")}");
        
        Debug.Log("=== END TEETH DEBUG ===");
    }
    
    [ContextMenu("Test Teeth BlendShape")]
    public void TestTeethBlendShape()
    {
        Debug.Log("Testing Teeth BlendShape...");
        
        if (targetCharacter == null)
        {
            Debug.LogError("Target character is null!");
            return;
        }
        
        var convaiLipSync = targetCharacter.GetComponent<ConvaiLipSync>();
        if (convaiLipSync == null)
        {
            Debug.LogError("ConvaiLipSync not found!");
            return;
        }
        
        var teethRenderer = convaiLipSync.FacialExpressionData.Teeth.Renderer;
        if (teethRenderer == null)
        {
            Debug.LogError("Teeth renderer not found!");
            return;
        }
        
        // Test jaw open blendshape
        var mesh = teethRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i);
            if (shapeName.Contains("Jaw") || shapeName.Contains("Open"))
            {
                Debug.Log($"Testing BlendShape: {shapeName} (Index: {i})");
                teethRenderer.SetBlendShapeWeight(i, 100f);
                break;
            }
        }
    }
} 