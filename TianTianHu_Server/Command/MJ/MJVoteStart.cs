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
    public class MJVoteStart : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, "MJVoteStart - " + requestInfo.Key + " : " + requestInfo.Body);
            lock (MJSession.sessionList)
            {
                session.Update(requestInfo);

                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = requestInfo.Key, data = requestInfo.Body };

                session.vote.Clear();
                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
                //向房间中所有成员发送 发起投票信息
                foreach (MJSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));

                    //设置解散房间的超时操作代理
                    DebugLog.Debug(log, "30秒后执行投票代理 - " + requestInfo.Key + " : " + requestInfo.Body);
                    s.StartAgent(30, "MJVoteSubmit:" + requestInfo.Body + "," + s.myPlayerInfo.userId + "," + EncriptAndDeciphering.EncryptDES(s.myPlayerInfo.nickName, GlobalConfig.encryptKey) + "," + true);
                }
            }
        }
    }
}
