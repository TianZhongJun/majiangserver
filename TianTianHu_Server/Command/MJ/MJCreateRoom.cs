using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.DB;
using log4net;

namespace MJ_FormsServer.Command.MJ
{
    public class MJCreateRoom : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public int GetRoomId()
        {
            //房间数达到 999999 后，将会无限循环
            lock (MJSession.roomIdList)
            {
                //生成随机值
                string ticks = DateTime.Now.Ticks.ToString();
                int roomId = int.Parse(ticks.Substring(ticks.Length - 6));
                if (roomId == 0)
                    roomId = 1;

                //优化随机值
                bool existed = false;
                for (int i = 0; i < MJSession.roomIdList.Count; i++)
                {
                    if (MJSession.roomIdList[i] == roomId)
                    {
                        existed = true;
                    }
                    if (existed)
                    {
                        if (i + 1 < MJSession.roomIdList.Count)
                        {
                            if (MJSession.roomIdList[i] + 1 < MJSession.roomIdList[i + 1])
                            {
                                roomId = MJSession.roomIdList[i] + 1;
                                break;
                            }
                        }
                        else
                        {
                            if (MJSession.roomIdList[i] < 999999)
                            {
                                roomId = MJSession.roomIdList[i] + 1;
                                break;
                            }
                            else
                            {
                                roomId = 1;
                                i = -1;
                            }
                        }
                    }
                }

                //添加到列表，并进行升序排列
                MJSession.roomIdList.Add(roomId);
                MJSession.roomIdList.Sort();

                return roomId;
            }
        }

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, requestInfo.Key + " : " + requestInfo.Body);
            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult();
                result.command = requestInfo.Key;
                int gameId = 0;
                int sex = 0;
                double longitude = 0;
                double latitude = 0;
                int createNeedRoomCard = 0;
                int joinNeedRoomCard = 0;
                bool isAgent = false;
                int gameSum = 0;
                int gameType = 0;
                int roundSecond = 0;
                bool daiHun = false;
                bool dianPaoHu = false;
                bool daiFeng = false;
                bool daiPao = false;
                bool gangPao = false;
                bool zhuangJiaJiaDi = false;
                bool gangShangHuaJiaBei = false;
                bool qiDuiJiaBei = false;
                int basalScore = 0;
                int playerMaxSum = 0;

                //参数检测
                if (int.TryParse(session.GetValue(requestInfo, "gameId"), out gameId)
                    && int.TryParse(session.GetValue(requestInfo, "sex"), out sex)
                    && double.TryParse(session.GetValue(requestInfo, "longitude"), out longitude)
                    && double.TryParse(session.GetValue(requestInfo, "latitude"), out latitude)
                    && int.TryParse(session.GetValue(requestInfo, "createNeedRoomCard"), out createNeedRoomCard)
                    && int.TryParse(session.GetValue(requestInfo, "joinNeedRoomCard"), out joinNeedRoomCard)
                    && bool.TryParse(session.GetValue(requestInfo, "isAgent"), out isAgent)
                    && int.TryParse(session.GetValue(requestInfo, "gameSum"), out gameSum)
                    && int.TryParse(session.GetValue(requestInfo, "gameType"), out gameType)
                    && int.TryParse(session.GetValue(requestInfo, "roundSecond"), out roundSecond)
                    && bool.TryParse(session.GetValue(requestInfo, "daiHun"), out daiHun)
                    && bool.TryParse(session.GetValue(requestInfo, "dianPaoHu"), out dianPaoHu)
                    && bool.TryParse(session.GetValue(requestInfo, "daiFeng"), out daiFeng)
                    && bool.TryParse(session.GetValue(requestInfo, "daiPao"), out daiPao)
                    && bool.TryParse(session.GetValue(requestInfo, "gangPao"), out gangPao)
                    && bool.TryParse(session.GetValue(requestInfo, "zhuangJiaJiaDi"), out zhuangJiaJiaDi)
                    && bool.TryParse(session.GetValue(requestInfo, "gangShangHuaJiaBei"), out gangShangHuaJiaBei)
                    && bool.TryParse(session.GetValue(requestInfo, "qiDuiJiaBei"), out qiDuiJiaBei)
                    && int.TryParse(session.GetValue(requestInfo, "basalScore"), out basalScore)
                    && int.TryParse(session.GetValue(requestInfo, "playerMaxSum"), out playerMaxSum))
                {
                    //roomId不为0 说明已经创建过房间
                    //if (session.myPlayerInfo.roomId == 0)
                    if (true)
                    {
                        session.myPlayerInfo.userId = session.GetValue(requestInfo, "userId");
                        session.myPlayerInfo.nickName = EncriptAndDeciphering.DecryptDES(session.GetValue(requestInfo, "nickName"), GlobalConfig.encryptKey);
                        session.myPlayerInfo.gameId = gameId;
                        session.myPlayerInfo.isAgent = isAgent;
                        session.myPlayerInfo.sex = sex;
                        session.myPlayerInfo.userIp = session.GetValue(requestInfo, "userIp");
                        session.myPlayerInfo.longitude = longitude;
                        session.myPlayerInfo.latitude = latitude;
                        session.myPlayerInfo.headImgUrl = session.GetValue(requestInfo, "headImgUrl");
                        session.myPlayerInfo.roomId = GetRoomId();
                        session.myPlayerInfo.isOwner = true;
                        session.myPlayerInfo.isLaird = false;
                        session.myPlayerInfo.playerMaxSum = playerMaxSum;
                        session.myPlayerInfo.playerSum = (isAgent ? 0 : 1);
                        session.myPlayerInfo.createNeedRoomCard = createNeedRoomCard;
                        session.myPlayerInfo.joinNeedRoomCard = joinNeedRoomCard;
                        session.myPlayerInfo.isGamed = false;
                        session.myPlayerInfo.seatIndex = (isAgent ? -1 : 0);
                        session.myPlayerInfo.gameSum = gameSum;
                        session.myPlayerInfo.curGameSum = 0;
                        session.myPlayerInfo.gameType = gameType;
                        session.myPlayerInfo.roundSecond = roundSecond;
                        session.myPlayerInfo.daiHun = daiHun;
                        session.myPlayerInfo.dianPaoHu = dianPaoHu;
                        session.myPlayerInfo.daiFeng = daiFeng;
                        session.myPlayerInfo.daiPao = daiPao;
                        session.myPlayerInfo.gangPao = gangPao;
                        session.myPlayerInfo.zhuangJiaJiaDi = zhuangJiaJiaDi;
                        session.myPlayerInfo.gangShangHuaJiaBei = gangShangHuaJiaBei;
                        session.myPlayerInfo.qiDuiJiaBei = qiDuiJiaBei;
                        session.myPlayerInfo.basalScore = basalScore;
                        session.myPlayerInfo.curPaoScore = -1;
                        session.myPlayerInfo.curGangScore = 0;
                        session.myPlayerInfo.curHuScore = 0;
                        session.myPlayerInfo.curScore = 0;
                        session.myPlayerInfo.roomScore = 0;
                        session.myPlayerInfo.curMingGangTimes = 0;
                        session.myPlayerInfo.mingGangTimes = 0;
                        session.myPlayerInfo.curAnGangTimes = 0;
                        session.myPlayerInfo.anGangTimes = 0;
                        session.myPlayerInfo.dianPaoTimes = 0;
                        session.myPlayerInfo.jiePaoTimes = 0;
                        session.myPlayerInfo.ziMoTimes = 0;
                        session.myPlayerInfo.inRoom = true;
                        session.myPlayerInfo.ready = false;
                        session.myPlayerInfo.deskCards = "";
                        session.myPlayerInfo.throwedCards = "";
                        session.myPlayerInfo.remainderCards = "";
                        session.myPlayerInfo.pengCards = "";
                        session.myPlayerInfo.gangCards = "";
                        session.myPlayerInfo.hunNumber = -1;
                        session.myPlayerInfo.oprateSeatIndex = -1;
                        session.myPlayerInfo.discardSeatIndex = -1;
                        session.myPlayerInfo.nextSeatIndex = 0;
                        session.myPlayerInfo.lastHuUserId = "";
                        session.myPlayerInfo.curGetCard = -1;
                        session.myPlayerInfo.lastThrowedCard = -1;
                        session.myPlayerInfo.throwed = false;
                        session.myPlayerInfo.canRemove = false;
                        session.myPlayerInfo.oprateState = (isAgent ? "MJAgentCreated" : "MJCreated");
                        session.myPlayerInfo.gameState = "MJStart";
                        session.myRecord = new RecordData();
                        session.myRecord.roomInfo = "roomId:" + session.myPlayerInfo.roomId.ToString("D6") + ","
                            + "dianPaoHu:" + session.myPlayerInfo.dianPaoHu + ","
                            + "gameSum:" + session.myPlayerInfo.gameSum + ","
                            + "daiHun:" + session.myPlayerInfo.daiHun + ","
                            + "gangPao:" + session.myPlayerInfo.gangPao + ","
                            + "daiFeng:" + session.myPlayerInfo.daiFeng + ","
                            + "daiPao:" + session.myPlayerInfo.daiPao + ","
                            + "qiDuiJiaBei:" + session.myPlayerInfo.qiDuiJiaBei + ","
                            + "zhuangJiaJiaDi:" + session.myPlayerInfo.zhuangJiaJiaDi + ","
                            + "gangShangHuaJiaBei:" + session.myPlayerInfo.gangShangHuaJiaBei + ",";

                        //如果是代开房间的话则直接扣除房主房卡
                        if (isAgent)
                        {
                            if (session.myPlayerInfo.createNeedRoomCard > 0)
                                DataBase.singleton.ConsumeRoomCard(session.myPlayerInfo.userId, "", session.myPlayerInfo.createNeedRoomCard);
                        }

                        result.code = 0;
                        result.data = session.myPlayerInfo;
                    }
                    else
                    {
                        result.code = 108;
                        result.msg = GlobalConfig.GetErrMsg(108);
                    }

                }
                else
                {
                    result.code = 107;
                    result.msg = GlobalConfig.GetErrMsg(107);
                }

                //创建代理房间后关闭连接但是不能清理数据
                if (isAgent)
                {
                    result.command = "MJAgentCreateRoom";
                    session.Send((new JavaScriptSerializer()).Serialize(result));
                    session.Close();
                }
                else
                    session.Send((new JavaScriptSerializer()).Serialize(result));

                DebugLog.Debug(log, "MJCreateRoom - " + requestInfo.Key + " : " + session.myPlayerInfo.roomId.ToString("D6"));
            }
        }
    }
}
