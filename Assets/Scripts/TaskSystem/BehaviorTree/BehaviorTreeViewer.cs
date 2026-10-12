using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Game.Cozyffee.TaskSystem
{
	public class BehaviorTreeViewer : EditorWindow
	{
		private Vector2 scrollPosition;
		BehaviorTree behaviorTree;
		float indentSize = 20f;
		int depth = 0;

		float ColorAnimationDuration = 0.75f;
		Color RunningColor = Color.green;
		Color SuccessColor = new Color32(0, 255, 0, 255);
		Color FailureColor = Color.red;
		Color UtilityColor = Color.yellow;

		float contentScale = 1.0f;
		float minContentScale = 0.5f;
		float maxContentScale = 2.0f;

		GUIStyle labelStyle;

		[MenuItem("Task System/Behavior Tree Viewer")]
		public static void ShowWindow()
		{
			GetWindow<BehaviorTreeViewer>("Behavior Tree Viewer");
		}

		
		private void OnEnable()
		{
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.update += OnUpdate;
		}

		private void OnDisable()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			EditorApplication.update -= OnUpdate;
		}

		void OnSelectionChanged()
		{
			if (Selection.activeGameObject == null)
			{
				return;
			}

			BehaviorTree bt = Selection.activeGameObject.GetComponent<BehaviorTree>();

			if (bt == null)
			{
				bt = Selection.activeGameObject.GetComponentInChildren<BehaviorTree>();
			}

			if (bt != null)
			{
				behaviorTree = bt;
			}
		}

		private void OnUpdate()
		{
			if (behaviorTree == null)
			{
				OnSelectionChanged();
			}

			Repaint();
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			contentScale = EditorGUILayout.Slider("Content scale", contentScale, minContentScale, maxContentScale);
			GUILayout.EndHorizontal();
			float scaledLineHeight = EditorGUIUtility.singleLineHeight * contentScale;

			labelStyle = new GUIStyle(EditorStyles.label)
			{
				fontSize = Mathf.RoundToInt(EditorStyles.label.fontSize * contentScale)
			};

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			depth = 0;

			if (behaviorTree != null)
			{
				NPC npc = behaviorTree.GetComponentInParent<NPC>();

				GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight * contentScale));
				EditorGUILayout.LabelField($"Behavior Tree (Restarts:{behaviorTree.Restarts})", labelStyle);

				if (npc != null)
				{
					EditorGUILayout.LabelField($"NPC: {npc.gameObject.name}", labelStyle);
				}

				if (!Application.isPlaying)
				{
					if (GUILayout.Button("Draw", new GUIStyle(EditorStyles.miniButton) { fontSize = labelStyle.fontSize}))
					{
						behaviorTree.CreateBehaviorTreeInternal();
					}
				}

				GUILayout.EndHorizontal();
			}

			if (behaviorTree != null && behaviorTree.Root != null)
			{
				DrawTask(behaviorTree.Root);
			}
			else
			{
				EditorGUILayout.LabelField("Select a BehaviorTree object to begin.");
			}

			EditorGUILayout.EndScrollView();
		}

		Color GetTaskTextColor(Task task)
		{
			if (task.Active == false)
			{
				if (task.ViewParameters.FinishedStatus == TaskStatus.Inactive)
				{
					return GUI.skin.label.normal.textColor;
				}

				Color targetColor = task.ViewParameters.FinishedStatus == TaskStatus.Success ? SuccessColor : FailureColor;
				float t = Mathf.InverseLerp(0f, ColorAnimationDuration, Time.time - task.ViewParameters.FinishedTime);

				return Color.Lerp(targetColor, GUI.skin.label.normal.textColor, t);
			}
			return RunningColor;
		}


		void DrawUtilityLabel(Task task)
		{
			Color oldColor = GUI.color;
			GUI.color = UtilityColor;

			string s = $" [Utility = {Mathf.Round(task.GetUtility() * 10.0f) / 10.0f}]";

			EditorGUILayout.LabelField(s, labelStyle);

			GUI.color = oldColor;
		}


		void DrawTask(Task task, bool drawUtility = false)
		{
			Color oldColor = GUI.color;
			GUI.color = GetTaskTextColor(task);

			if (task is Composite)
			{
				DrawComposite((Composite)task, drawUtility);
			}
			else if (task is Decorator)
			{
				DrawDecorator((Decorator)task, drawUtility);
			}
			else
			{
				BeginTask(task);
				EditorGUILayout.LabelField(task.FullName, labelStyle);

				if (drawUtility)
				{
					DrawUtilityLabel(task);
				}

				EndTask(task);
			}

			GUI.color = oldColor;
		}

		void BeginTask(Task task)
		{
			GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight * contentScale));
			GUILayout.Space(indentSize * contentScale * (depth + 1));
		}

		void EndTask(Task task)
		{
			GUILayout.EndHorizontal();
			float y = GUILayoutUtility.GetLastRect().y;
			Color color = task.CompareStatus(TaskStatus.Running) ? RunningColor : new Color(0.2f, 0.2f, 0.2f);
			Rect rect = new Rect(0, y, indentSize * contentScale, EditorGUIUtility.singleLineHeight * contentScale);

			if (task.CompareStatus(TaskStatus.Running))
			{
				color = RunningColor;
			}
			else
			{
				if (task.ViewParameters.FinishedStatus == TaskStatus.Inactive)
				{
					color = new Color(0.2f, 0.2f, 0.2f);
				}
				else
				{
					Color targetColor = task.ViewParameters.FinishedStatus == TaskStatus.Success ? SuccessColor : FailureColor;
					float t = Mathf.InverseLerp(0f, ColorAnimationDuration, Time.time - task.ViewParameters.FinishedTime);
					color = Color.Lerp(targetColor, new Color(0.2f, 0.2f, 0.2f), t);
				}
			}

			EditorGUI.DrawRect(rect, color);

			if (task == behaviorTree.Root)
			{
				return;
			}

			float arrowThickness = 1f * contentScale;
			float arrowLength = (indentSize + arrowThickness) * contentScale / 2f;
			float x = (indentSize * depth + indentSize * 0.5f - arrowThickness / 2f) * contentScale;
			rect = new Rect(x, y, arrowThickness, arrowLength);
			EditorGUI.DrawRect(rect, GUI.skin.label.normal.textColor);
			y += indentSize * 0.5f * contentScale - arrowThickness;
			rect = new Rect(x, y, arrowLength - arrowThickness, arrowThickness);
			EditorGUI.DrawRect(rect, GUI.skin.label.normal.textColor);
		}


		void DrawDecorator(Decorator task, bool drawUtility = false)
		{
			BeginTask(task);
			task.ViewParameters.ShowFoldout = EditorGUILayout.Foldout(task.ViewParameters.ShowFoldout, task.FullName, labelStyle);

			if (drawUtility)
			{
				DrawUtilityLabel(task);
			}

			EndTask(task);
			depth++;

			if (task.ViewParameters.ShowFoldout)
			{
				DrawTask(task.Child);
			}

			depth--;
		}

		void DrawComposite(Composite task, bool drawUtility = false)
		{
			bool isUtilitySelector = task is UtilitySelector;
			BeginTask(task);
			task.ViewParameters.ShowFoldout = EditorGUILayout.Foldout(task.ViewParameters.ShowFoldout, task.FullName, labelStyle);

			if (drawUtility)
			{
				DrawUtilityLabel(task);
			}

			EndTask(task);
			depth++;

			if (task.ViewParameters.ShowFoldout)
			{
				foreach (Task childTask in task.Tasks)
				{
					DrawTask(childTask, isUtilitySelector);
				}
			}
			depth--;
		}



	}
}

#endif