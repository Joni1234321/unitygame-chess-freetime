using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    private static Material kingMaterial;
    protected override Material material
    {
        get
        {
            if (kingMaterial == null) kingMaterial = Resources.Load<Material>("Materials/King") as Material;
            return kingMaterial;
        }
    }

    public King(ChessBoard board, int player, int x, int y) : base(Type.King, board, player, x, y)
    {

    }
}
