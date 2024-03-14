using AdminToys;
using Mirror;
using UnityEngine;

namespace BaphometPlugin.Modules.Scp1162.Utils;

public class Scp1162Toy(
    PrimitiveType type,
    Vector3 position,
    Vector3 rotation,
    Vector3 scale,
    Color color,
    Transform parent = null,
    float alpha = 1f)
{
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable Unity.InefficientPropertyAccess
    
    public PrimitiveType Type = type;
    public Vector3 Position = position;
    public Vector3 Rotation = rotation;
    public Vector3 Scale = scale;
    public Color PrimitiveColor = color;
    public float Alpha = alpha;
    public Transform Parent = parent;

    public PrimitiveObjectToy Base;

    private PrimitiveObjectToy ToyPrefab
    {
        get
        {
            if (Base == null)
            {
                foreach (var gameObject in NetworkClient.prefabs.Values)
                    if (gameObject.TryGetComponent<PrimitiveObjectToy>(out var component))
                        Base = component;
            }

            return Base;
        }
    }

    public PrimitiveObjectToy Spawn()
    {
        var toy = Object.Instantiate(ToyPrefab);

        toy.NetworkPrimitiveType = Type;

        if (Parent != null) 
            toy.transform.parent = Parent;

        toy.transform.localPosition = Position;
        toy.transform.localEulerAngles = Rotation;
        toy.transform.localScale = Scale;

        toy.NetworkScale = Scale;
        toy.NetworkMaterialColor = PrimitiveColor * Alpha;
        toy.NetworkMovementSmoothing = 60;

        NetworkServer.Spawn(toy.gameObject);

        return toy;
    }
}