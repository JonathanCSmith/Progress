using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PlanetMeshGenerator : MonoBehaviour
{

    public int planetTilesInX = 50;
    public int planetTilesInY = 50;
    public int tileSize = 50;


    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;

        StartCoroutine(CreateShape());
    }

    IEnumerator CreateShape()
    {
        vertices = new Vector3[(tileSize + 1) * (tileSize + 1)];
        int index = 0;
        for (int z = 0; z <= tileSize; z++)
        {
            for (int x = 0; x <= tileSize; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 2.0f;
                vertices[index] = new Vector3(x, y, z);
                index++;
            }
        }

        triangles = new int[tileSize * tileSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < tileSize; z++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + tileSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + tileSize + 1;
                triangles[tris + 5] = vert + tileSize + 2;

                vert++;
                tris += 6;

                yield return new WaitForSeconds(0.1f);
            }

            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
    }
}
