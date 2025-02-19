using UnityEngine;

public class CartController : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 touchStartPos;
    private Vector3 cartStartPos;

    void Update()
    {
        // **TOUCH MOVEMENT**
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = Camera.main.ScreenToWorldPoint(touch.position);
                cartStartPos = transform.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                float deltaX = touchPos.x - touchStartPos.x;
                transform.position = new Vector3(cartStartPos.x + deltaX, transform.position.y, transform.position.z);
            }
        }

        // **MOUSE MOVEMENT (PER TESTARE SU PC)**
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cartStartPos = transform.position;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float deltaX = mousePos.x - touchStartPos.x;
            transform.position = new Vector3(cartStartPos.x + deltaX, transform.position.y, transform.position.z);
        }
    }
}
