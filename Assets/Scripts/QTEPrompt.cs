using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QTEPrompt: MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // スロットのプレハブ
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private List<Sprite> arrowSprites; // 上、下、左、右の順で矢印のスプライトを格納
    private List<GameObject> _spawnedSlots = new ();
    private List<GameObject> _arrows = new ();

    public void Setup(QTEAction qteAction)
    {
        foreach (var s in _spawnedSlots) Destroy(s);
        _spawnedSlots.Clear();
        _arrows.Clear();

        foreach (var pattern in qteAction.inputPatterns)
        {
            var slot = Instantiate(slotPrefab, transform);
            _spawnedSlots.Add(slot);
            var arrow = Instantiate(arrowPrefab, slot.transform);
            _arrows.Add(arrow);
            arrow.GetComponent<Image>().sprite = arrowSprites[(int)pattern.Item1];
            var rect = arrow.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
        var needShift = new bool[qteAction.inputPatterns.Count];
        for (int i = 0; i < qteAction.inputPatterns.Count; i++)
        {
            needShift[i] = qteAction.inputPatterns[i].Item2;
        }
        UpdateSlotBackColor(0, needShift); // スロットの背景色を更新
    }
    public void UpdateSlotBackColor(int progress, bool[] needShift)
    {
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            var image = _spawnedSlots[i].GetComponent<Image>();
            if (i < progress)
            {
                _arrows[i].GetComponent<Image>().color = Color.darkGray; // 進行済みの矢印は灰色
                image.color = Color.green; // 進行済みのスロットは緑色
            }
            else
            {
                _arrows[i].GetComponent<Image>().color = Color.white; // 未進行の矢印はそのまま
                image.color = needShift[i] ? Color.orange : Color.white; // Shiftが必要なスロットは赤色、そうでないスロットは白色
            }
        }
    }
}