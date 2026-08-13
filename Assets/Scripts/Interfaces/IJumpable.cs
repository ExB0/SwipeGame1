using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InterFaces
{
    public interface IJumpable
    {
        UniTask JumpTo(Vector3 position, Transform parentTransform);
    }
}
