using UnityEngine;

public class SimpleLipSyncTester : MonoBehaviour
{
    [Header("Test Values")]
    [Range(0f, 100f)]
    public float V_Open_Test = 0f;
    
    [Range(0f, 100f)]
    public float V_Lip_Open_Test = 0f;
    
    [Range(0f, 100f)]
    public float Jaw_Open_Test = 0f;
    
    [Range(0f, 100f)]
    public float Mouth_Drop_Upper_Test = 0f;
    
    [Range(0f, 100f)]
    public float Mouth_Drop_Lower_Test = 0f;
    
    private SkinnedMeshRenderer targetRenderer;
    
    private void Start()
    {
        Transform ccBody = transform.Find("CC_Base_Body");
        if (ccBody != null)
        {
            targetRenderer = ccBody.GetComponent<SkinnedMeshRenderer>();
            Debug.Log("Target renderer found: " + ccBody.name);
        }
    }
    
    private void Update()
    {
        if (targetRenderer == null) return;
        
        targetRenderer.SetBlendShapeWeight(0, V_Open_Test);
        targetRenderer.SetBlendShapeWeight(7, V_Lip_Open_Test);
        targetRenderer.SetBlendShapeWeight(127, Jaw_Open_Test);
        targetRenderer.SetBlendShapeWeight(116, Mouth_Drop_Upper_Test);
        targetRenderer.SetBlendShapeWeight(117, Mouth_Drop_Lower_Test);
    }
}