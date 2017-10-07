using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;

namespace MJ_FormsServer.Command.MJ
{
    /// <summary>
    /// 麻将的退出房间命令
    /// </summary>
    public class MJExitRoom : CommandBase<MJSession, StringRequestInfo>
    {
        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            session.Update(requestInfo);

            CommandHelper.MJExitRoomHandle(session);
        }
    }
}
