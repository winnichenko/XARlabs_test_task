using UnityEngine;

public class ObjectOrient : MonoBehaviour
{
    [Header("References")]
    public Transform objectB;           // Object B
    private Renderer objRenderer;    // Renderer of Object A
    private Material objMaterial;
    [Header("Rotation Settings")]
    public float rotationSpeed = 1f;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        objMaterial = objRenderer.material;
    }

    void Update()
    {
        if (objectB == null || objRenderer == null) return;

        Vector3 directionToB = (objectB.position - transform.position).normalized;
        
        float dot = Vector3.Dot(transform.forward, directionToB);

        // Map dot from [-1, 1] to [0, 1] (Behind is 0, front is 1)
        float t = (dot + 1f) / 2f;
        
        // Apply color to object's material
        objMaterial.SetFloat("_Blend", t);

        //orient Object A towards Object B
        Quaternion tragetRotation = Quaternion.LookRotation(directionToB);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            tragetRotation,
            rotationSpeed*Time.deltaTime
            );
    }
}