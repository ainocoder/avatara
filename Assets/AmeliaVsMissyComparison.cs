using UnityEngine;
using Convai.Scripts.Runtime.Features.LipSync;
using Convai.Scripts.Runtime.Features.LipSync.Models;
using Convai.Scripts.Runtime.Features.LipSync.Visemes;

// Fixed compilation errors by using correct Convai types
public class AmeliaVsMissyComparison : MonoBehaviour
{
    [Header("Characters to Compare")]
    public GameObject ameliaCharacter;
    public GameObject missy1Character;
    
    [Header("Analysis Results")]
    [TextArea(10, 20)]
    public string analysisReport = "";
    
    void Start()
    {
        CompareCharacters();
    }
    
    void CompareCharacters()
    {
        string report = "=== Amelia vs Missy1 Lip Sync Comparison ===\n\n";
        
        // Find Amelia components
        ConvaiLipSync ameliaLipSync = ameliaCharacter?.GetComponent<ConvaiLipSync>();
        SkinnedMeshRenderer[] ameliaRenderers = ameliaCharacter?.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        // Find Missy1 components
        ConvaiLipSync missy1LipSync = missy1Character?.GetComponent<ConvaiLipSync>();
        SkinnedMeshRenderer[] missy1Renderers = missy1Character?.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        if (ameliaLipSync == null)
        {
            report += "❌ ERROR: Amelia ConvaiLipSync component not found!\n";
            analysisReport = report;
            return;
        }
        
        if (missy1LipSync == null)
        {
            report += "❌ ERROR: Missy1 ConvaiLipSync component not found!\n";
            analysisReport = report;
            return;
        }
        
        // Compare Head settings
        report += "=== HEAD COMPARISON ===\n";
        report += CompareRendererData("Head", ameliaLipSync.FacialExpressionData.Head, missy1LipSync.FacialExpressionData.Head);
        
        // Compare Teeth settings
        report += "\n=== TEETH COMPARISON ===\n";
        report += CompareRendererData("Teeth", ameliaLipSync.FacialExpressionData.Teeth, missy1LipSync.FacialExpressionData.Teeth);
        
        // Compare Tongue settings
        report += "\n=== TONGUE COMPARISON ===\n";
        report += CompareRendererData("Tongue", ameliaLipSync.FacialExpressionData.Tongue, missy1LipSync.FacialExpressionData.Tongue);
        
        // List all available renderers
        report += "\n=== AVAILABLE RENDERERS ===\n";
        report += "Amelia Renderers:\n";
        if (ameliaRenderers != null)
        {
            foreach (var renderer in ameliaRenderers)
            {
                report += $"  - {renderer.name} ({renderer.sharedMesh?.blendShapeCount ?? 0} BlendShapes)\n";
            }
        }
        
        report += "\nMissy1 Renderers:\n";
        if (missy1Renderers != null)
        {
            foreach (var renderer in missy1Renderers)
            {
                report += $"  - {renderer.name} ({renderer.sharedMesh?.blendShapeCount ?? 0} BlendShapes)\n";
            }
        }
        
        // Auto-fix suggestion
        report += "\n=== AUTO-FIX SUGGESTIONS ===\n";
        report += GenerateFixSuggestions(ameliaLipSync.FacialExpressionData, missy1LipSync.FacialExpressionData, missy1Renderers);
        
        analysisReport = report;
        Debug.Log("Analysis Complete - Check the analysisReport field in the inspector");
        
        // Auto-apply fix if suggested
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ApplyAutoFix(missy1LipSync.FacialExpressionData, missy1Renderers);
        }
    }
    
    string CompareRendererData(string partName, SkinMeshRendererData ameliaData, SkinMeshRendererData missy1Data)
    {
        string result = $"{partName}:\n";
        
        // Compare renderers
        string ameliaRenderer = ameliaData.Renderer != null ? ameliaData.Renderer.name : "NULL";
        string missy1Renderer = missy1Data.Renderer != null ? missy1Data.Renderer.name : "NULL";
        
        result += $"  Amelia {partName} Renderer: {ameliaRenderer}\n";
        result += $"  Missy1 {partName} Renderer: {missy1Renderer}\n";
        
        if (ameliaRenderer != missy1Renderer)
        {
            result += $"  ⚠️ DIFFERENCE: Renderer names don't match!\n";
        }
        
        // Compare BlendShape counts
        int ameliaBlendShapes = ameliaData.Renderer?.sharedMesh?.blendShapeCount ?? 0;
        int missy1BlendShapes = missy1Data.Renderer?.sharedMesh?.blendShapeCount ?? 0;
        
        result += $"  Amelia {partName} BlendShapes: {ameliaBlendShapes}\n";
        result += $"  Missy1 {partName} BlendShapes: {missy1BlendShapes}\n";
        
        if (ameliaBlendShapes != missy1BlendShapes)
        {
            result += $"  ⚠️ DIFFERENCE: BlendShape counts don't match!\n";
        }
        
        // Compare Weight Bounds
        result += $"  Amelia {partName} Weight Bounds: {ameliaData.WeightBounds}\n";
        result += $"  Missy1 {partName} Weight Bounds: {missy1Data.WeightBounds}\n";
        
        // Compare VisemeEffectorsList
        bool ameliaHasEffectors = ameliaData.VisemeEffectorsList != null;
        bool missy1HasEffectors = missy1Data.VisemeEffectorsList != null;
        
        result += $"  Amelia {partName} Has Effectors: {ameliaHasEffectors}\n";
        result += $"  Missy1 {partName} Has Effectors: {missy1HasEffectors}\n";
        
        if (ameliaHasEffectors != missy1HasEffectors)
        {
            result += $"  ⚠️ DIFFERENCE: Effector availability doesn't match!\n";
        }
        
        return result;
    }
    
    string GenerateFixSuggestions(FacialExpressionData ameliaData, FacialExpressionData missy1Data, SkinnedMeshRenderer[] missy1Renderers)
    {
        string suggestions = "";
        
        // Find CC_Base_Body renderer for Missy1
        SkinnedMeshRenderer missy1Body = null;
        foreach (var renderer in missy1Renderers)
        {
            if (renderer.name.Contains("CC_Base_Body"))
            {
                missy1Body = renderer;
                break;
            }
        }
        
        if (missy1Body == null)
        {
            suggestions += "❌ Could not find CC_Base_Body renderer for Missy1\n";
            return suggestions;
        }
        
        suggestions += $"✅ Found CC_Base_Body with {missy1Body.sharedMesh.blendShapeCount} BlendShapes\n";
        
        // Check if Missy1's teeth renderer is problematic
        if (missy1Data.Teeth.Renderer != null && missy1Data.Teeth.Renderer.sharedMesh.blendShapeCount == 0)
        {
            suggestions += "⚠️ Missy1's teeth renderer has 0 BlendShapes (this is the problem!)\n";
            suggestions += "💡 SUGGESTION: Set Missy1's teeth renderer to CC_Base_Body\n";
        }
        
        // Check for head/teeth conflict
        if (missy1Data.Head.Renderer == missy1Data.Teeth.Renderer && missy1Data.Head.Renderer != null)
        {
            suggestions += "⚠️ Head and Teeth use the same renderer (this causes conflicts!)\n";
            suggestions += "💡 SUGGESTION: Use different renderers or disable one\n";
        }
        
        suggestions += "\n🔧 HOLD LEFT SHIFT and run this script to auto-apply fixes!\n";
        
        return suggestions;
    }
    
    void ApplyAutoFix(FacialExpressionData missy1Data, SkinnedMeshRenderer[] missy1Renderers)
    {
        Debug.Log("Applying auto-fix...");
        
        // Find CC_Base_Body renderer
        SkinnedMeshRenderer missy1Body = null;
        foreach (var renderer in missy1Renderers)
        {
            if (renderer.name.Contains("CC_Base_Body"))
            {
                missy1Body = renderer;
                break;
            }
        }
        
        if (missy1Body == null)
        {
            Debug.LogError("Could not find CC_Base_Body for auto-fix!");
            return;
        }
        
        // Fix teeth renderer if it has 0 BlendShapes
        if (missy1Data.Teeth.Renderer != null && missy1Data.Teeth.Renderer.sharedMesh.blendShapeCount == 0)
        {
            Debug.Log("Fixing teeth renderer...");
            missy1Data.Teeth.Renderer = missy1Body;
            missy1Data.Teeth.WeightBounds = new Vector2(0, 100);
        }
        
        // Ensure head uses CC_Base_Body
        if (missy1Data.Head.Renderer == null || missy1Data.Head.Renderer.sharedMesh.blendShapeCount < 100)
        {
            Debug.Log("Fixing head renderer...");
            missy1Data.Head.Renderer = missy1Body;
            missy1Data.Head.WeightBounds = new Vector2(0, 100);
        }
        
        Debug.Log("Auto-fix complete! Run comparison again to verify.");
        
        // Re-run comparison
        Invoke("CompareCharacters", 1f);
    }
} 