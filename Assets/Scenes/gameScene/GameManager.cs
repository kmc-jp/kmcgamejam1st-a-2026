using Cysharp.Threading.Tasks;
using R3;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
	InBed = 0,
	AlarmStoped = 1,
	Playing = 2,
	Final = 3,
}



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
	[SerializeField] AnimationStateManager AnimationStateManager;
	private readonly ReactiveProperty<int> _score = new(0);
	public ReadOnlyReactiveProperty<int> Score => _score;
	private Subject<int> onScoreAdded = new();
	public Observable<int> OnScoreAdded => onScoreAdded;

    UniTaskCompletionSource GameEndTaskSource;

	private readonly ReactiveProperty<GameState> _State = new();
	public ReadOnlyReactiveProperty<GameState> State => _State;

	private void Start()
	{
		// BtnStart.onClick.AddListener(() => GameStart().Forget());
		GameStart().Forget();
	}

	#region アラーム
	public async UniTask GameStart()
	{
		AnimationStateManager.ArareAwakeTaskReset();
		BtnStart.interactable = false;
		_State.Value = GameState.InBed;
		GameEndTaskSource = new UniTaskCompletionSource();
		//float waitTime = Random.Range(minWaitTime, maxWaitTime);
		//await UniTask.Delay((int)(waitTime * 1000));
		await UniTask.Delay(1000);//固定
		ClockCon.TurnOn();
		_State.Value = GameState.AlarmStoped;
		await AnimationStateManager.OutFromBedAnimTask;
		_State.Value = GameState.Playing;
		QTEManagerObj.SetActive(true);

		await GameEndTaskSource.Task;
		GameEndTaskSource = null;
	}
	#endregion

	#region QTE関連
	public async UniTask QTEEnded(int combo)
	{
		QTEManagerObj.SetActive(false);
		GameEndTaskSource.TrySetResult();
		ClockCon.TurnOff();
		BtnStart.interactable = true;
		_State.Value = GameState.Final;
		await AnimationStateManager.IntoBedAnimTask;
		//リザルト表示
		Debug.Log("リザルト表示");
		Debug.Log(_score.Value);
	}
	#endregion
	public void AddScore(int score)
	{
		_score.Value += score;
		Debug.Log($"スコア加算: {score}, 現在のスコア: {_score.Value}");
		onScoreAdded.OnNext(score);
	}
}
