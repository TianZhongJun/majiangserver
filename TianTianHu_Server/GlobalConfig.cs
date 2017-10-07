using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MJ_FormsServer
{
    public class GlobalConfig
    {
        public static string encryptKey = "DouDiZhu";

        public static string GetErrMsg(int errCode)
        {
            string errMsg = "";
            switch (errCode)
            {
                case 101:
                    errMsg = "运行异常";
                    break;
                case 102:
                    errMsg = "服务器未初始化完成";
                    break;
                case 103:
                    errMsg = "用户已存在";
                    break;
                case 104:
                    errMsg = "用户不存在";
                    break;
                case 105:
                    errMsg = "用户已重新登录";
                    break;
                case 106:
                    errMsg = "数据异常";
                    break;
                case 107:
                    errMsg = "请求的参数错误";
                    break;
                case 108:
                    errMsg = "房间已存在";
                    break;
                case 109:
                    errMsg = "未知命令";
                    break;
                case 110:
                    errMsg = "服务器异常：";
                    break;
                case 111:
                    errMsg = "房间不存在";
                    break;
                case 112:
                    errMsg = "房间人数已满";
                    break;
                case 113:
                    errMsg = "房间未退出";
                    break;
                case 114:
                    errMsg = "重复操作";
                    break;
                case 115:
                    errMsg = "牌型不符";
                    break;
                case 116:
                    errMsg = "房卡不足";
                    break;
                case 117:
                    errMsg = "Session信息错误";
                    break;
                case 118:
                    errMsg = "未发现无效Session";
                    break;
                case 119:
                    errMsg = "状态错误";
                    break;
                case 120:
                    errMsg = "群ID不存在";
                    break;
                default:
                    break;
            }
            return errMsg;
        }
    }
}
