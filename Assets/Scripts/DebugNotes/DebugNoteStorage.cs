using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DebugNotesStorage", order = 1)]
public class DebugNotesStorage : ScriptableObject
{
	public List<DebugNoteData> notes = new();
}

[Serializable]
public class DebugNoteData
{
	public string title;
	public string note;
	public string author;

	public Vector3 position = Vector3.zero;

	public bool archived = false;
}