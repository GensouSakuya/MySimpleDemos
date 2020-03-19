using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;

namespace Service
{
    internal static class ServiceExtension
    {
        public static IAppBuilder UseRobotService(this IAppBuilder app, Func<BaseHub> hubResolver,
            List<IHubPipelineModule> modules)
        {
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = false
            };
            if (modules?.Any() ?? false)
            {
                modules.ForEach(p => GlobalHost.HubPipeline.AddModule(p));
            }
            GlobalHost.DependencyResolver.Register(
                typeof(BaseHub), hubResolver);
            app.MapSignalR("/signalr", hubConfiguration);
            return app;
        }

        public static IAppBuilder UseRobotService(this IAppBuilder app, Func<BaseHub> hubResolver)
        {
            return UseRobotService(app, hubResolver, null);
        }
    }
}
