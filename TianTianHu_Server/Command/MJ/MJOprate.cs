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
using CardHelper;

namespace MJ_FormsServer.Command.MJ
{
    public class MJOprate : CommandBase<MJSession, StringRequestInfo>
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ExecuteCommand(MJSession session, StringRequestInfo requestInfo)
        {
            DebugLog.Debug(log, requestInfo.Key + " - Start" + " : " + session.myPlayerInfo.gameId + " " + session.myPlayerInfo.nickName + " 操作码：" + session.GetValue(requestInfo, "oprateCode") + " 操作参数：" + session.GetValue(requestInfo, "oprateParameter"));

            lock (MJSession.sessionList)
            {
                session.lastOperateTime = DateTime.Now;
                SocketResult result = new SocketResult() { code = 0, command = requestInfo.Key };

                session.Update(requestInfo);

                int oprateCode;
                //操作参数(oprateCode=2时：原有杠的杠牌ID，没有原有杠的话为-1，oprateCode=3时:胡分倍数)
                int oprateParameter;

                //参数检测
                if (int.TryParse(session.GetValue(requestInfo, "oprateCode"), out oprateCode) && int.TryParse(session.GetValue(requestInfo, "oprateParameter"), out oprateParameter))
                {
                    List<MJSession> mySessionList = MJSession.sessionList.Where(p => p.myPlayerInfo.roomId == session.myPlayerInfo.roomId && p.myPlayerInfo.seatIndex != -1).ToList();
                    mySessionList.Sort((x, y) => x.myPlayerInfo.seatIndex.CompareTo(y.myPlayerInfo.seatIndex));

                    //实例化手牌
                    List<int> hCards = MJCardHelper.DecryptIntList(session.myPlayerInfo.remainderCards);

                    //过的操作
                    if (oprateCode == 0)
                    {
                        session.myPlayerInfo.oprateSeatIndex = session.myPlayerInfo.seatIndex;
                        session.myPlayerInfo.oprateState = "MJQi";
                        result.command = "MJQi";

                        CommandHelper.CheckOprate(session, "MJQi");
                    }
                    //碰的操作
                    else if (oprateCode == 1)
                    {
                        //添加碰牌
                        List<int> pengCards = MJCardHelper.DecryptIntList(session.myPlayerInfo.pengCards);
                        pengCards.Add(session.myPlayerInfo.lastThrowedCard);
                        pengCards.Add(session.myPlayerInfo.discardSeatIndex);
                        session.myPlayerInfo.pengCards = MJCardHelper.EncryptIntList(pengCards);

                        //移除 2 张相同点数手牌
                        for (int i = 0, n = 0; i < hCards.Count; i++)
                        {
                            if (MaJiangHelper.GetPointWithId(hCards[i]) == MaJiangHelper.GetPointWithId(session.myPlayerInfo.lastThrowedCard))
                            {
                                hCards.RemoveAt(i);
                                i--;
                                n++;
                                if (n == 2)
                                    break;
                            }
                        }

                        session.myPlayerInfo.remainderCards = MJCardHelper.EncryptIntList(hCards);

                        //碰后的数据更新
                        foreach (MJSession s in mySessionList)
                        {
                            s.myPlayerInfo.nextSeatIndex = session.myPlayerInfo.seatIndex;
                            //移除掉被碰的玩家已经打出的牌
                            if (s.myPlayerInfo.seatIndex == session.myPlayerInfo.discardSeatIndex)
                            {
                                //实例化打出去的牌
                                List<int> throwedCards = MJCardHelper.DecryptIntList(s.myPlayerInfo.throwedCards);
                                throwedCards.Remove(s.myPlayerInfo.lastThrowedCard);
                                s.myPlayerInfo.throwedCards = MJCardHelper.EncryptIntList(throwedCards);
                            }
                        }

                        session.myPlayerInfo.oprateState = "MJPeng";
                        result.command = "MJPeng";

                        //记录录像数据
                        foreach (MJSession s in mySessionList)
                        {
                            if (s.myPlayerInfo.seatIndex > -1)
                            {
                                s.myRecord.videoData += ("," + session.myPlayerInfo.seatIndex + " PP " + session.myPlayerInfo.discardSeatIndex + " " + session.myPlayerInfo.lastThrowedCard);
                            }
                        }

                        DebugLog.Debug(session.myPlayerInfo.nickName + " 碰 " + pengCards[pengCards.Count - 2] + " " + pengCards[pengCards.Count - 1]);
                    }
                    //杠的操作
                    else if (oprateCode == 2)
                    {
                        //实例化杠牌
                        List<int> gangCards = MJCardHelper.DecryptIntList(session.myPlayerInfo.gangCards);
                        int targetCard = (session.myPlayerInfo.curGetCard < 0 ? session.myPlayerInfo.lastThrowedCard : session.myPlayerInfo.curGetCard);
                        int targetSeatIndex = (session.myPlayerInfo.curGetCard < 0 ? session.myPlayerInfo.discardSeatIndex : session.myPlayerInfo.seatIndex);

                        bool huiTouGang = false;
                        //如果操作参数>=0则说明杠的是手中原有的4张牌
                        if (oprateParameter >= 0)
                        {
                            targetCard = oprateParameter;
                            targetSeatIndex = session.myPlayerInfo.seatIndex;
                        }
                        //如果是回头杠则移除碰牌
                        else
                        {
                            if (session.myPlayerInfo.curGetCard >= 0)
                            {
                                List<int> pengCards = MJCardHelper.DecryptIntList(session.myPlayerInfo.pengCards);
                                for (int pcIndex = 0; pcIndex < pengCards.Count; pcIndex++)
                                {
                                    if (MaJiangHelper.GetPointWithId(pengCards[pcIndex]) == MaJiangHelper.GetPointWithId(session.myPlayerInfo.curGetCard) && pcIndex % 2 == 0)
                                    {
                                        huiTouGang = true;
                                        targetSeatIndex = pengCards[pcIndex + 1];
                                        pengCards.RemoveAt(pcIndex + 1);
                                        pengCards.RemoveAt(pcIndex);
                                        session.myPlayerInfo.pengCards = MJCardHelper.EncryptIntList(pengCards);
                                        break;
                                    }
                                }
                            }
                        }

                        if (!huiTouGang)
                        {
                            //移除 3 张相同点数手牌
                            for (int i = 0, n = 0; i < hCards.Count; i++)
                            {
                                if (MaJiangHelper.GetPointWithId(hCards[i]) == MaJiangHelper.GetPointWithId(targetCard))
                                {
                                    hCards.RemoveAt(i);
                                    i--;
                                    n++;
                                    if (n == (oprateParameter >= 0 ? 4 : 3))
                                        break;
                                }
                            }
                            if (oprateParameter >= 0)
                                hCards.Add(session.myPlayerInfo.curGetCard);
                            session.myPlayerInfo.remainderCards = MJCardHelper.EncryptIntList(hCards);
                        }

                        //添加杠牌
                        gangCards.Add(targetCard);
                        gangCards.Add(targetSeatIndex);
                        session.myPlayerInfo.gangCards = MJCardHelper.EncryptIntList(gangCards);

                        //杠分计算
                        //暗杠(普通暗杠 或者 杠自己手中原有的牌)
                        if ((session.myPlayerInfo.curGetCard >= 0 && !huiTouGang) || oprateParameter >= 0)
                        {
                            int totalGangScore = 0;
                            foreach (MJSession s in mySessionList)
                            {
                                if (s.myPlayerInfo.userId != session.myPlayerInfo.userId)
                                {
                                    int tmpGangScore = 1;
                                    if (session.myPlayerInfo.zhuangJiaJiaDi && (session.myPlayerInfo.isLaird || s.myPlayerInfo.isLaird))
                                        tmpGangScore += 1;
                                    if (session.myPlayerInfo.daiPao && session.myPlayerInfo.gangPao)
                                        tmpGangScore += (session.myPlayerInfo.curPaoScore + s.myPlayerInfo.curPaoScore);
                                    s.myPlayerInfo.curGangScore -= tmpGangScore;
                                    totalGangScore += tmpGangScore;
                                }
                            }

                            session.myPlayerInfo.curAnGangTimes++;
                            session.myPlayerInfo.curGangScore += totalGangScore;
                        }
                        //明杠
                        else
                        {
                            int totalGangScore = 0;
                            foreach (MJSession s in mySessionList)
                            {
                                if (s.myPlayerInfo.seatIndex == targetSeatIndex)
                                {
                                    int tmpGangScore = 1;
                                    if (session.myPlayerInfo.zhuangJiaJiaDi && (session.myPlayerInfo.isLaird || s.myPlayerInfo.isLaird))
                                        tmpGangScore += 1;
                                    if (session.myPlayerInfo.daiPao && session.myPlayerInfo.gangPao)
                                        tmpGangScore += (session.myPlayerInfo.curPaoScore + s.myPlayerInfo.curPaoScore);
                                    s.myPlayerInfo.curGangScore -= tmpGangScore;
                                    totalGangScore += tmpGangScore;
                                    break;
                                }
                            }

                            if (!huiTouGang)
                            {
                                //移除掉被明杠的玩家已经打出的牌
                                foreach (MJSession s in mySessionList)
                                {
                                    if (s.myPlayerInfo.seatIndex == session.myPlayerInfo.discardSeatIndex)
                                    {
                                        //实例化打出去的牌
                                        List<int> throwedCards = MJCardHelper.DecryptIntList(s.myPlayerInfo.throwedCards);
                                        throwedCards.Remove(s.myPlayerInfo.lastThrowedCard);
                                        s.myPlayerInfo.throwedCards = MJCardHelper.EncryptIntList(throwedCards);
                                    }
                                }
                            }

                            session.myPlayerInfo.curMingGangTimes++;
                            session.myPlayerInfo.curGangScore += totalGangScore;
                        }

                        string gScore = "";
                        foreach (MJSession s in mySessionList)
                            gScore += s.myPlayerInfo.nickName + " " + s.myPlayerInfo.curGangScore + "；";
                        DebugLog.Debug(log, "杠分计算完毕 : " + gScore);

                        ////随机摸牌
                        //List<int> deskCards = MJCardHelper.DecryptIntList(session.GetValidSession().myPlayerInfo.deskCards);
                        //int newCardPosition = (new Random()).Next(deskCards.Count);
                        //int newCard = deskCards[newCardPosition];
                        //deskCards.RemoveAt(newCardPosition);

                        //依次摸牌
                        List<int> deskCards = MJCardHelper.DecryptIntList(session.GetValidSession().myPlayerInfo.deskCards);
                        int newCard = deskCards[0];
                        deskCards.RemoveAt(0);

                        //杠后的数据更新
                        foreach (MJSession s in mySessionList)
                        {
                            s.myPlayerInfo.deskCards = MJCardHelper.EncryptIntList(deskCards);
                            s.myPlayerInfo.curGetCard = newCard;
                            s.myPlayerInfo.nextSeatIndex = session.myPlayerInfo.seatIndex;
                        }

                        session.myPlayerInfo.oprateState = "MJGang";
                        result.command = "MJGang";
                        DebugLog.Debug(session.myPlayerInfo.nickName + " 杠 " + gangCards[gangCards.Count - 2] + " " + gangCards[gangCards.Count - 1]);

                        //记录录像数据
                        foreach (MJSession s in mySessionList)
                        {
                            if (s.myPlayerInfo.seatIndex > -1)
                            {
                                s.myRecord.videoData += ("," + session.myPlayerInfo.seatIndex + " GP " + targetSeatIndex + " " + targetCard);
                                s.myRecord.videoData += ("," + session.myPlayerInfo.seatIndex + " MP " + session.myPlayerInfo.seatIndex + " " + newCard);
                            }
                        }
                    }
                    //胡的操作
                    else if (oprateCode == 3)
                    {
                        //胡分计算
                        //自摸胡
                        if (session.myPlayerInfo.curGetCard >= 0)
                        {
                            int totalHuScore = 0;
                            foreach (MJSession s in mySessionList)
                            {
                                if (s.myPlayerInfo.userId != session.myPlayerInfo.userId)
                                {
                                    int tmpHuScore = 1;
                                    if (session.myPlayerInfo.zhuangJiaJiaDi && (session.myPlayerInfo.isLaird || s.myPlayerInfo.isLaird))
                                        tmpHuScore += 1;
                                    if (session.myPlayerInfo.daiPao)
                                        tmpHuScore += (session.myPlayerInfo.curPaoScore + s.myPlayerInfo.curPaoScore);
                                    tmpHuScore *= oprateParameter;

                                    s.myPlayerInfo.curHuScore -= tmpHuScore;
                                    totalHuScore += tmpHuScore;
                                }
                            }
                            session.myPlayerInfo.ziMoTimes++;
                            session.myPlayerInfo.curHuScore += totalHuScore;
                        }
                        //点炮胡
                        else
                        {
                            int totalHuScore = 0;
                            foreach (MJSession s in mySessionList)
                            {
                                if (s.myPlayerInfo.seatIndex == session.myPlayerInfo.discardSeatIndex)
                                {
                                    int tmpHuScore = 1;
                                    if (session.myPlayerInfo.zhuangJiaJiaDi && (session.myPlayerInfo.isLaird || s.myPlayerInfo.isLaird))
                                        tmpHuScore += 1;
                                    if (session.myPlayerInfo.daiPao)
                                        tmpHuScore += (session.myPlayerInfo.curPaoScore + s.myPlayerInfo.curPaoScore);
                                    tmpHuScore *= oprateParameter;

                                    s.myPlayerInfo.curHuScore -= tmpHuScore;
                                    s.myPlayerInfo.dianPaoTimes++;
                                    totalHuScore += tmpHuScore;
                                    break;
                                }
                            }

                            //移除掉点炮的玩家已经打出的牌
                            foreach (MJSession s in mySessionList)
                            {
                                if (s.myPlayerInfo.seatIndex == session.myPlayerInfo.discardSeatIndex)
                                {
                                    //实例化打出去的牌
                                    List<int> throwedCards = MJCardHelper.DecryptIntList(s.myPlayerInfo.throwedCards);
                                    throwedCards.Remove(s.myPlayerInfo.lastThrowedCard);
                                    s.myPlayerInfo.throwedCards = MJCardHelper.EncryptIntList(throwedCards);
                                }
                            }

                            session.myPlayerInfo.jiePaoTimes++;
                            session.myPlayerInfo.curHuScore += totalHuScore;
                        }

                        //本局结束
                        //当前局数加1
                        int nowGameSum = session.myPlayerInfo.curGameSum + 1;

                        //胡完的数据更新
                        foreach (MJSession s in mySessionList)
                        {
                            s.myPlayerInfo.curGameSum = nowGameSum;
                            s.myPlayerInfo.lastHuUserId = session.myPlayerInfo.userId;
                            s.myPlayerInfo.mingGangTimes += s.myPlayerInfo.curMingGangTimes;
                            s.myPlayerInfo.anGangTimes += s.myPlayerInfo.curAnGangTimes;
                            s.myPlayerInfo.curScore = s.myPlayerInfo.curGangScore + s.myPlayerInfo.curHuScore;
                            s.myPlayerInfo.roomScore += s.myPlayerInfo.curScore;

                            s.myPlayerInfo.inRoom = false;
                            s.myPlayerInfo.ready = false;
                            s.myPlayerInfo.oprateState = (s.myPlayerInfo.curGameSum >= s.myPlayerInfo.gameSum ? "MJRoomEnd" : "MJGameEnd");
                            s.myPlayerInfo.gameState = "MJGameEnd";
                        }
                        result.command = session.myPlayerInfo.oprateState;

                        //记录录像数据
                        foreach (MJSession s in mySessionList)
                        {
                            if (s.myPlayerInfo.seatIndex > -1)
                            {
                                s.myRecord.videoData += ("," + session.myPlayerInfo.seatIndex + " HP " + (session.myPlayerInfo.curGetCard < 0 ? session.myPlayerInfo.discardSeatIndex : session.myPlayerInfo.seatIndex) + " " + (session.myPlayerInfo.curGetCard < 0 ? session.myPlayerInfo.lastThrowedCard : session.myPlayerInfo.curGetCard));
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

                    //只有碰、杠、胡才发送数据，因为 过 有自己单独的逻辑处理及消息发送
                    if (oprateCode > 0)
                    {
                        //最终的数据更新
                        foreach (MJSession s in mySessionList)
                        {
                            s.myPlayerInfo.oprateSeatIndex = session.myPlayerInfo.seatIndex;
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

                        //向房间所有人发送结果消息
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
                }
                else
                {
                    result.code = 107;
                    result.msg = GlobalConfig.GetErrMsg(107);
                }
            }

            DebugLog.Debug(log, requestInfo.Key + " - End - 耗时：" + (DateTime.Now - session.GetValidSession().lastOperateTime).TotalSeconds + " 剩余人数：" + session.myPlayerInfo.playerSum);
        }
    }
}
