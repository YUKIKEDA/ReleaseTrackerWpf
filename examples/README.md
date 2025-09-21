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

### v2.0.0 → v3.0.0
- **追加**: `src/application/controllers/`, `src/infrastructure/database/migrations/`, `src/infrastructure/external/apis/`, `src/presentation/web/views/`, `tests/unit/`, `tests/integration/`, `config/environments/`, `scripts/deployment/`
- **変更**: アーキテクチャ全体の再構築、レイヤードアーキテクチャの導入
- **削除**: `src/core/`, `src/services/`（新しい構造に統合）

### v3.0.0 → v4.0.0
- **追加**: `services/`（マイクロサービス群）, `gateways/api-gateway/`, `infrastructure/monitoring/`
- **変更**: モノリシックアーキテクチャからマイクロサービスアーキテクチャへの移行
- **削除**: 単一のアプリケーション構造（分散アーキテクチャに変更）

### v3.0.0
v2.0.0からの大幅なアーキテクチャ変更：
- **src/application/**: アプリケーション層
  - `controllers/api/v1/`: REST APIコントローラー
    - `user_controller.cs`: ユーザー管理API
    - `product_controller.cs`: 商品管理API
  - `services/domain/user/`: ドメインサービス
    - `user_service.cs`: ユーザーサービス
    - `user_repository.cs`: ユーザーリポジトリ
- **src/infrastructure/**: インフラストラクチャ層
  - `database/migrations/2024/01/`: データベースマイグレーション
    - `001_create_users_table.sql`: ユーザーテーブル作成
    - `002_create_products_table.sql`: 商品テーブル作成
  - `external/apis/third_party/`: 外部API連携
    - `payment_gateway_client.cs`: 決済ゲートウェイクライアント
    - `email_service_client.cs`: メールサービスクライアント
  - `presentation/web/views/components/widgets/`: UIコンポーネント
    - `user_card.cshtml`: ユーザーカードコンポーネント
    - `product_grid.cshtml`: 商品グリッドコンポーネント
- **tests/**: テスト層
  - `unit/application/services/domain/user/`: ユニットテスト
    - `user_service_test.cs`: ユーザーサービステスト
  - `integration/infrastructure/database/`: インテグレーションテスト
    - `user_repository_integration_test.cs`: ユーザーリポジトリテスト
- **config/environments/**: 環境別設定
  - `development/appsettings.json`: 開発環境設定
  - `staging/appsettings.json`: ステージング環境設定
  - `production/appsettings.json`: 本番環境設定
- **scripts/deployment/ci/cd/pipelines/**: CI/CDパイプライン
  - `build_and_deploy.yml`: ビルド・デプロイパイプライン

### v4.0.0
v3.0.0からのマイクロサービスアーキテクチャへの移行：
- **services/**: マイクロサービス群
  - `user-service/`: ユーザー管理サービス
    - `src/application/use-cases/user-management/commands/handlers/`: コマンドハンドラー
      - `create_user_command_handler.cs`: ユーザー作成コマンド
    - `src/domain/entities/user/aggregates/`: ドメインエンティティ
      - `user_aggregate.cs`: ユーザーアグリゲート
    - `src/infrastructure/messaging/rabbitmq/exchanges/user/`: メッセージング
      - `user_event_publisher.cs`: ユーザーイベントパブリッシャー
    - `src/infrastructure/caching/redis/strategies/user/`: キャッシュ戦略
      - `user_cache_strategy.cs`: ユーザーキャッシュ戦略
  - `product-service/`: 商品管理サービス
  - `order-service/`: 注文管理サービス
  - `payment-service/`: 決済処理サービス
  - `notification-service/`: 通知サービス
- **gateways/**: APIゲートウェイ
  - `api-gateway/src/middleware/authentication/jwt/validators/`: JWT認証
    - `jwt_token_validator.cs`: JWTトークンバリデーター
  - `api-gateway/src/middleware/rate-limiting/strategies/user-based/`: レート制限
  - `api-gateway/src/middleware/load-balancing/algorithms/round-robin/`: ロードバランシング
- **infrastructure/monitoring/**: 監視・観測性
  - `logging/structured/elasticsearch/indices/`: 構造化ログ
  - `metrics/prometheus/collectors/custom/`: カスタムメトリクス
  - `tracing/jaeger/span-processors/user-activity/`: 分散トレーシング

このサンプル構造により、ReleaseTrackerWpfの比較機能を包括的にテストできます。
