using UnityEngine;
using System.Collections;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SpriteRenderer playerRenderer = collision.gameObject.GetComponent<SpriteRenderer>();

            if (playerRenderer.sortingOrder == 15)
            {
                foreach (Collider2D mountain in mountainColliders)
                {
                    mountain.enabled = true;
                }

                foreach (Collider2D boundary in boundaryColliders)
                {
                    boundary.enabled = false;
                }

                StartCoroutine(CambiarSortingOrderDespuesDeRetraso(playerRenderer, 0.3f));
            }
        }
    }

    private IEnumerator CambiarSortingOrderDespuesDeRetraso(SpriteRenderer spriteRenderer, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        spriteRenderer.sortingOrder = 5;
    }
}
