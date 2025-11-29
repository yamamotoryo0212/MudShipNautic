using UnityEngine;

public class Lustrouspanel : MonoBehaviour
{
	[SerializeField]
	private Material lustrousMaterial;
	public Color panelColor = Color.white;

	private Color _defColor = Color.white;

	private void Awake()
	{
		_defColor = lustrousMaterial.GetColor("_BaseColor");
	}
	private void Update()
	{
		if (lustrousMaterial != null)
		{
			lustrousMaterial.SetColor("_BaseColor", panelColor);
		}
	}

	private void OnDestroy()
	{
		if (lustrousMaterial != null)
		{
			lustrousMaterial.SetColor("_BaseColor", _defColor);
		}
	}
}
