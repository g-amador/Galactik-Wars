#region File Description
//-----------------------------------------------------------------------------
//ShipProfile.cs
//
//The hull a pilot picks before a single-player run (phase 3). The hull and the
//paint scheme are picked separately: the hull is one of three models and the
//paint decides the team. The original p1 models wear the blue textures and the
//p2 models the red ones.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System;
#endregion

namespace GalactikWars.Entities
{
    internal sealed class ShipProfile
    {
        #region Fields

        //the hull model base names, in menu order (p1/p2 are added per team)
        private static readonly string[] HullModelNames =
        [
            "pencil", "wedge", "saucer",
        ];

        //the hull names shown in the selection menu, matching HullModelNames
        private static readonly string[] HullNames =
        [
            "Pencil", "Wedge", "Saucer",
        ];

        ///<summary>The hull's short name, shown in the selection menu.</summary>
        public string Name { get; }

        ///<summary>The model loaded for this hull and paint scheme.</summary>
        public string ModelName { get; }

        ///<summary>True for the p1 hulls (blue textures), false for p2 (red).</summary>
        public bool IsBlueTeam { get; }

        public string TeamName => IsBlueTeam ? "Blue" : "Red";

        ///<summary>Ambient tint worn by allies (the same value gameplay uses).</summary>
        public Vector3 TeamTint => IsBlueTeam ? BlueTeamTint : RedTeamTint;

        ///<summary>Ambient tint worn by the ships fighting the pilot.</summary>
        public Vector3 EnemyTeamTint => IsBlueTeam ? RedTeamTint : BlueTeamTint;

        ///<summary>A bright version of the team color that reads well as text.</summary>
        public Color TeamLabelColor => IsBlueTeam
            ? new Color((byte)110, (byte)170, (byte)255)
            : new Color((byte)255, (byte)110, (byte)110);

        public static readonly Vector3 BlueTeamTint = new(0.15f, 0.30f, 0.70f);
        public static readonly Vector3 RedTeamTint = new(0.70f, 0.12f, 0.12f);

        ///<summary>How many hulls the selection screen offers.</summary>
        public static int HullCount => HullNames.Length;

        ///<summary>The fallback ship (Pencil, blue) when no pick is passed in.</summary>
        public static ShipProfile Default => Create(0, true);

        #endregion

        #region Initialization

        private ShipProfile(string name, string modelName, bool isBlueTeam)
        {
            Name = name;
            ModelName = modelName;
            IsBlueTeam = isBlueTeam;
        }

        ///<summary>
        ///Builds the profile for a hull index (menu order) and paint scheme.
        ///The index wraps, so callers can cycle through the hulls freely.
        ///</summary>
        public static ShipProfile Create(int hullIndex, bool blueTeam)
        {
            int index = hullIndex % HullModelNames.Length;

            if (index < 0)
            {
                index += HullModelNames.Length;
            }

            string modelName = "models/" + (blueTeam ? "p1_" : "p2_")
                + HullModelNames[index];

            return new ShipProfile(HullNames[index], modelName, blueTeam);
        }

        ///<summary>
        ///Swaps a hull into the requested team's model family (p1 for blue,
        ///p2 for red). Escorts are catalogued as p1 hulls, so this keeps the
        ///escort's texture and tint on the same side as the pilot.
        ///</summary>
        public static string ForTeam(string modelName, bool blueTeam)
        {
            bool isBlueHull = modelName.Contains("p1", StringComparison.Ordinal);

            if (isBlueHull == blueTeam)
            {
                return modelName;
            }

            return isBlueHull
                ? modelName.Replace("p1", "p2", StringComparison.Ordinal)
                : modelName.Replace("p2", "p1", StringComparison.Ordinal);
        }

        #endregion
    }
}
