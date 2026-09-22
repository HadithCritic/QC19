using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// PictureBoxEx implements Zoom and Pan for PictureBox
/// </summary>
public class PictureBoxEx : Control
{
    private Image m_image = null;
    [Category("Appearance"), Description("The image to be displayed")]
    public Image Image
    {
        get { return m_image; }
        set
        {
            m_image = value;
            if (m_image != null)
            {
                // DON'T DO THIS as it overrides user changes to m_zoom_factor
                // Fit whole image to control's size
                //Graphics g = this.CreateGraphics();
                //m_zoom_factor = Math.Min(
                //    ((float)this.Height / (float)m_image.Height) * (m_image.VerticalResolution / g.DpiY),
                //    ((float)this.Width / (float)m_image.Width) * (m_image.HorizontalResolution / g.DpiX)
                //);

                // centre image in control
                m_image_x = Math.Max(0, (this.Width / 2) - (m_image.Width / 2));
                m_image_y = Math.Max(0, (this.Height / 2) - (m_image.Height / 2));

                this.Refresh();
            }
        }
    }

    private PictureBoxSizeMode m_size_mode = PictureBoxSizeMode.Zoom;
    [Category("Appearance"), Description("The size mode of the image to be displayed")]
    public PictureBoxSizeMode SizeMode
    {
        get { return m_size_mode; }
        set
        {
            m_size_mode = value;

            if (m_image != null)
            {
                switch (m_size_mode)
                {
                    case PictureBoxSizeMode.AutoSize:
                        {
                        }
                        break;
                    case PictureBoxSizeMode.CenterImage:
                        {
                        }
                        break;
                    case PictureBoxSizeMode.Normal:
                        {
                        }
                        break;
                    case PictureBoxSizeMode.StretchImage:
                        {
                            // fit image width to control's width
                            Graphics g = this.CreateGraphics();
                            m_zoom_factor = ((float)this.Width / (float)m_image.Width) * (m_image.HorizontalResolution / g.DpiX);
                        }
                        break;
                    case PictureBoxSizeMode.Zoom:
                        {
                            // fit whole image to control's size
                            Graphics g = this.CreateGraphics();
                            m_zoom_factor = Math.Min(
                                ((float)this.Height / (float)m_image.Height) * (m_image.VerticalResolution / g.DpiY),
                                ((float)this.Width / (float)m_image.Width) * (m_image.HorizontalResolution / g.DpiX)
                            );
                        }
                        break;
                }
                this.Invalidate();
            }

            OnResize(EventArgs.Empty);
        }
    }

    private float m_zoom_factor = 1.0F;
    [Category("Appearance"), Description("The zoom factor. Less than 1 to reduce. More than 1 to magnify.")]
    public float ZoomFactor
    {
        get { return m_zoom_factor; }
        set
        {
            if (value <= 0.00001F)
                value = 0.1F;
            m_zoom_factor = value;
            this.Invalidate();
        }
    }

    private InterpolationMode m_interpolation_mode = InterpolationMode.Low;
    [Category("Appearance"), Description("The interpolation mode used to smooth the drawing")]
    public InterpolationMode InterpolationMode
    {
        get { return m_interpolation_mode; }
        set { m_interpolation_mode = value; }
    }

    private SmoothingMode m_smoothing_mode = SmoothingMode.None;
    [Category("Appearance"), Description("The smoothing mode used to smooth the drawing")]
    public SmoothingMode SmoothingMode
    {
        get { return m_smoothing_mode; }
        set { m_smoothing_mode = value; }
    }

    public PictureBoxEx()
    {
        //Set up double buffering and more.
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.ResizeRedraw |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor |
                      ControlStyles.DoubleBuffer, true);

        // indicate zoom and pan are available
        this.Cursor = Cursors.Hand;

        base.OnCreateControl();
    }

    private Point m_mouse_location; // current mouse location
    private bool m_mouse_pressed;   // true as long as left mousebutton is pressed
    private int m_start_x = 0;      // x offset of image where mouse was pressed
    private int m_start_y = 0;      // y offset of image where mouse was pressed
    private int m_image_x = 0;      // current x offset of image
    private int m_image_y = 0;      // current y offset of image
    [Category("Appearance"), Description("The X loxcatin mode of the image")]
    public int Image_X
    {
        get { return m_image_x; }
    }
    [Category("Appearance"), Description("The Y loxcatin mode of the image")]
    public int Image_Y
    {
        get { return m_image_y; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (m_image != null)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.ScaleTransform(m_zoom_factor, m_zoom_factor);
            e.Graphics.DrawImage(m_image, m_image_x, m_image_y);
        }

        base.OnPaint(e);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // hide event by not overriding base
    }
    protected override void OnResize(EventArgs e)
    {
        // fit image to new size
        if (m_image != null)
        {
            Graphics g = this.CreateGraphics();

            m_zoom_factor = Math.Min(
                ((float)this.Height / (float)m_image.Height) * (m_image.VerticalResolution / g.DpiY),
                ((float)this.Width / (float)m_image.Width) * (m_image.HorizontalResolution / g.DpiX)
            );

            this.Invalidate(); // after all other messages are processed (using PostMessage not SendMessage)
        }

        base.OnResize(e);
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (!m_mouse_pressed)
            {
                m_mouse_pressed = true;
                m_mouse_location = e.Location;
                m_start_x = m_image_x;
                m_start_y = m_image_y;
            }
        }

        base.OnMouseDown(e);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Point mousePosNow = e.Location;

            int deltaX = mousePosNow.X - m_mouse_location.X; // the distance the mouse has been moved since mouse was pressed
            int deltaY = mousePosNow.Y - m_mouse_location.Y;

            m_image_x = (int)(m_start_x + (deltaX / m_zoom_factor));  // calculate new offset of image based on the current zoom factor
            m_image_y = (int)(m_start_y + (deltaY / m_zoom_factor));

            this.Invalidate(); // Invalidate self only after all other message processed using PostMessage
        }

        base.OnMouseMove(e);
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        m_mouse_pressed = false;

        base.OnMouseUp(e);
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        float max_zoom = 10000F;
        float min_zoom = 0.01F;
        float oldzoom = m_zoom_factor;          // backup old zoom factor
        float amount = m_zoom_factor * 0.2F;    // scale the zoom factor
        if (e.Delta > 0)
        {
            m_zoom_factor = Math.Min(m_zoom_factor + amount, max_zoom);
        }
        else if (e.Delta < 0)
        {
            m_zoom_factor = Math.Max(m_zoom_factor - amount, min_zoom);
        }

        Point mousePosNow = e.Location;

        int x = mousePosNow.X - this.Location.X;    // Where location of the mouse in the pictureframe
        int y = mousePosNow.Y - this.Location.Y;

        int oldimagex = (int)(x / oldzoom);  // Where in the IMAGE is it now
        int oldimagey = (int)(y / oldzoom);

        int newimagex = (int)(x / m_zoom_factor);     // Where in the IMAGE will it be when the new zoom i made
        int newimagey = (int)(y / m_zoom_factor);

        m_image_x = newimagex - oldimagex + m_image_x;  // where to move image to keep focus on one point
        m_image_y = newimagey - oldimagey + m_image_y;

        this.Invalidate();

        base.OnMouseWheel(e);
    }
    //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    //{
    //    const int WM_KEYDOWN = 0x100;
    //    const int WM_SYSKEYDOWN = 0x104;

    //    if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
    //    {
    //        switch (keyData)
    //        {
    //            case Keys.Right:
    //                m_image_x -= (int)(this.Width * 0.1F / m_zoom_factor);
    //                this.Refresh();
    //                break;

    //            case Keys.Left:
    //                m_image_x += (int)(this.Width * 0.1F / m_zoom_factor);
    //                this.Refresh();
    //                break;

    //            case Keys.Down:
    //                m_image_y -= (int)(this.Height * 0.1F / m_zoom_factor);
    //                this.Refresh();
    //                break;

    //            case Keys.Up:
    //                m_image_y += (int)(this.Height * 0.1F / m_zoom_factor);
    //                this.Refresh();
    //                break;

    //            case Keys.PageDown:
    //                m_image_y -= (int)(this.Height * 0.90F / m_zoom_factor);
    //                this.Refresh();
    //                break;

    //            case Keys.PageUp:
    //                m_image_y += (int)(this.Height * 0.90F / m_zoom_factor);
    //                this.Refresh();
    //                break;
    //        }
    //    }

    //    return base.ProcessCmdKey(ref msg, keyData);
    //}

    public void ZoomIn()
    {
        MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 1);
        OnMouseWheel(e);
    }
    public void ZoomOut()
    {
        MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, -1);
        OnMouseWheel(e);
    }
}
