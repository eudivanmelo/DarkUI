using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;

namespace DarkUI.Docking
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkDockSplitter
    {
        #region Field Region

        private Control _parentControl;
        private Control _control;

        private DarkSplitterType _splitterType;

        private int _minimum;
        private int _maximum;
        private DarkTranslucentForm _overlayForm;

        #endregion

        #region Property Region

        public Rectangle Bounds { get; set; }

        public Cursor ResizeCursor { get; private set; }

        #endregion

        #region Constructor Region

        public DarkDockSplitter(Control parentControl, Control control, DarkSplitterType splitterType)
        {
            _parentControl = parentControl;
            _control = control;
            _splitterType = splitterType;

            switch (_splitterType)
            {
                case DarkSplitterType.Left:
                case DarkSplitterType.Right:
                    ResizeCursor = Cursors.SizeWE;
                    break;
                case DarkSplitterType.Top:
                case DarkSplitterType.Bottom:
                    ResizeCursor = Cursors.SizeNS;
                    break;
            }
        }

        #endregion

        #region Method Region

        public void ShowOverlay()
        {
            _overlayForm = new DarkTranslucentForm(Color.Black);
            _overlayForm.Visible = true;

            UpdateOverlay(new Point(0, 0));
        }

        public void HideOverlay()
        {
            _overlayForm.Visible = false;
        }

        public void UpdateOverlay(Point difference)
        {
            var bounds = new Rectangle(Bounds.Location, Bounds.Size);

            switch (_splitterType)
            {
                case DarkSplitterType.Left:
                    var leftX = ClampCoordinate(bounds.Location.X - difference.X);

                    bounds.Location = new Point(leftX, bounds.Location.Y);
                    break;
                case DarkSplitterType.Right:
                    var rightX = ClampCoordinate(bounds.Location.X - difference.X);

                    bounds.Location = new Point(rightX, bounds.Location.Y);
                    break;
                case DarkSplitterType.Top:
                    var topY = ClampCoordinate(bounds.Location.Y - difference.Y);

                    bounds.Location = new Point(bounds.Location.X, topY);
                    break;
                case DarkSplitterType.Bottom:
                    var bottomY = ClampCoordinate(bounds.Location.Y - difference.Y);

                    bounds.Location = new Point(bounds.Location.X, bottomY);
                    break;
            }

            _overlayForm.Bounds = bounds;
        }

        public void Move(Point difference)
        {
            switch (_splitterType)
            {
                case DarkSplitterType.Left:
                    var targetLeftX = ClampCoordinate(Bounds.Location.X - difference.X);
                    var leftDelta = targetLeftX - Bounds.Location.X;
                    _control.Width -= leftDelta;
                    break;
                case DarkSplitterType.Right:
                    var targetRightX = ClampCoordinate(Bounds.Location.X - difference.X);
                    var rightDelta = targetRightX - Bounds.Location.X;
                    _control.Width += rightDelta;
                    break;
                case DarkSplitterType.Top:
                    var targetTopY = ClampCoordinate(Bounds.Location.Y - difference.Y);
                    var topDelta = targetTopY - Bounds.Location.Y;
                    _control.Height -= topDelta;
                    break;
                case DarkSplitterType.Bottom:
                    var targetBottomY = ClampCoordinate(Bounds.Location.Y - difference.Y);
                    var bottomDelta = targetBottomY - Bounds.Location.Y;
                    _control.Height += bottomDelta;
                    break;
            }

            UpdateBounds();
        }

        public void UpdateBounds()
        {
            var bounds = _parentControl.RectangleToScreen(_control.Bounds);
            var parentBounds = _parentControl.RectangleToScreen(_parentControl.ClientRectangle);

            var minimumWidth = Math.Max(0, _control.MinimumSize.Width);
            var minimumHeight = Math.Max(0, _control.MinimumSize.Height);
            var maximumWidth = Math.Max(0, _control.MaximumSize.Width);
            var maximumHeight = Math.Max(0, _control.MaximumSize.Height);

            switch (_splitterType)
            {
                case DarkSplitterType.Left:
                    Bounds = new Rectangle(bounds.Left - 2, bounds.Top, 5, bounds.Height);
                    _minimum = parentBounds.Left - 2;
                    if (maximumWidth > 0)
                        _minimum = Math.Max(_minimum, bounds.Right - 2 - maximumWidth);
                    _maximum = bounds.Right - 2 - minimumWidth;
                    break;
                case DarkSplitterType.Right:
                    Bounds = new Rectangle(bounds.Right - 2, bounds.Top, 5, bounds.Height);
                    _minimum = bounds.Left - 2 + minimumWidth;
                    _maximum = parentBounds.Right - 2;
                    if (maximumWidth > 0)
                        _maximum = Math.Min(_maximum, bounds.Left - 2 + maximumWidth);
                    break;
                case DarkSplitterType.Top:
                    Bounds = new Rectangle(bounds.Left, bounds.Top - 2, bounds.Width, 5);
                    _minimum = parentBounds.Top - 2;
                    if (maximumHeight > 0)
                        _minimum = Math.Max(_minimum, bounds.Bottom - 2 - maximumHeight);
                    _maximum = bounds.Bottom - 2 - minimumHeight;
                    break;
                case DarkSplitterType.Bottom:
                    Bounds = new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 5);
                    _minimum = bounds.Top - 2 + minimumHeight;
                    _maximum = parentBounds.Bottom - 2;
                    if (maximumHeight > 0)
                        _maximum = Math.Min(_maximum, bounds.Top - 2 + maximumHeight);
                    break;
            }

            if (_minimum > _maximum)
                _minimum = _maximum;
        }

        private int ClampCoordinate(int coordinate)
        {
            return Math.Max(_minimum, Math.Min(_maximum, coordinate));
        }

        #endregion
    }
}
