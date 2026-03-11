using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scenes.gameScene
{
	/// <summary>
	/// スコアを管理するクラス
	/// staticクラスでよさそう
	/// </summary>
	internal static class ScoreManager
	{
		public static double AlarmTime;
		public static int Combo;

		public static double Score
			=> Combo / AlarmTime;
	}
}
