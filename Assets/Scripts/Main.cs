using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Main : MonoBehaviour
{
    //存储水果预制体的数组
    public GameObject[] fruitPrefabs;
    //存储水果游戏对象的数组
    public GameObject[,] fruitGameObjects;
    //数组行数 行表示Y轴
    public int rowNum;
    //数组列数 列表示X轴
    public int columeNum;
    //表示游戏对象的缩放比例
    public float size;
    //时间
    public float startTime = 1;
    //缓存，存储点击的目标及和他一样名字的游戏对象，但会排除掉已存储过的
    List<GameObject> cache = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //实例化一下存储游戏对象的数组
        fruitGameObjects = new GameObject[rowNum, columeNum];
        //Debug.Log("columeNum=:" + columeNum + " rowNum=：" + rowNum);

        //用来统计存了多少个水果预制体
        int count = fruitPrefabs.Length;
        //找到物体初始重载的位置
        float xZhou = columeNum * -0.5f * size + 0.5f * size;
        float yZhou = rowNum * -0.5f * size + 0.5f * size;

        for (int n = 0; n < rowNum; n++)
        {
            for (int m = 0; m < columeNum; m++)
            {
                int i = Random.Range(0, count);
                GameObject game = Instantiate(fruitPrefabs[i]);
                game.transform.SetParent(transform);
                //要以父对象为相对坐标
                game.transform.localPosition = new Vector3(m * size + xZhou, n * size + yZhou, 0);
                //设置他的大小
                game.transform.localScale = new Vector3(size, size, size);
                //将名为 Mark 的组件类添加到 game 游戏对象。
                Mark mark = game.AddComponent<Mark>();
                mark.Column = m;
                mark.Row = n;
                mark.Index = i;
                game.name = i.ToString();
                game.AddComponent<BoxCollider2D>();
                //Debug.Log("n=:" + n + " m=：" + m);
                //将创建的游戏对象放入数组中
                fruitGameObjects[n, m] = game;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > 0)
        {
            startTime -= Time.deltaTime;
        }
        if (startTime < 0)
        {
            Debug.Log("可以点击鼠标左键了");
            //判断鼠标是否按下左键
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("点击了鼠标左键");
                //生成射线，从主摄像机向鼠标所在的地方发射一条射线
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                if (hit.transform != null)
                {
                    Debug.Log("点击了有效物体");
                    Mark mark = hit.transform.GetComponent<Mark>();
                    Debug.Log("点击的物体名称是：" + hit.transform.name);
                    //提前清除一下
                    cache.Clear();
                    Check(hit.transform.name, mark.Row, mark.Column);
                    if (cache.Count >= 3)
                    {
                        //表示你所点的游戏对象上下左右加上自己有3个及三个以上相同对象名的游戏对象，需要删除
                        ObjectDelete();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查在某个游戏对象的上下左右是不是和他同名的游戏对象
    /// </summary>
    /// <param name="type">要查找的游戏对象</param>
    /// <param name="column">要查找的游戏对象所在的行</param>
    /// <param name="row">要查找的游戏对象所在的列</param>
    void Check(string type, int row, int column)
    {
        if (column < 0 || column >= columeNum || row < 0 || row >= rowNum)
        {
            return;
        }
        GameObject game = fruitGameObjects[row, column];
        if (type != game.name)
        {
            return;
        }
        if (cache.Contains(game))
        {
            //如果当前行和列的游戏对象已存在缓存List中，就返回
            return;
        }
        cache.Add(game);
        //递归一下寻找game的上下左右
        Check(type, row, column - 1);
        Check(type, row, column + 1);
        Check(type, row - 1, column);
        Check(type, row + 1, column);
    }

    /// <summary>
    /// 删除缓存中的游戏对象
    /// </summary>
    void ObjectDelete()
    {
        for (int n = 0; n < cache.Count; n++)
        {
            Mark mark = cache[n].GetComponent<Mark>();
            fruitGameObjects[mark.Row, mark.Column] = null;
            GameObject.Destroy(cache[n]);
        }
        //删除完之后调用下落方法
        Fall();
    }
    #region 自我理解的下降方法
    //void Fall()
    //{
    //    Debug.Log($"{nameof(Main)}:进入了下落方法 ");
    //    float xZhou = columeNum * -0.5f * size + 0.5f * size;
    //    float yZhou = rowNum * -0.5f * size + 0.5f * size;
    //    //一行行遍历
    //    for(int n=0;n<columeNum;n++)
    //    {

    //        //统计有多少个空白的
    //        int count = 0;
    //        //一列列遍历
    //        for(int m=0;m<rowNum;m++)
    //        {
    //            if (fruitGameObjects[m, n] == null)
    //            {
    //                Debug.Log($"{nameof(Main)}:当前是第{n}列");
    //                Debug.Log($"{nameof(Main)}:当前是第{m}行");
    //                Debug.Log($"{nameof(Main)}:该列目前有{count}个空白");
    //                count++;
    //            }
    //            else
    //            {
    //                if (count > 0)
    //                {
    //                    Mark mark = fruitGameObjects[m, n].GetComponent<Mark>();
    //                    int row = m - count;
    //                    Debug.Log($"{nameof(Main)}:row的值是{row}");
    //                    mark.Row = row;
    //              }
    //               }
    //        }
    //    }
    //}
    #endregion

    /// <summary>
    /// 消除一部分水果后，右边的水果应该向左移
    /// </summary>
    void Fall()
    {
        float xZhou = columeNum * -0.5f * size + 0.5f * size;
        float yZhou = rowNum * -0.5f * size + 0.5f * size;
        for (int i = 0; i < rowNum; i++)
        {
            int count = 0;
            for (int j = 0; j < columeNum; j++)
            {
                if (fruitGameObjects[i, j] == null)
                {
                    count++;
                }
                else
                {
                    if (count > 0)
                    {
                        Mark mark = fruitGameObjects[i, j].GetComponent<Mark>();
                        int col = j - count;
                        mark.Column = col;
                        fruitGameObjects[i, col] = fruitGameObjects[i, j];
                        fruitGameObjects[i, col].transform.DOMove(new Vector3(col * size + xZhou, i * size + yZhou, 0), 0.3f);
                        fruitGameObjects[i, j] = null;
                    }
                }
            }
        }
        Add();
    }

    /// <summary>
    /// 从最右方将某一行缺少的对象补充
    /// </summary>
    void Add()
    {
        float xZhou = columeNum * -0.5f * size + 0.5f * size;
        float yZhou = rowNum * -0.5f * size + 0.5f * size;
        for (int i = 0; i < rowNum; i++)
        {
            for (int j = 0; j < columeNum; j++)
            {
                if(fruitGameObjects[i,j]==null)
                {
                    int n = Random.Range(0, fruitPrefabs.Length);
                    GameObject game = Instantiate(fruitPrefabs[n]);
                    game.transform.SetParent(transform);
                    //要以父对象为相对坐标
                    game.transform.localPosition = new Vector3((columeNum+1) * size + xZhou, i * size + yZhou, 0);
                    game.transform.DOMove(new Vector3(j * size + xZhou, i * size + yZhou, 0), 0.3f);
                    //设置他的大小
                    game.transform.localScale = new Vector3(size, size, size);
                    //将名为 Mark 的组件类添加到 game 游戏对象。
                    Mark mark = game.AddComponent<Mark>();
                    mark.Column = i;
                    mark.Row = j;
                    mark.Index = n;
                    game.name = n.ToString();
                    game.AddComponent<BoxCollider2D>();
                    fruitGameObjects[i,j] = game;
                }
            }
        }
    }
}
