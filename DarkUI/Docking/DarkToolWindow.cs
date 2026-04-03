using DarkUI.Config;
using DarkUI.Icons;
using System.ComponentModel;
using System.Drawing;
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
        private bool _isPinned = false;
        private bool _showCloseButton = true;
        private bool _showPinButton = false;

        private Rectangle _headerRect;
        private bool _shouldDrag;

        private const int HeaderButtonSpacing = 8;

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
            base.Padding = new Padding(0, Consts.ToolWindowHeaderSize, 0, 0);

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
                Height = Consts.ToolWindowHeaderSize
            };

            var nextRight = ClientRectangle.Right - 8;

            if (ShowCloseButton)
            {
                _closeButtonRect = new Rectangle
                {
                    X = nextRight - DockIcons.tw_close.Width,
                    Y = ClientRectangle.Top + (Consts.ToolWindowHeaderSize / 2) - (DockIcons.tw_close.Height / 2),
                    Width = DockIcons.tw_close.Width,
                    Height = DockIcons.tw_close.Height
                };

                nextRight = _closeButtonRect.Left - HeaderButtonSpacing;
            }
            else
            {
                _closeButtonRect = Rectangle.Empty;
            }

            if (ShowPinButton)
            {
                var pinSize = DockIcons.tw_close.Width;
                _pinButtonRect = new Rectangle
                {
                    X = nextRight - pinSize,
                    Y = ClientRectangle.Top + (Consts.ToolWindowHeaderSize / 2) - (pinSize / 2),
                    Width = pinSize,
                    Height = pinSize
                };
            }
            else
            {
                _pinButtonRect = Rectangle.Empty;
            }
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

            var isActive = IsActive();

            // Fill body
            using (var b = new SolidBrush(Colors.GreyBackground))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            // Draw header
            var bgColor = isActive ? Colors.BlueBackground : Colors.HeaderBackground;
            var darkColor = isActive ? Colors.DarkBlueBorder : Colors.DarkBorder;
            var lightColor = isActive ? Colors.LightBlueBorder : Colors.LightBorder;

            using (var b = new SolidBrush(bgColor))
            {
                var bgRect = new Rectangle(0, 0, ClientRectangle.Width, Consts.ToolWindowHeaderSize);
                g.FillRectangle(b, bgRect);
            }

            using (var p = new Pen(darkColor))
            {
                g.DrawLine(p, ClientRectangle.Left, 0, ClientRectangle.Right, 0);
                g.DrawLine(p, ClientRectangle.Left, Consts.ToolWindowHeaderSize - 1, ClientRectangle.Right, Consts.ToolWindowHeaderSize - 1);
            }

            using (var p = new Pen(lightColor))
            {
                g.DrawLine(p, ClientRectangle.Left, 1, ClientRectangle.Right, 1);
            }

            var xOffset = 10;

            if (Icon != null)
            {
                g.DrawImageUnscaled(Icon, ClientRectangle.Left + 5, ClientRectangle.Top + (Consts.ToolWindowHeaderSize / 2) - (Icon.Height / 2) + 1);
                xOffset = Icon.Width + 8;
            }

            using (var b = new SolidBrush(Colors.LightText))
            {
                var buttonsLeft = ClientRectangle.Right - 4;
                if (ShowCloseButton)
                    buttonsLeft = Math.Min(buttonsLeft, _closeButtonRect.Left);
                if (ShowPinButton)
                    buttonsLeft = Math.Min(buttonsLeft, _pinButtonRect.Left);

                var textRect = new Rectangle(xOffset, 1, Math.Max(0, buttonsLeft - 6 - xOffset), Consts.ToolWindowHeaderSize);

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
                DrawPinGlyph(g, _pinButtonRect, IsPinned);
            }

            if (ShowCloseButton)
            {
                var closeImg = _closeButtonHot ? DockIcons.tw_close_selected : DockIcons.tw_close;
                if (isActive)
                    closeImg = _closeButtonHot ? DockIcons.tw_active_close_selected : DockIcons.tw_active_close;

                g.DrawImageUnscaled(closeImg, _closeButtonRect.Left, _closeButtonRect.Top);
            }
        }

        private static void DrawPinGlyph(Graphics g, Rectangle rect, bool isPinned)
        {
            var glyphColor = Colors.LightText;
            using (var p = new Pen(glyphColor, 1.2f))
            {
                if (isPinned)
                {
                    var midX = rect.Left + (rect.Width / 2);
                    g.DrawLine(p, midX - 3, rect.Top + 4, midX + 3, rect.Top + 4);
                    g.DrawLine(p, midX, rect.Top + 4, midX, rect.Bottom - 3);
                    g.DrawLine(p, midX - 1, rect.Bottom - 3, midX + 1, rect.Bottom - 3);
                }
                else
                {
                    g.DrawLine(p, rect.Left + 3, rect.Top + 4, rect.Right - 3, rect.Bottom - 4);
                    g.DrawLine(p, rect.Left + 4, rect.Top + 4, rect.Right - 3, rect.Top + 4);
                    g.DrawLine(p, rect.Right - 3, rect.Top + 4, rect.Right - 3, rect.Bottom - 6);
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
