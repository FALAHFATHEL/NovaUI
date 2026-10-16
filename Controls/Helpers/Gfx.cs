using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    internal static class Gfx
    {
        internal static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        internal static GraphicsPath RoundTop(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }

        internal static GraphicsPath RoundBottom(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddLine(r.Right, r.Y, r.X, r.Y);
            p.CloseFigure();
            return p;
        }

        internal static Color Darken(Color c, float factor)
        {
            return Color.FromArgb(
                c.A,
                (int)Math.Max(0, c.R * factor),
                (int)Math.Max(0, c.G * factor),
                (int)Math.Max(0, c.B * factor));
        }

        internal static Color Lighten(Color c, float factor)
        {
            return Color.FromArgb(
                c.A,
                (int)Math.Min(255, c.R * factor),
                (int)Math.Min(255, c.G * factor),
                (int)Math.Min(255, c.B * factor));
        }

        internal static Color Blend(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        internal static Color GetEffectiveParentColor(Control c)
        {
            if (c == null) return SystemColors.Control;
            var p = c.Parent;
            while (p != null)
            {
                if (p.BackColor != Color.Transparent && p.BackColor != Color.Empty)
                    return p.BackColor;
                p = p.Parent;
            }
            return SystemColors.Control;
        }

        internal static Rectangle Inflate(Rectangle r, int dx, int dy)
        {
            return new Rectangle(r.X - dx, r.Y - dy, r.Width + dx * 2, r.Height + dy * 2);
        }
    }
}