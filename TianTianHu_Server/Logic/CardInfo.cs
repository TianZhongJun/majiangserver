using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using CardHelper;

namespace MJ_FormsServer.Logic
{
    public enum CardType
    {
        None,
        Irregular,
        Single,
        Double,
        Three,
        ThreeBandTwo,
        UnifyDouble,
        UnifyThree,
        UnifyThreeBandTwo,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        FiredKing
    }

    public class CardInfo
    {
        bool canHaveBomb = false;
        List<int> cardIds = new List<int>();
        List<int> cardNumbers = new List<int>();
        public CardType type = CardType.None;
        public int point = 0;
        public int size = 0;
        public bool isBomb = false;

        public CardInfo(bool canBomb, List<int> cards)
        {
            canHaveBomb = canBomb;
            cardIds = cards;

            //对牌面进行从大到小的排序
            for (int i = 0; i < cardIds.Count - 1; i++)
            {
                for (int j = 0; j < cardIds.Count - 1 - i; j++)
                {
                    if (cardIds[j] < cardIds[j + 1])
                    {
                        int tmp = cardIds[j];
                        cardIds[j] = cardIds[j + 1];
                        cardIds[j + 1] = tmp;
                    }
                }
            }

            cardNumbers.Clear();
            foreach (int i in cardIds)
                cardNumbers.Add(MaJiangHelper.GetPointWithId(i));

            SetCardType();

            isBomb = (type == CardType.Four || type == CardType.Five || type == CardType.Six || type == CardType.Seven || type == CardType.Eight || type == CardType.FiredKing);
        }

        public void SetCardType()
        {
            if (cardNumbers.Count == 0)
            {
                type = CardType.None;
                return;
            }

            bool isSame = true;
            foreach (int i in cardNumbers)
            {
                if (i != cardNumbers[0])
                {
                    isSame = false;
                    break;
                }
            }

            //同牌判断
            if (isSame)
            {
                point = cardNumbers[0];
                size = 1;
                switch (cardNumbers.Count)
                {
                    case 1:
                        type = CardType.Single;
                        break;
                    case 2:
                        type = CardType.Double;
                        break;
                    case 3:
                        type = CardType.Three;
                        break;
                    case 4:
                        type = canHaveBomb ? CardType.Four : CardType.Irregular;
                        break;
                    case 5:
                        type = canHaveBomb ? CardType.Five : CardType.Irregular;
                        break;
                    case 6:
                        type = canHaveBomb ? CardType.Six : CardType.Irregular;
                        break;
                    case 7:
                        type = canHaveBomb ? CardType.Seven : CardType.Irregular;
                        break;
                    case 8:
                        type = canHaveBomb ? CardType.Eight : CardType.Irregular;
                        break;
                }
                return;
            }
            //天尊牌型判断
            if (cardNumbers.Count == 4 && cardNumbers.Sum() == 54)
            {
                point = 13;
                size = 1;
                type = canHaveBomb ? CardType.FiredKing : CardType.Irregular;
                return;
            }
            //三带二
            if (cardNumbers.Count == 5 && (cardNumbers[0] == cardNumbers[1] && cardNumbers[3] == cardNumbers[4] && (cardNumbers[2] == cardNumbers[1] || cardNumbers[2] == cardNumbers[3])))
            {
                point = cardNumbers[2];
                size = 1;
                type = CardType.ThreeBandTwo;
                return;
            }

            //连对
            if (cardNumbers.Count >= 6 && cardNumbers.Count % 2 == 0)
            {
                bool isAccord = true;
                for (int i = 0; i < cardNumbers.Count; i++)
                {
                    if (cardNumbers.Count > i + 1)
                    {
                        if (i % 2 == 1)
                        {
                            if (cardNumbers[i] > 11 || cardNumbers[i] - cardNumbers[i + 1] != 1)
                            {
                                isAccord = false;
                                break;
                            }
                        }
                        else
                        {
                            if (cardNumbers[i] != cardNumbers[i + 1])
                            {
                                isAccord = false;
                                break;
                            }
                        }
                    }
                }
                if (isAccord)
                {
                    point = cardNumbers[cardNumbers.Count - 1];
                    size = cardNumbers.Count / 2;
                    type = CardType.UnifyDouble;
                    return;
                }
            }

            //飞机
            if (cardNumbers.Count >= 6 && cardNumbers.Count % 3 == 0)
            {
                bool isAccord = true;
                for (int i = 0; i < cardNumbers.Count; i++)
                {
                    if (cardNumbers.Count > i + 1)
                    {
                        if (i % 3 == 2)
                        {
                            if (cardNumbers[i] > 11 || cardNumbers[i] - cardNumbers[i + 1] != 1)
                            {
                                isAccord = false;
                                break;
                            }
                        }
                        else
                        {
                            if (cardNumbers[i] != cardNumbers[i + 1])
                            {
                                isAccord = false;
                                break;
                            }
                        }
                    }
                }
                if (isAccord)
                {
                    point = cardNumbers[cardNumbers.Count - 1];
                    size = cardNumbers.Count / 3;
                    type = CardType.UnifyThree;
                    return;
                }
            }

            //飞机带翅膀
            if (cardNumbers.Count >= 10 && cardNumbers.Count % 5 == 0)
            {
                bool isAccord = true;
                List<int> threeList = new List<int>();
                List<int> twoList = new List<int>();
                //提取飞机牌
                for (int i = 0; i < cardNumbers.Count; i++)
                {
                    if (cardNumbers.Count > i + 2)
                    {
                        if (cardNumbers[i] == cardNumbers[i + 1] && cardNumbers[i] == cardNumbers[i + 2])
                            threeList.Add(cardNumbers[i]);
                    }
                }

                //提取对牌
                foreach (int i in cardNumbers)
                {
                    if (!threeList.Contains(i))
                        twoList.Add(i);
                }

                if (threeList.Count < 2 || twoList.Count != threeList.Count * 2)
                    isAccord = false;

                for (int i = 0; i < threeList.Count; i++)
                {
                    if (threeList.Count > i + 1)
                    {
                        if (threeList[i] > 11 || threeList[i] - threeList[i + 1] != 1)
                        {
                            isAccord = false;
                            break;
                        }
                    }
                }

                for (int i = 0; i < twoList.Count; i++)
                {
                    if (twoList.Count > i + 1)
                    {
                        if (i % 2 == 0 && twoList[i] != twoList[i + 1])
                        {
                            isAccord = false;
                            break;
                        }
                    }
                }

                if (isAccord)
                {
                    point = threeList[threeList.Count - 1];
                    size = threeList.Count;
                    type = CardType.UnifyThreeBandTwo;
                    return;
                }
            }

            type = CardType.Irregular;
        }

        public static bool CardCheck(string lastCards, bool canBomb, string curCards)
        {
            List<int> lCards = MJCardHelper.DecryptIntList(lastCards);
            List<int> cCards = MJCardHelper.DecryptIntList(curCards);

            return CardCheck(lCards, canBomb, cCards);
        }

        public static bool CardCheck(List<int> lastCards, bool canBomb, List<int> curCards)
        {
            CardInfo lCardInfo = new CardInfo(true, lastCards);
            CardInfo cCardInfo = new CardInfo(canBomb, curCards);

            return CardCheck(lCardInfo, cCardInfo);
        }

        public static bool CardCheck(CardInfo lastCards, CardInfo curCards)
        {
            if (curCards.type == CardType.None)
                return true;
            if (lastCards.type == CardType.None && curCards.type != CardType.Irregular)
                return true;
            if (lastCards.isBomb && ((int)curCards.type > (int)lastCards.type || (curCards.type == lastCards.type && curCards.point > lastCards.point)))
                return true;
            if (curCards.isBomb && !lastCards.isBomb)
                return true;
            if (curCards.type != CardType.Irregular && curCards.type == lastCards.type && curCards.size == lastCards.size && curCards.point > lastCards.point)
                return true;

            return false;
        }
    }
}
