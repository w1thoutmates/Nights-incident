using UnityEngine;

public class DoorMechanic_CamInteract : MonoBehaviour
{

    public bool CanInteract = true;




    // Update is called once per frame
    void Update()
    {


        if (Input.GetKeyDown(KeyCode.E))
        {
            // check what we interacted with 
            CheckInteraction();
        }


    }





    private void CheckInteraction()
    {


        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 5f))
        {

            if (hit.collider.CompareTag("Door"))
            {

                if(hit.collider.gameObject.GetComponent<SimpleDoor>() != null)
                {

                    hit.collider.gameObject.GetComponent<SimpleDoor>().ToggleDoorState();

                }

            }


        }


    }


}
