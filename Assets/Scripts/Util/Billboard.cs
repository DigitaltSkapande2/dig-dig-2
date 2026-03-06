using UnityEngine;

public class Billboard : MonoBehaviour
{
	[SerializeField] private bool onlyYAxis = false;

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
		if (onlyYAxis && Camera.main != null) transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, Camera.main.transform.rotation.y, transform.rotation.z));
		else transform.rotation = Camera.main.transform.rotation;
	}
}
