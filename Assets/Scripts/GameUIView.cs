using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameUIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI comboValueText;
    [SerializeField] private TextMeshProUGUI scorePlusText;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private QTEManager qteManager;
    

    void Start()
    {
        if (gameManager)
        {
            gameManager.Score.Subscribe(score =>
            {
                scoreValueText.text = $"{score:N0}";
            }).AddTo(this);
            gameManager.OnScoreAdded.Subscribe(scoreAddition =>
            {
                ShowScoreAddition(scoreAddition);
            }).AddTo(this);
        }
        if (qteManager)
        {
            qteManager.onComboUpdated.Subscribe(incrementedCombo =>
            {
                OnComboUpdated(incrementedCombo);
            });
            qteManager.onQTEFailed.Subscribe(_ =>
            {
                OnComboUpdated(0);
            });
        }
        scorePlusText.DOFade(0f, 0f);
    }

    public void OnComboUpdated(int newValue)
    {
        comboValueText.DOKill();
        if (newValue == 0) { // コンボリセット
            comboValueText.text = "0";
            return; // アニメーションは再生せず抜ける
        }
        comboValueText.text = newValue.ToString("N0");
        comboValueText.transform.DOPunchScale(0.5f * Vector3.one, 0.3f)
            .SetEase(Ease.OutBounce)
            .OnKill(() => comboValueText.transform.localScale = Vector3.one);
    }

    public void Test_AddScore(int score)
    {
        scoreValueText.text = $"{score:N0}";
    }


    public void ShowScoreAddition(int scoreAdd)
    {
        scorePlusText.text = $"+{scoreAdd:N0}";
        var rect = scorePlusText.rectTransform;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -420f);
        scorePlusText.DOFade(1f, 0f);
        scorePlusText.DOFade(0f, 0.3f).SetEase(Ease.InCubic);
        rect.DOAnchorPosY(30f, 0.3f).SetRelative(true);
    }

    void Update()
    {
        
    }
}
