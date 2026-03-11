using Assets.Scenes.gameScene;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] float minWaitTime = 1f;
	[SerializeField] float maxWaitTime = 3f;
	[Header("References")]
	[SerializeField] Button BtnStart;
	[SerializeField] ClockCon ClockCon;
	[SerializeField] GameObject QTEManagerObj;
	[SerializeField] QTEManager QTEManager;

	UniTaskCompletionSource GameEndTaskSource;

	private void Start()
	{
		BtnStart.onClick.AddListener(() => GameStart().Forget());
	}

	#region アラーム
	public async UniTask GameStart()
	{
		QTEManager.Reset();
		BtnStart.interactable = false;
		GameEndTaskSource = new UniTaskCompletionSource();
		float waitTime = Random.Range(minWaitTime, maxWaitTime);
		await UniTask.Delay((int)(waitTime * 1000));
		ClockCon.TurnOn();
		QTEManagerObj.SetActive(true);

		await GameEndTaskSource.Task;
		GameEndTaskSource = null;
	}
	#endregion

	#region QTE関連
	/// <summary>
	/// アラームを止める
	/// </summary>
	// public void AlarmStop()
	// {
	// 	ScoreManager.AlarmTime = ClockCon.AlarmStop();
	// }

	public void QTEEnded(int combo)
	{
		ScoreManager.Combo = combo;
		QTEManagerObj.SetActive(false);
		GameEndTaskSource.TrySetResult();
		ClockCon.TurnOff();
		BtnStart.interactable = true;
		//リザルト表示
		Debug.Log("リザルト表示");
		Debug.Log(ScoreManager.Score);
	}
	#endregion
}
