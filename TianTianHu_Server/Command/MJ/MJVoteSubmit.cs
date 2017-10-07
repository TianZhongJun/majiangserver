using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;

namespace MJ_FormsServer.Command.MJ
{
    public class MJVoteSubmit : CommandBase<MJSession, StringRequestInfo>
    {
        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            session.Update(requestInfo);
            session.StopAgent();
            CommandHelper.MJVoteSubmitHandle(session, requestInfo.Key, requestInfo.Body);
        }
    }
}
