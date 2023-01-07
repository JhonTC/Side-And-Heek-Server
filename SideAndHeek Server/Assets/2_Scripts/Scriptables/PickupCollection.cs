using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new pickup collection", menuName = "Data/Pickup Collection")]
public class PickupCollection : ScriptableObject
{
    public List<PickupDetails> pickupDetails;

    public PickupDetails GetPickupByCode(PickupType code)
    {
        foreach (PickupDetails pickupDetails in pickupDetails)
        {
            if (pickupDetails.pickupSO.pickupCode == code)
            {
                return pickupDetails;
            }
        }

        throw new System.Exception($"ERROR: No task exists with code {code}.");
    }


    public PickupDetails GetRandomItem()
    {
        return pickupDetails[Random.Range(0, pickupDetails.Count)];
    }
}

[System.Serializable]
public class PickupDetails
{
    public PickupSO pickupSO;
    public int numberOfUses;
}




