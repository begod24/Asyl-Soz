using UnityEngine;

/// <summary>
/// Interface for any object that can receive damage.
/// Decouples the damage system from a specific health implementation,
/// allowing future enemies, destructible objects, or shields to reuse
/// the same DamageOnTouch2D component without changes.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
