using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public int startGold = 1000;
    public int Gold { get; private set; }

    public event Action<int> OnGoldChanged;

    void Awake()
    {
        Gold = startGold;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool Spend(int amount)
    {
        if (Gold < amount)
            return false;

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
