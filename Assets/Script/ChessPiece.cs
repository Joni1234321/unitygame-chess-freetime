using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece
{
    public static Color[] PlayerColors = { Color.white, new Color(1f, .6f, .2f)};
    // int * int * (int * int)
    public enum Type
    {
        Pawn = 0,
        Rook = 1,
        Bishop = 2,
        Knight = 5,
        Queen = 10,
        King = 100
    }
    public readonly Type type;
    public readonly int player; 
    public int x { get; protected set; }
    public int y { get; protected set; }
    public bool hasMoved { get; protected set; }


    public void SetVisualPositionOnly(float x, float y)
    {
        if (gameObject == null) return;
        gameObject.transform.position = new Vector3(x, 0, y);
    }
    public void SetVisualPositionOnly(Vector3 worldPos)
    {
        if (gameObject == null) return;
        gameObject.transform.position = new Vector3(worldPos.x - .5f, .1f, worldPos.z - .5f);
    }
    public void ResetVisualPosition() => SetVisualPositionOnly(x, y);

    public void SetPosition (int x, int y)
    {
        SetPosition(x, y, true);
    }

    public void SetPosition (int x, int y, bool updateGameObject)
    {
        this.x = x;
        this.y = y;

        if (updateGameObject)
        {
            SetVisualPositionOnly(x, y);
            hasMoved = true;
        }
    }
    public bool SetHasMoved() => hasMoved = true;

    // Constructor and Destructor
    public ChessPiece(Type type, ChessBoard board, int player, int x, int y)
    {
        this.player = player;
        this.type = type;

        CreateGameObject(board.gameObject.transform);

        SetPosition(x, y);

        hasMoved = false;
    }
    public void Destroy ()
    {
        if (gameObject != null)
            Object.Destroy(gameObject);
        
    }

    public override string ToString()
    {
        return "[" + player + "] - " + type.ToString() + " (" +  x +  "," + y + ")";
    }
    #region Draw
    private GameObject gameObject { get;  set; }
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    protected abstract Material material { get; }

    Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        Vector3[] verts = {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1)
        };
        Vector3[] normals =
        {
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0)
        };

        Vector2[] uvs = {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
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
    

    private void CreateGameObject(Transform parent)
    {
        gameObject = new GameObject("[" + player + "] - " + type.ToString());
        gameObject.transform.parent = parent;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.mesh = GenerateQuad();
        meshRenderer.material = material;
        meshRenderer.material.color = PlayerColors[player];
    }

    
    #endregion
}
