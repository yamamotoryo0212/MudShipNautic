using UnityEngine;

public class RenderTextureCombiner : MonoBehaviour
{
	// インスペクターで設定するRenderTextureA (RGBソース)
	public RenderTexture textureA;
	// インスペクターで設定するRenderTextureB (Alphaソース)
	public RenderTexture textureB;
	// 結果を書き込むRenderTextureC (ターゲット)
	public RenderTexture resultTexture;

	// チャンネル合成用のマテリアル (カスタムシェーダーを使用)
	public Material channelMergeMaterial;

	void Awake()
	{
		// 実行テスト
		if (textureA != null && textureB != null && resultTexture != null && channelMergeMaterial != null)
		{
			MergeTextures();
		}
		else
		{
			Debug.LogError("すべてのRenderTextureとマテリアルをインスペクターに設定してください。");
		}
	}
	private void Update()
	{
		Graphics.Blit(null, resultTexture, channelMergeMaterial);
	}

	public void MergeTextures()
	{
		if (channelMergeMaterial == null) return;

		// ターゲットテクスチャのサイズがソースと一致するか確認 (通常は一致させる)
		if (resultTexture.width != textureA.width || resultTexture.height != textureA.height)
		{
			Debug.LogError("ターゲットのRenderTextureCは、ソースと同じサイズである必要があります。");
			return;
		}

		// マテリアルにテクスチャを設定
		channelMergeMaterial.SetTexture("_TexA", textureA);
		channelMergeMaterial.SetTexture("_TexB", textureB);

		// Graphics.Blitを使って合成処理を実行
		// source: null (画面全体を描画するわけではないので通常はnullやダミーで良いが、
		// ポストプロセス用途でなければマテリアルを使って直接ターゲットに描画)
		// dest: resultTexture (書き込み先)
		// material: channelMergeMaterial (合成ロジックを持つシェーダー)
		// pass: -1 (最初のパスを使用)

		// RenderTextureCをアクティブなレンダーターゲットに設定
		RenderTexture.active = resultTexture;

		// Blitを使用して、シェーダーの結果をRenderTextureCに焼き込む
		

		// アクティブなレンダーターゲットを元に戻す
		RenderTexture.active = null;

		Debug.Log("RenderTextureのチャンネル合成が完了し、結果が resultTexture に書き込まれました。");
	}

	private void OnDestroy()
	{
	
		// 使用後にRenderTextureを解放してメモリリークを防ぐ
		if (textureA != null) textureA.Release();
		if (textureB != null) textureB.Release();
		if (resultTexture != null) resultTexture.Release();
	}
}