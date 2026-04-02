using DarkUI.Config;
using DarkUI.Icons;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkScrollBar : Control
    {
        #region Event Region

        public event EventHandler<ScrollValueEventArgs> ValueChanged;

        #endregion

        #region Field Region

        private DarkScrollOrientation _scrollOrientation;

        private int _value;
        private int _minimum = 0;
        private int _maximum = 100;
        private int _viewSize;

        private Rectangle _trackArea;
        private float _viewContentRatio;

        private Rectangle _thumbArea;
        private Rectangle _upArrowArea;
        private Rectangle _downArrowArea;

        private bool _thumbHot;
        private bool _upArrowHot;
        private bool _downArrowHot;

        private bool _upArrowClicked;
        private bool _downArrowClicked;

        private bool _isScrolling;
        private int _initialValue;
        private Point _initialContact;

        private Timer _scrollTimer;

        // Aceleração progressiva do scroll por seta
        private int _scrollTimerInterval = 50;
        private const int ScrollTimerMinInterval = 10;
        private const int ScrollTimerDecrement = 2;
        private const int ScrollTimerInitialInterval = 50;

        // Animação de hover do thumb (fade suave)
        private float _thumbHoverProgress = 0f;
        private Timer _hoverAnimTimer;
        private const float HoverAnimStep = 0.12f;

        // Raio de curvatura do thumb
        private const int ThumbCornerRadius = 3;

        // Opacidade da track
        private const int TrackAlpha = 30;

        #endregion

        #region Property Region

        [Category("Behavior")]
        [Description("The orientation type of the scrollbar.")]
        [DefaultValue(DarkScrollOrientation.Vertical)]
        public DarkScrollOrientation ScrollOrientation
        {
            get { return _scrollOrientation; }
            set
            {
                _scrollOrientation = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The value that the scroll thumb position represents.")]
        [DefaultValue(0)]
        public int Value
        {
            get { return _value; }
            set
            {
                // Clamp antes de qualquer verificação para evitar recursão
                var maximumValue = Maximum - ViewSize;
                value = Math.Max(Minimum, Math.Min(value, maximumValue));

                if (_value == value)
                    return;

                _value = value;

                UpdateThumb(true);

                ValueChanged?.Invoke(this, new ScrollValueEventArgs(Value));
            }
        }

        [Category("Behavior")]
        [Description("The lower limit value of the scrollable range.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The upper limit value of the scrollable range.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The view size for the scrollable area.")]
        [DefaultValue(0)]
        public int ViewSize
        {
            get { return _viewSize; }
            set
            {
                _viewSize = value;
                UpdateScrollBar();
            }
        }

        #endregion

        #region Constructor Region

        public DarkScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            SetStyle(ControlStyles.Selectable, false);

            _scrollTimer = new Timer();
            _scrollTimer.Interval = ScrollTimerInitialInterval;
            _scrollTimer.Tick += ScrollTimerTick;

            // Timer de animação de hover (60 fps aprox.)
            _hoverAnimTimer = new Timer();
            _hoverAnimTimer.Interval = 16;
            _hoverAnimTimer.Tick += HoverAnimTick;
        }

        #endregion

        #region Dispose Region

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scrollTimer?.Stop();
                _scrollTimer?.Dispose();
                _scrollTimer = null;

                _hoverAnimTimer?.Stop();
                _hoverAnimTimer?.Dispose();
                _hoverAnimTimer = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Event Handler Region

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (_thumbArea.Contains(e.Location))
            {
                BeginThumbDrag(e.Location);
                return;
            }

            if (_upArrowArea.Contains(e.Location))
            {
                _upArrowClicked = true;
                StartScrollTimer();
                Invalidate();
                return;
            }

            if (_downArrowArea.Contains(e.Location))
            {
                _downArrowClicked = true;
                StartScrollTimer();
                Invalidate();
                return;
            }

            if (_trackArea.Contains(e.Location))
                HandleTrackClick(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isScrolling = false;
            _upArrowClicked = false;
            _downArrowClicked = false;

            ResetScrollTimer();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isScrolling)
            {
                if (e.Button != MouseButtons.Left)
                {
                    OnMouseUp(new MouseEventArgs(MouseButtons.None, 0, e.X, e.Y, 0));
                    return;
                }

                var difference = new Point(e.Location.X - _initialContact.X, e.Location.Y - _initialContact.Y);

                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                {
                    var thumbPos = _initialValue - _trackArea.Top;
                    ScrollToPhysical(thumbPos + difference.Y);
                }
                else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
                {
                    var thumbPos = _initialValue - _trackArea.Left;
                    ScrollToPhysical(thumbPos + difference.X);
                }

                UpdateScrollBar();
                return;
            }

            UpdateHoverStates(e.Location);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _thumbHot = false;
            _upArrowHot = false;
            _downArrowHot = false;

            StartHoverAnim();
        }

        private void ScrollTimerTick(object sender, EventArgs e)
        {
            if (!_upArrowClicked && !_downArrowClicked)
            {
                ResetScrollTimer();
                return;
            }

            if (_upArrowClicked)
                ScrollBy(-1);
            else if (_downArrowClicked)
                ScrollBy(1);

            if (_scrollTimer.Interval > ScrollTimerMinInterval)
            {
                _scrollTimerInterval = Math.Max(ScrollTimerMinInterval, _scrollTimerInterval - ScrollTimerDecrement);
                _scrollTimer.Interval = _scrollTimerInterval;
            }
        }

        private void HoverAnimTick(object sender, EventArgs e)
        {
            var target = (_thumbHot || _isScrolling) ? 1f : 0f;

            if (Math.Abs(_thumbHoverProgress - target) < HoverAnimStep)
            {
                _thumbHoverProgress = target;
                _hoverAnimTimer.Stop();
            }
            else
            {
                _thumbHoverProgress += target > _thumbHoverProgress ? HoverAnimStep : -HoverAnimStep;
            }

            Invalidate();
        }

        #endregion

        #region Method Region

        public void ScrollTo(int position)
        {
            Value = position;
        }

        public void ScrollToPhysical(int positionInPixels)
        {
            var isVert = _scrollOrientation == DarkScrollOrientation.Vertical;

            var trackAreaSize = isVert
                ? _trackArea.Height - _thumbArea.Height
                : _trackArea.Width - _thumbArea.Width;

            if (trackAreaSize <= 0)
            {
                Value = Minimum;
                return;
            }

            var positionRatio = Math.Min(1f, Math.Max(0f, (float)positionInPixels / trackAreaSize));
            Value = (int)(positionRatio * (Maximum - ViewSize));
        }

        public void ScrollBy(int offset)
        {
            ScrollTo(Value + offset);
        }

        public void ScrollByPhysical(int offsetInPixels)
        {
            var isVert = _scrollOrientation == DarkScrollOrientation.Vertical;

            var thumbPos = isVert
                ? _thumbArea.Top - _trackArea.Top
                : _thumbArea.Left - _trackArea.Left;

            ScrollToPhysical(thumbPos - offsetInPixels);
        }

        public void UpdateScrollBar()
        {
            var area = ClientRectangle;

            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                _upArrowArea   = new Rectangle(area.Left, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _downArrowArea = new Rectangle(area.Left, area.Bottom - Consts.ArrowButtonSize, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _trackArea     = new Rectangle(area.Left, area.Top + Consts.ArrowButtonSize, area.Width, area.Height - (Consts.ArrowButtonSize * 2));
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                _upArrowArea   = new Rectangle(area.Left, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _downArrowArea = new Rectangle(area.Right - Consts.ArrowButtonSize, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _trackArea     = new Rectangle(area.Left + Consts.ArrowButtonSize, area.Top, area.Width - (Consts.ArrowButtonSize * 2), area.Height);
            }

            UpdateThumb();
            Invalidate();
        }

        private void UpdateThumb(bool forceRefresh = false)
        {
            if (Maximum <= 0 || ViewSize < 0 || ViewSize >= Maximum)
            {
                _thumbArea = Rectangle.Empty;
                if (forceRefresh) Invalidate();
                return;
            }

            var viewAreaSize = Maximum - ViewSize;
            if (viewAreaSize <= 0)
            {
                _thumbArea = Rectangle.Empty;
                if (forceRefresh) Invalidate();
                return;
            }

            // Ajusta diretamente o campo para evitar recursão no setter
            var maximumValue = Maximum - ViewSize;
            if (_value > maximumValue)
                _value = maximumValue;

            _viewContentRatio = (float)ViewSize / Maximum;
            var positionRatio = Math.Min(1f, Math.Max(0f, (float)_value / viewAreaSize));

            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                var trackSize  = Math.Max(0, _trackArea.Height);
                var thumbSize  = CalculateThumbSize(trackSize);
                var thumbPos   = (int)((trackSize - thumbSize) * positionRatio);

                _thumbArea = new Rectangle(
                    _trackArea.Left + 3,
                    _trackArea.Top + thumbPos,
                    Consts.ScrollBarSize - 6,
                    thumbSize);
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                var trackSize  = Math.Max(0, _trackArea.Width);
                var thumbSize  = CalculateThumbSize(trackSize);
                var thumbPos   = (int)((trackSize - thumbSize) * positionRatio);

                _thumbArea = new Rectangle(
                    _trackArea.Left + thumbPos,
                    _trackArea.Top + 3,
                    thumbSize,
                    Consts.ScrollBarSize - 6);
            }

            if (forceRefresh) Invalidate();
        }

        private int CalculateThumbSize(int trackSize)
        {
            if (trackSize <= 0) return 0;

            var thumbSize = (int)(trackSize * _viewContentRatio);

            if (thumbSize < Consts.MinimumThumbSize) thumbSize = Consts.MinimumThumbSize;
            if (thumbSize > trackSize)               thumbSize = trackSize;

            return thumbSize;
        }

        private void BeginThumbDrag(Point location)
        {
            _isScrolling    = true;
            _initialContact = location;
            _initialValue   = _scrollOrientation == DarkScrollOrientation.Vertical
                ? _thumbArea.Top
                : _thumbArea.Left;

            StartHoverAnim();
        }

        private void StartScrollTimer()
        {
            _scrollTimerInterval = ScrollTimerInitialInterval;
            _scrollTimer.Interval = _scrollTimerInterval;
            _scrollTimer.Enabled = true;
        }

        private void ResetScrollTimer()
        {
            _scrollTimer.Enabled = false;
            _scrollTimerInterval = ScrollTimerInitialInterval;
            _scrollTimer.Interval = _scrollTimerInterval;
        }

        private void StartHoverAnim()
        {
            if (!_hoverAnimTimer.Enabled)
                _hoverAnimTimer.Start();
        }

        private void UpdateHoverStates(Point location)
        {
            var changed = false;

            var thumbHot     = _thumbArea.Contains(location);
            var upArrowHot   = _upArrowArea.Contains(location);
            var downArrowHot = _downArrowArea.Contains(location);

            if (_thumbHot     != thumbHot)     { _thumbHot     = thumbHot;     changed = true; StartHoverAnim(); }
            if (_upArrowHot   != upArrowHot)   { _upArrowHot   = upArrowHot;   changed = true; }
            if (_downArrowHot != downArrowHot) { _downArrowHot = downArrowHot; changed = true; }

            if (changed) Invalidate();
        }

        private void HandleTrackClick(Point location)
        {
            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                var modRect = new Rectangle(_thumbArea.Left, _trackArea.Top, _thumbArea.Width, _trackArea.Height);
                if (!modRect.Contains(location)) return;

                ScrollToPhysical(location.Y - (_upArrowArea.Bottom - 1) - (_thumbArea.Height / 2));
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                var modRect = new Rectangle(_trackArea.Left, _thumbArea.Top, _trackArea.Width, _thumbArea.Height);
                if (!modRect.Contains(location)) return;

                ScrollToPhysical(location.X - (_upArrowArea.Right - 1) - (_thumbArea.Width / 2));
            }

            _isScrolling    = true;
            _initialContact = location;
            _thumbHot       = true;
            _initialValue   = _scrollOrientation == DarkScrollOrientation.Vertical
                ? _thumbArea.Top
                : _thumbArea.Left;

            StartHoverAnim();
            Invalidate();
        }

        #endregion

        #region Paint Region

        // Retorna clone rotacionado somente quando necessário
        private static Bitmap GetArrowIcon(Bitmap source, RotateFlipType? transform)
        {
            if (!transform.HasValue)
                return source;

            var icon = (Bitmap)source.Clone();
            icon.RotateFlip(transform.Value);
            return icon;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawTrack(g);
            DrawArrow(g, _upArrowArea, isUp: true);
            DrawArrow(g, _downArrowArea, isUp: false);
            DrawThumb(g);
        }

        /// <summary>
        /// Desenha a área da track com cor sutil para dar profundidade visual.
        /// </summary>
        private void DrawTrack(Graphics g)
        {
            if (_trackArea.IsEmpty) return;

            using var trackBrush = new SolidBrush(Color.FromArgb(TrackAlpha, Colors.DarkBorder));
            using var path = RoundedRect(_trackArea, 2);
            g.FillPath(trackBrush, path);
        }

        /// <summary>
        /// Desenha o thumb com cantos arredondados e transição suave de cor via _thumbHoverProgress.
        /// </summary>
        private void DrawThumb(Graphics g)
        {
            if (!Enabled || _thumbArea == Rectangle.Empty) return;

            // Interpola entre cor padrão e hover/ativa
            var baseColor   = Colors.GreySelection;
            var hoverColor  = Colors.GreyHighlight;
            var activeColor = Colors.ActiveControl;

            Color thumbColor;

            if (_isScrolling)
            {
                thumbColor = activeColor;
            }
            else
            {
                thumbColor = Lerp(baseColor, hoverColor, _thumbHoverProgress);
            }

            using var brush = new SolidBrush(thumbColor);
            using var path  = RoundedRect(_thumbArea, ThumbCornerRadius);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Desenha uma seta (cima/baixo ou esquerda/direita) com overlay de hover/click.
        /// </summary>
        private void DrawArrow(Graphics g, Rectangle area, bool isUp)
        {
            // Overlay de hover/click no botão de seta
            if (Enabled)
            {
                if ((isUp && _upArrowClicked) || (!isUp && _downArrowClicked))
                {
                    using var clickBrush = new SolidBrush(Color.FromArgb(40, Colors.ActiveControl));
                    using var path = RoundedRect(area, 2);
                    g.FillPath(clickBrush, path);
                }
                else if ((isUp && _upArrowHot) || (!isUp && _downArrowHot))
                {
                    using var hoverBrush = new SolidBrush(Color.FromArgb(20, Colors.GreyHighlight));
                    using var path = RoundedRect(area, 2);
                    g.FillPath(hoverBrush, path);
                }
            }

            // Ícone da seta
            Bitmap icon;

            if (!Enabled)
                icon = ScrollIcons.scrollbar_arrow_disabled;
            else if ((isUp && _upArrowClicked) || (!isUp && _downArrowClicked))
                icon = ScrollIcons.scrollbar_arrow_clicked;
            else if ((isUp && _upArrowHot) || (!isUp && _downArrowHot))
                icon = ScrollIcons.scrollbar_arrow_hot;
            else
                icon = ScrollIcons.scrollbar_arrow_standard;

            RotateFlipType? transform = null;

            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                if (isUp) transform = RotateFlipType.RotateNoneFlipY;
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                transform = isUp
                    ? RotateFlipType.Rotate90FlipNone
                    : RotateFlipType.Rotate270FlipNone;
            }

            var iconCopy = GetArrowIcon(icon, transform);
            try
            {
                g.DrawImageUnscaled(
                    iconCopy,
                    area.Left + (area.Width  / 2) - (iconCopy.Width  / 2),
                    area.Top  + (area.Height / 2) - (iconCopy.Height / 2));
            }
            finally
            {
                if (transform.HasValue)
                    iconCopy.Dispose();
            }
        }

        /// <summary>
        /// Cria um GraphicsPath de retângulo com cantos arredondados.
        /// </summary>
        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            if (diameter >= rect.Width || diameter >= rect.Height || radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.Left,              rect.Top,               diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter,  rect.Top,               diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter,  rect.Bottom - diameter, diameter, diameter,   0, 90);
            path.AddArc(rect.Left,              rect.Bottom - diameter, diameter, diameter,  90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// Interpolação linear entre duas cores (para animação de hover).
        /// </summary>
        private static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        #endregion
    }
}