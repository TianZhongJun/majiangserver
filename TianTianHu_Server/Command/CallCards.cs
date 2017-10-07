using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SSS_FormsServer.Command
{
    public class CallCards : CommandBase<SSSSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SSSSession session, StringRequestInfo requestInfo)
        {
            lock (SSSSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { command = requestInfo.Key };

                session.Update(requestInfo);
                
                if (session.roomInfo.curPaoScore == -1)
                {
                    int oprate = 0;
                    //参数检测
                    if (requestInfo.Parameters.Length == 2 && int.TryParse(session.GetValue(requestInfo, "oprate"), out oprate))
                    {
                        session.StopAgent();
                        session.CallCardsRealize(oprate);
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
