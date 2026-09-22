using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Threading;
//using System.Diagnostics;
//using System.Management;

class Searcher
{
    private List<string> m_words = null;
    private char[,] m_board = null;
    private int m_rows = 0;
    private int m_cols = 0;

    private Form m_form = null; // form to notify using BeginInvoke
    private MainForm.UpdateProgressBar UpdateProgressBarDelegate = null;
    private MainForm.UpdateResultsListBox UpdateResultsListBoxDelegate = null;
    private const int SLEEP_TIME = 10; // ms
    public Searcher(Form form, List<string> words, char[,] board)
    {
        if (form == null) return;
        if (words == null) return;
        if (board == null) return;
        if (words.Count == 0) return;
        if (board.Length == 0) return;

        m_form = form;
        UpdateProgressBarDelegate = ((MainForm)m_form).UpdateProgressBarMethod;
        UpdateResultsListBoxDelegate = ((MainForm)m_form).UpdateResultsListBoxMethod;

        m_words = words;
        m_board = board;

        m_cancel = false;
        m_start = DateTime.Now;
        m_duration = TimeSpan.Zero;
    }

    private DateTime m_start;
    private TimeSpan m_duration;
    public TimeSpan Duration
    {
        get { return m_duration; }
    }

    private bool m_cancel;
    public bool Cancel
    {
        get { return m_cancel; }
        set { m_cancel = value; }
    }

    private bool m_left_to_right = false;
    private Directions m_directions = Directions.None;
    private char m_placeholder = ' ';
    private bool m_neighbours_only = false;
    private bool m_search_in_dictionary = true;
    private bool m_full_width_words_only = true;
    public void Run(bool left_to_right, Directions directions, bool neighbours_only, bool search_in_dictionary, char placeholder)
    {
        if (m_form != null)
        {
            m_start = DateTime.Now;
            try
            {
                m_left_to_right = left_to_right;
                m_directions = directions;
                m_placeholder = placeholder;
                m_neighbours_only = neighbours_only;
                m_search_in_dictionary = search_in_dictionary;

                if (m_board != null)
                {
                    m_rows = m_board.GetUpperBound(0) + 1;
                    m_cols = m_board.GetUpperBound(1) + 1;
                    if ((m_rows > 0) && (m_cols > 0))
                    {
                        // mark all characters as not visited
                        bool[,] visited = new bool[m_rows, m_cols];

                        m_full_width_words_only = true;
                        for (int j = 0; j < m_cols; j++)
                        {
                            if ((m_board[0, j] == ' ') || (m_board[0, j] == m_placeholder))
                            {
                                m_full_width_words_only = false;
                                break;
                            }
                        }

                        string result = null;
                        for (int i = 0; i < m_rows; i++)
                        {
                            for (int j = 0; j < m_cols; j++)
                            {
                                m_duration = DateTime.Now - m_start;
                                // UPDATE PROGRESS
                                m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { ((i * m_cols + j) * 100) / (m_rows * m_cols) });
                                Thread.Sleep(SLEEP_TIME);

                                if (m_board[i, j] != m_placeholder)
                                {
                                    if (m_neighbours_only)
                                    {
                                        SearchWordsFromNeighboursOnly(i, j, visited, result);
                                    }
                                    else
                                    {
                                        SearchWordsForwardOnly(i, j, visited, result);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                m_cancel = true;
            }
            finally
            {
                m_duration = DateTime.Now - m_start;
                m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { 100 });
                Thread.Sleep(SLEEP_TIME);
            }
        }
    }
    private void SearchWordsForwardOnly(int i, int j, bool[,] visited, string result)
    {
        visited[i, j] = true;
        result += m_board[i, j].ToString();

        //get full-width results only
        if ((!m_full_width_words_only) ||
             (m_full_width_words_only && (result.Length == m_cols))
           )
        {
            if (!m_search_in_dictionary ||
                (m_search_in_dictionary && m_words.Contains(result.ToUpper())))
            {
                if (!result.Contains(m_placeholder.ToString()))
                {
                    m_duration = DateTime.Now - m_start;
                    // UPDATE RESULTS
                    m_form.BeginInvoke(UpdateResultsListBoxDelegate, new object[] { result });
                    // UPDATE PROGRESS
                    m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { ((i * m_cols + j) * 100) / (m_rows * m_cols) });
                    Thread.Sleep(SLEEP_TIME);
                }
            }
        }

        // TRAVERSE all cells of left-column (x, y) to current cell(i, j)
        for (int x = 0; x < m_rows; x++)
        {
            if (m_cancel) break; // x loop

            // LeftToRight: add East column
            // RightToLeft: add West column
            //for (int n = 1; n < m_cols; n++) // jump to non-adjacent
            {
                //int y = j + (m_left_to_right ? n : -n);
                int y = j + (m_left_to_right ? 1 : -1); // move forward only
                if (x >= 0 && x < m_rows && y >= 0 && y < m_cols && !visited[x, y])
                {
                    SearchWordsForwardOnly(x, y, visited, result);
                }
            }
        }

        // REMOVE character from result and mark cell as unvisited
        result = result.Remove(result.Length - 1, 1);
        visited[i, j] = false;
    }
    private void SearchWordsFromNeighboursOnly(int i, int j, bool[,] visited, string result)
    {
        visited[i, j] = true;
        result += m_board[i, j].ToString();

        // words must be in dictionary otherwise the number grows exponentially
        if (m_words.Contains(result.ToUpper()))
        {
            m_duration = DateTime.Now - m_start;
            // UPDATE RESULTS
            m_form.BeginInvoke(UpdateResultsListBoxDelegate, new object[] { result });
            // UPDATE PROGRESS
            m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { ((i * m_cols + j) * 100) / (m_rows * m_cols) });
            Thread.Sleep(SLEEP_TIME);
        }

        // TRAVERSE adjacent cells
        for (int x = i - 1; x <= (i + 1) && x < m_rows; x++)
        {
            if (m_cancel) break; // x loop
            for (int y = j - 1; y <= (j + 1) && y < m_cols; y++)
            {
                if (m_cancel) break; // y loop

                if (x >= 0 && y >= 0 && !visited[x, y])
                {
                    if (m_directions == Directions.All)
                    {
                        // no filter, allow all including zigzag
                        SearchWordsFromNeighboursOnly(x, y, visited, result);
                    }
                    else // one of 8 m_directions
                    {
                        // filter out non-included m_directions
                        if ((x == (i - 1)) && (y == (j + 0)) && ((m_directions & Directions.North) == 0x0000)) continue;
                        else if ((x == (i + 1)) && (y == (j + 0)) && ((m_directions & Directions.South) == 0x0000)) continue;
                        else if ((x == (i + 0)) && (y == (j + 1)) && ((m_directions & Directions.East) == 0x0000)) continue;
                        else if ((x == (i + 0)) && (y == (j - 1)) && ((m_directions & Directions.West) == 0x0000)) continue;
                        else if ((x == (i - 1)) && (y == (j + 1)) && ((m_directions & Directions.NorthEast) == 0x0000)) continue;
                        else if ((x == (i - 1)) && (y == (j - 1)) && ((m_directions & Directions.NorthWest) == 0x0000)) continue;
                        else if ((x == (i + 1)) && (y == (j + 1)) && ((m_directions & Directions.SouthEast) == 0x0000)) continue;
                        else if ((x == (i + 1)) && (y == (j - 1)) && ((m_directions & Directions.SouthWest) == 0x0000)) continue;

                        SearchWordsFromNeighboursOnly(x, y, visited, result);
                    }
                }
            }
        }

        // REMOVE character from result and mark cell as unvisited
        result = result.Remove(result.Length - 1, 1);
        visited[i, j] = false;
    }
}
