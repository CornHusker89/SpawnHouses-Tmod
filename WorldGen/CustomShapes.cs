using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;
using System;
using ReLogic.Utilities;
using Terraria;

namespace SpawnHouses.WorldGen;

public static class CustomShapes
{
    public class ReverseMound : GenShape
    {
        private int _halfWidth;
        private int _height;

        public ReverseMound(int halfWidth, int height)
        {
            this._halfWidth = halfWidth;
            this._height = height;
        }

        public override bool Perform(Point origin, GenAction action)
        {
            _ = this._height;
            double num = this._halfWidth;
            for (int i = -this._halfWidth; i <= this._halfWidth; i++)
            {
                int num2 = Math.Min(this._height, (int)((0.0 - (double)(this._height + 1) / (num * num)) * ((double)i + num) * ((double)i - num)));
                for (int j = 0; j < num2; j++)
                {
                    if (!base.UnitApply(action, origin, i + origin.X, origin.Y + j) && base._quitOnFail)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

