using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePlayer : MonoBehaviour
{
    public Sprite[] sprites;
    public float speed = 0.016f;
    public float time;
    int curIndex;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (sprites != null)
        {
            if (speed > 0)
            {
                time += Time.deltaTime;
                int i = (int)(time / speed);
                if (i != curIndex)
                {
                    if (i < sprites.Length)
                    {
                        curIndex = i;
                        SpriteRenderer sr = GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = sprites[curIndex];
                        }
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
