using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InterFaces
{
    public interface IQueueable
    {
        UniTask MoveToPosition(Vector3 target, float speed, CancellationToken token);
    }
}