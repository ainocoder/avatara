using UnityEngine;

public class SimpleTeethChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== SIMPLE TEETH CHECKER STARTED ===");
        
        // Find missyT
        GameObject missyT = GameObject.Find("missyT");
        if (missyT == null)
        {
            Debug.LogError("missyT not found!");
            return;
        }
        
        Debug.Log("Found missyT: " + missyT.name);
        
        // Check ConvaiLipSync component
        var lipSync = missyT.GetComponent<Convai.Scripts.Runtime.Features.LipSync.ConvaiLipSync>();
        if (lipSync == null)
        {
            Debug.LogError("ConvaiLipSync not found on missyT!");
            return;
        }
        
        Debug.Log("ConvaiLipSync found!");
        
        // Check facial expression data
        var facialData = lipSync.FacialExpressionData;
        Debug.Log("Head Renderer: " + (facialData.Head.Renderer != null ? facialData.Head.Renderer.name : "NULL"));
        Debug.Log("Teeth Renderer: " + (facialData.Teeth.Renderer != null ? facialData.Teeth.Renderer.name : "NULL"));
        Debug.Log("Head WeightBounds: " + facialData.Head.WeightBounds);
        Debug.Log("Teeth WeightBounds: " + facialData.Teeth.WeightBounds);
        
        if (facialData.Teeth.Renderer == null)
        {
            Debug.LogError("PROBLEM FOUND: Teeth Renderer is NULL!");
            Debug.Log("This is why teeth don't move during lip sync!");
        }
        else
        {
            Debug.Log("Teeth Renderer is assigned: " + facialData.Teeth.Renderer.name);
        }
        
        Debug.Log("=== SIMPLE TEETH CHECKER FINISHED ===");
    }
} 