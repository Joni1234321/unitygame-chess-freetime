using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess 
{
    public bool isRunning;
    public int playerTurn;
    public ChessBoard board;
    

    private ChessPiece _crntPce;
    public ChessPiece currentPiece { 
        get { return _crntPce; } 
        set { _crntPce?.ResetVisualPosition(); ResetMoveShadows(); if (value != null) ShowMoveShadows(value); _crntPce = value; } 
    }

    private readonly GameObject shadowParent;
    private List<GameObject> moveShadows = new List<GameObject>();

    public Pawn promotingPiece;

    public delegate void GameStateAction();
    public static event GameStateAction OnNextTurn, OnGameOver;
    public delegate void ChessPieceAction();
    public static event ChessPieceAction OnPawnPromotion;


    public Chess()
    {
        gameObject = new GameObject("Chess controller");
        board = new ChessBoard(gameObject.transform);

        shadowParent = new GameObject("Shadows"); 
        shadowParent.transform.parent = gameObject.transform;
        

        playerTurn = 0;
        isRunning = true;
    }

    
    private void ShowMoveShadows (ChessPiece piece)
    {
        List<Vector2Int> moves = board.PossibleMoves(piece);

        for (int i = 0; i < moves.Count; i++)
        {
            GameObject shadow = Object.Instantiate(Controller.ShadowPrefab, shadowParent.transform);
            shadow.transform.position = new Vector3(moves[i].x + .5f, 0, moves[i].y + .5f);
            moveShadows.Add(shadow);
        }
    }

    private void ResetMoveShadows()
    {
        int len = moveShadows.Count;
        for (int i = len - 1; i >= 0; i--)
        {
            Object.Destroy(moveShadows[i]);
            moveShadows.RemoveAt(i);
        }
    }
    public void Restart()
    {
        currentPiece = null;
        playerTurn = 0;
        isRunning = true;
        board.ResetBoard();
    }

    private void NextTurn ()
    {
        // Toggle
        playerTurn = playerTurn == 0 ? 1 : 0;

        // Deselect piece
        currentPiece = null;

        if (board.winner != -1 && OnGameOver != null) OnGameOver();

        if (OnNextTurn != null) OnNextTurn();

    }

    private void PromotePawn (Pawn pawn)
    {
        isRunning = false;
        promotingPiece = pawn;

        if (OnPawnPromotion != null) OnPawnPromotion();
    }

    public bool TryPromotePawnTo(ChessPiece pawn, ChessPiece.Type promotion)
    {
        // INVALID PROMOTIONS
        if (promotion == ChessPiece.Type.King || promotion == ChessPiece.Type.Pawn) return false;
        ChessPiece newPiece;
        switch (promotion)
        {
            case ChessPiece.Type.Queen:
                newPiece = new Queen(board, pawn.player, pawn.x, pawn.y);
                break;
            case ChessPiece.Type.Bishop:
                newPiece = new Bishop(board, pawn.player, pawn.x, pawn.y);
                break;
            case ChessPiece.Type.Knight:
                newPiece = new Knight(board, pawn.player, pawn.x, pawn.y);
                break;
            case ChessPiece.Type.Rook:
                newPiece = new Rook(board, pawn.player, pawn.x, pawn.y);
                break;
            default:
                return false;
        }
        pawn.Destroy();

        newPiece.SetHasMoved();
        board.pieces[pawn.x, pawn.y] = newPiece;

        isRunning = true;

        // Check for new checkmate
        if (board.IsCheckMate(board.kings[board.GetEnemy(newPiece.player)], newPiece))
            board.winner = newPiece.player;
        
        NextTurn();
        return true;
    }

    #region OnClick
    public void LeftClickDown (Vector3 worldPos)
    {
        ChessPiece clickedPiece = board.GetPieceAt(worldPos);
        if (clickedPiece?.player == playerTurn)
            currentPiece = clickedPiece;
            
    }
    public void LeftClickUp(Vector3 worldPos) => RightClickDown(worldPos);
    public void LeftClick(Vector3 worldPos) => currentPiece?.SetVisualPositionOnly(worldPos);
    public void RightClickDown (Vector3 worldPos)
    {
        // No piece selected
        if (currentPiece == null) return;

        currentPiece.ResetVisualPosition();
        
        if (board.TryMove(currentPiece, worldPos))
        {
            // Promote before next turn
            if (board.CanBePromoted(currentPiece))
                PromotePawn((Pawn)currentPiece);
            else 
                NextTurn();
        }
    }
    #endregion

    public GameObject gameObject;
}
