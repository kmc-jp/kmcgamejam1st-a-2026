using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using NUnit.Framework;

using Random = UnityEngine.Random;

public class ClockCon : MonoBehaviour
{
	[SerializeField] Transform Transform;
    readonly Vector3 Jump = new(0, 0.5f);
	readonly Vector3 vibration = new(0.1f, 0);

	[SerializeField] float MaxAlarmTime;

	bool alarming = false;
	double alarmStartTime;
	CancellationTokenSource CTSAlarmStop = new CancellationTokenSource();
	CancellationToken CTAlarmStop;

	/// <summary>
	/// アラームのタイマーを動かす
	/// </summary>
	/// <returns></returns>
	public async UniTask AlarmTimerStart()
	{
		float alarmTime = Random.Range(1, 1 + MaxAlarmTime);
		Debug.Log(alarmTime);
		await UniTask.Delay(TimeSpan.FromSeconds(alarmTime));
	}

	/// <summary>
	/// アラームを鳴らす
	/// </summary>
	/// <returns></returns>
	public async UniTask AlarmStart()
	{
		//アラームが鳴る
		Debug.Log("Alarm Start");
		Transform.position += Jump;
		alarming = true;
		alarmStartTime = Time.time;
		CTAlarmStop = CTSAlarmStop.Token;
		await Alarming(CTAlarmStop);
	}

	/// <summary>
	/// アラームを止める
	/// </summary>
	/// <returns>止めるまでの時間</returns>
	public double AlarmStop()
	{
		alarming = false;
		CTSAlarmStop.Cancel();
		double alarmTime = Time.timeAsDouble - alarmStartTime;
		Debug.Log($"Alarm Stop  Time : {alarmTime}");
		return alarmTime;
	}

	/// <summary>
	/// 振動する
	/// </summary>
	/// <param name="CTAlarmStop"></param>
	/// <returns></returns>
	async UniTask Alarming(CancellationToken CTAlarmStop)
	{
		bool vibe = false;
		try
		{
			while(!CTAlarmStop.IsCancellationRequested)
			{
				Transform.position += vibration;
				vibe = true;
				await UniTask.Delay(1);
				Transform.position -= vibration;
				vibe = true;
				await UniTask.Delay(1);
			}
		}
		catch(OperationCanceledException) { /*何もせずにfinally*/}
		finally
		{
			if(vibe)
			{
				Transform.position -= vibration;
			}
			Debug.Log("Alarm Stop");
		}
	}

	
}
