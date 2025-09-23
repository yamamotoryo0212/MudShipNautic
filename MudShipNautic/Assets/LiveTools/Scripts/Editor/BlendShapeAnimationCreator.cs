using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BlendShapeAnimationCreator : EditorWindow
{
	// ���j���[���ڂ�ǉ�
	[MenuItem("GameObject/MudShip/Create Facial Clips", false, 100)]
	public static void CreateAnimClips()
	{
		// �I�𒆂�GameObject���擾
		GameObject selectedObject = Selection.activeGameObject;
		if (selectedObject == null)
		{
			EditorUtility.DisplayDialog("�G���[", "Skinned Mesh Renderer������GameObject��I�����Ă��������B", "OK");
			return;
		}

		// SkinnedMeshRenderer�R���|�[�l���g���擾
		SkinnedMeshRenderer skinnedMeshRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer == null)
		{
			EditorUtility.DisplayDialog("�G���[", "�I�����ꂽGameObject��Skinned Mesh Renderer���A�^�b�`����Ă��܂���B", "OK");
			return;
		}

		// �ۑ���t�H���_�����[�U�[�ɑI��������
		string savePath = EditorUtility.OpenFolderPanel("�A�j���[�V�����N���b�v�̕ۑ����I��", "Assets", "");
		if (string.IsNullOrEmpty(savePath))
		{
			return;
		}

		// Assets�t�H���_����̑��΃p�X�ɕϊ�
		if (!savePath.Contains(Application.dataPath))
		{
			EditorUtility.DisplayDialog("�G���[", "�v���W�F�N�g��Assets�t�H���_���̏ꏊ��I�����Ă��������B", "OK");
			return;
		}
		string relativePath = "Assets" + savePath.Substring(Application.dataPath.Length);

		// �u�����h�V�F�C�v�̐����擾
		int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
		if (blendShapeCount == 0)
		{
			EditorUtility.DisplayDialog("���", "�I�����ꂽ���b�V���Ƀu�����h�V�F�C�v������܂���B", "OK");
			return;
		}

		string defaultblendShape = null;
		AnimationClip defaultClip = new AnimationClip();
		defaultClip.name = "default";
		for (int i = 0; i < blendShapeCount; i++)
		{
			defaultblendShape = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
			AnimationCurve curve = new AnimationCurve();

			Keyframe keyframe = new Keyframe(0f, 0f);
			curve.AddKey(keyframe);
			string bindingPath = ""; // ���[�g����p�X���擾
			Transform currentTransform = skinnedMeshRenderer.transform;
			while (currentTransform.parent != null)
			{
				bindingPath = skinnedMeshRenderer.transform.parent.name + "/" + skinnedMeshRenderer.name;
				currentTransform = currentTransform.parent;
			}
			bindingPath = bindingPath.TrimEnd('/');

			EditorCurveBinding binding = new EditorCurveBinding
			{
				path = bindingPath,
				type = typeof(SkinnedMeshRenderer),
				propertyName = "blendShape." + defaultblendShape
			};

			AnimationUtility.SetEditorCurve(defaultClip, binding, curve);
			Debug.Log("def");
		}

		// ���������N���b�v���A�Z�b�g�Ƃ��ĕۑ�
		AssetDatabase.CreateAsset(defaultClip, Path.Combine(relativePath, "def" + ".anim"));

		// �e�u�����h�V�F�C�v�ɑ΂��ăA�j���[�V�����N���b�v���쐬
		for (int i = 0; i < blendShapeCount; i++)
		{
			string blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);

			// �A�j���[�V�����N���b�v��V�K�쐬
			AnimationClip clip = new AnimationClip();
			clip.name = blendShapeName;

			// �A�j���[�V�����J�[�u���쐬
			AnimationCurve curve = new AnimationCurve();

			// 0�t���[���ɒl��100�ŃL�[��ǉ�
			Keyframe keyframe = new Keyframe(0f, 100f);
			curve.AddKey(keyframe);

			// �A�j���[�V�����J�[�u���N���b�v�ɐݒ�
			string bindingPath = ""; // ���[�g����p�X���擾
			Transform currentTransform = skinnedMeshRenderer.transform;
			while (currentTransform.parent != null)
			{
				bindingPath = skinnedMeshRenderer.transform.parent.name + "/" + skinnedMeshRenderer.name;
				currentTransform = currentTransform.parent;
			}
			bindingPath = bindingPath.TrimEnd('/');

			EditorCurveBinding binding = new EditorCurveBinding
			{
				path = bindingPath,
				type = typeof(SkinnedMeshRenderer),
				propertyName = "blendShape." + blendShapeName
			};
			AnimationUtility.SetEditorCurve(clip, binding, curve);

			// ���������N���b�v���A�Z�b�g�Ƃ��ĕۑ�
			AssetDatabase.CreateAsset(clip, Path.Combine(relativePath, blendShapeName + ".anim"));
		}

		EditorUtility.DisplayDialog("����", $"{blendShapeCount}�̃A�j���[�V�����N���b�v�𐶐����܂����B", "OK");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}
