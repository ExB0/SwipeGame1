using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMatchStrategy : ITakeStrategy
{
    private static ColorMatchStrategy _instance;
    public static ColorMatchStrategy Instance => _instance ??= new ColorMatchStrategy();

    private ColorMatchStrategy() { }

    public bool TryTake(TakeContext ctx)
    {
        if (ctx.CarColor.GetColor() == ctx.PersonColor.GetColor())
        {
            if (!ctx.Car.TryGetSeat(out var seatTransform))
            {
                return false;
            }

            ctx.PersonJump.JumpTo(seatTransform.position, seatTransform);
            return true;
        }
        return false;
    }


}