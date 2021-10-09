using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


[CreateAssetMenu(menuName = "Utility/BioroidList")]
public class BioroidList : ScriptableObject
{
    [SerializeField] List<BioroidInformation> bioroids;

    public IList<BioroidInformation> Bioroids => bioroids.AsReadOnly();
}
