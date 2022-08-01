using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class o1 : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject o2;
    void Start()
    {
        o2 = GameObject.Find("o2");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x - 0.1f;
            this.transform.position = position;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x + 0.1f;
            this.transform.position = position;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Vector3 position = this.transform.position;
            position.y = position.y + 0.1f;
            this.transform.position = position;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 position = this.transform.position;
            position.y = position.y - 0.1f;
            this.transform.position = position;
        }

        Debug.Log(Rcc8Test.Rcc8(gameObject, o2));
    }
}
