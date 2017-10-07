using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase.Protocol;
using MJ_FormsServer.DB;
using log4net;
using CardHelper;

namespace MJ_FormsServer.Logic
{
    public class CommandHelper
    {
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 麻将的加入房间命令
        /// </summary>
        /// <param name="session">连接对象</param>
        /// <param name="requestInfo">请求信息</param>
        public static void MJJoinRoomHandle(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, "MJJoinRoomHandle - " + requestInfo.Key + " : " + requestInfo.Body);

            lock (MJSession.sessionList)
            {
                //更新最后操作时间
                session.lastOperateTime = DateTime.Now;
                //创建结果类型数据
                SocketResult result = new SocketResult();
                int gameId = 0;
                int sex = 0;
                double longitude = 0;
                double latitude = 0;
                int roomId = 0;

                //参数检测
                if (int.TryParse(session.GetValue(requestInfo, "gameId"), out gameId) && int.TryParse(session.GetValue(requestInfo, "sex"), out sex) && double.TryParse(session.GetValue(requestInfo, "longitude"), out longitude) && double.TryParse(session.GetValue(requestInfo, "latitude"), out latitude) && int.TryParse(session.GetValue(requestInfo, "roomId"), out roomId))
                {
                    //如果roomId不为0 说明已经加入的房间未退出
                    //if (session.myPlayerInfo.roomId == 0)
                    if (true)
                    {
                        //清理残留 Session
                        bool findLegacy = false;
                        for (int i = MJSession.sessionList.Count - 1; i >= 0; i--)
                        {
                            //seatIndex != -1 排除掉代开的房间
                            if (MJSession.sessionList[i].myPlayerInfo.userId == session.GetValue(requestInfo, "userId") && MJSession.sessionList[i].myPlayerInfo.seatIndex != -1 && MJSession.sessionList[i].SessionID != session.SessionID)
                            {
                                if (MJSession.sessionList[i].myPlayerInfo.roomId != 0)
                                {
                                    findLegacy = true;

                                    session.myPlayerInfo = MJSession.sessionList[i].myPlayerInfo;
                                    session.myRecord = MJSession.sessionList[i].myRecord;
                                    //清掉 roomId 为了后面的逻辑区分原有玩家和新加入的玩家
                                    session.myPlayerInfo.roomId = 0;

                                    DebugLog.Debug(log, "加入房间前的检查 - 已清理残留Session gameId : " + MJSession.sessionList[i].myPlayerInfo.gameId + " " + MJSession.sessionList[i].myPlayerInfo.nickName + " oprateState : " + session.myPlayerInfo.oprateState + " SessionID : " + MJSession.sessionList[i].SessionID);
                                }

                                MJSession.sessionList.RemoveAt(i);
                            }
                        }

                        //获取房间所有成员（只获取原有人员，如果是代开的话包括代开人员）
                        List<MJSession> roomSessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == roomId).ToList();

                        if (findLegacy || roomSessionList.Count > 0)
                        {
                            if (findLegacy || roomSessionList.Count < roomSessionList[0].myPlayerInfo.playerMaxSum + (roomSessionList[0].myPlayerInfo.isAgent ? 1 : 0))
                            {
                                if (!findLegacy)
                                {
                                    session.myPlayerInfo = roomSessionList[0].myPlayerInfo;

                                    session.myPlayerInfo.isOwner = false;
                                    session.myPlayerInfo.isLaird = false;
                                    session.myPlayerInfo.seatIndex = (session.myPlayerInfo.isAgent ? -1 : 0);
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
                                    session.myPlayerInfo.curGetCard = -1;
                                    session.myPlayerInfo.lastThrowedCard = -1;
                                    session.myPlayerInfo.throwed = false;
                                    session.myPlayerInfo.canRemove = false;
                                    session.myPlayerInfo.oprateState = "MJJoined";
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
                                }

                                //由于前面有playerInfo的对象赋值，这里放到最后赋值主要信息
                                session.myPlayerInfo.userId = session.GetValue(requestInfo, "userId");
                                session.myPlayerInfo.nickName = EncriptAndDeciphering.DecryptDES(session.GetValue(requestInfo, "nickName"), GlobalConfig.encryptKey);
                                session.myPlayerInfo.gameId = gameId;
                                session.myPlayerInfo.sex = sex;
                                session.myPlayerInfo.userIp = session.GetValue(requestInfo, "userIp");
                                session.myPlayerInfo.longitude = longitude;
                                session.myPlayerInfo.latitude = latitude;
                                session.myPlayerInfo.headImgUrl = session.GetValue(requestInfo, "headImgUrl");
                                session.myPlayerInfo.roomId = roomId;
                                session.myPlayerInfo.playerSum = roomSessionList.Count + (session.myPlayerInfo.isAgent ? 0 : 1);

                                List<PlayerInfo> roomList = new List<PlayerInfo>();
                                foreach (MJSession room in roomSessionList)
                                {
                                    room.myPlayerInfo.playerSum = session.myPlayerInfo.playerSum;
                                    roomList.Add(room.myPlayerInfo);
                                }
                                //对采集结果进行位置索引上的升序排列
                                roomList.Sort((x, y) => x.seatIndex.CompareTo(y.seatIndex));
                                //生成最小位置索引
                                foreach (PlayerInfo i in roomList)
                                {
                                    if (session.myPlayerInfo.seatIndex == i.seatIndex)
                                    {
                                        session.myPlayerInfo.seatIndex++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                //如果是代开人员加入房间 设置当前人员的属性
                                if (session.myPlayerInfo.isAgent)
                                {
                                    if (session.myPlayerInfo.userId == roomList[0].userId)
                                        session.myPlayerInfo.isOwner = true;
                                    roomList.RemoveAt(0);
                                }
                                roomList.Add(session.myPlayerInfo);

                                result.code = 0;
                                result.command = "MJRoomUpdate";
                                result.data = roomList;

                                roomSessionList.Add(session);
                                //向房间中所有成员发送 MJRoomUpdate 命令
                                foreach (MJSession s in roomSessionList)
                                {
                                    s.Send((new JavaScriptSerializer()).Serialize(result));
                                }
                            }
                            else
                            {
                                result.code = 112;
                                result.msg = GlobalConfig.GetErrMsg(112);
                            }
                        }
                        else
                        {
                            result.code = 111;
                            result.msg = GlobalConfig.GetErrMsg(111);
                        }
                    }
                    else
                    {
                        result.code = 113;
                        result.msg = GlobalConfig.GetErrMsg(113);
                    }

                }
                else
                {
                    result.code = 107;
                    result.msg = GlobalConfig.GetErrMsg(107);
                }
                result.command = requestInfo.Key;
                if (result.code != 0)
                    session.Send((new JavaScriptSerializer()).Serialize(result));
            }
        }

        /// <summary>
        /// 检测玩家是否有未退出的房间
        /// </summary>
        /// <param name="session">连接对象</param>
        /// <param name="requestInfo">请求信息</param>
        public static void MJRejoinRoomHandle(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, "MJRejoinRoomHandle - " + requestInfo.Key + " : " + requestInfo.Body);

            lock (MJSession.sessionList)
            {
                //更新最后操作时间
                session.lastOperateTime = DateTime.Now;
                //创建结果类型数据
                SocketResult result = new SocketResult() { command = requestInfo.Key };

                //UserId赋值
                session.myPlayerInfo.userId = session.GetValue(requestInfo, "userId");

                //是否发现残留Session
                bool findLegacy = false;

                //输出找到的同名Session
                foreach (MJSession s in MJSession.sessionList)
                {
                    if (s.myPlayerInfo.userId == session.myPlayerInfo.userId && s.myPlayerInfo.seatIndex != -1 && s.SessionID != session.SessionID)
                    {
                        DebugLog.Debug(log, "MJRejoinRoomHandle - " + "找到的同名Session - userId:" + s.myPlayerInfo.userId + " roomId:" + s.myPlayerInfo.roomId.ToString("D6") + " gameId:" + s.myPlayerInfo.gameId + " SessionID:" + s.SessionID + " \n" + session.SessionID);
                    }
                }

                //查找曾经进入过的房间并发送房间信息
                foreach (MJSession s in MJSession.sessionList)
                {
                    //roomId != 0 排除掉代开的房间
                    if (s.myPlayerInfo.userId == session.myPlayerInfo.userId && s.myPlayerInfo.roomId != 0 && s.myPlayerInfo.seatIndex != -1 && s.SessionID != session.SessionID)
                    {
                        DebugLog.Debug(log, "MJRejoinRoomHandle - " + "房间检测 roomId : " + s.myPlayerInfo.roomId.ToString("D6"));
                        findLegacy = true;

                        result.code = 0;
                        result.data = s.myPlayerInfo;
                        //此处只返回消息并不清除残留Session，清除逻辑在加入房间，否则会找不到房间
                        session.Send((new JavaScriptSerializer()).Serialize(result));
                        break;
                    }
                }
                if (!findLegacy)
                {
                    result.code = 118;
                    result.msg = GlobalConfig.GetErrMsg(118);

                    session.Send((new JavaScriptSerializer()).Serialize(result));

                    DebugLog.Debug(log, "没有遗留房间 userId : " + session.myPlayerInfo.userId);
                    session.myPlayerInfo = new PlayerInfo();
                    session.myPlayerInfo.canRemove = true;
                    session.Close();
                }
            }
        }

        /// <summary>
        /// 获取代开房间的信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static string MJGetRoomInfo(string userId)
        {
            DebugLog.Debug(log, "MJGetRoomInfo - userId:" + userId);

            lock (MJSession.sessionList)
            {
                List<MJSession> agentSessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.userId == userId && p.myPlayerInfo.seatIndex == -1).ToList();
                StringBuilder agentRoomStr = new StringBuilder();
                StringBuilder nicks = new StringBuilder();
                foreach (MJSession s in agentSessionList)
                {
                    List<PlayerInfo> infos = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == s.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList().Select(p => p.myPlayerInfo).ToList();
                    nicks.Clear();
                    foreach (PlayerInfo info in infos)
                    {
                        nicks.Append((nicks.Length == 0 ? "" : ":") + info.nickName);
                        Console.WriteLine("当前局数："+info.curGameSum);
                    }
                    agentRoomStr.Append((agentRoomStr.Length == 0 ? "" : ";")
                        + s.StartTime.ToString("yyyy年MM月dd日 HH:mm:ss")
                        + "," + s.myPlayerInfo.roomId.ToString("D6")
                        + "," + s.myPlayerInfo.gameSum
                        + "," + s.myPlayerInfo.playerMaxSum
                        + "," + s.myPlayerInfo.dianPaoHu
                        + "," + s.myPlayerInfo.daiHun
                        + "," + s.myPlayerInfo.daiFeng
                        + "," + s.myPlayerInfo.gangPao
                        + "," + s.myPlayerInfo.gangShangHuaJiaBei
                        + "," + s.myPlayerInfo.qiDuiJiaBei
                        + "," + s.myPlayerInfo.zhuangJiaJiaDi
                        + "," + nicks);
                }
                return agentRoomStr.ToString();
            }
        }

        public static int GetPokerNumber(Random r, List<int> pokerNumber)
        {
            int index = r.Next(pokerNumber.Count);
            int number = pokerNumber[index];
            pokerNumber.RemoveAt(index);
            return number;
        }

        public static void MJXuanPaoHandle(MJSession session, string key, int curPaoScore)
        {
            SocketResult result = new SocketResult() { code = 0, command = key };

            //更新用户上传的跑分
            session.myPlayerInfo.curPaoScore = curPaoScore;

            session.myPlayerInfo.oprateState = "MJYiXuanPao";
            session.myPlayerInfo.gameState = "MJYiXuanPao";

            List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
            mySessionList.Sort((x, y) => x.myPlayerInfo.seatIndex.CompareTo(y.myPlayerInfo.seatIndex));
            result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();
            //向房间中所有成员发送 MJXuanPao 命令
            foreach (MJSession s in mySessionList)
            {
                s.Send((new JavaScriptSerializer()).Serialize(result));
            }

            //所有人选跑之后自动发牌
            if (mySessionList.Select(p => p.myPlayerInfo.curPaoScore == -1 ? 0 : 1).Sum() == session.myPlayerInfo.playerMaxSum && session.myPlayerInfo.playerMaxSum >= 2)
            {
                Random r = new Random();
                List<int> pokerNumbers = new List<int>();
                List<int> userPokers = new List<int>();
                for (int i = 0; i < (session.myPlayerInfo.daiFeng ? 136 : 108); i++)
                {
                    pokerNumbers.Add(i);
                }

                while (pokerNumbers.Count > 0)
                {
                    userPokers.Add(GetPokerNumber(r, pokerNumbers));
                }

                ////造牌测试 
                //userPokers[0] = 0;
                //userPokers[1] = 1;
                //userPokers[2] = 2;
                //userPokers[3] = 3;
                //userPokers[4] = 4;
                //userPokers[5] = 5;
                //userPokers[6] = 6;
                //userPokers[7] = 7;
                //userPokers[8] = 8;
                //userPokers[9] = 9;
                //userPokers[10] =10 ;
                //userPokers[11] = 11;
                //userPokers[12] = 12;
                //userPokers[13] = 26;
                //userPokers[14] = 14;
                //userPokers[15] = 15;
                //userPokers[16] = 16;
                //userPokers[17] = 17;
                //userPokers[18] = 18;
                //userPokers[19] = 19;
                //userPokers[20] = 20;
                //userPokers[21] = 21;
                //userPokers[22] = 22;
                //userPokers[23] = 23;
                //userPokers[24] = 24;
                //userPokers[25] = 25;
                //userPokers[26] = 13;
                //userPokers[27] = 27;
                //for (int pIndex = 52; pIndex < userPokers.Count; pIndex++)
                //    userPokers[pIndex] = 0;
                userPokers[0] = 5;
                userPokers[1] = 6;
                userPokers[2] = 7;
                userPokers[3] = 9;
                userPokers[4] = 10;
                userPokers[5] = 11;
                userPokers[6] = 13;
                userPokers[7] = 14;
                userPokers[8] = 15;
                userPokers[9] = 36;
                userPokers[10] = 37;
                userPokers[11] = 24;
                userPokers[12] = 32;
                userPokers[13] = 33;
                userPokers[14] = 34;
                userPokers[15] = 28;
                userPokers[16] = 38;
                userPokers[17] = 20;
                userPokers[18] = 16;
                userPokers[19] = 12;
                userPokers[20] = 8;
                userPokers[21] = 4;
                userPokers[22] = 0;
                userPokers[23] = 1;
                userPokers[24] = 2;
                userPokers[25] = 3;
                userPokers[26] = 35;
                userPokers[27] = 29;
                userPokers[28] = 30;
                userPokers[29] = 31;
                userPokers[30] = 25;
                userPokers[31] = 26;
                userPokers[32] = 27;
                userPokers[33] = 21;
                userPokers[34] = 22;
                userPokers[35] = 23;
                userPokers[36] = 17;
                userPokers[37] = 18;
                userPokers[38] = 19;
                for (int i = 39; i < userPokers.Count; i++)
                {
                    userPokers[i] = i;
                }
                //生成混子值
                int hunNumber = r.Next(session.myPlayerInfo.daiFeng ? 34 : 27);

                //清除庄家属性，为了后面重新生成庄家席位
                foreach (MJSession s in mySessionList)
                {
                    s.myPlayerInfo.isLaird = false;
                }
                //生成庄家席位索引
                int lairdSeatIndex = 0;
                if (string.IsNullOrEmpty(session.myPlayerInfo.lastHuUserId))
                {
                    //获取房主座次索引
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.isAgent ? s.myPlayerInfo.seatIndex == 0 : s.myPlayerInfo.isOwner)
                        {
                            lairdSeatIndex = s.myPlayerInfo.seatIndex;
                            s.myPlayerInfo.isLaird = true;
                            break;
                        }
                    }
                }
                else
                {
                    //获取最近一个胡的人的座次索引
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.userId == session.myPlayerInfo.lastHuUserId)
                        {
                            lairdSeatIndex = s.myPlayerInfo.seatIndex;
                            s.myPlayerInfo.isLaird = true;
                            break;
                        }
                    }
                }

                //更新玩家信息
                foreach (MJSession s in mySessionList)
                {
                    s.myPlayerInfo.deskCards = MJCardHelper.EncryptIntList(userPokers.GetRange(13 * s.myPlayerInfo.playerMaxSum + 1, userPokers.Count - 13 * s.myPlayerInfo.playerMaxSum - 1));
                    s.myPlayerInfo.remainderCards = MJCardHelper.EncryptIntList(userPokers.GetRange(13 * s.myPlayerInfo.seatIndex, 13));
                    if (s.myPlayerInfo.daiHun)
                        s.myPlayerInfo.hunNumber = hunNumber;
                    s.myPlayerInfo.nextSeatIndex = lairdSeatIndex;
                    s.myPlayerInfo.oprateSeatIndex = lairdSeatIndex;
                    s.myPlayerInfo.curGetCard = userPokers[13 * s.myPlayerInfo.playerMaxSum];
                    s.myPlayerInfo.oprateState = "MJDealPokered";
                    s.myPlayerInfo.gameState = "MJInGame";
                }

                result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();

                //赋值录像信息
                StringBuilder remainderCards = new StringBuilder();
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.seatIndex > -1)
                    {
                        remainderCards.Append((remainderCards.Length == 0 ? "" : ",") + s.myPlayerInfo.remainderCards);
                    }
                }
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.seatIndex > -1)
                    {
                        s.myRecord.remainderCards += ((string.IsNullOrEmpty(s.myRecord.remainderCards) ? "" : ";") + remainderCards.ToString());
                        s.myRecord.videoData += ((string.IsNullOrEmpty(s.myRecord.videoData) ? "" : ";") + lairdSeatIndex + " MP 0 " + session.myPlayerInfo.curGetCard);
                    }
                }

                StringBuilder sbCardId = new StringBuilder();
                sbCardId.Append("房间 " + session.myPlayerInfo.roomId.ToString("D6") + " 第 " + (session.myPlayerInfo.curGameSum + 1) + " 局发牌完毕 混子:" + hunNumber + " 成员为:");
                foreach (MJSession s in mySessionList)
                    sbCardId.Append("<" + s.myPlayerInfo.seatIndex + " " + s.myPlayerInfo.nickName + " " + s.myPlayerInfo.gameId + ">");
                sbCardId.Append(" 发牌长度为:" + userPokers.Count + " 内容为:");
                for (int i = 0; i < userPokers.Count; i++)
                    sbCardId.Append(" " + userPokers[i]);
                DebugLog.Debug(log, sbCardId);

                result.command = "MJDealPoker";
                foreach (MJSession s in mySessionList)
                {
                    //向房间所有人发送发牌消息
                    s.Send((new JavaScriptSerializer()).Serialize(result));

                    ////设置出牌的超时操作代理
                    //if (s.myPlayerInfo.seatIndex == lairdSeatIndex)
                    //{
                    //    s.StartAgent("MJDiscard:" + CardHelper.DecryptIntList(s.myPlayerInfo.remainderCards)[0]);
                    //}
                }
            }
        }

        public static void MJDiscardHandle(MJSession session, string key, int card)
        {
            DebugLog.Debug(log, "MJDiscardHandle - Start - " + session.myPlayerInfo.roomId.ToString("D6") + " " + session.myPlayerInfo.nickName + " " + session.myPlayerInfo.gameId + " 牌面ID：" + card);

            lock (MJSession.sessionList)
            {
                //如果新Session已经出过牌，残留Session的逻辑取消
                foreach (MJSession s in MJSession.sessionList)
                {
                    if (s.myPlayerInfo.userId == session.myPlayerInfo.userId && s.myPlayerInfo.seatIndex != -1 && s.SessionID != session.SessionID && s.myPlayerInfo.throwed)
                    {
                        DebugLog.Debug(log, "MJSession - " + s.myPlayerInfo.userId + " 已出牌");
                        return;
                    }
                }

                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.GetValidSession().myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();

                //更新数据 把所有玩家可操作属性设置为真，并关闭当前玩家的可操作属性
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.userId != session.GetValidSession().myPlayerInfo.userId)
                        s.myPlayerInfo.throwed = false;
                }
                session.GetValidSession().myPlayerInfo.throwed = true;
                session.GetValidSession().myPlayerInfo.lastThrowedCard = card;
                session.GetValidSession().myPlayerInfo.oprateSeatIndex = session.GetValidSession().myPlayerInfo.seatIndex;
                session.GetValidSession().myPlayerInfo.discardSeatIndex = session.GetValidSession().myPlayerInfo.seatIndex;
                //实例化手中剩余的牌对象
                List<int> remainderCards = MJCardHelper.DecryptIntList(session.GetValidSession().myPlayerInfo.remainderCards);
                //如果新牌ID<0 则说明已经有玩家出过牌啦，这次出牌就判断为碰之后的出牌
                if (session.GetValidSession().myPlayerInfo.curGetCard >= 0)
                    remainderCards.Add(session.GetValidSession().myPlayerInfo.curGetCard);
                remainderCards.Remove(card);
                session.GetValidSession().myPlayerInfo.remainderCards = MJCardHelper.EncryptIntList(remainderCards);
                //实例化打出去的牌
                List<int> throwedCards = MJCardHelper.DecryptIntList(session.GetValidSession().myPlayerInfo.throwedCards);
                throwedCards.Add(card);
                session.GetValidSession().myPlayerInfo.throwedCards = MJCardHelper.EncryptIntList(throwedCards);
                session.GetValidSession().myPlayerInfo.curGetCard = -1;
                session.GetValidSession().myPlayerInfo.oprateState = "MJDiscard";
                //记录录像数据
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.seatIndex > -1)
                    {
                        s.myRecord.videoData += ("," + session.GetValidSession().myPlayerInfo.seatIndex + " DP 0 " + card);
                    }
                }

                CheckOprate(session, key);
            }

            DebugLog.Debug(log, "MJDiscardHandle - End - 耗时：" + (DateTime.Now - session.GetValidSession().lastOperateTime).TotalSeconds + " 剩余人数：" + session.myPlayerInfo.playerSum);
        }

        public static void CheckOprate(MJSession session, string key)
        {
            DebugLog.Debug(log, "CheckOprate - " + key);
            SocketResult result = new SocketResult() { code = 0, command = key };

            List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.GetValidSession().myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
            mySessionList.Sort((x, y) => x.myPlayerInfo.seatIndex.CompareTo(y.myPlayerInfo.seatIndex));

            List<int> deskCards = MJCardHelper.DecryptIntList(session.GetValidSession().myPlayerInfo.deskCards);

            //其他玩家是否能碰杠胡
            bool canPengGangHu = false;
            int tmpSeat = session.GetValidSession().myPlayerInfo.seatIndex + 1;
            if (tmpSeat >= session.GetValidSession().myPlayerInfo.playerMaxSum)
                tmpSeat = 0;
            while (tmpSeat != session.GetValidSession().myPlayerInfo.discardSeatIndex)
            {
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.seatIndex == tmpSeat)
                    {
                        List<int> tmpHCards = MJCardHelper.DecryptIntList(s.myPlayerInfo.remainderCards);
                        List<int> pgResult = MaJiangHelper.PengGang(tmpHCards, session.GetValidSession().myPlayerInfo.lastThrowedCard, s.myPlayerInfo.hunNumber);
                        int huType = MaJiangHelper.KeHu(tmpHCards, session.GetValidSession().myPlayerInfo.lastThrowedCard, true, s.myPlayerInfo.hunNumber);
                        if (pgResult.Count > 0 || huType > 0)
                        {
                            if (pgResult.Count == 0 && !session.GetValidSession().myPlayerInfo.dianPaoHu)
                            {
                                DebugLog.Debug(log, "自摸胡玩法不可以点炮胡");
                            }
                            else
                            {
                                DebugLog.Debug(log, "可以碰杠胡");
                                canPengGangHu = true;
                                session.GetValidSession().myPlayerInfo.nextSeatIndex = s.myPlayerInfo.seatIndex;
                            }
                        }
                    }
                }

                if (canPengGangHu)
                    break;

                tmpSeat++;
                if (tmpSeat >= session.GetValidSession().myPlayerInfo.playerMaxSum)
                    tmpSeat = 0;
            }

            //如果没有人能碰杠胡
            if (!canPengGangHu)
            {
                //如果剩余牌数量<=(14-所有用户的杠牌数量) 则本轮结束
                if (deskCards.Count <= (14 - mySessionList.Select(p => (p.myPlayerInfo.curMingGangTimes + p.myPlayerInfo.curAnGangTimes)).Sum()))
                {
                    //当前局数加1
                    int nowGameSum = session.GetValidSession().myPlayerInfo.curGameSum + 1;

                    //更新数据
                    foreach (MJSession s in mySessionList)
                    {
                        s.myPlayerInfo.curGameSum = nowGameSum;
                        s.myPlayerInfo.curMingGangTimes = 0;
                        s.myPlayerInfo.curAnGangTimes = 0;
                        s.myPlayerInfo.curGangScore = 0;
                        s.myPlayerInfo.curHuScore = 0;
                        s.myPlayerInfo.curScore = 0;

                        s.myPlayerInfo.inRoom = false;
                        s.myPlayerInfo.ready = false;
                        s.myPlayerInfo.oprateState = (s.myPlayerInfo.curGameSum >= s.myPlayerInfo.gameSum ? "MJRoomEnd" : "MJGameEnd");
                        s.myPlayerInfo.gameState = "MJGameEnd";
                    }

                    result.command = session.GetValidSession().myPlayerInfo.oprateState;

                    //记录录像数据
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.seatIndex > -1)
                        {
                            s.myRecord.videoData += ("," + session.myPlayerInfo.seatIndex + " HZ 0 0");
                        }
                    }

                    //赋值并上传战绩
                    StringBuilder uids = new StringBuilder();
                    StringBuilder nicks = new StringBuilder();
                    StringBuilder sexs = new StringBuilder();
                    StringBuilder heads = new StringBuilder();
                    int lairdSeatIndex = 0;
                    StringBuilder playerInfo = new StringBuilder();
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.seatIndex > -1)
                        {
                            uids.Append((uids.Length == 0 ? "" : ",") + s.myPlayerInfo.userId);
                            nicks.Append((nicks.Length == 0 ? "" : ",") + s.myPlayerInfo.nickName);
                            sexs.Append((sexs.Length == 0 ? "" : ",") + s.myPlayerInfo.sex);
                            heads.Append((heads.Length == 0 ? "" : ",") + s.myPlayerInfo.headImgUrl);
                            if (s.myPlayerInfo.isOwner)
                                session.myRecord.ownerSeatIndex = s.myPlayerInfo.seatIndex;
                            if (s.myPlayerInfo.isLaird)
                                lairdSeatIndex = s.myPlayerInfo.seatIndex;
                            playerInfo.Append((playerInfo.Length == 0 ? "" : ",") + s.myPlayerInfo.curPaoScore + " " + s.myPlayerInfo.curGangScore + " " + s.myPlayerInfo.curHuScore + " " + s.myPlayerInfo.roomScore);
                        }
                    }
                    List<string> updateNicks = new List<string>();
                    string recordStr = "";
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.seatIndex > -1)
                        {
                            //赋值并上传战绩
                            s.myRecord.endTime += ((string.IsNullOrEmpty(s.myRecord.endTime) ? "" : ";") + DateTime.Now.ToString("yyyy年MM月dd日\nHH:mm:ss"));
                            if (string.IsNullOrEmpty(s.myRecord.userId))
                                s.myRecord.userId = uids.ToString();
                            if (string.IsNullOrEmpty(s.myRecord.nickName))
                                s.myRecord.nickName = nicks.ToString();
                            if (string.IsNullOrEmpty(s.myRecord.sex))
                                s.myRecord.sex = sexs.ToString();
                            if (string.IsNullOrEmpty(s.myRecord.headImgUrl))
                                s.myRecord.headImgUrl = heads.ToString();
                            s.myRecord.hunNumber += ((string.IsNullOrEmpty(s.myRecord.hunNumber) ? "" : ";") + s.myPlayerInfo.hunNumber);
                            s.myRecord.ownerSeatIndex = session.myRecord.ownerSeatIndex;
                            s.myRecord.lairdSeatIndex += ((string.IsNullOrEmpty(s.myRecord.lairdSeatIndex) ? "" : ";") + lairdSeatIndex);
                            s.myRecord.playerInfo += ((string.IsNullOrEmpty(s.myRecord.playerInfo) ? "" : ";") + playerInfo.ToString());
                            //上传战绩
                            if (result.command == "MJRoomEnd" && session.myPlayerInfo.curGameSum > 0)
                            {
                                updateNicks.Add(s.myPlayerInfo.userId);
                                if (string.IsNullOrEmpty(recordStr))
                                    recordStr = (new JavaScriptSerializer()).Serialize(s.myRecord);
                                DataBase.singleton.UploadRecord(s.myPlayerInfo.userId, "", (new JavaScriptSerializer()).Serialize(s.myRecord));
                            }
                        }
                    }
                    foreach (MJSession s in MJSession.sessionList)
                    {
                        if (s.myPlayerInfo.roomId == session.GetValidSession().myPlayerInfo.roomId && s.myPlayerInfo.seatIndex == -1 && !string.IsNullOrEmpty(recordStr) && !updateNicks.Contains(s.myPlayerInfo.userId))
                        {
                            DataBase.singleton.UploadRecord(s.myPlayerInfo.userId, "", recordStr);
                        }
                    }
                }
                else
                {
                    if (session.GetValidSession().myPlayerInfo.curGetCard < 0)
                    {
                        //相对于当前出牌玩家的下一个席位
                        int nextSeat = session.GetValidSession().myPlayerInfo.discardSeatIndex + 1;
                        if (nextSeat >= session.GetValidSession().myPlayerInfo.playerMaxSum)
                            nextSeat = 0;

                        ////随机摸牌
                        //int newCardPosition = (new Random()).Next(deskCards.Count);
                        //int newCard = deskCards[newCardPosition];
                        //deskCards.RemoveAt(newCardPosition);

                        //依次摸牌
                        int newCard = deskCards[0];
                        deskCards.RemoveAt(0);

                        session.GetValidSession().myPlayerInfo.deskCards = MJCardHelper.EncryptIntList(deskCards);
                        session.GetValidSession().myPlayerInfo.nextSeatIndex = nextSeat;
                        session.GetValidSession().myPlayerInfo.oprateSeatIndex = nextSeat;
                        session.GetValidSession().myPlayerInfo.curGetCard = newCard;

                        //记录录像数据
                        foreach (MJSession s in mySessionList)
                        {
                            if (s.myPlayerInfo.seatIndex > -1)
                            {
                                s.myRecord.videoData += ("," + nextSeat + " MP 0 " + newCard);
                            }
                        }
                    }
                }
            }

            //更新数据 
            foreach (MJSession s in mySessionList)
            {
                s.myPlayerInfo.oprateSeatIndex = session.GetValidSession().myPlayerInfo.oprateSeatIndex;
                s.myPlayerInfo.discardSeatIndex = session.GetValidSession().myPlayerInfo.discardSeatIndex;
                s.myPlayerInfo.lastThrowedCard = session.GetValidSession().myPlayerInfo.lastThrowedCard;
                s.myPlayerInfo.deskCards = session.GetValidSession().myPlayerInfo.deskCards;
                s.myPlayerInfo.curGetCard = session.GetValidSession().myPlayerInfo.curGetCard;
                s.myPlayerInfo.nextSeatIndex = session.GetValidSession().myPlayerInfo.nextSeatIndex;
            }

            result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();

            if (result.command == "MJRoomEnd")
            {
                for (int i = MJSession.sessionList.Count - 1; i >= 0; i--)
                {
                    if (MJSession.sessionList[i].myPlayerInfo.roomId == session.myPlayerInfo.roomId && MJSession.sessionList[i].myPlayerInfo.seatIndex == -1)
                    {
                        MJSession.sessionList[i].myPlayerInfo = new PlayerInfo();
                        MJSession.sessionList[i].myPlayerInfo.canRemove = true;
                        MJSession.sessionList.RemoveAt(i);
                    }
                }
            }

            //向房间中所有成员发送命令
            foreach (MJSession s in mySessionList)
            {
                s.Send((new JavaScriptSerializer()).Serialize(result));

                if (result.command == "MJRoomEnd")
                {
                    s.myPlayerInfo = new PlayerInfo();
                    s.myPlayerInfo.canRemove = true;
                    s.Close();
                }
            }
        }

        public static void MJExitRoomHandle(MJSession session)
        {
            DebugLog.Debug(log, "MJExitRoomHandle : " + session.myPlayerInfo.roomId.ToString("D6"));

            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult();
                result.command = "MJExitRoom";

                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.GetValidSession().myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1 && p.SessionID != session.SessionID).ToList();

                int sum = mySessionList.Count;
                foreach (MJSession s in mySessionList)
                {
                    s.myPlayerInfo.playerSum = sum;
                }

                result.code = 0;
                result.command = "MJRoomUpdate";
                result.data = mySessionList.Select(p => p.myPlayerInfo).ToList();

                foreach (MJSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }

                session.myPlayerInfo = new PlayerInfo();
                session.myPlayerInfo.canRemove = true;
                session.myPlayerInfo.oprateState = "MJExited";
                result.command = "MJExitRoom";
                session.Send((new JavaScriptSerializer()).Serialize(result));
                session.Close();
                DebugLog.Debug(log, "MJExitRoom - " + (new JavaScriptSerializer()).Serialize(result));
            }
        }

        public static void MJDissolveRoomHandle(MJSession session)
        {
            DebugLog.Debug(log, "MJDissolveRoom : ");

            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0 };

                //移除房间对象
                MJSession.roomIdList.Remove(session.myPlayerInfo.roomId);

                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.GetValidSession().myPlayerInfo.roomId).ToList();
                mySessionList.Sort((x, y) => x.myPlayerInfo.seatIndex.CompareTo(y.myPlayerInfo.seatIndex));

                string createrId = "";
                List<string> updateNicks = new List<string>();
                string recordStr = "";
                foreach (MJSession s in mySessionList)
                {
                    if (s.myPlayerInfo.seatIndex == -1)
                    {
                        createrId = s.myPlayerInfo.userId;
                    }
                    if (s.myPlayerInfo.seatIndex > -1 && s.myPlayerInfo.curGameSum > 0)
                    {
                        updateNicks.Add(s.myPlayerInfo.userId);
                        //上传战绩
                        if (string.IsNullOrEmpty(recordStr))
                            recordStr = (new JavaScriptSerializer()).Serialize(s.myRecord);
                        DataBase.singleton.UploadRecord(s.myPlayerInfo.userId, "", (new JavaScriptSerializer()).Serialize(s.myRecord));
                    }

                    if (s.myPlayerInfo.userId == session.myPlayerInfo.userId)
                    {
                        s.myPlayerInfo = new PlayerInfo();
                        s.myPlayerInfo.canRemove = true;
                        s.myPlayerInfo.oprateState = "MJDissolved";
                        result.command = "MJDissolveRoom";
                        s.Send((new JavaScriptSerializer()).Serialize(result));
                        s.Close();
                    }
                    else
                    {
                        s.myPlayerInfo = new PlayerInfo();
                        s.myPlayerInfo.canRemove = true;
                        s.myPlayerInfo.oprateState = "MJOwnerDissolved";
                        result.command = "MJOwnerDissolveRoom";
                        if (s.Connected)
                        {
                            s.Send((new JavaScriptSerializer()).Serialize(result));
                            s.Close();
                        }
                        else
                            MJSession.sessionList.Remove(s);
                    }
                }
                if (!string.IsNullOrEmpty(createrId) && !string.IsNullOrEmpty(recordStr) && !updateNicks.Contains(createrId))
                {
                    DataBase.singleton.UploadRecord(createrId, "", recordStr);
                }

                DebugLog.Debug(log, "房间已解散，剩余房间数量：" + MJSession.roomIdList.Count);
            }
        }

        public static void MJVoteSubmitHandle(MJSession session, string key, string body)
        {
            DebugLog.Debug(log, "MJVoteSubmitHandle - " + key + " : " + body);
            lock (MJSession.sessionList)
            {
                //如果新Session已经投过票，残留Session的逻辑取消
                foreach (MJSession s in MJSession.sessionList)
                {
                    if (s.myPlayerInfo.userId == session.myPlayerInfo.userId && s.myPlayerInfo.seatIndex != -1 && s.SessionID != session.SessionID && s.myPlayerInfo.oprateState == "MJVoteSubmited")
                    {
                        DebugLog.Debug(log, "MJVoteSubmitHandle - " + s.myPlayerInfo.userId + " 已投票");
                        return;
                    }
                }

                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = key, data = body };

                session.myPlayerInfo.oprateState = "MJVoteSubmited";
                List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
                //向房间中所有成员发送 投票提交结果
                foreach (MJSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }

                string[] votePar = Regex.Split(body, ",");

                bool argue;
                if (votePar.Length > 6 && bool.TryParse(votePar[6], out argue) && argue)
                {
                    foreach (MJSession s in mySessionList)
                    {
                        if (s.myPlayerInfo.roomId == session.myPlayerInfo.roomId && s.myPlayerInfo.userId == votePar[1])
                        {
                            s.vote.Add(body);

                            if (s.vote.Count >= 2 && s.vote.Count >= (session.myPlayerInfo.playerSum - 1))
                            {
                                //向房间中所有成员发送 提交操作被允许的消息
                                foreach (MJSession ss in mySessionList)
                                {
                                    result.command = "MJVoteFinish";
                                    ss.Send((new JavaScriptSerializer()).Serialize(result));
                                }

                                if (votePar[3] == "MJDissolveRoom")
                                    CommandHelper.MJDissolveRoomHandle(s);
                                else
                                    CommandHelper.MJExitRoomHandle(s);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    //向房间中所有成员发送 提交操作被拒绝的消息
                    foreach (MJSession s in mySessionList)
                    {
                        s.StopAgent();
                        result.command = "MJVoteFinish";
                        s.Send((new JavaScriptSerializer()).Serialize(result));
                    }
                }

            }
        }
    }
}
