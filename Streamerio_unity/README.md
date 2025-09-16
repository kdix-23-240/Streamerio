# 命名規則
## BGM, SE名
Enumのパラメータとして使える名前
## シーン名
Enumのパラメータとして使える名前

# フォルダ階層
## 0_Scripts
C#のスクリプトを格納  
### 0_Common
ゲーム全体に影響するスクリプト(インゲーム、アウトゲームどっちでも使う)  
### 1_InGame
実際のゲーム(インゲーム)で使うスクリプトを格納  
### 2_OutGame
タイトルとかのUI(アウトゲーム)で使うスクリプトを格納  

## 1_Prefabs
プレファブを格納  
階層構造はできる限りスクリプトに合わせて欲しい  

## 2_ScriptableObject
スクリプタブルオブジェクトを格納  
階層構造はできる限りスクリプトに合わせて欲しい  

## 3_2D
画像を格納  

## 4_Musics
曲を格納  
### BGM
BGMを格納  
### SE
SEを格納  

## 5_Scenes
シーンを入れる  

## 6_Materials
マテリアルとかシェーダを格納

## 7_Animations
アニメーションとかアニメーションコントローラーを格納

## Editor
エディタ拡張のスクリプトに格納  
ただし、インスペクター拡張の場合は、0_Scriptに格納  

# ツール
UnityのツールバーのToolsから実行可能
## Play Game
ゲームをタイトルから再生 -> ビルドセッティングの先頭を再生
## Generate Scen Enum
シーン名をEnumに変換

# 基底クラス
## SingletonBase<T>
シングルトンパターンのオブジェクトを作る時に継承する  
Tには、シングルトンにしたいクラス名

# Debug.Log
UnityEngineではないほうのDebug.Logを使って欲しい  
あとで、ビルドする時に一括で消す

# SerializeDictionary<TKey, TValue>
辞書をシリアライズ化させたいなら使う  
辞書と同じ使い方ができる  
ToDictioneryで辞書に変換も可能

# Layer
レイヤーがEnum的に選択できる

# AudioManager
シングルトンなので、AudioManager.Instanceでアクセス可能
- BGM, SE再生
  - PlayAsync(BGMType bgmType, CancellationToken ct)
  - PlayAsync(SEType seType, CancellationToken ct)

# SceneManager
シングルトンなので、SceneManager.Instanceでアクセス可能
- シーン切り替え
  - LoadSceneAsync()
- リロード
  - ReloadSceneAsync
