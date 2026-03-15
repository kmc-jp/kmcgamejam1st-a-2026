using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Linq;
using System.Threading.Tasks;
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
			outFromBedAnimObj.SetActive(State.CurrentValue <= GameState.Playing);//プレイ中
			intoBedAnimObj.SetActive(State.CurrentValue == GameState.Final);
			BattlingAnimObj.SetActive(State.CurrentValue == GameState.Playing);
			switch (State.CurrentValue)
			{
				case GameState.AlarmStoped:
					outFromBedAnimCon.SetTrigger(StopClock);
					outFromBedAnimCon
						.GetBehaviours<ObservableStateMachineTrigger>()
						.First()
						.OnStateEnterAsObservable()
						.Subscribe(_ =>
						{
							Debug.Log("outFromBedAnimCon.GetBehaviours<ObservableStateMachineTrigger>()");
							ArareAwakeTaskSource.TrySetResult();
						})
						.AddTo(this);

					break;
				case GameState.Playing:
					outFromBedAnimCon.SetTrigger(Out);
					break;
				case GameState.Final:
					intoBedAnimCon
						.GetBehaviours<ObservableStateMachineTrigger>()
						.First()
						.OnStateEnterAsObservable()
						.Subscribe(_ =>
						{
							Debug.Log("intoBedAnimCon.GetBehaviours<ObservableStateMachineTrigger>()");
							ArareSleepTaskSource.TrySetResult();
						})
						.AddTo(this);
					break;
				default: break;
			}
		}).AddTo(this);

		//outFromBedAnimObj.SetActive(true);
		//intoBedAnimObj.SetActive(true);
		//BattlingAnimObj.SetActive(true);

		////outFromBedAnimCon
		////	.GetBehaviours<ObservableStateMachineTrigger>()
		////	.First()
		////	.OnStateEnterAsObservable()
		////	.Subscribe(_ =>
		////	{
		////		Debug.Log("outFromBedAnimCon.GetBehaviours<ObservableStateMachineTrigger>()");
		////		ArareAwakeTaskSource.TrySetResult();
		////	})
		////	.AddTo(this);

		//intoBedAnimCon
		//	.GetBehaviours<ObservableStateMachineTrigger>()
		//	.First()
		//	.OnStateEnterAsObservable()
		//	.Subscribe(_ =>
		//	{
		//		Debug.Log("intoBedAnimCon.GetBehaviours<ObservableStateMachineTrigger>()");
		//		ArareSleepTaskSource.TrySetResult();
		//	})
		//	.AddTo(this);

		//outFromBedAnimObj.SetActive(false);
		//intoBedAnimObj.SetActive(false);
		//BattlingAnimObj.SetActive(false);

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
