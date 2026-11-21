# CodePromptus
**言語:** [🇺🇸 English](README.md) | [🇯🇵 日本語](README.ja.md)

**👉 [バイナリダウンロードはこちら！](https://github.com/prota-p/CodePromptus/releases/)**

プロジェクトフォルダからファイルを選択し、AIプロンプトを自動生成するデスクトップアプリケーションです。

## 概要
開発者がAIにコーディングについて効率的に相談できるよう、プロジェクトの構造とコード内容を含むプロンプトを簡単に作成できるツールです。

## 背景
AIにコーディングの質問をする際の手動コピー&ペースト作業を自動化し、開発者とAIの対話を効率化するために開発しました。

## 主な機能

- プロジェクトフォルダの階層構造を表示
- ファイルの選択とフィルタリング
- .gitignoreテンプレートによる不要ファイルの自動除外
- 選択したファイルからAIプロンプトを自動生成
- 生成されたプロンプトのワンクリックコピー

## スクリーンショット

![プロジェクトフォルダを開いてAIプロンプトを生成する画面例](Images/screenshot1.png)

## 動作環境

- Windows 11 (動作確認済み)
- MacOS、Linux向けバイナリも公開していますが、動作確認はしていません。
- .NET 10.0
  - アプリのバイナリは自己完結形式のため、.NET 10.0を別途インストールする必要はありません。

## 使い方

1. **「Open Folder」** でプロジェクトフォルダを選択
2. 適切な **.gitignoreテンプレート** を選択（自動でプリセットテンプレートが設定されます）
3. 左のツリーで **ファイルを選択**
4. 右側に生成された **プロンプトをコピー**
5. お好みのAIサービス（例：ChatGPT）に貼り付けて使用

## 開発
### ビルドとデバッグ実行

```bash
git clone https://github.com/prota-p/CodePromptus.git
cd CodePromptus
dotnet build
dotnet run --project App/CodePromptus.App.csproj
```

### テスト実行

`Tests`フォルダ配下に、サービステスト・E2Eテストなどの自動テストを用意しています。
**※現時点ではもっとも基本的なシナリオのみです**

テストの実行方法：

```bash
dotnet test
```

### バイナリ作成

```bash
dotnet publish App/CodePromptus.App.csproj -c Release -r [RID] --self-contained true -o publish-[RID]
```
[RID] は win-x64、osx-x64、linux-x64 のいずれかに置き換えてください（App プロジェクトの `RuntimeIdentifiers` に書いてあるもの）。

### サードパーティライセンス
依存ライブラリのライセンスの検査と、バイナリに同梱する告知ファイルの生成には [NoticeForge](https://github.com/prota-p/NoticeForge) を使っています。設定は `ThirdPartyLicenses/noticeforge.json` にあります。
```bash
dotnet tool restore

# 依存ライブラリをすべて使ってよいか（packages.lock.json を読む）
dotnet noticeforge check -c ThirdPartyLicenses/noticeforge.json

# publish したものの告知ファイルを作る（dotnet publish のあと、RID ごとに実行）
dotnet noticeforge notices App/obj/Release/net10.0/[RID]/CodePromptus.App.deps.json -c ThirdPartyLicenses/noticeforge.json -o publish-[RID]/THIRD-PARTY-NOTICES.txt
```

### 依存ライブラリの追加・更新
依存ライブラリのバージョンは `packages.lock.json` で固定していて、一致しないと restore が失敗します。パッケージ参照を変えたあとは、lock ファイルを更新して差分を確認してください。
```bash
dotnet restore -p:RestoreLockedMode=false
```

## 技術スタック

- Avalonia UI (.NET クロスプラットフォーム UI)
- ReactiveUI (MVVM)
- Fluid (テンプレートエンジン)

## ライセンス

MIT License

## コントリビューション

Issues や Pull Request をお気軽にお送りください。

このリポジトリに貢献（コード、ドキュメント、その他のコンテンツの提供）を行うことにより、あなたの貢献がMITライセンスの下でライセンスされることに同意したものとみなされます。