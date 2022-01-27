using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    private static Material knightMaterial;
    protected override Material material
    {
        get
        {
            if (knightMaterial == null) knightMaterial = Resources.Load<Material>("Materials/Knight") as Material;
            return knightMaterial;
        }
    }

    public Knight(ChessBoard board, int player, int x, int y) : base(Type.Knight, board, player, x, y)
    {

    }
}
