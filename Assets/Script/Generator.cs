using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Generator : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public GameObject[] exitPrefabs;
    public GameObject[] blockedPrefabs;
    public GameObject[] doorPrefabs;
    public GameObject[] roomPrefabs;
    public GameObject[] hallwayPrefabs;

    [Header("Debugging Options")]
    public bool useBoxColliders;

    [Header("Key Bindings")]
    public KeyCode reloadKey = KeyCode.Backspace;
    public KeyCode toggleMapKey = KeyCode.M;

    [Header("Generation Limits")]
    [Range(0, 50)] public int branchLength = 5;
    [Range(0, 100)] public int doorPercent = 25;
    [Range(0, 1f)] public float constructionDelay;
    [Range(2, 100)] public int numberOfRooms = 5;

    [Header("Available at Runtime")]
    public List<Tile> generatedTiles = new List<Tile>();
    public List<Tile> generatedRooms = new List<Tile>();

    List<Connector> availableConnectors = new List<Connector>();
    Color startLightColor = Color.white;
    Transform tileFrom, tileTo, tileRoot;
    Transform container;

    int attempts;

    private void Start()
    {
        StartCoroutine(DungeonBuild());
    }

    void Update()
    {
        if(Input.GetKeyDown(reloadKey))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    IEnumerator DungeonBuild()
    {
        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        tileRoot = CreateStartTile();
        tileTo = tileRoot;

        //while (generatedTiles.Count(x => x.tile.name.Contains("Room")) < numberOfRooms){

        //}

        int numberOfRoomsOnMain = Mathf.CeilToInt((float) numberOfRooms/2);

        while(generatedTiles.Count(x => x.tile.name.Contains("Room")) < numberOfRoomsOnMain)
        {
            yield return new WaitForSeconds(constructionDelay);
            tileFrom = tileTo;

            if (tileFrom.name.Contains("Room"))
            {
                tileTo = CreateHallwayTile();
                ConnectTiles();
                CollisionCheck();
            }
            else
            {
                tileTo = CreateTile();
                ConnectTiles();
                CollisionCheck();
            }
        }

        foreach (Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected)
            {
                if (!availableConnectors.Contains(connector))
                {
                    availableConnectors.Add(connector);
                }
            }
        }

        int branchCount = 0;
        while (generatedTiles.Count(x => x.tile.name.Contains("Room")) < numberOfRooms)
        {
            bool roomGenerated = false;
            if (availableConnectors.Count > 0)
            {
                goContainer = new GameObject("Branch " + (branchCount + 1));
                container = goContainer.transform;
                container.SetParent(transform);
                int availIndex = Random.Range(0, availableConnectors.Count);
                tileRoot = availableConnectors[availIndex].transform.parent.parent;
                availableConnectors.RemoveAt(availIndex);
                tileTo = tileRoot;

                while (!roomGenerated)
                {
                    yield return new WaitForSeconds(constructionDelay);
                    tileFrom = tileTo;
                    if (tileFrom.name.Contains("Room"))
                    {
                        tileTo = CreateHallwayTile();
                        ConnectTiles();
                        CollisionCheck();
                    }
                    else
                    {
                        tileTo = CreateTile();
                        if (tileTo.name.Contains("Room"))
                        {
                            roomGenerated= true;
                            Debug.Log("Room Generated");
                        }
                        ConnectTiles();
                        CollisionCheck();
                    }
                }
                //for (int i = 0; i < branchLength - 1; i++)
                //{
                //    yield return new WaitForSeconds(constructionDelay);
                //    tileFrom = tileTo;
                //    tileTo = CreateTile();
                //    ConnectTiles();
                //    CollisionCheck();
                //}
            }
            else { break; }
        }

        CleanupBoxes();
        CleanupUnusedHallway();    
    }

    void CollisionCheck()
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }

        Vector3 offset = (tileTo.right * box.center.x) + (tileTo.up * box.center.y) + (tileTo.forward * box.center.z);
        Vector3 halfExtents = box.bounds.extents;
        List<Collider> hits = Physics.OverlapBox(tileTo.position + offset, halfExtents, Quaternion.identity, LayerMask.GetMask("Tile")).ToList();
        if (hits.Count > 0)
        {
            if (hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                // hit something (other than tileFrom or tileTo)
                attempts++;
                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);
                if (generatedTiles[toIndex].connector != null)
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }
                generatedTiles.RemoveAt(toIndex);
                DestroyImmediate(tileTo.gameObject);
                // backtracking

                if(attempts > 50)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);
                    Tile myTileFrom = generatedTiles[fromIndex];
                    if(tileFrom != tileRoot)
                    {
                        if(myTileFrom.connector != null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }
                        availableConnectors.RemoveAll(x => x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);
                        DestroyImmediate(tileFrom.gameObject);
                        if (myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;
                        }
                        else if (container.name.Contains("Main"))
                        {
                            if (myTileFrom.origin != null)
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if (availableConnectors.Count > 0)
                        {
                            int availIndex = Random.Range(0, availableConnectors.Count);
                            tileRoot = availableConnectors[availIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availIndex);
                            tileFrom = tileRoot;
                        }
                        else { return; }
                    }
                    else if (container.name.Contains("Main"))
                    {
                        if(myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom= tileRoot;
                        }
                    }
                    else if(availableConnectors.Count > 0) {
                        int availIndex = Random.Range(0, availableConnectors.Count);
                        tileRoot = availableConnectors[availIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(availIndex);
                        tileFrom= tileRoot;
                     }
                    else { return; }
                }
                if (tileFrom != null)
                {
                    if (tileFrom.name.Contains("Room"))
                    {
                        tileTo = CreateHallwayTile();
                        ConnectTiles();
                        CollisionCheck();
                    }
                    else
                    {
                        tileTo = CreateTile();
                        ConnectTiles();
                        CollisionCheck();
                    }
                }
            }
            else { attempts = 0; } // nothing other than tileTo and tileFrom was hit (so restore attempts back to zero)
        }
    }


    void CleanupBoxes()
    {
        if (!useBoxColliders)
        {
            foreach(Tile myTile in generatedTiles)
            {
                BoxCollider box = myTile.tile.GetComponent<BoxCollider>();
                if(box != null) { Destroy(box); }
            }
        }
    }

    void CleanupUnusedHallway()
    {
        List<Transform> branchesToDelete = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.Contains("Branch"))
            {
                bool hasRoom = false;

                foreach (Transform subChild in child)
                {
                    if (subChild.name.Contains("Room"))
                    {
                        hasRoom = true;
                        break;
                    }
                }

                if (!hasRoom)
                {
                    branchesToDelete.Add(child);
                }
            }
        }

        foreach (Transform branch in branchesToDelete)
        {
            Debug.Log("Deleting unused branch: " + branch.name);
            DestroyImmediate(branch.gameObject);
        }
    }




    void ConnectTiles()
    {
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (connectFrom == null)
        {
            return;
        }
        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null)
        {
            return;
        }
        connectTo.SetParent(connectFrom);
        tileTo.SetParent(connectTo);
        connectTo.localPosition = Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0, 180f, 0);
        tileTo.SetParent(container);
        connectTo.SetParent(tileTo.Find("Connectors"));
        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();

    }

    Transform GetRandomConnector(Transform tile)
    {
        if (tile == null)
        {
            return null;
        }

        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList().FindAll(x => x.isConnected == false);
        if (connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;
            if (tile == tileFrom)
            {
                BoxCollider box = tile.GetComponent<BoxCollider>();
                if (box == null)
                {

                    box = tile.gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                }
            }
            return connectorList[connectorIndex].transform;
        }
        return null;
    }

    Transform CreateTile()
    {
        int index = Random.Range(0, tilePrefabs.Length);
        GameObject goTile = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = tilePrefabs[index].name;
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));
        return goTile.transform;
    }

    Transform CreateHallwayTile()
    {
        int index = Random.Range(0, hallwayPrefabs.Length);
        GameObject goTile = Instantiate(hallwayPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = hallwayPrefabs[index].name;
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));
        return goTile.transform;
    }

    Transform CreateRoomTile()
    {
        int index = Random.Range(0, roomPrefabs.Length);
        GameObject goTile = Instantiate(roomPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = roomPrefabs[index].name;
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));
        return goTile.transform;
    }

    Transform CreateStartTile()
    {
        int index = Random.Range(0, roomPrefabs.Length);
        GameObject goTile = Instantiate(roomPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = "Start Room";
        float yRot = Random.Range(0, 4) * 90f;
        goTile.transform.Rotate(0, yRot, 0);
        generatedTiles.Add(new Tile(goTile.transform, null));
        return goTile.transform;
    }
}
