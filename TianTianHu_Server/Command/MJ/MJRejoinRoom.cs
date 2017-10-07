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
    /// 检测玩家是否有未退出的房间
    /// </summary>
    public class MJRejoinRoom : CommandBase<MJSession, StringRequestInfo>
    {
        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            CommandHelper.MJRejoinRoomHandle(session, requestInfo);
        }
    }
}
