using UnityEngine;

using InterFaces;

namespace Units
{
    [RequireComponent(typeof(Car))]
    public class CarClickHandler : MonoBehaviour, IClickable
    {
        [SerializeField] private Car _car;

        private void Awake()
        {
            if (_car == null)
            {
                _car = GetComponent<Car>();
            }
        }

        public void OnClick()
        {
            _car?.StartMove();
        }
    }
}