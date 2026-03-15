using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Linq;
using UnityEngine;

public class AnimationStateManager : MonoBehaviour
{
	[SerializeField] GameManager GameManager;
	//各アニメーションオブジェクト
	[Header("GameObject")]
	[SerializeField] GameObject outFromBedAnimObj;
	[SerializeField] GameObject intoBedAnimObj;
	[SerializeField] GameObject BattlingAnimObj;
	[Header("Animator")]
	[SerializeField] Animator outFromBedAnimCon;
	[SerializeField] Animator intoBedAnimCon;

	UniTaskCompletionSource ArareAwakeTaskSource;
	UniTaskCompletionSource ArareSleepTaskSource;



	const string StopClock = nameof(StopClock);
	const string Out = nameof(Out);

	public ReadOnlyReactiveProperty<GameState> State => GameManager.State;

	private void Awake()
	{
		//アニメーションオブジェクトのアクティベーション
		State.Subscribe(_ =>
		{
			switch (State.CurrentValue)
			{
				case GameState.AlarmStoped:
					outFromBedAnimCon.SetTrigger(StopClock);
					break;
				case GameState.Playing:
					outFromBedAnimCon.SetTrigger(Out);
					break;
				default: break;
			}
			outFromBedAnimObj.SetActive(State.CurrentValue <= GameState.Playing);//プレイ中
			intoBedAnimObj.SetActive(State.CurrentValue == GameState.Final);
			BattlingAnimObj.SetActive(State.CurrentValue == GameState.Playing);
		}).AddTo(this);

		outFromBedAnimCon
			.GetBehaviours<ObservableStateMachineTrigger>()
			.First()
			.OnStateEnterAsObservable()
			.Subscribe(_ =>
			{
				ArareAwakeTaskSource.TrySetResult();
			})
			.AddTo(this);

		intoBedAnimCon
			.GetBehaviours<ObservableStateMachineTrigger>()
			.First()
			.OnStateEnterAsObservable()
			.Subscribe(_ =>
			{
				ArareSleepTaskSource.TrySetResult();
			})
			.AddTo(this);

		ArareAwakeTaskReset();
	}

	public UniTask OutFromBedAnimTask
		=> ArareAwakeTaskSource.Task;

	public UniTask IntoBedAnimTask
		=> ArareSleepTaskSource.Task;

	public void ArareAwakeTaskReset()
	{
		ArareAwakeTaskSource = new();
		ArareSleepTaskSource = new();
	}
}
