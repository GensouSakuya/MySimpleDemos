using System;
using Microsoft.AspNet.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{

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
            _connectionDaemonThread = new Thread(() =>
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
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                    }

                    Thread.Sleep(4000);
                }
            });
            _connectionDaemonThread.IsBackground = true;
        }

        public HubProxy(string host, int port, string hubName, Dictionary<string, string> qsData, bool useHttps = true, Action<HubProxy> bindingMethods = null)
        {
            _serverAddr = $"{(useHttps ? "https" : "http")}://{host}:{port}/";
            _qsData = qsData;
            _hubName = hubName;
            _bindingAction = bindingMethods;
            Connect();
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
            catch
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
