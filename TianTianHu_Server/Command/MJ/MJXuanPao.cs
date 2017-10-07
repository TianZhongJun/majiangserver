using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;
using log4net;

namespace MJ_FormsServer.Command.MJ
{
    public class MJXuanPao : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, requestInfo.Key + " : " + requestInfo.Body);

            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = requestInfo.Key };

                session.Update(requestInfo);

                //重复操作检测
                if (session.myPlayerInfo.curPaoScore == -1)
                {
                    int curPaoScore = 0;

                    //参数检测
                    if (int.TryParse(session.GetValue(requestInfo, "curPaoScore"), out curPaoScore))
                    {
                        //session.StopAgent();
                        CommandHelper.MJXuanPaoHandle(session, requestInfo.Key, curPaoScore);
                    }
                    else
                    {
                        result.code = 107;
                        result.msg = GlobalConfig.GetErrMsg(107);
                    }
                }
                else
                {
                    result.code = 114;
                    result.msg = GlobalConfig.GetErrMsg(114);
                }
                if (result.code != 0)
                    session.Send((new JavaScriptSerializer()).Serialize(result));
            }
        }
    }
}
