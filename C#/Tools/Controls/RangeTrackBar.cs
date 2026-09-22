using System;
using System.Windows.Forms;
using System.Drawing;

public class RangeTrackBar : Control
{
    private int m_minimum = 0;
    private int m_maximum = 100;
    private int m_lower_value = 0;
    private int m_upper_value = 100;

    // Standard TrackBar Properties
    private int m_tick_frequency = 1;
    private int m_large_change = 5;
    private int m_small_change = 1;

    private bool m_dragging_lower = false;
    private bool m_dragging_upper = false;
    private bool m_is_undecided = false; // For overlapping thumbs initial drag direction
    private int m_drag_start_x = 0;

    // Keyboard & Focus support fields
    private int m_active_thumb = 0; // 0 = Lower thumb active, 1 = Upper thumb active
    private bool m_is_focused = false;

    private const int THUMB_WIDTH = 12;
    private const int THUMB_HEIGHT = 20;

    // Events
    public event EventHandler LowerValueChanged;
    public event EventHandler UpperValueChanged;
    public event EventHandler RangeChanged;
    public event EventHandler ValueChanged;

    public RangeTrackBar()
    {
        // Enable Selectable so TabStop works out-of-the-box
        this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor |
                      ControlStyles.Selectable, true);
        this.Size = new Size(200, 30);
        this.TabStop = true;
    }

    public int Minimum
    {
        get { return m_minimum; }
        set
        {
            if (m_minimum != value)
            {
                m_minimum = value;
                if (m_minimum > m_maximum) m_maximum = m_minimum;
                if (m_lower_value < m_minimum) LowerValue = m_minimum;
                if (m_upper_value < m_minimum) UpperValue = m_minimum;
                Invalidate();
            }
        }
    }

    public int Maximum
    {
        get { return m_maximum; }
        set
        {
            if (m_maximum != value)
            {
                m_maximum = value;
                if (m_maximum < m_minimum) m_minimum = m_maximum;
                if (m_lower_value > m_maximum) LowerValue = m_maximum;
                if (m_upper_value > m_maximum) UpperValue = m_maximum;
                Invalidate();
            }
        }
    }

    public int LowerValue
    {
        get { return m_lower_value; }
        set
        {
            int clamped = Math.Max(m_minimum, Math.Min(m_maximum, value));
            if (clamped > m_upper_value) clamped = m_upper_value; // Cannot exceed upper

            if (m_lower_value != clamped)
            {
                m_lower_value = clamped;
                OnLowerValueChanged(EventArgs.Empty);
                OnRangeChanged(EventArgs.Empty);
                OnValueChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public int UpperValue
    {
        get { return m_upper_value; }
        set
        {
            int clamped = Math.Max(m_minimum, Math.Min(m_maximum, value));
            if (clamped < m_lower_value) clamped = m_lower_value; // Cannot drop below lower

            if (m_upper_value != clamped)
            {
                m_upper_value = clamped;
                OnUpperValueChanged(EventArgs.Empty);
                OnRangeChanged(EventArgs.Empty);
                OnValueChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public int TickFrequency
    {
        get { return m_tick_frequency; }
        set
        {
            if (value > 0 && m_tick_frequency != value)
            {
                m_tick_frequency = value;
                Invalidate();
            }
        }
    }

    public int LargeChange
    {
        get { return m_large_change; }
        set
        {
            if (value > 0)
            {
                m_large_change = value;
            }
        }
    }

    public int SmallChange
    {
        get { return m_small_change; }
        set
        {
            if (value > 0)
            {
                m_small_change = value;
            }
        }
    }

    protected virtual void OnLowerValueChanged(EventArgs e)
    {
        if (LowerValueChanged != null) LowerValueChanged(this, e);
    }

    protected virtual void OnUpperValueChanged(EventArgs e)
    {
        if (UpperValueChanged != null) UpperValueChanged(this, e);
    }

    protected virtual void OnRangeChanged(EventArgs e)
    {
        if (RangeChanged != null) RangeChanged(this, e);
    }

    protected virtual void OnValueChanged(EventArgs e)
    {
        if (ValueChanged != null) ValueChanged(this, e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        m_is_focused = true;
        Invalidate();
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        m_is_focused = false;
        Invalidate();
        base.OnLostFocus(e);
    }

    private Rectangle GetChannelRect()
    {
        int trackY = ClientRectangle.Height / 2 - 2;
        return new Rectangle(THUMB_WIDTH / 2, trackY, ClientRectangle.Width - THUMB_WIDTH, 4);
    }

    private Rectangle GetLowerThumbRect()
    {
        int x = ValueToX(m_lower_value) - THUMB_WIDTH / 2;
        int y = ClientRectangle.Height / 2 - THUMB_HEIGHT / 2;
        return new Rectangle(x, y, THUMB_WIDTH, THUMB_HEIGHT);
    }

    private Rectangle GetUpperThumbRect()
    {
        int x = ValueToX(m_upper_value) - THUMB_WIDTH / 2;
        int y = ClientRectangle.Height / 2 - THUMB_HEIGHT / 2;
        return new Rectangle(x, y, THUMB_WIDTH, THUMB_HEIGHT);
    }

    private int ValueToX(int val)
    {
        if (m_maximum == m_minimum) return THUMB_WIDTH / 2;
        int usableWidth = ClientRectangle.Width - THUMB_WIDTH;
        float range = (float)(m_maximum - m_minimum);
        return THUMB_WIDTH / 2 + (int)((val - m_minimum) / range * usableWidth);
    }

    private int XToValue(int x)
    {
        int usableWidth = ClientRectangle.Width - THUMB_WIDTH;
        if (usableWidth <= 0) return m_minimum;
        int offsetX = x - THUMB_WIDTH / 2;
        if (offsetX < 0) offsetX = 0;
        if (offsetX > usableWidth) offsetX = usableWidth;

        float range = (float)(m_maximum - m_minimum);
        return m_minimum + (int)((offsetX / (float)usableWidth) * range + 0.5f);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (e != null && e.Graphics != null)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw Track Channel
            Rectangle channel = GetChannelRect();
            e.Graphics.FillRectangle(Brushes.LightGray, channel);
            e.Graphics.DrawRectangle(Pens.DarkGray, channel);

            // Draw Selected Range Highlight
            int x1 = ValueToX(m_lower_value);
            int x2 = ValueToX(m_upper_value);
            if (x2 > x1)
            {
                Rectangle rangeRect = new Rectangle(x1, channel.Y, x2 - x1, channel.Height);
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, rangeRect);
            }

            // Draw Thumbs
            Rectangle lowerThumb = GetLowerThumbRect();
            Rectangle upperThumb = GetUpperThumbRect();

            e.Graphics.FillRectangle(Brushes.White, lowerThumb);
            e.Graphics.DrawRectangle((m_is_focused && m_active_thumb == 0) ? Pens.Blue : Pens.Black, lowerThumb);

            e.Graphics.FillRectangle(Brushes.White, upperThumb);
            e.Graphics.DrawRectangle((m_is_focused && m_active_thumb == 1) ? Pens.Blue : Pens.Black, upperThumb);

            // Draw Focus Rectangle around control if focused
            if (m_is_focused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
            }
        }
        base.OnPaint(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e != null && e.Button == MouseButtons.Left)
        {
            // Take focus when clicked
            Focus();

            Rectangle lowerRect = GetLowerThumbRect();
            Rectangle upperRect = GetUpperThumbRect();

            bool hitLower = lowerRect.Contains(e.Location);
            bool hitUpper = upperRect.Contains(e.Location);

            if (hitLower && hitUpper)
            {
                // Bug fix case: Both thumbs are at the exact same location.
                m_is_undecided = true;
                m_drag_start_x = e.X;
            }
            else if (hitLower)
            {
                m_dragging_lower = true;
                m_active_thumb = 0;
                m_is_undecided = false;
            }
            else if (hitUpper)
            {
                m_dragging_upper = true;
                m_active_thumb = 1;
                m_is_undecided = false;
            }
            else
            {
                // Clicked on channel: snap whichever thumb is closer
                int clickedVal = XToValue(e.X);
                int distLower = Math.Abs(clickedVal - m_lower_value);
                int distUpper = Math.Abs(clickedVal - m_upper_value);

                if (distLower <= distUpper)
                {
                    LowerValue = clickedVal;
                    m_dragging_lower = true;
                    m_active_thumb = 0;
                }
                else
                {
                    UpperValue = clickedVal;
                    m_dragging_upper = true;
                    m_active_thumb = 1;
                }
                m_is_undecided = false;
            }
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e != null)
        {
            if (m_is_undecided)
            {
                // Thumbs were overlapping; determine active thumb based on drag direction
                if (e.X < m_drag_start_x)
                {
                    m_dragging_lower = true;
                    m_active_thumb = 0;
                    m_is_undecided = false;
                }
                else if (e.X > m_drag_start_x)
                {
                    m_dragging_upper = true;
                    m_active_thumb = 1;
                    m_is_undecided = false;
                }
            }

            if (m_dragging_lower)
            {
                LowerValue = XToValue(e.X);
            }
            else if (m_dragging_upper)
            {
                UpperValue = XToValue(e.X);
            }
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e != null && e.Button == MouseButtons.Left)
        {
            m_dragging_lower = false;
            m_dragging_upper = false;
            m_is_undecided = false;
        }
        base.OnMouseUp(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Handle Arrow keys and Tab navigation explicitly
        if (m_is_focused)
        {
            int change = (ModifierKeys == Keys.Control) ? m_large_change : m_small_change;

            if (keyData == Keys.Left)
            {
                if (m_active_thumb == 0) LowerValue -= change;
                else UpperValue -= change;
                return true;
            }
            else if (keyData == Keys.Right)
            {
                if (m_active_thumb == 0) LowerValue += change;
                else UpperValue += change;
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                // Switch active thumb between Lower (0) and Upper (1) using Tab key
                m_active_thumb = (m_active_thumb == 0) ? 1 : 0;
                Invalidate();
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.Tab))
            {
                // Switch active thumb backwards or let focus leave control if on lower
                if (m_active_thumb == 1)
                {
                    m_active_thumb = 0;
                    Invalidate();
                    return true;
                }
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
