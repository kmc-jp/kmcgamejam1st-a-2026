using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 次に押す方向を指示する仮UI用(できればインターフェースにしたかった)
/// </summary>
public class DirectionArrowCon : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TMP;

    Dictionary<QTEActionType, string> QTEActionTypeToArrowString = new();

	private void Awake()
	{
		QTEActionTypeToArrowString.Add(QTEActionType.Left, "←");
		QTEActionTypeToArrowString.Add(QTEActionType.Right, "→");
		QTEActionTypeToArrowString.Add(QTEActionType.Up, "↑");
		QTEActionTypeToArrowString.Add(QTEActionType.Down, "↓");

		TMP.text = "";
	}

	internal void SetDirection(QTEActionType actionType)
	{
		TMP.text=QTEActionTypeToArrowString[actionType];
	}
}