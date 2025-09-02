using System;
using System.Collections.Generic;
using System.Linq;
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

        public Table<Tile> StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }
        private List<BoardSequence> ModifyBoard(Func<Table<Tile>, Table<Tile>> tileManipulation,
            Action<Table<bool>> destructionParameters,
            bool useFindMatches,
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

            if(useFindMatches)
                tilesToDestroy.MarkMatches(matchInformation);
            else
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
                
                // for (int y = 0; y < newBoard.Height; y++)
                // {
                //     //tilesToDestroy.Add(new List<bool>(newBoard[y].Count));
                //     for (int x = 0; x < newBoard.Count; x++)
                //     {
                //         tilesToDestroy[y].Add(false);
                //     }
                // }
                
                matchInformation = FindMatches(newBoard);
                tilesToDestroy.MarkMatches(matchInformation);
                tilesToDestroy = ApplySpecials(newBoard, matchInformation, tilesToDestroy);

            } while (matchInformation.Count > 0);

            _boardTiles = newBoard;

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
            null,
            true
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
                null,
                true
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
                null,
                true
            );
        }

        public List<BoardSequence> DestroySingleTile(int x, int y)
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board[x,y] = true,
                false);
        }

        public List<BoardSequence> Explosion(int x, int y, int radius)
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board.MarkRadius(x, y, radius),
                false);
        }

        public List<BoardSequence> EarthQuake()
        {
            return ModifyBoard(
                //Swap
                null,
                //Destroy
                board => board.MarkEarthquakePattern(),
                false,
                TileAdditionBlockers
            );
        }

        private Table<Tile> CreateBoard(int width, int height, List<int> tileTypes)
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

        private static List<MatchInformation> FindMatches(Table<Tile> newBoard)
        {
            List<MatchInformation> matchInformation = new();
            
            for (int y = 0; y < newBoard.Height; y++)
            {
                for (int x = 0; x < newBoard.Width; x++)
                {
                    if (newBoard[x,y].Type == (int)TileType.Gray)
                        continue;
                    
                    if (x > 1 &&
                        newBoard[x,y].Type == newBoard[x-1,y].Type &&
                        newBoard[x - 1,y].Type == newBoard[x - 2,y].Type)
                    {
                        matchInformation.AddAndCombine(new MatchInformation(Direction.Horizontal, x-2,y,3, newBoard[x,y].Type));
                    }

                    if (y > 1 &&
                        newBoard[x,y].Type == newBoard[x,y - 1].Type &&
                        newBoard[x,y - 1].Type == newBoard[x,y - 2].Type)
                    {
                        matchInformation.AddAndCombine(new MatchInformation(Direction.Vertical, x,y-2,3, newBoard[x,y].Type));
                    }
                }
            }

            return matchInformation;
        }
        
        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            Table<Tile> newBoard = Table<Tile>.Clone(_boardTiles);

            (newBoard[toX,toY], newBoard[fromX,fromY]) = (newBoard[fromX,fromY], newBoard[toX,toY]);

            for (int y = 0; y < newBoard.Height; y++)
            {
                for (int x = 0; x < newBoard.Width; x++)
                {
                    if(newBoard[x,y].Type == (int)TileType.Gray)
                        continue;
                    
                    if (x > 1 &&
                        newBoard[x,y].Type == newBoard[x - 1,y].Type &&
                        newBoard[x - 1,y].Type == newBoard[x - 2,y].Type)
                    {
                        return true;
                    }

                    if (y > 1 &&
                        newBoard[x,y].Type == newBoard[x,y - 1].Type &&
                        newBoard[x,y - 1].Type == newBoard[x,y - 2].Type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        //Non-exhaustive list of possible tiles to move
        public List<Vector2Int> GetSuggestions()
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
        
    }
}
