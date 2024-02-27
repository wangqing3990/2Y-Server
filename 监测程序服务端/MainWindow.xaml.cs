using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace 监测程序服务端
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateStackPanels();//动态创建设备图标
        }

        private void CreateStackPanels()
        {
            for (int i = 0; i < 13; i++)
            {
                // 创建一个新的StackPanel
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                // 创建并添加TextBlock到StackPanel中
                if (i == 2 || i == 12)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = j < 6 ? "\ue605" : "\ue866"; // 设置不同的图标

                        // 设置字体和样式
                        textBlock.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#iconfont");
                        textBlock.Style = (Style)FindResource("eqTBStyle");
                        textBlock.Margin = new Thickness(45, 0, 8, 0);

                        // 将TextBlock添加到StackPanel中
                        stackPanel.Children.Add(textBlock);
                    }
                }
                else if (i == 0 || i == 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = j < 4 ? "\ue605" : "\ue866";

                        textBlock.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#iconfont");
                        textBlock.Style = (Style)FindResource("eqTBStyle");
                        textBlock.Margin = new Thickness(45, 0, 1, 0);

                        stackPanel.Children.Add(textBlock);
                    }
                }
                else
                {
                    for (int j = 0; j < 6; j++)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = j < 4 ? "\ue605" : "\ue866";

                        textBlock.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#iconfont");
                        textBlock.Style = (Style)FindResource("eqTBStyle");

                        stackPanel.Children.Add(textBlock);
                    }
                }

                // 将StackPanel添加到Grid中的第二列的对应行
                Grid.SetColumn(stackPanel, 1);
                Grid.SetRow(stackPanel, i + 1);
                grid.Children.Add(stackPanel);
            }
        }

        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == btnClose)
            {
                Close();
            }
            if (btn == btnMin)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个旋转动画
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.From = 0;
            rotateAnimation.To = 360;
            rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            rotateAnimation.RepeatBehavior = new RepeatBehavior(1);

            // 创建一个旋转转换，并将其应用到TextBlock的RenderTransform属性上
            RotateTransform transform = new RotateTransform();
            refreshIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            refreshIcon.RenderTransform = transform;

            // 将动画应用到RotateTransform的Angle属性上
            transform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }
    }
}
