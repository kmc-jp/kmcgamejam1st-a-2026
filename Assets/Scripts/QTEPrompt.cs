using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QTEPrompt: MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // スロットのプレハブ
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private List<Sprite> arrowSprites; // 上、下、左、右の順で矢印のスプライトを格納
    private List<GameObject> _spawnedSlots = new ();

    public void Setup(QTEAction qteAction)
    {
        foreach (var s in _spawnedSlots) Destroy(s);
        _spawnedSlots.Clear();

        foreach (var pattern in qteAction.inputPatterns)
        {
            var slot = Instantiate(slotPrefab, transform);
            _spawnedSlots.Add(slot);
            for (int i = 0; i < 4; i++)
            {
                if (pattern[i])
                {
                    var arrow = Instantiate(arrowPrefab, slot.transform);
                    arrow.GetComponent<Image>().sprite = arrowSprites[i];

                    var rect = arrow.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);

                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }
    public void UpdateSlotBackColor(int progress)
    {
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            var image = _spawnedSlots[i].GetComponent<Image>();
            if (i < progress)
            {
                image.color = Color.green; // 進行済みのスロットは緑色
            }
            else
            {
                image.color = Color.white; // 未進行のスロットは白色
            }
        }
    }
}