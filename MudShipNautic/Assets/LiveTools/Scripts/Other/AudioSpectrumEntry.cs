using UnityEngine;

public class AudioSpectrumEntry : MonoBehaviour
{
	[SerializeField]
	private AudioSpectrum spectrum;
	[SerializeField]
	private Transform[] cubes;
	[SerializeField]
	private AudioSpectrumGradientController _gradientController;

	public float Scale;
	public Color StartColor = Color.white;
	public Color EndColor = Color.white;
	public Color MidColor = Color.white;


	private void Update()
	{
		if (_gradientController != null)
		{
			foreach (var obj in _gradientController.spectrumObjects)
			{
				obj.GetComponent<Renderer>().material.SetColor("_ColorStart", StartColor);
				obj.GetComponent<Renderer>().material.SetColor("_ColorEnd", EndColor);
				obj.GetComponent<Renderer>().material.SetColor("_ColorMid", MidColor);
			}
		}

		for (int i = 0; i < cubes.Length; i++)
		{
			var cube = cubes[i];
			var localScale = cube.localScale;
			localScale.y = spectrum.Levels[i] * Scale;
			cube.localScale = localScale;
		}
	}
}
