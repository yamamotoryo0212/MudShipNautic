using UnityEngine;

public class RenderTextureCombiner : MonoBehaviour
{
	// �C���X�y�N�^�[�Őݒ肷��RenderTextureA (RGB�\�[�X)
	public RenderTexture textureA;
	// �C���X�y�N�^�[�Őݒ肷��RenderTextureB (Alpha�\�[�X)
	public RenderTexture textureB;
	// ���ʂ���������RenderTextureC (�^�[�Q�b�g)
	public RenderTexture resultTexture;

	// �`�����l�������p�̃}�e���A�� (�J�X�^���V�F�[�_�[���g�p)
	public Material channelMergeMaterial;

	void Awake()
	{
		// ���s�e�X�g
		if (textureA != null && textureB != null && resultTexture != null && channelMergeMaterial != null)
		{
			MergeTextures();
		}
		else
		{
			Debug.LogError("���ׂĂ�RenderTexture�ƃ}�e���A�����C���X�y�N�^�[�ɐݒ肵�Ă��������B");
		}
	}
	private void Update()
	{
		Graphics.Blit(null, resultTexture, channelMergeMaterial);
	}

	public void MergeTextures()
	{
		if (channelMergeMaterial == null) return;

		// �^�[�Q�b�g�e�N�X�`���̃T�C�Y���\�[�X�ƈ�v���邩�m�F (�ʏ�͈�v������)
		if (resultTexture.width != textureA.width || resultTexture.height != textureA.height)
		{
			Debug.LogError("�^�[�Q�b�g��RenderTextureC�́A�\�[�X�Ɠ����T�C�Y�ł���K�v������܂��B");
			return;
		}

		// �}�e���A���Ƀe�N�X�`����ݒ�
		channelMergeMaterial.SetTexture("_TexA", textureA);
		channelMergeMaterial.SetTexture("_TexB", textureB);

		// Graphics.Blit���g���č������������s
		// source: null (��ʑS�̂�`�悷��킯�ł͂Ȃ��̂Œʏ��null��_�~�[�ŗǂ����A
		// �|�X�g�v���Z�X�p�r�łȂ���΃}�e���A�����g���Ē��ڃ^�[�Q�b�g�ɕ`��)
		// dest: resultTexture (�������ݐ�)
		// material: channelMergeMaterial (�������W�b�N�����V�F�[�_�[)
		// pass: -1 (�ŏ��̃p�X���g�p)

		// RenderTextureC���A�N�e�B�u�ȃ����_�[�^�[�Q�b�g�ɐݒ�
		RenderTexture.active = resultTexture;

		// Blit���g�p���āA�V�F�[�_�[�̌��ʂ�RenderTextureC�ɏĂ�����
		

		// �A�N�e�B�u�ȃ����_�[�^�[�Q�b�g�����ɖ߂�
		RenderTexture.active = null;

		Debug.Log("RenderTexture�̃`�����l���������������A���ʂ� resultTexture �ɏ������܂�܂����B");
	}

	private void OnDestroy()
	{
	
		// �g�p���RenderTexture��������ă��������[�N��h��
		if (textureA != null) textureA.Release();
		if (textureB != null) textureB.Release();
		if (resultTexture != null) resultTexture.Release();
	}
}