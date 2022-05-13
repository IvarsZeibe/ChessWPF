using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace ChessWPF
{
    // team pieceType
    // 0    000
    internal enum Piece : int
    {
        Empty   = 0b000,
        Pawn    = 0b001,
        Bishop  = 0b010,
        Knight  = 0b011,
        Queen   = 0b100,
        King    = 0b101,
        Rook    = 0b110
    }
    internal enum GameStatus
    {
        WhiteWins,
        BlackWins,
        Stalemate,
        InProgress
    }
    internal class ChessBoard
    {
        public int Height { get; } = 8;
        public int Width { get; } = 8;
        public bool IsWhiteTurn { get; private set; } = true;
        private int[,] _board = new int[8,8];
        private Dictionary<(int x, int y), List<(int x, int y)>> _cachedValidMoves = new();
        public ChessBoard()
        {
            ResetGame();
        }
        public void ResetGame()
        {
            _board = new int[,]
            {
                { 0b01110, 0b01011, 0b01010, 0b01101, 0b01100, 0b01010, 0b01011, 0b01110 },
                { 0b01001, 0b01001, 0b01001, 0b01001, 0b01001, 0b01001, 0b01001, 0b01001 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0b00001, 0b00001, 0b00001, 0b00001, 0b00001, 0b00001, 0b00001, 0b00001 },
                { 0b00110, 0b00011, 0b00010, 0b00101, 0b00100, 0b00010, 0b00011, 0b00110 }
            };
            IsWhiteTurn = true;
        }
        public void Load(string fen)
        {
            int y = 0;
            int x = 0;
            foreach (char l in fen)
            {

                int teamFlag = Char.IsUpper(l) ? 0 : 0b1000;
                switch (Char.ToLower(l))
                {
                    case 'p':
                        _board[y, x] = (int)Piece.Pawn + teamFlag;
                        x++;
                        break;
                    case 'r':
                        _board[y, x] = (int)Piece.Rook + teamFlag;
                        x++;
                        break;
                    case 'n':
                        _board[y, x] = (int)Piece.Knight + teamFlag;
                        x++;
                        break;
                    case 'b':
                        _board[y, x] = (int)Piece.Bishop + teamFlag;
                        x++;
                        break;
                    case 'q':
                        _board[y, x] = (int)Piece.Queen + teamFlag;
                        x++;
                        break;
                    case 'k':
                        _board[y, x] = (int)Piece.King + teamFlag;
                        x++;
                        break;
                    case '/':
                        y++;
                        x = 0;
                        break;
                    default:
                        for(int i = 0; i < l - '0'; i++)
                        {
                            _board[y, x] = 0;
                            x++;
                        }
                        break;
                }
            }
            IsWhiteTurn = true;
        }
        public GameStatus CalculateGameStatus()
        {
            if (!HasMovesLeftFor(IsWhiteTurn))
            {
                if (IsCheckFor(IsWhiteTurn))
                    return IsWhiteTurn ? GameStatus.BlackWins : GameStatus.WhiteWins;
                else
                    return GameStatus.Stalemate;
            }
            else
                return GameStatus.InProgress;
        }
        public bool TryGetPiece(int x, int y, out int pieceType, out bool isWhite)
        {
            pieceType = 0;
            isWhite = true;
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            int piece = _board[y, x];
            isWhite = (piece & 0b01000) != 0b01000;
            pieceType = piece & 0b00111;
            return true;
        }
        public bool TryMove(int x1, int y1, int x2, int y2)
        {
            List<(int x, int y)> validMoves = _cachedValidMoves.ContainsKey((x1, y1))
                ? _cachedValidMoves[(x1, y1)]
                : GetValidMoves(x1, y1);
            _cachedValidMoves.Clear();

            if (!validMoves.Contains((x2, y2))) return false;

            _board[y2, x2] = _board[y1, x1];
            _board[y1, x1] = 0;
            IsWhiteTurn = !IsWhiteTurn;
            return true;
        }
        private bool IsEmptyField(int x, int y)
        {
            return TryGetPiece(x, y, out int targetPieceType, out _) && targetPieceType == (int)Piece.Empty;
        }
        private bool IsEnemy(int x, int y, bool isWhite)
        {
            return TryGetPiece(x, y, out int targetType, out bool isTargetWhite) && isTargetWhite != isWhite && targetType != (int)Piece.Empty;
        }
        private bool IsEnemyOrEmpty(int x, int y, bool isWhite)
        {
            return TryGetPiece(x, y, out int targetType, out bool isTargetWhite) && (targetType == (int)Piece.Empty || isTargetWhite != isWhite);
        }
        public List<(int x, int y)> GetValidMoves(int x, int y, bool checkForCheckMate = true)
        {
            return GetValidMoves(x, y, out _, checkForCheckMate);
        }
        public List<(int x, int y)> GetValidMoves(int x, int y, out List<(int x, int y)> kingDeadMoves, bool checkForCheckMate = true)
        {
            List<(int x, int y)> validMoves = new();
            TryGetPiece(x, y, out int pieceType, out bool isWhite);
            switch (pieceType)
            {
                case (int)Piece.Pawn:
                    int verticalDirection = isWhite ? -1 : 1;
                    if ((isWhite && y == 6) || (!isWhite && y == 1))
                    {
                        (int x, int y) move = (x, y + 2 * verticalDirection);
                        if (TryGetPiece(move.x, move.y, out int targetPieceType, out _) && targetPieceType == (int)Piece.Empty)
                        {
                            validMoves.Add(move);
                        };
                    }

                    for (int i = -1; i < 2; i++)
                    {

                        (int x, int y) move = (x + i, y + verticalDirection);
                        if (i == 0)
                        {
                            if (IsEmptyField(move.x, move.y))
                            {
                                validMoves.Add(move);
                            }
                        }
                        else
                        {
                            if (IsEnemy(move.x, move.y, isWhite))
                            {
                                validMoves.Add(move);
                            }
                        }
                    }
                    break;
                case (int)Piece.Knight:
                    (int x, int y)[] possibleMoves = { (1, 2), (1, -2), (-1, 2), (-1, -2), (2, 1), (-2, 1), (2, -1), (-2, -1) };
                    validMoves.AddRange(possibleMoves
                        .Select(possibleMove => (x: possibleMove.x + x, y: possibleMove.y + y))
                        .Where(move => IsEnemyOrEmpty(move.x, move.y, isWhite)));
                    break;
                case (int)Piece.Queen:
                    List<(int x, int y)> directions = new() { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
                    validMoves.AddRange(GetValidSlidingMoves(x, y, isWhite, directions));
                    break;
                case (int)Piece.Rook:
                    directions = new() { (1, 0), (-1, 0), (0, 1), (0, -1) };
                    validMoves.AddRange(GetValidSlidingMoves(x, y, isWhite, directions));
                    break;
                case (int)Piece.Bishop:
                    directions = new() { (1, 1), (1, -1), (-1, 1), (-1, -1) };
                    validMoves.AddRange(GetValidSlidingMoves(x, y, isWhite, directions));
                    break;
                case (int)Piece.King:
                    directions = new() { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
                    validMoves.AddRange(GetValidSlidingMoves(x, y, isWhite, directions, 1));
                    break;
            }

            if (checkForCheckMate)
            {
                List<(int, int)> fullyValidMoves = new();
                foreach (var move in validMoves)
                {
                    int targetPiece = _board[move.y, move.x];
                    _board[move.y, move.x] = _board[y, x];
                    _board[y, x] = 0;
                    if (!IsCheckFor(isWhite))
                        fullyValidMoves.Add(move);
                    _board[y, x] = _board[move.y, move.x];
                    _board[move.y, move.x] = targetPiece;
                }
                kingDeadMoves = validMoves.Except(fullyValidMoves).ToList();
                return fullyValidMoves;
            }
            kingDeadMoves = new();
            return validMoves;
        }

        private List<(int x, int y)> GetValidSlidingMoves(int x, int y, bool isWhite, List<(int x, int y)> directions, int maxDistance = 8)
        {
            var validMoves = new List<(int x, int y)>();
            List<(int x, int y)> directionsToRemove = new();
            int i = 1;
            while (directions.Count > 0 && i <= maxDistance)
            {
                foreach (var direction in directions)
                {
                    (int x, int y) move = (direction.x * i + x, direction.y * i + y);
                    if (IsEmptyField(move.x, move.y))
                    {
                        validMoves.Add(move);
                    }
                    else if (IsEnemy(move.x, move.y, isWhite))
                    {
                        validMoves.Add(move);
                        directionsToRemove.Add(direction);
                    }
                    else
                    {
                        directionsToRemove.Add(direction);
                    }
                }

                foreach (var direction in directionsToRemove)
                {
                    directions.Remove(direction);
                }
                directionsToRemove.Clear();
                i++;
            }
            return validMoves;
        }

        public bool IsCheckFor(bool isWhite)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (!TryGetPiece(x, y, out int pieceType, out bool isCurrentPieceWhite)) continue;
                    if (isWhite == isCurrentPieceWhite) continue;
                    if (pieceType == (int)Piece.Empty) continue;
                    var moves = GetValidMoves(x, y, false);
                    if (moves.Any(move => !TryGetPiece(move.x, move.y, out int pieceType, out _) || pieceType == (int)Piece.King))
                        return true;
                }
            return false;
        }
        private bool HasMovesLeftFor(bool isWhite)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (!TryGetPiece(x, y, out int pieceType, out bool isCurrentPieceWhite)) continue;
                    if (isWhite != isCurrentPieceWhite || pieceType == (int)Piece.Empty) continue;
                    var moves = GetValidMoves(x, y);
                    if (moves.Count > 0)
                        return true;
                }
            return false;
        }
    }
}
