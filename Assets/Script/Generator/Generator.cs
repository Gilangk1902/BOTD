using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
    public GameObject[] exitPrefabs;
    public GameObject[] blockedPrefabs;
    public GameObject[] doorPrefabs;
    public GameObject[] hallwayPrefabs;
    public RoomScriptable[] _roomPrefabs;
    public GameObject[] chestPrefab;
    public GameObject characterPrefab;

    [Header("Debugging Options")]
    public bool useBoxColliders;

    [Header("Key Bindings")]
    public KeyCode reloadKey = KeyCode.Backspace;
    public KeyCode toggleMapKey = KeyCode.M;

    [Header("Generation Limits")]
    [Range(0, 1f)] public float constructionDelay;
    [Range(2, 100)] public int numberOfRooms = 10;

    [Header("Available at Runtime")]
    public List<Tile> generatedTiles = new List<Tile>();

    List<Connector> availableConnectors = new List<Connector>();
    Transform tileFrom, tileTo, tileRoot;
    Transform container;

    int attempts;

    public NavMeshSurface navMeshSurface;

    public enum DungeonGeneratorState
    {
        Inactive,
        Generating,
        Finished
    }

    [SerializeField] public DungeonGeneratorState dungeonGeneratorState = DungeonGeneratorState.Inactive;

    private void Start()
    {
        StartCoroutine(DungeonBuild());
    }

    IEnumerator DungeonBuild()
    {
        dungeonGeneratorState = DungeonGeneratorState.Generating;
        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        tileRoot = CreateStartTile();
        tileTo = tileRoot;

        int numberOfRoomsOnMain = Mathf.CeilToInt((float) numberOfRooms * 0.6f);

        while(generatedTiles.Count(x => x.tile.name.Contains("Room")) <= numberOfRoomsOnMain)
        {
            yield return new WaitForSeconds(constructionDelay);
            tileFrom = tileTo;

            if (generatedTiles.Count(x => x.tile.name.Contains("Room")) == numberOfRoomsOnMain)
            {
                tileTo = CreateExitTile();
                ConnectTiles();
                CollisionCheck();
            }
            else if (tileFrom.name.Contains("Room"))
            {
                tileTo = CreateHallwayTile();
                ConnectTiles();
                CollisionCheck();
            }
            else
            {
                tileTo = CreateRoomTile();
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

                Branch branchScript = goContainer.AddComponent<Branch>();
                branchScript.Init(availableConnectors[availIndex]);
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
                        tileTo = CreateRoomTile();
                        if (tileTo.name.Contains("Room"))
                        {
                            roomGenerated = true;
                        }
                        ConnectTiles();
                        CollisionCheck();
                    }
                }
            }
            else { break; }
        }

        CleanupBoxes();
        CleanupUnusedHallway();
        BlockUnusedConnector();
        PlaceDoorsOnConnectedRoomConnectors();

        dungeonGeneratorState = DungeonGeneratorState.Finished;

        if(dungeonGeneratorState == DungeonGeneratorState.Finished)
        {
            navMeshSurface.BuildNavMesh();
            SpawnChest();
            SpawnPlayer();
        }
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
                        int index = Random.Range(0, 1);
                        if(index == 0)
                        {
                            tileTo = CreateHallwayTile();
                        }
                        else
                        {
                            tileTo = CreateRoomTile();
                        }
                        ConnectTiles();
                        CollisionCheck();
                    }
                }
            }
            else { attempts = 0; }
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

    Transform CreateHallwayTile()
    {
        int index = Random.Range(0, hallwayPrefabs.Length);
        GameObject goTile = Instantiate(hallwayPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = hallwayPrefabs[index].name;
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    Transform CreateExitTile()
    {
        GameObject goTile = Instantiate(exitPrefabs[0], Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = exitPrefabs[0].name + "_Exit";
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    Transform CreateRoomTile()
    {
        //int index = Random.Range(0, roomPrefabs.Length);
        int index = GetWeightedRandomRoom();


        GameObject goTile = Instantiate(_roomPrefabs[index].prefab, Vector3.zero, Quaternion.identity, container) as GameObject;
        goTile.name = _roomPrefabs[index].name;
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(goTile.transform, origin));

        return goTile.transform;
    }

    Transform CreateStartTile()
    {
        int index = GetWeightedRandomRoom();

        GameObject goTile = Instantiate(_roomPrefabs[index].prefab, Vector3.zero, Quaternion.identity, container);
        goTile.name = _roomPrefabs[index].prefab.name + "_Start";
        float yRot = Random.Range(0, 4) * 90f;
        goTile.transform.Rotate(0, yRot, 0);
        generatedTiles.Add(new Tile(goTile.transform, null));

        return goTile.transform;
    }

    void BlockUnusedConnector()
    {
        List<Connector> availableConnectors2= new List<Connector>();
        foreach (Connector connector in transform.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected)
            {
                if (!availableConnectors2.Contains(connector))
                {
                    availableConnectors2.Add(connector);
                }
            }
        }
        for (int i = 0; i < availableConnectors2.Count; i++)
        {
            GameObject door = Instantiate(blockedPrefabs[0], Vector3.zero, Quaternion.identity, transform) as GameObject;
            door.name = blockedPrefabs[0].name;
            Transform origin = availableConnectors2[i].transform;

            door.transform.SetParent(availableConnectors2[i].transform);
            door.transform.localPosition = Vector3.zero;
            door.transform.localRotation = Quaternion.identity;
        }
    }

    int GetWeightedRandomRoom()
    {
        float totalWeight = 0f;
        foreach (var wp in _roomPrefabs)
            totalWeight += wp.weight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < _roomPrefabs.Count(); i++)
        {
            currentSum += _roomPrefabs[i].weight;
            if (randomValue <= currentSum)
                return i;
        }

        return 0; // fallback
    }

    void SpawnChest()
    {
        foreach (Tile tile in generatedTiles)
        {
            if (tile.tile == null) continue;

            if (!tile.tile.name.Contains("Room")) continue;

            if (tile.tile.name.ToLower().Contains("start"))
            {
                Transform[] spawnPoints = tile.tile.GetComponentsInChildren<Transform>()
                    .Where(t => t.name.Equals("ChestSpawnPoint")).ToArray();

                if (spawnPoints.Length == 0)
                {
                    Debug.LogWarning("No chest spawn points in: " + tile.tile.name);
                    continue;
                }

                Transform chosen = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject chest = Instantiate(chestPrefab[0], chosen.position, chosen.rotation, tile.tile);

                chest.transform.SetParent(chosen.transform);
            }
            else
            {
                Transform[] spawnPoints = tile.tile.GetComponentsInChildren<Transform>()
                    .Where(t => t.name.Equals("ChestSpawnPoint")).ToArray();

                if (spawnPoints.Length == 0)
                {
                    continue;
                }

                Transform chosen = spawnPoints[Random.Range(0, spawnPoints.Length)];
                int availIndex = Random.Range(0, chestPrefab.Length);
                GameObject chest = Instantiate(chestPrefab[availIndex], chosen.position, chosen.rotation, tile.tile);

                chest.transform.SetParent(chosen.transform);
            }

        }
    }


    void PlaceDoorsOnConnectedRoomConnectors()
    {
        foreach (Tile tile in generatedTiles)
        {
            if (tile.tile == null) continue;

            if (!tile.tile.name.Contains("Room")) continue;

            Connector[] connectors = tile.tile.GetComponentsInChildren<Connector>();

            foreach (Connector connector in connectors)
            {
                if (connector.isConnected)
                {
                    GameObject door = Instantiate(doorPrefabs[0], Vector3.zero, Quaternion.identity);
                    door.name = doorPrefabs[0].name;
                    door.transform.SetParent(connector.transform);
                    door.transform.localPosition = Vector3.zero;
                    door.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    void CleanupUnusedHallway()
    {
        //List<Transform> branchesToDelete = new List<Transform>();

        //foreach (Transform child in transform)
        //{
        //    if (child.name.Contains("Branch"))
        //    {
        //        bool hasRoom = false;

        //        foreach (Transform subChild in child)
        //        {
        //            if (subChild.name.Contains("Room"))
        //            {
        //                hasRoom = true;
        //                break;
        //            }
        //        }

        //        if (!hasRoom)
        //        {
        //            branchesToDelete.Add(child);
        //        }
        //    }
        //}

        //foreach (Transform branch in branchesToDelete)
        //{
        //    Debug.Log("Deleting unused branch: " + branch.name);
        //    DestroyImmediate(branch.gameObject);
        //}
    }

    void SpawnPlayer()
    {
        if (characterPrefab == null)
        {
            return;
        }

        // Spawn karakter
        Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
    }



}
