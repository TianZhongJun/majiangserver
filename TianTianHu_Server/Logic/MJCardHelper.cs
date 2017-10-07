using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace MJ_FormsServer.Logic
{
    /// <summary>
    /// 棋牌点数和花色计算的辅助类
    /// </summary>
    public class MJCardHelper
    {
        //public static int GetDesignWithId(int cardId)
        //{
        //    int pokerDesign = 0;

        //    if (cardId < 0)
        //    {
        //        pokerDesign = (cardId + 1) % 4 + 3;
        //    }
        //    else
        //    {
        //        pokerDesign = cardId % 4;
        //    }

        //    return pokerDesign;
        //}

        /// <summary>
        /// 解密Int类型的List数组
        /// </summary>
        /// <param name="cardStr">待解密的字符串</param>
        /// <returns>解密后的Int类型数组</returns>
        public static List<int> DecryptIntList(string cardStr)
        {
            List<int> cards = new List<int>();
            if (!string.IsNullOrEmpty(cardStr) && !string.IsNullOrEmpty(EncriptAndDeciphering.DecryptDES(cardStr, GlobalConfig.encryptKey)))
                cards = (new JavaScriptSerializer()).Deserialize<List<int>>(EncriptAndDeciphering.DecryptDES(cardStr, GlobalConfig.encryptKey));
            return cards;
        }

        /// <summary>
        /// 加密Int类型的List数组
        /// </summary>
        /// <param name="cards">待加密的Int类型数组</param>
        /// <returns>加密后的字符串</returns>
        public static string EncryptIntList(List<int> cards)
        {
            if (cards == null)
                cards = new List<int>();
            return EncriptAndDeciphering.EncryptDES((new JavaScriptSerializer()).Serialize(cards), GlobalConfig.encryptKey);
        }

        /// <summary>
        /// 解密String类型的List数组
        /// </summary>
        /// <param name="cardStr">待解密的字符串</param>
        /// <returns>解密后的String类型数组</returns>
        public static List<string> DecryptStringList(string cardStr)
        {
            List<string> cards = new List<string>();
            if (!string.IsNullOrEmpty(cardStr) && !string.IsNullOrEmpty(EncriptAndDeciphering.DecryptDES(cardStr, GlobalConfig.encryptKey)))
                cards = (new JavaScriptSerializer()).Deserialize<List<string>>(EncriptAndDeciphering.DecryptDES(cardStr, GlobalConfig.encryptKey));
            return cards;
        }

        /// <summary>
        /// 加密String类型的List数组
        /// </summary>
        /// <param name="cards">待加密的String类型数组</param>
        /// <returns>加密后的字符串</returns>
        public static string EncryptStringList(List<string> cards)
        {
            if (cards == null)
                cards = new List<string>();
            return EncriptAndDeciphering.EncryptDES((new JavaScriptSerializer()).Serialize(cards), GlobalConfig.encryptKey);
        }
    }
}
