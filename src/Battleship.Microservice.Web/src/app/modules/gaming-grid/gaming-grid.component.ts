import { Component, OnInit, Input } from '@angular/core';
import { BoardService } from '../../core/services/board.service';
import { ScoreCard } from '../../core/models/scoreCard';
import { Configuration } from '../../core/helper/configuration';
import { Coordinate } from '../../core/models/coordinate';
import { Player } from '../../core/models/player';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { PlayerCommand } from '../../core/models/playerCommand';
import { ScoreCardService } from '../../core/services/score-card.service';
import HttpStatusCode from 'src/app/core/helper/http.codes';

@Component({
  selector: 'app-battleship-gaming-root',
  templateUrl: './gaming-grid.component.html',
  styleUrls: ['./gaming-grid.component.css'],
})
export class GamingGridComponent implements OnInit {
  playerScoreCard: ScoreCard;
  player: Player;
  isGameStarted = false;
  selectedShipCounter: number;
  numberOfShipOptions: Array<any>;

  currentCoordinates: Array<Coordinate> = [];
  xAxis: Array<string> = [];
  yAxis: Array<number> = [];

  x: number;
  y: number;

  constructor(
    private router: Router,
    private battleShipService: BoardService,
    private config: Configuration,
    private dialog: MatDialog,
    private scoreCardService: ScoreCardService
  ) {
    this.numberOfShipOptions = config.shipCounter;
    this.player = this.router.getCurrentNavigation().extras.state.player;
  }
  numberOfShips: number;
  completed: boolean;
  errorMessage: string;
  gameStatus: string;

  public ngOnInit(): void {
    this.buildGamingGrid();
    this.selectedShipCounter = 0;
  }

  public onSaveGame(data: any) {
    if (this.player.isDemoAccount) {
      this.config.openDialog(
        this.dialog,
        this.config.demoAccountSaveError,
        this.config.close
      );
      return;
    }
  }

  public onChange(data) {
    this.selectedShipCounter = parseInt(data, 10);
    this.isGameStarted = true;
    const ref = this.config.openDialog(
      this.dialog,
      this.config.gameStarted,
      this.config.start
    );
    ref.afterClosed().subscribe(() => {
      this.getScoreCard();
    });

    this.battleShipService.startGame(this.selectedShipCounter).subscribe((gameStartedResponse) => {
      if (gameStartedResponse.status === HttpStatusCode.OK) {
        return;
      } else {
        this.gameStatus = this.config.somethingWentWrongError;
      }
    },
      (error) => {
        this.config.handleError(error);
      }
    );
  }

  private buildGamingGrid() {
    this.battleShipService.getGamingGrid().subscribe(
      data => {
        const x = 'x';
        const y = 'y';
        this.xAxis = data.body[x];
        this.yAxis = data.body[y];
        this.errorMessage = '';
      },
      error => {
        this.errorMessage = this.config.applicationError;
        console.log(error);
      }
    );
  }

  getScoreCard() {
    this.scoreCardService.getPlayerScoreCard().subscribe(
      response => {
        if (response.status === HttpStatusCode.OK) {
          this.playerScoreCard = JSON.parse(response.body) as ScoreCard;
        }
      },
      error => {
        console.log(error);
      }
    );
  }


  private isCoordinateHandled(X: number, Y: number): boolean {
    let result = false;
    if (
      this.currentCoordinates.filter((q) => q.x === X && q.y === Y).length === 0
    ) {
      const coordinate: Coordinate = { x: X, y: Y };
      this.currentCoordinates.push(coordinate);
    } else {
      result = true;
    }

    return result;
  }

  checkCoordinate(event) {
    // first see if the ships have been selected
    if (this.selectedShipCounter == null || this.selectedShipCounter === 0) {
      this.config.openDialog(
        this.dialog,
        this.config.gameNotStarted,
        this.config.close
      );
      return;
    }

    const target = event.target || event.srcElement || event.currentTarget;
    if (target != null) {
      this.x = target.getAttribute('data-x').charCodeAt(0);
      this.y = parseInt(target.getAttribute('data-y'), 10);

      const coordinate: Coordinate = { x: this.x, y: this.y };
      const playerCommand: PlayerCommand = new PlayerCommand();

      playerCommand.coordinate = coordinate;
      playerCommand.scoreCard = this.playerScoreCard as ScoreCard;

      this.battleShipService.UserInput(playerCommand).subscribe((result) => {
        if (result.status === HttpStatusCode.OK) {
          if (result.body.isHit) {
            target.className = this.config.hitClass;
            target.innerHTML = '<i class="material-icons">directions_boat</i>';
          } else {
            target.className = this.config.missClass;
          }
          this.playerScoreCard = result.body as ScoreCard;
        }
      });
    }
  }
}
