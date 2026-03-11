using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ScoreIndicatorCon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TMP;
	[SerializeField] GameManager Manager;



    public void ShowScore()
	{
		 Manager.Score.Subscribe(score => scoreUI.text = $"score").AddTo(this);
	}

	public void Reset()
	{
		
	}
}
