using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SquareBoard
{
    // Logic
    public int n { get; private set; }

    public SquareBoard (int n, Transform parent)
    {
        CreateGameObject(parent);
        SetSize(n);
    }

    public bool SetSize(int n)
    {
        if (n < 1) return false;
        if (this.n != n)
        {
            this.n = n;
            meshFilter.mesh = GenerateMesh();
        }

        return true;
    }

    #region Draw
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    public GameObject gameObject { get; private set; }

    Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] verts = {
            new Vector3(0, 0, 0),
            new Vector3(n, 0, 0),
            new Vector3(n, 0, n),
            new Vector3(0, 0, n)
        };
        Vector3[] normals =
        {
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0)
        };

        Vector2[] uvs = {
            new Vector2(0,0),
            new Vector2(n * .5f ,0),
            new Vector2(n * .5f, n * .5f),
            new Vector2(0, n * .5f)
        };

        int[] tris = {
            0, 2, 1,
            0, 3, 2
        };

        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();

        return mesh;
    }

    void CreateGameObject(Transform parent)
    {
        gameObject = new GameObject("Chessboard");
        gameObject.transform.parent = parent;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.material = Controller.BoardMaterial;
    }
    #endregion

}
