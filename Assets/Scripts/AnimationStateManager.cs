using R3;
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
			outFromBedAnimObj.SetActive((int)State.CurrentValue <= 3);//プレイ中
			intoBedAnimObj.SetActive(State.CurrentValue == GameState.Final);
			BattlingAnimObj.SetActive(State.CurrentValue == GameState.Playing);
		}).AddTo(this);
	}
}
