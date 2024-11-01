using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Material[] ColorMaterials;
    public GameObject[] Tets;
    public int height;
    public int width;
    public Transform[,] cells;
    public Transform selectedObject;
    private Transform middleObject;
    public float fallTime;
    private float fallTimeKeeper;
    private bool gameOver;
    private void Awake()
    {
        gameOver = false;
        cells = new Transform[height, width];
        fallTimeKeeper = fallTime;
    }
    private void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
            return;
        }
        bool canMove;
        if (selectedObject != null)
        {
            #region Fall
            if (Input.GetKey(KeyCode.Space))
                fallTimeKeeper -= Time.deltaTime * 5;
            else
                fallTimeKeeper -= Time.deltaTime;
            if (fallTimeKeeper <= 0)
            {
                canMove = ControlTet(selectedObject, Vector3.down);
                if (canMove)
                {
                    MoveTet(selectedObject, Vector3.down);
                }
                else
                {
                    DestroyLine(selectedObject);
                    selectedObject = null;
                }
                fallTimeKeeper = fallTime;
            }
            #endregion
            #region Move
            Vector3 direction = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.A))
                direction.x = -1;
            else if (Input.GetKeyDown(KeyCode.D))
                direction.x = 1;
            if (direction.x != 0)
            {
                if (ControlTet(selectedObject, direction))
                {
                    MoveTet(selectedObject, direction);
                }
            }
            #endregion
            #region Rotate
            if (Input.GetKeyDown(KeyCode.W))
            {
                Dictionary<Transform,Vector3> tempDic = new Dictionary<Transform,Vector3>();
                bool canRotate = true;
                foreach (Transform t in selectedObject)
                {
                    tempDic.Add(t, t.position);
                    cells[(int)t.position.y, (int)t.position.x] = null;
                }
                foreach (Transform t in selectedObject)
                {
                    if (t != middleObject)
                    {
                        Vector3 dif = t.position - middleObject.position;
                        Vector3 newpos = Vector3.zero;
                        newpos.x = middleObject.position.x + dif.y;
                        newpos.y = middleObject.position.y - dif.x;
                        if (ControlPosition(newpos))
                        {
                            if (cells[(int)newpos.y, (int)newpos.x] == null)
                            {
                                t.position = newpos;
                                cells[(int)t.position.y, (int)t.position.x] = t;
                            }
                            else
                            {
                                canRotate = false;
                                break;
                            }
                        }
                        else
                        {
                            canRotate = false;
                            break;
                        }
                    }
                }
                if (!canRotate)
                {
                    foreach (Transform t in tempDic.Keys)
                    {
                        cells[(int)t.position.y, (int)t.position.x] = null;
                        t.position = tempDic[t];
                        cells[(int)t.position.y, (int)t.position.x] = t;
                    }
                    tempDic.Clear();
                }
            }
            #endregion
        }
        else
        {
            CreateTet();
        }
    }
    void DestroyLine(Transform tet)
    {

        List<int> line = new List<int>();
        foreach (Transform t in tet)
        {
            int n = (int)t.position.y;
            bool destroy = true;
            for (int i = 0; i < width; i++)
            {
                if (cells[n, i] == null)
                {
                    destroy = false; break;
                }
            }
            if (destroy)
            {
                if (!line.Contains(n))
                    line.Add(n);
            }
        }
        line.Sort();
        line.Reverse();
        foreach (int i in line)
        {
            for (int j = 0; j < width; j++)
            {
                Destroy(cells[i, j].gameObject);
                cells[i, j] = null;
            }
            for (int x = i + 1; x < height; x++)
            {
                for (int a = 0; a < width; a++)
                {
                    if (cells[x, a] != null)
                    {
                        Transform temp = cells[x, a];
                        cells[x, a] = null;
                        cells[x - 1, a] = temp;
                        cells[x - 1, a].position += Vector3.down;
                    }
                }
            }
        }
    }
    void CreateTet()
    {
        selectedObject = Instantiate(Tets[GetRandomNumber(Tets.Length)], transform.position, Quaternion.identity).transform;
        foreach (Transform tet in selectedObject)
        {
            if (ControlCube(tet, Vector3.zero))
            {
                cells[(int)tet.position.y, (int)tet.position.x] = tet;
            }
            else
            {
                GameOver();
                break;
            }
        }
        middleObject = selectedObject.GetComponent<Tet>().midObject;
    }
    void MoveTet(Transform tet, Vector3 vec)
    {
        foreach (Transform t in tet.transform)
        {
            cells[(int)t.position.y, (int)t.position.x] = null;
            t.position += vec;
            cells[(int)t.position.y, (int)t.position.x] = t;
        }

    }
    int GetRandomNumber(int value) => Random.Range(0, value);
    bool ControlTet(Transform tet, Vector3 direction)
    {
        bool move = false;
        foreach (Transform t in tet)
        {
            cells[(int)t.position.y, (int)t.position.x] = null;
        }
        foreach (Transform t in tet)
        {
            if (ControlCube(t, direction))
                move = true;
            else
            {
                move = false;
                break;
            }
        }
        foreach (Transform t in tet)
        {
            cells[(int)t.position.y, (int)t.position.x] = t;
        }
        return move;
    }
    bool ControlCube(Transform tet, Vector3 vec)
    {
        if (ControlPosition(tet.position+vec))
        {
            return cells[(int)(tet.position.y + vec.y), (int)(tet.position.x + vec.x)] == null;
        }
        return false;
    }
    bool ControlPosition(Vector3 pos)
    {
        return pos.y>-1 && pos.y < height && pos.x>-1 && pos.x<width;
    }
    void GameOver()
    {
        Debug.Log("GAME OVER");
        gameOver = true;
    }
}
