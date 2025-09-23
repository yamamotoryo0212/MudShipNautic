using UnityEngine;
//using Unity.Cinemachine;
using System.Collections;

public class PlanarReflectionView : MonoBehaviour
{
	[Header("References")]
	//[SerializeField] private CinemachineBrain _cinemachineBrain;
	[SerializeField] private Camera _mainCamera;// ���C���J����
	[SerializeField] private Camera _reflectionCamera = null;// ���˗p�e�N�X�`�����擾���邽�߂̃��t���N�V�����J����
	[SerializeField] private GameObject _reflectionTargetPlane = null; // ���˕��ʂ��s���I�u�W�F�N�g
	[SerializeField] private Skybox _mainSkybox;
	[SerializeField] private Skybox _reflectionSkybox;


	[Header("Render Settings")]
	[SerializeField, Range(0.3f, 1.0f)] private float _resolutionScale = 1.0f;// �e�N�X�`���𑜓x�i���l���グ��قǍ����ׁj 0.3f: ��𑜓x, 1.0f: �t���𑜓x    

	[Header("Material Properties")]
	[SerializeField] private Color _reflectionColor = Color.white; // ���˂̐F
	[SerializeField, Range(0.0f, 1.0f)] private float _reflectionFactor = 1.0f; // ���ˋ��x�@0:���˂Ȃ� �x�[�X�J���[�̂݁@1:���S�ɔ��˂̂�
	[SerializeField, Range(0.0f, 1.0f)] private float _roughness = 0.0f; // �ڂ�������
	private const float _blurRadius = 5.0f; // �ڂ������a

	[Header("Internal Runtime States")]
	private RenderTexture _renderTarget; // ���t���N�V�����J�����̎B�e���ʂ��i�[����RenderTexture
	private Material _floorMaterial; // ���ʂ̃}�e���A���@�V�F�[�_�[�iPlanarReflection�j����p

	private int _lastScreenWidth;
	private int _lastScreenHeight;
	private float _lastResolutionScale;

	//private ICinemachineCamera _lastActiveVirtualCamera;

	private void Start()
	{
		/*
        if (_mainCamera == null || _reflectionTargetPlane == null || _cinemachineBrain == null)
        {
            Debug.LogError("PlanarReflection: �K�v�ȃR���|�[�l���g���ݒ肳��Ă��܂���B���C���J�����A���˕��ʁACinemachineBrain���m�F���Ă��������B");
            enabled = false;
            return;
        }
        */

		//���˕��ʂ̃}�e���A���擾
		Renderer renderer = _reflectionTargetPlane.GetComponent<Renderer>();
		_floorMaterial = renderer.sharedMaterial;


		// �J�����R���|�[�l���g�������F���t���N�V�����J������Unity�̃f�t�H���g�����_�����O�t���[�ɂ͎Q���������A�s�v�ȃ����_�����O�⏇���̖��������
		_reflectionCamera.enabled = false;

		// �����X�N���[���T�C�Y�ƃX�P�[��
		_lastScreenWidth = Screen.width;
		_lastScreenHeight = Screen.height;
		_lastResolutionScale = _resolutionScale;

		CreateRenderTarget();
	}

	void Update()
	{
		if (_floorMaterial != null)
		{
			_floorMaterial.SetColor("_BaseColor", _reflectionColor);
			_floorMaterial.SetFloat("_reflectionFactor", _reflectionFactor);
			_floorMaterial.SetFloat("_Roughness", _roughness);
		}

		// �X�N���[���T�C�Y or �𑜓x�X�P�[���ύX���o
		if (Screen.width != _lastScreenWidth ||
			Screen.height != _lastScreenHeight ||
			!Mathf.Approximately(_resolutionScale, _lastResolutionScale))
		//|| _lastActiveVirtualCamera != _cinemachineBrain.ActiveVirtualCamera) �V�l�}�V�[�����g�p����ꍇ�͂��̏�����if���ɒǉ�
		{
			_lastScreenWidth = Screen.width;
			_lastScreenHeight = Screen.height;
			_lastResolutionScale = _resolutionScale;
			RecreateRenderTarget();

			//_lastActiveVirtualCamera = _cinemachineBrain.ActiveVirtualCamera;
		}
	}

	private void LateUpdate()
	{
		// �t���[���I�����ɔ��˕`��
		StartCoroutine(RenderReflectionAtEndOfFrame());
	}

	private IEnumerator RenderReflectionAtEndOfFrame()
	{
		/*
        WaitForEndOfFrame �� Unity ���t���[���`����s���u���O�v�Ɏ��s����܂��B
        CinemachineBrain �� LateUpdate �� Transform �X�V �� WaitForEndOfFrame() �� ���˕`�� �Ƃ������ԂŁA�m���ɐ������ʒu�Ŕ��˂�`��ł��܂��B
        */
		yield return new WaitForEndOfFrame();
		RenderReflection();
	}


	private void CreateRenderTarget()
	{
		int width = Mathf.Max(256, Mathf.RoundToInt(Screen.width * _resolutionScale));
		int height = Mathf.Max(256, Mathf.RoundToInt(Screen.height * _resolutionScale));

		// ������RenderTexture������Ή��
		if (_renderTarget != null)
		{
			_reflectionCamera.targetTexture = null;
			_renderTarget.Release();
			DestroyImmediate(_renderTarget);
		}

		// �V����RenderTexture���쐬
		_renderTarget = new RenderTexture(width, height, 24)
		{
			name = "PlanarReflectionRT",
			useMipMap = true,
			autoGenerateMips = true
		};

		_floorMaterial.SetTexture("_ReflectionTex", _renderTarget);// �}�e���A���Ƀ��t���N�V�����e�N�X�`����ݒ�
		_floorMaterial.SetFloat("_BlurRadius", _blurRadius);
	}

	private void RecreateRenderTarget()
	{
		if (_renderTarget != null)
		{
			_reflectionCamera.targetTexture = null;
			_renderTarget.Release();
			DestroyImmediate(_renderTarget);
		}
		CreateRenderTarget();

		RenderReflection();//�J�����ύX���ɐ^�����ȏ�����u�\������邽�߂����`�悷��B
	}

	private void RenderReflection()
	{
		// ���C���J�����̐ݒ���R�s�[���A�ʒu�E�����Ȃǂ𔽉f
		_reflectionCamera.CopyFrom(_mainCamera);

		// Skybox����
		if (_mainSkybox != null && _mainSkybox.material != null)
		{
			_reflectionSkybox.material = _mainSkybox.material;
		}

		// ���[���h��Ԃł̃��C���J�����̕����E������E�ʒu
		Vector3 cameraDirectionWorldSpace = _mainCamera.transform.forward;
		Vector3 cameraUpWorldSpace = _mainCamera.transform.up;
		Vector3 cameraPositionWorldSpace = _mainCamera.transform.position;

		// ���˕��ʃI�u�W�F�N�g�̃��[�J����Ԃɕϊ�
		Vector3 cameraDirectionPlaneSpace = _reflectionTargetPlane.transform.InverseTransformDirection(cameraDirectionWorldSpace);
		Vector3 cameraUpPlaneSpace = _reflectionTargetPlane.transform.InverseTransformDirection(cameraUpWorldSpace);
		Vector3 cameraPositionPlaneSpace = _reflectionTargetPlane.transform.InverseTransformPoint(cameraPositionWorldSpace);

		// ���[�J����Ԃł͕��ʂ̖@���� (0, 1, 0) �Ɖ��肵�AY�������𔽓]���ċ��ʑΏ̂𓾂�
		cameraDirectionPlaneSpace.y *= -1.0f;
		cameraUpPlaneSpace.y *= -1.0f;
		cameraPositionPlaneSpace.y *= -1.0f;

		// �Ăу��[���h��Ԃ֕ϊ�
		cameraDirectionWorldSpace = _reflectionTargetPlane.transform.TransformDirection(cameraDirectionPlaneSpace);
		cameraUpWorldSpace = _reflectionTargetPlane.transform.TransformDirection(cameraUpPlaneSpace);
		cameraPositionWorldSpace = _reflectionTargetPlane.transform.TransformPoint(cameraPositionPlaneSpace);


		// ���˃J�����Ɉʒu�ƌ�����ݒ�
		_reflectionCamera.transform.position = cameraPositionWorldSpace;
		_reflectionCamera.transform.LookAt(cameraPositionWorldSpace + cameraDirectionWorldSpace, cameraUpWorldSpace);

		// �����_�����O�^�[�Q�b�g��ݒ肵�ĕ`��
		_reflectionCamera.targetTexture = _renderTarget;
		_reflectionCamera.Render();
	}
}