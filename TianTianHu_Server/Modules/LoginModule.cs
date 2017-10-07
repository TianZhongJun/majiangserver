using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using System.Threading.Tasks;
using Nancy;
using MJ_FormsServer.DB;
using MJ_FormsServer.Logic;

namespace MJ_FormsServer.Modules
{
    public class LoginModule : NancyModule
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public LoginModule()
        {
            Get["/dbinfo"] = p =>
            {
                DBResult result = DataBase.singleton.GetDBInfo();
                DebugLog.Debug(log, "dbinfo 返回数据库信息 - 错误码：" + result.code + " 错误信息：" + result.msg + " 最大ID：" + ((DBInfo)result.data).maxId + " 最新版本：" + ((DBInfo)result.data).version);
                return Response.AsJson(result);
            };

            Get["/login/{uid}/{nickName}/{sex}"] = p =>
            {
                DebugLog.Debug(log, "login : " + "nickName密文：" + p.nickName.ToString().Replace("ShiSanShuiFanXieGang", "\\").Replace("ShiSanShuiXieGang", "/").Replace("ShiSanShuiJiaHao", "+") + "\nnickName明文：" + EncriptAndDeciphering.DecryptDES(p.nickName.ToString().Replace("ShiSanShuiFanXieGang", "\\").Replace("ShiSanShuiXieGang", "/").Replace("ShiSanShuiJiaHao", "+"), GlobalConfig.encryptKey));

                DBResult result = DataBase.singleton.Login(p.uid, EncriptAndDeciphering.DecryptDES(p.nickName.ToString().Replace("ShiSanShuiFanXieGang", "\\").Replace("ShiSanShuiXieGang", "/").Replace("ShiSanShuiJiaHao", "+"), GlobalConfig.encryptKey), p.sex);
                return Response.AsJson(result);
            };

            Get["/getroominfo/{uid}"] = p =>
            {
                DBResult result = new DBResult() { code = 0, data = CommandHelper.MJGetRoomInfo(p.uid) };
                return Response.AsJson(result);
            };

            Get["/beatsync/{uid}/{token}"] = p =>
            {
                DBResult result = DataBase.singleton.BeatSync(p.uid, p.token);
                return Response.AsJson(result);
            };

            Get["/checkroomcard/{uid}/{token}"] = p =>
            {
                DBResult result = DataBase.singleton.CheckRoomCard(p.uid, p.token);
                return Response.AsJson(result);
            };

            Get["/uploadrecord/{uid}/{token}/{record}"] = p =>
            {
                DBResult result = DataBase.singleton.UploadRecord(p.uid, p.token, p.record);
                return Response.AsJson(result);
            };

            Get["/downloadrecord/{uid}/{token}"] = p =>
            {
                DBResult result = DataBase.singleton.DownloadRecord(p.uid, p.token);
                return Response.AsJson(result);
            };

            Get["/bindinggroupid/{uid}/{groupid}"] = p =>
            {
                DBResult result = DataBase.singleton.BindingGroupID(p.uid, p.groupid);
                return Response.AsJson(result);
            };
        }
    }
}
