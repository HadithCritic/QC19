// Source: http://www.siao2.com/2005/11/20/494829.aspx
// Modified by Ali Adams - heliwave@yahoo.com

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

public class FontBuilder
{
    // Adding a private font (Win2000 and later)
    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern IntPtr AddFontMemResourceEx(byte[] pbFont, int cbFont, IntPtr pdv, out uint pcFonts);

    // Cleanup of a private font (Win2000 and later)
    [DllImport("gdi32.dll", ExactSpelling = true)]
    internal static extern bool RemoveFontMemResourceEx(IntPtr fh);

    // Private holders of font information we are loading
    private static IntPtr s_handler = IntPtr.Zero;
    private static PrivateFontCollection s_private_font_collection = new PrivateFontCollection();
    private static FontFamily CreateFontFamily(Stream font_stream)
    {
        try
        {
            if (s_private_font_collection != null)
            {
                // First load the font as a memory stream
                if (font_stream != null)
                {
                    // 
                    // GDI+ wants a pointer to memory,
                    // GDI wants the memory.
                    // We will make them both happy.
                    // 

                    // First read the font into a buffer
                    byte[] buffer = new Byte[font_stream.Length];
                    font_stream.Read(buffer, 0, buffer.Length);

                    // Then do the unmanaged font (Windows 2000 and later)
                    // The reason this works is that GDI+ will create a font object for
                    // controls like the RichTextBox and this call will make sure that GDI
                    // recognizes the font name, later.
                    uint font_count;
                    AddFontMemResourceEx(buffer, buffer.Length, IntPtr.Zero, out font_count);

                    // Now do the managed font
                    IntPtr p_buffer = Marshal.AllocCoTaskMem(buffer.Length);
                    if (p_buffer != null)
                    {
                        // save previous families to know which is the newly added family
                        List<FontFamily> current_familieis = new List<FontFamily>(s_private_font_collection.Families);

                        // add family
                        Marshal.Copy(buffer, 0, p_buffer, buffer.Length);
                        s_private_font_collection.AddMemoryFont(p_buffer, buffer.Length);
                        Marshal.FreeCoTaskMem(p_buffer);

                        // return newly added family
                        foreach (FontFamily family in s_private_font_collection.Families)
                        {
                            if (!current_familieis.Contains(family))
                            {
                                return family;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            return null;
        }
        return null;
    }
    private static FontFamily CreateFontFamily(string font_filename)
    {
        try
        {
            if (s_private_font_collection != null)
            {
                if (!String.IsNullOrEmpty(font_filename))
                {
                    // save previous families to know which is the newly added family
                    List<FontFamily> current_familieis = new List<FontFamily>(s_private_font_collection.Families);

                    // add family
                    s_private_font_collection.AddFontFile(font_filename);

                    // return newly added family
                    foreach (FontFamily family in s_private_font_collection.Families)
                    {
                        if (!current_familieis.Contains(family))
                        {
                            return family;
                        }
                    }
                }
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

    public static Font Build(Stream font_stream, string font_name, float font_size)
    {
        Font font = null;
        if (s_private_font_collection != null)
        {
            FontFamily font_family = CreateFontFamily(font_stream);
            if (font_family != null)
            {
                font = new Font(font_family, font_size);
            }
        }
        return font;
    }
    public static Font Build(string font_path, float font_size)
    {
        Font font = null;
        if (s_private_font_collection != null)
        {
            FontFamily font_family = CreateFontFamily(font_path);
            if (font_family != null)
            {
                font = new Font(font_family, font_size);
            }
        }
        return font;
    }
}
