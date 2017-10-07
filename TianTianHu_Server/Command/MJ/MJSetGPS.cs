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
    /// <summary>
    /// 设置玩家的经纬度
    /// </summary>
    public class MJSetGPS : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, requestInfo.Key + " : " + requestInfo.Body);

            lock (MJSession.sessionList)
            {
                session.Update(requestInfo);
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = "MJRoomUpdate" };

                double longitude = 0;
                double latitude = 0;
                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
                if (double.TryParse(session.GetValue(requestInfo, "longitude"), out longitude) && double.TryParse(session.GetValue(requestInfo, "latitude"), out latitude))
                {
                    if (longitude != 0 || latitude != 0)
                    {
                        session.myPlayerInfo.longitude = longitude;
                        session.myPlayerInfo.latitude = latitude;
                    }
                    session.myPlayerInfo.oprateState = requestInfo.Key;
                    result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();
                }
                else
                {
                    result.code = 107;
                    result.msg = GlobalConfig.GetErrMsg(107);
                }

                //向房间中所有成员发送命令
                foreach (MJSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }
            }
        }
    }
}
