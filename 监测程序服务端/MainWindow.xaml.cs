using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            CreateStackPanels();//动态创建设备图标
        }
        private void MainForm_Load(object sender, RoutedEventArgs e)
        {
            // 初始化服务端Socket监听
            string ipAddress = "172.22.50.3";
            int port = 8888;
            serverSocket = new TcpListener(IPAddress.Parse(ipAddress), port);
            serverSocket.Start();

            // 启动一个新线程来监听客户端连接
            Thread listenThread = new Thread(() => Task.Run(async () => await ListenForClients()));
            listenThread.Start();
        }

        // 在服务器类中定义一个列表来保存客户端的 IP 地址
        private List<string> connectedClients = new List<string>();

        // 定义一个字典，用于存储客户端 IP 地址与对应的 TcpClient 对象
        private Dictionary<string, TcpClient> clientDictionary = new Dictionary<string, TcpClient>();

        // 方法：根据客户端 IP 地址获取对应的 TcpClient 对象
        private async Task<TcpClient> GetTcpClientByIpAddressAsync(string clientIpAddress)
        {
            return await Task.Run(() =>
            {
                // 检查字典中是否包含指定的客户端 IP 地址
                if (clientDictionary.ContainsKey(clientIpAddress))
                {
                    // 如果包含，则返回对应的 TcpClient 对象
                    return clientDictionary[clientIpAddress];
                }
                else
                {
                    return null;
                }
            });
        }

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
                    connectedClients.Add(clientIpAddress);
                    clientDictionary.Add(clientIpAddress, clientSocket);

                    // 启动一个新线程来处理客户端连接
                    listenThread = new Thread(() => HandleClient(clientSocket, clientIpAddress));
                    listenThread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ListenForClientsError: " + ex.Message);
                }
            }
        }

        private void HandleClient(TcpClient clientSocket, string clientIpAddress)
        {
            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // 收到客户端消息，做相应处理
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //MessageBox.Show("Received from client: " + message);

                    // 响应客户端
                    byte[] response = Encoding.ASCII.GetBytes("Server received message: " + message);
                    networkStream.Write(response, 0, response.Length);
                    networkStream.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling client: " + ex.Message);
            }
            finally
            {
                // 从列表中移除客户端的 IP 地址
                connectedClients.Remove(clientIpAddress);
                clientDictionary.Remove(clientIpAddress);

                // 关闭客户端连接
                clientSocket.GetStream().Close(); // 关闭客户端的网络流
                clientSocket.Close();
            }
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

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个旋转动画
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.From = 0;
            rotateAnimation.To = 360;
            rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            // rotateAnimation.RepeatBehavior = new RepeatBehavior(10);
            rotateAnimation.RepeatBehavior = RepeatBehavior.Forever; // 设置为无限循环

            // 创建一个旋转转换，并将其应用到TextBlock的RenderTransform属性上
            RotateTransform transform = new RotateTransform();
            refreshIcon.RenderTransformOrigin = new Point(0.5, 0.5);
            refreshIcon.RenderTransform = transform;

            // 将动画应用到RotateTransform的Angle属性上
            transform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            //实现功能
            foreach (string deviceIP in allDeviceIPs)
            {
                try
                {
                    TcpClient clientSocket = await GetTcpClientByIpAddressAsync(deviceIP);

                    Brush foregroundBrush = Brushes.DarkGray;

                    if (clientSocket != null && clientSocket.Connected)
                    {
                        NetworkStream networkStream = clientSocket.GetStream();
                        byte[] heartbeat = Encoding.ASCII.GetBytes("Heartbeat");
                        networkStream.Write(heartbeat, 0, heartbeat.Length);
                        networkStream.Flush();

                        foregroundBrush = Brushes.Green;
                    }

                    UpdateTextBlockForeground(deviceIP, foregroundBrush);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Test:{ex.Message}");
                }
            }

            // 更新 TextBlock 的前景色
            async Task UpdateTextBlockForeground(string deviceIP, Brush brush)
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (StackPanel panel in grid.Children.OfType<StackPanel>())
                        {
                            foreach (TextBlock textBlock in panel.Children.OfType<TextBlock>())
                            {
                                string ip = textBlock.Tag as string;

                                if (!string.IsNullOrEmpty(ip) && ip == deviceIP)
                                {
                                    textBlock.Foreground = brush;
                                    return; // 找到对应的 TextBlock 就可以直接返回
                                }
                            }
                        }
                    });
                });
            }

            // 停止旋转动画
            transform.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        private async Task StopListeningForClients()
        {
            isListening = false; // 停止接受新的客户端连接

            // 关闭 serverSocket
            if (serverSocket != null)
            {
                serverSocket.Stop();
            }

            // 等待所有已连接的客户端连接完成并关闭他们的 clientSocket
            foreach (var clientSocket in clientDictionary.Values)
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Close();
                }
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
