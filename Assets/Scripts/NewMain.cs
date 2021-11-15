using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NewMain : MonoBehaviour
{
    public GameObject[] prefabs;
    public GameObject[] effects;
    public GameObject[,] objects;
    public int Column;
    public int Row;
    //游戏对象缩放比例
    public float Size=1;
    public float startTime = 1;
    List<GameObject> cache = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        objects = new GameObject[Column, Row];
        int max = prefabs.Length;
        //列数乘以-0.5得到一个在x轴上最左边的值 +后面的属于偏移量
        float sx = Column * -0.5f * Size + 0.5f * Size;
        float sy = Row * -0.5f * Size + 0.5f * Size;
        for (int i = 0; i < Column; i++)
        {
            for (int j = 0; j < Row; j++)
            {
                int index = Random.Range(0, max);
                GameObject go = GameObject.Instantiate(prefabs[index]);
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(i * Size + sx, j * Size + sy, 0);
                go.transform.localScale = new Vector3(Size, Size, Size);
                go.AddComponent<BoxCollider2D>();
                //将名为 Mark 的组件类添加到 game 游戏对象。
                Mark mark = go.AddComponent<Mark>();
                mark.Column = i;
                mark.Row = j;
                mark.Index = index;
                go.name = index.ToString();
                objects[i, j] = go;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(startTime>0)
        {
            startTime -= Time.deltaTime;
        }
        if(startTime<0)
        {
            //按下鼠标 0：左键
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                if(hit.transform!=null)
                {
                    lastCol = curCol;
                    lastRow = curRow;
                    //GetCompoment<T>()从当前游戏对象获取组件T，只在当前游戏对象中获取，没得到的就返回null，不会去子物体中去寻找。
                    Mark mark = hit.transform.GetComponent<Mark>();
                    curCol = mark.Column;
                    curRow = mark.Row;
                    cache.Clear();
                    Check(hit.transform.name, mark.Column, mark.Row);
                    Debug.Log($"{nameof(NewMain)}: 缓存里的对象数量：{cache.Count}");
                    if(cache.Count>=3)
                    {
                        //销毁
                        PlayDestory();
                    }
                    else
                    {
                        cache.Clear();
                        Swap();
                    }
                }
            }
        }
    }

    void Check(string type,int column,int row)
    {
        if(column<0||column>=Column||row<0||row>=Row)
        {
            return;
        }
        GameObject go = objects[column, row];
        if(go.name!=type)
        {
            return;
        }
        if(cache.Contains(go))
        {
            return;
        }
        cache.Add(go);
        Check(type, column - 1, row);
        Check(type, column + 1, row);
        Check(type, column, row - 1);
        Check(type, column, row + 1);
    }

    void PlayDestory()
    {
        float sx = Column * -0.5f * Size + 0.5f * Size;
        float sy = Row * -0.5f * Size + 0.5f * Size;
        for (int i=0;i<cache.Count;i++)
        {
            Mark mark = cache[i].GetComponent<Mark>();
            objects[mark.Column, mark.Row] = null;
            GameObject go = GameObject.Instantiate(effects[mark.Index]);
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(mark.Column * Size + sx, mark.Row * Size + sy, 0);
            go.transform.localScale = new Vector3(Size, Size, Size);
            GameObject.Destroy(cache[i]);
            //Debug.Log($"{nameof(NewMain)}: 要生成的动画是{go},他的父对象是{transform},他的位置是{go.transform.position}");
        }
        StartCoroutine(Wait(0.6f, Fall));
    }

    IEnumerator Wait(float time,System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    void Fall()
    {
        float sx = Column * -0.5f * Size + 0.5f * Size;
        float sy = Row * -0.5f * Size + 0.5f * Size;
        for (int i = 0; i < Column; i++)
        {
            int count = 0;
            for (int j = 0; j < Row; j++)
            {
                if(objects[i,j]==null)
                {
                    count++;
                }
                else
                {
                    if(count>0)
                    {
                        Mark m = objects[i, j].GetComponent<Mark>();
                        int row = j - count;
                        m.Row = row;
                        objects[i, row] = objects[i, j];
                        objects[i, row].transform.DOMove(new Vector3(i * Size + sx, row * Size + sy, 0), 0.3f);
                        objects[i, j] = null;
                    }
                }
            }
        }
        Add();
    }
    
    void Add()
    {
        float sx = Column * -0.5f * Size + 0.5f * Size;
        float sy = Row * -0.5f * Size + 0.5f * Size;
        for (int i = 0; i < Column; i++)
        {
            for (int j = 0; j < Row; j++)
            {
                if(objects[i,j]==null)
                {
                    int max = prefabs.Length;
                    int index = Random.Range(0, max);
                    GameObject go = GameObject.Instantiate(prefabs[index]);
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(i * Size + sx, (Row + 1) * Size + sy, 0);
                    go.transform.DOMove(new Vector3(i * Size + sx, j * Size + sy, 0), 0.3f);
                    go.transform.localScale = new Vector3(Size, Size, Size);
                    go.AddComponent<BoxCollider2D>();
                    //将名为 Mark 的组件类添加到 game 游戏对象。
                    Mark mark = go.AddComponent<Mark>();
                    mark.Column = i;
                    mark.Row = j;
                    mark.Index = index;
                    go.name = index.ToString();
                    objects[i, j] = go;
                }
            }
        }
    }

    int curCol, curRow;
    int lastCol, lastRow;

    void Swap()
    {
        int ox = curCol - lastCol;
        int oy = curRow - lastRow;
        if(oy==0)
        {
            if(ox==1||ox==-1)
            {
                SwapTest();
            }
        }
        else if(ox==0)
        {
            if(oy==1||oy==-1)
            {
                SwapTest();
            }
        }
    }

    void SwapTest()
    {
        bool fail = true;
        GameObject a = objects[curCol, curRow];
        GameObject b = objects[lastCol, lastRow];
        objects[curCol, curRow] = b;
        objects[lastCol, lastRow] = a;
        var mb = b.GetComponent<Mark>();
        mb.Column = curCol;
        mb.Row = curRow;
        var ma = a.GetComponent<Mark>();
        ma.Column = lastCol;
        ma.Row = lastRow;
        int cc = curCol;
        int cr = curRow;
        int lc = lastCol;
        int lr = lastRow;
        a.transform.DOMove(b.transform.position, 0.3f);
        b.transform.DOMove(a.transform.position, 0.3f).onComplete = () =>
           {
               cache.Clear();
               Check(b.name, cc, cr);
               if (cache.Count >= 3)
               {
                   fail = false;
                   PlayDestory();
               }
               cache.Clear();
               Check(a.name, lc, lr);
               if (cache.Count >= 3)
               {
                   fail = false;
                   PlayDestory();
               }
               cache.Clear();
               if (fail)
               {
                   objects[cc, cr] = a;
                   objects[lc, lr] = b;
                   a.transform.DOMove(b.transform.position, 0.3f);
                   b.transform.DOMove(a.transform.position, 0.3f);
                   ma.Column = cc;
                   ma.Row = cr;
                   mb.Column = lc;
                   mb.Row = lr;
               }
           };
    }
}
