using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Script.Serialization;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SSS_FormsServer.Logic;
using log4net;

namespace SSS_FormsServer
{
    public struct RoomInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId;
        /// <summary>
        /// 游戏ID
        /// </summary>
        public string gameId;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string nickName;
        /// <summary>
        /// 用户性别
        /// </summary>
        public int sex;
        /// <summary>
        /// 用户头像地址
        /// </summary>
        public string headImgUrl;
        /// <summary>
        /// 房间ID
        /// </summary>
        public int roomId;
        /// <summary>
        /// 是否是创建者
        /// </summary>
        public bool isOwner;
        /// <summary>
        /// 是否是庄家
        /// </summary>
        public bool isLaird;
        /// <summary>
        /// 房间中最大玩家人数
        /// </summary>
        public int playerMaxSum;
        /// <summary>
        /// 房间中当前玩家人数
        /// </summary>
        public int playerSum;
        /// <summary>
        /// 创建房间需要的房卡数
        /// </summary>
        public int createNeedRoomCard;
        /// <summary>
        /// 加入房间需要的房卡数
        /// </summary>
        public int joinNeedRoomCard;
        /// <summary>
        /// 是否进行过游戏
        /// </summary>
        public bool isGamed;
        /// <summary>
        /// 相对于创建者的座次索引
        /// </summary>
        public int seatIndex;
        /// <summary>
        /// 游戏总局数
        /// </summary>
        public int gameSum;
        /// <summary>
        /// 当前游戏局数
        /// </summary>
        public int curGameSum;
        /// <summary>
        /// 玩法类型
        /// </summary>
        public int gameType;
        /// <summary>
        /// 出牌时间
        /// </summary>
        public int roundSecond;
        /// <summary>
        /// 是否带混
        /// </summary>
        public bool daiHun;
        /// <summary>
        /// 是否允许点炮胡
        /// </summary>
        public bool dianPaoHu;
        /// <summary>
        /// 是否带风
        /// </summary>
        public bool daiFeng;
        /// <summary>
        /// 是否带跑
        /// </summary>
        public bool daiPao;
        /// <summary>
        /// 是否杠跑
        /// </summary>
        public bool gangPao;
        /// <summary>
        /// 庄家是否加底
        /// </summary>
        public bool zhuangJiaJiaDi;
        /// <summary>
        /// 杠上花是否加倍
        /// </summary>
        public bool gangShangHuaJiaBei;
        /// <summary>
        /// 七对是否加倍
        /// </summary>
        public bool qiDuiJiaBei;
        /// <summary>
        /// 底分
        /// </summary>
        public int basalScore;
        /// <summary>
        /// 本局输赢分数
        /// </summary>
        public int curScore;
        /// <summary>
        /// 总共输赢分数
        /// </summary>
        public int roomScore;
        /// <summary>
        /// 发牌模式（1张发还是5张发）
        /// </summary>
        public int faPaiMoShi;
        /// <summary>
        /// 带马玩法（不带马，或者马几）
        /// </summary>
        public int daiMaWanFa;
        /// <summary>
        /// A-5顺子大小（只比10-A小还是顺子中最小）
        /// </summary>
        public int shunZiDaXiao;
        /// <summary>
        /// 是否计算特殊分
        /// </summary>
        public int teShuFen;
        /// <summary>
        /// 当前倍数
        /// </summary>
        public int curMultiple;
        /// <summary>
        /// 赢的次数
        /// </summary>
        public int winTimes;
        /// <summary>
        /// 输的次数
        /// </summary>
        public int loseTimes;
        /// <summary>
        /// 平的次数
        /// </summary>
        public int pingTimes;
        /// <summary>
        /// 是否在房间内
        /// </summary>
        public bool inRoom;
        /// <summary>
        /// 是否已准备
        /// </summary>
        public bool ready;
        /// <summary>
        /// 玩家选择的跑分
        /// </summary>
        public int curPaoScore;
        /// <summary>
        /// 手中剩余的牌
        /// </summary>
        public string remainderCards;
        /// <summary>
        /// 底牌
        /// </summary>
        public string basalCards;
        /// <summary>
        /// 最后一个玩家出的牌
        /// </summary>
        public string lastCards;
        /// <summary>
        /// 玩家当前出的牌
        /// </summary>
        public string curCards;
        /// <summary>
        /// 是否已出过牌
        /// </summary>
        public bool throwed;
        /// <summary>
        /// 出牌的次数
        /// </summary>
        public int discardTimes;
        /// <summary>
        /// 是否出过炸弹
        /// </summary>
        public bool bombed;
        /// <summary>
        /// 比牌结果
        /// </summary>
        public string biPaiResult;
        /// <summary>
        /// 用户token
        /// </summary>
        public string token;
        /// <summary>
        /// 所属Session是否可移除
        /// </summary>
        public bool canRemove;
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public string gameState;
    }

    public class SSSSession : AppSession<SSSSession>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static List<SSSSession> sessionList = new List<SSSSession>();
        public static List<int> roomIdList = new List<int>();

        public RoomInfo roomInfo = new RoomInfo();
        public List<string> vote = new List<string>();
        public DateTime lastOperateTime = DateTime.Now;
        private Thread tdAgent;

        public void StartAgent(string parameter)
        {
            StartAgent(roomInfo.roundSecond, parameter);
        }

        public void StartAgent(int second, string parameter)
        {
            StopAgent();
            tdAgent = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(second * 1000);
                string cmd = parameter.Split(':')[0];
                switch (cmd)
                {
                    case "CallCards":
                        CallCardsRealize(int.Parse(parameter.Split(':')[1]));
                        break;
                    case "Discard":
                        DiscardRealize(parameter.Split(':')[1]);
                        break;
                    case "VoteSubmit":
                        CommandHelper.VoteSubmitHandle(this, parameter.Split(':')[0], parameter.Split(':')[1]);
                        break;
                    default:
                        break;
                }
            }));
            tdAgent.IsBackground = true;
            tdAgent.Start();
        }

        public void StopAgent()
        {
            if (tdAgent != null)
            {
                tdAgent.Abort();
                tdAgent = null;
            }
        }

        public void CallCardsRealize(int opr)
        {
            lock (SSSSession.sessionList)
            {
                this.roomInfo.curPaoScore = opr;
                if (opr > this.roomInfo.curMultiple)
                    this.roomInfo.curMultiple = opr;
                SocketResult result = new SocketResult() { code = 0, command = "CallCards", data = this.roomInfo };
                //发送叫地主的操作结果消息
                List<SSSSession> roomList = SSSSession.sessionList.Where(p => p.roomInfo.roomId == this.roomInfo.roomId).ToList();
                foreach (SSSSession s in roomList)
                {
                    s.roomInfo.curMultiple = this.roomInfo.curMultiple;
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }

                //下一个席位
                int nextSeat = this.roomInfo.seatIndex + 1;
                if (nextSeat > 3)
                    nextSeat = 0;
                //地主席位
                int lairdSeatIndex = 0;
                foreach (SSSSession s in roomList)
                    if (s.roomInfo.isLaird)
                        lairdSeatIndex = s.roomInfo.seatIndex;

                //如果有人叫2分或者 一轮完毕则结束叫地主
                if (opr == 2 || nextSeat == lairdSeatIndex)
                {
                    int maxOprate = roomList.Select(p => p.roomInfo.curPaoScore).Max();
                    //获取叫牌结束后的地主席位索引
                    int lairdIndex = 0;
                    foreach (SSSSession s in roomList)
                    {
                        if ((maxOprate == 0 && s.roomInfo.isLaird) || (maxOprate != 0 && s.roomInfo.curPaoScore == maxOprate))
                        {
                            lairdIndex = s.roomInfo.seatIndex;
                            break;
                        }
                    }

                    //更新玩家在房间中的对象信息
                    foreach (SSSSession s in roomList)
                    {
                        if (s.roomInfo.seatIndex == lairdIndex)
                        {
                            s.roomInfo.isLaird = true;
                            //更新服务器上的地主牌
                            List<int> tmpCards = CardHelper.StringToCard(s.roomInfo.remainderCards);
                            tmpCards.AddRange(CardHelper.StringToCard(s.roomInfo.basalCards));
                            ////排序
                            //for (int i = 0; i < tmpCards.Count - 1; i++)
                            //{
                            //    for (int j = 0; j < tmpCards.Count - 1 - i; j++)
                            //    {
                            //        if (tmpCards[j] < tmpCards[j + 1])
                            //        {
                            //            int tmp = tmpCards[j];
                            //            tmpCards[j] = tmpCards[j + 1];
                            //            tmpCards[j + 1] = tmp;
                            //        }
                            //    }
                            //}
                            s.roomInfo.remainderCards = CardHelper.CardToString(tmpCards);
                        }
                        else
                            s.roomInfo.isLaird = false;
                        if (s.roomInfo.curMultiple < 1)
                            s.roomInfo.curMultiple = 1;
                    }

                    result = new SocketResult() { code = 0, command = "EndCallCards", data = roomList.Select(p => p.roomInfo).ToList() };
                    foreach (SSSSession s in roomList)
                    {
                        //向房间所有人发送叫牌结束消息
                        s.Send((new JavaScriptSerializer()).Serialize(result));
                        //设置地主出牌的超时代理
                        if (s.roomInfo.isLaird)
                        {
                            s.StartAgent("Discard:" + CardHelper.CardToString(PlayCue.Cue(s.roomInfo.lastCards, s.roomInfo)));
                        }
                        //向房间所有人发送开始出牌的玩家席位索引
                        s.Send((new JavaScriptSerializer()).Serialize(new SocketResult() { code = 0, command = "NextDiscard", data = lairdIndex }));
                    }
                }
                else
                //设置下一个玩家叫地主的超时操作代理
                {
                    foreach (SSSSession s in roomList)
                    {
                        //设置下一个玩家出牌的超时代理
                        if (s.roomInfo.seatIndex == nextSeat)
                        {
                            s.StartAgent("CallCards:" + ((s.roomInfo.gameType == 1 && PlayCue.MustCall(s.roomInfo)) ? 2 : 0));
                        }
                        //向房间所有人发送下一个叫牌玩家的席位索引
                        s.Send((new JavaScriptSerializer()).Serialize(new SocketResult() { code = 0, command = "NextCallCards", data = nextSeat }));
                    }
                }
            }
        }

        public SSSSession GetValidSession()
        {
            foreach (SSSSession s in SSSSession.sessionList)
            {
                if (s.roomInfo.userId == this.roomInfo.userId && s.roomInfo.gameId == this.roomInfo.gameId && s.SessionID != this.SessionID)
                {
                    return s;
                }
            }
            return this;
        }

        public void DiscardRealize(string cards)
        {
            DebugLog.Debug(log, "SSSSession - " + this.roomInfo.nickName + " " + cards);

            lock (SSSSession.sessionList)
            {
                //如果新Session已经出过牌，残留Session的逻辑取消
                foreach (SSSSession s in SSSSession.sessionList)
                {
                    if (s.roomInfo.userId == this.roomInfo.userId && s.SessionID != this.SessionID && s.roomInfo.throwed)
                    {
                        DebugLog.Debug(log, "SSSSession - " + s.roomInfo.userId + " 已出牌");
                        return;
                    }
                }
                //if (!this.GetValidSession().Connected)
                //{
                //    if (GlobalConfig.isDebug)
                //        Console.WriteLine(this.roomInfo.userId + " Session无效");
                //    return;
                //}

                DebugLog.Debug(log, "SSSSession - " + this.roomInfo.userId + " 正常比牌");
                this.GetValidSession().roomInfo.throwed = true;

                this.GetValidSession().roomInfo.curCards = cards;
                this.GetValidSession().roomInfo.gameState = "Discard";

                SocketResult result = new SocketResult() { code = 0, command = "Discard", data = this.GetValidSession().roomInfo };

                List<SSSSession> mySessionList = SSSSession.sessionList.Where(p => p.roomInfo.roomId == this.GetValidSession().roomInfo.roomId).ToList();
                result.data = mySessionList.Select(p => p.roomInfo).ToList();
                //向房间中所有成员发送 Discard 命令
                foreach (SSSSession s in mySessionList)
                {
                    s.Send((new JavaScriptSerializer()).Serialize(result));
                }

                //所有人出完牌后开始比牌
                if (mySessionList.Select(p => p.roomInfo.throwed ? 1 : 0).Sum() == this.GetValidSession().roomInfo.playerMaxSum)
                {
                    //当前局数加1
                    this.GetValidSession().roomInfo.curGameSum++;

                    bool finalQuanLeiDa = false;
                    foreach (SSSSession s1 in mySessionList)
                    {
                        s1.roomInfo.curScore = 0;
                        s1.roomInfo.biPaiResult = "";
                        bool quanLeiDa = true;
                        foreach (SSSSession s2 in mySessionList)
                        {
                            if (s1.roomInfo.userId != s2.roomInfo.userId)
                            {
                                s1.roomInfo.biPaiResult += (s2.roomInfo.userId + ":");
                                int tmpScore = 0;
                                List<int> card1 = CardHelper.StringToCard(s1.roomInfo.curCards);
                                List<int> card2 = CardHelper.StringToCard(s2.roomInfo.curCards);

                                int teShu1 = PlayCue.GetCurrentSpecialCardType(card1);
                                int teShu2 = PlayCue.GetCurrentSpecialCardType(card2);

                                //普通牌型比较
                                if (teShu1 == 0 && teShu2 == 0)
                                {
                                    int daQiang = 0;
                                    string tmpResult = "";
                                    for (int i = 0; i < 3; i++)
                                    {
                                        int[] compareResult = PlayCue.CompareDaoCard(card1.GetRange((i == 0 ? 0 : ((i - 1) * 5 + 3)), i == 0 ? 3 : 5), card2.GetRange((i == 0 ? 0 : ((i - 1) * 5 + 3)), i == 0 ? 3 : 5), i, this.GetValidSession().roomInfo.teShuFen == 0, this.GetValidSession().roomInfo.shunZiDaXiao);
                                        tmpResult += (s2.roomInfo.userId + i + "a:" + compareResult[0] + "," + s2.roomInfo.userId + i + "b:" + compareResult[1] + ",");
                                        tmpScore += (compareResult[0] + compareResult[1]);
                                        daQiang += (compareResult[0] < 0 ? -1 : (compareResult[0] > 0 ? 1 : 0));
                                        if (compareResult[0] <= 0)
                                        {
                                            quanLeiDa = false;
                                        }
                                    }

                                    //打枪 积分*2
                                    if (daQiang == 3 || daQiang == -3)
                                    {
                                        DebugLog.Debug(log, "SSSSession - " + "计算打枪积分 " + daQiang);
                                        s1.roomInfo.biPaiResult += (tmpScore + "," + tmpResult);

                                        tmpScore *= 2;
                                    }
                                    else
                                        s1.roomInfo.biPaiResult += "0,";

                                    s1.roomInfo.curScore += tmpScore;
                                }
                                //特殊牌型比较
                                else
                                {
                                    s1.roomInfo.biPaiResult += "0,";
                                    quanLeiDa = false;
                                    if (teShu1 > teShu2)
                                    {
                                        s1.roomInfo.curScore += PlayCue.GetSpecialScoreByCardType(PlayCue.GetSpecialMaxCardType(card1));
                                    }
                                    else if (teShu1 < teShu2)
                                    {
                                        s1.roomInfo.curScore -= PlayCue.GetSpecialScoreByCardType(PlayCue.GetSpecialMaxCardType(card2));
                                    }
                                }
                            }
                        }
                        //判断最终房间中有没有人是全垒打
                        if (quanLeiDa)
                        {
                            if (this.GetValidSession().roomInfo.playerMaxSum == 4)
                                s1.roomInfo.biPaiResult += "quanleida";
                            finalQuanLeiDa = true;
                        }

                        s1.roomInfo.roomScore += s1.roomInfo.curScore;

                        //游戏结束后关闭所有玩家的inRoom及ready属性
                        s1.roomInfo.curMultiple = this.GetValidSession().roomInfo.curMultiple;
                        s1.roomInfo.curGameSum = this.GetValidSession().roomInfo.curGameSum;
                        if (s1.roomInfo.curScore > 0)
                            s1.roomInfo.winTimes++;
                        else if (s1.roomInfo.curScore < 0)
                            s1.roomInfo.loseTimes++;
                        else
                            s1.roomInfo.pingTimes++;

                        s1.roomInfo.inRoom = false;
                        s1.roomInfo.ready = false;
                    }


                    //全垒打积分翻倍
                    if (finalQuanLeiDa && this.GetValidSession().roomInfo.playerMaxSum == 4)
                    {
                        foreach (SSSSession s in SSSSession.sessionList)
                        {
                            if (s.roomInfo.roomId == this.GetValidSession().roomInfo.roomId)
                            {
                                //注意顺序不要错
                                s.roomInfo.roomScore += s.roomInfo.curScore;
                                s.roomInfo.curScore += s.roomInfo.curScore;
                            }
                        }
                    }

                    result.command = (this.GetValidSession().roomInfo.curGameSum >= this.GetValidSession().roomInfo.gameSum ? "EndRoom" : "CompareCard");

                    //修改各个玩家的游戏状态
                    foreach (SSSSession s in SSSSession.sessionList)
                    {
                        if (s.roomInfo.roomId == this.GetValidSession().roomInfo.roomId)
                        {
                            s.roomInfo.gameState = (this.GetValidSession().roomInfo.curGameSum >= this.GetValidSession().roomInfo.gameSum ? "EndRoom" : "EndGame");
                        }
                    }

                    result.data = SSSSession.sessionList.Where(p => p.roomInfo.roomId == this.GetValidSession().roomInfo.roomId).Select(p => p.roomInfo).ToList();

                    for (int sessionIndex = 0; sessionIndex < SSSSession.sessionList.Count; sessionIndex++)
                    {
                        if (SSSSession.sessionList[sessionIndex].roomInfo.roomId == this.GetValidSession().roomInfo.roomId)
                        {
                            //向房间所有人发送比牌结束消息
                            SSSSession.sessionList[sessionIndex].Send((new JavaScriptSerializer()).Serialize(result));
                            if (result.command == "EndRoom")
                            {
                                if (SSSSession.sessionList[sessionIndex].Connected)
                                {
                                    SSSSession.sessionList[sessionIndex].roomInfo.canRemove = true;
                                    SSSSession.sessionList[sessionIndex].Close();
                                }
                                else
                                {
                                    SSSSession.sessionList[sessionIndex].roomInfo = new RoomInfo();
                                    SSSSession.sessionList.Remove(this.GetValidSession());
                                    sessionIndex--;
                                }
                            }
                        }
                    }
                }
            }
        }
        public string GetValue(StringRequestInfo requestInfo, string key)
        {
            foreach (string par in requestInfo.Parameters)
            {
                if (par.StartsWith(key))
                {
                    return par.Substring(key.Length);
                }
            }
            return "";
        }

        /// <summary>
        /// 通过请求的Session删除掉原有的断开连接的Session，并把原有Session的数据复制到新Session上
        /// </summary>
        /// <param name="requestInfo">String类型的请求信息</param>
        public void Update(StringRequestInfo requestInfo)
        {
            lock (sessionList)
            {
                //获取请求参数中提供的当前用户ID
                string userId = GetValue(requestInfo, "userId");

                //如果ID有效并且 与当前Session中ID不一致
                if (!string.IsNullOrEmpty(userId) && this.roomInfo.userId != userId)
                {
                    for (int i = 0; i < sessionList.Count; i++)
                    {
                        //如果找到同名Session
                        if (sessionList[i].roomInfo.userId == userId && sessionList[i].SessionID != this.SessionID)
                        {
                            //只赋值第一个对象，确保只执行一次
                            if (this.roomInfo.userId != userId)
                            {
                                this.roomInfo = sessionList[i].roomInfo;
                                this.vote.Clear();
                                this.vote.AddRange(sessionList[i].vote);
                            }

                            //sessionList[i].roomInfo = new SSS_FormsServer.RoomInfo();
                            //sessionList[i].vote = new List<string>();
                            sessionList.RemoveAt(i);
                            i--;
                            DebugLog.Debug(log, "SessionUpdate - 已找到同名Session：" + userId + " token：" + sessionList[i].SessionID);
                        }
                    }
                }
            }
        }

        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();

            lock (sessionList)
            {
                sessionList.Add(this);
                DebugLog.Debug(log, "SSSSession - " + "Session连接：" + sessionList.Count);
            }
        }

        protected override void HandleUnknownRequest(StringRequestInfo requestInfo)
        {
            //调用父类方法：返回Unknown request: +key
            //base.HandleUnknownRequest(requestInfo);

            SocketResult result = new SocketResult();
            result.code = 109;
            result.command = requestInfo.Key;
            result.msg = GlobalConfig.GetErrMsg(109);
            this.Send((new JavaScriptSerializer()).Serialize(result));
        }

        protected override void HandleException(Exception e)
        {
            base.HandleException(e);

            SocketResult result = new SocketResult();
            result.code = 110;
            result.msg = GlobalConfig.GetErrMsg(110) + e.Message;
            this.Send((new JavaScriptSerializer()).Serialize(result));
        }

        protected override void OnSessionClosed(CloseReason reason)
        {
            base.OnSessionClosed(reason);

            lock (sessionList)
            {
                //DebugLog.Debug(log, "SSSSession - " + "Session剩余：" + sessionList.Count);
                DebugLog.Debug(log, "SSSSession - " + "Session关闭：" + this.roomInfo.gameId + (this.roomInfo.canRemove ? " 可移除" : " 不移除"));
                //DebugLog.Debug(log, "SSSSession - " + "Session房间号1：" + this.roomInfo.roomId);
                if (this.roomInfo.canRemove)
                {
                    this.roomInfo = new RoomInfo();
                    sessionList.Remove(this);
                }
                //DebugLog.Debug(log, "SSSSession - " + "Session房间号2：" + this.roomInfo.roomId);
                DebugLog.Debug(log, "SSSSession - " + "Session剩余：" + sessionList.Count);
            }
        }
    }
}
