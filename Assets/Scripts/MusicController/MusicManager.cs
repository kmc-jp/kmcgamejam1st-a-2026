using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable]
public class MusicManager : MonoBehaviour
{
	[SerializeField] AudioSource AudioSource;
	[SerializeField] GameObject MusicConPrefab;
	[SerializeField] List<MusicCon> Musics;


#if UNITY_EDITOR
	/// <summary>
	/// カスタムインスペクター
	/// </summary>
	[CustomEditor(typeof(MusicManager))]
	public class MusicanagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			MusicManager Manager = target as MusicManager;

			// Musics
			List<MusicCon> MusicList = Manager.Musics;
			int i, len = MusicList.Count;

			//削除するオブジェクトのリスト
			List<MusicCon> deleteList = new();
			// リスト表示
			for(i = 0; i < len; ++i)
			{
				EditorGUILayout.BeginHorizontal();
				CreateEditor(MusicList[i])?.OnInspectorGUI();
				//if(GUILayout.Button("削除"))
				//{
				//	deleteList.Add(MusicList[i]);
				//}
				EditorGUILayout.EndHorizontal();
			}

			//削除実行
			foreach(var Removed in deleteList) MusicList.Remove(Removed);

			if(GUILayout.Button("追加"))
			{
				Undo.RecordObject(Manager, "Add Item to List");//Ctrl+Zで戻せるように

				MusicCon Con = GameObject.Instantiate(Manager.MusicConPrefab).GetComponent<MusicCon>();
				//Debug.Log($"null : {Con is null}, fake null : {Con == null}");
				Con.AudioSource = Manager.AudioSource;
				Con.OnInspectorAction = c => deleteList.Add(c);
				MusicList.Add(Con);
				EditorUtility.SetDirty(Manager);
			}

		}
	}
#endif

}
