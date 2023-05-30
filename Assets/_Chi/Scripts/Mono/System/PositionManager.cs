using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace _Chi.Scripts.Mono.System
{
    public class PositionManager : MonoBehaviour
    {
        public float rectHeight = 5;
        public float rectWidth = 5;
        public int rectCountX = 8;
        public int rectCountY = 8;

        [NonSerialized] private float minRelativePositionX = float.MaxValue;
        [NonSerialized] private float maxRelativePositionX = float.MinValue;
        [NonSerialized] private float minRelativePositionY = float.MaxValue;
        [NonSerialized] private float maxRelativePositionY = float.MinValue;
        
        private Dictionary<Npc, Rect> npcRects;
        public Rect[,] rects;


        public List<Rect> allRects;
        public Rect[] sortedEdgeRects;
        public bool debugAllRects;

        public bool sortEdges = false;

        public bool influenceNeighbourRects = false;

        public void Start()
        {
            npcRects = new Dictionary<Npc, Rect>();
            rects = new Rect[rectCountX, rectCountY];
            var edgeRects = new List<Rect>();

            float offsetX = (rectCountX / 2f) * rectWidth;
            float offsetY = (rectCountY / 2f) * rectHeight;
            
            for (int i = 0; i < rectCountX; i++)
            {
                for (int j = 0; j < rectCountY; j++)
                {
                    float posX = (i * rectWidth) - offsetX;
                    float posY = (j * rectHeight) - offsetY;

                    var direction = Gamesystem.PlayerMoveDirection.Idle;

                    if (posX >= 0)
                    {
                        direction |= Gamesystem.PlayerMoveDirection.Right;
                    }
                    else
                    {
                        direction |= Gamesystem.PlayerMoveDirection.Left;
                    }
                    
                    if(posY >= 0)
                    {
                        direction |= Gamesystem.PlayerMoveDirection.Up;
                    }
                    else
                    {
                        direction |= Gamesystem.PlayerMoveDirection.Down;
                    }
                    
                    rects[i,j] = new Rect(i, j, posX, posY, direction);
                    
                    bool isEdgeRect = i == 0 || j == 0 || i == rectCountX - 1 || j == rectCountY - 1;

                    if (isEdgeRect)
                    {
                        edgeRects.Add(rects[i,j]);
                    }
                    
                    allRects.Add(rects[i,j]);
                }
            }
            
            sortedEdgeRects = edgeRects.ToArray();
            
            minRelativePositionX = -offsetX;
            maxRelativePositionX = offsetX;
            minRelativePositionY = -offsetY;
            maxRelativePositionY = offsetY;
            
            StartCoroutine(UpdatePositions());

            if (sortEdges)
            {
                StartCoroutine(SortRects());
            }
        }
        
        public IEnumerator UpdatePositions()
        {
            const int updatesPerFrame = 50;
            int updates = 0;

            var npcs = Gamesystem.instance.objects.npcEntitiesList;

            while (true)
            {
                for (var index = 0; index < npcs.Count; index++)
                {
                    if (index >= npcs.Count)
                    {
                        break;
                    }
                
                    var npc = npcs[index];
                    if (updates++ > updatesPerFrame)
                    {
                        updates = 0;
                        yield return null;
                    }

                    if (npc != null && npc.activated && npc.takesUpPositionInPositionManager)
                    {
                        UpdatePosition(npc, npc.GetPosition());
                    }
                }
            }
        }

        public void RemovePosition(Npc npc)
        {
            if (npc.takesUpPositionInPositionManager && npcRects.TryGetValue(npc, out var previousNpcRect))
            {
                previousNpcRect.monsters -= 2;

                if (influenceNeighbourRects)
                {
                    ApplyToNeighbourCells(previousNpcRect.x, previousNpcRect.y, (r) =>
                    {
                        r.monsters -= 1;
                    });
                }

                npcRects.Remove(npc);
            }
        }

        public void UpdatePosition(Npc npc, Vector3 position)
        {
            var playerPosition = Gamesystem.instance.objects.currentPlayer.position;

            var relativePosition = position - playerPosition;
                
            var currentNpcRect = GetRect(relativePosition);

            if (currentNpcRect != null)
            {
                if (npcRects.TryGetValue(npc, out var previousNpcRect))
                {
                    if(previousNpcRect != currentNpcRect)
                    {
                        previousNpcRect.monsters -= 2;
                        currentNpcRect.monsters += 2;

                        if (influenceNeighbourRects)
                        {
                            ApplyToNeighbourCells(previousNpcRect.x, previousNpcRect.y, (r) =>
                            {
                                r.monsters -= 1;
                            });
                        
                            ApplyToNeighbourCells(currentNpcRect.x, currentNpcRect.y, (r) =>
                            {
                                r.monsters += 1;
                            });
                        }
                        
                        npcRects[npc] = currentNpcRect;
                    }
                }
                else
                {
                    currentNpcRect.monsters += 2;
                    npcRects[npc] = currentNpcRect;

                    if (influenceNeighbourRects)
                    {
                        ApplyToNeighbourCells(currentNpcRect.x, currentNpcRect.y, (r) =>
                        {
                            r.monsters += 1;
                        });
                    }
                }
            }
        }

        public void Update()
        {
            if (debugAllRects)
            {
                foreach (var rect in sortedEdgeRects)
                {
                    var color = Color.Lerp(Color.black, Color.white, rect.monsters / 100f);

                    if (rect.highlightDebug) color = Color.green;
                        
                    DebugRect(rect, color);
                }
            }
        }

        private void DebugRect(Rect rect, Color c)
        {
            var playerPos = Gamesystem.instance.objects.currentPlayer.position;

            var center = new Vector3(playerPos.x + rect.posX + (rectWidth / 2f), playerPos.y + rect.posY + (rectHeight / 2f), playerPos.z);
                    
            // draw top line
            Debug.DrawLine(new Vector3(center.x - rectWidth / 2f, center.y + rectHeight / 2f, center.z), new Vector3(center.x + rectWidth / 2f, center.y + rectHeight / 2f, center.z), c);
                    
            // draw bottom line
            Debug.DrawLine(new Vector3(center.x - rectWidth / 2f, center.y - rectHeight / 2f, center.z), new Vector3(center.x + rectWidth / 2f, center.y - rectHeight / 2f, center.z), c);
                    
            // draw right line
            Debug.DrawLine(new Vector3(center.x + rectWidth / 2f, center.y - rectHeight / 2f, center.z), new Vector3(center.x + rectWidth / 2f, center.y + rectHeight / 2f, center.z), c);
                    
            // draw left line
            Debug.DrawLine(new Vector3(center.x - rectWidth / 2f, center.y - rectHeight / 2f, center.z), new Vector3(center.x - rectWidth / 2f, center.y + rectHeight / 2f, center.z), c);
        }

        private Rect GetRect(Vector3 relativeToPlayerPosition)
        {
            /*if(relativeToPlayerPosition.x < minRelativePositionX 
               || relativeToPlayerPosition.x > maxRelativePositionX 
               || relativeToPlayerPosition.y < minRelativePositionY 
               || relativeToPlayerPosition.y > maxRelativePositionY)
            {
                return null;
            }*/
            
            int cellX = Mathf.FloorToInt((relativeToPlayerPosition.x + rectCountX * rectWidth / 2f) / rectWidth);
            int cellY = Mathf.FloorToInt((relativeToPlayerPosition.y + rectCountY * rectHeight / 2f) / rectHeight);

            // Clamp cellX and cellY values within the grid dimensions.
            cellX = Mathf.Clamp(cellX, 0, rectCountX - 1);
            cellY = Mathf.Clamp(cellY, 0, rectCountY - 1);

            // Return the cell at these coordinates
            return rects[cellX, cellY];
        }
        
        private void ApplyToNeighbourCells(int x, int y, Action<Rect> action)
        {
            // Check each possible neighbour position.
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip the current cell.
                    if (dx == 0 && dy == 0)
                        continue;

                    // Compute the neighbour's coordinates.
                    int nx = x + dx;
                    int ny = y + dy;

                    // If the neighbour's coordinates are within the grid's bounds, add the neighbour to the list.
                    if (nx >= 0 && nx < rectCountX && ny >= 0 && ny < rectCountY)
                    {
                        action(rects[nx, ny]);
                    }
                }
            }
        }

        private IEnumerator SortRects()
        {
            while (true)
            {
                Array.Sort(sortedEdgeRects, (rect1, rect2) => rect1.monsters.CompareTo(rect2.monsters));
                yield return new WaitForSeconds(0.33f);
            }
        }
        
        public Rect GetLeastOccupiedRect(Gamesystem.PlayerMoveDirection favoredDirection = Gamesystem.PlayerMoveDirection.Idle)
        {
            Rect leastOccupiedRect = null;
            float leastMonsters = float.MaxValue;
            
            foreach (Rect rect in allRects)
            {
                if (favoredDirection > Gamesystem.PlayerMoveDirection.Idle && GetCommonValues(favoredDirection, rect.direction) < 2)
                {
                    continue;
                }
                    
                if (rect.monsters < leastMonsters)
                {
                    leastOccupiedRect = rect;
                    leastMonsters = rect.monsters;
                }
            }
            
            if(leastOccupiedRect == null)
            {
                foreach (Rect rect in allRects)
                {
                    if (favoredDirection > Gamesystem.PlayerMoveDirection.Idle && GetCommonValues(favoredDirection, rect.direction) < 1)
                    {
                        continue;
                    }
                    
                    if (rect.monsters < leastMonsters)
                    {
                        leastOccupiedRect = rect;
                        leastMonsters = rect.monsters;
                    }
                }
            }
                
            if(leastOccupiedRect == null)
            {
                foreach (Rect rect in allRects)
                {
                    if (rect.monsters < leastMonsters)
                    {
                        leastOccupiedRect = rect;
                        leastMonsters = rect.monsters;
                    }
                }
            }

            return leastOccupiedRect;
        }

        private int GetCommonValues(Gamesystem.PlayerMoveDirection direction1, Gamesystem.PlayerMoveDirection direction2)
        {
            // Perform a bitwise AND operation to get the common values
            var commonValues = direction1 & direction2;

            // Count the number of set bits (common values)
            int count = 0;
            while (commonValues != 0)
            {
                commonValues &= (commonValues - 1);
                count++;
            }

            return count;
        }

        public Rect GetLeastOccupiedRect(float randomFactor = 0f)
        {
            int maxIndex = Mathf.FloorToInt(randomFactor * sortedEdgeRects.Length);

            int randomIndex = UnityEngine.Random.Range(0, maxIndex + 1);

            // Return the rect at the random index.
            return sortedEdgeRects[randomIndex];
        }

        public Vector3 GetRandomPositionInRect(Rect rect, UnityEngine.Rect screenAreaRectangle, float randomShift)
        {
            // Generate a random X and Y position within the rectangle's bounds.
            float randomX = UnityEngine.Random.Range(rect.posX, rect.posX + rectWidth);
            float randomY = UnityEngine.Random.Range(rect.posY, rect.posY + rectHeight);
            
            Vector3 position = new Vector3(randomX, randomY, 0);

            // Check if the position is inside the screenAreaRectangle
            if(screenAreaRectangle.Contains(position))
            {
                // If it is, move the position to the nearest point on the edge of the screenAreaRectangle
                float left = Mathf.Abs(screenAreaRectangle.xMin - position.x);
                float right = Mathf.Abs(screenAreaRectangle.xMax - position.x);
                float bottom = Mathf.Abs(screenAreaRectangle.yMin - position.y);
                float top = Mathf.Abs(screenAreaRectangle.yMax - position.y);

                float min = Mathf.Min(left, right, bottom, top);

                if (min == left)
                    position.x = screenAreaRectangle.xMin - UnityEngine.Random.Range(0, randomShift);
                else if (min == right)
                    position.x = screenAreaRectangle.xMax + UnityEngine.Random.Range(0, randomShift);
                else if (min == bottom)
                    position.y = screenAreaRectangle.yMin - UnityEngine.Random.Range(0, randomShift);
                else if (min == top)
                    position.y = screenAreaRectangle.yMax + UnityEngine.Random.Range(0, randomShift);
            }
            else
            {
                /*// Apply a random shift in X and Y direction
                float randomShiftX = UnityEngine.Random.Range(0, randomShift);
                float randomShiftY = UnityEngine.Random.Range(0, randomShift);
                position += new Vector3(randomShiftX, randomShiftY, 0);*/
            }

            return position;
        }

        [Serializable]
        public class Rect
        {
            public int x;
            public int y;
            
            public float posX;
            public float posY;

            public float monsters = 0;
            
            // relative direction to player position: Idle (none), Up, Down, Left, Right
            public Gamesystem.PlayerMoveDirection direction;
            
            public bool highlightDebug;

            public Rect(int x, int y, float posX, float posY, Gamesystem.PlayerMoveDirection direction)
            {
                this.x = x;
                this.y = y;
                this.posX = posX;
                this.posY = posY;
                this.direction = direction;
            }

        }
    }
}