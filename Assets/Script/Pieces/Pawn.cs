using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    private static Material pawnMaterial;
    protected override Material material 
    { 
        get { 
            if (pawnMaterial == null) pawnMaterial = Resources.Load<Material>("Materials/Pawn") as Material;
            return pawnMaterial; 
        } 
    }

    public Pawn(ChessBoard board, int player, int x, int y) : base(Type.Pawn, board, player, x, y)
    {
        
    }
    
}
