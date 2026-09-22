using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

public enum DrawingShape { Lines, Spiral, SquareSpiral, Square, HGoldenRect, VGoldenRect, Cube };
public enum DrawingDirection { Right, Up, Left, Down };

public static class Drawing
{
    static Drawing()
    {
        if (!Directory.Exists(Globals.DRAWINGS_FOLDER))
        {
            Directory.CreateDirectory(Globals.DRAWINGS_FOLDER);
        }
    }

    public static void DrawValues(Bitmap bitmap, List<long> values, Color color, DrawingShape shape)
    {
        if (values != null)
        {
            int count = values.Count;

            int width = 1;
            int height = 1;

            switch (shape)
            {
                case DrawingShape.Lines:
                    {
                        width = (int)Math.Round(Math.Sqrt(count + 1));
                        height = width;
                    }
                    break;
                case DrawingShape.Spiral:
                    {
                        DrawValuesSpiral(bitmap, values, color);
                        return;
                    }
                case DrawingShape.SquareSpiral:
                    {
                        DrawValuesSquareSpiral(bitmap, values, color);
                        return;
                    }
                case DrawingShape.Square:
                case DrawingShape.Cube:
                    {
                        width = (int)Math.Round(Math.Sqrt(count + 1));
                        height = width;
                    }
                    break;
                case DrawingShape.HGoldenRect:
                    {
                        width = (int)Math.Round(Math.Sqrt((count * Numbers.PHI) + 1));
                        height = (int)Math.Round(Math.Sqrt((count / Numbers.PHI) + 1));
                    }
                    break;
                case DrawingShape.VGoldenRect:
                    {
                        width = (int)Math.Round(Math.Sqrt((count / Numbers.PHI) + 1));
                        height = (int)Math.Round(Math.Sqrt((count * Numbers.PHI) + 1));
                    }
                    break;
            }

            double normalizer = 1.0D;
            long max_value = long.MinValue;
            foreach (long value in values)
            {
                if (max_value < value)
                {
                    max_value = value;
                }
            }
            if (max_value > 0L)
            {
                normalizer = (255.0 / max_value);
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                if (graphics != null)
                {
                    int dx = 1;
                    int dy = 1;
                    int x_shift = (bitmap.Width - dx * width) / 2;
                    int y_shift = (bitmap.Height - dy * height) / 2;

                    graphics.Clear(Color.Black); // set background
                    for (int i = 0; i < count; i++)
                    {
                        // draw point at new location in color shaded by numerical value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, new Rectangle(x_shift + (i % width) * dx, y_shift + (i / width) * dy, dx, dy));
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSpiral(Bitmap bitmap, List<long> values, Color color)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)Math.Round(Math.Sqrt(count) + 1);
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                if (values != null)
                {
                    double normalizer = 1.0D;
                    long max_value = long.MinValue;
                    foreach (long value in values)
                    {
                        if (max_value < value)
                        {
                            max_value = value;
                        }
                    }
                    if (max_value > 0L)
                    {
                        normalizer = (255.0 / max_value);
                    }

                    PointF[] points = new PointF[count];
                    float angle = 0.0F;
                    float scale = 0.0F;
                    for (int i = 0; i < count; i++)
                    {
                        angle = (float)((-2 * Math.PI * i) / (x + y));
                        scale = (float)i / (count / 1.0F);

                        points[i].X = (float)(x * (1 + scale * Math.Cos(angle))) + x_shift / 2;
                        points[i].Y = (float)(y * (1 + scale * Math.Sin(angle))) + y_shift / 2;

                        // draw point at new location in color shaded by numerical value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, points[i].X, points[i].Y, dx, dy);
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSquareSpiral(Bitmap bitmap, List<long> values, Color color)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)Math.Round(Math.Sqrt(count) + 1);
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                DrawingDirection direction = DrawingDirection.Right;
                int steps = 1;
                int steps_in_directoin = steps;

                if (values != null)
                {
                    double normalizer = 1.0D;
                    long max_value = long.MinValue;
                    foreach (long value in values)
                    {
                        if (max_value < value)
                        {
                            max_value = value;
                        }
                    }
                    if (max_value > 0L)
                    {
                        normalizer = (255.0 / max_value);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        // draw point at new location in color shaded by numerical value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, x, y, dx, dy);
                        }

                        // has direction finished?
                        if (steps == 0)
                        {
                            // change direction
                            switch (direction)
                            {
                                case DrawingDirection.Right: { direction = DrawingDirection.Up; } break;
                                case DrawingDirection.Up: { direction = DrawingDirection.Left; steps_in_directoin++; } break;
                                case DrawingDirection.Left: { direction = DrawingDirection.Down; } break;
                                case DrawingDirection.Down: { direction = DrawingDirection.Right; steps_in_directoin++; } break;
                            }
                            steps = steps_in_directoin;
                        }

                        // move one step in current direction
                        switch (direction)
                        {
                            case DrawingDirection.Right: x += dx; break;
                            case DrawingDirection.Up: y -= dy; break;
                            case DrawingDirection.Left: x -= dx; break;
                            case DrawingDirection.Down: y += dy; break;
                        }

                        // one step done
                        steps--;
                    }
                }
            }
        }
    }

    public static void DrawPoints(Bitmap bitmap, List<long> values, Dictionary<long, Color> colors, DrawingShape shape)
    {
        if (values != null)
        {
            int count = values.Count;

            int width = 1;
            int height = 1;

            switch (shape)
            {
                case DrawingShape.Lines:
                    {
                        width = (int)Math.Round(Math.Sqrt(count + 1));
                        height = width;
                    }
                    break;
                case DrawingShape.Spiral:
                    {
                        DrawPointsSpiral(bitmap, values, colors);
                        return;
                    }
                case DrawingShape.SquareSpiral:
                    {
                        DrawPointsSquareSpiral(bitmap, values, colors);
                        return;
                    }
                case DrawingShape.Square:
                case DrawingShape.Cube:
                    {
                        width = (int)Math.Round(Math.Sqrt(count + 1));
                        height = width;
                    }
                    break;
                case DrawingShape.HGoldenRect:
                    {
                        width = (int)Math.Round(Math.Sqrt((count * Numbers.PHI) + 1));
                        height = (int)Math.Round(Math.Sqrt((count / Numbers.PHI) + 1));
                    }
                    break;
                case DrawingShape.VGoldenRect:
                    {
                        width = (int)Math.Round(Math.Sqrt((count / Numbers.PHI) + 1));
                        height = (int)Math.Round(Math.Sqrt((count * Numbers.PHI) + 1));
                    }
                    break;
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                if (graphics != null)
                {
                    graphics.Clear(Color.Black); // set background
                    for (int i = 0; i < count; i++)
                    {
                        int dx = 1;
                        int dy = 1;
                        int x_shift = (bitmap.Width - dx * width) / 2;
                        int y_shift = (bitmap.Height - dy * height) / 2;

                        Color color = Color.Black;
                        if (colors.ContainsKey(values[i]))
                        {
                            color = colors[values[i]];
                        }

                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            graphics.FillRectangle(brush, new Rectangle(x_shift + (i % width) * dx, y_shift + (i / width) * dy, dx, dy));
                        }
                    }
                }
            }
        }
    }
    private static void DrawPointsSpiral(Bitmap bitmap, List<long> values, Dictionary<long, Color> colors)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                if (values != null)
                {
                    int count = values.Count;
                    int width = (int)(Math.Sqrt(count) + 1);
                    int height = width;
                    int dx = 1;
                    int dy = 1;
                    int x_shift = (bitmap.Width - dx * width) / 2;
                    int y_shift = (bitmap.Height - dy * height) / 2;
                    int x = (bitmap.Width - x_shift) / 2;
                    int y = (bitmap.Height - y_shift) / 2;

                    PointF[] points = new PointF[count];
                    float angle = 0.0F;
                    float scale = 0.0F;
                    for (int i = 0; i < count; i++)
                    {
                        angle = (float)((-2 * Math.PI * i) / (x + y));
                        scale = (float)i / (count / 1.0F);

                        points[i].X = (float)(x * (1 + scale * Math.Cos(angle))) + x_shift / 2;
                        points[i].Y = (float)(y * (1 + scale * Math.Sin(angle))) + y_shift / 2;

                        Color color = Color.Black;
                        if (colors.ContainsKey(values[i]))
                        {
                            color = colors[values[i]];
                        }

                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            graphics.FillRectangle(brush, points[i].X, points[i].Y, dx, dy);
                        }
                    }
                }
            }
        }
    }
    private static void DrawPointsSquareSpiral(Bitmap bitmap, List<long> values, Dictionary<long, Color> colors)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)(Math.Sqrt(count) + 1);
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                DrawingDirection direction = DrawingDirection.Right;
                int steps = 1;
                int steps_in_directoin = steps;

                if (values != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Color color = Color.Black;
                        if (colors.ContainsKey(values[i]))
                        {
                            color = colors[values[i]];
                        }

                        using (SolidBrush brush = new SolidBrush(color))
                        {
                            graphics.FillRectangle(brush, x, y, dx, dy);
                        }

                        // has direction finished?
                        if (steps == 0)
                        {
                            // change direction
                            switch (direction)
                            {
                                case DrawingDirection.Right: { direction = DrawingDirection.Up; } break;
                                case DrawingDirection.Up: { direction = DrawingDirection.Left; steps_in_directoin++; } break;
                                case DrawingDirection.Left: { direction = DrawingDirection.Down; } break;
                                case DrawingDirection.Down: { direction = DrawingDirection.Right; steps_in_directoin++; } break;
                            }
                            steps = steps_in_directoin;
                        }

                        // move one step in current direction
                        switch (direction)
                        {
                            case DrawingDirection.Right: x += dx; break;
                            case DrawingDirection.Up: y -= dy; break;
                            case DrawingDirection.Left: x -= dx; break;
                            case DrawingDirection.Down: y += dy; break;
                        }

                        // one step done
                        steps--;
                    }
                }
            }
        }
    }
    public static void DrawPageLines(Bitmap bitmap, List<List<long>> lengthss, List<List<long>> valuess, Dictionary<long, Color> colors)
    {
        if ((lengthss == null) || lengthss.Count == 0) return;
        if ((valuess == null) || valuess.Count == 0) return;

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background

                if (lengthss != null)
                {
                    int x = bitmap.Width;
                    int y = 0;
                    int width = 0;
                    int height = 1;
                    int space = 1;
                    for (int i = 0; i < lengthss.Count; i++)
                    {
                        for (int j = 0; j < lengthss[i].Count; j++)
                        {
                            width = (int)lengthss[i][j];
                            x -= (width + space);

                            Color color = Color.Black;
                            if (colors.ContainsKey(valuess[i][j]))
                            {
                                color = colors[valuess[i][j]];
                            }

                            using (SolidBrush brush = new SolidBrush(color))
                            {
                                graphics.FillRectangle(brush, new Rectangle(x, y, width, height));
                            }
                        }

                        x = bitmap.Width;
                        y += height;
                    }
                }
            }
        }
    }

    public static int WIDTH = 1200;
    public static int HEIGHT = 1200;
    public static void SaveDrawing(String path, Bitmap bitmap)
    {
        if (bitmap != null)
        {
            try
            {
                if (Directory.Exists(Globals.DRAWINGS_FOLDER))
                {
                    path = Globals.DRAWINGS_FOLDER + Path.DirectorySeparatorChar + path;
                    bitmap.Save(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace.ToString(), Application.ProductName);
            }
        }
    }
    public static void GeneratePrimeDrawings(Color color1, Color color2)
    {
        GeneratePrimeBitmaps(NumberType.AdditivePrime, color1, color2);
        GeneratePrimeBitmaps(NumberType.NonAdditivePrime, color1, color2);
        GeneratePrimeBitmaps(color1, color2);

        GeneratePrimeSpiralBitmaps(NumberType.AdditivePrime, color1, color2);
        GeneratePrimeSpiralBitmaps(NumberType.NonAdditivePrime, color1, color2);
        GeneratePrimeSpiralBitmaps(color1, color2);

        GeneratePrimeSquareSpiralBitmaps(NumberType.AdditivePrime, color1, color2);
        GeneratePrimeSquareSpiralBitmaps(NumberType.NonAdditivePrime, color1, color2);
        GeneratePrimeSquareSpiralBitmaps(color1, color2);

        if (Directory.Exists(Globals.DRAWINGS_FOLDER))
        {
            System.Diagnostics.Process.Start(Globals.DRAWINGS_FOLDER);
        }
    }

    public static void GeneratePrimeBitmaps(NumberType number_type, Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.NonAdditivePrime:
                        prime_color = color1;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                int max = WIDTH * HEIGHT;
                int prime_count = 0;
                for (int i = 0; i < max; i++)
                {
                    int x = (i % WIDTH);
                    int y = (i / WIDTH);
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel(x, y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                }

                String path = String.Format(number_type.ToString() + "_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(path, bitmap);
            }
        }
    }
    public static void GeneratePrimeSpiralBitmaps(NumberType number_type, Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.NonAdditivePrime:
                        prime_color = color1;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                int x = (bitmap.Width) / 2;
                int y = (bitmap.Height) / 2;

                int prime_count = 0;
                int max = WIDTH * HEIGHT;
                PointF[] points = new PointF[max];
                float angle = 0.0F;
                float scale = 0.0F;
                for (int i = 0; i < max; i++)
                {
                    angle = (float)((-2 * Math.PI * i) / (x + y));
                    scale = (float)i / (max / 1.0F);

                    points[i].X = (float)(x * (1 + scale * Math.Cos(angle)));
                    points[i].Y = (float)(y * (1 + scale * Math.Sin(angle)));
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel((int)points[i].X, (int)points[i].Y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel((int)points[i].X, (int)points[i].Y, Color.Black);
                    }
                }

                String path = String.Format(number_type.ToString() + "Spiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(path, bitmap);
            }
        }
    }
    public static void GeneratePrimeSquareSpiralBitmaps(NumberType number_type, Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.NonAdditivePrime:
                        prime_color = color1;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                // initialize first step
                int x = (WIDTH / 2) - 1;
                int y = (HEIGHT / 2);
                DrawingDirection direction = DrawingDirection.Right;
                int steps = 1;
                int remaining_steps = steps;

                int max = WIDTH * HEIGHT;
                int prime_count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel(x, y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }

                    // has direction finished?
                    if (remaining_steps == 0)
                    {
                        // change direction
                        switch (direction)
                        {
                            case DrawingDirection.Right: { direction = DrawingDirection.Up; } break;
                            case DrawingDirection.Up: { direction = DrawingDirection.Left; steps++; } break;
                            case DrawingDirection.Left: { direction = DrawingDirection.Down; } break;
                            case DrawingDirection.Down: { direction = DrawingDirection.Right; steps++; } break;
                        }
                        remaining_steps = steps;
                    }

                    // move one step in current direction
                    switch (direction)
                    {
                        case DrawingDirection.Right: x += 1; break;
                        case DrawingDirection.Up: y -= 1; break;
                        case DrawingDirection.Left: x -= 1; break;
                        case DrawingDirection.Down: y += 1; break;
                    }

                    // one step done
                    remaining_steps--;
                }

                String path = String.Format(number_type.ToString() + "SquareSpiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(path, bitmap);
            }
        }
    }

    public static void GeneratePrimeBitmaps(Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color color = Color.Black;
                int max = WIDTH * HEIGHT;
                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsNonAdditivePrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    int x = (i % WIDTH);
                    int y = (i / WIDTH);
                    bitmap.SetPixel(x, y, color);
                }

                String path = String.Format("Primes_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, count);
                SaveDrawing(path, bitmap);
            }
        }
    }
    public static void GeneratePrimeSpiralBitmaps(Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                int x = (bitmap.Width) / 2;
                int y = (bitmap.Height) / 2;

                int max = WIDTH * HEIGHT;
                int count = 0;
                PointF[] points = new PointF[max];
                float angle = 0.0F;
                float scale = 0.0F;
                for (int i = 0; i < max; i++)
                {
                    Color color = Color.Black;
                    if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsNonAdditivePrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    angle = (float)((-2 * Math.PI * i) / (x + y));
                    scale = (float)i / (max / 1.0F);

                    points[i].X = (float)(x * (1 + scale * Math.Cos(angle)));
                    points[i].Y = (float)(y * (1 + scale * Math.Sin(angle)));
                    bitmap.SetPixel((int)points[i].X, (int)points[i].Y, color);
                }

                String path = String.Format("PrimeSpiral_{0:000}x{1:000}_{2:0000}.bmp", bitmap.Width, bitmap.Height, count);
                SaveDrawing(path, bitmap);
            }
        }
    }
    public static void GeneratePrimeSquareSpiralBitmaps(Color color1, Color color2)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                // initialize first step
                int x = (WIDTH / 2) - 1;
                int y = (HEIGHT / 2);
                DrawingDirection direction = DrawingDirection.Right;
                int steps = 1;
                int remaining_steps = steps;

                Color color = Color.Black;
                int max = WIDTH * HEIGHT;
                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsNonAdditivePrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    bitmap.SetPixel(x, y, color);

                    // has direction finished?
                    if (remaining_steps == 0)
                    {
                        // change direction
                        switch (direction)
                        {
                            case DrawingDirection.Right: { direction = DrawingDirection.Up; } break;
                            case DrawingDirection.Up: { direction = DrawingDirection.Left; steps++; } break;
                            case DrawingDirection.Left: { direction = DrawingDirection.Down; } break;
                            case DrawingDirection.Down: { direction = DrawingDirection.Right; steps++; } break;
                        }
                        remaining_steps = steps;
                    }

                    // move one step in current direction
                    switch (direction)
                    {
                        case DrawingDirection.Right: x += 1; break;
                        case DrawingDirection.Up: y -= 1; break;
                        case DrawingDirection.Left: x -= 1; break;
                        case DrawingDirection.Down: y += 1; break;
                    }

                    // one step done
                    remaining_steps--;
                }

                String path = String.Format("PrimeSquareSpiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, count);
                SaveDrawing(path, bitmap);
            }
        }
    }
}
