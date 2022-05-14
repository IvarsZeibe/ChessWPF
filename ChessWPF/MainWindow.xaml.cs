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
                    _board = new ChessBoard(FEN.Text);
                    RefreshChessGrid();
                    FEN.Background = Brushes.White;
                    CapturedBlacks.Children.Clear();
                    CapturedWhites.Children.Clear();
                }
                catch (Exception ex)
                {
                    FEN.Background = Brushes.Salmon;
                }
            };
            QueenButton.Click += PromotionButton;
            KnightButton.Click += PromotionButton;
            RookButton.Click += PromotionButton;
            BishopButton.Click += PromotionButton;
        }
        void ShowPromotionChoice(bool isWhite)
        {
            if (QueenButton.Content is Image image1)
                image1.Source = GetPieceImage((int)Piece.Queen, isWhite);
            if (KnightButton.Content is Image image2)
                image2.Source = GetPieceImage((int)Piece.Knight, isWhite);
            if (RookButton.Content is Image image3)
                image3.Source = GetPieceImage((int)Piece.Rook, isWhite);
            if (BishopButton.Content is Image image4)
                image4.Source = GetPieceImage((int)Piece.Bishop, isWhite);
            PromotionChoice.Visibility = Visibility.Visible;
        }
        void PromotionButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                Piece pieceType = Piece.Empty;
                switch (button.Name)
                {
                    case "QueenButton":
                        pieceType = Piece.Queen;
                        break;
                    case "KnightButton":
                        pieceType = Piece.Knight;
                        break;
                    case "RookButton":
                        pieceType = Piece.Rook;
                        break;
                    case "BishopButton":
                        pieceType = Piece.Bishop;
                        break;
                }
                _board.TryPromote(pieceType);
            }
            PromotionChoice.Visibility = Visibility.Hidden;
            RefreshChessGrid();
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
                else if (_board.GetLastMovedLocation() is (int, int) position && position == (x, y))
                {
                    button.Background = Brushes.Gold;
                }
                else
                    button.Background = (y + x) % 2 == 1 ? Brushes.DarkKhaki : Brushes.LightYellow;
            }
            _gameStatus = _board.CalculateGameStatus();
            UpdateMessage();
        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_board.IsPromoting) return;
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
                if (_board.TryMove(_startPos.Value.x, _startPos.Value.y, x, y, out int capturedPiece))
                {
                    if (_board.GetType(capturedPiece) != Piece.Empty)
                    {
                        var pieceImage = new Image() { Source = GetPieceImage((int)_board.GetType(capturedPiece), _board.IsWhite(capturedPiece)) };
                        // if is white turn, then a piece was captured by black player
                        if (_board.IsWhiteTurn)
                        {
                            Grid.SetRow(pieceImage, CapturedBlacks.Children.Count / CapturedBlacks.ColumnDefinitions.Count);
                            Grid.SetColumn(pieceImage, CapturedBlacks.Children.Count % CapturedBlacks.ColumnDefinitions.Count);
                            CapturedBlacks.Children.Add(pieceImage);
                        }
                        else
                        {
                            Grid.SetRow(pieceImage, CapturedWhites.Children.Count / CapturedWhites.ColumnDefinitions.Count);
                            Grid.SetColumn(pieceImage, CapturedWhites.Children.Count % CapturedWhites.ColumnDefinitions.Count);
                            CapturedWhites.Children.Add(pieceImage);
                        }
                    }
                }
                if (_board.IsPromoting)
                {
                    ShowPromotionChoice(!_board.IsWhiteTurn);
                }
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
                    bool temp = _board.IsWhiteTurn;
                    if (_board.IsPromoting)
                        temp = !temp;
                    Message.Content = (temp ? "White" : "Black") + " player's turn.";
                    if (_board.IsPromoting)
                    {
                        IsCheck.Content = "Choose promotion!";
                    }
                    else if (_board.IsCheckForCurrentTeam)
                    {
                        IsCheck.Content = "Check!";
                    }
                    else
                    {
                        IsCheck.Content = "";
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