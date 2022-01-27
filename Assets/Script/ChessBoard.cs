using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : SquareBoard
{
    public ChessPiece[,] pieces { get; private set; }
    public King[] kings = new King[2];
    public int winner;
    public int checkedPlayer;
    
    public ChessBoard (Transform parent) : base (8, parent)
    {
        pieces = new ChessPiece[n, n];
        SetupBoard();
    }


    #region Movement
    public bool TryMove(ChessPiece piece, int x, int y)
    {
        int enemyPlayer = GetEnemy(piece.player);
        bool isEnemyChecked = false;

        // Castle
        if (TryCastle(piece, x, y))
            isEnemyChecked = IsPlayersChecked()[enemyPlayer];   // have to check if rook causes a check
        // Move
        else if (IsLegalMove(piece, x, y, out isEnemyChecked))
            ForceMovePiece(piece, x, y);
        else
            return false;

        // Check for checkmate
        if (isEnemyChecked)
        {
            if (IsCheckMate(kings[enemyPlayer], piece))
            {
                Debug.Log("Check Mate " + enemyPlayer);
                winner = piece.player;
            }
            checkedPlayer = enemyPlayer;
        }
        else
        {
            checkedPlayer = -1;
        }

        return true;
    }
    public bool TryMove(ChessPiece piece, Vector3 worldPos)
    {
        Vector2Int coord = GetCoord(worldPos);
        return TryMove(piece, coord.x, coord.y);
    }

    public List<Vector2Int> PossibleMoves (ChessPiece piece)
    {
        // COULD BE OPTIMIZED A LOT :)
        List<Vector2Int> moves = new List<Vector2Int>();
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                if (IsLegalMove(piece, x, y)) 
                    moves.Add(new Vector2Int(x, y));

        if (piece.type == ChessPiece.Type.King && !piece.hasMoved)
        {
            if (CanCastle((King)piece, piece.x + 2, piece.y, out _)) moves.Add(new Vector2Int(piece.x + 2, piece.y));
            if (CanCastle((King)piece, piece.x - 2, piece.y, out _)) moves.Add(new Vector2Int(piece.x - 2, piece.y));
        }

        return moves;
    }

    // Rules taken from https://en.wikipedia.org/wiki/Castling
    private bool TryCastle(King king, int x, int y)
    {
        bool kingSide;
        if (!CanCastle(king, x, y, out kingSide)) return false;

        Rook rook = (Rook)GetPieceAt(kingSide ? n - 1 : 0, y);
        
        ForceMovePiece(rook, kingSide ? king.x + 1 : king.x - 1, y);
        ForceMovePiece(king, x, y);

        return true;
    }
    private bool TryCastle(ChessPiece piece, int x, int y) =>
        piece.type == ChessPiece.Type.King ? TryCastle((King)piece, x, y) : false;

    private bool CanCastle(King king, int x, int y, out bool kingSide)
    {
        kingSide = false;
        
        // Castling is performed on the kingside or queenside with the rook on the same rank
        if (king.y != y) return false;

        // Get corner piece
        if (x == king.x + 2)
            kingSide = true;
        else if (x == king.x - 2)
            kingSide = false;
        else
            return false;

        ChessPiece rook = GetPieceAt(kingSide ? n - 1 : 0, y);

        // if it isn't the players rook at corner
        if (rook?.type != ChessPiece.Type.Rook || rook?.player != king.player)
            return false;


        // Neither the king nor the chosen rook has previously moved.
        if (rook.hasMoved || king.hasMoved) return false;

        // There are no pieces between the king and the chosen rook.
        if (IsOtherPiecesBlocking(king, rook.x, rook.y)) return false;

        // The king is not currently in check.
        if (checkedPlayer == king.player) return false;

        // The king does not pass through a square that is attacked by an enemy piece
        List<Vector2Int> coords = GetCoordsInBetween(king, rook);
        // And the king does not end up in check. (True of any legal move.)
        coords.Add(new Vector2Int(x, y));

        for (int i = 0; i < coords.Count; i++)
        {
            Vector2Int coord = coords[i];
            if (IsPlayersCheckedAfterMove(king, coord.x, coord.y)[king.player])
                return false;
        }

        return true;
    }

    private bool IsLegalMove (ChessPiece piece, int x, int y, out bool isEnemyChecked)
    {
        isEnemyChecked = false;

        // If outside of board
        if (IsOutOfBounds(x, y)) return false;

        // If cant place at 
        if (!CanBePlacedAt(piece, x, y)) return false;

        // If other pieces block
        if (IsOtherPiecesBlocking(piece, x, y)) return false;

        bool[] checkStatus = IsPlayersCheckedAfterMove(piece, x, y);

        // Cant place self in check
        if (checkStatus[piece.player]) return false; 

        if (checkStatus[GetEnemy(piece.player)]) isEnemyChecked = true; 

        return true;
    }
    private bool IsLegalMove (ChessPiece piece, int x, int y) => IsLegalMove(piece, x, y, out _);

    private bool CanBePlacedAt (ChessPiece piece, int x, int y)
    {
        int dx = x - piece.x;
        int dy = y - piece.y;

        // If not moving
        if (dx == 0 && dy == 0) return false;

        // If trying to move to a position with a tile that is already owned by the player
        ChessPiece pieceAtPosition = GetPieceAt(x, y);

        // Test castling
        if (pieceAtPosition?.player == piece.player) return false;

        switch (piece.type)
        {
            case ChessPiece.Type.Pawn:
                // Player 0 goes up, player 1 goes down
                int direction = GetForwardDirection(piece.player);

                // if move is not possible
                bool ableToMoveTwo = !piece.hasMoved && pieceAtPosition == null;
                bool movingOne = dy == direction;
                bool movingTwo = dy == direction * 2;
                if (!(ableToMoveTwo && movingTwo || movingOne)) return false;

                // moving
                if (pieceAtPosition == null)
                    return dx == 0;
                // attacking
                else
                    return Mathf.Abs(dx) == 1;

            case ChessPiece.Type.Bishop:
                return Mathf.Abs(dy) == Mathf.Abs(dx);

            case ChessPiece.Type.Knight:
                return
                    (Mathf.Abs(dy) == 2 && Mathf.Abs(dx) == 1) ||
                    (Mathf.Abs(dy) == 1 && Mathf.Abs(dx) == 2);

            case ChessPiece.Type.Rook:
                return dx == 0 || dy == 0;

            case ChessPiece.Type.Queen:
                return dx == 0 || dy == 0 || Mathf.Abs(dy) == Mathf.Abs(dx);

            case ChessPiece.Type.King:
                return (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1);

            default:
                Debug.LogError("ChessPiece not implemented yet!");
                return false;
        }
    }


    private bool IsOtherPiecesBlocking(ChessPiece piece, int x, int y) 
    {
        List<Vector2Int> coords = GetCoordsInBetween(piece.type, piece.x, piece.y, x, y);
        for (int i = 0; i < coords.Count; i++)
            if (GetPieceAt(coords[i].x, coords[i].y) != null) return true;
        return false;
    }

    private void ForceMovePiece (ChessPiece piece, int x, int y)
    {
        pieces[x, y]?.Destroy();

        // Remove from old position
        pieces[piece.x, piece.y] = null;
        // Add to new position
        piece.SetPosition(x, y);
        pieces[piece.x, piece.y] = piece;
    }
    

    private bool[] IsPlayersChecked ()
    {
        bool[] result = { false, false };
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                // For every piece on the map, check if they can hit the king
                ChessPiece checkingPiece = GetPieceAt(x, y);
                if (checkingPiece == null) continue;

                int enemy = GetEnemy(checkingPiece.player);
                if (result[enemy]) continue;

                King enemyKing = kings[enemy];

                if (!CanBePlacedAt(checkingPiece, enemyKing.x, enemyKing.y)) continue;
                if (IsOtherPiecesBlocking(checkingPiece, enemyKing.x, enemyKing.y)) continue;

                result[enemy] = true;
            }
        }
        return result;
    }
    public bool[] IsPlayersCheckedAfterMove (ChessPiece piece, int toX, int toY)
    {
        // Change
        int fromX = piece.x;
        int fromY = piece.y;

        ChessPiece target = pieces[toX, toY];

        pieces[fromX, fromY] = null;
        pieces[toX, toY] = piece;
        
        piece.SetPosition(toX, toY, false);

        // Get check
        bool[] result = IsPlayersChecked();

        // Revert
        pieces[fromX, fromY] = piece;
        pieces[toX, toY] = target;
        piece.SetPosition(fromX, fromY, false);

        return result;
    }

    #region CheckMate
    private bool KingHasLegalMove (ChessPiece checkedKing)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;
                if (IsLegalMove(checkedKing, checkedKing.x + x, checkedKing.y + y)) return true;
            }
        }
        return false;
    }
    private bool ThreatCanBeKilledOrBlocked (ChessPiece checkedKing, ChessPiece threat)
    {
        List<Vector2Int> coordsBetween = GetCoordsInBetween(threat, checkedKing);
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                ChessPiece savior = GetPieceAt(x, y);
                if (savior == null) continue;
                // Skip if savior is the same color as the threat
                if (savior.player == threat.player) continue;

                // For all pieces, check if it can kill the threat
                if (IsLegalMove(savior, threat.x, threat.y)) return true;

                // Check if savior can block the threat
                for (int i = 0; i < coordsBetween.Count; i++)
                {
                    Vector2Int coord = coordsBetween[i];
                    if (IsLegalMove(savior, coord.x, coord.y)) return true;
                }

            }
        }
        return false;
    }

    public bool IsCheckMate (ChessPiece checkedKing, ChessPiece threat)
    {
        // Can move king and not be in check?
        if (KingHasLegalMove(checkedKing)) return false;

        // Can kill or block the threat
        if (ThreatCanBeKilledOrBlocked(checkedKing, threat)) return false;

        return true;
    }
    #endregion

    private List<Vector2Int> GetCoordsInBetween(ChessPiece.Type attackerType, int fromX, int fromY, int toX, int toY)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        // Knight (doesnt care)
        if (attackerType == ChessPiece.Type.Knight) 
                return result;

        // Get direction
        int incX = toX - fromX;
        int incY = toY - fromY;
        incX = incX != 0 ? (int)Mathf.Sign(incX) : 0;
        incY = incY != 0 ? (int)Mathf.Sign(incY) : 0;

        // Core
        int x = fromX + incX;
        int y = fromY + incY;
        while (x != toX || y != toY)
        {
            result.Add(new Vector2Int(x, y));

            x += incX;
            y += incY;
        }

        return result;
    }
    private List<Vector2Int> GetCoordsInBetween(ChessPiece attacker, ChessPiece target) =>
        GetCoordsInBetween(attacker.type, attacker.x, attacker.y, target.x, target.y);

    #endregion

    #region Spawn
    public void ResetBoard()
    {
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                pieces[x, y]?.Destroy();
                pieces[x, y] = null;
            }
        }

        SetupBoard();
    }

    private void SetupBoard ()
    {
        winner = -1;
        checkedPlayer = -1;

        // Player 0
        SpawnRowOfPawns(0, 1);
        SpawnBackLine(0, 0);

        // Player 1
        SpawnRowOfPawns(1, n - 2);
        SpawnBackLine(1, n - 1);
    }
    private void SpawnBackLine(int player, int y)
    {
        SpawnChessPiece(new Rook(this, player, 0, y));
        SpawnChessPiece(new Rook(this, player, n-1, y));
        
        SpawnChessPiece(new Knight(this, player, 1, y));
        SpawnChessPiece(new Knight(this, player, n-2, y));
        
        SpawnChessPiece(new Bishop(this, player, 2, y));
        SpawnChessPiece(new Bishop(this, player, n - 3, y));

        SpawnChessPiece(new Queen(this, player, 3, y));

        kings[player] = new King(this, player, 4, y);
        SpawnChessPiece(kings[player]);

    }
    private void SpawnRowOfPawns (int player, int y)
    {
        for (int x = 0; x < n; x++)
            SpawnChessPiece(new Pawn(this, player, x, y));
    }
    private void SpawnChessPiece (ChessPiece piece)
    {
        pieces[piece.x, piece.y] = piece;
    }
    #endregion

    public bool IsOutOfBounds(int x, int y) => (x < 0 || x >= n || y < 0 || y >= n);
    public Vector2Int GetCoord(Vector3 worldPos) => new Vector2Int((int)worldPos.x, (int)worldPos.z);
    public int GetEnemy(int player) => player == 0 ? 1 : 0;
    public int GetForwardDirection(int player) => player == 0 ? 1 : -1;
    public bool CanBePromoted(ChessPiece piece) => piece.type == ChessPiece.Type.Pawn && piece.y == (piece.player == 0 ? n - 1 : 0);

    public ChessPiece GetPieceAt(int x, int y)
    {
        if (IsOutOfBounds(x, y)) return null;
        return pieces[x, y];
    }
    public ChessPiece GetPieceAt(Vector3 worldPos)
    {
        Vector2Int coord = GetCoord(worldPos);

        return GetPieceAt(coord.x, coord.y);
    }
}
