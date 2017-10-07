using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using log4net;

namespace MJ_FormsServer.Command.MJ
{
    public class MJGameChat : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, requestInfo.Key + " : " + requestInfo.Body);

            lock (MJSession.sessionList)
            {
                session.Update(requestInfo);

                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = requestInfo.Key, data = requestInfo.Body };

                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
                //向房间中所有成员发送 聊天消息
                foreach (MJSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }
            }
        }
    }
}
