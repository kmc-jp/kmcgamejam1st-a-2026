using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using R3;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// (MusicManager).Musics[曲の名前].Playで音を鳴らせる
/// </summary>
[Serializable]
public class MusicManager : MonoBehaviour
{
	[SerializeField] GameObject AudioSourceProvider;
	[SerializeField] List<MusicCon> _Musics = new();

	//これを参照
	[DoNotSerialize] public Dictionary<string, MusicCon> Musics;

	private void Awake()
	{
		Musics = _Musics.ToDictionary(
			m => m.Name,
			m => m
			);
	}

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

			// _Musics
			List<MusicCon> MusicList = Manager._Musics;
			var serializedMusicField = serializedObject.FindProperty(nameof(Manager.Musics));
			int i, len = MusicList.Count;


			//削除するオブジェクトのリスト
			List<MusicCon> deleteList = new();

			if(GUILayout.Button("追加"))
			{
				Undo.RecordObject(Manager, "Add Item to List");//Ctrl+Zで戻せるように

				GameObject AudioSourceProvider = Instantiate(Manager.AudioSourceProvider);
				AudioSourceProvider.transform.parent = Manager.transform;
				AudioSource AS = AudioSourceProvider.GetComponent<AudioSource>();
				//Debug.Log($"null : {Con is null}, fake null : {Con == null}");
				MusicCon Con = new(AS);
				MusicList.Add(Con);
				EditorUtility.SetDirty(Manager);
			}

			//削除実行
			foreach(var Con in MusicList) if(Con.Delete) deleteList.Add(Con);
			foreach(var Con in deleteList) MusicList.Remove(Con);

		}
	}
#endif

}
