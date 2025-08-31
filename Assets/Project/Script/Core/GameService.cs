using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        return true;
                    }

                    if (y > 1 &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }
        private List<BoardSequence> ModifyBoard(Func<List<List<Tile>>, List<List<Tile>>> tileManipulation, Action<List<List<bool>>> destructionParameters, bool useFindMatches)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            if(tileManipulation!=null)
                newBoard = tileManipulation(newBoard);

            List<BoardSequence> boardSequences = new();
            List<MatchInformation> matchInformation = new();
            matchInformation = FindMatches(newBoard);

            List<List<bool>> tilesToDestroy = new List<List<bool>>();
            for (int y = 0; y < newBoard.Count; y++)
            {
                tilesToDestroy.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard.Count; x++)
                {
                    tilesToDestroy[y].Add(false);
                }
            }

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
                for (int y = 0; y < newBoard.Count; y++)
                {
                    for (int x = 0; x < newBoard[y].Count; x++)
                    {
                        if (tilesToDestroy[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            newBoard[y][x] = new Tile { Id = -1, Type = -1 };
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
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
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

                        newBoard[0][x] = new Tile
                        {
                            Id = -1,
                            Type = -1
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].Type == -1)
                        {
                            int tileType = Random.Range(0, _tilesTypes.Count);
                            Tile tile = newBoard[y][x];
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

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles
                };
                boardSequences.Add(sequence);
                
                tilesToDestroy.Clear();
                
                //TODO deixar salvo um board falso para copiar
                for (int y = 0; y < newBoard.Count; y++)
                {
                    tilesToDestroy.Add(new List<bool>(newBoard[y].Count));
                    for (int x = 0; x < newBoard.Count; x++)
                    {
                        tilesToDestroy[y].Add(false);
                    }
                }
                
                matchInformation = FindMatches(newBoard);
                tilesToDestroy.MarkMatches(matchInformation);
                tilesToDestroy = ApplySpecials(newBoard, matchInformation, tilesToDestroy);

            } while (matchInformation.Count > 0);

            _boardTiles = newBoard;

            return boardSequences;

        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {

            return ModifyBoard(
            //Swap
            (board) =>
            {
                (board[toY][toX], board[fromY][fromX]) = (board[fromY][fromX], board[toY][toX]);
                return board;
            },
            null,
            true
            );
        }

        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type });
                }
            }

            return newBoard;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1 });
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
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].Type);
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }

        private static List<MatchInformation> FindMatches(List<List<Tile>> newBoard, bool applySpecials = false)
        {
            List<MatchInformation> matchInformation = new();
            
            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        matchInformation.AddAndCombine(new MatchInformation(Direction.Horizontal, x-2,y,3, newBoard[y][x].Type));
                    }

                    if (y > 1 &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        matchInformation.AddAndCombine(new MatchInformation(Direction.Vertical, x,y-2,3, newBoard[y][x].Type));
                    }
                }
            }

            return matchInformation;
        }

        private static List<List<bool>> ApplySpecials(List<List<Tile>> board, List<MatchInformation> matchInformation, List<List<bool>> markedTiles)
        {
            for (int i = 0; i < matchInformation.Count; i++)
            {
                var match = matchInformation[i];
                
                if(match.Length < 4)
                    continue;
        
                switch (match.TileType)
                {
                    case 0: // Blue - Line Clear
                        if (match.Direction == Direction.Horizontal)
                            markedTiles.MarkLine(match.y);
                        return markedTiles; 
                    
                    case 1: // Green - Column Clear
                        if (match.Direction == Direction.Vertical)
                            markedTiles.MarkColumn(match.x);
                        return markedTiles;
                    
                    case 2: // Orange - Clear all orange
                        markedTiles.MarkSameType(board, match.TileType);
                        return markedTiles;
                    
                    case 3: //Yellow = Explosion
                        markedTiles.MarkRadius(match.x, match.y, 3);
                        return markedTiles;
                }
                
            }
        
            return markedTiles;
        }
        
        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < list[y].Count; x++)
                {
                    if (list[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
