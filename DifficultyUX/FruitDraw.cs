using osu.Game.Rulesets.Catch.Difficulty;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DifficultyUX
{
    class FruitDraw
    {
        private Ellipse fruit;

        private double strainValue;
        private double fruitNumber { get; }

        public FruitDraw(Ellipse shape, double strain, double number)
        {
            fruit = shape;
            strainValue = strain;
            fruitNumber = number;
        }

        public double getX()
        {
            return Canvas.GetLeft(fruit);
        }

        public double getY()
        {
            return Canvas.GetBottom(fruit);
        }
    }
}
