using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;

namespace SignalRClient
{
    public class SignalRAdapter:ClientAdapter
    {
        public override void Connect()
        {
            var proxy = new HubProxy("localhost", 8888, "Demo", new Dictionary<string, string>(), false, BindMethodToProxy);
            var service = new ServiceClient(proxy);
            proxy.Start();
            this.ServiceProxy = service;
        }

        private void BindMethodToProxy(HubProxy proxy)
        {
            proxy.On<string>("ReceiveMessage", (message) => ((IRobotClient) Client).ReceiveMessage(message));
        }
    }

    public class ServiceClient : IRobotService
    {
        private readonly HubProxy _proxy;
        internal ServiceClient(HubProxy proxy)
        {
            _proxy = proxy;
        }

        public void Dispose()
        {
            _proxy.Dispose();
        }

        public void SendMessage(string message)
        {
            _proxy.Invoke("SendMessage", message).Wait();
        }
    }

    internal class HubProxy : IDisposable
    {
        private HubConnection _hubConnection;
        private IHubProxy _proxy;

        private readonly string _serverAddr;
        private readonly Dictionary<string, string> _qsData;
        private readonly string _hubName;
        private Thread _connectionDaemonThread;
        private Action<HubProxy> _bindingAction;

        private void Connect()
        {
            _hubConnection = new HubConnection(_serverAddr, _qsData);
            _proxy = _hubConnection.CreateHubProxy(_hubName);
            _connectionDaemonThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (_isReconnecting)
                    {
                        lock (ReconnectLock)
                        {
                            if (_isReconnecting)
                            {
                                try
                                {
                                    Reconnect();
                                    _isReconnecting = false;
                                }
                                catch (Exception re)
                                {
                                }
                            }
                        }
                    }

                    Thread.Sleep(4000);
                }
            }));
            _connectionDaemonThread.IsBackground = true;
        }

        public HubProxy(string host, int port, string hubName, Dictionary<string, string> qsData, bool useHttps = true, Action<HubProxy> bindingMethods = null)
        {
            try
            {
                _serverAddr = $"{(useHttps ? "https" : "http")}://{host}:{port}/";
                _qsData = qsData;
                _hubName = hubName;
                _bindingAction = bindingMethods;
                Connect();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Start()
        {
            try
            {
                _bindingAction?.Invoke(this);
                _hubConnection.Start().Wait();
                _connectionDaemonThread.Start();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                throw;
            }
        }

        private bool _isReconnecting = false;
        private static readonly object ReconnectLock = new object();

        private void Reconnect()
        {
            Connect();
            Start();
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
        }

        public async Task Invoke(string methodName, params object[] param)
        {
            try
            {
                await _proxy.Invoke(methodName, param);
            }
            catch (Exception e)
            {
                if (!_isReconnecting)
                {
                    lock (ReconnectLock)
                    {
                        if (!_isReconnecting)
                        {
                            _isReconnecting = true;
                        }
                    }
                }
            }
        }

        public void On(string methodName, Action action)
        {
            _proxy.On(methodName, action);
        }

        public void On<T1>(string methodName, Action<T1> action)
        {
            _proxy.On(methodName, action);
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> action)
        {
            _proxy.On(methodName, action);
        }
    }
}
