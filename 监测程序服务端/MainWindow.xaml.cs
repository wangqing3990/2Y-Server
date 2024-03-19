using System;
using System.Net.NetworkInformation;
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

        //13个车站设备IP地址
        string[,] allDeviceIPs = new string[13, 8]
        {
                { "172.22.19.176", "172.22.19.177", "172.22.19.179", "172.22.19.180", "172.22.19.161", "172.22.19.162", "172.22.19.163", "172.22.19.164" }, // qhDevIPs
                { "172.22.20.175", "172.22.20.176", "172.22.20.181", "172.22.20.182", "172.22.20.161", "172.22.20.162", "", "" }, // fxlDevIPs
                { "172.22.43.176", "172.22.43.177", "172.22.43.178", "172.22.43.179", "172.22.43.180", "172.22.43.181", "172.22.43.161", "172.22.43.162" }, // yzlDevIPs
                { "172.22.44.177", "172.22.44.178", "172.22.44.179", "172.22.44.180", "172.22.44.161", "172.22.44.162", "", "" }, // gxDevIPs
                { "172.22.45.175", "172.22.45.176", "172.22.45.181", "172.22.45.182", "172.22.45.161", "172.22.45.162", "", "" }, // gylDevIPs
                { "172.22.46.175", "172.22.46.176", "172.22.46.181", "172.22.46.182", "172.22.46.161", "172.22.46.162", "", "" }, // yshDevIPs
                { "172.22.47.175", "172.22.47.176", "172.22.47.181", "172.22.47.182", "172.22.47.161", "172.22.47.162", "", "" }, // dshnDevIPs
                { "172.22.48.175", "172.22.48.176", "172.22.48.181", "172.22.48.182", "172.22.48.161", "172.22.48.162", "", "" }, // llxzDevIPs
                { "172.22.49.175", "172.22.49.176", "172.22.49.181", "172.22.49.182", "172.22.49.161", "172.22.49.162", "172.22.49.163", "172.22.49.164" }, // ylwDevIPs
                { "172.22.50.177", "172.22.50.178", "172.22.50.179", "172.22.50.180", "172.22.50.161", "172.22.50.162", "", "" }, // stjDevIPs
                { "172.22.51.177", "172.22.51.178", "172.22.51.179", "172.22.51.180", "172.22.51.161", "172.22.51.162", "", "" }, // jglDevIPs
                { "172.22.52.175", "172.22.52.176", "172.22.52.181", "172.22.52.182", "172.22.52.161", "172.22.52.162", "", "" }, // jslDevIPs
                { "172.22.53.177", "172.22.53.178", "172.22.53.179", "172.22.53.181", "172.22.53.182", "172.22.53.183", "172.22.53.161", "172.22.53.162" } // stdDevIPs
        };
        StackPanel stackPanel;
        private void CreateStackPanels()
        {
            for (int i = 0; i < 13; i++)
            {
                // 创建一个新的StackPanel
                stackPanel = new StackPanel();
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

                        // 使用Tag属性来标识TextBlock
                        textBlock.Tag = $"{allDeviceIPs[i, j]}";
                        textBlock.ToolTip = $"{allDeviceIPs[i, j]}";

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
                        textBlock.Tag = $"{allDeviceIPs[i, j]}";
                        textBlock.ToolTip = $"{allDeviceIPs[i, j]}";
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
                        textBlock.Tag = $"{allDeviceIPs[i, j]}";
                        textBlock.ToolTip = $"{allDeviceIPs[i, j]}";
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

            //实现功能
            Ping ping = new Ping();
            int count = VisualTreeHelper.GetChildrenCount(stackPanel);
            for (int i = 0; i < allDeviceIPs.GetLength(0); i++)
            {
                for (int j = 0; j < allDeviceIPs.GetLength(1); j++)
                {
                    string ip = allDeviceIPs[i, j];
                    if (!string.IsNullOrEmpty(ip))
                    {
                        try
                        {
                            PingReply reply = ping.Send(ip);
                            Brush foregroundColor = reply.Status == IPStatus.Success ? Brushes.Green : Brushes.Red;

                            for (int k = 0; k < count; k++)
                            {
                                DependencyObject child = VisualTreeHelper.GetChild(stackPanel, k);
                                if (child is TextBlock textBlock && textBlock.Tag != null && textBlock.Tag.ToString() == ip)
                                {
                                    textBlock.Dispatcher.Invoke(() =>
                                    {
                                        textBlock.Foreground = foregroundColor;
                                    });
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
}
