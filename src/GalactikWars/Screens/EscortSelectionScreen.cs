#region File Description
//-----------------------------------------------------------------------------
//EscortSelectionScreen.cs
//
//Single-player loadout choice (phase 3): each escort grants the pilot one
//advantage (shield, thrust, rockets, atom bombs or fire rate). The pick is
//passed to the gameplay screen, which applies it to the player's loadout.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GalactikWars.Entities;
#endregion

namespace GalactikWars.Screens
{
    ///<summary>
    ///Lets the pilot pick the escort that will fly alongside them for this
    ///single-player run.
    ///</summary>
    internal class EscortSelectionScreen : MenuScreen
    {
        #region Initialization

        public EscortSelectionScreen()
            : base("Choose Your Escort")
        {
            foreach (EscortProfile escort in EscortProfile.All)
            {
                EscortProfile picked = escort;

                //stick to ASCII: the menu spritefont has no em dash glyph
                MenuEntry entry = new(escort.Name + " - " + escort.Advantage);
                entry.Selected += (sender, e) => StartRun(picked);

                MenuEntries.Add(entry);
            }
        }

        #endregion

        #region Handle Input

        ///<summary>
        ///Loads the single-player arena with the chosen escort. The loading
        ///screen replaces this menu and the main menu behind it.
        ///</summary>
        private void StartRun(EscortProfile escort)
        {
            LoadingScreen.Load(ScreenManager, true,
                new GameplayScreenSinglePlayer(escort));
        }

        ///<summary>
        ///Cancelling returns to the main menu (this screen is pushed on top of
        ///it, so it is still in the screen stack).
        ///</summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }

        #endregion
    }
}
