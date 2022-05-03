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
        BitmapImage chessPieceTilemap;
        Dictionary<(int, int), CroppedBitmap> chessPieceImages = new Dictionary<(int, int), CroppedBitmap>();
        ChessBoard board;
        int tileSize = 60;
        (int x, int y)? startPos = null;
        (int x, int y)? targetPos = null;
        public MainWindow()
        {
            InitializeComponent();
            chessPieceTilemap = new BitmapImage(new Uri(Environment.CurrentDirectory + "/ChessPiecesArray.png"));
            for (int i = 0; i < 8; i++)
            {
                ChessGrid.ColumnDefinitions.Add(new ColumnDefinition());
                ChessGrid.RowDefinitions.Add(new RowDefinition());
            }
            board = new ChessBoard();
            UpdateChessGrid();
        }
        void UpdateChessGrid()
        {
            ChessGrid.Children.Clear();
            for (int y = 0; y < board.Height; y++)
                for(int x = 0; x < board.Width; x++)
                {
                    board.TryGetPiece(x, y, out int pieceType, out bool isWhite);
                    Button button = new Button
                    {
                        Content = new Image() { Source = GetPieceImage(pieceType, isWhite)},
                        Name = "btn"
                    };
                    button.Click += ButtonClicked;
                    Grid.SetColumn(button, x);
                    Grid.SetRow(button, y);
                    if ((y + x) % 2 == 1) 
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(100, 100, 0));
                    }
                    ChessGrid.Children.Add(button);
                }
        }
        void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button button = sender as Button;
                int x = Grid.GetColumn(button);
                int y = Grid.GetRow(button);
                Message.Text = "";
                if (startPos is null)
                {
                    board.TryGetPiece(x, y, out int pieceType, out bool isWhite);
                    if (pieceType != 0 && isWhite == board.IsWhiteTurn)
                    {
                        startPos = (x, y);
                        button.Background = Brushes.Gray;
                    }
                }
                else
                {
                    targetPos = (x, y);
                    if (board.Move(startPos.Value.x, startPos.Value.y, targetPos.Value.x, targetPos.Value.y))
                    {
                        Message.Text = "successfully moved";
                    }
                    else
                    {
                        Message.Text = "move failed";
                    }
                    UpdateChessGrid();
                    startPos = null;
                    targetPos = null;
                }
            }
        }
        CroppedBitmap GetPieceImage(int pieceType, bool isWhite)
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
                    x = 0;
                    break;
                case (int)Piece.Knight:
                    x = 3;
                    break;
                case (int)Piece.Queen:
                    x = 1;
                    break;
                case (int)Piece.Bishop:
                    x = 4;
                    break;
            }
            Int32Rect rect = new Int32Rect(x * tileSize, y * tileSize, tileSize, tileSize);
            
            if (chessPieceImages.ContainsKey((x, y)))
                return chessPieceImages[(x, y)];
            chessPieceImages[(x, y)] = new CroppedBitmap(chessPieceTilemap, rect);
            return chessPieceImages[(x, y)];
        }
    }
}
