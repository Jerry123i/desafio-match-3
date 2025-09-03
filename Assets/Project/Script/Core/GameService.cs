using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private Table<Tile> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;
        private GameMode _gameMode;
        
        //For use in Find the Pattern game
        private Table<int> pattern;

        public Table<Tile> StartGame(int boardWidth, int boardHeight, GameMode gameMode, List<TileType> initialBuildTiles)
        {
            _tilesTypes = new List<int>();
            for (int i = 0; i < initialBuildTiles.Count; i++)
                _tilesTypes.Add((int)initialBuildTiles[i]);
            
            _gameMode = gameMode;
            switch (_gameMode)
            {
                case GameMode.SquareMatch:
                    _boardTiles = CreateBoardSquareGameMode(boardWidth, boardHeight, _tilesTypes);
                    break;
                case GameMode.Standard:
                default:
                    _boardTiles = CreateBoardStandard(boardWidth, boardHeight, _tilesTypes);
                    GeneratePattern(); //TODO mover isso
                    break;
            }
            
            return _boardTiles;
        }
        private List<BoardSequence> ModifyBoard(Func<Table<Tile>, Table<Tile>> tileManipulation,
            Action<Table<bool>> destructionParameters,
            Func<Table<Tile>,List<AddedTileInfo>> newTilesParameters = null)
        {
            Table<Tile> newBoard = Table<Tile>.Clone(_boardTiles);

            if (newTilesParameters == null)
                newTilesParameters = TileAdditionStandard;
            
            //Makes sure that any special effects that change tile addition rules only run once
            bool specialNewTilesApplied = false; 
            
            if(tileManipulation!=null)
                newBoard = tileManipulation(newBoard);

            List<BoardSequence> boardSequences = new();
            List<MatchInformation> matchInformation = new();

            matchInformation = FindMatches(newBoard);

            Table<bool> tilesToDestroy = new Table<bool>(newBoard.Width, newBoard.Height);

            if(tileManipulation !=null) //No need to check matches if no move was made
                tilesToDestroy.MarkMatches(matchInformation);
            
            if (destructionParameters != null)
                destructionParameters(tilesToDestroy);

            tilesToDestroy = ApplySpecials(newBoard, matchInformation, tilesToDestroy);
            
            do
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new();
                for (int y = 0; y < newBoard.Height; y++)
                {
                    for (int x = 0; x < newBoard.Width; x++)
                    {
                        if (tilesToDestroy[x,y])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            newBoard[x,y] = new Tile { Id = -1, Type = -1 };
                        }
                    }
                }

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[x,j - 1];
                            newBoard[x,j] = movedTile;
                            if (movedTile.Type > -1)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[x,0] = new Tile
                        {
                            Id = -1,
                            Type = -1
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();

                if (!specialNewTilesApplied)
                {
                    addedTiles = newTilesParameters(newBoard);
                    specialNewTilesApplied = true;
                }
                else
                    addedTiles = TileAdditionStandard(newBoard);

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles
                };
                boardSequences.Add(sequence);
                
                tilesToDestroy.Clear();

                matchInformation = FindMatches(newBoard);                
                
                tilesToDestroy.MarkMatches(matchInformation);
                tilesToDestroy = ApplySpecials(newBoard, matchInformation, tilesToDestroy);

            } while (matchInformation.Count > 0);

            _boardTiles = newBoard;

            GeneratePattern();

            return boardSequences;

        }

        private List<AddedTileInfo> TileAdditionStandard(Table<Tile> newBoard)
        {
            List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
            for (int y = newBoard.Height - 1; y > -1; y--)
            {
                for (int x = newBoard.Width - 1; x > -1; x--)
                {
                    if (newBoard[x,y].Type == -1)
                    {
                        int tileType = Random.Range(0, _tilesTypes.Count);
                        Tile tile = newBoard[x,y];
                        tile.Id = _tileCount++;
                        tile.Type = _tilesTypes[tileType];
                        addedTiles.Add(new AddedTileInfo
                        {
                            Position = new Vector2Int(x, y),
                            Type = tile.Type
                        });
                    }
                }
            }

            return addedTiles;
        }
        
        private List<AddedTileInfo> TileAdditionBlockers(Table<Tile> newBoard)
        {
            List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();

            for (int x = newBoard.Width - 1; x >= 0; x--)
            {
                for (int y = newBoard.Height - 1; y >= 0; y--)
                {
                    if (newBoard[x,y].Type == -1)
                    {
                        int tileType = (int)TileType.Gray;
                        Tile tile = newBoard[x,y];
                        tile.Id = _tileCount++;
                        tile.Type = tileType;
                        addedTiles.Add(new AddedTileInfo
                        {
                            Position = new Vector2Int(x, y),
                            Type = tile.Type
                        });
                    }
                }
            }

            return addedTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {

            return ModifyBoard(
            //Swap
            (board) =>
            {
                (board[toX,toY], board[fromX,fromY]) = (board[fromX,fromY], board[toX,toY]);
                return board;
            },
            null
            );
        }

        public List<BoardSequence> TableSlideLeft()
        {
            return ModifyBoard(
                //Swap
                (board) =>
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        var first = board[0,y];
                        for (int x = 0; x < board.Width - 1; x++)
                        {
                            board[x, y] = board[x + 1, y];
                        }
                        board[board.Width-1,y] = first;
                    }
                    return board;
                },
                null
            );
        }
        
        public List<BoardSequence> TableSlideRight()
        {
            return ModifyBoard(
                //Swap
                (board) =>
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        int lastIndex = board.Width - 1;
                        var last = board[lastIndex,y];
                        
                        for (int x = lastIndex; x > 0; x--)
                        {
                            //Debug.Log($"{row},{col} -> {row},{col-1}");
                            board[x,y] = board[x-1,y];
                        }
                        board[0,y] = last;
                    }
                    
                    return board;
                },
                null
            );
        }
        
        public List<BoardSequence> SquareRotateClockwise(int x, int y)
        {
            return ModifyBoard(
                //Swap
                (board) =>
                {
                    Tile first = board[x, y];
                    board[x, y] = board[x, y - 1];
                    board[x, y - 1] = board[x - 1, y - 1];
                    board[x - 1, y - 1] = board[x - 1, y];
                    board[x - 1, y] = first;
                    
                    return board;
                },
                null
                );
        }
        
        public List<BoardSequence> SquareRotateCounterClockwise(int x, int y)
        {
            return ModifyBoard(
                //Swap
                (board) =>
                {
                    Tile first = board[x, y];
                    board[x, y] = board[x - 1, y];
                    board[x - 1, y] = board[x - 1, y - 1];
                    board[x - 1, y - 1] = board[x, y - 1];
                    board[x, y - 1] = first;

                    return board;
                },
                null
            );
        }

        public List<BoardSequence> DestroySingleTile(int x, int y)
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board[x,y] = true
                );
        }

        public List<BoardSequence> Explosion(int x, int y, int radius)
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board.MarkRadius(x, y, radius)
                );
        }

        public List<BoardSequence> EarthQuake()
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board.MarkEarthquakePattern(),
                TileAdditionBlockers
            );
        }

        private Table<Tile> CreateBoardSquareGameMode(int width, int height, List<int> tileTypes)
        {
            Table<Tile> board = new(width, height);
            
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    board[x, y] = new Tile { Id = -1, Type = -1 };
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 0 && y > 0 &&
                        board[x - 1, y].Type == board[x - 1, y - 1].Type &&
                        board[x - 1, y].Type == board[x, y - 1].Type)
                    {
                        noMatchTypes.Remove(board[x - 1, y].Type);
                    }
                    
                    board[x,y].Id = _tileCount++;
                    board[x,y].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }
        
        private Table<Tile> CreateBoardStandard(int width, int height, List<int> tileTypes)
        {
            Table<Tile> board = new(width, height);
            
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    board[x, y] = new Tile { Id = -1, Type = -1 };
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 1 &&
                        board[x - 1,y].Type == board[x - 2,y].Type)
                    {
                        noMatchTypes.Remove(board[x - 1,y].Type);
                    }

                    if (y > 1 &&
                        board[x,y - 1].Type == board[x,y - 2].Type)
                    {
                        noMatchTypes.Remove(board[x,y - 1].Type);
                    }

                    board[x,y].Id = _tileCount++;
                    board[x,y].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }

        private void ScanForPattern(Table<Tile> board, Action<MatchInformation> onMatchFound, bool stopAfterMatch)
        {
            bool isMatch = false;
            
            for (int y = 0; y < board.Height-pattern.Height + 1; y++)
            {
                for (int x = 0; x < board.Width-pattern.Width + 1; x++)
                {
                    CheckPattern(x, y);

                    if (isMatch)
                    {
                        List<Vector2Int> coordinates = new();
                        
                        for (int h = 0; h < pattern.Height; h++)
                        {
                            for (int w = 0; w < pattern.Width; w++)
                            {
                                if(pattern[w,h] == -1)
                                    continue;
                                coordinates.Add(new Vector2Int(x+w,y+h));
                            }
                        }

                        onMatchFound(new MatchInformation(coordinates));
                        if(stopAfterMatch)
                            return;
                        isMatch = false;
                    }
                }
            }

            void CheckPattern(int x, int y)
            {
                for (int h = 0; h < pattern.Height; h++)
                {
                    for (int w = 0; w < pattern.Width; w++)
                    {
                        if(pattern[w, h] == -1)
                            continue;
                        
                        if (board[x+w, y+h].Type != pattern[w, h])
                            return;
                    }
                }

                isMatch = true;
            }
        }
        
        private void ScanForLineMatches(Table<Tile> board, Action<MatchInformation> onMatchFound, bool stopAfterMatch)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var tileType = board[x, y].Type;
                    if (tileType == (int)TileType.Gray) continue;

                    if (x > 1 &&
                        tileType == board[x - 1, y].Type &&
                        tileType == board[x - 2, y].Type)
                    {
                        onMatchFound(new MatchInformation(Direction.Horizontal, x - 2, y, 3, tileType));
                        if(stopAfterMatch)
                            return;
                    }

                    if (y > 1 &&
                        tileType == board[x, y - 1].Type &&
                        tileType == board[x, y - 2].Type)
                    {
                        onMatchFound(new MatchInformation(Direction.Vertical, x, y - 2, 3, tileType));
                        if(stopAfterMatch)
                            return;
                    }
                }
            }
        }

        private void ScanForSquareMatches(Table<Tile> board, Action<MatchInformation> onMatchFound, bool stopAfterMatch)
        {
            for (int y = 1; y < board.Height; y++)
            {
                for (int x = 1; x < board.Width; x++)
                {
                    var tileType = board[x, y].Type;
                    if (tileType == (int)TileType.Gray) continue;

                    if (tileType == board[x - 1, y].Type &&
                        tileType == board[x, y - 1].Type &&
                        tileType == board[x - 1, y - 1].Type)
                    {
                        onMatchFound(new MatchInformation(Direction.Square, x, y, 2, tileType));
                        if(stopAfterMatch)
                            return;
                    }
                }
            }
        }

        private List<MatchInformation> FindMatches(Table<Tile> board)
        {
            List<MatchInformation> matches = new();

            switch (_gameMode) //TODO simplificar essas chamadas
            {
                case GameMode.FindThePattern:
                    ScanForPattern(board,
                        match => matches.AddAndCombine(match),
                        false);
                    break;
                
                case GameMode.SquareMatch:
                    ScanForSquareMatches(board,
                        match => matches.AddAndCombine(match),
                        false);
                    break;
                
                case GameMode.Standard:
                default:
                    ScanForLineMatches(board,
                        match=>matches.AddAndCombine(match),
                        false);
                    break;
            }

            return matches;

        }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            Table<Tile> newBoard = Table<Tile>.Clone(_boardTiles);
            (newBoard[toX, toY], newBoard[fromX, fromY]) = (newBoard[fromX, fromY], newBoard[toX, toY]);
            
            bool found = false;
            
            switch (_gameMode) //TODO simplificar essas chamadas
            {
                case GameMode.FindThePattern:
                    ScanForPattern(newBoard,
                        a=>found=true,
                        true);
                    break;
                case GameMode.SquareMatch:
                    ScanForSquareMatches(newBoard,
                        a=>found=true,
                        true);
                    break;                
                case GameMode.Standard:
                default:
                    ScanForLineMatches(newBoard,
                        a=>found=true,
                        true);
                    break;
            }

            return found;
        }

        public List<Vector2Int> GetSuggestions()
        {
            switch (_gameMode)
            {
                case GameMode.FindThePattern:
                    return new List<Vector2Int>();
                    
;                case GameMode.SquareMatch:
                    return GetSuggestionSquare(); 
                
                case GameMode.Standard:
                default:
                    return GetSuggestionsLines();
            }
        }
        
        //Non-exhaustive list of possible tiles to move
        private List<Vector2Int> GetSuggestionsLines()
        {
            List<Vector2Int> suggestions = new List<Vector2Int>();

            //Checks only center tiles
            for (int y = 1; y < _boardTiles.Height-1; y++)
            {
                for (int x = 1; x < _boardTiles.Width-1; x++)
                {
                    int tileType = _boardTiles[x,y].Type;
                    
                    CheckTPatternHorizontal(x,y);
                    CheckTPatternVertical(x,y);
                    CheckCrossPatternHorizontal(x,y);
                    CheckCrossPatternVertical(x,y);

                }
            }
            
            return suggestions;

            void CheckTPatternHorizontal(int x, int y)
            {
                int tileType = _boardTiles[x,y].Type;
                
                if(tileType == (int)TileType.Gray)
                    return;

                if (tileType == _boardTiles[x - 1,y].Type)
                {
                    if(_boardTiles[x+1,y+1].Type == tileType)
                        suggestions.Add(new Vector2Int(x+1,y+1));
                    if (_boardTiles[x + 1,y - 1].Type == tileType)
                        suggestions.Add(new Vector2Int(x+1, y-1));

                    if (x + 2 < _boardTiles.Width)
                    {
                        if (_boardTiles[x + 2,y].Type == tileType)
                            suggestions.Add(new Vector2Int(x+2,y));
                    }
                }

                if (tileType == _boardTiles[x + 1,y].Type)
                {
                    if(_boardTiles[x-1,y+1].Type == tileType)
                        suggestions.Add(new Vector2Int(x-1,y+1));
                    if(_boardTiles[x-1,y-1].Type == tileType)
                        suggestions.Add(new Vector2Int(x-1,y-1));

                    if (x > 1)
                    {
                        if(_boardTiles[x-2,y].Type == tileType)
                            suggestions.Add(new Vector2Int(x-2,y));
                    }
                }
            }

            void CheckTPatternVertical(int x, int y)
            {
                int tileType = _boardTiles[x,y].Type;
                
                if(tileType == (int)TileType.Gray)
                    return;

                if (tileType == _boardTiles[x,y - 1].Type)
                {
                    if(_boardTiles[x-1,y+1].Type == tileType)
                        suggestions.Add(new Vector2Int(x-1, y+1));
                    if(_boardTiles[x+1,y+1].Type == tileType)
                        suggestions.Add(new Vector2Int(x+1,y+1));

                    if (y + 2 < _boardTiles.Height)
                    {
                        if(_boardTiles[x,y+2].Type ==tileType)
                            suggestions.Add(new Vector2Int(x,y+2));
                    }
                }

                if (tileType == _boardTiles[x,y + 1].Type)
                {
                    if(_boardTiles[x-1,y-1].Type == tileType)
                        suggestions.Add(new Vector2Int(x-1, y-1));
                    if(_boardTiles[x+1,y-1].Type == tileType)
                        suggestions.Add(new Vector2Int(x+1,y-1));

                    if (y > 1)
                    {
                        if(_boardTiles[x,y-2].Type == tileType)
                            suggestions.Add(new Vector2Int(x,y-2));
                    }
                }
                
            }

            void CheckCrossPatternHorizontal(int x, int y)
            {
                int tileType = _boardTiles[x - 1,y].Type;
                if(tileType == (int)TileType.Gray)
                    return;
                
                if(_boardTiles[x+1,y].Type == tileType&&
                   _boardTiles[x,y+1].Type == tileType)
                    suggestions.Add(new Vector2Int(x, y+1));
                
                if(_boardTiles[x+1,y].Type == tileType&&
                   _boardTiles[x,y-1].Type == tileType)
                    suggestions.Add(new Vector2Int(x, y-1));
            }

            void CheckCrossPatternVertical(int x, int y)
            {
                int tileType = _boardTiles[x,y-1].Type;
                if(tileType == (int)TileType.Gray)
                    return;
                
                if(_boardTiles[x,y+1].Type == tileType &&
                   _boardTiles[x-1,y].Type == tileType)
                    suggestions.Add(new Vector2Int(x-1,y));
                
                if(_boardTiles[x,y+1].Type == tileType &&
                   _boardTiles[x+1,y].Type == tileType)
                    suggestions.Add(new Vector2Int(x+1,y));
                
            }
            
        }

        private List<Vector2Int> GetSuggestionSquare()
        {
            List<Vector2Int> suggestions = new List<Vector2Int>();

            int tileType = -1;
            
            for (int y = 0; y < _boardTiles.Height-1; y++)
            {
                for (int x = 0; x < _boardTiles.Width-1; x++)
                {
                    //■■
                    //■□
                    if (x > 0 && y > 0)
                    {
                        tileType = _boardTiles[x - 1, y].Type;
                        
                        if (tileType == _boardTiles[x - 1, y - 1].Type &&
                            tileType == _boardTiles[x, y - 1].Type)
                        {
                            if (x < _boardTiles.Width)
                            {
                                if(tileType == _boardTiles[x+1,y].Type)
                                    suggestions.Add(new Vector2Int(x+1,y));
                            }

                            if (y < _boardTiles.Height)
                            {
                                if(tileType == _boardTiles[x,y+1].Type)
                                    suggestions.Add(new Vector2Int(x,y+1));
                            }
                        }
                    }
                    
                    //■■
                    //□■
                    if (x < _boardTiles.Width && y > 0)
                    {
                        tileType = _boardTiles[x + 1, y].Type;

                        if (tileType == _boardTiles[x + 1, y - 1].Type &&
                            tileType == _boardTiles[x, y - 1].Type)
                        {
                            if (x > 0)
                            {
                                if(tileType == _boardTiles[x-1,y].Type)
                                    suggestions.Add(new Vector2Int(x-1,y));
                            }

                            if (y < _boardTiles.Height)
                            {
                                if(tileType == _boardTiles[x,y+1].Type)
                                    suggestions.Add(new Vector2Int(x,y+1));
                            }
                        }
                    }
                    
                    //■□
                    //■■
                    if (x > 0 && y < _boardTiles.Height)
                    {
                        tileType = _boardTiles[x - 1, y].Type;

                        if (tileType == _boardTiles[x - 1, y + 1].Type &&
                            tileType == _boardTiles[x, y + 1].Type)
                        {
                            if (x < _boardTiles.Width)
                            {
                                if(tileType == _boardTiles[x+1,y].Type)
                                    suggestions.Add(new Vector2Int(x+1,y));
                            }

                            if (y > 0)
                            {
                                if(tileType == _boardTiles[x,y-1].Type)
                                    suggestions.Add(new Vector2Int(x,y-1));
                            }
                        }
                    }
                    
                    //□■
                    //■■
                    if (x < _boardTiles.Width && y < _boardTiles.Height)
                    {
                        tileType = _boardTiles[x + 1, y].Type;

                        if (tileType == _boardTiles[x, y + 1].Type &&
                            tileType == _boardTiles[x + 1, y + 1].Type)
                        {
                            if (x > 0)
                            {
                                if(tileType == _boardTiles[x-1,y].Type)
                                    suggestions.Add(new Vector2Int(x-1,y));
                            }

                            if (y > 0)
                            {
                                if(tileType == _boardTiles[x,y-1].Type)
                                    suggestions.Add(new Vector2Int(x,y-1));
                            }
                        }
                    }
                }
            }

            return suggestions;

        }

        private static Table<bool> ApplySpecials(Table<Tile> board, List<MatchInformation> matchInformation, Table<bool> markedTiles)
        {
            for (int i = 0; i < matchInformation.Count; i++)
            {
                var match = matchInformation[i];
                
                if(match.Length < 4)
                    continue;
        
                switch ((TileType)match.TileType)
                {
                    case TileType.Blue: // Blue - Line Clear
                        if (match.Direction == Direction.Horizontal)
                            markedTiles.MarkLine(match.y);
                        return markedTiles; 
                    
                    case TileType.Green: // Green - Column Clear
                        if (match.Direction == Direction.Vertical)
                            markedTiles.MarkColumn(match.x);
                        return markedTiles;
                    
                    case TileType.Orange: // Orange - Clear all orange
                        markedTiles.MarkSameType(board, match.TileType);
                        return markedTiles;
                    
                    case TileType.Yellow: //Yellow = Explosion
                        markedTiles.MarkRadius(match.x, match.y, 3);
                        return markedTiles;
                }
                
            }
        
            return markedTiles;
        }

        private void GeneratePattern()
        {
            Table<int> patternModel = PatternModels.GetRandomPattern();
            
            Table<int> table = new(3, 3, -1);

            int targetX = Random.Range(1, _boardTiles.Width - table.Width-1);
            int targetY = Random.Range(1, _boardTiles.Height - table.Height-1);

            for (int x = 0; x < table.Width; x++)
            {
                for (int y = 0; y < table.Height; y++)
                {
                    if(patternModel[x,y] == 0)
                        continue;
                    table[x, y] = _boardTiles[targetX + x, targetY + y].Type;
                }
            }

            Vector2Int indexToMove = patternModel.GetIndexRandomValueDifferentFrom(0);

            if (indexToMove.x < 0)
            {
                pattern = table;
                return;
            }

            List<Vector2Int> directions = new ()
            {
                new(1,0),
                new(0,1),
                new(-1,0),
                new(0,-1)
            };
            
            Vector2Int randomDirection = directions[Random.Range(0, 4)];
            
            //Checks if the swapping includes tiles already in the pattern
            bool insideMove = false;
            
            int xToSwap = indexToMove.x + randomDirection.x;
            int yToSwap = indexToMove.y + randomDirection.y;

            if (xToSwap < table.Width && yToSwap < table.Height &&
                xToSwap >= 0 && yToSwap >= 0)
            {
                if (table[xToSwap, yToSwap] > -1)
                    insideMove = true;
            }

            if (insideMove)
                (table[indexToMove.x, indexToMove.y], table[xToSwap, yToSwap]) = (table[xToSwap, yToSwap], table[indexToMove.x, indexToMove.y]);
            else
                table[indexToMove.x, indexToMove.y] = _boardTiles[targetX+xToSwap, targetY+yToSwap].Type;

            pattern = table;

        }

        public Table<int> GetPattern()
        {
            return pattern;
        }
        
    }
}
