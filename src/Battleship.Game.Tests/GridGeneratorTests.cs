﻿namespace Battleship.Game.Tests
{
    using System;
    using System.Collections.Generic;

    using Battleship.Game.Board;
    using Battleship.Game.Ships;
    using Battleship.Infrastructure.Core.Components;

    using NUnit.Framework;

    [TestFixture]
    public class GridGeneratorTests : ComponentBase
    {
        private GridGenerator gridGenerator;

        private IShipRandomiser shipRandomiser;

        [Test]
        public void Board_WhenGridGenerated_ReturnOneHundredSegments()
        {
            // Arrange
            int totalSegments = this.GridDimension * this.GridDimension;
            int? result = 0;

            // Act
            try
            {
                this.gridGenerator = GridGenerator.Instance();
                result = this.gridGenerator.TotalSegments;
            }
            catch (IndexOutOfRangeException)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.Fail($"{e.Message}\n{e.StackTrace}");
            }

            // Assert
            Assert.AreEqual(totalSegments, result);
        }

        [Test]
        public void Board_WhenGridGenerated_ReturnThirteenOccupiedSegments()
        {
            // Arrange
            List<IShip> ships = new List<IShip> { new BattleShip(1), new Destroyer(2), new Destroyer(3) };
            this.shipRandomiser = ShipRandomiser.Instance();
            int occupiedSegments = this.shipRandomiser.GetRandomisedShipCoordinates(ships).Count;

            // Act
            int? result = this.gridGenerator.NumberOfOccupiedSegments;

            // Assert
            Assert.AreNotEqual(occupiedSegments, result);
        }
    }
}