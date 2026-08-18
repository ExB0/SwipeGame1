using Cysharp.Threading.Tasks;
using UnityEngine;

using InterFaces;
using Units;

namespace GameStrategies
{
    public class TakeValidator
    {
        public bool TryTake(Car car, Person person)
        {
            if (car == null ||
                person == null)
            {
                return false;
            }

            if (!HasSameColor(car, person))
            {
                return false;
            }

            if (!car.TryGetSeat(out Transform seatTransform))
            {
                return false;
            }

            person.JumpTo(
                seatTransform.position,
                seatTransform
            ).Forget();

            return true;
        }

        private bool HasSameColor(
            IColorMatchable first,
            IColorMatchable second)
        {
            if (first == null ||
                second == null)
            {
                return false;
            }

            return first.GetColor() == second.GetColor();
        }
    }
}