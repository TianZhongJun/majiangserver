using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;

namespace MJ_FormsServer.Command.MJ
{
    public class MJJoinRoom : CommandBase<MJSession, StringRequestInfo>
    {
        /// <summary>
        /// 麻将的加入房间命令
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            CommandHelper.MJJoinRoomHandle(session, requestInfo);
        }
    }
}
