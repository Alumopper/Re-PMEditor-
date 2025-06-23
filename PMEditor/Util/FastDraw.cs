namespace PMEditor.Util;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class FastDrawing
{
    public int Width { get; }
    public int Height { get; }
    
    private readonly int stride;
    private readonly IntPtr backBuffer;

    public FastDrawing(int width, int height)
    {
        Width = width;
        Height = height;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        stride = Bitmap.BackBufferStride;
        backBuffer = Bitmap.BackBuffer;
    }

    public WriteableBitmap Bitmap { get; }

    public void Clear(Color color)
    {
        Bitmap.Lock();
        unsafe
        {
            int colorData = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
            int* pBackBuffer = (int*)backBuffer;
            int pixels = Bitmap.PixelWidth * Bitmap.PixelHeight;
            for (int i = 0; i < pixels; i++)
            {
                *pBackBuffer = colorData;
                pBackBuffer++;
            }
        }
        Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
        Bitmap.Unlock();
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color color, int thickness)
    {
        Bitmap.Lock();
        unsafe
        {
            int colorData = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
            int* pBackBuffer = (int*)backBuffer;

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
                {
                    for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
                    {
                        int px = x1 + tx;
                        int py = y1 + ty;
                        if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                        {
                            *(pBackBuffer + py * stride / 4 + px) = colorData;
                        }
                    }
                }

                if (x1 == x2 && y1 == y2) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
        Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
        Bitmap.Unlock();
    }

    public void Update()
    {
        Bitmap.Lock();
        Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
        Bitmap.Unlock();
    }
    
    
    public void DrawRectangle(int x, int y, int width, int height, Color color, int borderThickness, bool fill = false)
    {
        Bitmap.Lock();
        unsafe
        {
            int colorData = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
            int* pBackBuffer = (int*)backBuffer;

            // 绘制填充
            if (fill)
            {
                for (int py = y; py < y + height; py++)
                {
                    for (int px = x; px < x + width; px++)
                    {
                        if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                        {
                            *(pBackBuffer + py * stride / 4 + px) = colorData;
                        }
                    }
                }
            }

            // 绘制边框
            for (int i = 0; i < borderThickness; i++)
            {
                // 上边
                for (int px = x; px < x + width; px++)
                {
                    int py = y + i;
                    if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                    {
                        *(pBackBuffer + py * stride / 4 + px) = colorData;
                    }
                }

                // 下边
                for (int px = x; px < x + width; px++)
                {
                    int py = y + height - 1 - i;
                    if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                    {
                        *(pBackBuffer + py * stride / 4 + px) = colorData;
                    }
                }

                // 左边
                for (int py = y; py < y + height; py++)
                {
                    int px = x + i;
                    if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                    {
                        *(pBackBuffer + py * stride / 4 + px) = colorData;
                    }
                }

                // 右边
                for (int py = y; py < y + height; py++)
                {
                    int px = x + width - 1 - i;
                    if (px >= 0 && px < Bitmap.PixelWidth && py >= 0 && py < Bitmap.PixelHeight)
                    {
                        *(pBackBuffer + py * stride / 4 + px) = colorData;
                    }
                }
            }
        }

        if(y < 0) y = 0;
        try
        {
            Bitmap.AddDirtyRect(new Int32Rect(x, y, width, height));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Bitmap.Unlock();
    }
}