using DarkUI.Config;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System;
using System.Runtime.Versioning;

namespace DarkUI.Docking
{
    [ToolboxItem(false)]
    [SupportedOSPlatform("windows6.1")]
    public class DarkToolWindow : DarkDockContent
    {
        #region Field Region

        private Rectangle _closeButtonRect;
        private Rectangle _pinButtonRect;
        private bool _closeButtonHot = false;
        private bool _closeButtonPressed = false;
        private bool _pinButtonHot = false;
        private bool _pinButtonPressed = false;
        private bool _isPinned = true;
        private bool _showCloseButton = true;
        private bool _showPinButton = true;

        private Rectangle _headerRect;
        private bool _shouldDrag;

        private const int CornerRadius = 6;
        private const int HeaderHeight = Consts.ToolWindowHeaderSize + 3;
        private const int HeaderButtonSize = 14;
        private const int HeaderButtonRightMargin = 6;
        private const int HeaderButtonSpacing = 4;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
        }

        [Category("Behavior")]
        [Description("Indicates whether the tool window is pinned in the dock area.")]
        [DefaultValue(true)]
        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                if (_isPinned == value)
                    return;

                _isPinned = value;
                Invalidate();

                if (PinnedChanged != null)
                    PinnedChanged(this, EventArgs.Empty);
            }
        }

        [Category("Behavior")]
        [Description("Determines whether the close button is displayed in the header.")]
        [DefaultValue(true)]
        public bool ShowCloseButton
        {
            get { return _showCloseButton; }
            set
            {
                if (_showCloseButton == value)
                    return;

                _showCloseButton = value;

                if (!_showCloseButton)
                {
                    _closeButtonHot = false;
                    _closeButtonPressed = false;
                }

                UpdateHeaderLayout();
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Determines whether the pin button is displayed in the header.")]
        [DefaultValue(true)]
        public bool ShowPinButton
        {
            get { return _showPinButton; }
            set
            {
                if (_showPinButton == value)
                    return;

                _showPinButton = value;

                if (!_showPinButton)
                {
                    _pinButtonHot = false;
                    _pinButtonPressed = false;
                }

                UpdateHeaderLayout();
                Invalidate();
            }
        }

        public event EventHandler PinnedChanged;

        #endregion

        #region Constructor Region

        public DarkToolWindow()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = Colors.GreyBackground;
            // Reserve 1px around the content so the rounded frame remains visible.
            base.Padding = new Padding(1, HeaderHeight, 1, 1);

            UpdateHeaderLayout();
        }

        #endregion

        #region Method Region

        private bool IsActive()
        {
            if (DockPanel == null)
                return false;

            return DockPanel.ActiveContent == this;
        }

        private void UpdateHeaderLayout()
        {
            _headerRect = new Rectangle
            {
                X = ClientRectangle.Left,
                Y = ClientRectangle.Top,
                Width = ClientRectangle.Width,
                Height = HeaderHeight
            };

            var nextRight = ClientRectangle.Right - HeaderButtonRightMargin;

            if (ShowCloseButton)
            {
                _closeButtonRect = new Rectangle
                {
                    X = nextRight - HeaderButtonSize,
                    Y = ClientRectangle.Top + (HeaderHeight / 2) - (HeaderButtonSize / 2),
                    Width = HeaderButtonSize,
                    Height = HeaderButtonSize
                };

                nextRight = _closeButtonRect.Left - HeaderButtonSpacing;
            }
            else
            {
                _closeButtonRect = Rectangle.Empty;
            }

            if (ShowPinButton)
            {
                _pinButtonRect = new Rectangle
                {
                    X = nextRight - HeaderButtonSize,
                    Y = ClientRectangle.Top + (HeaderHeight / 2) - (HeaderButtonSize / 2),
                    Width = HeaderButtonSize,
                    Height = HeaderButtonSize
                };
            }
            else
            {
                _pinButtonRect = Rectangle.Empty;
            }

            using (var path = CreateRoundedRectPath(ClientRectangle, CornerRadius))
            {
                var oldRegion = Region;
                Region = new Region(path);

                if (oldRegion != null)
                    oldRegion.Dispose();
            }
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            var diameter = radius * 2;

            if (radius <= 0 || diameter >= rect.Width || diameter >= rect.Height)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void TogglePinned()
        {
            IsPinned = !IsPinned;
        }

        #endregion

        #region Event Handler Region

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateHeaderLayout();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var closeHot = ShowCloseButton && (_closeButtonRect.Contains(e.Location) || _closeButtonPressed);
            if (_closeButtonHot != closeHot)
            {
                _closeButtonHot = closeHot;
                Invalidate();
            }

            var pinHot = ShowPinButton && (_pinButtonRect.Contains(e.Location) || _pinButtonPressed);
            if (_pinButtonHot != pinHot)
            {
                _pinButtonHot = pinHot;
                Invalidate();
            }

            if (_shouldDrag && !_closeButtonRect.Contains(e.Location) && !_pinButtonRect.Contains(e.Location))
            {
                if (DockPanel != null)
                    DockPanel.DragContent(this);
                return;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ShowCloseButton && _closeButtonRect.Contains(e.Location))
            {
                _closeButtonPressed = true;
                _closeButtonHot = true;
                Invalidate();
                return;
            }

            if (ShowPinButton && _pinButtonRect.Contains(e.Location))
            {
                _pinButtonPressed = true;
                _pinButtonHot = true;
                Invalidate();
                return;
            }

            if (_headerRect.Contains(e.Location))
            {
                _shouldDrag = true;
                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (ShowCloseButton && _closeButtonRect.Contains(e.Location) && _closeButtonPressed)
                Close();

            if (ShowPinButton && _pinButtonRect.Contains(e.Location) && _pinButtonPressed)
                TogglePinned();

            _closeButtonPressed = false;
            _closeButtonHot = false;
            _pinButtonPressed = false;
            _pinButtonHot = false;

            _shouldDrag = false;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_closeButtonHot || _pinButtonHot)
            {
                _closeButtonHot = false;
                _pinButtonHot = false;
                Invalidate();
            }
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var isActive = IsActive();

            using (var outerPath = CreateRoundedRectPath(new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1), CornerRadius))
            {
                var bodyColor = isActive ? Colors.DarkBlueBackground : Colors.GreyBackground;

                using (var b = new SolidBrush(bodyColor))
                {
                    g.FillPath(b, outerPath);
                }

                var headerRect = new Rectangle(1, 1, Math.Max(0, ClientRectangle.Width - 2), Math.Max(0, HeaderHeight - 1));

                var state = g.Save();
                g.SetClip(outerPath);
                using (var b = new SolidBrush(Colors.HeaderBackground))
                {
                    g.FillRectangle(b, headerRect);
                }
                g.Restore(state);

                using (var p = new Pen(Colors.DarkBorder))
                {
                    g.DrawLine(p, 1, HeaderHeight - 1, ClientRectangle.Width - 2, HeaderHeight - 1);
                }

                // Subtle inner frame so the rounded body is readable across the whole component.
                using (var innerPath = CreateRoundedRectPath(new Rectangle(1, 1, ClientRectangle.Width - 3, ClientRectangle.Height - 3), Math.Max(0, CornerRadius - 1)))
                {
                    var innerColor = isActive ? Colors.DarkBlueBorder : Colors.LightBorder;
                    using (var p = new Pen(Color.FromArgb(120, innerColor)))
                    {
                        g.DrawPath(p, innerPath);
                    }
                }

                var borderColor = isActive ? Colors.BlueSelection : Colors.DarkBorder;
                using (var p = new Pen(borderColor))
                {
                    g.DrawPath(p, outerPath);
                }
            }

            var xOffset = 8;

            if (Icon != null)
            {
                g.DrawImageUnscaled(Icon, ClientRectangle.Left + 6, ClientRectangle.Top + (HeaderHeight / 2) - (Icon.Height / 2) + 1);
                xOffset = Icon.Width + 10;
            }

            using (var b = new SolidBrush(Colors.LightText))
            {
                var buttonsLeft = ClientRectangle.Right - HeaderButtonRightMargin;
                if (ShowCloseButton)
                    buttonsLeft = Math.Min(buttonsLeft, _closeButtonRect.Left);
                if (ShowPinButton)
                    buttonsLeft = Math.Min(buttonsLeft, _pinButtonRect.Left);

                var textRect = new Rectangle(xOffset, 0, Math.Max(0, (buttonsLeft - 6) - xOffset), HeaderHeight);

                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(DockText, Font, b, textRect, format);
            }

            if (ShowPinButton)
            {
                DrawHeaderButton(g, _pinButtonRect, _pinButtonHot, _pinButtonPressed, isActive);
                DrawPinGlyph(g, _pinButtonRect, IsPinned);
            }

            if (ShowCloseButton)
            {
                DrawHeaderButton(g, _closeButtonRect, _closeButtonHot, _closeButtonPressed, isActive);
                DrawCloseGlyph(g, _closeButtonRect);
            }
        }

        private static void DrawHeaderButton(Graphics g, Rectangle rect, bool isHot, bool isPressed, bool isActive)
        {
            Color fillColor;

            if (isPressed)
                fillColor = Color.FromArgb(90, isActive ? Colors.BlueSelection : Colors.GreySelection);
            else if (isHot)
                fillColor = Color.FromArgb(60, isActive ? Colors.BlueSelection : Colors.GreySelection);
            else
                fillColor = Color.FromArgb(30, Colors.DarkBorder);

            using (var path = CreateRoundedRectPath(rect, 3))
            {
                using (var b = new SolidBrush(fillColor))
                {
                    g.FillPath(b, path);
                }

                using (var p = new Pen(Color.FromArgb(120, Colors.DarkBorder)))
                {
                    g.DrawPath(p, path);
                }
            }
        }

        private static void DrawCloseGlyph(Graphics g, Rectangle rect)
        {
            using (var p = new Pen(Colors.LightText, 1.6f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;

                var pad = 4;
                g.DrawLine(p, rect.Left + pad, rect.Top + pad, rect.Right - pad, rect.Bottom - pad);
                g.DrawLine(p, rect.Right - pad, rect.Top + pad, rect.Left + pad, rect.Bottom - pad);
            }
        }

        private static void DrawPinGlyph(Graphics g, Rectangle rect, bool isPinned)
        {
            using (var p = new Pen(Colors.LightText, 1.5f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;

                if (isPinned)
                {
                    var midX = rect.Left + (rect.Width / 2);
                    g.DrawLine(p, midX - 4, rect.Top + 5, midX + 4, rect.Top + 5);
                    g.DrawLine(p, midX, rect.Top + 5, midX, rect.Bottom - 4);
                    g.DrawLine(p, midX - 2, rect.Bottom - 4, midX + 2, rect.Bottom - 4);
                }
                else
                {
                    g.DrawLine(p, rect.Left + 4, rect.Top + 5, rect.Right - 4, rect.Bottom - 5);
                    g.DrawLine(p, rect.Left + 6, rect.Top + 5, rect.Right - 4, rect.Top + 5);
                    g.DrawLine(p, rect.Right - 4, rect.Top + 5, rect.Right - 4, rect.Bottom - 7);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Absorb event
        }

        #endregion
    }
}
