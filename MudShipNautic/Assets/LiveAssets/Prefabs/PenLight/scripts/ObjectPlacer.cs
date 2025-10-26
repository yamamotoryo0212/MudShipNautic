using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 指定された形状と範囲内に、Y座標を固定してオブジェクトをランダムに配置します。
/// 配置時に除外エリアチェックとオブジェクト間の非重複チェックを行います。
/// </summary>
public class ObjectPlacer : MonoBehaviour
{
	// 配置エリアの形状を定義する列挙型
	public enum PlacementShape { Square, Circle, Donut }

	[Tooltip("配置するオブジェクトのプレハブ配列。ここからランダムに選ばれます。")]
	public GameObject[] prefabsToPlace;

	[SerializeField,Range(0.1f,1)]
	private float _maxAnimationSpeed = 1.0f;

	[Header("1. 配置形状とサイズ設定")]
	[Tooltip("ランダム配置を行うエリアの形状を選択します。")]
	public PlacementShape shape = PlacementShape.Square;

	[Header("Square/Rectangle Settings (中心からの半幅/半長)")]
	[Tooltip("X軸方向の配置範囲の半分の長さ。")]
	public float halfWidthX = 20f;
	[Tooltip("Z軸方向の配置範囲の半分の長さ。")]
	public float halfLengthZ = 20f;

	[Header("Circle Settings")]
	[Tooltip("円形の最大半径。")]
	public float radius = 20f;

	[Header("Donut/Annulus Settings")]
	[Tooltip("ドーナツの内側の半径 (この範囲内には配置されない)。")]
	public float innerRadius = 5f;
	[Tooltip("ドーナツの外側の最大半径。")]
	public float outerRadius = 20f;

	[Header("2. オブジェクトの総数")]
	public int numberOfObjects = 50;

	[Header("3. オブジェクトの向き設定")]
	[Tooltip("配置されたオブジェクトが常に向くターゲットのTransform。未設定の場合、ランダムなY軸回転になります。")]
	public Transform targetToLookAt;

	[Header("4. 非重複/除外エリア設定")]
	[Tooltip("生成されたオブジェクトの中心間の最小距離 (1.0fの場合、1x1の範囲で重ならないように配置)。")]
	public float minSeparationDistance = 1.0f;

	[Tooltip("ワールド座標系の原点(0,0,0)を中心としたこの範囲内(X, Zの半幅)には配置しない。初期値1.0fでX,Z共に±1.0mが除外エリア。")]
	public Vector2 exclusionHalfExtent = new Vector2(1.0f, 1.0f);

	// 配置が成功した位置を記録するためのリスト
	private List<Vector3> placedPositions = new List<Vector3>();

	// 生成位置を見つけるための最大試行回数 (無限ループ防止)
	private const int MAX_PLACEMENT_ATTEMPTS_PER_OBJECT = 50;

	void Start()
	{
		if (!ValidateInputs())
		{
			return;
		}

		// オブジェクトのランダム配置を実行
		PlaceObjectsRandomly();
	}

	/// <summary>
	/// 入力値のチェック
	/// </summary>
	bool ValidateInputs()
	{
		if (prefabsToPlace == null || prefabsToPlace.Length == 0)
		{
			Debug.LogError("【エラー】配置するプレハブが設定されていません。`prefabsToPlace`配列にオブジェクトをドラッグ＆ドロップしてください。");
			return false;
		}
		if (shape == PlacementShape.Donut && innerRadius >= outerRadius)
		{
			Debug.LogError("【エラー】ドーナツ形状では、内側半径 (innerRadius) は外側半径 (outerRadius) より小さく設定してください。");
			return false;
		}
		return true;
	}

	/// <summary>
	/// 指定された範囲内にオブジェクトをランダムに生成します。
	/// </summary>
	void PlaceObjectsRandomly()
	{
		Vector3 center = transform.position;
		placedPositions.Clear(); // 既存の位置リストをクリア

		int successfulPlacements = 0;
		// 成功した配置数が目標に達するまでループし、同時に無限ループを避けるために最大試行回数を設定
		for (int i = 0; successfulPlacements < numberOfObjects; i++)
		{
			if (i >= numberOfObjects * MAX_PLACEMENT_ATTEMPTS_PER_OBJECT)
			{
				Debug.LogWarning($"【警告】配置リトライ回数が上限({numberOfObjects * MAX_PLACEMENT_ATTEMPTS_PER_OBJECT}回)に達しました。要求された {numberOfObjects} 個のうち {successfulPlacements} 個のみ配置されました。");
				break;
			}

			// 1. 形状に基づいたランダムな配置位置候補を取得 (Y座標は0固定)
			Vector3 candidatePosition = GetCandidatePosition(center);

			// 2. 位置が有効かチェック (除外エリア＆非重複)
			if (IsValidPosition(candidatePosition))
			{
				// 成功したら位置を保存
				placedPositions.Add(candidatePosition);
				successfulPlacements++;

				// 3. 向きを計算
				Quaternion rotation = GetObjectRotation(candidatePosition);

				// 4. 配置するプレハブを配列からランダムに選択
				GameObject prefabToSpawn = prefabsToPlace[Random.Range(0, prefabsToPlace.Length)];

				// 5. オブジェクトをインスタンス化
				GameObject newObject = Instantiate(
					prefabToSpawn,
					candidatePosition,
					rotation
				);

				newObject.GetComponent<Animator>().SetFloat("Speed", Random.Range(0.1f, _maxAnimationSpeed));


				// 6. コンテナの子として設定
				newObject.transform.SetParent(gameObject.transform);
				newObject.name = prefabToSpawn.name + "_" + successfulPlacements;
			}
		}

		Debug.Log($"【配置完了】合計 {successfulPlacements} 個のオブジェクトを {shape} 形状で配置しました。");
	}

	/// <summary>
	/// 選択された形状に基づいてランダムなワールド座標候補を計算します (Y=0固定)。
	/// </summary>
	Vector3 GetCandidatePosition(Vector3 center)
	{
		float randomX = 0f;
		float randomZ = 0f;

		switch (shape)
		{
			case PlacementShape.Square:
				// 四角 (矩形) 
				randomX = Random.Range(-halfWidthX, halfWidthX);
				randomZ = Random.Range(-halfLengthZ, halfLengthZ);
				break;

			case PlacementShape.Circle:
				// 円 (均一な配置のために半径の二乗をランダム化)
				float randomAngleC = Random.Range(0f, 360f) * Mathf.Deg2Rad;
				float randomRadiusC = Mathf.Sqrt(Random.Range(0f, radius * radius));

				randomX = randomRadiusC * Mathf.Cos(randomAngleC);
				randomZ = randomRadiusC * Mathf.Sin(randomAngleC);
				break;

			case PlacementShape.Donut:
				// ドーナツ (環状 - 均一な配置のために半径の二乗をランダム化)
				float randomAngleD = Random.Range(0f, 360f) * Mathf.Deg2Rad;
				float minR2 = innerRadius * innerRadius;
				float maxR2 = outerRadius * outerRadius;
				float randomRadiusD = Mathf.Sqrt(Random.Range(minR2, maxR2));

				randomX = randomRadiusD * Mathf.Cos(randomAngleD);
				randomZ = randomRadiusD * Mathf.Sin(randomAngleD);
				break;
		}

		// Y座標は常に0に固定し、中心座標をオフセットとして加算
		return new Vector3(center.x + randomX, gameObject.transform.position.y, center.z + randomZ);
	}

	/// <summary>
	/// 候補位置が有効かどうかをチェックします（除外エリア＆非重複）。
	/// </summary>
	bool IsValidPosition(Vector3 candidatePosition)
	{
		// 1. 除外エリアチェック (ワールド原点(0,0,0)を中心とした範囲)
		// ユーザー指定の「xz座標1,1の範囲」を、原点中心のX, Zそれぞれ±1.0mの範囲と解釈します
		if (Mathf.Abs(candidatePosition.x) <= exclusionHalfExtent.x &&
			Mathf.Abs(candidatePosition.z) <= exclusionHalfExtent.y)
		{
			// 位置が除外エリア内にある
			return false;
		}

		// 2. 非重複チェック
		float sqrMinSeparation = minSeparationDistance * minSeparationDistance;

		foreach (Vector3 placedPos in placedPositions)
		{
			// X-Z平面の距離のみをチェック (Y成分は無視)
			Vector3 delta = candidatePosition - placedPos;
			delta.y = 0;

			if (delta.sqrMagnitude < sqrMinSeparation)
			{
				// 既存のオブジェクトと近すぎる (重複する)
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// ターゲットがある場合、ターゲットの方向を向く回転を計算します。
	/// </summary>
	Quaternion GetObjectRotation(Vector3 position)
	{
		if (targetToLookAt != null)
		{
			Vector3 directionToTarget = targetToLookAt.position - position;

			// Y軸の回転のみを考慮し、地面に平行を保つ
			directionToTarget.y = 0;

			if (directionToTarget != Vector3.zero)
			{
				return Quaternion.LookRotation(directionToTarget);
			}
		}

		// ターゲットがない場合や方向がゼロの場合、ランダムなY軸回転を返す
		return Quaternion.Euler(0, Random.Range(0f, 360f), 0);
	}

	// エディタ上で配置範囲と除外エリアを視覚化するためのギズモ描画
	private void OnDrawGizmosSelected()
	{
		// 描画の中心座標を取得 (Y座標は0に固定)
		Vector3 center = new Vector3(transform.position.x, 0, transform.position.z);

		Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // 配置エリアの色

		// 1. 配置エリアのギズモ描画
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
				Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 内側を強調
				Gizmos.DrawWireSphere(center, innerRadius);
				break;
		}

		// 2. 除外エリアのギズモ描画 (ワールド原点(0,0,0)を中心)
		Gizmos.color = new Color(1f, 0f, 0f, 0.8f); // 赤色
		Vector3 exclusionSize = new Vector3(exclusionHalfExtent.x * 2, 0.1f, exclusionHalfExtent.y * 2);
		Vector3 exclusionPosition = new Vector3(0, 0, 0);

		// 除外エリアをワイヤーフレームとボックスで描画
		Gizmos.DrawWireCube(exclusionPosition, exclusionSize);
		Gizmos.DrawCube(exclusionPosition, exclusionSize);

		// 3. ターゲット方向を可視化（ターゲットが設定されている場合）
		if (targetToLookAt != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(center, targetToLookAt.position);
			Gizmos.DrawSphere(targetToLookAt.position, 0.5f);
		}
	}
}
