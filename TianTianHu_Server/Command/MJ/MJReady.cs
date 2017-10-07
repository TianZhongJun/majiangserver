using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.Logic;
using MJ_FormsServer.DB;
using log4net;

namespace MJ_FormsServer.Command.MJ
{
    /// <summary>
    /// 麻将的准备命令
    /// </summary>
    public class MJReady : CommandBase<MJSession, StringRequestInfo>
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

                if (session.myPlayerInfo.oprateState == "MJAllReadyed")
                {
                    result.code = 119;
                    result.msg = GlobalConfig.GetErrMsg(119);
                    session.Send((new JavaScriptSerializer()).Serialize(result));
                }
                else
                {
                    //GameAgain的数据初始化
                    session.myPlayerInfo.isLaird = false;
                    session.myPlayerInfo.inRoom = true;
                    session.myPlayerInfo.deskCards = "";
                    session.myPlayerInfo.hunNumber = -1;
                    session.myPlayerInfo.throwed = false;

                    session.myPlayerInfo.ready = !session.myPlayerInfo.ready;
                    session.myPlayerInfo.gameState = "MJStart";
                    session.myPlayerInfo.oprateState = "MJReadyed";

                    List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();

                    bool isAllReady = (mySessionList.Select(p => p.myPlayerInfo.ready ? 1 : 0).Sum() == session.myPlayerInfo.playerMaxSum && session.myPlayerInfo.playerMaxSum >= 2);
                    if (isAllReady)
                    {
                        foreach (MJSession s in mySessionList)
                        {
                            //只有在第一次发牌的时候扣除用户房卡(每个用户执行一次)
                            if (!s.myPlayerInfo.isGamed)
                            {
                                s.myPlayerInfo.isGamed = true;
                                if (!s.myPlayerInfo.isAgent)
                                {
                                    //如果是房主则扣除创建房间需要的房卡(如果AA制时，创建房间需要的房卡是和加入房间需要的房卡数目是一样的)
                                    if (s.myPlayerInfo.isOwner)
                                    {
                                        if (s.myPlayerInfo.createNeedRoomCard > 0)
                                            DataBase.singleton.ConsumeRoomCard(s.myPlayerInfo.userId, "", s.myPlayerInfo.createNeedRoomCard);
                                    }
                                    //如果不是房主则扣除加入房间需要的房卡
                                    else
                                    {
                                        if (s.myPlayerInfo.joinNeedRoomCard > 0)
                                            DataBase.singleton.ConsumeRoomCard(s.myPlayerInfo.userId, "", s.myPlayerInfo.joinNeedRoomCard);
                                    }
                                }
                            }
                            s.myPlayerInfo.curPaoScore = -1;
                            s.myPlayerInfo.curMingGangTimes = 0;
                            s.myPlayerInfo.curAnGangTimes = 0;
                            s.myPlayerInfo.curGangScore = 0;
                            s.myPlayerInfo.curHuScore = 0;
                            s.myPlayerInfo.throwedCards = "";
                            s.myPlayerInfo.remainderCards = "";
                            s.myPlayerInfo.pengCards = "";
                            s.myPlayerInfo.gangCards = "";
                            s.myPlayerInfo.curGetCard = -1;
                            s.myPlayerInfo.lastThrowedCard = -1;

                            s.myPlayerInfo.oprateState = "MJAllReadyed";
                            s.myPlayerInfo.gameState = "MJQingXuanPao";
                        }
                    }

                    result.command = (isAllReady ? "MJAllReady" : "MJReady");
                    result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();
                    //向房间中所有成员发送命令
                    foreach (MJSession s in mySessionList)
                    {
                        s.Send((new JavaScriptSerializer()).Serialize(result));

                        ////设置选跑的超时操作代理
                        //s.StartAgent("MJXuanPao:" + 0);

                        //如果创建房间为不带跑则自动提交默认跑分0
                        if (isAllReady && !session.myPlayerInfo.daiPao)
                            CommandHelper.MJXuanPaoHandle(s, "MJXuanPao", 0);
                    }
                }
            }
        }
    }
}
