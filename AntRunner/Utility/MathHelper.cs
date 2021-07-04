using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace AntRunner.Utility
{
    public class MathUtilities
    {
        // The value that is used for generating the gravity based
        // on the distance between two particle masses.
        public const float GRAVITY_CONST = 10000; 

        // Static math help functions that are to be used.
        public static double DegreeToRadian(double pAngle)
        {
            return Math.PI * pAngle / 180.0f;
        }

        public static double RadianToDegree(double pAngle)
        {
            return pAngle * (180.0f / Math.PI);
        }

        /// <summary>
        /// Returns a vector that is rotated by the amount that is specified
        /// </summary>
        /// <param name="pPoint">The value that we are rotating</param>
        /// <param name="pRotationAmount">Expected to be sent through in radians</param>
        /// <returns></returns>
        public static Vector2 RotateVector(Vector2 pPoint, float pRotationAmount)
        {
            float _cos = (float)Math.Cos(pRotationAmount);
            float _sin = (float)Math.Sin(pRotationAmount);

            // Return the new value
            return new Vector2(pPoint.X * _cos - pPoint.Y * _sin,
                               pPoint.X * _sin + pPoint.Y * _cos);
        }

        /// <summary>
        /// Basic function for returning the gravity on the ant.
        /// </summary>
        /// <param name="pGravity">The value for determining the gravity constant</param>
        /// <param name="pMassOne">Mass value for the first item</param>
        /// <param name="pMassTwo">Mass value for the second item</param>
        /// <param name="pR">The distance between the two items</param>
        /// <returns></returns>
        public static Vector2 Gravity(float pGravity, float pMassOne, float pMassTwo, Vector2 pR)
        {
            return pR * -pGravity * (pMassOne * pMassTwo / (pR.LengthSquared() * pR.Length()));
        }

        // Used for normalising or doing something else that's wonderful
        public static float Truncate(Vector2 pForce, float pMax)
        {
            float _return = pMax / pForce.Length();
            _return = _return < 1.0f ? 1.0f : _return;

            return _return;
        }

        /// <summary>
        /// Ensure that the magnitude of a vector is within a certain amount
        /// </summary>
        /// <param name="pToTruncate">The vector in question</param>
        /// <param name="pMaxVelocity">The max value that we are truncating by.</param>
        /// <returns></returns>
        public static Vector2 TruncateVector(Vector2 pToTruncate, float pMaxVelocity)
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Simply rotate a vector without any kind of point of origin
        /// </summary>
        /// <param name="pOrigin">The pivot point that we are going to be rotating around</param>
        /// <param name="pRotationAmount">The amount that we are rotating by</param>
        /// <param name="pPoint">The vector that we are rotating</param>
        /// <returns></returns>
        public static Vector2 RotateVector(Vector2 pOrigin, float pRotationAmount, Vector2 pPoint)
        {
            float _cos = (float)Math.Cos(pRotationAmount);
            float _sin = (float)Math.Sin(pRotationAmount);

            Vector2 _translatedpoint = new Vector2();
            _translatedpoint.X = pPoint.X - pOrigin.X;
            _translatedpoint.Y = pPoint.Y - pOrigin.Y;

            Vector2 _rotatedpoint = new Vector2();
            _rotatedpoint.X = _translatedpoint.X * _cos - _translatedpoint.Y * _sin + pOrigin.X;
            _rotatedpoint.Y = _translatedpoint.X * _sin + _translatedpoint.Y * _cos + pOrigin.Y;

            return _rotatedpoint;
        }

        /// <summary>
        /// Return the float distance between vectors
        /// </summary>
        /// <param name="pVectorOne">The first vector parameter</param>
        /// <param name="pVectorTwo">The second vector parameter</param>
        /// <returns></returns>
        public static float DistanceBetweenVectors(Vector2 pVectorOne, Vector2 pVectorTwo)
        {
            return (float) Math.Sqrt(Math.Pow(pVectorTwo.X - pVectorOne.X,2) + Math.Pow(pVectorTwo.Y - pVectorOne.Y,2));
        }
    }
}
