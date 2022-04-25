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
using System.Diagnostics;

namespace WordGameWpfApp
{
    class DrawGrid
    {
        private const int size = 48;
        private const int space = 6;

        public void DrawRectangles(Canvas LetterGrid, Grid GameCompleteOverlay, WordGame game, Point hoverIndex, Point dragStartIndex)
        {
            LetterGrid.Children.Clear();

            for (int j = 0; j < game.GetGridSize(); j++)
            {
                for (int i = 0; i < game.GetGridSize(); i++)
                {
                    bool IsHover = (hoverIndex.X == i && hoverIndex.Y == j);
                    bool IsDragStart = (dragStartIndex.X == i && dragStartIndex.Y == j);
                    bool IsInWord = game.IsIndexInAFoundWord(i, j);

                    Rectangle rectangle = new Rectangle
                    {
                        Height = size,
                        Width = size,
                    };

                    rectangle.RadiusX = size / 5;
                    rectangle.RadiusY = size / 5;
                    rectangle.Fill = IsInWord ? Brushes.Green : Brushes.Gray;
                    rectangle.Stroke = Brushes.Black;
                    rectangle.StrokeThickness = (IsHover || IsDragStart) ? 4 : 0;
                    LetterGrid.Children.Add(rectangle);

                    Canvas.SetLeft(rectangle, i * (size + space) + space / 2);
                    Canvas.SetTop(rectangle, j * (size + space) + space / 2);


                    TextBlock textBlock = new TextBlock();
                    textBlock.Height = size;
                    textBlock.Width = size;
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.FontSize = 30;
                    textBlock.Text = game.GetGridChar(i, j).ToString().ToUpper();
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Canvas.SetLeft(textBlock, i * (size + space) + space / 2);
                    Canvas.SetTop(textBlock, j * (size + space) + space / 2);
                    LetterGrid.Children.Add(textBlock);
                }
            }

            if (game.IsGameComplete() != WordGame.GameCompleteState.NOT_COMPLETE)
            {
                string msg = "";
                switch (game.IsGameComplete())
                {
                    case WordGame.GameCompleteState.COMPLETE_WIN:
                        GameCompleteOverlay.Visibility = Visibility.Visible;
                        msg = "you win!";
                        break;

                    case WordGame.GameCompleteState.COMPLETE_LOSE:
                        GameCompleteOverlay.Visibility = Visibility.Visible;
                        msg = "You ran out of swaps!";
                        break;
                }
                //Specify if the player won or lost here!!!
                Debug.Write(msg);
                GameCompleteOverlay.Visibility = Visibility.Visible;
            }
            else
                GameCompleteOverlay.Visibility = Visibility.Hidden;
        }

        public Point ConvertPosToGridIndex(Point pos)
        {
            Point index;

            index.X = Math.Floor(pos.X / (space + size));
            index.Y = Math.Floor(pos.Y / (space + size));

            return index;
        }

    }

    public class Lexicon
    {
        public Lexicon()
        {
            //load in lexicon file
            m_lexicon = new Dictionary<string, string>();

            foreach (string line in System.IO.File.ReadLines(@".\CSW22.txt"))
            {
                m_lexicon.Add(line.ToLower(), "");
            }

        }

        public bool IsWord(string word)
        {
            return m_lexicon.ContainsKey(word.ToLower());
        }

        IDictionary<string, string> m_lexicon;
    }

    public class WordGame
    {
        public struct FoundWord
        {
            public string word;
            public List<Point> indexes;
        };

        public WordGame()
        {
            m_allowDiagonals = true;
            m_gridSize = 3;
            m_minWordLength = 3;
            m_MaxSwaps = 0;
        }

        public int GetGridSize()
        {
             return m_gridSize;
        }

        public char GetGridChar(int i, int j)
        {
            return m_grid[i, j];
        }

        public void Reset(int difficulty)
        {
            m_state = GameCompleteState.NOT_COMPLETE;
            int MaxNumWords;

            Dictionary<char, int> letters;
            switch (difficulty)
            {
                case 0:
                    m_gridSize = 3;
                    m_allowDiagonals = true;
                    m_minWordLength = 3;
                    MaxNumWords = 1;
                    letters = new Dictionary<char, int>()
            {
                {'a', 9},
                {'b', 2},
                {'c', 2},
                {'d', 4},
                {'e', 12},
                {'f', 2},
                {'g', 3},
                {'h', 2},
                {'i', 9},
                {'j', 1},
                {'k', 1},
                {'l', 4},
                {'m', 2},
                {'n', 6},
                {'o', 8},
                {'p', 2},
                {'q', 0},
                {'r', 6},
                {'s', 4},
                {'t', 6},
                {'u', 4},
                {'v', 2},
                {'w', 2},
                {'x', 1},
                {'y', 2},
                {'z', 0}
            };
                    break;
                case 1:
                    m_gridSize = 4;
                    m_allowDiagonals = false;
                    m_minWordLength = 3;
                    MaxNumWords = 2;
                    letters = new Dictionary<char, int>()
            {
                {'a', 9},
                {'b', 2},
                {'c', 2},
                {'d', 4},
                {'e', 12},
                {'f', 2},
                {'g', 3},
                {'h', 2},
                {'i', 9},
                {'j', 1},
                {'k', 1},
                {'l', 4},
                {'m', 2},
                {'n', 6},
                {'o', 8},
                {'p', 2},
                {'q', 1},
                {'r', 6},
                {'s', 4},
                {'t', 6},
                {'u', 4},
                {'v', 2},
                {'w', 2},
                {'x', 1},
                {'y', 2},
                {'z', 1}
            };
                    break;
                case 2:
                default:
                    m_gridSize = 5;
                    m_allowDiagonals = false;
                    m_minWordLength = 4;
                    MaxNumWords = 3;
                    letters = new Dictionary<char, int>()
            {
                {'a', 9},
                {'b', 2},
                {'c', 2},
                {'d', 4},
                {'e', 12},
                {'f', 2},
                {'g', 3},
                {'h', 2},
                {'i', 9},
                {'j', 1},
                {'k', 1},
                {'l', 4},
                {'m', 2},
                {'n', 6},
                {'o', 8},
                {'p', 2},
                {'q', 1},
                {'r', 6},
                {'s', 4},
                {'t', 6},
                {'u', 4},
                {'v', 2},
                {'w', 2},
                {'x', 1},
                {'y', 2},
                {'z', 1}
            };
                    break;
            }

            m_grid = new char[m_gridSize, m_gridSize];
            m_swapCounter = 0;

            System.Random random = new System.Random();

            List<char> allLetters = new List<char>();
            foreach(char key in letters.Keys)
            {
                for (int i = 0; i < letters[key]; i++) allLetters.Add(key);
            }

            do
            {
                for (int i = 0; i < m_gridSize; i++)
                {
                    for (int j = 0; j < m_gridSize; j++)
                    {
                        m_grid[i, j] = allLetters[random.Next(allLetters.Count())];
                    }
                }
                FindWords();
            } while (m_foundWords.Count != MaxNumWords);
        }

        public string RenderToString()
        {
            string gridText = "";
            for (int j = 0; j < m_gridSize; j++)
            {
                for (int i = 0; i < m_gridSize; i++)
                {
                    gridText += " | " + m_grid[i, j];
                    if (i == m_gridSize - 1)
                    {
                        gridText += '\n';
                    }
                }
            }
            return gridText;
        }

        public struct IndexContainsWords
        {
            public List<Point> Indexes;
            public List<FoundWord> Words;
        }

        private void FindWords()
        {
            List<FoundWord> words = new List<FoundWord>();
            int[] dx = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int[] dy = { 0, -1, -1, -1, 0, 1, 1, 1 };

            //create structure holding all of the indexes. For each index, store all of the words that contain it

            for (int j = 0; j < m_gridSize; j++)
            {
                for (int i = 0; i < m_gridSize; i++)
                {
                    for (int dir = 0; dir < 8; dir += (m_allowDiagonals ? 1 : 2))
                    {
                        var word = new StringBuilder("");
                        var indexes = new List<Point>();
                        int i1 = i, j1 = j;
                        while (i1 >= 0 & j1 >= 0 & i1 < m_gridSize & j1 < m_gridSize)
                        {
                            word.Append(m_grid[i1, j1]);
                            indexes.Add(new Point(i1, j1));

                            if (word.Length >= m_minWordLength & m_lexicon.IsWord(word.ToString()))
                            {
                                FoundWord foundWord;
                                foundWord.word = word.ToString();
                                foundWord.indexes = new List<Point>(indexes);
                                words.Add(foundWord);
                            }
                            i1 += dx[dir];
                            j1 += dy[dir];
                        }
                        
                    }
                }
            }
            m_foundWords = words;
        }

        public void Swap(int i1, int j1, int i2, int j2)
        {
            if ((i1 == i2) & (j1 == j2)) return;
            if (m_state != GameCompleteState.NOT_COMPLETE) return;

            char temp;

            temp = m_grid[i1, j1];
            m_grid[i1, j1] = m_grid[i2, j2];
            m_grid[i2, j2] = temp;

            m_swapCounter++;
            FindWords();
            UpdateGameState();
        }

        private void UpdateGameState()
        {

            // check if game is won
            bool win = true;
            //loop through all i & j, if not, return false. if is, return true
            for (int i = 0; i < m_gridSize; i++)
            {
                for (int j = 0; j < m_gridSize; j++)
                {
                    if (!IsIndexInAFoundWord(i, j))
                    {
                        win = false;
                    }
                }
            }

            if (win)
            {
                m_state = GameCompleteState.COMPLETE_WIN;
                //SetPNGSource();
            }
            else if (m_MaxSwaps > 0 && m_swapCounter >= m_MaxSwaps)
            {
                m_state = GameCompleteState.COMPLETE_LOSE;
                //SetPNGSource();
            }
            else m_state = GameCompleteState.NOT_COMPLETE;
        }

        public int GetSwapCounter()
        {
            return m_swapCounter;
        }
        public int GetFoundWordCount()
        {
            return m_foundWords.Count();
        }

        public List<string> GetFoundWords()
        {
            var words = new List<string>();

            foreach(FoundWord word in m_foundWords)
            {
                words.Add(word.word);
            }
            return words;
        }

        public bool IsIndexInAFoundWord(int i, int j)
        {
            var p = new Point(i, j);

            foreach(FoundWord word in m_foundWords)
            {
                foreach(Point index in word.indexes)
                {
                    if (index == p) return true;
                }
            }

            return false;
        }

        public enum GameCompleteState { NOT_COMPLETE, COMPLETE_WIN, COMPLETE_LOSE };
        public GameCompleteState IsGameComplete()
        {
            return m_state;
        }

        Lexicon m_lexicon = new Lexicon();
        private int m_MaxSwaps;
        private int m_gridSize;
        private char[,] m_grid;
        private bool m_allowDiagonals;
        private int m_minWordLength;
        private List<FoundWord> m_foundWords;
        private int m_swapCounter;
        private GameCompleteState m_state;
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            m_IsDragging = false;
            m_DragStartIndex = new Point(-1, -1);
            m_HoverIndex = new Point(-1, -1);

            m_game = new WordGame();
            m_game.Reset(GridSize.SelectedIndex);

            drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);
            
            WordCount.Text = m_game.GetFoundWordCount().ToString();
            Words.Text = string.Join("\n",m_game.GetFoundWords());
            SwapCounter.Text = m_game.GetSwapCounter().ToString();
        }

        private void LetterGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point index = drawGrid.ConvertPosToGridIndex(e.GetPosition(LetterGrid));
         
            m_IsDragging = true;
            m_DragStartIndex = index;
        }

        private void LetterGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point index = drawGrid.ConvertPosToGridIndex(e.GetPosition(LetterGrid));

            if (m_IsDragging)
            {
                m_game.Swap((int)m_DragStartIndex.X, (int)m_DragStartIndex.Y, (int)index.X, (int)index.Y);
                m_IsDragging = false;
                m_DragStartIndex = new Point(-1, -1);

                Words.Text = string.Join("\n", m_game.GetFoundWords());
                drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);
                WordCount.Text = m_game.GetFoundWordCount().ToString();
                SwapCounter.Text = (m_game.GetSwapCounter().ToString() + "/∞");
                //m_game.IsIndexInAFoundWord(m_HoverIndex);
            }
        }


        private void LetterGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point index = drawGrid.ConvertPosToGridIndex(e.GetPosition(LetterGrid));

            if (m_HoverIndex != index)
            {
                m_HoverIndex = index;

                drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);
            }
        }

        private void Reset_Button(object sender, RoutedEventArgs e)
        {
            m_game.Reset(GridSize.SelectedIndex);

            drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);

            WordCount.Text = m_game.GetFoundWordCount().ToString();
            Words.Text = string.Join("\n", m_game.GetFoundWords());
            SwapCounter.Text = (m_game.GetSwapCounter().ToString() + "/∞");
            GameCompleteOverlay.Visibility = Visibility.Hidden;
        }

        /*public void ScoreUpdater(int PlayedCount, int WonPercentage, int CurrentStreak, int MaxStreak, int SwapCountScore, int WordCountScore)
        {
            SwapCountScore.text = m_game.GetSwapCounter().ToString();
            WordCountScore = m_game.GetFoundWordCount();

        }*/

        WordGame m_game;
        private DrawGrid drawGrid = new DrawGrid();
        Point m_DragStartIndex;
        bool m_IsDragging;
        Point m_HoverIndex;
    }
}


