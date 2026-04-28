using UnityEngine;

public class PlayerMoneyManager : MonoBehaviour
{
    private int _money;

    public int Money => _money;

    public void AddMoney(int amount)
    {
        _money += amount;
    }

    public void RemoveMoney(int amount)
    {
        _money = Mathf.Max(0, _money - amount);
    }
}
