# Examples Directory

このディレクトリには、ReleaseTrackerWpfアプリケーションでテストできるサンプルディレクトリ構造が含まれています。

## ディレクトリ構造

### v1.0.0
- **src/**: 基本的なソースコード
  - `main.cs`: メインプログラム
  - `utils.cs`: ユーティリティクラス
- **config/**: 設定ファイル
  - `app.config`: アプリケーション設定
- **docs/**: ドキュメント
  - `README.md`: アプリケーション説明
- **tests/**: テストファイル
  - `test_main.cs`: メイン機能のテスト
- **bin/**: 実行ファイル用ディレクトリ（空）

### v1.1.0
v1.0.0からの変更点：
- **src/**: 機能拡張
  - `main.cs`: コマンドライン引数サポート追加
  - `utils.cs`: エラーログ機能追加
  - `logger.cs`: 新しいLoggerクラス追加
- **config/**: 設定拡張
  - `app.config`: LogLevel設定追加
- **docs/**: ドキュメント更新
- **tests/**: テスト拡張
  - `test_main.cs`: 引数処理テスト追加
  - `test_logger.cs`: Loggerテスト追加
- **plugins/**: プラグインサポート追加
  - `plugin1.dll`: サンプルプラグイン

### v2.0.0
v1.1.0からの大幅な変更：
- **src/core/**: 新しいアーキテクチャ
  - `application.cs`: アプリケーションクラス
- **src/services/**: サービス層
  - `ilogger_service.cs`: ロガーサービスインターフェース
  - `logger_service.cs`: ロガーサービス実装
  - `iconfig_service.cs`: 設定サービスインターフェース
  - `config_service.cs`: 設定サービス実装
- **config/**: 設定大幅拡張
  - `app.config`: データベース接続、API設定追加
- **docs/**: ドキュメント完全更新
- **tests/**: テスト完全刷新
  - `test_application.cs`: アプリケーションテスト
  - `test_logger_service.cs`: ロガーサービステスト
  - `test_config_service.cs`: 設定サービステスト
- **plugins/**: プラグイン拡張
  - `plugin1.dll`: 更新されたプラグイン
  - `plugin2.dll`: 新しいプラグイン
- **assets/**: アセットファイル
  - `logo.png`: アプリケーションロゴ
  - `icon.ico`: アプリケーションアイコン

## 使用方法

1. ReleaseTrackerWpfアプリケーションを起動
2. 「Scan Directory」ボタンで各バージョンディレクトリをスキャン
3. スナップショットを保存
4. 異なるバージョン間で比較を実行

## 期待される比較結果

### v1.0.0 → v1.1.0
- **追加**: `src/logger.cs`, `tests/test_logger.cs`, `plugins/plugin1.dll`
- **変更**: `src/main.cs`, `src/utils.cs`, `config/app.config`, `docs/README.md`, `tests/test_main.cs`
- **削除**: なし

### v1.1.0 → v2.0.0
- **追加**: `src/core/`, `src/services/`, `plugins/plugin2.dll`, `assets/`
- **変更**: `src/main.cs`, `config/app.config`, `docs/README.md`, `tests/`, `plugins/plugin1.dll`
- **削除**: `src/utils.cs`, `src/logger.cs`, `tests/test_logger.cs`

このサンプル構造により、ReleaseTrackerWpfの比較機能を包括的にテストできます。
