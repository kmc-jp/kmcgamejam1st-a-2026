using UnityEngine;
using UnityEngine.InputSystem;

enum QTEActionType
{
    Up,
    Down,
    Left,
    Right
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
    private int comboCount = 0;
    [SerializeField] private float defaultQTETimeLimit = 0.5f; // デフォルトのQTE時間制限（秒）
    private float qteTimeLimit = 0.5f; // QTEの時間制限（秒）
    private QTEActionType currentQTEAction;
    // ゲームの最初の入力を検出するためのInputAction
    [SerializeField] private InputAction upInputAction;
    [SerializeField] private InputAction downInputAction;
    [SerializeField] private InputAction leftInputAction;
    [SerializeField] private InputAction rightInputAction;
    void Start()
    {
        setNextQTEAction();
        // 最初の入力アクションにコールバックを設定
        upInputAction.performed += ctx => OnQTEInput(QTEActionType.Up);
        downInputAction.performed += ctx => OnQTEInput(QTEActionType.Down);
        leftInputAction.performed += ctx => OnQTEInput(QTEActionType.Left);
        rightInputAction.performed += ctx => OnQTEInput(QTEActionType.Right);
    }
    void OnEnable()
    {
        upInputAction?.Enable();
        downInputAction?.Enable();
        leftInputAction?.Enable();
        rightInputAction?.Enable();
    }
    void OnDisable()
    {
        upInputAction?.Disable();
        downInputAction?.Disable();
        leftInputAction?.Disable();
        rightInputAction?.Disable();
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
            }
        }
    }
    private void OnQTEInput(QTEActionType inputType)
    {
        if (qteTimeLimit > 0 && inputType == currentQTEAction)
        {
            Debug.Log($"QTE入力に成功: {inputType}");
            comboCount++;
            setNextQTEAction();
        }
    }
    private void setNextQTEAction()
    {
        // ランダムに次のQTEアクションを設定
        currentQTEAction = (QTEActionType)Random.Range(0, 4);
        // コンボ数に応じて時間制限を短くする
        qteTimeLimit = defaultQTETimeLimit * Mathf.Pow(0.9f, comboCount);
        Debug.Log($"次のQTEアクション: {currentQTEAction}, 時間制限: {qteTimeLimit}");
    }
}