using UnityEngine;

public class FluteController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on FluteController GameObject.");
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayfluteAnimation();




        }



        if (Input.GetKeyDown(KeyCode.Quote))
        {


            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayingFlute"))
            {
                PlayfluteAnimation();
            }

        }


    }


    public void PlayfluteAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("PlayingFlute");
            Debug.Log("Flute animation triggered.");
        }
    }




}


