using System;
using System.Collections.Generic;
using System.Linq;
using Communication;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Owin;

namespace SignalRCom
{
    public class SignalRAdapter : ServiceAdapter
    {
        private static readonly object ResolverLock = new object();
        [ThreadStatic] internal static List<IHubPipelineModule> Modules;

        [ThreadStatic]
        internal static Func<RobotHub> Resolver;
        internal class StartUp
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseRobotService(Resolver, Modules);
            }
        }

        //public IDisposable Run<T>(int port) where T : RobotHub, new()
        //{
        //    return Run<T>(port, null);
        //}

        //public IDisposable Run(/*int port, List<IHubPipelineModule> modules*/)
        //{
        //    IDisposable app = null;
        //    lock (_resolverLock)
        //    {
        //        //Modules = modules;
        //        Resolver = () => new RobotHub(Service);
        //        app = WebApp.Start<StartUp>($"http://localhost:8888");
        //        Resolver = null;
        //        Modules = null;
        //        return app;
        //    }
        //}

        public override void Run()
        {
            IDisposable app = null;
            lock (ResolverLock)
            {
                //Modules = modules;
                Resolver = () => new RobotHub((RobotService) Service);
                app = WebApp.Start<StartUp>($"http://localhost:8888");
                Resolver = null;
                Modules = null;
            }
        }
    }

    [HubName("Demo")]
    public class RobotHub : Hub<IRobotClient>, IRobotService
    {
        private readonly RobotService _service;
        public RobotHub(RobotService service)
        {
            _service = service;
            _service.CallerResolver = () => Clients.Caller;
        }
        public void SendMessage(string message)
        {
            _service.SendMessage(message);
        }
    }

    internal static class ServiceExtension
    {
        public static IAppBuilder UseRobotService(this IAppBuilder app, Func<RobotHub> hubResolver,
            List<IHubPipelineModule> modules)
        {
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = false
            };
            //GlobalHost.HubPipeline.AddModule(new ExceptionHandleModule());
            if (modules?.Any() ?? false)
            {
                modules.ForEach(p => GlobalHost.HubPipeline.AddModule(p));
            }
            GlobalHost.DependencyResolver.Register(
                typeof(RobotHub), hubResolver);
            app.MapSignalR("/signalr", hubConfiguration);
            return app;
        }

        public static IAppBuilder UseRobotService(this IAppBuilder app, Func<RobotHub> hubResolver)
        {
            return UseRobotService(app, hubResolver, null);
        }
    }
}
