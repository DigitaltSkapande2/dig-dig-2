using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void Awake()
    {
		UpdateRotation();
    }
	
    private void Update()
	{
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		transform.rotation = Camera.main.transform.rotation;
	}
}
