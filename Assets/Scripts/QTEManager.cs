using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Properties;

public enum QTEInputType
{
    Up,
    Down,
    Left,
    Right,
}
public struct QTEAction
{
    // (上下左右, Shiftが必要かどうか)の組み合わせで入力パターンを定義する
    public List<(QTEInputType, bool)> inputPatterns; // 入力パターンのリスト
    public float timeLimit; // 制限時間

    public QTEAction(int difficulty, float defaultTimeLimit)
    {
        Debug.Log($"難易度：{difficulty}");
        // 入力の長さは難易度に応じて増やす
        // Shiftキーが必要である確率は難易度に応じて増やす
        // int inputLength = 1 + (int) Mathf.Sqrt(difficulty * 2); // 難易度の平方根に応じて入力の長さを増やす
        int inputLength = 3 + difficulty; // 難易度（0始まり）に3を足した数を入力数にする
        inputPatterns = new List<(QTEInputType, bool)>();
        // 完全なランダムだと同じ入力が連続して出る可能性があるため、前回の入力と同じものが出ないようにする
        QTEInputType? lastInput = null;
        for (int i = 0; i < inputLength; i++)
        {
            QTEInputType inputType;
            do
            {
                inputType = (QTEInputType)Random.Range(0, 4);
            } while (inputType == lastInput);
            lastInput = inputType;
            bool requiresShift = Random.value < Mathf.Clamp(difficulty * 0.1f, 0f, 0.5f); // 難易度に応じてShiftキーが必要になる確率を設定
            inputPatterns.Add((inputType, requiresShift));
        }
        timeLimit = defaultTimeLimit * inputLength * Mathf.Clamp(1f - difficulty * 0.05f, 0.3f, 1f); // 難易度に応じて時間制限を短くする
    }

    public void SetTimeLimit(float timeLimit)
    {
        this.timeLimit = timeLimit;
    }
}

// ゲームの進行の流れ
// 1. ゲーム開始 -> アラームが鳴り始めたりプレイヤーが飛び起きるなどして一定時間が経過 -> QTE開始
// 2. QTE開始 -> プレイヤーは表示された入力パターンを時間内に入力 -> 入力成功ならスコア加算、次のQTEへ。入力失敗ならコンボリセット、次のQTEへ
// 3. コンボが一定の倍数になるごとに、アニメーションやカットインとともに大きなスコア加算が入る

class QTEManager: MonoBehaviour
{
    [SerializeField] private bool resetProgressOnMiss = true; // ミスしたときに進行状況をリセットするかどうか
    [SerializeField] private float defaultQTETimeLimit = 2.0f; // デフォルトのQTE時間制限（秒）
    private int progress = 0; // 現在のQTEアクションの進行状況を管理する変数
    private int countOfQTEs = 0; // これまでに出現したQTEの数
    private int comboCount = 0;
    private float qteTimeLimit = 1f; // QTEの時間制限（秒)
    public float TimeLeft => qteTimeLimit; // 外部から残り時間を参照できるようにするプロパティ
    private QTEAction currentQTEAction; // 現在のQTEアクション


    // UIなどに伝達するためのイベント　R3を使用
    [SerializeField] private QTEPrompt qTEPrompt;
    [SerializeField] private QTETimerView qTETimerView;
    public Subject<float> onTimeLimitReset = new(); // 制限時間がリフレッシュされたとき
    public Subject<int> onComboUpdated = new(); // コンボ数がアップデートされたとき
    public Subject<Unit> onQTECompleted = new(); // QTEが成功したとき
    public Subject<Unit> onQTEFailed = new(); // QTEが失敗したとき

	[SerializeField] GameManager GameManager;
    [SerializeField] AudioSource smallSuccessES; // シーケンス一個ごとのSE
    [SerializeField] AudioSource smallFailSE; // ミスしたときのSE
    [SerializeField] AudioSource bigSuccessES; // シーケンス完成時のSE
    // Animationへの参照
    [SerializeField] Animator playerAnimator;
    private UniTaskCompletionSource animationWaitCompletionSource;

    private CancellationTokenSource qteCts;
    private bool isPaused = false; // アニメーション中などに時間を止めるためのフラグ

    [Header("Input Actions")]
    [SerializeField] private InputAction upInputAction;
    [SerializeField] private InputAction downInputAction;
    [SerializeField] private InputAction leftInputAction;
    [SerializeField] private InputAction rightInputAction;
    [SerializeField] private InputAction shiftInputAction;

    private float[] initialTimeLimits = new float[6] {2f, 1.7f, 1.5f, 1.3f, 1.15f, 1f};
    private float[] TimeDecrease = new float[6] {0.1f, 0.085f, 0.075f, 0.065f, 0.0575f, 0.5f};

    void OnEnable()
    {
        upInputAction?.Enable();
        downInputAction?.Enable();
        leftInputAction?.Enable();
        rightInputAction?.Enable();
        shiftInputAction?.Enable();
        RunQTEAsync(CancellationToken.None).Forget();
	}

	void OnDisable()
    {
        upInputAction?.Disable();
        downInputAction?.Disable();
        leftInputAction?.Disable();
        rightInputAction?.Disable();
        shiftInputAction?.Disable();
    }


    // R3で入力をストリーム化しておく
    private Observable<(QTEInputType type, bool siShift)> OnInputAsObservable()
    {
        return Observable.EveryUpdate()
            .Where(_ => !isPaused) // ポーズ中は入力を受け付けない
            .Select(_ => GetCurrentInput())
            .Where(input => input.HasValue)
            .Select(input => input.Value);
    }

    private (QTEInputType type, bool siShift)? GetCurrentInput()
    {
        bool shiftPressed = shiftInputAction?.IsPressed() ?? false;
        if (upInputAction?.WasPressedThisFrame() == true) return (QTEInputType.Up, shiftPressed);
        if (downInputAction?.WasPressedThisFrame() == true) return (QTEInputType.Down, shiftPressed);
        if (leftInputAction?.WasPressedThisFrame() == true) return (QTEInputType.Left, shiftPressed);
        if (rightInputAction?.WasPressedThisFrame() == true) return (QTEInputType.Right, shiftPressed);
        return null; // 入力がない場合はnullを返す
    }
    public void StartQTEPhase()
    {
        qteCts?.Cancel();
        qteCts = new CancellationTokenSource();
        RunQTEAsync(qteCts.Token).Forget();
    }
    public void InterruptQTEPhase()
    {
        qteCts?.Cancel();
    }
    public void PauseQTE(bool pause) => isPaused = pause;
    private async UniTask RunQTEAsync(CancellationToken ct)
    {
        // 入力ストリームの監視を開始
        using var inputSubscription = OnInputAsObservable().Subscribe(input => OnPlayerInput(input));

        // ゲームオーバーになるまでのループ
        while (!ct.IsCancellationRequested)
        {
            SetNextQTEAction(); // 次のQTEアクションを設定
            // １つのQTEに対する待機ループ
            while (qteTimeLimit > 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct); // 毎フレーム待機して、キャンセルが要求されたらループを抜ける
                if (!isPaused)
                {
                    qteTimeLimit -= Time.deltaTime; // 時間を減らす
                }
                if (progress >= currentQTEAction.inputPatterns.Count)
                {
                    break; // すでに全ての入力を成功させている場合はループを抜ける
                }
            }
            if (progress >= currentQTEAction.inputPatterns.Count)
            {
                await UniTask.Delay(300, cancellationToken: ct); // 全ての入力を成功させた後、少し待機してから次のQTEに移る
                // QTE成功
                countOfQTEs++;
                onComboUpdated.OnNext(comboCount + 1);
                comboCount++; // コンボ数を増やす
                bigSuccessES?.Play(); // シーケンス完成のSEを再生
                Debug.Log($"QTE成功！コンボ数: {comboCount }");
                GameManager.AddScore(100 + comboCount * 10); // スコア加算
                onQTECompleted.OnNext(Unit.Default);

                // コンボ数が一定の倍数になったらアニメーションを再生
                if (comboCount % 5 == 0)
                {
                    qTEPrompt.gameObject.SetActive(false); // アニメーション中はQTEプロンプトを非表示にする
                    qTETimerView.gameObject.SetActive(false); // タイマービューも非表示にする
                    PlayAnimation();
                    animationWaitCompletionSource = new UniTaskCompletionSource();
                    await animationWaitCompletionSource.Task.AttachExternalCancellation(ct); // アニメーションの完了を待つ
                    qTEPrompt.gameObject.SetActive(true); // アニメーションが終わったらQTEプロンプトを再表示する
                    qTETimerView.gameObject.SetActive(true); // タイマービューも再表示する
                    GameManager.AddScore(100 * comboCount); // コンボボーナスのスコア加算
                }
            }
            else
            {
                // 時間切れの処理
                Debug.Log("時間切れ！QTE失敗");
                onQTEFailed.OnNext(Unit.Default);
                GameManager.QTEEnded(comboCount);
                return; // QTEフェーズを終了
            }
        }
    }
    private void OnPlayerInput((QTEInputType type, bool shift) input)
    {
        if (currentQTEAction.inputPatterns.Count <= progress)
            return; // すでに全ての入力を成功させている場合は何もしない
        
        var expectedInput = currentQTEAction.inputPatterns[progress];
        if (input.type != expectedInput.Item1 || input.shift != expectedInput.Item2)
        {
            // 入力が正しくない場合の処理
            Debug.Log("入力ミス！");
            smallFailSE?.Play(); // ミスのSEを再生
            onQTEFailed.OnNext(Unit.Default);
            comboCount = 0; // コンボ数をリセット
            if (resetProgressOnMiss)
            {
                progress = 0; // 進行状況をリセット
                UpdatePrompt(); // プロンプトの表示を更新
            }
            return;
        }
        progress++; // 進行状況を更新
        smallSuccessES?.Play(); // SEを再生
        UpdatePrompt(); // プロンプトの表示を更新
    }
    private void UpdatePrompt()
    {
        var needShift = new bool[currentQTEAction.inputPatterns.Count];
        for (int i = 0; i < currentQTEAction.inputPatterns.Count; i++)
        {
            needShift[i] = currentQTEAction.inputPatterns[i].Item2;
        }
        qTEPrompt.UpdateSlotBackColor(progress, needShift); // スロットの背景色を更新
    }
    private void SetNextQTEAction()
    {
        progress = 0; // 進行状況をリセット
        int difficulty = countOfQTEs / 10; // 難易度
        int step = countOfQTEs % 10; // その難易度内の何番目か？
        int indexOfTable = Mathf.Min(difficulty, initialTimeLimits.Length - 1); // 配列外を防止
        // ランダムに次のQTEアクションを設定
        currentQTEAction = new QTEAction(countOfQTEs / 10, defaultQTETimeLimit);
        currentQTEAction.SetTimeLimit((initialTimeLimits[indexOfTable] - step * TimeDecrease[indexOfTable]) * (indexOfTable+3));
        // コンボ数に応じて時間制限を短くする
        qteTimeLimit = currentQTEAction.timeLimit;
        qTEPrompt.Setup(currentQTEAction);
        Debug.Log($"次のQTEアクション: {string.Join(", ", System.Linq.Enumerable.Select(currentQTEAction.inputPatterns, p => $"{p.Item1}{(p.Item2 ? "+Shift" : "")}"))}, 制限時間: {qteTimeLimit:F2}秒");
        onTimeLimitReset.OnNext(qteTimeLimit);
    }
    private void PlayAnimation()
    {
        // とりあえず４つのアニメーションの中からランダムに再生する
        int animIndex = Random.Range(0, 4);
        if (animIndex == 0)
        {
            playerAnimator.SetTrigger("Bat");
        }
        else if (animIndex == 1)
        {
            playerAnimator.SetTrigger("Cyclone");
        }
        else if (animIndex == 2)
        {
            playerAnimator.SetTrigger("Kick");
        }
        else if (animIndex == 3)
        {
            playerAnimator.SetTrigger("Punch");
        }
    }
    // アニメーションの完了を通知する関数（アニメーションイベントから呼び出される想定）
    public void OnAnimationComplete()
    {
        if (animationWaitCompletionSource != null)
        {
            animationWaitCompletionSource.TrySetResult();
            animationWaitCompletionSource = null;
        }
    }
}