using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] ClockCon ClockCon;
	[SerializeField] GameObject QTE_Manager;
	[SerializeField] QTEManager QTEManager;
	[SerializeField] MusicManager MusicManager;

	private void Start()
	{
		GameStart();
	}

	#region アラーム
	async UniTask GameStart()
	{
		await ClockCon.AlarmTimerStart();
		ClockCon.AlarmStart();
		QTE_Manager.SetActive(true);
	}
	#endregion

	#region QTE関連
	/// <summary>
	/// アラームを止める
	/// </summary>
	public void AlarmStop()
	{
		ClockCon.AlarmStop();
	}

	public void QTEEnded(int combo)
	{
		QTE_Manager.SetActive(false);
		//リザルト表示
		Debug.Log("リザルト表示");
	}
	#endregion
}
