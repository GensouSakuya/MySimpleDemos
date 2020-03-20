using System;
using System.ServiceModel;

namespace Communication
{
    [ServiceContract]
    public interface IRobotClient:IClientMethods
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(string message);
    }

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IRobotClient))]
    public interface IRobotService: IServiceMethods
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);
    }

    public class RobotClient : ClientBase<IRobotService>, IRobotClient
    {
        public RobotClient(ClientAdapter adapter):base(adapter)
        {
        }

        public void ReceiveMessage(string message)
        {
            Console.WriteLine("Client:" + message);
        }
    }

    public class RobotService :ServiceBase<IRobotClient> , IRobotService
    {
        public void SendMessage(string message)
        {
            Console.WriteLine("Service:" + message);
            var caller = GetCaller();
            caller.ReceiveMessage(message);
        }

        public RobotService(ServiceAdapter adapter) : base(adapter)
        {
        }
    }
}
