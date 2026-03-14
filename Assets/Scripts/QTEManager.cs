using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using System.Collections.Generic;

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

    public QTEAction(int difficluty, float defaultTimeLimit)
    {
        // 入力の長さは難易度に応じて増やす
        // Shiftキーが必要である確率は難易度に応じて増やす
        int inputLength = 1 + (int) Mathf.Sqrt(difficluty * 2); // 難易度の平方根に応じて入力の長さを増やす
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
            bool requiresShift = Random.value < Mathf.Clamp(difficluty * 0.1f, 0f, 0.5f); // 難易度に応じてShiftキーが必要になる確率を設定
            inputPatterns.Add((inputType, requiresShift));
        }
        timeLimit = defaultTimeLimit * inputLength * Mathf.Clamp(1f - difficluty * 0.05f, 0.3f, 1f); // 難易度に応じて時間制限を短くする
    }
}

// ゲームの進行の流れ
// 1. ゲーム開始前: 目覚まし時計が鳴るまでの時間をランダムに設定
// 2. ゲーム開始後: 目覚ましが鳴り始めるまでの時間をカウントダウン
// 3. 目覚まし時計が鳴り始めてからfirstInputActionを受け取れるようにし、QTEによるゲームが開始
// (ここからQTEManagerがロジックの責任を持つ部分)
// 4. 受け付けるべきQTE入力をランダムに、限度時間を回を重ねるごとに短く設定する
// 5. プレイヤーが正しい入力をした場合、コンボカウントを増やし、次のQTEに進む
// 6. 時間切れになった場合、ゲームオーバー

class QTEManager: MonoBehaviour
{
    [SerializeField] private bool resetProgressOnMiss = true; // ミスしたときに進行状況をリセットするかどうか
    [SerializeField] private float defaultQTETimeLimit = 2.0f; // デフォルトのQTE時間制限（秒）
    private int progress = 0; // 現在のQTEアクションの進行状況を管理する変数
    private int countOfQTEs = 0; // これまでに出現したQTEの数
    private int comboCount = 0;
    private float qteTimeLimit = 0.5f; // QTEの時間制限（秒
    public float TimeLeft => qteTimeLimit; // 外部から残り時間を参照できるようにするプロパティ
    private QTEAction currentQTEAction; // 現在のQTEアクション

    // ゲームの最初の入力を検出するためのInputAction
    [SerializeField] private InputAction upInputAction;
    [SerializeField] private InputAction downInputAction;
    [SerializeField] private InputAction leftInputAction;
    [SerializeField] private InputAction rightInputAction;
    [SerializeField] private InputAction shiftInputAction;

    // UIなどに伝達するためのイベント　R3を使用
    [SerializeField] private QTEPrompt qTEPrompt;
    public Subject<float> onTimeLimitReset = new(); // 制限時間がリフレッシュされたとき
    public Subject<int> onComboUpdated = new(); // コンボ数がアップデートされたとき
    public Subject<Unit> onQTECompleted = new(); // QTEが成功したとき
    public Subject<Unit> onQTEFailed = new(); // QTEが失敗したとき

	[SerializeField] GameManager GameManager;
    [SerializeField] AudioSource smallSuccessES; // シーケンス一個ごとのSE
    [SerializeField] AudioSource smallFailSE; // ミスしたときのSE
    [SerializeField] AudioSource bigSuccessES; // シーケンス完成時のSE

	public void Reset()
	{
        comboCount = 0;
	}

	void OnEnable()
    {
        countOfQTEs = 0;
        comboCount = 0;
        upInputAction?.Enable();
        downInputAction?.Enable();
        leftInputAction?.Enable();
        rightInputAction?.Enable();
        shiftInputAction?.Enable();
        SetNextQTEAction(); // 最初のQTEアクションを設定
	}

	void OnDisable()
    {
        upInputAction?.Disable();
        downInputAction?.Disable();
        leftInputAction?.Disable();
        rightInputAction?.Disable();
        shiftInputAction?.Disable();
    }

    void Update()
    {
        if (qteTimeLimit > 0)
        {
            qteTimeLimit -= Time.deltaTime;
            if (qteTimeLimit <= 0)
            {
                // 時間切れの処理
                Debug.Log("時間切れ！ゲームオーバー");
                // ゲームオーバーのロジックをここに追加
                Debug.Log($"最終コンボ数: {comboCount}");
                GameManager.QTEEnded(comboCount);
            }
        }
        // 入力の検出
        bool shiftPressed = shiftInputAction?.IsPressed() ?? false;
        QTEInputType? inputType = upInputAction?.WasPressedThisFrame() == true ? QTEInputType.Up :
                                  downInputAction?.WasPressedThisFrame() == true ? QTEInputType.Down :
                                  leftInputAction?.WasPressedThisFrame() == true ? QTEInputType.Left :
                                  rightInputAction?.WasPressedThisFrame() == true ? QTEInputType.Right : (QTEInputType?)null;
        if (inputType == null)        {
            return; // 入力がない場合は何もしない
        }
        var expectedInput = currentQTEAction.inputPatterns[progress];
        if (inputType != expectedInput.Item1 || shiftPressed != expectedInput.Item2)
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
        if (currentQTEAction.inputPatterns.Count == progress)
        {
            countOfQTEs++;
            onComboUpdated.OnNext(comboCount + 1);
            comboCount++; // コンボ数を増やす
            bigSuccessES?.Play(); // シーケンス完成のSEを再生
            Debug.Log($"QTE成功！コンボ数: {comboCount + 1}");
            GameManager.AddScore(100 + comboCount * 10); // スコア加算
            SetNextQTEAction();
            onQTECompleted.OnNext(Unit.Default);
        }
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
        countOfQTEs++;
        // ランダムに次のQTEアクションを設定
        currentQTEAction = new QTEAction(countOfQTEs / 5, defaultQTETimeLimit);
        // コンボ数に応じて時間制限を短くする
        qteTimeLimit = currentQTEAction.timeLimit;
        qTEPrompt.Setup(currentQTEAction);
        Debug.Log($"次のQTEアクション: {string.Join(", ", System.Linq.Enumerable.Select(currentQTEAction.inputPatterns, p => $"{p.Item1}{(p.Item2 ? "+Shift" : "")}"))}, 制限時間: {qteTimeLimit:F2}秒");
        onTimeLimitReset.OnNext(qteTimeLimit);
    }
}