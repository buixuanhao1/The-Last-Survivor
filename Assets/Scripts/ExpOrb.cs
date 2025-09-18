using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    private int expAmount;

    public void SetExpAmount(int amount)
    {
        expAmount = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExperience exp = other.GetComponent<PlayerExperience>();
            if (exp != null)
            {
                exp.AddExp(expAmount);
            }
            Destroy(gameObject);
        }
    }
}
