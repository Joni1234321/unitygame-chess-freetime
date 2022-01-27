using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    private static Material bishopMaterial;
    protected override Material material
    {
        get
        {
            if (bishopMaterial == null) bishopMaterial = Resources.Load<Material>("Materials/Bishop") as Material;
            return bishopMaterial;
        }
    }

    public Bishop(ChessBoard board, int player, int x, int y) : base(Type.Bishop, board, player, x, y)
    {

    }
}
