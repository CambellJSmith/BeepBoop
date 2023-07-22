using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Simon : MonoBehaviour
{
    public List<int> CompList;
    public List<int> PlayerList;
    public GameObject[] prefabs;
    public GameObject resetPrefab;
    public float resetPrefabDuration = 1f;

    private bool compMode = false;
    private bool acceptingInput = true; // Flag to control accepting player inputs
    private List<GameObject> instantiatedPrefabs = new List<GameObject>();
    private int currentIndex = 0;
    private AudioListener audioListener; // Reference to the AudioListener component

    private void Start()
    {
        audioListener = GetComponent<AudioListener>(); // Get the AudioListener component
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds

        compMode = true;
        PlayerList.Clear(); // Clear PlayerList when entering CompMode
        currentIndex = 0;
        Debug.Log("Entering CompMode");

        int randomNumber = Random.Range(1, 5);
        CompList.Add(randomNumber);

        Debug.Log("Computer chose: " + randomNumber);

        for (int i = 0; i < CompList.Count; i++)
        {
            GameObject prefabInstance = InstantiatePrefab(CompList[i]);
            instantiatedPrefabs.Add(prefabInstance);

            yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds

            Destroy(prefabInstance, 0.25f); // Destroy the prefab after 0.25 seconds
            yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds
        }

        compMode = false;
        Debug.Log("Entering PlayMode");
    }

    private GameObject InstantiatePrefab(int number)
    {
        GameObject prefabInstance = Instantiate(prefabs[number - 1]);
        prefabInstance.transform.position = prefabs[number - 1].transform.position;
        return prefabInstance;
    }

    private void Update()
    {
        if (!compMode && acceptingInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                bool isTopLeft = mousePosition.x < Screen.width / 2 && mousePosition.y > Screen.height / 2;
                bool isTopRight = mousePosition.x > Screen.width / 2 && mousePosition.y > Screen.height / 2;
                bool isBottomLeft = mousePosition.x < Screen.width / 2 && mousePosition.y < Screen.height / 2;
                bool isBottomRight = mousePosition.x > Screen.width / 2 && mousePosition.y < Screen.height / 2;

                if (isTopLeft)
                {
                    AddPlayerInput(1);
                }
                else if (isTopRight)
                {
                    AddPlayerInput(2);
                }
                else if (isBottomLeft)
                {
                    AddPlayerInput(3);
                }
                else if (isBottomRight)
                {
                    AddPlayerInput(4);
                }
            }
        }
    }

    private void AddPlayerInput(int number)
    {
        PlayerList.Add(number);
        GameObject prefabInstance = InstantiatePrefab(number);
        instantiatedPrefabs.Add(prefabInstance);

        StartCoroutine(DestroyPrefabAfterDelay(prefabInstance, 0.25f));

        if (PlayerList.Count <= CompList.Count)
        {
            bool listsMatch = true;
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i] != CompList[i])
                {
                    listsMatch = false;
                    break;
                }
            }

            if (!listsMatch)
            {
                Debug.Log("Player made a mistake! Resetting the game.");
                StartCoroutine(ResetGame());
            }
        }

        if (PlayerList.Count == CompList.Count && PlayerList.SequenceEqual(CompList))
        {
            StartCoroutine(SuccessfulClickDelay());
        }
        else if (PlayerList.Count <= CompList.Count)
        {
            currentIndex++;
        }
    }

    private IEnumerator DestroyPrefabAfterDelay(GameObject prefabInstance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(prefabInstance);
    }

    private IEnumerator ResetGame()
    {
        GameObject resetInstance = Instantiate(resetPrefab);
        acceptingInput = false; // Disable accepting player inputs during reset delay
        audioListener.enabled = false; // Disable the AudioListener

        yield return new WaitForSeconds(resetPrefabDuration);

        bool leftClickDetected = false;
        while (!leftClickDetected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                leftClickDetected = true;
                Debug.Log("Left click detected. Restarting the game...");
            }
            yield return null;
        }

        Destroy(resetInstance);

        CompList.Clear();
        PlayerList.Clear();
        instantiatedPrefabs.Clear();
        currentIndex = 0;
        acceptingInput = true; // Enable accepting player inputs after the reset delay
        audioListener.enabled = true; // Enable the AudioListener

        StartCoroutine(StartGame());
    }

    private IEnumerator SuccessfulClickDelay()
    {
        PlayerList.Clear(); // Clear PlayerList after successful click
        yield return new WaitForSeconds(1f); // Wait for 1 second
        StartCoroutine(StartGame());
    }
}
