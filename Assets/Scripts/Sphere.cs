using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralSphereWithCone : MonoBehaviour
{
    [Range(0, 5)]
    public int subdivisions = 3;
    public float radius = 1f;
    public Material material;

    public float coneHight = 0.5f;
    public float coneRadius = 0.2f;
    public int coneResolution =20;

    private void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Mesh sphereMesh = GenerateIcosphere(radius, subdivisions);
        Mesh coneMesh = GenerateCone(coneHight,coneRadius,coneResolution,radius);

        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = sphereMesh;
        combine[0].transform = Matrix4x4.identity;

        combine[1].mesh = coneMesh;
        combine[1].transform = Matrix4x4.identity;

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine, true, false);

        meshFilter.mesh = finalMesh;
        meshRenderer.material = material != null ? material : new Material(Shader.Find("Standard"));
    }

    private Mesh GenerateCone(float height, float bottomRadius, int resolution, float sphereRadius)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(new Vector3(0f, 0f, sphereRadius + height));

        float angleStep = 2*Mathf.PI/resolution;

        for(int i=0; i<=resolution; i++)
        {
            float angle = i*angleStep;
            float x = bottomRadius*Mathf.Cos(angle);
            float y = bottomRadius*Mathf.Sin(angle);
            vertices.Add(new Vector3(x, y, sphereRadius - 0.2f));
            if (i<resolution)
            {
                triangles.Add(0);
                triangles.Add(i+2);
                triangles.Add(i+1);
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh GenerateIcosphere(float radius, int subdivisions)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Icosphere";

        List<Vector3> vertices = new List<Vector3>();
        Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
        List<TriangleIndices> faces = new List<TriangleIndices>();

        // Create 12 vertices of a icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        vertices.Add(new Vector3(-1,  t,  0).normalized * radius);
        vertices.Add(new Vector3( 1,  t,  0).normalized * radius);
        vertices.Add(new Vector3(-1, -t,  0).normalized * radius);
        vertices.Add(new Vector3( 1, -t,  0).normalized * radius);

        vertices.Add(new Vector3( 0, -1,  t).normalized * radius);
        vertices.Add(new Vector3( 0,  1,  t).normalized * radius);
        vertices.Add(new Vector3( 0, -1, -t).normalized * radius);
        vertices.Add(new Vector3( 0,  1, -t).normalized * radius);

        vertices.Add(new Vector3( t,  0, -1).normalized * radius);
        vertices.Add(new Vector3( t,  0,  1).normalized * radius);
        vertices.Add(new Vector3(-t,  0, -1).normalized * radius);
        vertices.Add(new Vector3(-t,  0,  1).normalized * radius);

        // Create 20 triangles of the icosahedron
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));

        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));

        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));

        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));

        // Refine triangles
        for (int i = 0; i < subdivisions; i++)
        {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (var tri in faces)
            {
                int a = GetMiddlePoint(tri.v1, tri.v2, ref vertices, ref middlePointIndexCache, radius);
                int b = GetMiddlePoint(tri.v2, tri.v3, ref vertices, ref middlePointIndexCache, radius);
                int c = GetMiddlePoint(tri.v3, tri.v1, ref vertices, ref middlePointIndexCache, radius);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }
            faces = faces2;
        }

        List<int> triangles = new List<int>();
        foreach (var tri in faces)
        {
            triangles.Add(tri.v1);
            triangles.Add(tri.v2);
            triangles.Add(tri.v3);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices,
                               ref Dictionary<long, int> cache, float radius)
    {
        // First check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        // Not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = ((point1 + point2) / 2f).normalized * radius;

        int i = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, i);
        return i;
    }
}
