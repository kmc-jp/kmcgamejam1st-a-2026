using Assets.Scenes.gameScene;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] ClockCon ClockCon;
	[SerializeField] GameObject QTEManagerObj;
	[SerializeField] QTEManager QTEManager;

	private void Start()
	{
		
	}

	#region アラーム
	public async UniTask GameStart()
	{
		await ClockCon.AlarmTimerStart();
		ClockCon.AlarmStart();
		QTEManagerObj.SetActive(true);
	}
	#endregion

	#region QTE関連
	/// <summary>
	/// アラームを止める
	/// </summary>
	public void AlarmStop()
	{
		ScoreManager.AlarmTime = ClockCon.AlarmStop();
	}

	public void QTEEnded(int combo)
	{
		ScoreManager.Combo = combo;
		QTEManagerObj.SetActive(false);
		//リザルト表示
		Debug.Log("リザルト表示");
		Debug.Log(ScoreManager.Score);
	}
	#endregion
}
