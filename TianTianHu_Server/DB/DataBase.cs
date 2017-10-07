using System;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using log4net;

namespace MJ_FormsServer.DB
{
    public struct DBResult
    {
        public int code;
        public string msg;
        public object data;
    }

    public struct UserInfo
    {
        public string userId;
        public int gameId;
        public string nickName;
        public int sex;
        public int permission;
        public int manager;
        public string token;
        public int roomCard;
        public int diamond;
        public int coin;
        public int score;
        public bool first;
        //避免登录时数据接收过多
        //public string record;
    }

    public struct DBInfo
    {
        public int maxId;
        public string version;
        public int initialRoomCard;
        public int initialDiamond;
        public int initialCoin;
        public int initialScore;
        public string splitChar;
        public string gamesNumber;
        public string roomCard;
        public string downLoadPath;
        public string notice;
        public bool isDebug;
    }

    public class DataBase
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static DataBase singleton = new DataBase();
        public static int initialRoomCard;
        public static int initialDiamond;
        public static int initialCoin;
        public static int initialScore;

        private bool initialized = false;
        public string dbType = "MySql";
        private string connectStr = ConfigurationManager.ConnectionStrings["MySqlConStr"].ToString();
        private string dbName = "henanmajiang";
        private MySqlConnection myCon;
        private MySqlCommand myCom;
        private Random rd = new Random();

        private DBResult ExecuteCommand(string comStr, bool hasData = false, bool isHold = false, bool initializing = false)
        {
            lock (singleton)
            {
                DBResult result = new DBResult();
                if (!initializing && !initialized)
                {
                    result.code = 102;
                    result.msg = GlobalConfig.GetErrMsg(102);
                    return result;
                }

                try
                {
                    if (myCon == null || myCon.State != ConnectionState.Open)
                    {
                        myCon = new MySqlConnection(connectStr);
                        myCon.Open();
                    }

                    if (myCom == null)
                        myCom = new MySqlCommand(comStr, myCon);
                    else
                        myCom.CommandText = comStr;

                    if (hasData)
                    {
                        using (MySqlDataReader myReader = myCom.ExecuteReader())
                        {
                            List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                            while (myReader.Read())
                            {
                                Dictionary<string, string> dicRow = new Dictionary<string, string>();
                                for (int i = 0; i < myReader.FieldCount; i++)
                                {
                                    dicRow.Add(myReader.GetName(i), myReader.GetValue(i).ToString());
                                }
                                dataList.Add(dicRow);
                            }
                            result.data = dataList;
                            myReader.Close();
                        }
                    }
                    else
                    {
                        myCom.ExecuteNonQuery();
                    }

                    result.code = 0;
                    result.msg = "";
                }
                catch (Exception ex)
                {
                    DisposeConnection();

                    result.code = 101;
                    result.msg = ex.Message;
                }
                finally
                {
                    if (!isHold)
                    {
                        DisposeConnection();
                    }
                }
                return result;
            }
        }

        private void DisposeConnection()
        {
            lock (singleton)
            {
                if (myCom != null)
                    myCom.Dispose();
                myCom = null;

                if (myCon != null)
                {
                    myCon.Close();
                    myCon.Dispose();
                }
                myCon = null;
            }
        }

        public DBResult InitDB()
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("create database if not exists " + dbName, false, false, true);
                if (result.code != 0)
                    return result;
                else
                {
                    result = ExecuteCommand("USE " + dbName + @";
CREATE TABLE IF NOT EXISTS `dbinfo` (
  `MaxId` int(16) NOT NULL DEFAULT '0' COMMENT '所有用户中最大ID',
  `Version` char(16) CHARACTER SET utf8 NOT NULL DEFAULT '1.0.0' COMMENT '最新版本号',
  `InitialRoomCard` int(16) NOT NULL DEFAULT '0' COMMENT '用户的初始房卡',
  `InitialDiamond` int(16) NOT NULL DEFAULT '0' COMMENT '用户的初始钻石',
  `InitialCoin` int(16) NOT NULL DEFAULT '0' COMMENT '用户的初始金币',
  `InitialScore` int(16) NOT NULL DEFAULT '0' COMMENT '用户的初始积分',
  `SplitChar` char(4) CHARACTER SET utf8 NOT NULL DEFAULT ',' COMMENT '分隔符',
  `GamesNumber` char(16) CHARACTER SET utf8 DEFAULT '8,16' COMMENT '游戏局数',
  `RoomCard` char(16) CHARACTER SET utf8 DEFAULT '4,8' COMMENT '房卡消耗（与游戏局数对应）',
  `DownLoadPath` char(255) CHARACTER SET utf8 DEFAULT 'https://fir.im/tiantianhu' COMMENT '下载地址',
  `Notice` varchar(255) CHARACTER SET utf8 DEFAULT '购买福豆请联系代理商<NoticeSplit>文明娱乐，禁止赌博。<NoticeSplit>购买福豆：请联系群主\n\n推广招募：tiantianhu08(加微信)\n\n客服咨询：zbf521zbf(加微信)' COMMENT '公告',
  `IsDebug` tinyint(1) DEFAULT '0' COMMENT '是否是调试模式'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO `dbinfo` SELECT '100000', '1.0.0', '15', '10', '100', '500', ',', '8,16', '4,8', 'https://fir.im/tiantianhu', '购买福豆请联系代理商<NoticeSplit>文明娱乐，禁止赌博。<NoticeSplit>购买福豆：请联系群主\n\n推广招募：tiantianhu08(加微信)\n\n客服咨询：zbf521zbf(加微信)', '0' FROM DUAL WHERE NOT EXISTS (SELECT * FROM dbinfo);

CREATE TABLE IF NOT EXISTS `userinfo` (
  `UserId` char(64) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '用户ID',
  `GameId` int(16) NOT NULL DEFAULT '0' COMMENT '游戏ID',
  `NickName` char(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '' COMMENT '用户昵称',
  `Sex` int(4) NOT NULL DEFAULT '1' COMMENT '性别（1：男，2：女）',
  `Permission` int(16) NOT NULL DEFAULT '3' COMMENT '用户权限',
  `Manager` int(16) unsigned NOT NULL DEFAULT '0' COMMENT '管理者或者叫上级同时也做群ID使用',
  `Token` char(128) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '用户Token',
  `HeartBeatTime` char(64) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '心跳同步时间',
  `RoomCard` int(16) NOT NULL DEFAULT '0' COMMENT '房卡',
  `Diamond` int(16) NOT NULL DEFAULT '0' COMMENT '钻石',
  `Coin` int(16) NOT NULL DEFAULT '0' COMMENT '金币',
  `Score` int(16) NOT NULL DEFAULT '0' COMMENT '积分',
  `First` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否是首次登陆',
  `Record` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '战绩',
  PRIMARY KEY (`UserId`,`GameId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `admin` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '用户ID',
  `Accounts` varchar(255) CHARACTER SET utf8 NOT NULL COMMENT '帐户',
  `NickName` char(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '' COMMENT '用户昵称',
  `Password` char(64) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '用户密码',
  `Permission` int(4) NOT NULL DEFAULT '0' COMMENT '用户权限',
  `Manager` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '管理者或者叫上级',
  `RealName` varchar(128) CHARACTER SET utf8 DEFAULT NULL COMMENT '姓名',
  `IDNumber` varchar(18) CHARACTER SET utf8 DEFAULT NULL COMMENT '身份证号',
  `GameCode` varchar(10) CHARACTER SET utf8 DEFAULT NULL COMMENT '游戏代码或游戏标识',
  `Coin` int(20) unsigned NOT NULL DEFAULT '0' COMMENT '金币',
  `Roomcard` int(20) unsigned NOT NULL DEFAULT '0' COMMENT '房卡',
  `GameId` int(16) unsigned NOT NULL DEFAULT '0' COMMENT '与UserInfor表中的GameId对应',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;

INSERT INTO `admin` SELECT '1', 'admin', '超级管理员', 'e10adc3949ba59abbe56e057f20f883e', '0', '0', '', '', 'zzmj', '99999', '99999', '100000' FROM DUAL WHERE NOT EXISTS (SELECT * FROM admin);

CREATE TABLE IF NOT EXISTS `log` (
  `UserId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `NickName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `AddTime` datetime NOT NULL,
  `RoomCard` int(255) NOT NULL,
  `UserName` varchar(60) NOT NULL,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
", false, false, true);
                    if (result.code == 0)
                    {
                        connectStr += ";Database=" + dbName;
                        initialized = true;
                    }
                    return result;
                }
            }
        }

        public DBResult GetDBInfo()
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT * FROM dbinfo", true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        DBInfo dbInfo = new DBInfo();

                        dbInfo.maxId = int.Parse(reader[0]["MaxId"]);
                        dbInfo.version = reader[0]["Version"];
                        dbInfo.initialRoomCard = int.Parse(reader[0]["InitialRoomCard"]);
                        if (initialRoomCard != dbInfo.initialRoomCard)
                            initialRoomCard = dbInfo.initialRoomCard;
                        dbInfo.initialDiamond = int.Parse(reader[0]["InitialDiamond"]);
                        if (initialDiamond != dbInfo.initialDiamond)
                            initialDiamond = dbInfo.initialDiamond;
                        dbInfo.initialCoin = int.Parse(reader[0]["InitialCoin"]);
                        if (initialCoin != dbInfo.initialCoin)
                            initialCoin = dbInfo.initialCoin;
                        dbInfo.initialScore = int.Parse(reader[0]["InitialScore"]);
                        if (initialScore != dbInfo.initialScore)
                            initialScore = dbInfo.initialScore;
                        dbInfo.splitChar = reader[0]["SplitChar"];
                        dbInfo.gamesNumber = reader[0]["GamesNumber"];
                        dbInfo.roomCard = reader[0]["RoomCard"];
                        dbInfo.downLoadPath = reader[0]["DownLoadPath"];
                        dbInfo.notice = reader[0]["Notice"];
                        dbInfo.isDebug = bool.Parse(reader[0]["IsDebug"]);

                        result.data = dbInfo;
                    }
                    else
                    {
                        result = ExecuteCommand("INSERT INTO `dbinfo` VALUES ('100000', '1.0.0', '15', '10', '100', '500', ',', '8,16', '4,8', 'https://fir.im/tiantianhu', '购买福豆请联系代理商<NoticeSplit>文明娱乐，禁止赌博。<NoticeSplit>购买福豆：请联系群主\n\n推广招募：tiantianhu08(加微信)\n\n客服咨询：zbf521zbf(加微信)', '0')");
                        if (result.code == 0)
                        {
                            DBInfo dbInfo = new DBInfo();
                            dbInfo.maxId = 100000;
                            dbInfo.version = "1.0.0";
                            dbInfo.initialRoomCard = initialRoomCard = 15;
                            dbInfo.initialDiamond = initialDiamond = 10;
                            dbInfo.initialCoin = initialCoin = 100;
                            dbInfo.initialScore = initialScore = 500;
                            dbInfo.splitChar = ",";
                            dbInfo.gamesNumber = "8,16";
                            dbInfo.roomCard = "4,8";
                            dbInfo.downLoadPath = "https://fir.im/tiantianhu";
                            dbInfo.notice = "购买福豆请联系代理商<NoticeSplit>文明娱乐，禁止赌博。<NoticeSplit>购买福豆：请联系群主\n\n推广招募：tiantianhu08(加微信)\n\n客服咨询：zbf521zbf(加微信)";
                            dbInfo.isDebug = false;

                            result.data = dbInfo;
                        }
                    }
                }
                return result;
            }
        }

        public DBResult UserExists(string userId, bool isHold = false)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT * FROM userinfo WHERE UserId='" + userId + "'", true, isHold);
                UserInfo uInfo = new UserInfo();
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        uInfo.userId = reader[0]["UserId"];
                        uInfo.gameId = int.Parse(reader[0]["GameId"]);
                        uInfo.nickName = reader[0]["NickName"];
                        uInfo.sex = int.Parse(reader[0]["Sex"]);
                        uInfo.permission = int.Parse(reader[0]["Permission"]);
                        uInfo.manager = int.Parse(reader[0]["Manager"]);
                        uInfo.token = reader[0]["Token"];
                        uInfo.roomCard = int.Parse(reader[0]["RoomCard"]);
                        uInfo.diamond = int.Parse(reader[0]["Diamond"]);
                        uInfo.coin = int.Parse(reader[0]["Coin"]);
                        uInfo.score = int.Parse(reader[0]["Score"]);
                        uInfo.first = bool.Parse(reader[0]["First"]);
                        //uInfo.record = reader[0]["Record"];
                    }
                    result.data = uInfo;
                }
                return result;
            }
        }

        public int GetGameId()
        {
            lock (singleton)
            {
                //生成随机值
                string ticks = DateTime.Now.Ticks.ToString();
                int gameId = int.Parse(ticks.Substring(ticks.Length - 6));
                if (gameId < 100000)
                    gameId += 100000;

                DBResult result;
                List<Dictionary<string, string>> reader;
                while (true)
                {
                    result = ExecuteCommand("SELECT GameId FROM userinfo WHERE GameId='" + gameId + "'", true);
                    if (result.code == 0)
                    {
                        reader = result.data as List<Dictionary<string, string>>;
                        if (reader.Count > 0)
                        {
                            gameId++;
                        }
                        else
                            break;
                    }
                }
                return gameId;
            }
        }

        public DBResult Login(string userId, string nickName, string sex)
        {
            lock (singleton)
            {
                DBResult result = UserExists(userId, true);

                if (result.code == 0)
                {
                    UserInfo uInfo = (UserInfo)result.data;
                    if (!string.IsNullOrEmpty(uInfo.userId))
                    {
                        uInfo.token = Guid.NewGuid().ToString();
                        uInfo.first = false;
                        result = ExecuteCommand("update userinfo set Token='" + uInfo.token + "',HeartBeatTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',`First`='0' where UserId='" + uInfo.userId + "'");
                        if (result.code == 0)
                            result.data = uInfo;
                    }
                    else
                    {
                        int gameId = GetGameId();
                        string token = Guid.NewGuid().ToString();
                        result = ExecuteCommand("INSERT INTO `userinfo` VALUES ('" + userId + "', '" + gameId + "', '" + nickName + "', '" + sex + "', '3', '0', '" + token + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + initialRoomCard + "', '" + initialDiamond + "', '" + initialCoin + "', '" + initialScore + "', '1' ,'')", false, true);
                        DebugLog.Debug(log, nickName + " 登录结果 " + result.code + " " + result.msg);
                        if (result.code == 0)
                        {
                            uInfo.userId = userId;
                            uInfo.gameId = gameId;
                            uInfo.nickName = nickName;
                            uInfo.sex = int.Parse(sex);
                            uInfo.permission = 3;
                            uInfo.manager = 0;
                            uInfo.token = token;
                            uInfo.roomCard = initialRoomCard;
                            uInfo.diamond = initialDiamond;
                            uInfo.coin = initialCoin;
                            uInfo.score = initialScore;
                            uInfo.first = true;
                            result.data = uInfo;
                        }
                    }
                }
                return result;
            }
        }

        public DBResult BeatSync(string userId, string token)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT Token, HeartBeatTime FROM userinfo WHERE UserId = '" + userId + "'", true, true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        //if (token == reader[0]["Token"] && (DateTime.Now - DateTime.Parse(reader[0]["HeartBeatTime"])).TotalSeconds < 60)
                        if (string.IsNullOrEmpty(token) || token == reader[0]["Token"])
                        {
                            result = ExecuteCommand("update userinfo set HeartBeatTime = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where UserId = '" + userId + "'");
                            if (result.code == 0)
                                result.data = true;
                        }
                        else
                        {
                            DisposeConnection();
                            result.data = false;
                        }
                    }
                    else
                    {
                        DisposeConnection();
                        result.code = 104;
                        result.msg = GlobalConfig.GetErrMsg(104);
                    }
                }
                return result;
            }
        }

        public DBResult CheckRoomCard(string userId, string token)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT Token, RoomCard FROM userinfo WHERE UserId = '" + userId + "'", true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        if (string.IsNullOrEmpty(token) || token == reader[0]["Token"])
                        {
                            int roomCardNumber = 0;
                            if (int.TryParse(reader[0]["RoomCard"], out roomCardNumber))
                            {
                                result.code = 0;
                                result.data = roomCardNumber;
                            }
                            else
                            {
                                result.code = 106;
                                result.msg = GlobalConfig.GetErrMsg(106);
                            }
                        }
                        else
                        {
                            result.code = 105;
                            result.msg = GlobalConfig.GetErrMsg(105);
                        }
                    }
                    else
                    {
                        result.code = 104;
                        result.msg = GlobalConfig.GetErrMsg(104);
                    }
                }
                return result;
            }
        }

        public DBResult ConsumeRoomCard(string userId, string token, int roomCard)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT Token, RoomCard FROM userinfo WHERE UserId = '" + userId + "'", true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        if (string.IsNullOrEmpty(token) || token == reader[0]["Token"])
                        {
                            int roomCardNumber = 0;
                            if (int.TryParse(reader[0]["RoomCard"], out roomCardNumber))
                            {
                                int surplusRoomCard = roomCardNumber - roomCard;
                                if (surplusRoomCard >= 0)
                                {
                                    result = ExecuteCommand("update userinfo set RoomCard = '" + surplusRoomCard + "' where UserId = '" + userId + "'");
                                }
                                else
                                {
                                    result.code = 116;
                                    result.msg = GlobalConfig.GetErrMsg(116);
                                }
                            }
                            else
                            {
                                result.code = 106;
                                result.msg = GlobalConfig.GetErrMsg(106);
                            }
                        }
                        else
                        {
                            result.code = 105;
                            result.msg = GlobalConfig.GetErrMsg(105);
                        }
                    }
                    else
                    {
                        result.code = 104;
                        result.msg = GlobalConfig.GetErrMsg(104);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 上传战绩
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="token">用户Token</param>
        /// <param name="record">格式化后的战绩Json字符串</param>
        /// <returns></returns>
        public DBResult UploadRecord(string userId, string token, string record)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT Token, Record FROM userinfo WHERE UserId = '" + userId + "'", true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        if (string.IsNullOrEmpty(token) || token == reader[0]["Token"])
                        {
                            string newRecore = reader[0]["Record"];
                            List<RecordData> myRecordData = new List<RecordData>();
                            if (!string.IsNullOrEmpty(newRecore))
                            {
                                myRecordData = (new JavaScriptSerializer()).Deserialize<List<RecordData>>(newRecore);
                            }
                            myRecordData.Add((new JavaScriptSerializer()).Deserialize<RecordData>(record));
                            while (myRecordData.Count > 10)
                            {
                                myRecordData.RemoveAt(0);
                            }
                            newRecore = (new JavaScriptSerializer()).Serialize(myRecordData);

                            result = ExecuteCommand("update userinfo set Record = '" + newRecore + "' where UserId = '" + userId + "'");
                            if (result.code == 0)
                                result.data = newRecore;
                        }
                        else
                        {
                            result.code = 105;
                            result.msg = GlobalConfig.GetErrMsg(105);
                        }
                    }
                    else
                    {
                        result.code = 104;
                        result.msg = GlobalConfig.GetErrMsg(104);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 下载战绩
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="token">用户Token</param>
        /// <returns></returns>
        public DBResult DownloadRecord(string userId, string token)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT Token, Record FROM userinfo WHERE UserId = '" + userId + "'", true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        if (string.IsNullOrEmpty(token) || token == reader[0]["Token"])
                        {
                            result.data = reader[0]["Record"];
                        }
                        else
                        {
                            result.code = 105;
                            result.msg = GlobalConfig.GetErrMsg(105);
                        }
                    }
                    else
                    {
                        result.code = 104;
                        result.msg = GlobalConfig.GetErrMsg(104);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 绑定群ID
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupID">群ID</param>
        /// <returns></returns>
        public DBResult BindingGroupID(string userId, string groupID)
        {
            lock (singleton)
            {
                DBResult result = ExecuteCommand("SELECT GameId FROM admin WHERE GameId = " + groupID, true);
                if (result.code == 0)
                {
                    List<Dictionary<string, string>> reader = result.data as List<Dictionary<string, string>>;
                    if (reader.Count > 0)
                    {
                        int gID = 0;
                        if (int.TryParse(reader[0]["GameId"], out gID) && gID > 0)
                        {
                            result = ExecuteCommand("UPDATE userinfo SET Manager = " + gID + " WHERE UserId = '" + userId + "'");
                            if (result.code == 0)
                                result.data = gID;
                        }
                        else
                        {
                            result.code = 106;
                            result.msg = GlobalConfig.GetErrMsg(106);
                        }
                    }
                    else
                    {
                        result.code = 120;
                        result.msg = GlobalConfig.GetErrMsg(120);
                    }
                }
                return result;
            }
        }
    }
}
