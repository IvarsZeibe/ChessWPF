using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Diagnostics;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage _chessPieceTilemap;
        private Dictionary<(int, int), CroppedBitmap> _chessPieceImages = new();
        private ChessBoard _board;
        private int _tileSize = 60;
        private (int x, int y)? _startPos;
        private List<(int x, int y)> validMoves = new();
        private List<(int x, int y)> kingDeadMoves = new();
        private GameStatus _gameStatus;
        public MainWindow()
        {
            InitializeComponent();
            _chessPieceTilemap = new BitmapImage(new Uri(Environment.CurrentDirectory + "/ChessPiecesArray.png"));
            _board = new ChessBoard();
            CreateChessGrid();
            RefreshChessGrid();
            ResetButton.Click += (s, e) => { _board = new ChessBoard(); RefreshChessGrid(); };
            LoadButton.Click += (s, e) => 
            {
                try
                {
                    _board = new ChessBoard(FEN.Text); RefreshChessGrid();
                    FEN.Background = Brushes.White;
                }
                catch (Exception ex)
                {
                    FEN.Background = Brushes.Salmon;
                }
            };
        }
        private void CreateChessGrid()
        {
            for (int i = 0; i < 8; i++)
            {
                ChessGrid.ColumnDefinitions.Add(new ColumnDefinition());
                ChessGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                Button button = new Button
                {
                    Name = "btn",
                    Content = new Image()
                };
                button.Click += ButtonClicked;
                Grid.SetColumn(button, x);
                Grid.SetRow(button, y);
                ChessGrid.Children.Add(button);
            }
        }
        private void RefreshChessGrid()
        {
            foreach (object? children in ChessGrid.Children)
            {
                if (children is not Button button) continue;
                int x = Grid.GetColumn(button);
                int y = Grid.GetRow(button);
                _board.TryGetPiece(x, y, out int pieceType, out bool isWhite);
                if (button.Content is not Image image) continue;
                image.Source = GetPieceImage(pieceType, isWhite);
                if ((x, y) == _startPos)
                    button.Background = Brushes.DarkTurquoise;
                else if (validMoves.Contains((x, y)))
                    button.Background = Brushes.LightGreen;
                else if (kingDeadMoves.Contains((x, y)))
                    button.Background = Brushes.MistyRose;
                else
                    button.Background = (y + x) % 2 == 1 ? Brushes.DarkKhaki : Brushes.LightYellow;
            }
            _gameStatus = _board.CalculateGameStatus();
            UpdateMessage();
        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            int x = Grid.GetColumn(button);
            int y = Grid.GetRow(button);
            if (_startPos is null)
            {
                if (!_board.TryGetPiece(x, y, out int pieceType, out bool isWhite)) return;
                if (pieceType == 0 || isWhite != _board.IsWhiteTurn) return;

                _startPos = (x, y);
                validMoves = _board.GetValidMoves(x, y, out kingDeadMoves);
            }
            else
            {
                _board.TryMove(_startPos.Value.x, _startPos.Value.y, x, y);
                validMoves.Clear();
                kingDeadMoves.Clear();
                _startPos = null;
            }
            RefreshChessGrid();
        }
        void UpdateMessage()
        {
            switch (_gameStatus)
            {
                case GameStatus.InProgress:
                    Message.Content = (_board.IsWhiteTurn ? "White" : "Black") + " player's turn.";
                    if (_board.IsCheckForCurrentTeam)
                    {
                        Message.Content += "\nCheck!";
                    }
                    break;
                case GameStatus.WhiteWins:
                    Message.Content = "White Wins!";
                    break;
                case GameStatus.BlackWins:
                    Message.Content = "Black Wins!";
                    break;
                case GameStatus.Stalemate:
                    Message.Content = "Stalemate!";
                    break;
            }
        }
        
        private CroppedBitmap GetPieceImage(int pieceType, bool isWhite)
        {
            int y = isWhite ? 1 : 0;
            int x = 0;
            switch (pieceType)
            {
                case 0:
                    return null;
                case (int)Piece.Pawn:
                    x = 5;
                    break;
                case (int)Piece.Rook:
                    x = 2;
                    break;
                case (int)Piece.King:
                    x = 1;
                    break;
                case (int)Piece.Knight:
                    x = 3;
                    break;
                case (int)Piece.Queen:
                    x = 0;
                    break;
                case (int)Piece.Bishop:
                    x = 4;
                    break;
            }
            Int32Rect rect = new Int32Rect(x * _tileSize, y * _tileSize, _tileSize, _tileSize);
            
            if (_chessPieceImages.ContainsKey((x, y)))
                return _chessPieceImages[(x, y)];
            _chessPieceImages[(x, y)] = new CroppedBitmap(_chessPieceTilemap, rect);
            return _chessPieceImages[(x, y)];
        }
    }
}