using System;

namespace UnityEngine.Purchasing.Security
{
	public class AppleTangle
	{
		private static byte[] data = Convert.FromBase64String("wVPDlLMBJr9N4Wh6SKJSdLIaQ5lqJSxqPiIvaj4iLyRqKzo6JiMpKyMsIykrPiMlJGoLPz4iJTgjPjN7g1M4vxdEnzUV0bhvSfAfxQcXR7vf1DBG7g3BEZ5cfXmBjkUHhF4jm09KSchLRUp6yEtASMhLS0qu2+NDTEkfV0ROXE5eYZojDd48Q7S+Icd3bC1qwHkgvUfIhZSh6WWzGSARLvt6EqYQTnjGIvnFV5QvObUtFC/2LcVC/mq9geZmaiU6/HVLesb9CYUkLmopJSQuIz4jJSQ5aiUsaj85L26ooZv9OpVFD6ttgLsnMqet/11diil5Pb1wTWYcoZBFa0SQ8DlTBf/KXmGaIw3ePEO0viHHZArsvQ0HNRPtT0M2XQocW1Q+mf3BaXEN6Z8lyEtKTENgzALMvSkuT0t6y7h6YExCYUxLT09NSEtcVCI+Pjo5cGVlPRgvJiMrJCkvaiUkaj4iIzlqKS84/VH32QhuWGCNRVf8B9YUKYIByl0DkjzVeV4v6z3eg2dISUtKS+nIS3zTBmcy/afG0Za5PdG4PJg9egWLOiYvagkvOD4jLCMpKz4jJSRqCz84Kyk+Iykvajk+Kz4vJy8kPjlkejomL2oYJSU+agkLelRdR3p8en54Ji9qAyQpZHtsem5MSR9OQVlXCzqTfDWLzR+T7dPzeAixkp871DTrGFXPyc9R03cNfbjj0QrEZp772liSQhR6yEtbTEkfV2pOyEtCeshLTnpmaikvOD4jLCMpKz4vajolJiMpM8U5yyqMURFDZdj4sg4Cuipy1F+/VduRVA0aoU+nFDPOZ6F86B0GH6Y1C+LSs5uALNZuIVua6fGuUWCJVXrITvF6yEnp6klIS0hIS0h6R0xDaiskLmopLzg+IywjKSs+IyUkajo+IiU4Iz4ze1x6XkxJH05JWUcLOlx6XkxJH05JWUcLOjomL2oYJSU+PT1kKzo6Ji9kKSUnZSs6OiYvKSsPNFUGIRrcC8OOPihBWskLzXnAy/9w575FRErYQftrXGQ+n3ZHkShcTaY3c8nBGWqZco779dAFQCG1Ybbh6TvYDRkfi+VlC/mysak6h6zpBi5/aV8BXxNX+d69vNbUhRrwixIa9L450aSYLkWBMwV+kuh0szK1IYJlesuJTEJhTEtPT01ISHrL/FDL+UdMQ2DMAsy9R0tLT09KSchLS0oWPiMsIykrPi9qKDNqKyQzajorOD5qCQt6yEtoekdMQ2DMAsy9R0tLS0x6RUxJH1dZS0u1Tk96SUtLtXpXTkxZSB8Ze1l6W0xJH05AWUALOjpF13e5YQNiUIK0hP/zRJMUVpyBd2QK7L0NBzVCFHpVTEkfV2lOUnpcYMwCzL1HS0tPT0p6KHtBekNMSR9sem5MSR9OQVlXCzo6Ji9qCS84PjNqKzk5PycvOWorKSkvOj4rJCkveXwQeih7QXpDTEkfTkxZSB8Ze1l/eHt+enl8EF1HeX96eHpzeHt+ejB6yEs8ekRMSR9XRUtLtU5OSUhLeltMSR9OQFlACzo6Ji9qAyQpZHsoJi9qOT4rJC4rOC5qPi84JzlqK+KWNGh/gG+fk0WcIZ7obmlbvevmGuDAn5CutppDTX36Pz9r");

		private static int[] order = new int[61]
		{
			16, 41, 11, 19, 18, 14, 30, 56, 26, 21,
			43, 53, 57, 32, 52, 34, 35, 38, 59, 27,
			48, 59, 46, 30, 27, 58, 49, 33, 44, 55,
			34, 38, 35, 45, 52, 57, 36, 58, 52, 51,
			45, 48, 50, 58, 47, 51, 57, 55, 53, 57,
			52, 53, 58, 53, 57, 56, 56, 59, 58, 59,
			60
		};

		private static int key = 74;

		public static readonly bool IsPopulated = true;

		public static byte[] Data()
		{
			if (!IsPopulated)
			{
				return null;
			}
			return Obfuscator.DeObfuscate(data, order, key);
		}
	}
}
