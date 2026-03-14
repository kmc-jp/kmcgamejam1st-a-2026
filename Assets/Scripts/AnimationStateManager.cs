using R3;
using UnityEngine;

public class AnimationStateManager : MonoBehaviour
{
	[SerializeField] GameManager GameManager;
	//各アニメーションオブジェクト
	[SerializeField] GameObject outFromBedAnimObj;
	[SerializeField] GameObject intoBedAnimObj;
	[SerializeField] GameObject BattlingAnimObj;
	[SerializeField] GameObject BattlingBedAnimObj;

	public ReadOnlyReactiveProperty<GameState> State => GameManager.State;

	private void Awake()
	{
		//アニメーションオブジェクトのアクティベーション
		State.Subscribe(_ =>
		{
			outFromBedAnimObj.SetActive(State.CurrentValue == GameState.AlarmStoped);
			intoBedAnimObj.SetActive(State.CurrentValue == GameState.Final);
			BattlingAnimObj.SetActive(State.CurrentValue == GameState.Playing);
			BattlingBedAnimObj.SetActive(State.CurrentValue == GameState.Playing);
		}).AddTo(this);
	}
}
