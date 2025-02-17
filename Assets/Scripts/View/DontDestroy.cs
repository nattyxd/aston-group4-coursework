using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
//@author: Ann Tomi
public class DontDestroy : MonoBehaviour {

    void Awake()
	{
        
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("music");
       // GameObject[] objs2 = GameObject.FindGameObjectsWithTag("altered");//all music objects tagged music are stored in array
        if (objs.Length > 1) //if more than one sound clip present 
        {
            Debug.Log(SceneManager.GetActiveScene().name);
            GameObject.Find("sound");
            Destroy(this.gameObject); //destroy second instance
        }
        DontDestroyOnLoad (this.gameObject); //if you only sound object in scene keep object alive

    }
		
}
