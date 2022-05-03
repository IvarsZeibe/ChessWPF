using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF
{
    enum Pieces : int
    {
        Empty = 0,
        Pawn = 1,
        Bishop = 3,
        Knight = 5,
        Queen = 7,
        King = 9,
        Rook = 11
    }

    enum Piece : int
    {
        Empty   = 0b000,
        Pawn    = 0b001,
        Bishop  = 0b010,
        Knight  = 0b011,
        Queen   = 0b100,
        King    = 0b101,
        Rook    = 0b110
    }
    // mode team pieceType
    // 0    0    000
    internal class ChessBoard
    {
        public int Height { get; } = 8;
        public int Width { get; } = 8;
        public bool IsWhiteTurn { get; private set; } = true;
        private int[,] board;
        public ChessBoard()
        {
            board = new int[,]
            {
                { 11, 5, 3, 9, 7, 3, 5, 11 },
                { 1, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 2, 2, 2, 2, 2, 2, 2, 2 },
                { 12, 6, 4, 10, 8, 4, 6, 12 }
            };
            board = new int[,]
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
        }
        public bool TryGetPiece(int x, int y, out int pieceType, out bool isTeam1)
        {
            //pieceType = 0;
            //isTeam1 = true;
            //if (x < 0 || x >= Width || y < 0 || y >= Height)
            //    return false;
            //pieceType = board[y, x];
            //if (pieceType == 0) return true;
            //if (!IsPlayer1Piece(pieceType))
            //{
            //    isTeam1 = false;
            //    pieceType--;
            //}
            //return true;
            pieceType = 0;
            isTeam1 = true;
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            int piece = board[y, x];
            isTeam1 = (piece & 0b01000) == 0b01000;
            pieceType = piece & 0b00111;
            return true;
        }
        public static bool IsPlayer1Piece(int piece)
        {
            return piece % 2 == 1;
        }

        // x1, y1 = startpos, x2, y2 = targetPos
        public bool Move(int x1, int y1, int x2, int y2)
        {
            // Checks if pieces exist;
            if (!TryGetPiece(x1, y1, out int pieceType, out bool isWhite) ||
                !TryGetPiece(x2, y2, out int targetType, out bool isTargetWhite) ||
                pieceType == 0)
            {
                return false;
            }

            // check if correct player turn and if target is friendly
            if ((IsWhiteTurn == !isWhite) || (targetType != 0 && isTargetWhite == isWhite))
                return false;
            
            int x = x2 - x1; // delta x
            int y = y2 - y1; // delta y

            if (pieceType == (int)Piece.Pawn)
            {
                if (isWhite && y != 1 && !(y == 2 && y1 == 1))
                    return false;
                else if (!isWhite && y != -1 && !(y == -2 && y1 == 6))
                    return false;
                
                if (x > 1 || x < -1)
                    return false;
                if (x != 0 && targetType == 0)
                    return false;
                if (x == 0 && targetType != 0)
                    return false;



                //if (x > 1 || x < -1)
                //    return false;
                ////if (y == 2 && isWhite )
                //if (isWhite)
                //{
                //    if (y == 1 &&
                //        !((x == 1 || x == -1) &&
                //            !(targetSpot != 0 &&
                //            isTargetWhite != isWhite) ||
                //        x == 0 && targetSpot == 0))
                //    {
                //        return false;
                //    }
                //    else if (y == 2 && x != 0 && targetSpot != 0)
                //    {
                //        return false;
                //    }
                //    else return false;
                //}
            }
            else if (pieceType == (int)Piece.Bishop)
            {
                if (Math.Abs(x) != Math.Abs(y))
                    return false;
                if (!IsValidSlide(x1, y1, x, y, x2, y2))
                    return false;
            }
            else if (pieceType == (int)Piece.Queen)
            {
                if (Math.Abs(x) != Math.Abs(y) && x != 0 && y != 0)
                    return false;
                if (!IsValidSlide(x1, y1, x, y, x2, y2))
                    return false;
            }
            else if (pieceType == (int)Piece.Rook)
            {
                if (x != 0 && y != 0)
                    return false;
                if (!IsValidSlide(x1, y1, x, y, x2, y2))
                    return false;
            }
            else if (pieceType == (int)Piece.Knight)
            {
                if (!((Math.Abs(x) == 1 && Math.Abs(y) == 2) ||
                    (Math.Abs(x) == 2 && Math.Abs(y) == 1)))
                {
                    return false;
                }
            }
            else if (pieceType == (int)Piece.King)
            {
                if (Math.Abs(x) > 1 || Math.Abs(y) > 1)
                    return false;
                if (!IsValidSlide(x1, y1, x, y, x2, y2))
                    return false;
            }
            var h = 1 << 3;
            var j = h + 0b10 == 0b1010;
            board[y2, x2] = pieceType + (Convert.ToInt32(isWhite) << 3);
            board[y1, x1] = 0;
            IsWhiteTurn = !IsWhiteTurn;
            return true;
        }
        private bool IsValidSlide(int x, int y, int deltaX, int deltaY, int targetX, int targetY)
        {
            while (Math.Abs(deltaX) + Math.Abs(deltaY) != 0)
            {
                int xPos = x + deltaX;
                int yPos = y + deltaY;
                if (TryGetPiece(xPos, yPos, out int tempSpot, out bool tempIsWhite))
                {
                    if (tempSpot != 0 && (xPos != targetX || yPos != targetY))
                        return false;
                }
                if (deltaX > 0)
                    deltaX--;
                else if (deltaX < 0)
                    deltaX++;
                if (deltaY > 0)
                    deltaY--;
                else if (deltaY < 0)
                    deltaY++;
            }
            return true;
        }
        public int GetPieceType(int piece)
        {
            return piece - Convert.ToInt32(!IsWhiteTurn);
        }
    }
}
