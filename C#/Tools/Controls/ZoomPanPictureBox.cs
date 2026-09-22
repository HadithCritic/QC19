// Original code is the DDPanBox CodeProject by Rickey Ward, 19 Feb 2010
// http://beta.codeproject.com/KB/docview/DDPanBox.aspx
// email: rickeyward@diamonddrake.com
// Added zoom on current mouse location by Ali Adams, 31 May 2010
// website http://www.heliwave.com

using System;
using System.Windows.Forms;
using System.Drawing;

public class ZoomPanPictureBox : Control
{
    public int MAX_ZOOM_FACTOR = 114;

    private bool panning = false;
    private PointF start_point;
    private PointF current_point;
    private PointF zoom_point;
    private int zoom_steps = 1;
    private float zoom_factor = 1.0F;

    public float ZoomFactor
    {
        get { return zoom_factor; }
    }

    private Image image;
    public Image Image
    {
        get { return image; }
        set
        {
            image = value;
            if (image != null)
            {
                zoom_factor = 1.0F;
                zoom_steps = 1;
                clip = PadRectangle(ClientRectangle, Padding);
                ClampClip();
            }
            Invalidate();
        }
    }

    private RectangleF clip;
    public RectangleF ClipRectangle
    {
        get { return clip; }
    }

    public Rectangle ImageRectangle
    {
        get
        {
            long w_long = (long)(clip.Width * zoom_factor);
            long h_long = (long)(clip.Height * zoom_factor);
            long x_long = (long)(-clip.X * zoom_factor);
            long y_long = (long)(-clip.Y * zoom_factor);

            int w = (int)Math.Max(1, Math.Min(int.MaxValue, w_long));
            int h = (int)Math.Max(1, Math.Min(int.MaxValue, h_long));
            int x = (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, x_long));
            int y = (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, y_long));

            return new Rectangle(x, y, w, h);
        }
    }

    public ZoomPanPictureBox()
    {
        this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
        ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

        clip = PadRectangle(ClientRectangle, Padding);
        base.OnCreateControl();
    }

    private void ClampClip()
    {
        if (image != null)
        {
            float max_width = image.Width + Padding.Horizontal;
            float max_height = image.Height + Padding.Vertical;

            if (clip.Width > max_width) clip.Width = max_width;
            if (clip.Height > max_height) clip.Height = max_height;

            float min_width = Math.Max(0.5F, ClientRectangle.Width / (float)MAX_ZOOM_FACTOR);
            float min_height = Math.Max(0.5F, ClientRectangle.Height / (float)MAX_ZOOM_FACTOR);

            if (clip.Width < min_width) clip.Width = min_width;
            if (clip.Height < min_height) clip.Height = min_height;

            float min_x = -Padding.Left;
            float min_y = -Padding.Top;
            float max_x = image.Width - clip.Width + Padding.Right;
            float max_y = image.Height - clip.Height + Padding.Bottom;

            if (image.Width < clip.Width)
                min_x = max_x = (image.Width - clip.Width) / 2F;

            if (image.Height < clip.Height)
                min_y = max_y = (image.Height - clip.Height) / 2F;

            float clamped_x = Math.Max(min_x, Math.Min(max_x, clip.X));
            float clamped_y = Math.Max(min_y, Math.Min(max_y, clip.Y));

            clip.Location = new PointF(clamped_x, clamped_y);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (image != null)
        {
            try
            {
                RectangleF source_rect = clip;
                RectangleF source_draw_rect = new RectangleF(
                    source_rect.X + Padding.Left,
                    source_rect.Y + Padding.Top,
                    source_rect.Width - Padding.Horizontal,
                    source_rect.Height - Padding.Vertical
                );

                if (source_draw_rect.Width > 0 && source_draw_rect.Height > 0)
                {
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    e.Graphics.DrawImage(image, ClientRectangle, source_draw_rect, GraphicsUnit.Pixel);
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                System.Diagnostics.Debug.WriteLine("OnPaint Error: " + ex.Message);
#endif
            }
#if (DEBUG)
            e.Graphics.DrawString("Zoom\tx" + ((zoom_factor < 2) ? zoom_factor.ToString("0.00") : zoom_factor.ToString("0")), this.Font, Brushes.Red, new PointF(5, 25));
            e.Graphics.DrawString("Step\t" + zoom_steps, this.Font, Brushes.Red, new PointF(5, 40));
#endif
        }
        base.OnPaint(e);
    }

    protected override void OnResize(EventArgs e)
    {
        if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
        {
            base.OnResize(e);
            return;
        }

        float new_clip_width = (float)ClientRectangle.Width / zoom_factor;
        float new_clip_height = (float)ClientRectangle.Height / zoom_factor;

        float dw = new_clip_width - clip.Width;
        float dh = new_clip_height - clip.Height;

        clip.X -= dw / 2.0f;
        clip.Y -= dh / 2.0f;
        clip.Width = new_clip_width;
        clip.Height = new_clip_height;

        ClampClip();
        this.Invalidate();

        base.OnResize(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            panning = true;
            start_point = new PointF(e.X, e.Y);
            current_point = start_point;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (panning && image != null)
        {
            current_point = new PointF(e.X, e.Y);
            float dx = (current_point.X - start_point.X) / zoom_factor;
            float dy = (current_point.Y - start_point.Y) / zoom_factor;
            clip.Location = new PointF(clip.X - dx, clip.Y - dy);
            start_point = current_point;

            ClampClip();
            this.Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (panning)
        {
            this.Invalidate();
            panning = false;
        }

        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!panning && image != null)
        {
            float old_zoom_factor = zoom_factor;

            if (e.Delta >= 0)
            {
                zoom_steps++;
                if (zoom_steps <= MAX_ZOOM_FACTOR)
                    zoom_factor *= 1.1F;
                else
                    zoom_steps = MAX_ZOOM_FACTOR;
            }
            else
            {
                zoom_steps--;
                if (zoom_steps >= 1)
                    zoom_factor /= 1.1F;
                else
                    zoom_steps = 1;
            }

            if (zoom_factor != old_zoom_factor)
            {
                zoom_point = new PointF(e.X, e.Y);
                ZoomOnPoint(zoom_point, zoom_factor, old_zoom_factor);
            }

            base.OnMouseWheel(e);
        }
    }

    private void ZoomOnPoint(PointF zoom_point, float new_zoom_factor, float old_zoom_factor)
    {
        double w = (double)ClientRectangle.Width / new_zoom_factor;
        double h = (double)ClientRectangle.Height / new_zoom_factor;

        double x = clip.X + zoom_point.X * ((1.0 / old_zoom_factor) - (1.0 / new_zoom_factor));
        double y = clip.Y + zoom_point.Y * ((1.0 / old_zoom_factor) - (1.0 / new_zoom_factor));

        clip = new RectangleF((float)x, (float)y, (float)w, (float)h);
        ClampClip();
        this.Invalidate();
    }

    private RectangleF PadRectangle(RectangleF rectangle, Padding padding)
    {
        rectangle.X += padding.Left;
        rectangle.Y += padding.Top;
        rectangle.Width -= padding.Horizontal;
        rectangle.Height -= padding.Vertical;
        return rectangle;
    }

    public PointF ImageToClient(PointF imagePoint)
    {
        float scale_x = ClientRectangle.Width / clip.Width;
        float scale_y = ClientRectangle.Height / clip.Height;

        float screen_x = (imagePoint.X - clip.X) * scale_x;
        float screen_y = (imagePoint.Y - clip.Y) * scale_y;
        return new PointF(screen_x, screen_y);
    }

    public PointF ClientToImage(PointF clientPoint)
    {
        float scale_x = ClientRectangle.Width / clip.Width;
        float scale_y = ClientRectangle.Height / clip.Height;

        float image_x = (clientPoint.X / scale_x) + clip.X;
        float image_y = (clientPoint.Y / scale_y) + clip.Y;
        return new PointF(image_x, image_y);
    }

    public void CenterOnImagePoint(PointF image_point)
    {
        if (image != null)
        {
            float w = (float)ClientRectangle.Width / zoom_factor;
            float h = (float)ClientRectangle.Height / zoom_factor;

            float x = image_point.X - ((float)ClientRectangle.Width / 2.0f / zoom_factor);
            float y = image_point.Y - ((float)ClientRectangle.Height / 2.0f / zoom_factor);

            clip = new RectangleF(x, y, w, h);
            ClampClip();
            Invalidate();
        }
    }
}
