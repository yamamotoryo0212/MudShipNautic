using UnityEngine;

namespace MS_LiveTools.Lighting
{
	public class MS_CharacterSpotLight : MonoBehaviour
	{
		[SerializeField]
		private Transform _targetCharacyer = null;
		[SerializeField]
		private float _followSpeed = 5.0f;
		[SerializeField]
		private Vector3 _offset = new Vector3(0f, 5f, -5f);
		private Vector3 targetPosition;
		public bool IsFollow = true;

		private Light _characterSpotLight = null;
		private float _defLightIntensity = 1f;

		[ColorUsage(false, true)]
		public Color LightColor = Color.white;
		private Color _lastLightColor = Color.white;

		[Range(0, 1)]
		public float LightIntensity = 1f;
		private float _lastLightIntensity = 0;

		private void Start()
		{
			_characterSpotLight = GetComponent<Light>();
			_defLightIntensity = _characterSpotLight.intensity;
		}

		private void Update()
		{
			if (LightIntensity != _lastLightIntensity)
			{
				_characterSpotLight.intensity = LightIntensity * _defLightIntensity;
				_lastLightIntensity = LightIntensity;
			}

			if (LightColor != _lastLightColor)
			{
				_characterSpotLight.color = LightColor;
				_lastLightColor = LightColor;
			}
			
		}

		void LateUpdate()
		{
			if (_targetCharacyer == null)
			{
				Debug.LogWarning("ターゲットが設定されていません。追従させるTransformを設定してください。", this);
				return;
			}

			// 1. 目的の回転（ターゲットを正確に向く回転）を計算

			// ターゲットへの方向ベクトルを計算 (目的地 - 現在地)
			Vector3 directionToTarget = _targetCharacyer.position - transform.position;

			// その方向を向くために必要な理想的な回転（クォータニオン）を計算
			// ここで計算されるのは、ターゲットに「ピッタリ」向く回転です。
			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

			// 2. 現在の回転から目的の回転へのSlerp（球面線形補間）

			// Slerpを使って、現在の回転から理想的な回転へ、滑らかに移行させます。
			// rotationSpeed * Time.deltaTime で追従速度を制御し、フレームレートに依存しない慣性を表現します。
			transform.rotation = Quaternion.Slerp(
				transform.rotation,     // 現在の回転
				targetRotation,         // 目的の回転
				_followSpeed * Time.deltaTime); // 補間係数
		}
	}
}

