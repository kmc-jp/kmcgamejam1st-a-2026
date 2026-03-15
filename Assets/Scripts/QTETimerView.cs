using TMPro;
using UnityEngine;
using R3;
using UnityEngine.UI;

public class QTETimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI secondText;
    [SerializeField] private TextMeshProUGUI milliSecondText;
    [SerializeField] private TextMeshProUGUI qteCountText;
    [SerializeField] private Image timerRingSprite;
    [SerializeField] private QTEManager qteManager;
    private float maxTime;
    void Start()
    {
        if (qteManager)
        {
            // イベント購読

            // 制限時間更新時
            qteManager.onTimeLimitReset.Subscribe(newTimeLimit => {
                if (newTimeLimit < 0) // 無制限なら
                {
                    timerRingSprite.fillAmount = 1f;
                    secondText.text = "-.";
                    milliSecondText.text = "---";
                    return;
                }
                maxTime = newTimeLimit;
                timerRingSprite.fillAmount = 1f;
                int milliSecond = (int)(1000 * newTimeLimit)%1000;
                secondText.text = $"{Mathf.FloorToInt(maxTime)}.";
                milliSecondText.text = $"{milliSecond:000}";
            }).AddTo(this);

            // 残り時間減ったとき
            Observable.EveryUpdate()
                .Where(_ => qteManager.gameObject.activeSelf) // QTEManagerがアクティブなときだけ
                .Subscribe(_ => {
                var timeLeft = qteManager.TimeLeft;
                if (timeLeft <= 0f) // 無制限なら
                {
                    timerRingSprite.fillAmount = 0f;
                    secondText.text = "0.";
                    milliSecondText.text = "000";
                    return;
                }
                timerRingSprite.fillAmount = timeLeft/maxTime;
                int milliSecond = (int)(1000 * timeLeft) % 1000;
                secondText.text = $"{Mathf.FloorToInt(timeLeft)}.";
                milliSecondText.text = $"{milliSecond:000}";
            }).AddTo(this);

            qteManager.CountOfQTEs.Subscribe(count => {
                qteCountText.text = count.ToString("N0");
            }
            ).AddTo(this);
        }
    }
}
