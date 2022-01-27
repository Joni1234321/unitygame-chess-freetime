using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    private static Material rookMaterial;
    protected override Material material
    {
        get
        {
            if (rookMaterial == null) rookMaterial = Resources.Load<Material>("Materials/Rook") as Material;
            return rookMaterial;
        }
    }

    public Rook(ChessBoard board, int player, int x, int y) : base(Type.Rook, board, player, x, y)
    {

    }
}
