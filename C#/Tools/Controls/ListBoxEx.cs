//Usage:
//listBox1.Items.Add("Item1");
//listBox1.Items.Add("Item2");
//listBox1.Items.Add("Item3");
//listBox1.SetItemColor(0, System.Drawing.Color.Aqua);
//listBox1.SetItemColor(2, System.Drawing.Color.Blue);

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//// Enable Extensions in .NET 2.0
//namespace System.Runtime.CompilerServices
//{
//    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
//    internal sealed class ExtensionAttribute : Attribute { }
//}

public class ListBoxEx : ListBox
{
    private Color highlightColor = SystemColors.Highlight;
    private Color highlightTextColor = SystemColors.HighlightText;
    private IDictionary<int, Color> colorList;

    public ListBoxEx()
    {
        DrawMode = DrawMode.OwnerDrawFixed;

        this.colorList = new Dictionary<int, Color>();
    }

    ////??? cannot override Items because non-virtual !!! why oh why Microsoft
    //protected override void Items.Clear()
    //{
    //    colorList.Clear();
    //}

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (this.Font.Height < 0)
            this.Font = Control.DefaultFont;

        if (e.Index < 0)
            return;

        if (this.Items.Count == 0)
        {
            return;
        }

        Rectangle rect = base.GetItemRectangle(e.Index);

        Color backColor = Color.Empty;
        if ((this.SelectionMode != SelectionMode.None) && ((e.State & DrawItemState.Selected) == DrawItemState.Selected))
            backColor = this.highlightColor;
        else
            backColor = this.BackColor;

        using (Brush brush = new SolidBrush(backColor))
        {
            e.Graphics.FillRectangle(brush, rect);
        }

        Color foreColor = Color.Empty;
        //if (this.SelectedIndices.Contains(e.Index))
        //{
        //    foreColor = this.highlightTextColor;
        //}
        //else
        {
            if (colorList.Count > 0)
            {
                if ((this.SelectionMode != SelectionMode.None) && ((e.State & DrawItemState.Selected) != DrawItemState.Selected))
                {
                    foreColor = GetItemColor(e.Index);

                    if (foreColor.IsEmpty)
                    {
                        foreColor = base.ForeColor;
                    }
                }
                else
                {
                    foreColor = GetItemColor(e.Index);
                }
            }
        }

        // hide hidden items even if selected
        if (foreColor == this.BackColor)
        {
            foreColor = backColor;
        }

        string text = this.Items[e.Index].ToString();

        TextRenderer.DrawText(e.Graphics, text, this.Font, rect, foreColor, TextFormatFlags.GlyphOverhangPadding);
    }

    public void ClearItemColors()
    {
        colorList.Clear();
    }

    public void SetItemColor(int index, Color color)
    {
        colorList.Add(index, color);
    }

    public Color GetItemColor(int index)
    {
        if (colorList.ContainsKey(index))
        {
            return colorList[index];
        }
        else
        {
            return base.ForeColor;
        }
    }
}
