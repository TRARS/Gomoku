using Gomoku.Core.Helper.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Gomoku.UI.Control.CustomControlEx.ScrollViewerEx
{
    public partial class ScrollInfoAdapter : UIElement, IScrollInfo
    {
        private IScrollInfo _child;
        private double _computedVerticalOffset = 0;
        private double _computedHorizontalOffset = 0;
        private const double _scrollLineDelta = 16.0;
        //internal const double _mouseWheelDelta = 48.0;
        private double _mouseWheelDelta => ViewportHeight * 0.25;

        private double _scrollableHeight = 0.0;//记录当前可滚动高度
        //private bool _isItemChanged => (_child.ScrollOwner.ScrollableHeight != _scrollableHeight);//判断可滚动高度是否变化
        private bool _isHeightIncrease => _child.ScrollOwner.ScrollableHeight > _scrollableHeight;//只靠高度判断有点顶不顺，编辑的时候换个行就触发了- -
        private bool _isHeightDecrease => _child.ScrollOwner.ScrollableHeight < _scrollableHeight;
        private int _itemCount = 0;
        private bool _timerLock = false;

        public ScrollInfoAdapter(IScrollInfo child,
                                 Action? ScrollChangedCallback = null,
                                 Func<int>? ListViewItemChangedCallback = null)
        {
            _child = child;

            _child.ScrollOwner.ScrollChanged += (s, e) =>
            {
                var item_count = ListViewItemChangedCallback?.Invoke();
                {
                    //LogProxy.Instance.Print($"{_child.ScrollOwner.ScrollableHeight},{_scrollableHeight}");

                    if (item_count != _itemCount) { Timer(); }
                    if (_isHeightIncrease && (_timerLock))
                    {
                        //LogProxy.Instance.Print($"{_child.ScrollOwner.VerticalOffset} to {_computedVerticalOffset + _child.ScrollOwner.ScrollableHeight}");
                        //增加，用动画调整Offset滚到底
                        VerticalScroll(_computedVerticalOffset + _child.ScrollOwner.ScrollableHeight);
                    }
                    else if (_isHeightDecrease && (_timerLock))
                    {
                        //减少，直接设置Offset
                        _computedVerticalOffset = _child.ScrollOwner.VerticalOffset;
                        //LogProxy.Instance.Print($"_computedVerticalOffset 设置成 {_computedVerticalOffset}");
                    }
                    _scrollableHeight = _child.ScrollOwner.ScrollableHeight;
                    ScrollChangedCallback?.Invoke();
                }
                _itemCount = item_count ?? 0;
            };
        }
        private void Timer()
        {
            //_timerLock = true;
            //return;
            if (_timerLock is false)
            {
                _timerLock = true;
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    //意为500毫秒之后，即使_isHeightIncrease为true，也不允许执行滚到底动画
                    //使得添加新Item导致的高度变动可以触发完整的滚到底动画（500ms内）
                    //使得修改文本时换行导致的高度变动不会触发滚到底动画（500ms外）
                    //将就用，这样足够了
                    _timerLock = false;
                });
            }
        }
    }
    public partial class ScrollInfoAdapter
    {
        public bool CanVerticallyScroll
        {
            get => _child.CanVerticallyScroll;
            set => _child.CanVerticallyScroll = value;
        }
        public bool CanHorizontallyScroll
        {
            get => _child.CanHorizontallyScroll;
            set => _child.CanHorizontallyScroll = value;
        }

        public double ExtentWidth => _child.ExtentWidth;

        public double ExtentHeight => _child.ExtentHeight;

        public double ViewportWidth => _child.ViewportWidth;

        public double ViewportHeight => _child.ViewportHeight;

        public double HorizontalOffset => _child.HorizontalOffset;
        public double VerticalOffset => _child.VerticalOffset;

        public ScrollViewer ScrollOwner
        {
            get => _child.ScrollOwner;
            set => _child.ScrollOwner = value;
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return _child.MakeVisible(visual, rectangle);
        }

        #region normal
        public void LineUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineUp();
            else
                VerticalScroll(_computedVerticalOffset - _scrollLineDelta);
        }

        public void LineDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineDown();
            else
                VerticalScroll(_computedVerticalOffset + _scrollLineDelta);
        }

        public void LineLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - _scrollLineDelta);
        }

        public void LineRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.LineRight();
            else
                HorizontalScroll(_computedHorizontalOffset + _scrollLineDelta);
        }
        //
        public void MouseWheelUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelUp();
            else
            {
                //LogProxy.Instance.Print($"UP {_computedVerticalOffset}, {VerticalScrollOffset}, {_child.ScrollOwner.VerticalOffset}");
                if (_computedVerticalOffset == VerticalScrollOffset && _computedVerticalOffset == 0)
                {
                    _computedVerticalOffset = VerticalScrollOffset = _child.ScrollOwner.VerticalOffset;
                }
                VerticalScroll(_computedVerticalOffset - _mouseWheelDelta);
            }


        }
        //
        public void MouseWheelDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelDown();
            else
            {
                //LogProxy.Instance.Print($"down {_computedVerticalOffset}, {VerticalScrollOffset}, {_child.ScrollOwner.VerticalOffset}");
                if (_computedVerticalOffset == VerticalScrollOffset && _computedVerticalOffset == 0)
                {
                    _computedVerticalOffset = VerticalScrollOffset = _child.ScrollOwner.VerticalOffset;
                }
                VerticalScroll(_computedVerticalOffset + _mouseWheelDelta);
            }
        }

        public void MouseWheelLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - _mouseWheelDelta);
        }

        public void MouseWheelRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.MouseWheelRight();
            else
                HorizontalScroll(_computedHorizontalOffset + _mouseWheelDelta);
        }

        public void PageUp()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageUp();
            else
                VerticalScroll(_computedVerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageDown();
            else
                VerticalScroll(_computedVerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageLeft();
            else
                HorizontalScroll(_computedHorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.PageRight();
            else
                HorizontalScroll(_computedHorizontalOffset + ViewportWidth);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.SetHorizontalOffset(offset);
            else
            {
                _computedHorizontalOffset = offset;
                Animate(HorizontalScrollOffsetProperty, offset, 0);
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (_child.ScrollOwner.CanContentScroll == true)
                _child.SetVerticalOffset(offset);
            else
            {
                _computedVerticalOffset = offset;
                Animate(VerticalScrollOffsetProperty, offset, 0);
            }
        }
        #endregion

        #region not exposed methods
        private void Animate(DependencyProperty property, double targetValue, int duration = 300)
        {
            //make a smooth animation that starts and ends slowly
            var keyFramesAnimation = new DoubleAnimationUsingKeyFrames();
            keyFramesAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            keyFramesAnimation.KeyFrames.Add(
                new SplineDoubleKeyFrame(
                    targetValue,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)),
                    new KeySpline(0.5, 0.0, 0.5, 1.0)
                    )
                );
            BeginAnimation(property, keyFramesAnimation);
        }

        private void VerticalScroll(double val)
        {
            if (Math.Abs(_computedVerticalOffset - ValidateVerticalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedVerticalOffset = ValidateVerticalOffset(val);
                Animate(VerticalScrollOffsetProperty, _computedVerticalOffset);
                //LogProxy.Instance.Print($"{VerticalScrollOffset} -> {_computedVerticalOffset}");
            }
        }

        private void HorizontalScroll(double val)
        {
            if (Math.Abs(_computedHorizontalOffset - ValidateHorizontalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedHorizontalOffset = ValidateHorizontalOffset(val);
                Animate(HorizontalScrollOffsetProperty, _computedHorizontalOffset);
            }
        }

        private double ValidateVerticalOffset(double verticalOffset)
        {
            if (verticalOffset < 0)
                return 0;
            if (verticalOffset > _child.ScrollOwner.ScrollableHeight)
                return _child.ScrollOwner.ScrollableHeight;
            return verticalOffset;
        }

        private double ValidateHorizontalOffset(double horizontalOffset)
        {
            if (horizontalOffset < 0)
                return 0;
            if (horizontalOffset > _child.ScrollOwner.ScrollableWidth)
                return _child.ScrollOwner.ScrollableWidth;
            return horizontalOffset;
        }
        #endregion

        #region helper dependency properties as scrollbars are not animatable by default
        internal double VerticalScrollOffset
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalScrollOffsetChanged)));
        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            if (e.NewValue is not double.NaN)
            {
                smoothScrollViewer._child.SetVerticalOffset((double)e.NewValue);
            }
        }

        internal double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalScrollOffsetChanged)));
        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            smoothScrollViewer._child.SetHorizontalOffset((double)e.NewValue);
        }
        #endregion
    }

    public partial class cScrollViewer : ScrollViewer
    {
        static cScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cScrollViewer), new FrameworkPropertyMetadata(typeof(cScrollViewer)));
        }

        public cScrollViewer()
        {
            Loaded += (s, e) =>
            {
                this.ScrollInfo = new ScrollInfoAdapter(this.ScrollInfo, OnScrollChanged,
                                                                         OnListViewItemChanged);
            };
        }

        // 滚动条变化
        private void OnScrollChanged()
        {
            //
        }
        // 成员数量变化
        private int OnListViewItemChanged()
        {
            var obj = this.FindVisualAncestor<ItemsControl>();
            return obj?.Items?.Count ?? 0;
        }
    }

    public partial class cScrollViewer
    {
        public Brush BackgroundColor
        {
            get { return (Brush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            name: "BackgroundColor",
            propertyType: typeof(Brush),
            ownerType: typeof(cScrollViewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public CornerRadius BackgroundCornerRadius
        {
            get { return (CornerRadius)GetValue(BackgroundCornerRadiusProperty); }
            set { SetValue(BackgroundCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BackgroundCornerRadiusProperty = DependencyProperty.Register(
            name: "BackgroundCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cScrollViewer),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Brush ScrollBarColor
        {
            get { return (Brush)GetValue(ScrollBarColorProperty); }
            set { SetValue(ScrollBarColorProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarColorProperty = DependencyProperty.Register(
            name: "ScrollBarColor",
            propertyType: typeof(Brush),
            ownerType: typeof(cScrollViewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Gray), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Brush ScrollBarBackgroundColor
        {
            get { return (Brush)GetValue(ScrollBarBackgroundColorProperty); }
            set { SetValue(ScrollBarBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty ScrollBarBackgroundColorProperty = DependencyProperty.Register(
            name: "ScrollBarBackgroundColor",
            propertyType: typeof(Brush),
            ownerType: typeof(cScrollViewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public bool HideContent
        {
            get { return (bool)GetValue(HideContentProperty); }
            set { SetValue(HideContentProperty, value); }
        }
        public static readonly DependencyProperty HideContentProperty = DependencyProperty.Register(
            name: "HideContent",
            propertyType: typeof(bool),
            ownerType: typeof(cScrollViewer),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
