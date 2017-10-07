using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace MJ_FormsServer
{
    public struct SocketResult
    {
        public int code;
        public string command;
        public string msg;
        public object data;
    }

    public class MJServer : AppServer<MJSession>
    {
        public MJServer() : base(new CommandLineReceiveFilterFactory(Encoding.UTF8, new BasicRequestInfoParser(":", ",")))
        {
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            return base.Setup(rootConfig, config);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
        }
    }
}
