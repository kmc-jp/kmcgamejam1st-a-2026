using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3;

public class ClockCon : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float jumpHeight = 0.5f;
	[SerializeField] private float jumpSpeed = 10f;
	[SerializeField] private AudioSource audioSource;

	private readonly ReactiveProperty<bool> _isAlarming = new(false);
	private CancellationTokenSource _alarmCts;

	private void Start()
	{
		_isAlarming
			.Where(x => x == true)
			.Subscribe(_ =>
			{
				ResetCts(); // 念の為
				_alarmCts = new CancellationTokenSource();
				PlayAlarmLoop(_alarmCts.Token).Forget();
			})
			.AddTo(this);

		_isAlarming
			.Where(x => x == false)
			.Subscribe(_ => ResetCts())
			.AddTo(this);
	}

	// 外部公開用
	public void TurnOn() => _isAlarming.Value = true;
	public void TurnOff() => _isAlarming.Value = false;

	private async UniTaskVoid PlayAlarmLoop(CancellationToken ct)
	{
		Vector3 initialPosition = transform.position;
		try
		{
			while (!ct.IsCancellationRequested)
			{
				// ジャンプの計算
				float newY = initialPosition.y + Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed)) * jumpHeight;
				transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

				// 音を鳴らす
				if (!audioSource.isPlaying)
				{
					audioSource.Play();
				}

				await UniTask.Yield(PlayerLoopTiming.Update, ct);
			}
		}
		finally
		{
			transform.position = initialPosition; // 元の位置に戻す
			audioSource.Stop(); // 音を止める
		}
	}
	private void ResetCts()
	{
		_alarmCts?.Cancel();
		_alarmCts?.Dispose();
		_alarmCts = null;
	}
	private void OnDestroy()
	{
		ResetCts();
		_isAlarming.Dispose();
	}
}
