using TMPro;
using UnityEngine;
using R3;
using UnityEngine.UI;

public class QTETimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI secondText;
    [SerializeField] private TextMeshProUGUI milliSecondText;
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
            qteManager.onTimeLimitTick.Subscribe(newTimeLeft => {
                if (newTimeLeft < 0) // 無制限なら
                {
                    timerRingSprite.fillAmount = 0f;
                    secondText.text = "0.";
                    milliSecondText.text = "000";
                    return;
                }
                timerRingSprite.fillAmount = newTimeLeft/maxTime;
                int milliSecond = (int)(1000 * newTimeLeft) % 1000;
                secondText.text = $"{Mathf.FloorToInt(newTimeLeft)}.";
                milliSecondText.text = $"{milliSecond:000}";
            }).AddTo(this);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
