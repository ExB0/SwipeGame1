using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IJumpable
{
    UniTask JumpTo(Vector3 position, Transform parentTransform);
}
