using System;

namespace Communication
{
    public interface IClientMethods
    {

    }

    public interface IServiceMethods
    {

    }

    public abstract class ClientBase<T> : IClientMethods where T: IServiceMethods
    {
        public Lazy<T> ServiceProxy;
        private readonly ClientAdapter _adapter;

        protected ClientBase(ClientAdapter adapter)
        {
            _adapter = adapter;
            _adapter.Client = this;
            _adapter.Connect();
            ServiceProxy = new Lazy<T>(() => (T) _adapter.ServiceProxy);
        }
    }

    public abstract class ClientAdapter
    {
        public abstract void Connect();
        public object Client { get; set; }
        public object ServiceProxy { get; set; }
    }


    public abstract class ServiceBase<T> : IServiceMethods where T: IClientMethods
    {
        private readonly ServiceAdapter _adapter;
        protected ServiceBase(ServiceAdapter adapter)
        {
            _adapter = adapter;
            _adapter.Service = this;
        }

        public void Run()
        {
            _adapter.Run();
        }

        public Func<T> CallerResolver;

        internal T GetCaller()
        {
            return CallerResolver.Invoke();
        }
    }


    public abstract class ServiceAdapter
    {
        public object Service { get; set; }
        public abstract void Run();
    }
}
