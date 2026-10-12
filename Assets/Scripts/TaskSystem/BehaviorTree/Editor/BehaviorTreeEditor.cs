using UnityEditor;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	[CustomEditor(typeof(BehaviorTree))]
	public class BehaviorTreeEditor : Editor
	{
		bool showData = true;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			BehaviorTree behaviorTree = (BehaviorTree)target;
			EditorGUILayout.Space(15f);

			if (GUILayout.Button("Open Viewer"))
			{
				BehaviorTreeViewer.ShowWindow();
			}

			EditorGUILayout.Space(15f);
			showData = EditorGUILayout.Foldout(showData, "Data");

			if (showData)
			{
				EditorGUI.indentLevel++;
				DrawData(behaviorTree.Data);
				EditorGUI.indentLevel--;
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(behaviorTree);
			}
		}


		void DrawData(BehaviorTreeData data)
		{
			GUI.enabled = false;

			foreach (var v in data.NPCList.List)
			{
				EditorGUILayout.ObjectField(v.Name, v.Value, typeof(NPC), true);
			}

			foreach (var v in data.FloatList.List)
			{
				EditorGUILayout.FloatField(v.Name, v.Value);
			}

			foreach (var v in data.BoolList.List)
			{
				EditorGUILayout.Toggle(v.Name, v.Value);
			}

			foreach (var v in data.IntList.List)
			{
				EditorGUILayout.IntField(v.Name, v.Value);
			}

			foreach (var v in data.StringList.List)
			{
				EditorGUILayout.TextField(v.Name, v.Value);
			}

			foreach (var v in data.Vector3List.List)
			{
				EditorGUILayout.Vector3Field(v.Name, v.Value);
			}

			foreach (var v in data.Vector2List.List)
			{
				EditorGUILayout.Vector2Field(v.Name, v.Value);
			}

			foreach (var v in data.TransformList.List)
			{
				EditorGUILayout.ObjectField(v.Name, v.Value, typeof(Transform), true);
			}

			foreach (var v in data.GameObjectList.List)
			{
				EditorGUILayout.ObjectField(v.Name, v.Value, typeof(GameObject), true);
			}

			GUI.enabled = true;
		}


	}
}
