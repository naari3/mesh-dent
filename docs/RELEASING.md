# リリース手順

## 概要

このリポジトリでは GitHub Actions を使用してリリースを自動化しています。
タグをプッシュするとドラフトリリースが自動作成され、公開するとVPMリポジトリが自動更新されます。

## リリースフロー

```
1. バージョン更新 → 2. タグ作成・プッシュ → 3. ドラフトリリース確認 → 4. 公開 → 5. VPM自動更新
```

## 手順

### 1. バージョンを更新

`package.json` と `CHANGELOG.md` のバージョンを更新します。

```json
// package.json
{
  "version": "0.2.0"  // 新しいバージョン
}
```

```markdown
<!-- CHANGELOG.md に追記 -->
## [0.2.0] - 2024-XX-XX

### Added
- 新機能の説明

### Fixed
- 修正内容
```

### 2. 変更をコミット

```bash
git add package.json CHANGELOG.md
git commit -m "Bump version to 0.2.0"
git push
```

### 3. タグを作成してプッシュ

```bash
git tag 0.2.0
git push --tags
```

### 4. ドラフトリリースを確認

GitHub Actions が自動で以下を実行します：
- パッケージの zip ファイルを作成
- ドラフトリリースを作成
- リリースノートを自動生成

[Releases ページ](https://github.com/naari3/mesh-dent/releases) でドラフトを確認してください。

### 5. リリースを公開

1. ドラフトリリースを開く
2. リリースノートを確認・編集
3. 「Publish release」をクリック

### 6. VPM リポジトリの自動更新

リリース公開後、GitHub Actions が自動で vpm.json を更新します。
数分後に以下の URL で新しいバージョンが利用可能になります：

```
https://naari3.github.io/mesh-dent/vpm.json
```

## バージョニング規則

[Semantic Versioning](https://semver.org/) に従います：

- **MAJOR** (x.0.0): 破壊的変更
- **MINOR** (0.x.0): 後方互換性のある新機能
- **PATCH** (0.0.x): 後方互換性のあるバグ修正

## トラブルシューティング

### ワークフローが失敗した場合

1. [Actions タブ](https://github.com/naari3/mesh-dent/actions) でログを確認
2. 問題を修正して再度タグをプッシュ（既存タグを削除する場合は `git tag -d 0.2.0 && git push --delete origin 0.2.0`）

### VPM に反映されない場合

1. リリースが「Published」状態か確認
2. zip ファイルがリリースに添付されているか確認
3. [Build VPM Listing ワークフロー](https://github.com/naari3/mesh-dent/actions/workflows/pages.yml) を手動実行
