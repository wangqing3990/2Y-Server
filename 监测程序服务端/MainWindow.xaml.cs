using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Threading.Timer;

namespace 监测程序服务端
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int port = 26730;
        private bool isListening = false;
        private Dictionary<IPEndPoint, DateTime> clientLastHeartbeat = new Dictionary<IPEndPoint, DateTime>();
        private List<IPEndPoint> disconnectedClients = new List<IPEndPoint>();
        private readonly object lockObject = new object();
        private Timer statusCheckTimer;
        private StackPanel stackPanel;
        //13个车站设备IP地址
        private string[,] allDeviceIPs = new string[13, 8]
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

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            _ = UDPlistenMethodAsync();

        }

        private async Task UDPlistenMethodAsync()
        {
            using (UdpClient udpServer = new UdpClient(port))
            {
                while (true)
                {
                    try
                    {
                        UdpReceiveResult receiveResult = await udpServer.ReceiveAsync();
                        byte[] receiveBytes = receiveResult.Buffer;
                        IPEndPoint remoteEndPoint = receiveResult.RemoteEndPoint;
                        string receiveString = Encoding.UTF8.GetString(receiveBytes);
                        // MessageBox.Show(receiveString);
                        await updateServerStatusAsync(receiveString, remoteEndPoint);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        private async Task updateServerStatusAsync(string message, IPEndPoint remoteEndPoint)
        {
            if (message != null)
            {
                lock (lockObject)
                {
                    clientLastHeartbeat[remoteEndPoint] = DateTime.Now;
                    List<IPEndPoint> toRemove = new List<IPEndPoint>();
                    foreach (var dc in disconnectedClients)
                    {
                        if (dc.Address.Equals(remoteEndPoint.Address))
                        {
                            toRemove.Add(dc);
                        }
                    }
                    disconnectedClients.RemoveAll(dc => toRemove.Contains(dc));
                }

                if (message.Contains("V"))
                {
                    await ProcessCustomControls(remoteEndPoint.Address.ToString(), message);
                }
                else
                {
                    var parts = message.Split(',');
                    if (parts.Length == 2)
                    {
                        var temperature = parts[0];
                        var humidity = parts[1];

                        await ProcessCustomControls(remoteEndPoint.Address.ToString(), temperature, humidity);
                    }
                }
            }
        }
        private async Task GetOfflineDeviceAsync()
        {
            DateTime now = DateTime.Now;
            await Dispatcher.InvokeAsync(() =>
            {
                List<AGMping> customControls1 = FindCustomControls<AGMping>(TCwsd);
                List<AGMshu> customControls2 = FindCustomControls<AGMshu>(TCwsd);
                List<TextBlock> customControls3 = FindCustomControls<TextBlock>(grid);

                lock (lockObject)
                {
                    if (clientLastHeartbeat != null)
                    {
                        foreach (var clientDict in clientLastHeartbeat)
                        {
                            if ((now - clientDict.Value).TotalSeconds > 5)
                            {
                                disconnectedClients.Add(clientDict.Key);
                            }
                        }
                    }

                    try
                    {
                        foreach (var client in disconnectedClients)
                        {
                            clientLastHeartbeat.Remove(client);
                            foreach (AGMping agp in customControls1)
                            {
                                if (agp.Tag != null)
                                {
                                    if (client.Address.ToString() == agp.Tag.ToString())
                                    {
                                        agp.BorderBrush = Brushes.DarkGray;
                                        agp.BorderThickness = new Thickness(1);
                                        // agp.lblTemp.Style = (Style)FindResource("AGMpingOfflineLabelStyle");
                                        // agp.lblHumi.Style = (Style)FindResource("AGMpingOfflineLabelStyle");
                                        agp.lblHumi.Foreground = Brushes.DarkGray;
                                        agp.lblTemp.Foreground = Brushes.DarkGray;
                                        agp.lblbfh.Style = (Style)FindResource("AGMpingOfflineLabelStyle");
                                        agp.lblssd.Style = (Style)FindResource("AGMpingOfflineLabelStyle");
                                    }
                                }
                            }
                            foreach (AGMshu ags in customControls2)
                            {
                                if (ags.Tag != null)
                                {
                                    if (client.Address.ToString() == ags.Tag.ToString())
                                    {
                                        ags.BorderBrush = Brushes.DarkGray;
                                        ags.BorderThickness = new Thickness(1);
                                        // ags.lblTemp.Style = (Style)FindResource("AGMshuOfflineLabelStyle");
                                        // ags.lblHumi.Style = (Style)FindResource("AGMshuOfflineLabelStyle");
                                        ags.lblHumi.Foreground = Brushes.DarkGray;
                                        ags.lblTemp.Foreground = Brushes.DarkGray;
                                        ags.lblssd.Style = (Style)FindResource("AGMshuOfflineLabelStyle");
                                        ags.lblbfh.Style = (Style)FindResource("AGMshuOfflineLabelStyle");
                                    }
                                }
                            }
                            foreach (TextBlock tb in customControls3)
                            {
                                if (tb.Tag != null && (tb.Tag.ToString() == client.Address.ToString()))
                                {
                                    tb.Foreground = Brushes.DarkGray;
                                    tb.ToolTip = $"{tb.Tag}\r\n{null}";
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            });
        }
        private List<T> FindCustomControls<T>(DependencyObject parent) where T : FrameworkElement
        {
            List<T> controls = new List<T>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    controls.Add(typedChild);
                }
                controls.AddRange(FindCustomControls<T>(child));
            }
            return controls;
        }
        // 查找并处理自定义控件
        private void ProcessCustomControls()
        {
            Dispatcher.Invoke(() =>
            {
                List<AGMping> customControls1 = FindCustomControls<AGMping>(TCwsd);
                List<AGMshu> customControls2 = FindCustomControls<AGMshu>(TCwsd);
                List<TextBlock> customControls3 = FindCustomControls<TextBlock>(grid);

                foreach (AGMping agp in customControls1)
                {
                    agp.lblname.Content = agp.Name.Substring(Math.Max(0, agp.Name.Length - 6), 6);
                }

                foreach (AGMshu ags in customControls2)
                {
                    ags.lblname.Content = ags.Name.Substring(Math.Max(0, ags.Name.Length - 6), 6);
                }
            });
        }
        private async Task ProcessCustomControls(string ip, string temp, string humi)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                List<AGMping> customControls1 = FindCustomControls<AGMping>(TCwsd);
                List<AGMshu> customControls2 = FindCustomControls<AGMshu>(TCwsd);

                foreach (AGMping agp in customControls1)
                {
                    agp.lblname.Content = agp.Name.Substring(Math.Max(0, agp.Name.Length - 6), 6);
                    if (agp.Tag.ToString() == ip)
                    {
                        agp.lblTemp.Content = temp;
                        agp.lblHumi.Content = humi;

                        agp.BorderBrush = Brushes.Black;
                        agp.BorderThickness = new Thickness(2);
                        agp.lblssd.Style = (Style)FindResource("AGMpingOnlineLabelStyle");
                        agp.lblbfh.Style = (Style)FindResource("AGMpingOnlineLabelStyle");
                        agp.lblTemp.Foreground = Convert.ToDouble(agp.lblTemp.Content.ToString()) > 40 ? Brushes.Red : Brushes.Green;
                        agp.lblHumi.Foreground = Convert.ToDouble(agp.lblHumi.Content.ToString()) > 70 ? Brushes.Red : Brushes.Green;
                    }
                }

                foreach (AGMshu ags in customControls2)
                {
                    ags.lblname.Content = ags.Name.Substring(Math.Max(0, ags.Name.Length - 6), 6);
                    if (ags.Tag.ToString() == ip)
                    {
                        ags.lblTemp.Content = temp;
                        ags.lblHumi.Content = humi;
                        ags.BorderBrush = Brushes.Black;
                        ags.BorderThickness = new Thickness(2);
                        ags.lblssd.Style = (Style)FindResource("AGMshuOnlineLabelStyle");
                        ags.lblbfh.Style = (Style)FindResource("AGMshuOnlineLabelStyle");
                        ags.lblTemp.Foreground = Convert.ToDouble(ags.lblTemp.Content.ToString()) > 40 ? Brushes.Red : Brushes.Green;
                        ags.lblHumi.Foreground = Convert.ToDouble(ags.lblHumi.Content.ToString()) > 70 ? Brushes.Red : Brushes.Green;
                    }
                }
            });
        }
        private async Task ProcessCustomControls(string ip, string version)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                List<TextBlock> customControls = FindCustomControls<TextBlock>(grid);

                foreach (TextBlock tb in customControls)
                {
                    if (tb.Tag != null && tb.Tag.ToString() == ip)
                    {
                        tb.Foreground = Brushes.Green;
                        tb.ToolTip = $"{ip}\r\n{version}";
                    }
                }
            });
        }
        private void MainForm_Load(object sender, RoutedEventArgs e)
        {

            try
            {
                CreateStackPanels();//动态创建设备图标
                statusCheckTimer = new Timer(async _ => await GetOfflineDeviceAsync(), null, 0, 5000);//检测离线定时器
            }
            catch (Exception ex)
            {
            }
        }
        private void TCwsd_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAllTabItems(TCwsd);
            // 延迟执行 ProcessCustomControls 确保所有控件都已加载
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ProcessCustomControls));

            // Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => GetOfflineDeviceAsync()));
        }
        private void TCjc_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // LoadAllTabItems(TCjc);
            // Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => GetOfflineDeviceAsync()));
        }
        private void LoadAllTabItems(TabControl tabControl)
        {
            foreach (TabItem tab in tabControl.Items)
            {
                tab.Loaded += (sender, e) => { /* 更新UI逻辑 */ };
            }
        }
        private void CreateStackPanels()
        {
            for (int i = 0; i < 13; i++)
            {
                // 创建一个新的StackPanel
                stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                // 决定每行需要创建的 TextBlock 数量
                int textBlockCount = (i == 2 || i == 12) ? 8 : ((i == 0 || i == 8) ? 8 : 6);

                // 根据不同情况创建 TextBlock
                for (int j = 0; j < textBlockCount; j++)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = (i == 2 || i == 12) ? (j < 6 ? "\ue605" : "\ue866") : (j < 4 ? "\ue605" : "\ue866"); // 设置不同的图标
                    textBlock.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#iconfont");
                    textBlock.Style = (Style)FindResource("eqTBStyle");
                    textBlock.Margin = new Thickness(41, 0, 8, 0);

                    // 设置 Tag 和 ToolTip 属性
                    string ip = allDeviceIPs[i, j];
                    textBlock.Tag = ip;
                    textBlock.ToolTip = ip;
                    textBlock.Foreground = Brushes.DarkGray;

                    // 将 TextBlock 添加到 StackPanel 中
                    stackPanel.Children.Add(textBlock);
                }

                // 将 StackPanel 添加到 Grid 中的第二列的对应行
                Grid.SetColumn(stackPanel, 1);
                Grid.SetRow(stackPanel, i);
                grid.Children.Add(stackPanel);
            }
        }
        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn == btnMin || btn == btnClose)
            {
                WindowState = WindowState.Minimized;
            }

            if (btn == btnMax)
            {
                WindowState = (WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal);
            }
        }
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            statusCheckTimer.Dispose();
        }
    }
}
