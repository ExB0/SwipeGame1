using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IQueueable
{
    UniTask MoveToPosition(Vector3 target, float speed, CancellationToken token);
}