using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
	InBed,
	AlarmStoped,
	Playing,
	Final,
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
	[SerializeField] GameObject ScoreIndicator;
	[SerializeField] AnimationStateManager AnimationStateManager;
	private readonly ReactiveProperty<int> _score = new(0);
	public ReadOnlyReactiveProperty<int> Score => _score;
	private Subject<int> onScoreAdded = new();
	public Observable<int> OnScoreAdded => onScoreAdded;

	UniTaskCompletionSource GameEndTaskSource;

	public ReadOnlyReactiveProperty<GameState> State => AnimationStateManager.State;

	private void Start()
	{
		BtnStart.onClick.AddListener(() => GameStart().Forget());
	}

	#region アラーム
	public async UniTask GameStart()
	{
		QTEManager.Reset();
		AnimationStateManager.Reset();
		BtnStart.interactable = false;
		ScoreIndicator.SetActive(false);
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
	public void QTEEnded(int combo)
	{
		QTEManagerObj.SetActive(false);
		GameEndTaskSource.TrySetResult();
		ClockCon.TurnOff();
		BtnStart.interactable = true;
		//リザルト表示
		Debug.Log("リザルト表示");
		Debug.Log(_score.Value);
		ScoreIndicator.SetActive(true);
	}
	#endregion
	public void AddScore(int score)
	{
		_score.Value += score;
		Debug.Log($"スコア加算: {score}, 現在のスコア: {_score.Value}");
		onScoreAdded.OnNext(score);
	}
}
