﻿namespace Battleship.Game.Ships
{
    public interface IShip
    {
        #region Properties

        // The length of the ship
        int ShipLength { get; }

        // The type if char
        char ShipChar { get; }

        // Ship hit counter
        int CoordinateStatus { get; set; }

        // Ship hit counter
        int ShipHit { get; set; }

        // Ship Index
        int ShipIndex { get; }

        // Ship Sunk
        bool IsShipSunk { get; set; }

        #endregion
    }
}