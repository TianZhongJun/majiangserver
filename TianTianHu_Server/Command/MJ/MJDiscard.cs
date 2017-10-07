using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;

namespace MJ_FormsServer.Command.MJ
{
    public class MJDiscard : CommandBase<MJSession, StringRequestInfo>
    {
        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { command = requestInfo.Key };

                session.Update(requestInfo);

                //重复操作检测
                if (!session.myPlayerInfo.throwed)
                {
                    int card;
                    //参数检测
                    if (int.TryParse(session.GetValue(requestInfo, "card"), out card))
                    {
                        //session.StopAgent();
                        CommandHelper.MJDiscardHandle(session, requestInfo.Key, card);
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
