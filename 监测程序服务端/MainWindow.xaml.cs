using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        TcpListener serverSocket;
        TcpClient clientSocket;

        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void MainForm_Load(object sender, RoutedEventArgs e)
        {
            CreateStackPanels();//动态创建设备图标
            try
            {
                initServerSocket();

                // 启动监听客户端连接
                ListenForClients();
            }
            catch (Exception exception)
            {
            }
            
            lbVersion.Content = $"版本号：V{Assembly.GetEntryAssembly()?.GetName().Version}";
        }

        private void initServerSocket()
        {
            // 初始化服务端Socket监听
            string ipAddress = "172.22.50.3";

            int port = 49200;
            serverSocket = new TcpListener(IPAddress.Parse(ipAddress), port);
            serverSocket.Start();
        }

        // 定义一个字典，用于存储客户端 IP 地址与对应的 TcpClient 对象
        private Dictionary<string, TcpClient> clientDictionary = new Dictionary<string, TcpClient>();

        // 方法：根据客户端 IP 地址获取对应的 TcpClient 对象

        // 获取客户端 IP 地址的方法
        private async Task<string> GetClientIpAddressAsync(TcpClient clientSocket)
        {
            return await Task.Run(() =>
            {
                var clientEndPoint = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                return clientEndPoint.Address.ToString();
            });
        }

        private bool isListening = false;
        private Thread listenThread;

        //监听客户端连接
        private async Task ListenForClients()
        {
            isListening = true;
            while (isListening)
            {
                try
                {
                    // 等待客户端连接
                    clientSocket = await serverSocket.AcceptTcpClientAsync();

                    // 获取客户端的 IP 地址并添加到列表中
                    var clientIpAddress = await GetClientIpAddressAsync(clientSocket);
                    clientDictionary.Add(clientIpAddress, clientSocket);

                    // 记录客户端最后一次活动的时间
                    // lastActivityTime[clientIpAddress] = DateTime.Now;

                    // 启动一个新线程来处理客户端连接
                    Task.Run(() => HandleClient(clientSocket, clientIpAddress));
                }
                catch (ObjectDisposedException)
                {
                    // 客户端主动关闭连接，可以忽略此异常
                }
                catch (Exception ex)
                {

                }

                await Task.Delay(500);
            }
        }

        private async Task HandleClient(TcpClient clientSocket, string clientIpAddress)
        {
            try
            {
                // MessageBox.Show("新线程");
                string previousClientIP = clientIpAddress;
                await ProcessDeviceIPAsync(clientSocket, clientIpAddress);

                // MessageBox.Show("连接关闭");

                await UpdateTextBlockForegroundAsync(previousClientIP, Brushes.DarkGray, null);
                clientSocket.Close();
                clientDictionary.Remove(previousClientIP);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted)
            {
                // 远程主机强制关闭了连接，忽略该异常
            }
            catch (IOException ex)
            {
                // 客户端意外断开连接，不需要显示错误消息，可以忽略此异常
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ProcessDeviceIPAsync(TcpClient clientSocket, string deviceIP)
        {
            Brush foregroundBrush = Brushes.Black;
            StringBuilder messageBuilder = new StringBuilder();
            string message = null;
            string firstIP = deviceIP;

            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    using (NetworkStream networkStream = clientSocket.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        while (clientSocket.Connected && (bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            messageBuilder.Append(receivedData);

                            if (receivedData.Contains("\n"))
                            {
                                // 收到换行符，表示消息接收完整
                                message = messageBuilder.ToString().TrimEnd('\n');
                                if (message == "clientSocket close")
                                {
                                    break;
                                }
                                foregroundBrush = Brushes.Green;
                                await UpdateTextBlockForegroundAsync(deviceIP, foregroundBrush, message);
                                messageBuilder.Clear();
                            }
                        }

                        // MessageBox.Show($"检测到连接中断,更新{firstIP}为灰色");
                        await UpdateTextBlockForegroundAsync(firstIP, Brushes.DarkGray, null);
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show($"Perror:" + ex.Message);
                    await UpdateTextBlockForegroundAsync(deviceIP, Brushes.DarkGray, null);
                }
            }
        }

        // 更新 TextBlock 的前景色
        private async Task UpdateTextBlockForegroundAsync(string deviceIP, Brush brush, string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                foreach (StackPanel panel in grid.Children.OfType<StackPanel>())
                {
                    foreach (TextBlock textBlock in panel.Children.OfType<TextBlock>())
                    {
                        string ip = textBlock.Tag as string;

                        if (!string.IsNullOrEmpty(ip) && ip == deviceIP)
                        {
                            textBlock.Foreground = brush;
                            textBlock.ToolTip = $"{deviceIP}\r\n{message}";

                            return; // 找到对应的 TextBlock 就可以直接返回
                        }
                    }
                }
            });
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

            if (btn == btnRefresh)
            {
                refreshAnimation();
                try
                {
                    StopListeningForClients();
                    initServerSocket();
                    ListenForClients();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                // refreshIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        private void refreshAnimation()
        {
            // 创建一个旋转动画
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.From = 0;
            rotateAnimation.To = 360;
            rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            rotateAnimation.RepeatBehavior = new RepeatBehavior(30);
            // rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;

            // 创建一个旋转转换，并将其应用到TextBlock的RenderTransform属性上
            RotateTransform transform = new RotateTransform();
            refreshIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            refreshIcon.RenderTransform = transform;

            // 将动画应用到RotateTransform的Angle属性上
            transform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

        }

        private async Task StopListeningForClients()
        {
            isListening = false; // 停止接受新的客户端连接

            // 等待所有已连接的客户端连接完成并关闭他们的 clientSocket
            foreach (var clientSocket in clientDictionary.Values)
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Close();
                }
            }

            // 关闭 serverSocket
            if (serverSocket != null)
            {
                serverSocket.Stop();
            }

            // 等待所有处理客户端连接的线程完成
            await Task.WhenAll(clientDictionary.Values.Select(c => Task.Run(() => c?.Close())));
        }
        private void MainForm_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopListeningForClients();
        }
    }
}
