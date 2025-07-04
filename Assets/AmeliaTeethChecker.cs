using UnityEngine;

public class AmeliaTeethChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== AMELIA TEETH CHECKER STARTED ===");
        
        // Find Amelia
        GameObject amelia = GameObject.Find("Amelia");
        if (amelia == null)
        {
            Debug.LogError("Amelia not found!");
            return;
        }
        
        Debug.Log("Found Amelia: " + amelia.name);
        
        // Check ConvaiLipSync component
        var lipSync = amelia.GetComponent<Convai.Scripts.Runtime.Features.LipSync.ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync not found on Amelia!");
            return;
        }
        
        Debug.Log("Amelia ConvaiLipSync found!");
        
        // Check facial expression data
        var facialData = lipSync.FacialExpressionData;
        Debug.Log("=== AMELIA FACIAL EXPRESSION DATA ===");
        Debug.Log("Amelia Head Renderer: " + (facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL"));
        Debug.Log("Amelia Teeth Renderer: " + (facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL"));
        Debug.Log("Amelia Head WeightBounds: " + facialData.Head.WeightBounds);
        Debug.Log("Amelia Teeth WeightBounds: " + facialData.Teeth.WeightBounds);
        Debug.Log("Amelia Head VisemeEffectorsList: " + (facialData.Head.VisemeEffectorsList != null ? "Assigned" : "NULL"));
        Debug.Log("Amelia Teeth VisemeEffectorsList: " + (facialData.Teeth.VisemeEffectorsList != null ? "Assigned" : "NULL"));
        
        // Check teeth BlendShapes
        if (facialData.Teeth.Renderer != null)
        {
            var mesh = facialData.Teeth.Renderer.sharedMesh;
            if (mesh != null)
            {
                Debug.Log("Amelia Teeth Mesh BlendShape Count: " + mesh.blendShapeCount);
                for (int i = 0; i < Mathf.Min(mesh.blendShapeCount, 10); i++)
                {
                    Debug.Log("  Amelia BlendShape " + i + ": " + mesh.GetBlendShapeName(i));
                }
            }
            else
            {
                Debug.LogError("Amelia Teeth renderer mesh is null!");
            }
        }
        else
        {
            Debug.LogError("Amelia Teeth Renderer is NULL!");
        }
        
        // Check jaw bones
        Debug.Log("Amelia Jaw Bone: " + (facialData.JawBone != null ? facialData.JawBone.name : "NULL"));
        Debug.Log("Amelia Tongue Bone: " + (facialData.TongueBone != null ? facialData.TongueBone.name : "NULL"));
        
        Debug.Log("=== AMELIA TEETH CHECKER FINISHED ===");
    }
} 