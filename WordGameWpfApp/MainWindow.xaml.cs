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
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace WordGameWpfApp
{
    class DrawGrid
    {
        private const int size = 48;
        private const int space = 6;

        public void DrawRectangles(Canvas LetterGrid, Grid GameCompleteOverlay, WordGame game, Point hoverIndex, Point dragStartIndex)
        {
            // clear the canvas
            LetterGrid.Children.Clear();

            // loop through each cell
            for (int j = 0; j < game.GetGridSize(); j++)
            {
                for (int i = 0; i < game.GetGridSize(); i++)
                {
                    // set values based on if the cell is being hovered over or dragged
                    bool IsHover = (hoverIndex.X == i && hoverIndex.Y == j);
                    bool IsDragStart = (dragStartIndex.X == i && dragStartIndex.Y == j);
                    // check if the current cell is in a found word
                    bool IsInWord = game.IsIndexInAFoundWord(i, j);

                    // create a new rectangle element
                    Rectangle rectangle = new Rectangle
                    {
                        Height = size, Width = size,
                        // set the corner radius of the rectangle
                        RadiusX = size / 5, RadiusY = size / 5,
                        // Fill the rectangle with green if the cell is in a found word, otherwise grey 
                        Fill = IsInWord ? Brushes.Green : Brushes.Gray,
                        // Set the stroke color and thickness of the rectangle based on whether it's being hovered over or dragged
                        Stroke = Brushes.Black,
                        StrokeThickness = (IsHover || IsDragStart) ? 4 : 0,
                    };

                    // add the rectangle to the lettergrid canvas
                    LetterGrid.Children.Add(rectangle);

                    // set the position of the rectangle on the canvas, ready for the next cell
                    Canvas.SetLeft(rectangle, i * (size + space) + space / 2);
                    Canvas.SetTop(rectangle, j * (size + space) + space / 2);


                    // create a new text block element to display the letter for that cell
                    TextBlock textBlock = new TextBlock
                    {
                        Height = size, Width = size,
                        TextAlignment = TextAlignment.Center, FontSize = 30,
                        // retrieves the char for this cell and adds it to the text element
                        Text = game.GetGridChar(i, j).ToString().ToUpper(),
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    };

                    //set the position of the text element
                    Canvas.SetLeft(textBlock, i * (size + space) + space / 2);
                    Canvas.SetTop(textBlock, j * (size + space) + space / 2);

                    // add the text block to the lettergrid canvas
                    LetterGrid.Children.Add(textBlock);
                }
            }

            // check if the game is over
            checkGameOver(game, GameCompleteOverlay);
        }

        private void checkGameOver(WordGame game, Grid GameCompleteOverlay)
        {
            string msg = "";

            // Show the GameCompleteOverlay and display a message based on whether the player won or lost
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
                case WordGame.GameCompleteState.NOT_COMPLETE:
                GameCompleteOverlay.Visibility = Visibility.Hidden;
                msg = "not complete";
                    break;
            }
            // send winning message in consol for debugging
            Debug.WriteLine(msg);
        }

        public void DisplayGameOver(Grid GameCompleteOverlay, WordGame game, TextBox PlayedScore, 
            TextBox WonPercentageScore, TextBox CurrentStreakScore, TextBox MaxStreakScore, 
            TextBox SwapCountScore, TextBox WordCountScore, TextBox WonOrLostDisplay)
        {
            // sets the on screen text to the relevant values.
            WonOrLostDisplay.Text = game.GetWonOrLostDisplay();
            PlayedScore.Text = game.GetPlayedScore().ToString();
            WonPercentageScore.Text = game.GetWonPercentageScore().ToString();
            CurrentStreakScore.Text = game.GetCurrentStreakScore().ToString();
            MaxStreakScore.Text = game.GetMaxStreakScore().ToString();

            // if the max number of swaps is set to 0 or less:
            if(game.GetMaxSwaps() <= 0)
            {
                // set the out of score to be the infinite symbol as the player will have infinite swaps
                // updates the swap count score to the new value, allowing the player to see their live swap counter
                SwapCountScore.Text = game.GetSwapCountScore().ToString() + "/∞";
            } else
            {
                // otherwise make the max swaps value equal to the maxSwaps value.
                // this will allow the user to see how many swaps they have left
                // again update the swap count
                SwapCountScore.Text = game.GetSwapCountScore().ToString() + "/" + game.GetMaxSwaps().ToString();
            }
            //update the word count score 
            WordCountScore.Text = game.CalculateScores().ToString();
        }

        public Point ConvertPosToGridIndex(Point pos)
        {
            Point index;

            index.X = Math.Floor(pos.X / (space + size));
            index.Y = Math.Floor(pos.Y / (space + size));

            Debug.WriteLine("ConvertPosToGridIndex - index:" + index);
            return index;
        }

    }
    
    public class Lexicon
    {
        public Lexicon()
        {
            // constructor for the lexicon class

            // creates a dictionary to store the lexicon words
            m_lexicon = new Dictionary<string, string>();

            // add each line of the text file to my lexicon
            foreach (string line in System.IO.File.ReadLines(@".\CSW22.txt"))
            {
                // adds the word to the lexicon as lowercase as the key and an empty string as the value.
                // this value could be set to something else in future, for example, the meaning of the word
                m_lexicon.Add(line.ToLower(), "");
            }

        }

        public bool IsWord(string word)
        {
            //checks if a string is in the dictionary

            //converts the string to lowercase and checks if it exists as a key in the dictionary
            return m_lexicon.ContainsKey(word.ToLower());
        }

        //member variable dictionary to store the words
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
            m_state = GameCompleteState.NOT_COMPLETE;  //makes the game not complete
            int MaxNumWords;
            int salt = 0;

            Dictionary<char, int> letters; //declares a dictionary where the key is a letter and the value is an int (scrabble value)
            switch (difficulty) //switch to execute certain code based on the difficulty
            {
                case 0:
                default:
                    // DAILY CHALLENGE MODE
                    // set the salt based on date
                    salt = DateTime.Today.Ticks.GetHashCode();

                    m_gridSize = 4; //sets the game settings
                    m_allowDiagonals = false;
                    m_minWordLength = 3;
                    MaxNumWords = 2;
                    SetMaxSwaps(10);
                    letters = new Dictionary<char, int>() //fills letters with the alphabet and its values
                    {
                        {'a', 9}, {'b', 2},  {'c', 2}, {'d', 4}, {'e', 12}, {'f', 2}, {'g', 3},{'h', 2},
                        {'i', 9}, {'j', 1}, {'k', 1}, {'l', 4}, {'m', 2}, {'n', 6}, {'o', 8}, {'p', 2},
                        {'q', 0}, {'r', 6}, {'s', 4}, {'t', 6}, {'u', 4}, {'v', 2}, {'w', 2}, {'x', 1},
                        {'y', 2}, {'z', 0}
                    };
                    break;
                case 2:
                    m_gridSize = 4; //sets the game settings
                    m_allowDiagonals = false;
                    m_minWordLength = 3;
                    MaxNumWords = 2;
                    letters = new Dictionary<char, int>() //fills letters with the alphabet and its values
                    {
                        {'a', 9}, {'b', 2},  {'c', 2}, {'d', 4}, {'e', 12}, {'f', 2}, {'g', 3},{'h', 2},
                        {'i', 9}, {'j', 1}, {'k', 1}, {'l', 4}, {'m', 2}, {'n', 6}, {'o', 8}, {'p', 2},
                        {'q', 0}, {'r', 6}, {'s', 4}, {'t', 6}, {'u', 4}, {'v', 2}, {'w', 2}, {'x', 1},
                        {'y', 2}, {'z', 0}
                    };
                    break;
                case 3:
                    m_gridSize = 5; //sets the game settings
                    m_allowDiagonals = false;
                    m_minWordLength = 4;
                    MaxNumWords = 3;
                    letters = new Dictionary<char, int>() //fills letters with the alphabet and its values
                    {
                        {'a', 9}, {'b', 2},  {'c', 2}, {'d', 4}, {'e', 12}, {'f', 2}, {'g', 3},{'h', 2},
                        {'i', 9}, {'j', 1}, {'k', 1}, {'l', 4}, {'m', 2}, {'n', 6}, {'o', 8}, {'p', 2},
                        {'q', 0}, {'r', 6}, {'s', 4}, {'t', 6}, {'u', 4}, {'v', 2}, {'w', 2}, {'x', 1},
                        {'y', 2}, {'z', 0}
                    };
                    break;
                case 1:
                    m_gridSize = 3; //sets the game settings
                    m_allowDiagonals = true;
                    m_minWordLength = 3;
                    MaxNumWords = 1;
                    letters = new Dictionary<char, int>() //fills letters with the alphabet and its values
                    {
                        {'a', 9}, {'b', 2},  {'c', 2}, {'d', 4}, {'e', 12}, {'f', 2}, {'g', 3},{'h', 2},
                        {'i', 9}, {'j', 1}, {'k', 1}, {'l', 4}, {'m', 2}, {'n', 6}, {'o', 8}, {'p', 2},
                        {'q', 0}, {'r', 6}, {'s', 4}, {'t', 6}, {'u', 4}, {'v', 2}, {'w', 2}, {'x', 1},
                        {'y', 2}, {'z', 0}
                    };
                    break;
            }

            m_grid = new char[m_gridSize, m_gridSize]; //declares a 2D array with size m_gridSize
            m_swapCounter = 0; //resets the swap counter

            System.Random random = salt != 0 ? new System.Random(salt) : new System.Random();

            List<char> allLetters = new List<char>(); //initialises allLetters, a list of chars
            foreach(char key in letters.Keys) //fills all letters with each letter the amount of times of its value
            {
                for (int i = 0; i < letters[key]; i++) allLetters.Add(key);
            }

            do //fills the m_grid with random letters from allLetters 
            {
                for (int i = 0; i < m_gridSize; i++)
                {
                    for (int j = 0; j < m_gridSize; j++)
                    {
                        m_grid[i, j] = allLetters[random.Next(allLetters.Count())];
                    }
                }
                FindWords(); //repeats the process until the grid contains the desireable amount of words
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

//        public struct IndexContainsWords
//        {
//            public List<Point> Indexes;
//            public List<FoundWord> Words;
//        }

        private void FindWords()
        {
            List<FoundWord> words = new List<FoundWord>(); // A list of the found words, sorted with a string and a list of indexes
            int[] dx = { 1, 1, 0, -1, -1, -1, 0, 1 }; // dx and dy mark the directions for the search algorithm.
            int[] dy = { 0, -1, -1, -1, 0, 1, 1, 1 }; // for example, dx[0], dy[0] would represent the direction to the right

            // create structure holding all of the indexes. For each index, store all of the words that contain it

            for (int j = 0; j < m_gridSize; j++)
            { // the j and i loops will iterate over each cell or element of the grid
                for (int i = 0; i < m_gridSize; i++)
                {
                    // for each cell, explore all directions (only including diagonals if allowDiagonals is true)
                    // if allowDiagonals is false, it will skip every other element of dx/dy
                    for (int dir = 0; dir < 8; dir += (m_allowDiagonals ? 1 : 2))
                    {
                        // initialise a new StringBuilder and a list of indexes
                        var word = new StringBuilder("");
                        var indexes = new List<Point>();
                        int i1 = i, j1 = j;

                        // continue searching in the current direction until we hit the edge of the grid
                        while (i1 >= 0 & j1 >= 0 & i1 < m_gridSize & j1 < m_gridSize)
                        {
                            // Append the current character to the word and add the current index to the list of indexes
                            word.Append(m_grid[i1, j1]);
                            indexes.Add(new Point(i1, j1));

                            // if the current 'word' is at least the length of the minWordLength, and is a valid word then:
                            if (word.Length >= m_minWordLength & m_lexicon.IsWord(word.ToString()))
                            {
                                // creates a new FoundWord object using 'word' and 'indexes'
                                FoundWord foundWord;
                                foundWord.word = word.ToString();
                                foundWord.indexes = new List<Point>(indexes);
                                words.Add(foundWord); // adds it to the list of found words
                            }
                            //increments i1 and j1 using dx and dy, specifying the next cell in the current direction
                            i1 += dx[dir]; 
                            j1 += dy[dir];
                        }
                        
                    }
                }
            }
            // updates the member variables to store the list of found words.
            m_foundWords = words;
        }

        public void Swap(int i1, int j1, int i2, int j2)
        {
            // i1, j1 and i2, j2 are two sets of coordinates.

            // check if the two coordinates are the same.
            // if so then they dont need to swap and the program can return
            if ((i1 == i2) & (j1 == j2)) return;
            // if the game is already complete, then it will return
            if (m_state != GameCompleteState.NOT_COMPLETE) return;

            char temp; // temporary variable to hold a char while swapping

            // swaps the two charachters at the two sets of coordinates
            temp = m_grid[i1, j1];
            m_grid[i1, j1] = m_grid[i2, j2];
            m_grid[i2, j2] = temp;

            m_swapCounter++; // increments the swap counter
            FindWords(); //search the grid for words
            UpdateGameState(); // checks if the game is complete
        }

        private void UpdateGameState()
        {
            // this method will update the game state accordingly

            // initialises the win variable as true
            bool win = true;

            //loop through all cells in the grid, if not, return false. if is, return true
            for (int i = 0; i < m_gridSize; i++)
            {
                for (int j = 0; j < m_gridSize; j++)
                {
                    // checks if the current cell is included in a found word
                    if (!IsIndexInAFoundWord(i, j))
                    {
                        //if its not then set win to false
                        win = false;

                        // Halt the loop and exit early, since we've already found a cell that's not part of a found word
                        i = m_gridSize;
                        j = m_gridSize;
                    }
                }
            }

            // if all cells are part of a word then game is won
            if (win)
            {
                // sets the game state to complete win
                m_state = GameCompleteState.COMPLETE_WIN;
                // updates the game over scores, which will call the game over overlay and end the game
                UpdateGameOverScores();
            }
            // if the maxSwaps has been reached, the game is lost
            else if ((m_MaxSwaps > 0 && m_swapCounter >= m_MaxSwaps))
            {
                // sets the game state to complete lose
                m_state = GameCompleteState.COMPLETE_LOSE;
                // updates the game over scores, which will call the game over overlay and end the game
                UpdateGameOverScores();
            }
            // otherwise sets the game state to not complete and continues the game
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

        private void UpdateGameOverScores()
        {
            m_playedScore += 1;

            if (m_state == GameCompleteState.COMPLETE_WIN)
            {
                m_WonOrLostDisplay = "You Won!";
                m_wonTotal += 1;
                m_currentStreakScore += 1;
                if (m_currentStreakScore > m_maxStreakScore)
                {
                    m_maxStreakScore = m_currentStreakScore;
                }

            } else if (m_state == GameCompleteState.COMPLETE_LOSE)
            {
                m_WonOrLostDisplay = "You Lost :(";
                m_lostTotal += 1;
                m_currentStreakScore = 0;
            }
            m_wonPercentageScore = m_wonTotal / m_playedScore * 100;
        }

        public int CalculateScores()
        {
            int score = (m_MaxSwaps - m_swapCounter) * 20 + (m_state == GameCompleteState.COMPLETE_WIN ? 1000 : 0);


            return score;
        }

        public enum GameCompleteState { NOT_COMPLETE, COMPLETE_WIN, COMPLETE_LOSE };
        public GameCompleteState IsGameComplete()
        {
            return m_state;
        }

        public void setGameState(GameCompleteState state)
        {
            m_state = state;
        }

        public string GetWonOrLostDisplay()
        {
            return m_WonOrLostDisplay;
        }
        public int GetPlayedScore() //ADD REST
        {
            return m_playedScore;
        }
        public int GetWonPercentageScore()
        {
            return m_wonPercentageScore;
        }
        public int GetCurrentStreakScore()
        {
            return m_currentStreakScore;
        }
        public int GetMaxStreakScore()
        {
            return m_maxStreakScore;
        }
        public int GetSwapCountScore()
        {
            return m_swapCounter;
        }
        public int GetMaxSwaps()
        {
            return m_MaxSwaps;
        }
        public void SetMaxSwaps(int maxSwaps)
        {
            m_MaxSwaps = maxSwaps;
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

        private String m_WonOrLostDisplay;
        private int m_playedScore;
        private int m_wonPercentageScore;
        private int m_wonTotal;
        private int m_lostTotal;
        private int m_currentStreakScore;
        private int m_maxStreakScore;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the start time to the current time
            m_StartTime = DateTime.Now;
            m_GameDuration = TimeSpan.FromMinutes(5);

            m_IsDragging = false;
            m_DragStartIndex = new Point(-1, -1);
            m_HoverIndex = new Point(-1, -1);

            m_game = new WordGame();
            m_game.Reset(GridSize.SelectedIndex);

            drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);
            drawGrid.DisplayGameOver(GameCompleteOverlay, m_game, PlayedScore, WonPercentageScore, CurrentStreakScore, 
            MaxStreakScore, SwapCountScore, WordCountScore, WonOrLostDisplay);

            WordCount.Text = m_game.GetFoundWordCount().ToString();
            Words.Text = string.Join("\n",m_game.GetFoundWords());
            SetSwapCountDisplay();
            //TimeRemainingLabel.Content = string.Format("{0:mm\\:ss}", m_TimeRemaining);
        }

        private void LetterGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point index = drawGrid.ConvertPosToGridIndex(e.GetPosition(LetterGrid));
         
            m_IsDragging = true;
            m_DragStartIndex = index;

            Debug.WriteLine("LetterGrid_MouseDown - dragging: " + m_IsDragging + ", dragStartIndex: " + m_DragStartIndex);
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
                drawGrid.DisplayGameOver(GameCompleteOverlay, m_game, PlayedScore, WonPercentageScore, CurrentStreakScore, 
                    MaxStreakScore, SwapCountScore, WordCountScore, WonOrLostDisplay);
                WordCount.Text = m_game.GetFoundWordCount().ToString();
                SetSwapCountDisplay();
            }
            Debug.WriteLine("LetterGrid_MouseUp - swapping: (" + (int)m_DragStartIndex.X + ", "+ (int)m_DragStartIndex.Y + ") with (" + (int)index.X +", "+ (int)index.Y + ")");
        }
        private void LetterGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point index = drawGrid.ConvertPosToGridIndex(e.GetPosition(LetterGrid));

            if (m_HoverIndex != index && m_game.IsGameComplete() == WordGame.GameCompleteState.NOT_COMPLETE)
            {
                m_HoverIndex = index;

                drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);
            }
            Debug.WriteLine("LetterGrid_MouseMove - index: " + m_HoverIndex);
        }
        private void LetterGrid_Background_MouseDown(object sender, RoutedEventArgs e)
        {
            if (GameCompleteOverlay.Visibility == Visibility.Visible)
            {
                GameCompleteOverlay.Visibility = Visibility.Hidden;
            }
            Debug.WriteLine("LetterGrid_Background_MouseDown");
        }

        private void SetSwapCountDisplay() //updates the swap counter on screen
        {
            if (m_game.GetMaxSwaps() <= 0) //checks if there is a max swaps or not
            { //if not then the max is set to infinate
                SwapCounter.Text = m_game.GetSwapCounter().ToString() + "/∞"; 
            }
            else
            { //otherwise its set to the max swap amount
                SwapCounter.Text = m_game.GetSwapCounter().ToString() + "/" + m_game.GetMaxSwaps().ToString();
            }
        }

        private TimeSpan GetTimeRemaining()
        {
            // Calculate the elapsed time since the start of the game
            TimeSpan elapsedTime = DateTime.Now - m_StartTime;

            // Calculate the time remaining in the game
            TimeSpan m_TimeRemaining = m_GameDuration - elapsedTime;

            // Make sure the time remaining is not negative
            if (m_TimeRemaining < TimeSpan.Zero)
            {
                m_TimeRemaining = TimeSpan.Zero;
                m_game.setGameState(WordGame.GameCompleteState.COMPLETE_LOSE);
            }

            return m_TimeRemaining;
        }

        private void Reset_Button(object sender, RoutedEventArgs e)
        { //button listener which executes whenever NEW GAME button is clicked
            if (InputMaxSwaps.Text != "") //WILL BREAK IF !INT ENTERED
            { //takes the inputted max swaps and sets it
                m_game.SetMaxSwaps(int.Parse(InputMaxSwaps.Text));
            }
            
            //resets the game, using the set settings (difficulty & max swaps override)
            m_game.Reset(GridSize.SelectedIndex); 

            //regenerates the grid
            drawGrid.DrawRectangles(LetterGrid, GameCompleteOverlay, m_game, m_HoverIndex, m_DragStartIndex);

            //regenerates any on screen scores
            WordCount.Text = m_game.GetFoundWordCount().ToString();
            Words.Text = string.Join("\n", m_game.GetFoundWords());
            SetSwapCountDisplay();
            //hides the game complete overlay
            GameCompleteOverlay.Visibility = Visibility.Hidden;
        }

        private void Share_Button(object sender, RoutedEventArgs e)
        {
            int IntDifficulty = GridSize.SelectedIndex;
            String difficulty = IntDifficulty == 0 ? "Daily Challenge" : IntDifficulty == 1 ? "Easy" : IntDifficulty == 2 ? "Normal" : "Impossible";
            int score = m_game.CalculateScores();
            int swaps = m_game.GetSwapCounter();
            int wordCount = m_game.GetFoundWordCount();

            Clipboard.SetText(
                "Splot score! \n" 
                + "Difficulty: " + difficulty + "\n" 
                + "🏆 Score: " + score.ToString() + "\n" 
                + "↔️ Swaps: " + swaps.ToString());
        }

        public TimeSpan m_TimeRemaining;
        private DateTime m_StartTime;
        private TimeSpan m_GameDuration;
        WordGame m_game;
        private DrawGrid drawGrid = new DrawGrid();
        Point m_DragStartIndex;
        bool m_IsDragging;
        Point m_HoverIndex;
    }
}


