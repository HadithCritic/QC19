using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

public partial class ProgressBarEx : ProgressBar
{
    public ProgressBarEx()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        RenderMode = 1;
    }

    public bool Vertical { get { return this.Height > this.Width; } }
    
    private Image image;
    [Category("Appearance")]
    [Browsable(true)]
    [Description("Set image.")]
    public Image Image { get { return image; } set { image = value; Invalidate(); } }
    
    private int renderMode;
    [Category("Appearance")]
    [Browsable(true)]
    [Description("Render mode.")]
    public int RenderMode { get { return renderMode; } set { renderMode = value; Invalidate(); } }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Maximum - Minimum == 0)
        {
            base.OnPaint(e);
            return;
        }
    
        Rectangle innerRectangle = ClientRectangle;
        
        if (Vertical)
        {
            int height = (Value - Minimum) * (ClientRectangle.Height - 2) / (Maximum - Minimum);
            innerRectangle = new Rectangle(ClientRectangle.X + 1, ClientRectangle.Y + ClientRectangle.Height - 1 - height, ClientRectangle.Width - 2, height);
        }
        else
        {
            int width = (Value - Minimum) * (ClientRectangle.Width - 2) / (Maximum - Minimum);
            innerRectangle = new Rectangle(ClientRectangle.X + 1, ClientRectangle.Y + 1, width, ClientRectangle.Height - 2);
        }
        
        if (RenderMode == 0)
        {
            if (Vertical)
            {
                ProgressBarRenderer.DrawVerticalBar(e.Graphics, ClientRectangle);
                ProgressBarRenderer.DrawVerticalChunks(e.Graphics, innerRectangle);
            }
            else
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, ClientRectangle);
                ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, innerRectangle);
            }
        }
        else
        {
            int i = -1;
            if (RenderMode == 1)
            {
                i = 1;
                if (Value <= Minimum + 30 * (Maximum - Minimum) / 100) { i = 2; }
                if (Value >= Minimum + 80 * (Maximum - Minimum) / 100) { i = 3; }
            }
            else if (RenderMode == -1)
            {
                i = 1;
                if (Value <= Minimum + 30 * (Maximum - Minimum) / 100) { i = 3; }
                if (Value >= Minimum + 80 * (Maximum - Minimum) / 100) { i = 2; }
            }
            else if (RenderMode == 2) { i = 1; }
            else if (RenderMode == 3) { i = 2; }
            else if (RenderMode == 4) { i = 3; }
            if (Image != null && i >= 0)
            {
                int n = 4;
                e.Graphics.DrawImage(Image, ClientRectangle, new Rectangle(0, 0, Image.Width / n, Image.Height), GraphicsUnit.Pixel);
                e.Graphics.DrawImage(Image, innerRectangle, new Rectangle(i * Image.Width / n + 1, 1, Image.Width / n - 2, innerRectangle.Height - 2), GraphicsUnit.Pixel);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
                e.Graphics.FillRectangle(new SolidBrush(ForeColor), innerRectangle);
            }
        }
    }
}
