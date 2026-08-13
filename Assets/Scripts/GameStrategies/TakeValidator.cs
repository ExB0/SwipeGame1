using Cysharp.Threading.Tasks;
using UnityEngine;

using Units;

namespace GameStrategies
{
    public class TakeValidator
    {
        public bool TryTake(Car car, Person person)
        {
            if (car == null || person == null)
            {
                return false;
            }

            if (car.GetColor() != person.GetColor())
            {
                return false;
            }

            if (!car.TryGetSeat(out Transform seatTransform))
            {
                return false;
            }

            person.JumpTo(seatTransform.position, seatTransform).Forget();

            return true;
        }
    }
}