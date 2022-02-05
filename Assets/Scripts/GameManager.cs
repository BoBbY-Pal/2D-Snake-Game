using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DontConflict
{
    public class GameManager : MonoBehaviour
    {   
        public int maxHeight = 15;
        public int maxWidth = 17;
        
        public Color color1;
        public Color color2;
        public Color playerColor = Color.black;
        public Color appleColor = Color.red;
        public Transform cameraHolder;


        GameObject playerObj;
        GameObject mapObject;
        GameObject appleObj;
        GameObject tailParent;
        Node playerNode;
        Node appleNode;
        Node prevPlayerNode;
        SpriteRenderer mapRenderer;
        Sprite playerSprite;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up,down,left,right;        // Player Input variables
        
        public float moveRate = 0.5f;
        float timer;

        Direction targetDirection;
        Direction curDirection;

        public enum Direction
        {
            up,down,left,right
        }

        #region Init
        private void Start()
        {
            CreateMap();
            PlacePlayer();
            PlaceCamera(); 
            CreateApple();
        }
        
        private void CreateMap() 
        {   
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();   
            grid = new Node[maxWidth, maxHeight];
            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for(int x = 0; x < maxWidth; x++) 
            {
                for(int y = 0; y < maxHeight; y++) 
                {   
                    Vector3 tp = Vector3.zero;
                    tp.x = x;
                    tp.y = y; 
                      
                    Node n = new Node() {
                        x = x,
                        y = y,
                        worldPosition = tp
                    };
                    
                    grid[x,y] = n;
                    availableNodes.Add(n);
                    
                    #region Visual
                    if(x % 2 != 0)
                    {
                        if(y % 2 !=0) 
                        {
                            txt.SetPixel(x, y, color1);
                        } 
                        else 
                        {
                            txt.SetPixel(x, y, color2);
                        }
                    }   
                    else
                    {
                        if( y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color2);
                        }   
                        else
                        {
                            txt.SetPixel(x, y, color1);
                        }
                    }
                    #endregion       
                }    
            }
            txt.filterMode = FilterMode.Point;

            txt.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;
        }
        
        private void PlacePlayer()
        {   
            playerObj = new GameObject("Player"); 
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerSprite = CreateSprite(playerColor);
            playerRender.sprite = playerSprite;
            playerRender.sortingOrder = 1;
            playerNode = GetNode(3, 3);
            PlacePlayerObject(playerObj, playerNode.worldPosition);
            playerObj.transform.localScale = Vector3.one * 1.2f;
            
            tailParent = new GameObject("tailParent");
        }
        
        void PlaceCamera()
        {
            Node n = GetNode(maxWidth/2, maxHeight/2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f;
            cameraHolder.position = p;
        }
       
        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>();
            appleRenderer.sprite = CreateSprite(appleColor);
            appleRenderer.sortingOrder = 1;
            RandomlyPlaceApple();
        }
        #endregion
        
        #region update
        private void Update() 
        {
            GetInput();
            SetPlayerDirection();
           
            timer += Time.deltaTime;
            if(timer > moveRate)
            {
                timer = 0;
                curDirection = targetDirection;
                MovePlayer();

            }
        }

        private void GetInput()
        {
            up = Input.GetKeyDown(KeyCode.W);
            down = Input.GetKeyDown(KeyCode.S);
            left = Input.GetKeyDown(KeyCode.A);
            right = Input.GetKeyDown(KeyCode.D);
        }

        void SetPlayerDirection()
        {
            if(up)
            {
                SetDirection(Direction.up);
                
            }
            else if(down)
            {
                SetDirection(Direction.down);
                
            }
            else if(left)
            {
                SetDirection(Direction.left);
                
            }
            else if(right)
            {
                SetDirection(Direction.right);
                
            }
        }

        void SetDirection(Direction d)
        {
            if(!IsOpposite(d))
            {
                targetDirection = d;

            }
        }


        void MovePlayer()
        {   int x = 0;
            int y = 0;

            switch (curDirection) 
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down :
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right :
                    x = 1;
                    break;
            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);
            if(targetNode == null)
            {
                //Game OVER
            }
            else
            {   
                if(isTailNode(targetNode))
                {
                    //game over
                }   
                else
                {
                    
                    bool isScore = false;

                    if(targetNode == appleNode)
                    {
                        //  You've scored
                        isScore = true;
                        
                    }

                    Node previousNode = playerNode;
                    availableNodes.Add(previousNode);
                    

                    if(isScore)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                        availableNodes.Remove(previousNode);
                    }

                    MoveTail();
                    PlacePlayerObject(playerObj, targetNode.worldPosition);
                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if(isScore)
                    {
                        if(availableNodes.Count > 0)
                        {
                            RandomlyPlaceApple();
                        }
                        else 
                        {
                            //You Won
                        }
                    }
                }
            }
        }
       
        void MoveTail()
        {
            Node prevNode = null;
            for(int i = 0; i < tail.Count ; i++) 
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node); 
                if(i == 0)
                {
                    prevNode = p.node;
                    p.node = playerNode;
                    
                }   else
                {
                    Node prev = p.node;
                    p.node = prevNode;
                    prevNode = prev;
                }

                availableNodes.Remove(p.node);
                PlacePlayerObject(p.obj, p.node.worldPosition);
            }
        }
        #endregion

        #region Utillities
        Node GetNode(int x, int y)
        {
            if(x < 0 || x > maxWidth-1 || y < 0 || y > maxHeight-1)
                return null;

            return grid[x, y];
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1,1);
            txt.SetPixel(0,0, targetColor);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return  Sprite.Create(txt, rect, Vector2.one * .5f, 1, 0, SpriteMeshType.FullRect);
        }

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x,y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.localScale = Vector3.one * .95f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
            r.sortingOrder = 1;

            return s;
        }

        bool IsOpposite(Direction d)
        {   
            switch (d)
           {    
               default:
               case Direction.up:
                    if(curDirection == Direction.down)
                        return true;
                    else
                     return false;
                case Direction.down:
                    if(curDirection == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if(curDirection == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if(curDirection == Direction.left)
                        return true;
                   else
                       return false;
           }
           
        }

        bool isTailNode(Node n)
        {
            for(int i = 0; i < tail.Count; i++) 
            {
                if(tail[i].node == n)
                {
                    return true;
                }
            }
            return false;
        }

        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            PlacePlayerObject(appleObj, n.worldPosition);
            appleNode = n;
        }
        #endregion
    }
}