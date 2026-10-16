using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum ChartType { Bar, Line, Area }

    public class ChartData
    {
        public string Label { get; set; }
        public double Value { get; set; }

        private Color _color = Color.Empty;

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        private bool ShouldSerializeColor() => _color != Color.Empty;
        private void ResetColor() => _color = Color.Empty;

        public ChartData()
        {
            Label = "Item";
            Value = 0;
        }

        public ChartData(string label, double value, Color? color = null)
        {
            Label = label;
            Value = value;
            if (color.HasValue) _color = color.Value;
        }
    }

    [ToolboxItem(true)]
    public class NovaChart : Control
    {
        private readonly List<ChartData> _data = new List<ChartData>();
        private ChartType _chartType = ChartType.Bar;
        private Color _chartColor = Color.FromArgb(99, 102, 241);
        private Color _chartColor2 = Color.FromArgb(139, 92, 246);
        private Color _gridColor = Color.FromArgb(241, 245, 249);
        private Color _axisColor = Color.FromArgb(203, 213, 225);
        private Color _labelColor = Color.FromArgb(100, 116, 139);
        private Color _valueColor = Color.FromArgb(15, 23, 42);
        private int _paddingLeft = 50, _paddingRight = 20, _paddingTop = 20, _paddingBottom = 40;
        private bool _showGrid = true, _showValues = true, _showLabels = true, _gradientFill = true;
        private int _gridLines = 4, _barGap = 8;

        public NovaChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(500, 300);
        }

        [Category("NovaChart")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ChartData> Data => _data;

        [Category("NovaChart"), DefaultValue(ChartType.Bar)]
        public ChartType ChartType { get => _chartType; set { _chartType = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color ChartColor { get => _chartColor; set { _chartColor = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color ChartColor2 { get => _chartColor2; set { _chartColor2 = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color GridColor { get => _gridColor; set { _gridColor = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color AxisColor { get => _axisColor; set { _axisColor = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color LabelColor { get => _labelColor; set { _labelColor = value; Invalidate(); } }

        [Category("NovaChart")]
        public Color ValueColor { get => _valueColor; set { _valueColor = value; Invalidate(); } }

        [Category("NovaChart"), DefaultValue(true)]
        public bool ShowGrid { get => _showGrid; set { _showGrid = value; Invalidate(); } }

        [Category("NovaChart"), DefaultValue(true)]
        public bool ShowValues { get => _showValues; set { _showValues = value; Invalidate(); } }

        [Category("NovaChart"), DefaultValue(true)]
        public bool ShowLabels { get => _showLabels; set { _showLabels = value; Invalidate(); } }

        [Category("NovaChart"), DefaultValue(true)]
        public bool GradientFill { get => _gradientFill; set { _gradientFill = value; Invalidate(); } }

        [Category("NovaChart"), DefaultValue(4)]
        public int GridLines { get => _gridLines; set { _gridLines = Math.Max(2, value); Invalidate(); } }

        [Category("NovaChart"), DefaultValue(8)]
        public int BarGap { get => _barGap; set { _barGap = Math.Max(2, value); Invalidate(); } }

        public void AddData(string label, double value)
        {
            _data.Add(new ChartData(label, value));
            Invalidate();
        }

        public void ClearData()
        {
            _data.Clear();
            Invalidate();
        }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;
            var state = g.Save();
            g.TranslateTransform(-this.Left, -this.Top);
            using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
            {
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
            }
            g.Restore(state);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (_data.Count == 0) return;

            int cx = _paddingLeft, cy = _paddingTop;
            int cw = Width - _paddingLeft - _paddingRight;
            int ch = Height - _paddingTop - _paddingBottom;
            if (cw <= 10 || ch <= 10) return;

            double maxValue = 0;
            foreach (var d in _data)
                if (d.Value > maxValue) maxValue = d.Value;

            if (maxValue <= 0) maxValue = 1;
            maxValue *= 1.1;

            if (_showGrid)
            {
                using (var gridPen = new Pen(_gridColor, 1) { DashStyle = DashStyle.Dash })
                {
                    for (int i = 0; i <= _gridLines; i++)
                    {
                        int y = cy + ch - (i * ch / _gridLines);
                        g.DrawLine(gridPen, cx, y, cx + cw, y);
                        double val = maxValue * i / _gridLines;

                        using (var f = new Font("Segoe UI", 8F))
                        using (var b = new SolidBrush(_labelColor))
                        using (var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Far,
                            LineAlignment = StringAlignment.Center
                        })
                        {
                            g.DrawString(FormatValue(val), f, b, new RectangleF(0, y - 8, cx - 6, 16), sf);
                        }
                    }
                }
            }

            using (var axisPen = new Pen(_axisColor, 1.5f))
            {
                g.DrawLine(axisPen, cx, cy, cx, cy + ch);
                g.DrawLine(axisPen, cx, cy + ch, cx + cw, cy + ch);
            }

            switch (_chartType)
            {
                case ChartType.Bar:
                    DrawBars(g, cx, cy, cw, ch, maxValue);
                    break;
                case ChartType.Line:
                    DrawLine(g, cx, cy, cw, ch, maxValue, false);
                    break;
                case ChartType.Area:
                    DrawLine(g, cx, cy, cw, ch, maxValue, true);
                    break;
            }
        }

        private void DrawBars(Graphics g, int x, int y, int w, int h, double max)
        {
            int count = _data.Count;
            int barWidth = (w - (count + 1) * _barGap) / count;
            if (barWidth < 2) barWidth = 2;

            for (int i = 0; i < count; i++)
            {
                var item = _data[i];
                Color color = item.Color != Color.Empty ? item.Color : _chartColor;

                int barH = (int)(item.Value / max * h);
                int bx = x + _barGap + i * (barWidth + _barGap);
                int by = y + h - barH;
                var barRect = new Rectangle(bx, by, barWidth, barH);

                if (barH > 0)
                {
                    if (_gradientFill)
                    {
                        using (var brush = new LinearGradientBrush(barRect, color, _chartColor2, 90f))
                        using (var path = Round(barRect, Math.Min(6, barWidth / 2)))
                            g.FillPath(brush, path);
                    }
                    else
                    {
                        using (var path = Round(barRect, Math.Min(6, barWidth / 2)))
                        using (var b = new SolidBrush(color))
                            g.FillPath(b, path);
                    }
                }

                if (_showValues)
                {
                    var vr = new Rectangle(bx - 10, by - 22, barWidth + 20, 18);
                    using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    using (var b = new SolidBrush(_valueColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                    {
                        g.DrawString(FormatValue(item.Value), f, b, vr, sf);
                    }
                }

                if (_showLabels)
                {
                    var lr = new Rectangle(bx - 10, y + h + 6, barWidth + 20, 24);
                    using (var f = new Font("Segoe UI", 8.5F))
                    using (var b = new SolidBrush(_labelColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter
                    })
                    {
                        g.DrawString(item.Label ?? "", f, b, lr, sf);
                    }
                }
            }
        }

        private void DrawLine(Graphics g, int x, int y, int w, int h, double max, bool fillArea)
        {
            int count = _data.Count;
            if (count < 2) return;

            int step = w / (count - 1);
            var points = new PointF[count];

            for (int i = 0; i < count; i++)
            {
                int px = x + i * step;
                int py = y + h - (int)(_data[i].Value / max * h);
                points[i] = new PointF(px, py);
            }

            if (fillArea)
            {
                var area = new PointF[count + 2];
                area[0] = new PointF(points[0].X, y + h);
                Array.Copy(points, 0, area, 1, count);
                area[count + 1] = new PointF(points[count - 1].X, y + h);

                using (var brush = new LinearGradientBrush(
                    new Rectangle(x, y, w, h),
                    Color.FromArgb(120, _chartColor),
                    Color.FromArgb(10, _chartColor2),
                    90f))
                {
                    g.FillPolygon(brush, area);
                }
            }

            using (var pen = new Pen(_chartColor, 2.5f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, points);
            }

            foreach (var p in points)
            {
                using (var b = new SolidBrush(Color.White))
                    g.FillEllipse(b, p.X - 4, p.Y - 4, 8, 8);

                using (var pen = new Pen(_chartColor, 2f))
                    g.DrawEllipse(pen, p.X - 4, p.Y - 4, 8, 8);
            }

            for (int i = 0; i < count; i++)
            {
                if (_showValues)
                {
                    using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    using (var b = new SolidBrush(_valueColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                    {
                        g.DrawString(
                            FormatValue(_data[i].Value),
                            f, b,
                            new RectangleF(points[i].X - 30, points[i].Y - 24, 60, 18),
                            sf);
                    }
                }

                if (_showLabels)
                {
                    using (var f = new Font("Segoe UI", 8.5F))
                    using (var b = new SolidBrush(_labelColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    })
                    {
                        g.DrawString(
                            _data[i].Label ?? "",
                            f, b,
                            new RectangleF(points[i].X - 30, y + h + 6, 60, 24),
                            sf);
                    }
                }
            }
        }

        private static string FormatValue(double v)
        {
            if (v >= 1_000_000) return (v / 1_000_000).ToString("0.#") + "M";
            if (v >= 1_000) return (v / 1_000).ToString("0.#") + "K";
            return v.ToString("0.##");
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));

            if (d <= 0)
            {
                p.AddRectangle(r);
                return p;
            }

            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
