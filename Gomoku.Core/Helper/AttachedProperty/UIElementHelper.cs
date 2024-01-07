using Gomoku.Core.Helper.Base;
using Gomoku.Core.Helper.Extensions;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gomoku.Core.Helper.AttachedProperty
{
    // 鼠标经过
    public partial class UIElementHelper
    {
        //
        public static readonly DependencyProperty MouseMoveAttachedProperty = DependencyProperty.RegisterAttached(
            name: "MouseMoveAttached",
            propertyType: typeof(bool),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(false, OnMouseMoveAttachedChanged)
        );
        public static bool GetMouseMoveAttached(DependencyObject target)
        {
            return (bool)target.GetValue(MouseMoveAttachedProperty);
        }
        public static void SetMouseMoveAttached(DependencyObject target, bool value)
        {
            target.SetValue(MouseMoveAttachedProperty, value);
        }
        private static void OnMouseMoveAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (GetMouseMoveAttached(element))
                {
                    element.MouseMove += OnMouseMove;
                }
                else
                {
                    element.MouseMove -= OnMouseMove;
                }
            }
        }
        private static void OnMouseMove(object s, RoutedEventArgs e)
        {
            if (s is UIElement element)
            {
                GetMouseMoveCommand(element)?.Execute(s);
            }
        }

        //
        public static readonly DependencyProperty MouseMoveCommandProperty = DependencyProperty.RegisterAttached(
            name: "MouseMoveCommand",
            propertyType: typeof(AsyncRelayCommand),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(null)
        );
        public static AsyncRelayCommand GetMouseMoveCommand(DependencyObject target)
        {
            return (AsyncRelayCommand)target.GetValue(MouseMoveCommandProperty);
        }
        public static void SetMouseMoveCommand(DependencyObject target, AsyncRelayCommand value)
        {
            target.SetValue(MouseMoveCommandProperty, value);
        }
    }

    // 鼠标左键
    public partial class UIElementHelper
    {
        //
        public static readonly DependencyProperty MouseLeftButtonDownAttachedProperty = DependencyProperty.RegisterAttached(
            name: "MouseLeftButtonDownAttached",
            propertyType: typeof(bool),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(false, OnMouseLeftButtonDownAttachedChanged)
        );
        public static bool GetMouseLeftButtonDownAttached(DependencyObject target)
        {
            return (bool)target.GetValue(MouseLeftButtonDownAttachedProperty);
        }
        public static void SetMouseLeftButtonDownAttached(DependencyObject target, bool value)
        {
            target.SetValue(MouseLeftButtonDownAttachedProperty, value);
        }
        private static void OnMouseLeftButtonDownAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (GetMouseLeftButtonDownAttached(element))
                {
                    element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
                else
                {
                    element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }
        private static void OnMouseLeftButtonDown(object s, RoutedEventArgs e)
        {
            if (s is UIElement element)
            {
                GetMouseLeftButtonDownCommand(element)?.Execute(s);
            }
        }

        //
        public static readonly DependencyProperty MouseLeftButtonDownCommandProperty = DependencyProperty.RegisterAttached(
            name: "MouseLeftButtonDownCommand",
            propertyType: typeof(AsyncRelayCommand),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(null)
        );
        public static AsyncRelayCommand GetMouseLeftButtonDownCommand(DependencyObject target)
        {
            return (AsyncRelayCommand)target.GetValue(MouseLeftButtonDownCommandProperty);
        }
        public static void SetMouseLeftButtonDownCommand(DependencyObject target, AsyncRelayCommand value)
        {
            target.SetValue(MouseLeftButtonDownCommandProperty, value);
        }
    }

    // AlternationIndex等于0的项倒计时结束后删除自身
    public partial class UIElementHelper
    {
        // 获得序号
        public static readonly DependencyProperty AlternationIndexAttachedProperty = DependencyProperty.RegisterAttached(
            name: "AlternationIndexAttached",
            propertyType: typeof(int),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(-1, OnAlternationIndexAttachedChanged)
        );
        public static int GetAlternationIndexAttached(DependencyObject target)
        {
            return (int)target.GetValue(AlternationIndexAttachedProperty);
        }
        public static void SetAlternationIndexAttached(DependencyObject target, int value)
        {
            target.SetValue(AlternationIndexAttachedProperty, value);
        }
        private static void OnAlternationIndexAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 
            if (d is UIElement element && (int)(e.NewValue) == 0)
            {
                var item = element.FindVisualAncestor<ContentPresenter>();
                var parent = element.FindVisualAncestor<ItemsControl>();

                if (item is not null && parent is not null && item.IsDescendantOf(parent))
                {
                    int cooldown = GetCountdownToRemoveAttached(element); //倒计时

                    Task.Run(async () =>
                    {
                        if (parent.ItemsSource is IList iL && iL.Count > 0)
                        {
                            var item = iL[0];
                            var hash = item!.GetHashCode();

                            await Task.Delay(cooldown);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (iL.Count > 0 && iL[0]!.GetHashCode() == hash)
                                {
                                    iL.Remove(item);
                                }
                            });
                        }
                    });
                }
            }
        }

        // 删除专用倒计时
        public static readonly DependencyProperty CountdownToRemoveAttachedProperty = DependencyProperty.RegisterAttached(
            name: "CountdownToRemoveAttached",
            propertyType: typeof(int),
            ownerType: typeof(UIElementHelper),
            defaultMetadata: new FrameworkPropertyMetadata(1000)
        );
        public static int GetCountdownToRemoveAttached(DependencyObject target)
        {
            return (int)target.GetValue(CountdownToRemoveAttachedProperty);
        }
        public static void SetCountdownToRemoveAttached(DependencyObject target, int value)
        {
            target.SetValue(CountdownToRemoveAttachedProperty, value);
        }
    }
}
