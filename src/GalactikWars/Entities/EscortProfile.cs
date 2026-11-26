#region File Description
//-----------------------------------------------------------------------------
//EscortProfile.cs
//
//The escort a pilot picks before a single-player run (phase 3). Every escort
//grants exactly one advantage to the pilot's loadout, so the pick changes how
//the run plays without touching the shared player stats.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
#endregion

namespace GalactikWars.Entities
{
    internal sealed class EscortProfile
    {
        #region Fields

        ///<summary>The escort's call sign, shown in the menu and on the HUD.</summary>
        public string Name { get; }

        ///<summary>The single advantage this escort grants, for the menu line.</summary>
        public string Advantage { get; }

        ///<summary>The hull model flown by the escort, catalogued as a p1 hull;
        ///gameplay swaps it into the pilot's team family with
        ///ShipProfile.ForTeam.</summary>
        public string AllyModelName { get; }

        //pilot loadout granted by the escort
        public int Shield { get; }
        public float ThrustScale { get; }
        public int Rockets { get; }
        public int AtomBombs { get; }

        ///<summary>Seconds between two bullets while the trigger is held.</summary>
        public float FireInterval { get; }

        #endregion

        #region Initialization

        private EscortProfile(string name, string advantage,
            string allyModelName, int shield, float thrustScale, int rockets,
            int atomBombs, float fireInterval)
        {
            Name = name;
            Advantage = advantage;
            AllyModelName = allyModelName;
            Shield = shield;
            ThrustScale = thrustScale;
            Rockets = rockets;
            AtomBombs = atomBombs;
            FireInterval = fireInterval;
        }

        ///<summary>The unmodified loadout, used when no escort was picked.</summary>
        public static readonly EscortProfile Standard = new(
            "No escort", "standard loadout", "models/p1_saucer",
            GameVariables.Shield, 1.0f, GameVariables.RocketAmmo,
            GameVariables.NumAtomBombs, GameVariables.BulletFireInterval);

        ///<summary>The escorts offered on the selection screen, in menu order.</summary>
        public static readonly EscortProfile[] All =
        [
            //more shield
            new("Guardian", "+50 shield", "models/p1_saucer",
                GameVariables.Shield + 50, 1.0f, GameVariables.RocketAmmo,
                GameVariables.NumAtomBombs, GameVariables.BulletFireInterval),

            //faster
            new("Scout", "+25% thrust", "models/p1_wedge",
                GameVariables.Shield, 1.25f, GameVariables.RocketAmmo,
                GameVariables.NumAtomBombs, GameVariables.BulletFireInterval),

            //more missiles
            new("Rocketeer", "+10 rockets", "models/p1_pencil",
                GameVariables.Shield, 1.0f, GameVariables.RocketAmmo + 10,
                GameVariables.NumAtomBombs, GameVariables.BulletFireInterval),

            //more atom bombs
            new("Bombardier", "+2 atom bombs", "models/p1_wedge",
                GameVariables.Shield, 1.0f, GameVariables.RocketAmmo,
                GameVariables.NumAtomBombs + 2, GameVariables.BulletFireInterval),

            //faster normal shooting
            new("Gunner", "double fire rate", "models/p1_pencil",
                GameVariables.Shield, 1.0f, GameVariables.RocketAmmo,
                GameVariables.NumAtomBombs,
                GameVariables.BulletFireInterval / 2.0f),
        ];

        #endregion
    }
}
