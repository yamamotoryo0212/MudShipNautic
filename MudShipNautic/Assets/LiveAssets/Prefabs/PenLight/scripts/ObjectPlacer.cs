using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �w�肳�ꂽ�`��Ɣ͈͓��ɁAY���W���Œ肵�ăI�u�W�F�N�g�������_���ɔz�u���܂��B
/// �z�u���ɏ��O�G���A�`�F�b�N�ƃI�u�W�F�N�g�Ԃ̔�d���`�F�b�N���s���܂��B
/// </summary>
public class ObjectPlacer : MonoBehaviour
{
	// �z�u�G���A�̌`����`����񋓌^
	public enum PlacementShape { Square, Circle, Donut }

	[Tooltip("�z�u����I�u�W�F�N�g�̃v���n�u�z��B�������烉���_���ɑI�΂�܂��B")]
	public GameObject[] prefabsToPlace;

	[SerializeField,Range(0.1f,1)]
	private float _maxAnimationSpeed = 1.0f;

	[Header("1. �z�u�`��ƃT�C�Y�ݒ�")]
	[Tooltip("�����_���z�u���s���G���A�̌`���I�����܂��B")]
	public PlacementShape shape = PlacementShape.Square;

	[Header("Square/Rectangle Settings (���S����̔���/����)")]
	[Tooltip("X�������̔z�u�͈͂̔����̒����B")]
	public float halfWidthX = 20f;
	[Tooltip("Z�������̔z�u�͈͂̔����̒����B")]
	public float halfLengthZ = 20f;

	[Header("Circle Settings")]
	[Tooltip("�~�`�̍ő唼�a�B")]
	public float radius = 20f;

	[Header("Donut/Annulus Settings")]
	[Tooltip("�h�[�i�c�̓����̔��a (���͈͓̔��ɂ͔z�u����Ȃ�)�B")]
	public float innerRadius = 5f;
	[Tooltip("�h�[�i�c�̊O���̍ő唼�a�B")]
	public float outerRadius = 20f;

	[Header("2. �I�u�W�F�N�g�̑���")]
	public int numberOfObjects = 50;

	[Header("3. �I�u�W�F�N�g�̌����ݒ�")]
	[Tooltip("�z�u���ꂽ�I�u�W�F�N�g����Ɍ����^�[�Q�b�g��Transform�B���ݒ�̏ꍇ�A�����_����Y����]�ɂȂ�܂��B")]
	public Transform targetToLookAt;

	[Header("4. ��d��/���O�G���A�ݒ�")]
	[Tooltip("�������ꂽ�I�u�W�F�N�g�̒��S�Ԃ̍ŏ����� (1.0f�̏ꍇ�A1x1�͈̔͂ŏd�Ȃ�Ȃ��悤�ɔz�u)�B")]
	public float minSeparationDistance = 1.0f;

	[Tooltip("���[���h���W�n�̌��_(0,0,0)�𒆐S�Ƃ������͈͓̔�(X, Z�̔���)�ɂ͔z�u���Ȃ��B�����l1.0f��X,Z���Ɂ}1.0m�����O�G���A�B")]
	public Vector2 exclusionHalfExtent = new Vector2(1.0f, 1.0f);

	// �z�u�����������ʒu���L�^���邽�߂̃��X�g
	private List<Vector3> placedPositions = new List<Vector3>();

	// �����ʒu�������邽�߂̍ő厎�s�� (�������[�v�h�~)
	private const int MAX_PLACEMENT_ATTEMPTS_PER_OBJECT = 50;

	void Start()
	{
		if (!ValidateInputs())
		{
			return;
		}

		// �I�u�W�F�N�g�̃����_���z�u�����s
		PlaceObjectsRandomly();
	}

	/// <summary>
	/// ���͒l�̃`�F�b�N
	/// </summary>
	bool ValidateInputs()
	{
		if (prefabsToPlace == null || prefabsToPlace.Length == 0)
		{
			Debug.LogError("�y�G���[�z�z�u����v���n�u���ݒ肳��Ă��܂���B`prefabsToPlace`�z��ɃI�u�W�F�N�g���h���b�O���h���b�v���Ă��������B");
			return false;
		}
		if (shape == PlacementShape.Donut && innerRadius >= outerRadius)
		{
			Debug.LogError("�y�G���[�z�h�[�i�c�`��ł́A�������a (innerRadius) �͊O�����a (outerRadius) ��菬�����ݒ肵�Ă��������B");
			return false;
		}
		return true;
	}

	/// <summary>
	/// �w�肳�ꂽ�͈͓��ɃI�u�W�F�N�g�������_���ɐ������܂��B
	/// </summary>
	void PlaceObjectsRandomly()
	{
		Vector3 center = transform.position;
		placedPositions.Clear(); // �����̈ʒu���X�g���N���A

		int successfulPlacements = 0;
		// ���������z�u�����ڕW�ɒB����܂Ń��[�v���A�����ɖ������[�v������邽�߂ɍő厎�s�񐔂�ݒ�
		for (int i = 0; successfulPlacements < numberOfObjects; i++)
		{
			if (i >= numberOfObjects * MAX_PLACEMENT_ATTEMPTS_PER_OBJECT)
			{
				Debug.LogWarning($"�y�x���z�z�u���g���C�񐔂����({numberOfObjects * MAX_PLACEMENT_ATTEMPTS_PER_OBJECT}��)�ɒB���܂����B�v�����ꂽ {numberOfObjects} �̂��� {successfulPlacements} �̂ݔz�u����܂����B");
				break;
			}

			// 1. �`��Ɋ�Â��������_���Ȕz�u�ʒu�����擾 (Y���W��0�Œ�)
			Vector3 candidatePosition = GetCandidatePosition(center);

			// 2. �ʒu���L�����`�F�b�N (���O�G���A����d��)
			if (IsValidPosition(candidatePosition))
			{
				// ����������ʒu��ۑ�
				placedPositions.Add(candidatePosition);
				successfulPlacements++;

				// 3. �������v�Z
				Quaternion rotation = GetObjectRotation(candidatePosition);

				// 4. �z�u����v���n�u��z�񂩂烉���_���ɑI��
				GameObject prefabToSpawn = prefabsToPlace[Random.Range(0, prefabsToPlace.Length)];

				// 5. �I�u�W�F�N�g���C���X�^���X��
				GameObject newObject = Instantiate(
					prefabToSpawn,
					candidatePosition,
					rotation
				);

				newObject.GetComponent<Animator>().SetFloat("Speed", Random.Range(0.1f, _maxAnimationSpeed));


				// 6. �R���e�i�̎q�Ƃ��Đݒ�
				newObject.transform.SetParent(gameObject.transform);
				newObject.name = prefabToSpawn.name + "_" + successfulPlacements;
			}
		}

		Debug.Log($"�y�z�u�����z���v {successfulPlacements} �̃I�u�W�F�N�g�� {shape} �`��Ŕz�u���܂����B");
	}

	/// <summary>
	/// �I�����ꂽ�`��Ɋ�Â��ă����_���ȃ��[���h���W�����v�Z���܂� (Y=0�Œ�)�B
	/// </summary>
	Vector3 GetCandidatePosition(Vector3 center)
	{
		float randomX = 0f;
		float randomZ = 0f;

		switch (shape)
		{
			case PlacementShape.Square:
				// �l�p (��`) 
				randomX = Random.Range(-halfWidthX, halfWidthX);
				randomZ = Random.Range(-halfLengthZ, halfLengthZ);
				break;

			case PlacementShape.Circle:
				// �~ (�ψ�Ȕz�u�̂��߂ɔ��a�̓��������_����)
				float randomAngleC = Random.Range(0f, 360f) * Mathf.Deg2Rad;
				float randomRadiusC = Mathf.Sqrt(Random.Range(0f, radius * radius));

				randomX = randomRadiusC * Mathf.Cos(randomAngleC);
				randomZ = randomRadiusC * Mathf.Sin(randomAngleC);
				break;

			case PlacementShape.Donut:
				// �h�[�i�c (�� - �ψ�Ȕz�u�̂��߂ɔ��a�̓��������_����)
				float randomAngleD = Random.Range(0f, 360f) * Mathf.Deg2Rad;
				float minR2 = innerRadius * innerRadius;
				float maxR2 = outerRadius * outerRadius;
				float randomRadiusD = Mathf.Sqrt(Random.Range(minR2, maxR2));

				randomX = randomRadiusD * Mathf.Cos(randomAngleD);
				randomZ = randomRadiusD * Mathf.Sin(randomAngleD);
				break;
		}

		// Y���W�͏��0�ɌŒ肵�A���S���W���I�t�Z�b�g�Ƃ��ĉ��Z
		return new Vector3(center.x + randomX, gameObject.transform.position.y, center.z + randomZ);
	}

	/// <summary>
	/// ���ʒu���L�����ǂ������`�F�b�N���܂��i���O�G���A����d���j�B
	/// </summary>
	bool IsValidPosition(Vector3 candidatePosition)
	{
		// 1. ���O�G���A�`�F�b�N (���[���h���_(0,0,0)�𒆐S�Ƃ����͈�)
		// ���[�U�[�w��́uxz���W1,1�͈̔́v���A���_���S��X, Z���ꂼ��}1.0m�͈̔͂Ɖ��߂��܂�
		if (Mathf.Abs(candidatePosition.x) <= exclusionHalfExtent.x &&
			Mathf.Abs(candidatePosition.z) <= exclusionHalfExtent.y)
		{
			// �ʒu�����O�G���A���ɂ���
			return false;
		}

		// 2. ��d���`�F�b�N
		float sqrMinSeparation = minSeparationDistance * minSeparationDistance;

		foreach (Vector3 placedPos in placedPositions)
		{
			// X-Z���ʂ̋����݂̂��`�F�b�N (Y�����͖���)
			Vector3 delta = candidatePosition - placedPos;
			delta.y = 0;

			if (delta.sqrMagnitude < sqrMinSeparation)
			{
				// �����̃I�u�W�F�N�g�Ƌ߂����� (�d������)
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// �^�[�Q�b�g������ꍇ�A�^�[�Q�b�g�̕�����������]���v�Z���܂��B
	/// </summary>
	Quaternion GetObjectRotation(Vector3 position)
	{
		if (targetToLookAt != null)
		{
			Vector3 directionToTarget = targetToLookAt.position - position;

			// Y���̉�]�݂̂��l�����A�n�ʂɕ��s��ۂ�
			directionToTarget.y = 0;

			if (directionToTarget != Vector3.zero)
			{
				return Quaternion.LookRotation(directionToTarget);
			}
		}

		// �^�[�Q�b�g���Ȃ��ꍇ��������[���̏ꍇ�A�����_����Y����]��Ԃ�
		return Quaternion.Euler(0, Random.Range(0f, 360f), 0);
	}

	// �G�f�B�^��Ŕz�u�͈͂Ə��O�G���A�����o�����邽�߂̃M�Y���`��
	private void OnDrawGizmosSelected()
	{
		// �`��̒��S���W���擾 (Y���W��0�ɌŒ�)
		Vector3 center = new Vector3(transform.position.x, 0, transform.position.z);

		Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // �z�u�G���A�̐F

		// 1. �z�u�G���A�̃M�Y���`��
		switch (shape)
		{
			case PlacementShape.Square:
				Vector3 size = new Vector3(halfWidthX * 2, 0.1f, halfLengthZ * 2);
				Gizmos.DrawWireCube(center, size);
				Gizmos.DrawCube(center, size);
				break;

			case PlacementShape.Circle:
				Gizmos.DrawWireSphere(center, radius);
				break;

			case PlacementShape.Donut:
				Gizmos.DrawWireSphere(center, outerRadius);
				Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // ����������
				Gizmos.DrawWireSphere(center, innerRadius);
				break;
		}

		// 2. ���O�G���A�̃M�Y���`�� (���[���h���_(0,0,0)�𒆐S)
		Gizmos.color = new Color(1f, 0f, 0f, 0.8f); // �ԐF
		Vector3 exclusionSize = new Vector3(exclusionHalfExtent.x * 2, 0.1f, exclusionHalfExtent.y * 2);
		Vector3 exclusionPosition = new Vector3(0, 0, 0);

		// ���O�G���A�����C���[�t���[���ƃ{�b�N�X�ŕ`��
		Gizmos.DrawWireCube(exclusionPosition, exclusionSize);
		Gizmos.DrawCube(exclusionPosition, exclusionSize);

		// 3. �^�[�Q�b�g�����������i�^�[�Q�b�g���ݒ肳��Ă���ꍇ�j
		if (targetToLookAt != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(center, targetToLookAt.position);
			Gizmos.DrawSphere(targetToLookAt.position, 0.5f);
		}
	}
}
