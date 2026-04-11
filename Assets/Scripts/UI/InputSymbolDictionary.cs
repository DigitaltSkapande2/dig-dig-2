using System;

using DigDig2.Util;

using UnityEngine;

namespace DigDig2.UI
{
	[CreateAssetMenu( fileName = "InputSymbolDictionary", menuName = "Input System/Input Symbol Dictionary" )]
	public class InputSymbolDictionary : ScriptableObject
	{
		[SerializeField] public SerializableDictionary<String, Sprite> dictionary;
	}
}
