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
    [SerializeField] List<MusicCon> Musics;


#if UNITY_EDITOR
	/// <summary>
	/// カスタムインスペクター
	/// </summary>
	[CustomEditor(typeof(MusicManager))]
	public class MusicConEditor : Editor
	{
		bool folding = false;

		public override void OnInspectorGUI()
		{
			MusicManager Mane = target as MusicManager;

			AudioSource AS = EditorGUILayout.ObjectField("AudioSource", null, typeof(AudioSource), true) as AudioSource;
			Mane.AudioSource = AS;

			// Musics
			List<MusicCon> MusicList = Mane.Musics;
			int i, len = MusicList.Count;

			// 折りたたみ表示
			if(folding = EditorGUILayout.Foldout(folding, "Musics"))
			{
				List<MusicCon> deleteList = new();
				// リスト表示
				for(i = 0; i < len; ++i)
				{
					EditorGUILayout.BeginHorizontal();
					MusicList[i] = EditorGUILayout.ObjectField(MusicList[i], typeof(MusicCon), true) as MusicCon;
					if(GUILayout.Button("削除"))
					{
						deleteList.Add(MusicList[i]);
					}
					EditorGUILayout.EndHorizontal();
				}

				foreach(var Removed in deleteList) MusicList.Remove(Removed);

				if(GUILayout.Button("追加"))
				{
					MusicList.Add(new(AS));
				}
			}

		}
	}
#endif

}
