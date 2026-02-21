using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSwap : MonoBehaviour
{
    [Tooltip("The GameObject to enable when the player enters the trigger.")]
    public GameObject objectToEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.SetActive(false);

            if (objectToEnable != null)
                objectToEnable.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}