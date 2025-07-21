using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class SimpleMultiPathMazeGenerator : MonoBehaviour
{
    [Header("迷路設定")]
    public int mazeWidth = 21;      // 奇数にすることをお勧めします
    public int mazeHeight = 21;     // 奇数にすることをお勧めします
    public GameObject wallPrefab;    // 壁のプレハブ
    public GameObject floorPrefab;   // 床のプレハブ
    public GameObject startMarker;   // スタートマーカー
    public GameObject goalMarker;    // ゴールマーカー
    
    [Header("経路設定")]
    [Range(1, 5)]
    public int numberOfPaths = 3;    // 生成する経路の数
    [Range(0.1f, 0.5f)]
    public float branchProbability = 0.25f; // 分岐経路を作る確率
    
    [Header("NavMesh設定")]
    public bool buildNavMesh = true; // NavMeshを自動的に構築するか
    [Range(0.05f, 0.3f)]
    public float voxelSize = 0.1f;   // ボクセルサイズ（細かいほど精度が上がる）
    
    // 内部変数
    private bool[,] maze;            // true=壁、false=通路
    private Vector2Int startPos;     // スタート位置
    private Vector2Int goalPos;      // ゴール位置
    private List<Vector2Int> mainPath = new List<Vector2Int>(); // メインの経路
    private NavMeshSurface navMeshSurface;
    
    void Start()
    {
        // NavMeshSurfaceコンポーネントを取得または追加
        if (buildNavMesh)
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }
        }
        
        GenerateMaze();
        
        // NavMeshをビルド
        if (buildNavMesh)
        {
            StartCoroutine(BuildNavMeshDelayed());
        }
    }
    
    void GenerateMaze()
    {
        // 奇数サイズを確保
        if (mazeWidth % 2 == 0) mazeWidth++;
        if (mazeHeight % 2 == 0) mazeHeight++;
        
        // 迷路の初期化（すべて壁）
        maze = new bool[mazeWidth, mazeHeight];
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = true; // true = 壁
            }
        }
        
        // スタートとゴールの位置を設定
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(mazeWidth - 2, mazeHeight - 2);
        
        // 複数の経路を生成
        for (int i = 0; i < numberOfPaths; i++)
        {
            GeneratePath(i == 0); // 最初の経路はメインパス
        }
        
        // 通路をさらに追加（オプション）
        AddRandomConnections();
        
        // 通路を少し広げる（NavMeshのために）
        WidenCorridors();
        
        // 迷路を可視化
        InstantiateMaze();
    }
    
    void GeneratePath(bool isMainPath)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = startPos;
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        // スタート位置を通路に設定
        maze[current.x, current.y] = false;
        stack.Push(current);
        visited.Add(current);
        path.Add(current);
        
        // ランダムな値を使って経路を少し変える（メインでない経路の場合）
        Vector2Int offset = Vector2Int.zero;
        if (!isMainPath)
        {
            // スタート位置を少し変える
            int randX = Random.Range(1, 3) * 2 - 1; // 1 or 3
            int randY = Random.Range(1, 3) * 2 - 1; // 1 or 3
            current = new Vector2Int(randX, randY);
            
            // 目標位置も少し変える
            offset = new Vector2Int(
                Random.Range(-4, 5) * 2,
                Random.Range(-4, 5) * 2
            );
        }
        
        // 目標位置
        Vector2Int target = goalPos + offset;
        target.x = Mathf.Clamp(target.x, 1, mazeWidth - 2);
        target.y = Mathf.Clamp(target.y, 1, mazeHeight - 2);
        
        while (stack.Count > 0)
        {
            current = stack.Pop();
            
            // ゴールに到達したら経路生成完了
            if (current == target)
            {
                // メインパスの場合は経路を保存
                if (isMainPath)
                {
                    mainPath = new List<Vector2Int>(path);
                }
                break;
            }
            
            // 次に進める方向をリストアップ
            List<Vector2Int> directions = new List<Vector2Int>();
            
            // 上下左右の方向
            Vector2Int[] possibleDirs = new Vector2Int[]
            {
                new Vector2Int(0, 2),   // 上
                new Vector2Int(2, 0),   // 右
                new Vector2Int(0, -2),  // 下
                new Vector2Int(-2, 0)   // 左
            };
            
            // 方向をシャッフル
            ShuffleDirections(possibleDirs);
            
            // ゴールに向かう方向を優先（確率的に）
            if (Random.value < 0.7f)
            {
                // ゴールの方向を計算
                int dx = target.x > current.x ? 2 : (target.x < current.x ? -2 : 0);
                int dy = target.y > current.y ? 2 : (target.y < current.y ? -2 : 0);
                
                // x方向とy方向のどちらかをランダムに選ぶ
                if (dx != 0 && dy != 0)
                {
                    if (Random.value < 0.5f)
                    {
                        directions.Add(new Vector2Int(dx, 0));
                        directions.Add(new Vector2Int(0, dy));
                    }
                    else
                    {
                        directions.Add(new Vector2Int(0, dy));
                        directions.Add(new Vector2Int(dx, 0));
                    }
                }
                else if (dx != 0)
                {
                    directions.Add(new Vector2Int(dx, 0));
                }
                else if (dy != 0)
                {
                    directions.Add(new Vector2Int(0, dy));
                }
            }
            
            // 残りの方向を追加
            foreach (Vector2Int dir in possibleDirs)
            {
                if (!directions.Contains(dir))
                {
                    directions.Add(dir);
                }
            }
            
            bool moved = false;
            
            // 各方向を試す
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                
                // 範囲内かつ未訪問の場合
                if (next.x > 0 && next.x < mazeWidth - 1 && 
                    next.y > 0 && next.y < mazeHeight - 1 && 
                    maze[next.x, next.y])
                {
                    // 現在位置から次の位置までの壁を取り除く
                    Vector2Int wall = current + new Vector2Int(dir.x / 2, dir.y / 2);
                    maze[wall.x, wall.y] = false;  // 壁を通路に
                    maze[next.x, next.y] = false;  // 次の位置も通路に
                    
                    stack.Push(current);   // 現在位置をスタックに戻す
                    stack.Push(next);      // 次の位置をスタックに追加
                    
                    visited.Add(next);
                    path.Add(next);
                    
                    moved = true;
                    break;
                }
            }
            
            // 行き止まりの場合、パスから現在位置を削除
            if (!moved && path.Count > 0 && path[path.Count - 1] == current)
            {
                path.RemoveAt(path.Count - 1);
            }
        }
    }
    
    void AddRandomConnections()
    {
        // ランダムな接続を追加して迷路の複雑さを増す
        for (int i = 0; i < mainPath.Count; i += 4)
        {
            if (i >= mainPath.Count) break;
            
            Vector2Int pos = mainPath[i];
            
            // ランダムな方向に分岐
            if (Random.value < branchProbability)
            {
                int direction = Random.Range(0, 4);
                Vector2Int dir = Vector2Int.zero;
                
                switch (direction)
                {
                    case 0: dir = new Vector2Int(0, 2); break;  // 上
                    case 1: dir = new Vector2Int(2, 0); break;  // 右
                    case 2: dir = new Vector2Int(0, -2); break; // 下
                    case 3: dir = new Vector2Int(-2, 0); break; // 左
                }
                
                Vector2Int next = pos + dir;
                
                // 範囲内かつ壁の場合
                if (next.x > 0 && next.x < mazeWidth - 1 && 
                    next.y > 0 && next.y < mazeHeight - 1 && 
                    maze[next.x, next.y])
                {
                    // 壁を取り除く
                    Vector2Int wall = pos + new Vector2Int(dir.x / 2, dir.y / 2);
                    maze[wall.x, wall.y] = false;  // 壁を通路に
                    maze[next.x, next.y] = false;  // 次の位置も通路に
                }
            }
        }
    }
    
    // 通路を広げる処理（NavMeshがより生成しやすくなる）
    void WidenCorridors()
    {
        bool[,] originalMaze = new bool[mazeWidth, mazeHeight];
        
        // 元の迷路を一時保存
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                originalMaze[x, y] = maze[x, y];
            }
        }
        
        // 通路の周りの壁も通路にする（特に角を滑らかにする）
        for (int x = 1; x < mazeWidth - 1; x++)
        {
            for (int y = 1; y < mazeHeight - 1; y++)
            {
                if (!originalMaze[x, y]) // 通路の場合
                {
                    // 周囲の斜め方向の壁をチェック
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        for (int dy = -1; dy <= 1; dy += 2)
                        {
                            // 対角の位置
                            int cornerX = x + dx;
                            int cornerY = y + dy;
                            
                            // 範囲内かつ壁の場合
                            if (cornerX > 0 && cornerX < mazeWidth - 1 && 
                                cornerY > 0 && cornerY < mazeHeight - 1 && 
                                originalMaze[cornerX, cornerY])
                            {
                                // 対角に隣接する2つの位置が通路なら、対角も通路にする
                                if (!originalMaze[x + dx, y] && !originalMaze[x, y + dy])
                                {
                                    maze[cornerX, cornerY] = false;  // 壁を通路に
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    void InstantiateMaze()
    {
        // 子オブジェクトをすべて削除
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // 床を生成（すべてのマスに）
        GameObject floorParent = new GameObject("Floors");
        floorParent.transform.parent = transform;
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 floorPosition = new Vector3(x, 0, y);
                GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.identity, floorParent.transform);
                floor.name = $"Floor_{x}_{y}";
            }
        }
        
        // 壁を生成
        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.parent = transform;
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (maze[x, y]) // 壁の場合
                {
                    Vector3 wallPosition = new Vector3(x, 0.5f, y);
                    GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
                    wall.name = $"Wall_{x}_{y}";
                }
            }
        }
        
        // スタートとゴールのマーカーを設置
        if (startMarker != null)
        {
            GameObject start = Instantiate(startMarker, new Vector3(startPos.x, 0.1f, startPos.y), Quaternion.identity, transform);
            start.name = "StartMarker";
        }
        
        if (goalMarker != null)
        {
            GameObject goal = Instantiate(goalMarker, new Vector3(goalPos.x, 0.1f, goalPos.y), Quaternion.identity, transform);
            goal.name = "GoalMarker";
        }
    }
    
    // 方向配列をシャッフルするヘルパーメソッド
    void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = Random.Range(i, directions.Length);
            Vector2Int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }
    
    // NavMeshを構築するコルーチン - シンプル版
    IEnumerator BuildNavMeshDelayed()
    {
        // 迷路の生成が完了するまでより長く待機
        yield return new WaitForSeconds(0.5f);
        
        if (navMeshSurface != null)
        {
            Debug.Log("NavMeshを構築しています...");
            
            // 基本的な設定のみ
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.defaultArea = 0; // Walkable
            
            // より詳細な設定
            navMeshSurface.overrideVoxelSize = true;
            navMeshSurface.voxelSize = voxelSize;
            
            // NavMeshを構築
            navMeshSurface.BuildNavMesh();
            
            Debug.Log("NavMesh構築完了");
        }
    }
    
    // 手動でNavMeshを再構築するためのパブリックメソッド
    public void RebuildNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh再構築完了");
        }
    }
    
    // エディタでのギズモ表示（迷路構造を視覚化）
    void OnDrawGizmosSelected()
    {
        if (maze == null) return;
        
        // 迷路の構造を表示
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (!maze[x, y]) // 通路
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(new Vector3(x, 0.1f, y), new Vector3(0.8f, 0.1f, 0.8f));
                }
            }
        }
        
        // メインパスを強調表示
        Gizmos.color = Color.blue;
        for (int i = 0; i < mainPath.Count - 1; i++)
        {
            Vector3 start = new Vector3(mainPath[i].x, 0.15f, mainPath[i].y);
            Vector3 end = new Vector3(mainPath[i+1].x, 0.15f, mainPath[i+1].y);
            Gizmos.DrawLine(start, end);
        }
    }
}