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
using MJ_FormsServer.Logic;
using log4net;

namespace MJ_FormsServer
{
    /// <summary>
    /// 房间中的玩家信息数据结构
    /// </summary>
    public struct PlayerInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId;
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int gameId;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string nickName;
        /// <summary>
        /// 用户性别(1:男，2:女)
        /// </summary>
        public int sex;
        /// <summary>
        /// 用户IP地址
        /// </summary>
        public string userIp;
        /// <summary>
        /// 用户位置的经度
        /// </summary>
        public double longitude;
        /// <summary>
        /// 用户位置的纬度
        /// </summary>
        public double latitude;
        /// <summary>
        /// 用户头像地址
        /// </summary>
        public string headImgUrl;
        /// <summary>
        /// 房间ID
        /// </summary>
        public int roomId;
        /// <summary>
        /// 是否是代开的房间
        /// </summary>
        public bool isAgent;
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
        /// 玩家选择的跑分
        /// </summary>
        public int curPaoScore;
        /// <summary>
        /// 玩家本局的杠分
        /// </summary>
        public int curGangScore;
        /// <summary>
        /// 玩家本局的胡分
        /// </summary>
        public int curHuScore;
        /// <summary>
        /// 本局输赢分数
        /// </summary>
        public int curScore;
        /// <summary>
        /// 总共输赢分数
        /// </summary>
        public int roomScore;
        /// <summary>
        /// 本局明杠的次数
        /// </summary>
        public int curMingGangTimes;
        /// <summary>
        /// 明杠的总次数
        /// </summary>
        public int mingGangTimes;
        /// <summary>
        /// 本局暗杠的次数
        /// </summary>
        public int curAnGangTimes;
        /// <summary>
        /// 暗杠的总次数
        /// </summary>
        public int anGangTimes;
        /// <summary>
        /// 点炮的次数
        /// </summary>
        public int dianPaoTimes;
        /// <summary>
        /// 接炮的次数
        /// </summary>
        public int jiePaoTimes;
        /// <summary>
        /// 自摸的次数
        /// </summary>
        public int ziMoTimes;
        /// <summary>
        /// 是否在房间内
        /// </summary>
        public bool inRoom;
        /// <summary>
        /// 是否已准备
        /// </summary>
        public bool ready;
        /// <summary>
        /// 桌面上剩余的牌
        /// </summary>
        public string deskCards;
        /// <summary>
        /// 打出去的牌
        /// </summary>
        public string throwedCards;
        /// <summary>
        /// 手中剩余的牌
        /// </summary>
        public string remainderCards;
        /// <summary>
        /// 手中碰的牌(两个元素为一组，第一个元素为牌面ID，第二个元素为目标玩家座次索引)
        /// </summary>
        public string pengCards;
        /// <summary>
        /// 手中杠的牌(两个元素为一组，第一个元素为牌面ID，第二个元素为目标玩家座次索引)
        /// </summary>
        public string gangCards;
        /// <summary>
        /// 混子值
        /// </summary>
        public int hunNumber;
        /// <summary>
        /// 当前操作的用户的座次索引
        /// </summary>
        public int oprateSeatIndex;
        /// <summary>
        /// 当前出牌的用户的座次索引
        /// </summary>
        public int discardSeatIndex;
        /// <summary>
        /// 下一个需要出牌的玩家的座次索引
        /// </summary>
        public int nextSeatIndex;
        /// <summary>
        /// 最近一局胡的人的ID
        /// </summary>
        public string lastHuUserId;
        /// <summary>
        /// 玩家摸到的牌
        /// </summary>
        public int curGetCard;
        /// <summary>
        /// 最后一个玩家出的牌
        /// </summary>
        public int lastThrowedCard;
        /// <summary>
        /// 是否已出过牌
        /// </summary>
        public bool throwed;
        /// <summary>
        /// 所属Session是否可移除
        /// </summary>
        public bool canRemove;
        /// <summary>
        /// 当前操作状态
        /// </summary>
        public string oprateState;
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public string gameState;
    }

    /// <summary>
    /// 战绩数据结构
    /// </summary>
    public struct RecordData
    {
        /// <summary>
        /// 结束时间的字符串组合
        /// </summary>
        public string endTime;
        /// <summary>
        /// 所有玩家ID的字符串组合
        /// </summary>
        public string userId;
        /// <summary>
        /// 所有玩家昵称的字符串组合
        /// </summary>
        public string nickName;
        /// <summary>
        /// 所有玩家性别的字符串组合
        /// </summary>
        public string sex;
        /// <summary>
        /// 所有玩家头像地址的字符串组合
        /// </summary>
        public string headImgUrl;
        /// <summary>
        /// 房间信息
        /// </summary>
        public string roomInfo;
        /// <summary>
        /// 混子值的组合字符串
        /// </summary>
        public string hunNumber;
        /// <summary>
        /// 房主座次索引
        /// </summary>
        public int ownerSeatIndex;
        /// <summary>
        /// 庄家座次索引的组合字符串
        /// </summary>
        public string lairdSeatIndex;
        /// <summary>
        /// 手牌的组合字符串
        /// </summary>
        public string remainderCards;
        /// <summary>
        /// 玩家信息的组合字符串
        /// </summary>
        public string playerInfo;
        /// <summary>
        /// 录像的步骤数据
        /// </summary>
        public string videoData;
    }

    public class MJSession : AppSession<MJSession>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static List<MJSession> sessionList = new List<MJSession>();
        public static List<int> roomIdList = new List<int>();

        public PlayerInfo myPlayerInfo = new PlayerInfo();
        public List<string> vote = new List<string>();
        public RecordData myRecord = new RecordData();
        public DateTime lastOperateTime = DateTime.Now;
        private Thread tdAgent;

        public void StartAgent(string parameter)
        {
            StartAgent(myPlayerInfo.roundSecond, parameter);
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
                    case "MJXuanPao":
                        CommandHelper.MJXuanPaoHandle(this, parameter.Split(':')[0], int.Parse(parameter.Split(':')[1]));
                        break;
                    case "MJDiscard":
                        CommandHelper.MJDiscardHandle(this, parameter.Split(':')[0], int.Parse(parameter.Split(':')[1]));
                        break;
                    case "MJVoteSubmit":
                        CommandHelper.MJVoteSubmitHandle(this, parameter.Split(':')[0], parameter.Split(':')[1]);
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

        public MJSession GetValidSession()
        {
            foreach (MJSession s in MJSession.sessionList)
            {
                if (s.myPlayerInfo.userId == this.myPlayerInfo.userId && s.myPlayerInfo.gameId == this.myPlayerInfo.gameId && s.myPlayerInfo.seatIndex != -1 && s.SessionID != this.SessionID)
                {
                    DebugLog.Debug(log, "SessionGetValidSession - 已找到同名Session：" + s.myPlayerInfo.gameId + " " + s.myPlayerInfo.nickName + " SessionID：" + s.SessionID);
                    return s;
                }
            }
            return this;
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

                //如果ID有效并且 与当前Session中ID不一致(this.myPlayerInfo.userId != userId 如果当前Session上绑定的信息无效时)
                if (!string.IsNullOrEmpty(userId) && this.myPlayerInfo.userId != userId)
                {
                    for (int i = sessionList.Count - 1; i >= 0; i--)
                    {
                        //如果找到同名Session
                        if (sessionList[i].myPlayerInfo.userId == userId && sessionList[i].myPlayerInfo.seatIndex != -1 && sessionList[i].SessionID != this.SessionID)
                        {
                            //只赋值第一个对象，确保只执行一次
                            if (this.myPlayerInfo.userId != userId && sessionList[i].myPlayerInfo.gameId != 0)
                            {
                                this.myPlayerInfo = sessionList[i].myPlayerInfo;
                                this.myRecord = sessionList[i].myRecord;
                                this.vote.Clear();
                                this.vote.AddRange(sessionList[i].vote);
                            }

                            DebugLog.Debug(log, "SessionUpdate - 已找到同名Session：" + sessionList[i].myPlayerInfo.gameId + " " + sessionList[i].myPlayerInfo.nickName + " SessionId：" + sessionList[i].SessionID);
                            //移除所有同userId的Session
                            sessionList.RemoveAt(i);
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
                DebugLog.Debug(log, "MJSession - " + "Session连接：" + sessionList.Count);
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
                //DebugLog.Debug(log, "MJSession - " + "Session剩余：" + sessionList.Count);
                DebugLog.Debug(log, "MJSession - " + "Session关闭：" + this.myPlayerInfo.gameId + (this.myPlayerInfo.canRemove ? " 可移除" : " 不移除"));
                //DebugLog.Debug(log, "MJSession - " + "Session房间号1：" + this.myPlayerInfo.roomId.ToString("D6"));
                if (this.myPlayerInfo.canRemove)
                {
                    this.myPlayerInfo = new PlayerInfo();
                    sessionList.Remove(this);
                }
                //DebugLog.Debug(log, "MJSession - " + "Session房间号2：" + this.myPlayerInfo.roomId.ToString("D6"));
                DebugLog.Debug(log, "MJSession - " + "Session剩余：" + sessionList.Count);
            }
        }
    }
}
