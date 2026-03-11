using TMPro;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using R3;

public class ScoreIndicatorCon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TMP;
	[SerializeField] GameManager Manager;



    public void ShowScore()
	{
		 Manager.Score.Subscribe(score => TMP.text = $"score : {score}").AddTo(this);
	}

	public void Reset()
	{
		
	}
}
