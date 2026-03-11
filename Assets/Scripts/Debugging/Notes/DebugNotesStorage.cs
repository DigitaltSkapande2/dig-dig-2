using System;
using System.Collections.Generic;

using DigDig2.Util;

using UnityEngine;

namespace DigDig2.Debugging.Notes
{
	[CreateAssetMenu( fileName = "DebugNotesStorage", menuName = "Scriptable Objects/Debug Notes Storage", order = 1 )]
	public class DebugNotesStorage : ScriptableObject
	{
		public SerializableDictionary<string, DebugNoteScene> scenes = new( );
	}

	[Serializable]
	public class DebugNoteScene
	{
		public List<DebugNoteData> notes = new( );
	}

	[Serializable]
	public class DebugNoteData
	{
		public string title;
		public string note;
		public string author;

		public Vector3 position = Vector3.zero;

		public bool archived;
	}
}
