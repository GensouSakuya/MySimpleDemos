using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Owin;

namespace Service
{
    public class Service
    {
        static Service()
        {
            //dumb methods
            //to ensure references assembly are copied
            var _ = typeof(Microsoft.Owin.Host.HttpListener.OwinHttpListener);
            StreamWriter.Null.WriteLine(_);
            _ = typeof(Microsoft.Owin.Security.AppBuilderSecurityExtensions);
            StreamWriter.Null.WriteLine(_);
            _ = typeof(Microsoft.AspNet.SignalR.Hub);
            StreamWriter.Null.WriteLine(_);
        }

        private static readonly object ResolverLock = new object();
        [ThreadStatic] internal static List<IHubPipelineModule> Modules;

        [ThreadStatic]
        internal static Func<BaseHub> Resolver;
        internal class StartUp
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseRobotService(Resolver, Modules);
            }
        }

        public static IDisposable Run<T>(int port) where T : BaseHub, new()
        {
            return Run<T>(port, null);
        }

        public static IDisposable Run<T>(int port, List<IHubPipelineModule> modules) where T : BaseHub, new()
        {
            IDisposable app = null;
            lock (ResolverLock)
            {
                Modules = modules;
                Resolver = () => new T();
                app = WebApp.Start<StartUp>($"http://localhost:{port}");
                Resolver = null;
                Modules = null;
                return app;
            }
        }
    }
}
