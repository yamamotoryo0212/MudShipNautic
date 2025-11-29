using UnityEngine;

public class Lustrousreflectionpanel : MonoBehaviour
{
	[SerializeField]
	private Material lustrousMaterial;
	public Color panelColor = Color.white;

	private Color _defColor = Color.white;

	private void Awake()
	{
		_defColor = lustrousMaterial.GetColor("_Color");
	}
	private void Update()
	{
		if (lustrousMaterial != null)
		{
			lustrousMaterial.SetColor("_Color", panelColor);
		}
	}

	private void OnDestroy()
	{
		if (lustrousMaterial != null)
		{
			lustrousMaterial.SetColor("_Color", _defColor);
		}
	}
}
