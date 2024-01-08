using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gomoku.Core.Helper.Behavior
{
    // 拿点击坐标
    public class GetMouseLeftButtonDownPosBehavior : Behavior<Border>
    {
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            name: "Target",
            propertyType: typeof(object),
            ownerType: typeof(GetMouseLeftButtonDownPosBehavior),
            typeMetadata: new FrameworkPropertyMetadata(null)
        );


        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
        }

        private void MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point relativePoint = Mouse.GetPosition((Border)sender);

            if (Target is not null)
            {
                ((dynamic)Target).ClickPos = relativePoint;
            }
        }
    }

    // 拿进入坐标（左键点击也会激活MouseEnter，不得行）
    public class GetMouseEnterPosBehavior : Behavior<Border>
    {
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            name: "Target",
            propertyType: typeof(object),
            ownerType: typeof(GetMouseEnterPosBehavior),
            typeMetadata: new FrameworkPropertyMetadata(null)
        );


        protected override void OnAttached()
        {
            AssociatedObject.MouseEnter += MouseEnter;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.MouseEnter -= MouseEnter;
        }

        private void MouseEnter(object sender, MouseEventArgs e)
        {
            Point relativePoint = Mouse.GetPosition((Border)sender);

            if (Target is not null)
            {
                ((dynamic)Target).EnterPos = relativePoint;
            }
        }
    }
}
