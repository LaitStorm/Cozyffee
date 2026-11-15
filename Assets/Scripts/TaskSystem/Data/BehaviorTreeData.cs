using Game.Cozyffee.TaskSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
	public class BehaviorTreeData
	{
		public SharedVariableList<NPC> NPCList = new();

		public SharedVariableList<float> FloatList = new();
		public SharedVariableList<bool> BoolList = new();
		public SharedVariableList<int> IntList = new();
		public SharedVariableList<string> StringList = new();

		public SharedVariableList<Vector3> Vector3List = new();
		public SharedVariableList<Vector2> Vector2List = new();

		public SharedVariableList<Transform> TransformList = new();
		public SharedVariableList<GameObject> GameObjectList = new();

		public SharedVariableList<Color> ColorList = new();
		public SharedVariableList<Sprite> SpriteList = new();

		public void SetNPC(string name, SharedVariable<NPC> npc) { NPCList.Set(name, npc); }
		public SharedVariable<NPC> GetNPC(string name) => NPCList.Get(name);

		public void SetFloat(string name, SharedVariable<float> value) { FloatList.Set(name, value); }
		public SharedVariable<float> GetFloat(string name) => FloatList.Get(name);

		public void SetBool(string name, SharedVariable<bool> value) { BoolList.Set(name, value); }
		public SharedVariable<bool> GetBool(string name) => BoolList.Get(name);

		public void SetInt(string name, SharedVariable<int> value) { IntList.Set(name, value); }
		public SharedVariable<int> GetInt(string name) => IntList.Get(name);

		public void SetString(string name, SharedVariable<string> value) { StringList.Set(name, value); }
		public SharedVariable<string> GetString(string name) => StringList.Get(name);

		public void SetVector3(string name, SharedVariable<Vector3> value) { Vector3List.Set(name, value); }
		public SharedVariable<Vector3> GetVector3(string name) => Vector3List.Get(name);

		public void SetVector2(string name, SharedVariable<Vector2> value) { Vector2List.Set(name, value); }
		public SharedVariable<Vector2> GetVector2(string name) => Vector2List.Get(name);

		public void SetTransform(string name, SharedVariable<Transform> value) { TransformList.Set(name, value); }
		public SharedVariable<Transform> GetTransform(string name) => TransformList.Get(name);

		public void SetGameObject(string name, SharedVariable<GameObject> value) { GameObjectList.Set(name, value); }
		public SharedVariable<GameObject> GetGameObject(string name) => GameObjectList.Get(name);

		public void SetColor(string name, SharedVariable<Color> value) { ColorList.Set(name, value); }
		public SharedVariable<Color> GetColor(string name) => ColorList.Get(name);

		public void SetSprite(string name, SharedVariable<Sprite> value) { SpriteList.Set(name, value); }
		public SharedVariable<Sprite> GetSprite(string name) => SpriteList.Get(name);
	}
}