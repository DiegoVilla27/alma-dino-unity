using System;
using UnityEngine;
namespace AlmaDino.Core.Interfaces
{
    public interface IRoarEmitter2D { event Action<Vector2, Vector2> OnRoared; }
}
