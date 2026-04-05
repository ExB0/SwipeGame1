using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IQueueable
{
    public UniTask MoveToPosition(Vector3 target, float speed);
}
