using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Кого сажаем")]
    public Customer[] customerPrefabs;

    [Header("Места за стойкой")]
    public BarSeat[] seats;

    [Header("Рецепты, из которых берётся заказ")]
    public RecipeData[] availableRecipes;

    [Header("Тайминг, секунд")]
    public float minInterval = 15f;
    public float maxInterval = 30f;
    public bool spawnFirstImmediately = true;

    private float _timer;

    private void Start()
    {
        _timer = spawnFirstImmediately ? 1.5f : Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0) return;
        if (availableRecipes == null || availableRecipes.Length == 0) return;

        BarSeat seat = GetFreeSeat();
        if (seat == null) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        Spawn(seat);
        _timer = Random.Range(minInterval, maxInterval);
    }

    private BarSeat GetFreeSeat()
    {
        if (seats == null) return null;

        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] != null && seats[i].IsFree) return seats[i];
        }

        return null;
    }

    private void Spawn(BarSeat seat)
    {
        Customer prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        if (prefab == null) return;

        RecipeData recipe = availableRecipes[Random.Range(0, availableRecipes.Length)];

        Customer customer = Instantiate(prefab, seat.Position, seat.Rotation);
        customer.Seat = seat;
        customer.order = recipe;
        customer.name = "Customer_" + (recipe != null ? recipe.drinkName : "none");

        seat.Occupant = customer;
    }
}
