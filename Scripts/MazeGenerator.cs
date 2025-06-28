using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using UnityEngine;
using System.Linq;

public class MazeGenerator : MonoBehaviour
{
    [Tooltip("Number of iterations per algorithm")] public int runs = 10;
    // List of algorithms to benchmark
    private Dictionary<string, Action> algorithms;

    [Header("Maze Settings")]
    public int width = 51;
    public int height = 51;
    public float tileSize = 1f;      // размер ячейки в мир-единицах
    public GameObject cheesePrefab; 


    [Header("Prefabs & Transforms")]
    public GameObject wallPrefab;     // префаб стены
    public GameObject roadPrefab;     // префаб дорожки
    public GameObject playerPrefab;   // префаб персонажа (с Rigidbody2D и Collider2D)
    public Transform mazeParent;      // пустой объект для группировки тайлов

    private int[,] maze;
    private UnionFind uf;
    private List<Edge> edges = new List<Edge>();
    private System.Random rnd = new System.Random();

    void Awake()
    {
        // Populate your algorithms here
        algorithms = new Dictionary<string, Action>
        {
            { "Eller", () => GenerateMazeEller() },
            { "Prim", () => GenerateMazePrim() },
            { "Wilson", () => GenerateMazeWilson() },
            { "HuntAndKill", () => GenerateMazeHuntAndKill() },
            { "BinaryTree", () => GenerateMazeBinaryTree() },
            { "RecursiveDivision", () => GenerateMazeRecursiveDivision() },
            { "Sidewinder", () => GenerateMazeSidewinder() },
            { "AldousBroder", () => GenerateMazeAldousBroder() },
            { "Kruskal", () => GenerateMazeKruskal() },
            { "Backtracking ", () => GenerateMazeBacktracking() }
        };

        StartCoroutine(RunBenchmarks());
    }

    private System.Collections.IEnumerator RunBenchmarks()
    {
        string basePath = "D://documents//mazer";
        UnityEngine.Debug.Log("Benchmark folder: " + basePath);

        foreach (var kvp in algorithms)
        {
            string name = kvp.Key;
            var gen = kvp.Value;
            string filePath = Path.Combine(basePath, name + "_benchmark.csv");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Run,TimeMs,MemoryBytes");

                for (int i = 1; i <= runs; i++)
                {
                    // collect memory
                    long memBefore = GC.GetTotalMemory(true);

                    var sw = Stopwatch.StartNew();
                    gen();
                    sw.Stop();

                    long memAfter = GC.GetTotalMemory(false);
                    long memoryUsed = memAfter - memBefore;

                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F4},{2}", i, sw.Elapsed.TotalMilliseconds, memoryUsed));

                    // yield to avoid freezing the editor
                    if (i % 50 == 0)
                        yield return null;
                }
            }

            UnityEngine.Debug.Log($"Finished benchmarking {name}, results: {filePath}");
        }

        UnityEngine.Debug.Log("All benchmarks completed.");
    }

    void Start()
    {
        // Выбор алгоритма на основе настройки
        switch (MazeSettings.SelectedAlgorithm)
        {
            case MazeSettings.Algorithm.Kruskal:
                GenerateMazeKruskal();
                break;
            case MazeSettings.Algorithm.Backtracking:
                GenerateMazeBacktracking();
                break;
            case MazeSettings.Algorithm.Prim:
                GenerateMazePrim();
                break;
            case MazeSettings.Algorithm.Eller:
                GenerateMazeEller();
                break;
            case MazeSettings.Algorithm.Wilson:
                GenerateMazeWilson();
                break;
            case MazeSettings.Algorithm.AldousBroder:
                GenerateMazeAldousBroder();
                break;
            case MazeSettings.Algorithm.HuntAndKill:
                GenerateMazeHuntAndKill();
                break;
            case MazeSettings.Algorithm.BinaryTree:
                GenerateMazeBinaryTree();
                break;
            case MazeSettings.Algorithm.RecursiveDivision:
                GenerateMazeRecursiveDivision();
                break;
            case MazeSettings.Algorithm.Sidewinder:
                GenerateMazeSidewinder();
                break;
            default:
                GenerateMazeKruskal();
                break;
        }
        DrawMaze();
        SpawnPlayer();
        SetupCamera();
    }

    #region Kruskal
    private void GenerateMazeKruskal()
    {
        maze = new int[width, height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                maze[i, j] = 1;

        int cxCount = (width - 1) / 2;
        int cyCount = (height - 1) / 2;
        uf = new UnionFind(cxCount * cyCount);

        edges.Clear();
        for (int cx = 0; cx < cxCount; cx++)
            for (int cy = 0; cy < cyCount; cy++)
            {
                int idx = cx + cy * cxCount;
                if (cx < cxCount - 1)
                    edges.Add(new Edge(idx, idx + 1, cx * 2 + 2, cy * 2 + 1));
                if (cy < cyCount - 1)
                    edges.Add(new Edge(idx, idx + cxCount, cx * 2 + 1, cy * 2 + 2));
            }

        Shuffle(edges);
        foreach (var e in edges)
            if (uf.Union(e.a, e.b))
                maze[e.wx, e.wy] = 0;

        for (int cx = 0; cx < cxCount; cx++)
            for (int cy = 0; cy < cyCount; cy++)
                maze[cx * 2 + 1, cy * 2 + 1] = 0;
    }
    #endregion

    #region Backtracking
    private void GenerateMazeBacktracking()
    {
        // Инициализируем все стены
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        // Запускаем DFS из случайной стартовой клетки
        System.Random rnd = new System.Random();
        int startX = rnd.Next(width / 2) * 2 + 1;
        int startY = rnd.Next(height / 2) * 2 + 1;
        Carve(startX, startY, rnd);
    }

    private void Carve(int x, int y, System.Random rnd)
    {
        maze[x, y] = 0;
        // Четыре направления: вверх, вниз, влево, вправо
        var dirs = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };
        // Перемешиваем порядок
        for (int i = dirs.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
        }

        // Пробуем пройти по каждому направлению
        foreach (var d in dirs)
        {
            int nx = x + d.x * 2;
            int ny = y + d.y * 2;
            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && maze[nx, ny] == 1)
            {
                // Убираем стену между текущей и соседней клеткой
                maze[x + d.x, y + d.y] = 0;
                Carve(nx, ny, rnd);
            }
        }
    }
    #endregion

    #region Prim
    private void GenerateMazePrim()
    {
        // Подгоняем размеры под нечетные
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Заполняем всё стенами
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                maze[i, j] = 1;

        // Случайная стартовая ячейка с нечетными координатами
        Vector2Int start = new Vector2Int();
        do { start.x = UnityEngine.Random.Range(0, h); } while (start.x % 2 == 0);
        do { start.y = UnityEngine.Random.Range(0, w); } while (start.y % 2 == 0);

        maze[start.x, start.y] = 0;

        // Список открытых клеток
        List<Vector2Int> openCells = new List<Vector2Int> { start };

        // Пока есть открытые клетки
        while (openCells.Count > 0)
        {
            // Берём случайную открытую клетку
            int idx = UnityEngine.Random.Range(0, openCells.Count);
            Vector2Int cell = openCells[idx];

            // Ищем её непосещённых «соседей»
            List<Vector2Int> neigh = GetNeighbors(cell, h, w);

            // Если нет доступных соседей — удаляем её из списка
            while (neigh.Count == 0)
            {
                openCells.RemoveAt(idx);
                if (openCells.Count == 0) break;
                idx = UnityEngine.Random.Range(0, openCells.Count);
                cell = openCells[idx];
                neigh = GetNeighbors(cell, h, w);
            }
            if (openCells.Count == 0) break;

            // Выбираем случайного соседа
            Vector2Int choice = neigh[UnityEngine.Random.Range(0, neigh.Count)];
            openCells.Add(choice);

            // Если это был единственный сосед — удаляем исходную
            if (neigh.Count == 1)
                openCells.RemoveAt(idx);

            // Прорубаем ход в choice и между cell и choice
            maze[choice.x, choice.y] = 0;
            int midX = (choice.x + cell.x) / 2;
            int midY = (choice.y + cell.y) / 2;
            maze[midX, midY] = 0;
        }

    }

    // Возвращает список соседних клеток на расстоянии 2, ещё не вырубленных
    List<Vector2Int> GetNeighbors(Vector2Int cell, int h, int w)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        // Четыре направления: влево, вправо, вверх, вниз (смещение ±2)
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(0, -2),
            new Vector2Int(0,  2),
            new Vector2Int(-2, 0),
            new Vector2Int( 2, 0)
        };

        foreach (var d in dirs)
        {
            Vector2Int n = cell + d;
            if (n.x > 0 && n.x < h && n.y > 0 && n.y < w && maze[n.x, n.y] == 1)
                result.Add(n);
        }
        return result;
    }

    #endregion

    #region Eller
    private void GenerateMazeEller()
    {
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;

        maze = new int[h, w];

        // Инициализация: стены по краям и между клетками
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                // если обе координаты нечетные → проходим, иначе стена
                maze[i, j] = (i % 2 == 1 && j % 2 == 1) ? 0 : 1;
            }
        }

        // Вход
        // maze[0, 1] = 0;

        // Каждая клетка (1, j) отдаётся в свой собственный сет
        List<List<Vector2Int>> sets = new List<List<Vector2Int>>();
        for (int j = 1; j < w; j += 2)
        {
            sets.Add(new List<Vector2Int> { new Vector2Int(1, j) });
        }

        // Основной цикл по строкам
        for (int i = 1; i < h; i += 2)
        {
            // Очищаем сеты старых строк
            foreach (var set in sets)
            {
                set.RemoveAll(c => c.x < i);
            }

            // Соединяем соседние по горизонтали
            for (int j = 3; j < w; j += 2)
            {
                int set1 = IndexOfSet(sets, new Vector2Int(i, j - 2));
                int set2 = IndexOfSet(sets, new Vector2Int(i, j));
                if (set1 != set2)
                {
                    bool join = (i != h - 2)
                        ? (UnityEngine.Random.Range(0, 2) == 0)
                        : true;

                    if (join)
                    {
                        // объединяем множества и рубим стену между ними
                        var removed = sets[set2];
                        sets.RemoveAt(set2);
                        if (set2 < set1) set1--;
                        sets[set1].AddRange(removed);
                        maze[i, j - 1] = 0;
                    }
                }
            }

            // Если это предпоследняя строка — дальше не нужно
            if (i == h - 2) break;

            // Строим «ступеньки» вниз
            int initialSetCount = sets.Count;
            for (int s = 0; s < initialSetCount; s++)
            {
                bool continued = false;
                int initialLen = sets[s].Count;

                for (int k = 0; k < initialLen; k++)
                {
                    Vector2Int coord = sets[s][k];
                    Vector2Int down = new Vector2Int(coord.x + 2, coord.y);
                    if (down.x != i + 2) continue;

                    bool add = UnityEngine.Random.Range(0, 2) == 0;
                    if (add)
                    {
                        continued = true;
                        sets[s].Add(down);
                        maze[down.x - 1, down.y] = 0;
                    }
                    else
                    {
                        // создаём новый сет для этой клетки
                        sets.Add(new List<Vector2Int> { down });
                    }
                }

                // Если ни одной вертикали не провели — обеспечиваем как минимум одну
                if (!continued)
                {
                    Vector2Int pick;
                    do
                    {
                        pick = sets[s][UnityEngine.Random.Range(0, sets[s].Count)];
                    } while (pick.x != i);
                    Vector2Int down = new Vector2Int(pick.x + 2, pick.y);

                    // удаляем дублирующий сет, если он уже есть
                    int dup = IndexOfSet(sets, down);
                    if (dup != -1) sets.RemoveAt(dup);

                    sets[s].Add(down);
                    maze[down.x - 1, down.y] = 0;
                }
            }
        }

        // Выход
        // maze[h - 1, w - 2] = 0;
    }

    // Поиск, к какому множеству принадлежит клетка c
    int IndexOfSet(List<List<Vector2Int>> sets, Vector2Int c)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].Contains(c))
                return i;
        return -1;
    }
    #endregion

    #region Wilson

    void GenerateMazeWilson()
    {
        // Подгоняем размеры под нечетные
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Заполняем всё стенами (1)
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                maze[i, j] = 1;

        // Начальная клетка
        Vector2Int s = RandCoord(w, h);
        maze[s.x, s.y] = 0;

        // Пока лабиринт не заполнен
        while (!IsComplete(h, w))
        {
            // Выбираем случайную ещё не пройденную клетку
            Vector2Int c;
            do
            {
                c = RandCoord(w, h);
            } while (maze[c.x, c.y] != 1);

            // Отмечаем её «во временном пути»
            maze[c.x, c.y] = 2;
            List<Vector2Int> path = new List<Vector2Int> { c };

            // Делаем случайные шаги, пока не вернёмся в уже вырубленный участок
            while (maze[path[path.Count - 1].x, path[path.Count - 1].y] != 0)
            {
                Vector2Int last = path[path.Count - 1];
                List<Vector2Int> neigh = NeighborsAB(last, h, w);
                Vector2Int nb = neigh[UnityEngine.Random.Range(0, neigh.Count)];

                // Добавляем в путь и рубим стену между
                path.Add(nb);
                int midX = (nb.x + last.x) / 2;
                int midY = (nb.y + last.y) / 2;
                maze[midX, midY] = 2;

                if (maze[nb.x, nb.y] == 0)
                {
                    // Замыкаем путь: все 2 → 0
                    for (int i = 0; i < h; i++)
                        for (int j = 0; j < w; j++)
                            if (maze[i, j] == 2)
                                maze[i, j] = 0;
                }
                else
                {
                    // Помечаем nb в пути
                    maze[nb.x, nb.y] = 2;
                    int loc = IndexOfCoord(path, nb);
                    if (loc != path.Count - 1)
                    {
                        // Удаляем цикл: элементы после loc
                        var removed = path.GetRange(loc + 1, path.Count - loc - 1);
                        path.RemoveRange(loc + 1, path.Count - loc - 1);

                        // Восстанавливаем стены в удалённом цикле
                        for (int k = removed.Count - 1; k >= 0; k--)
                        {
                            Vector2Int on = removed[k];
                            Vector2Int next = (k > 0) ? removed[k - 1] : path[path.Count - 1];

                            if (k != removed.Count - 1)
                                maze[on.x, on.y] = 1;

                            int mx = (on.x + next.x) / 2;
                            int my = (on.y + next.y) / 2;
                            maze[mx, my] = 1;
                        }
                    }
                }
            }
        }

        // Вход и выход
        // maze[0, 1] = 0;
        // maze[h - 1, w - 2] = 0;
    }

    // Проверяет, все ли «клетки» пройдены (чётные по обеим координатам)
    bool IsComplete(int h, int w)
    {
        for (int i = 1; i < h; i += 2)
            for (int j = 1; j < w; j += 2)
                if (maze[i, j] != 0)
                    return false;
        return true;
    }

    // Возвращает список соседей на расстоянии 2 (без проверки статуса)
    List<Vector2Int> NeighborsAB(Vector2Int cell, int h, int w)
    {
        var result = new List<Vector2Int>();
        Vector2Int[] dirs = {
            new Vector2Int(0, -2),
            new Vector2Int(0,  2),
            new Vector2Int(-2, 0),
            new Vector2Int( 2, 0)
        };
        foreach (var d in dirs)
        {
            var n = cell + d;
            if (n.x > 0 && n.x < h && n.y > 0 && n.y < w)
                result.Add(n);
        }
        return result;
    }

    // Находит индекс координаты c в списке s
    int IndexOfCoord(List<Vector2Int> s, Vector2Int c)
    {
        for (int i = 0; i < s.Count; i++)
            if (s[i] == c)
                return i;
        return -1;
    }

    // Возвращает случайную клетку с обеими координатами нечётными
    Vector2Int RandCoord(int w, int h)
    {
        int rx, ry;
        do { rx = UnityEngine.Random.Range(1, h); } while (rx % 2 == 0);
        do { ry = UnityEngine.Random.Range(1, w); } while (ry % 2 == 0);
        return new Vector2Int(rx, ry);
    }

    #endregion

    #region Aldous-Broder 

    void GenerateMazeAldousBroder()
    {
        // Подгоняем размеры к нечетным
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Считаем общее число «не посещённых» клеток (по обеим координатам нечётных)
        int unvisited = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                if (i % 2 == 1 && j % 2 == 1)
                    unvisited++;
                maze[i, j] = 1; // стена
            }
        }

        // Случайная стартовая клетка с нечётными координатами
        Vector2Int current = Vector2Int.zero;
        do
        {
            current.x = UnityEngine.Random.Range(0, h);
            current.y = UnityEngine.Random.Range(0, w);
        } while (current.x % 2 == 0 || current.y % 2 == 0);

        maze[current.x, current.y] = 0;
        unvisited--;

        // Пока есть непосещённые
        while (unvisited > 0)
        {
            // Список соседних клеток на шаг 2
            List<Vector2Int> neigh = NeighborsAB(current, h, w);
            if (neigh.Count == 0)
                break;

            // Выбираем случайного соседа
            Vector2Int next = neigh[UnityEngine.Random.Range(0, neigh.Count)];

            // Если сосед ещё не посещён — рубим между ними стену и отмечаем
            if (maze[next.x, next.y] == 1)
            {
                maze[next.x, next.y] = 0;
                int mx = (next.x + current.x) / 2;
                int my = (next.y + current.y) / 2;
                maze[mx, my] = 0;
                unvisited--;
            }

            // Переходим в эту клетку
            current = next;
        }

        // Вход и выход
        // maze[0, 1] = 0;
        // maze[h - 1, w - 2] = 0;
    }

    #endregion

    #region Hunt and Kill

    void GenerateMazeHuntAndKill()
    {
        // Подгоняем размеры под нечетные
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Заполняем всё стенами (1)
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                maze[i, j] = 1;

        // Вход сверху
        // maze[0, 1] = 0;
        // maze[1, 1] = 0;

        // Текущая позиция
        Vector2Int current = new Vector2Int(1, 1);

        // Пока не всё пройдено
        while (!IsComplete(h, w))
        {
            // Соседи через шаг 2, которые ещё стены
            List<Vector2Int> n = Neighbors(maze, current, h, w);

            if (n.Count == 0)
            {
                // «Охота»: находим новую стартовую стенную клетку с соседним проходом
                (Vector2Int cell, Vector2Int neighbour) = FindCoord(maze, h, w);
                current = cell;
                maze[cell.x, cell.y] = 0;
                // Удаляем стену между cell и neighbour
                int mx = (cell.x + neighbour.x) / 2;
                int my = (cell.y + neighbour.y) / 2;
                maze[mx, my] = 0;
            }
            else
            {
                // «Убийство»: случайный сосед
                Vector2Int nb = n[UnityEngine.Random.Range(0, n.Count)];
                // Прорубаем в nb и между ними
                maze[nb.x, nb.y] = 0;
                int mx = (nb.x + current.x) / 2;
                int my = (nb.y + current.y) / 2;
                maze[mx, my] = 0;
                // Переходим на nb
                current = nb;
            }
        }

        // Выход справа внизу
        // maze[h - 2, w - 1] = 0;
    }

    // Возвращает непроходные соседние клетки через шаг 2
    List<Vector2Int> Neighbors(int[,] maze, Vector2Int cell, int h, int w)
    {
        var result = new List<Vector2Int>();
        Vector2Int[] dirs = {
            new Vector2Int(0, -2),
            new Vector2Int(0,  2),
            new Vector2Int(-2, 0),
            new Vector2Int( 2, 0)
        };
        foreach (var d in dirs)
        {
            Vector2Int n = cell + d;
            if (n.x > 0 && n.x < h && n.y > 0 && n.y < w && maze[n.x, n.y] == 1)
                result.Add(n);
        }
        return result;
    }

    // Находит первую стенную клетку с нечётными координатами, у которой есть смежный проход
    // Возвращает пару (cell, neighbour)
    (Vector2Int, Vector2Int) FindCoord(int[,] maze, int h, int w)
    {
        for (int i = 1; i < h; i += 2)
        {
            for (int j = 1; j < w; j += 2)
            {
                if (maze[i, j] == 1)
                {
                    // Ищем любого соседнего прохода через шаг 2
                    Vector2Int cell = new Vector2Int(i, j);
                    foreach (var neighbour in NeighborsAB(cell, h, w))
                    {
                        if (maze[neighbour.x, neighbour.y] == 0)
                            return (cell, neighbour);
                    }
                }
            }
        }
        // На всякий случай: никогда не должно случиться
        return (new Vector2Int(1, 1), new Vector2Int(1, 1));
    }

    #endregion

    #region Binary Tree

    void GenerateMazeBinaryTree()
    {
        // Подгоняем размеры к нечетным
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Инициализация: все стены, кроме «клеток» (где обе координаты нечетные)
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                maze[i, j] = (i % 2 == 1 && j % 2 == 1) ? 0 : 1;
            }
        }
            

        // Основной проход: для каждой «клетки» решаем, прорубить юг или восток
        for (int row = 1; row < w; row += 2)
        {
            for (int col = 1; col < h; col += 2)
            {
                // Угловая клетка (нижний правый) пропускается
                if (row == w - 2 && col == h - 2)
                    break;
                int carveSouth = (Mathf.FloorToInt(UnityEngine.Random.Range(0, 2)));

                if (row == w - 2)
                    // в последней строке клеток нельзя рубить вниз
                    carveSouth = 1;
                if (col == h - 2)
                    // в последнем столбце клеток нельзя рубить вправо
                    carveSouth = 0;
                    
                if (carveSouth == 1)
                {
                    // рубим стену вниз
                    maze[col + 1, row] = 0;
                }
                else
                {
                    // рубим стену вправо
                    maze[col, row + 1] = 0;
                }
            }
        }

        // Вход и выход
        // maze[0, 1] = 0;
        // maze[h - 1, w - 2] = 0;
    }

    #endregion

    #region Recursive Division

    void GenerateMazeRecursiveDivision()
    {
        // Подгоняем размеры к нечётным
        int w = width - (width % 2) + 1;
        int h = height - (height % 2) + 1;
        maze = new int[h, w];

        // Инициализация: внешние стены = 1, внутри = 0
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                // по границе — стена, иначе проход
                if (i == 0 || j == 0 || i == h - 1 || j == w - 1)
                    maze[i, j] = 1;
                else
                    maze[i, j] = 0;
            }
        }

        // Запускаем рекурсивное деление во внутренней области [1..h-2]×[1..w-2]
        Divide(1, h - 2, 1, w - 2);

        // Вход и выход
        // maze[0, 1] = 0;
        // maze[h - 1, w - 2] = 0;
    }

    // Рекурсивная функция деления области iStart..iEnd, jStart..jEnd
    void Divide(int iStart, int iEnd, int jStart, int jEnd)
    {
        int iDim = iEnd - iStart;
        int jDim = jEnd - jStart;
        if (iDim <= 0 || jDim <= 0)
            return;

        bool horizontal = ChooseOrientation(iDim, jDim);

        if (horizontal)
        {
            // горизонтальная стена → выбираем нечётный ряд «split» и чётный столб «hole»
            int split;
            do { split = UnityEngine.Random.Range(iStart, iEnd + 1); } while (split % 2 == 1);
            int hole;
            do { hole = UnityEngine.Random.Range(jStart, jEnd + 1); } while (hole % 2 == 0);

            // рисуем стену
            for (int j = jStart; j <= jEnd; j++)
                if (j != hole)
                    maze[split, j] = 1;

            // рекурсия сверху и снизу
            Divide(iStart, split - 1, jStart, jEnd);
            Divide(split + 1, iEnd,   jStart, jEnd);
        }
        else
        {
            // вертикальная стена → выбираем нечётный столб «split» и чётный ряд «hole»
            int split;
            do { split = UnityEngine.Random.Range(jStart, jEnd + 1); } while (split % 2 == 1);
            int hole;
            do { hole = UnityEngine.Random.Range(iStart, iEnd + 1); } while (hole % 2 == 0);

            // рисуем стену
            for (int i = iStart; i <= iEnd; i++)
                if (i != hole)
                    maze[i, split] = 1;

            // рекурсия слева и справа
            Divide(iStart, iEnd, jStart,   split - 1);
            Divide(iStart, iEnd, split + 1, jEnd);
        }
    }

    // Выбор ориентации: горизонталь, если область более «широкая», иначе вертикаль; при равенстве — случайно
    bool ChooseOrientation(int iDim, int jDim)
    {
        if (iDim < jDim)      return false;  // вертикальная
        if (jDim < iDim)      return true;   // горизонтальная
        return (UnityEngine.Random.Range(0, 2) == 0);
    }

    #endregion

    #region Sidewinder

    void GenerateMazeSidewinder()
    {
        // Убедимся, что размеры нечетные
        int w = width + (width % 2 == 0 ? 1 : 0);
        int h = height + (height % 2 == 0 ? 1 : 0);
        maze = new int[h, w];

        // Инициализация: 0 — путь, 1 — стена
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                maze[i, j] = (i % 2 == 1 && j % 2 == 1) ? 0 : 1;
            }
        }

        // Основной проход
        for (int row = 1; row < h; row += 2)
        {
            int runStart = 1;

            for (int col = 1; col < w; col += 2)
            {
                int continueRun = (row == 1) ? 1 : (Mathf.FloorToInt(UnityEngine.Random.Range(0, 2)));
                if (col == w - 2) continueRun = 0;

                if (continueRun == 1)
                {
                    maze[row, col + 1] = 0;
                }
                else if (row != 1)
                {
                    // Завершаем серию: пробиваем вверх в случайной ячейке серии
                    int up;
                    do
                    {
                        up = Mathf.FloorToInt(UnityEngine.Random.Range(0, col - runStart)) + runStart;
                    } while (up % 2 == 0);

                    maze[row - 1, up] = 0;
                    runStart = col + 2;
                }
            }
        }

        // Вход и выход
        // maze[0, 1] = 0;
        // maze[h - 1, w - 2] = 0;
    }

    #endregion

    private void DrawMaze()
    {
        // очистка
        for (int i = mazeParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(mazeParent.GetChild(i).gameObject);

        // отрисовка на XY плоскости
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x * tileSize, -y * tileSize);
                GameObject prefab = maze[x, y] == 1 ? wallPrefab : roadPrefab;
                if (prefab == null) continue;

                var tile = Instantiate(prefab, pos, Quaternion.identity, mazeParent);
                tile.name = maze[x, y] == 1 ? $"Wall_{x}_{y}" : $"Road_{x}_{y}";
                tile.transform.localScale = Vector2.one * tileSize;

                if (maze[x, y] == 1)
                {
                    // 2D коллайдер и статичный Rigidbody2D
                    if (tile.GetComponent<Collider2D>() == null)
                        tile.AddComponent<BoxCollider2D>();
                    if (tile.GetComponent<Rigidbody2D>() == null)
                    {
                        var rb2d = tile.AddComponent<Rigidbody2D>();
                        rb2d.bodyType = RigidbodyType2D.Static;
                    }
                }
            }
        }

        int goalX = width - 2;
        int goalY = height - 2;
        Vector2 goalPos = new Vector2(goalX * tileSize, -goalY * tileSize);

        if (cheesePrefab != null)
        {
            // создаём сыр
            GameObject cheese = Instantiate(cheesePrefab, goalPos, Quaternion.identity, mazeParent);
            cheese.name = "Cheese";

            // добавляем коллайдер-триггер на ту же GameObject
            var bc = cheese.GetComponent<BoxCollider2D>();
            if (bc == null) bc = cheese.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = Vector2.one * tileSize;

            // и скрипт-обработчик конца уровня
            if (cheese.GetComponent<GoalTrigger>() == null)
                cheese.AddComponent<GoalTrigger>();
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        // for (int y = height - 1; y >= 0; y--)
        // {
        //     for (int x = 0; x < width; x++)
        //     {
        //         if (maze[x, y] == 0)
        //         {
        //             Vector2 pos = new Vector2(x * tileSize, y * tileSize);
        //             Instantiate(playerPrefab, pos, Quaternion.identity);
        //             return;
        //         }
        //     }
        // }
        Vector2 pos = new Vector2(1f * tileSize, -0.95f * tileSize);
        Instantiate(playerPrefab, pos, Quaternion.identity);
        return;
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        float cenX = (width - 1) * tileSize / 2f;
        float cenY = (height - 1) * tileSize / 2f;
        cam.transform.position = new Vector3(cenX, -cenY, -10f);

        // orthoSize по большей стороне
        float sizeX = width * tileSize / 2f;
        float sizeY = height * tileSize / 2f;
        cam.orthographicSize = Mathf.Max(sizeX, sizeY);
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rnd.Next(n--);
            var tmp = list[n]; list[n] = list[k]; list[k] = tmp;
        }
    }

    private class Edge { public int a,b,wx,wy; public Edge(int a,int b,int wx,int wy){this.a=a;this.b=b;this.wx=wx;this.wy=wy;} }
    private class UnionFind { private int[] p; public UnionFind(int n){p=new int[n];for(int i=0;i<n;i++)p[i]=-1;} public int Find(int x)=>p[x]<0?x:(p[x]=Find(p[x])); public bool Union(int a,int b){a=Find(a);b=Find(b);if(a==b)return false; if(p[a]>p[b]) (a,b)=(b,a); p[a]+=p[b];p[b]=a;return true;} }
}