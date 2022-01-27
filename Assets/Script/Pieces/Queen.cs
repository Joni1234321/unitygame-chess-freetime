using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    private static Material queenMaterial;
    protected override Material material
    {
        get
        {
            if (queenMaterial == null) queenMaterial = Resources.Load<Material>("Materials/Queen") as Material;
            return queenMaterial;
        }
    }

    public Queen(ChessBoard board, int player, int x, int y) : base(Type.Queen, board, player, x, y)
    {

    }
}
