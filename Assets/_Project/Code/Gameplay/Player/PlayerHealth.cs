using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    float _currentHealth;
    [SerializeField] float _maxHealth;

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if ( _currentHealth < 0)
        {
            //temp for now
            CurrentPlayers.Instance.RemovePlayer(gameObject);
            SceneManager.LoadScene("LevelBlockOut");
            Debug.Log("Player is DEAD");
            //reload scene for prototype, handle proper multiplayer death logic later
        }
    }
    public void Awake()
    {
        _currentHealth = _maxHealth;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
